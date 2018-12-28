using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using System.Collections.ObjectModel;
using Converse.Models;
using Client;
using BarcodeScanner;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace Converse.ViewModels
{
    public class SelectUserPopupPageViewModel : ViewModelBase
    {
        IBarcodeScannerService _barcodeScanner { get; }

        public DelegateCommand ScanCommand { get; }
        public DelegateCommand SelectCommand { get; private set; }

        public string Address { get; set; }
        public ObservableCollection<UserInfo> Users { get; set; }

        public SelectUserPopupPageViewModel(INavigationService navigationService, IBarcodeScannerService barcodeScannerService, IPageDialogService pageDialogService, IDeviceService deviceService, Plugin.FirebasePushNotification.Abstractions.IFirebasePushNotification firebasePushNotification, Acr.UserDialogs.IUserDialogs userDialogs, Services.SyncServerConnection syncServerConnection, Services.TronConnection tronConnection, Tron.WalletManager walletManager, Services.TokenMessagesQueueService tokenMessagesQueueService, Database.ConverseDatabase converseDatabase) : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServerConnection, tronConnection, walletManager, tokenMessagesQueueService, converseDatabase)
        {
            _barcodeScanner = barcodeScannerService;
            Users = new ObservableCollection<UserInfo>();

            ScanCommand = new DelegateCommand(ScanCommandExecuted);
            SelectCommand = new DelegateCommand(SelectCommandExecuted);
        }

        async void ScanCommandExecuted()
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

        async void SelectCommandExecuted()
        {
            if(WalletAddress.Decode58Check(Address) != null)
            {
                var navParams = new NavigationParameters();
                navParams.Add("selected_address", Address);
                await _navigationService.GoBackAsync(navParams);
            }
            else
            {
                await _userDialogs.AlertAsync("No valid address", "Invalid");
            }
        }

        public override async void OnNavigatingTo(INavigationParameters parameters)
        {
            var dbChats = await _database.Chats.GetAll();
            foreach (var dbChat in dbChats)
            {
                var chat = dbChat.ToChatEntry();

                if(chat.Type == Enums.ChatType.Normal)
                {
                    Users.Add(chat.ChatPartner);
                }
            }
        }
    }
}
