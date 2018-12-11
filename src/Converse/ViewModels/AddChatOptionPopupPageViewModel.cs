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

namespace Converse.ViewModels
{
    public class AddChatOptionPopupPageViewModel : ViewModelBase
    {
        IBarcodeScannerService _barcodeScanner { get; }
        IUserDialogs _userDialogs { get; }
        TronConnection _tronConnection { get; }

        public DelegateCommand ScanCommand { get; }
        public DelegateCommand OpenChatCommand { get; }

        public string UserAddress { get; set; }

        public AddChatOptionPopupPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService,
                                                IBarcodeScannerService barcodeScanner, IUserDialogs userDialogs, TronConnection tronConnection)
            : base(navigationService, pageDialogService, deviceService)
        {
            _barcodeScanner = barcodeScanner;
            _userDialogs = userDialogs;
            _tronConnection = tronConnection;

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
                        var navParams = new NavigationParameters();
                        navParams.Add("address", UserAddress);
                        await _navigationService.NavigateAsync("ChatPage", navParams);
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
