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

namespace Converse.ViewModels
{
    public class AddChatOptionPopupPageViewModel : ViewModelBase
    {
        IBarcodeScannerService _barcodeScanner { get; }

        public DelegateCommand ScanCommand { get; }
        public DelegateCommand OpenChatCommand { get; }

        public string UserAddress { get; set; }

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
                var addressBytes = WalletAddress.Decode58Check(UserAddress);
                if (addressBytes != null)
                {
                    var account = await _tronConnection.Client.GetAccountAsync(new Protocol.Account { Address = ByteString.CopyFrom(addressBytes) });
                    if(!account.Address.IsEmpty)
                    {
                        var user = await _syncServer.GetUserAsync(UserAddress);

                        if (user != null)
                        {
                            var navParams = new NavigationParameters();
                            navParams.Add("address", UserAddress);
                            await _navigationService.NavigateAsync("ChatPage", navParams);
                        }
                        else
                        {
                            _userDialogs.Toast("User is not registered");
                        }
                    }
                    else
                    {
                        _userDialogs.Toast("User is not yet activated");
                    }
                }
                else
                {
                    _userDialogs.Toast("Not a user address");
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
                UserAddress = await _barcodeScanner.ReadBarcodeAsync();
                if (!string.IsNullOrWhiteSpace(UserAddress))
                {
                    OpenChatCommand.Execute();
                }
            }
            catch
            {
            }
        }
    }
}
