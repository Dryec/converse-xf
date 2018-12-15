using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using System.Windows.Input;
using Plugin.FirebasePushNotification.Abstractions;
using Acr.UserDialogs;
using Converse.Tron;
using Converse.Services;
using Converse.Database;

namespace Converse.ViewModels.Register
{
    public class RegisterInfoPageViewModel : ViewModelBase
    {
        
        public RegisterInfoPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IUserDialogs userDialogs, IDeviceService deviceService, IFirebasePushNotification firebasePushNotification, SyncServerConnection syncServerConnection, TronConnection tronConnection, WalletManager walletManager, TokenMessagesQueueService tokenMessagesQueueService, ConverseDatabase converseDatabase)
         : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServerConnection, tronConnection, walletManager, tokenMessagesQueueService, converseDatabase)
        {
            Title = "Register Info";
        }
    }
}
