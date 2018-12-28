using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using Converse.Models;
using System.Collections.ObjectModel;
using Converse.TokenMessages;
using Converse.Helpers;
using Client;
using Converse.Tron;
using System.Diagnostics;

namespace Converse.ViewModels
{
    public class GroupPopupPageViewModel : ViewModelBase
    {
        public DelegateCommand LeaveCommand { get; private set; }
        public DelegateCommand JoinCommand { get; private set; }

        public GroupInfo Group { get; private set; }
        public UserInfo GroupUserInfo { get; private set; }
        public bool IsAdmin => GroupUserInfo?.Rank > 0;
        public bool IsMember => GroupUserInfo != null;
        public bool IsNotMember => !IsMember;

        public ObservableCollection<UserInfo> Users { get; private set; }
        public string AddressQrCodeContent { get; set; }
        public bool IsLoaded { get; private set; }

        public GroupPopupPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, Plugin.FirebasePushNotification.Abstractions.IFirebasePushNotification firebasePushNotification, Acr.UserDialogs.IUserDialogs userDialogs, Services.SyncServerConnection syncServerConnection, Services.TronConnection tronConnection, Tron.WalletManager walletManager, Services.TokenMessagesQueueService tokenMessagesQueueService, Database.ConverseDatabase converseDatabase) : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServerConnection, tronConnection, walletManager, tokenMessagesQueueService, converseDatabase)
        {
            AddressQrCodeContent = "none";

            LeaveCommand = new DelegateCommand(LeaveCommandExecuted);
            JoinCommand = new DelegateCommand(JoinCommandExecuted);
        }

        async void JoinCommandExecuted()
        {
            var ok = await _userDialogs.ConfirmAsync("Do you want to join the group?", Group.Name);
            if (ok)
            {
                _userDialogs.ShowLoading();
                var pendingId = await _tokenMessagesQueue.AddAsync(_walletManager.Wallet.Address, Group.TronAddress, new JoinGroupTokenMessage());
                _fcm.Subscribe($"{AppConstants.FCM.Topics.Group}_{Group.TronAddress}");

                var waitResult = await _tokenMessagesQueue.WaitForAsync(pendingId);
                _userDialogs.HideLoading();

                await _userDialogs.AlertAsync("You will be added shortly", "Group Joined");
                await _navigationService.GoBackToRootAsync();
                await _navigationService.ClearPopupStackAsync();
            }
        }

        async void LeaveCommandExecuted()
        {
            var ok = await _userDialogs.ConfirmAsync("Do you want to leave the group?", Group.Name);
            if (ok)
            {
                _userDialogs.ShowLoading();
                var pendingId = await _tokenMessagesQueue.AddAsync(_walletManager.Wallet.Address, Group.TronAddress, new LeaveGroupTokenMessage());

                var waitResult = await _tokenMessagesQueue.WaitForAsync(pendingId);
                _userDialogs.HideLoading();

                await _userDialogs.AlertAsync("You will be removed shortly", "Group Left");
                await _navigationService.GoBackToRootAsync();
                await _navigationService.ClearPopupStackAsync();
            }
        }


        public override async void OnNavigatingTo(INavigationParameters parameters)
        {
            var navMode = parameters.GetNavigationMode();
            if (navMode == NavigationMode.New)
            {
                if (parameters.TryGetValue(KnownNavigationParameters.XamlParam, out GroupInfo group)
                    || parameters.TryGetValue("group", out group))
                {
                    AddressQrCodeContent = group.TronAddress;

                    var dbGroup = await _database.Groups.GetByGroupID(group.GroupID);
                    if (dbGroup != null)
                    {
                        Group = dbGroup.ToGroupInfo();
                    }
                    group = await _syncServer.GetGroupAsync(group.GroupID, _walletManager.Wallet.Address);
                    if (group != null)
                    {
                        Group = group;
                    }
                }

                if (Group != null)
                {
                    Users = new ObservableCollection<UserInfo>(Group.Users);
                    foreach (var user in Group.Users)
                    {
                        if (user.TronAddress.Equals(_walletManager.Wallet.Address))
                        {
                            GroupUserInfo = user;
                        }
                    }
                    IsLoaded = true;
                }
                else
                {
                    _userDialogs.Toast("Could not load group info");
                    await _navigationService.GoBackAsync();
                }
            }
            else if (navMode == NavigationMode.Back)
            {
                if (parameters.TryGetValue("selected_address", out string address))
                {
                    // Add user
                    if (WalletAddress.Decode58Check(address) != null)
                    {
                        _userDialogs.ShowLoading();
                        var user = await _syncServer.GetUserAsync(address);
                        if (user != null)
                        {
                            var ok = await _userDialogs.ConfirmAsync($"Do you want to add {user.Name}?", Group.Name);
                            if (ok)
                            {
                                try
                                {
                                    var privKey = _walletManager.Wallet.Decrypt(Group.PrivateKey, Group.PublicKey);
                                    var groupWallet = new Wallet(privKey);
                                    var privKeyEnc = groupWallet.Encrypt(groupWallet.PrivateKey, user.PublicKey);
                                    var pubKeyEnc = _walletManager.Wallet.Encrypt(user.TronAddress, AppConstants.PropertyAddressPublicKey);

                                    var pendingId = await _tokenMessagesQueue.AddAsync(_walletManager.Wallet.Address, Group.TronAddress,
                                                                                       new AddUserToGroupTokenMessage
                                                                                       {
                                                                                           Address = pubKeyEnc,
                                                                                           PrivateKey = privKeyEnc
                                                                                       });

                                    var waitResult = await _tokenMessagesQueue.WaitForAsync(pendingId);

                                    await _userDialogs.AlertAsync($"{user.Name} will be added shortly", waitResult ? "Added" : "Pending");
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex);
                                    _userDialogs.Alert($"Something went wrong while adding {user.Name}", "Failed");
                                }
                            }
                        }
                        else
                        {
                            _userDialogs.Alert("Could not find user", "Adding Failed");
                        }
                        _userDialogs.HideLoading();
                    }
                }
            }
        }
    }
}
