using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using Converse.Models;
using Converse.Services;
using Converse.Tron;
using Plugin.FirebasePushNotification.Abstractions;
using Acr.UserDialogs;
using Converse.Database;
using System.Diagnostics;
using Xamarin.Forms;

namespace Converse.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase
    {
        public UserInfo User { get; set; }

        public string AddressQrCodeContent => string.IsNullOrWhiteSpace(User?.TronAddress) ? "none" : User.TronAddress;

        public SettingsPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, IFirebasePushNotification firebasePushNotification, IUserDialogs userDialogs,
                                     SyncServerConnection syncServerConnection, TronConnection tronConnection, WalletManager walletManager, TokenMessagesQueueService tokenMessagesQueueService, ConverseDatabase converseDatabase)
        : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServerConnection, tronConnection, walletManager, tokenMessagesQueueService, converseDatabase)
        {
            Title = "Settings";
            Icon = "baseline_settings_white_32";
        }

        public override async void OnNavigatingTo(INavigationParameters parameters)
        {
            _fcm.OnNotificationReceived += _fcm_OnNotificationReceived;

            User = await _syncServer.GetUserAsync(_walletManager.Wallet.Address);
        }

        async void _fcm_OnNotificationReceived(object source, FirebasePushNotificationDataEventArgs e)
        {
            Debug.WriteLine(e);
            Device.BeginInvokeOnMainThread(async () => User = await _syncServer.GetUserAsync(_walletManager.Wallet.Address)); ;
        }
    }
}
