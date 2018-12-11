using System;
using System.IO;
using System.Threading.Tasks;
using Converse.Database;
using Converse.Services;
using Converse.Tron;
using Prism.AppModel;
using Prism.Navigation;
using Prism.Services;

namespace Converse.ViewModels
{
    public class SplashScreenPageViewModel : ViewModelBase
    {

        TronConnection _tronConnection { get; }
        ConverseDatabase _database { get; }
        TokenMessagesQueueService _transactionsQueueService { get; }
        SyncServerConnection _syncServerConnection { get; }
        WalletManager _walletManager { get; }

        public SplashScreenPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService,
                                            IDeviceService deviceService, TronConnection tronConnection, ConverseDatabase database,
                                            TokenMessagesQueueService transactionsQueueService, WalletManager walletManager, SyncServerConnection syncServerConnection)
            : base(navigationService, pageDialogService, deviceService)
        {
            _tronConnection = tronConnection;
            _database = database;
            _transactionsQueueService = transactionsQueueService;
            _walletManager = walletManager;
            _syncServerConnection = syncServerConnection;
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            _tronConnection.Connect();
            _database.Init(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ConverseDatabase.db3"));
            _transactionsQueueService.Start(_tronConnection, _database, _walletManager, _syncServerConnection);
            _syncServerConnection.SetDatabase(_database);

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