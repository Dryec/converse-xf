using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using Acr.UserDialogs;
using Converse.Services;
using Converse.Tron;
using Converse.Database;

namespace Converse.ViewModels
{
    public class QrCodePopupPageViewModel : ViewModelBase
    {
        public string QrCodeContent { get; set; }

        public QrCodePopupPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, Plugin.FirebasePushNotification.Abstractions.IFirebasePushNotification firebasePushNotification, IUserDialogs userDialogs, SyncServerConnection syncServer, TronConnection tronConnection, WalletManager walletManager, TokenMessagesQueueService tokenMessagesQueueService, ConverseDatabase converseDatabase) : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServer, tronConnection, walletManager, tokenMessagesQueueService, converseDatabase)
        {
            QrCodeContent = "none";
        }

        public override void OnNavigatingTo(INavigationParameters parameters)
        {
            if (parameters.TryGetValue(KnownNavigationParameters.XamlParam, out string content)
               || parameters.TryGetValue("content", out content))
            {
                QrCodeContent = string.IsNullOrWhiteSpace(content) ? "none" : content;
            }
        }
    }
}
