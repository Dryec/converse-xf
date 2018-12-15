using System;
using System.IO;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Converse.Database;
using Converse.Services;
using Converse.Tron;
using Plugin.FirebasePushNotification.Abstractions;
using Prism.AppModel;
using Prism.Navigation;
using Prism.Services;

namespace Converse.ViewModels
{
    public class SplashScreenPageViewModel : ViewModelBase
    {
        public SplashScreenPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService,
                                            IDeviceService deviceService, IFirebasePushNotification firebasePushNotification, IUserDialogs userDialogs, TronConnection tronConnection, ConverseDatabase database,
                                            TokenMessagesQueueService tokenMessagesQueueService, WalletManager walletManager, SyncServerConnection syncServerConnection)
            : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServerConnection, tronConnection, walletManager, tokenMessagesQueueService, database)
        {
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            _tronConnection.Connect();
            _database.Init(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ConverseDatabase.db3"));
            _tokenMessagesQueue.Start(_tronConnection, _database, _walletManager, _syncServer);
            _syncServer.SetDatabase(_database);

            var loadedWallet = await _walletManager.LoadWalletAsync();

            // After performing the long running task we perform an absolute Navigation to remove the SplashScreen from the Navigation Stack.
            //loadedWallet = false;
            if (loadedWallet)
            {
                //await _navigationService.NavigateAsync("/NavigationPage/ChatsOverviewPage");
                await _navigationService.NavigateAsync("/NavigationPage/MainPage");
                //await _navigationService.NavigateAsync("/NavigationPage/SettingsPage");
            }
            else
            {
                await _navigationService.NavigateAsync("/NavigationPage/WelcomePage");
            }

            //await _navigationService.NavigateAsync("/ChatPage");
            //await _navigationService.NavigateAsync("/ChatsOverviewPage");
        }
    }
}