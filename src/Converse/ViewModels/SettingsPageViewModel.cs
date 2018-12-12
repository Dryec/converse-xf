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

namespace Converse.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase
    {
        public UserInfo User { get; set; }
        SyncServerConnection _syncServerConnection { get; }
        public WalletManager _walletManager { get; }

        public SettingsPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService,
                                     SyncServerConnection syncServerConnection, WalletManager walletManager) 
        : base(navigationService, pageDialogService, deviceService)
        {
            Title = "Settings";
            _syncServerConnection = syncServerConnection;
            _walletManager = walletManager;
        }

        public override async void OnNavigatingTo(INavigationParameters parameters)
        {
            User = await _syncServerConnection.GetUserAsync(_walletManager.Wallet.Address);
        }
    }
}
