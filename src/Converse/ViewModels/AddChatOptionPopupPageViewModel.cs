using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using BarcodeScanner;
using Converse.Services;
using Client;
using Google.Protobuf;
using Grpc.Core;
using Acr.UserDialogs;
using Plugin.FirebasePushNotification.Abstractions;
using Converse.Tron;
using Converse.Database;
using System.Diagnostics;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace Converse.ViewModels
{
    public class AddChatOptionPopupPageViewModel : ViewModelBase
    {
        IBarcodeScannerService _barcodeScanner { get; }

        public DelegateCommand ScanCommand { get; }
        public DelegateCommand OpenChatCommand { get; }

        public string Address { get; set; }

        public AddChatOptionPopupPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, IFirebasePushNotification firebasePushNotification,
                                                IBarcodeScannerService barcodeScanner, IUserDialogs userDialogs, SyncServerConnection syncServerConnection, TronConnection tronConnection, WalletManager walletManager, TokenMessagesQueueService tokenMessagesQueueService, ConverseDatabase converseDatabase)
            : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServerConnection, tronConnection, walletManager, tokenMessagesQueueService, converseDatabase)
        {
            _barcodeScanner = barcodeScanner;

            ScanCommand = new DelegateCommand(OnScanCommandExecuted);
            OpenChatCommand = new DelegateCommand(OpenChatCommandExecuted);
        }

        async void OpenChatCommandExecuted()
        {
            IsBusy = true;
            _userDialogs.ShowLoading(maskType: MaskType.Black);
            try
            {
                await Task.Delay(1000);
                var addressBytes = WalletAddress.Decode58Check(Address);
                if (addressBytes != null)
                {
                    var account = await _tronConnection.Client.GetAccountAsync(new Protocol.Account { Address = ByteString.CopyFrom(addressBytes) });
                    if (!account.Address.IsEmpty)
                    {
                        var group = await _syncServer.GetGroupAsync(Address, _walletManager.Wallet.Address);
                        if (group != null)
                        {

                            var navParams = new NavigationParameters();
                            navParams.Add("group", group);
                            await _navigationService.NavigateAsync("GroupPopupPage", navParams);
                        }
                        else
                        {
                            var user = await _syncServer.GetUserAsync(Address);
                            if (user != null)
                            {
                                var navParams = new NavigationParameters();
                                navParams.Add("address", Address);
                                await _navigationService.NavigateAsync("ChatPage", navParams);
                            }
                            else
                            {
                                _userDialogs.Toast("User is not registered or unable to load");
                            }
                        }
                    }
                    else
                    {
                        _userDialogs.Toast("User or Group is not yet activated");
                    }
                }
                else
                {
                    _userDialogs.Toast("Not an user or group address");
                }
            }
            catch (RpcException ex)
            {
                _userDialogs.Toast("Unable to load user");
            }
            _userDialogs.HideLoading();
            IsBusy = false;
        }

        async void OnScanCommandExecuted()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
                if (status != PermissionStatus.Granted)
                {
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Camera);
                    //Best practice to always check that the key exists
                    if (results.ContainsKey(Permission.Camera))
                        status = results[Permission.Camera];
                }

                if (status == PermissionStatus.Granted)
                {
                    var content = await _barcodeScanner.ReadBarcodeAsync();
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        Address = content;
                        //OpenChatCommand.Execute();
                    }
                }
                else if (status != PermissionStatus.Unknown)
                {
                    await _userDialogs.AlertAsync("Camera access denied", "Not Available", "Ok");
                }
            }
            catch (Exception ex)
            {
                _userDialogs.Toast(ex.Message);
            }
        }
    }
}
