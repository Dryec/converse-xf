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
using Plugin.Media;
using Plugin.Media.Abstractions;
using Firebase.Storage;
using NETCore.Encrypt;

namespace Converse.ViewModels
{
    public class GroupPopupPageViewModel : ViewModelBase
    {
        public DelegateCommand LeaveCommand { get; private set; }
        public DelegateCommand JoinCommand { get; private set; }
        public DelegateCommand ShareCommand { get; private set; }
        public DelegateCommand ToggleEditModeCommand { get; private set; }
        public DelegateCommand SelectProfilePictureCommand { get; private set; }
        public DelegateCommand SaveCommand { get; private set; }

        public GroupInfo Group { get; private set; }
        public UserInfo GroupUserInfo { get; private set; }
        public bool IsAdmin => GroupUserInfo?.Rank > 0;
        public bool IsNotAdmin => !IsAdmin;
        public bool IsMember => GroupUserInfo != null;
        public bool IsNotMember => !IsMember;
        public bool IsEditMode { get; set; }
        public bool IsNotEditMode => !IsEditMode;

        public string EditName { get; set; }
        public string EditDescription { get; set; }
        public string EditImageUrl { get; set; }


        public ObservableCollection<UserInfo> Users { get; private set; }
        public string AddressQrCodeContent { get; set; }
        public bool IsLoaded { get; private set; }

        public GroupPopupPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, Plugin.FirebasePushNotification.Abstractions.IFirebasePushNotification firebasePushNotification, Acr.UserDialogs.IUserDialogs userDialogs, Services.SyncServerConnection syncServerConnection, Services.TronConnection tronConnection, Tron.WalletManager walletManager, Services.TokenMessagesQueueService tokenMessagesQueueService, Database.ConverseDatabase converseDatabase) : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServerConnection, tronConnection, walletManager, tokenMessagesQueueService, converseDatabase)
        {
            AddressQrCodeContent = "none";

            LeaveCommand = new DelegateCommand(LeaveCommandExecuted);
            JoinCommand = new DelegateCommand(JoinCommandExecuted);
            ShareCommand = new DelegateCommand(ShareCommandExecuted);
            ToggleEditModeCommand = new DelegateCommand(ToggleEditModeCommandExecuted);
            SelectProfilePictureCommand = new DelegateCommand(SelectProfilePictureCommandExecuted);
            SaveCommand = new DelegateCommand(SaveCommandExecuted);
        }



        async void SaveCommandExecuted()
        {
            if (IsBusy)
            {
                return;
            }

            var update = await _userDialogs.ConfirmAsync("Update Group?", Group.Name, "Yes", "No");
            if (!update)
            {
                return;
            }

            EditName = EditName.Trim();
            EditDescription = EditDescription.Trim();

            if (string.IsNullOrWhiteSpace(EditName))
            {
                await _userDialogs.AlertAsync("Enter a name", "Invalid Name", "Ok");
                return;
            }
            if (string.IsNullOrWhiteSpace(EditImageUrl) || !Uri.IsWellFormedUriString(EditImageUrl, UriKind.Absolute))
            {
                var res = await _userDialogs.ConfirmAsync("No group picture, do you want to continue without?", "Group Picture", "Yes", "No");
                if (!res)
                    return;
            }

            IsBusy = true;
            _userDialogs.ShowLoading();

            var wallet = _walletManager.Wallet;

            // Set Name
            var pendingId = await _tokenMessagesQueue.AddAsync(
                wallet.Address,
                Group.TronAddress,
                new ChangeGroupNameTokenMessage { Name = wallet.Encrypt(EditName, AppConstants.PropertyAddressPublicKey) }
            );

            // Set Description
            await _tokenMessagesQueue.AddAsync(
                wallet.Address,
                                 Group.TronAddress,
                                 new ChangeGroupDescriptionTokenMessage { Description = wallet.Encrypt(EditDescription, AppConstants.PropertyAddressPublicKey) });

            // Set Image if exist
            var existImage = _walletManager.Wallet.ProfileImageUrl != null && Uri.IsWellFormedUriString(EditImageUrl, UriKind.Absolute);
            await _tokenMessagesQueue.AddAsync(
                wallet.Address,
                                 Group.TronAddress,
                                 new ChangeGroupImageTokenMessage
                                 {
                                     Clear = !existImage,
                                     ImageUrl = existImage ? wallet.Encrypt(EditImageUrl, AppConstants.PropertyAddressPublicKey) : null
                                 });

            var result = await _tokenMessagesQueue.WaitForAsync(pendingId);


            await _navigationService.GoBackAsync();
            _userDialogs.HideLoading();
            await _userDialogs.AlertAsync("Group edit is pending and will be updated shortly", "Group Edit");
            IsBusy = false;
        }

        void ToggleEditModeCommandExecuted()
        {
            IsEditMode = !IsEditMode;
        }

        async void ShareCommandExecuted()
        {
            await Xamarin.Essentials.Clipboard.SetTextAsync(Group.TronAddress);
            await _userDialogs.AlertAsync("Copied group address to clipboard", "Copied");
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
                    EditName = Group.Name;
                    EditDescription = Group.Description;
                    EditImageUrl = Group.ImageUrl;
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

        async void SelectProfilePictureCommandExecuted()
        {
            await CrossMedia.Current.Initialize();
            try
            {
                var selection = await _userDialogs.ActionSheetAsync("Group Picture", "", null, null, "Take Photo", "Select from Gallery", "Clear");

                _userDialogs.ShowLoading();
                MediaFile file = null;
                if (selection.Equals("Take Photo"))
                {
                    if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                    {
                        _userDialogs.Toast("No camera available");
                        return;
                    }

                    file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                    {
                        SaveToAlbum = true,
                        Directory = "Converse",
                        PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                        MaxWidthHeight = 1024,
                        CompressionQuality = 50
                    });
                }
                else if (selection.Equals("Select from Gallery"))
                {
                    if (!CrossMedia.Current.IsPickPhotoSupported)
                    {
                        _userDialogs.Toast("Not supported");
                        return;
                    }
                    file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                    {
                        PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                        MaxWidthHeight = 1024,
                        CompressionQuality = 50
                    });
                }
                else if (selection.Equals("Clear"))
                {
                    EditImageUrl = "baseline_group_grayish_48";
                }

                if (file != null)
                {
                    var stream = file.GetStream();
                    var storedImageUrl = await new FirebaseStorage("converse-8a53c.appspot.com")
                                                    .Child("users")
                                                    .Child(_walletManager.Wallet.Address)
                                                    .Child("images")
                                                    .Child(EncryptProvider.Md5(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"), MD5Length.L16)
                                                                                + $".{file.Path.Split('.').Last()}")
                                                    .PutAsync(stream);
                    if (!string.IsNullOrWhiteSpace(storedImageUrl))
                    {
                        EditImageUrl = storedImageUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                _userDialogs.Toast("Image upload failed");
            }
            _userDialogs.HideLoading();
        }
    }
}
