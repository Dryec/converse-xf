using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.AppModel;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using Converse.Resources;
using Client;
using Xamarin.Forms;
using System.Threading.Tasks;
using BarcodeScanner;
using Plugin.FirebasePushNotification.Abstractions;
using Acr.UserDialogs;
using Converse.Services;
using Converse.Tron;
using Converse.Database;

namespace Converse.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public Command OpenWelcomePageCommand { get; }

        private readonly IBarcodeScannerService barcodeScannerService;

        public MainPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IFirebasePushNotification firebasePushNotification,
                                 IDeviceService deviceService, IBarcodeScannerService barcodeScannerService, IUserDialogs userDialogs, SyncServerConnection syncServer, TronConnection tronConnection, WalletManager walletManager, TokenMessagesQueueService tokenMessagesQueueService, ConverseDatabase converseDatabase)
            : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServer,tronConnection,walletManager,tokenMessagesQueueService, converseDatabase)
        {
            Title = AppResources.MainPageTitle;
            this.barcodeScannerService = barcodeScannerService;

            OpenWelcomePageCommand = new Command(async () => await _navigationService.NavigateAsync("MainPage"));
        }

        public override void OnNavigatingTo(INavigationParameters parameters)
        {
            // TODO: Implement your initialization logic
        }

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            // TODO: Handle any final tasks before you navigate away
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            switch (parameters.GetNavigationMode())
            {
                case NavigationMode.Back:
                    // TODO: Handle any tasks that should occur only when navigated back to
                    break;
                case NavigationMode.New:
                    // TODO: Handle any tasks that should occur only when navigated to for the first time
                    break;
            }

            // TODO: Handle any tasks that should be done every time OnNavigatedTo is triggered
        }
    }
}