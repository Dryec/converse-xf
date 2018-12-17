using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using Converse.Models;

namespace Converse.ViewModels
{
    public class UserPopupPageViewModel : ViewModelBase
    {
        public UserInfo User { get; set; }
        public string AddressQrCodeContent { get; set; }

        public UserPopupPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, Plugin.FirebasePushNotification.Abstractions.IFirebasePushNotification firebasePushNotification, Acr.UserDialogs.IUserDialogs userDialogs, Services.SyncServerConnection syncServerConnection, Services.TronConnection tronConnection, Tron.WalletManager walletManager, Services.TokenMessagesQueueService tokenMessagesQueueService, Database.ConverseDatabase converseDatabase) : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServerConnection, tronConnection, walletManager, tokenMessagesQueueService, converseDatabase)
        {
            AddressQrCodeContent = "none";
        }

        public override void OnNavigatingTo(INavigationParameters parameters)
        {
            if (parameters.TryGetValue(KnownNavigationParameters.XamlParam, out UserInfo user)
                || parameters.TryGetValue("user", out user))
            {
                User = user;
                AddressQrCodeContent = User.TronAddress;
            }

            if(User == null)
            {
                _navigationService.GoBackAsync();
            }
        }
    }
}
