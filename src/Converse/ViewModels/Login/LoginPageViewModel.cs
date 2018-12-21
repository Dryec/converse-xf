using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using System.Diagnostics;

namespace Converse.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
        public DelegateCommand ContinueCommand { get; private set; }

        public bool UsePrivateKey { get; set; }
        public bool UseRecoveryPhrase => !UsePrivateKey;

        public string PrivateKey { get; set; }
        public List<string> RecoveryPhrase { get; private set; }

        public LoginPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, Plugin.FirebasePushNotification.Abstractions.IFirebasePushNotification firebasePushNotification, Acr.UserDialogs.IUserDialogs userDialogs, Services.SyncServerConnection syncServerConnection, Services.TronConnection tronConnection, Tron.WalletManager walletManager, Services.TokenMessagesQueueService tokenMessagesQueueService, Database.ConverseDatabase converseDatabase) : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServerConnection, tronConnection, walletManager, tokenMessagesQueueService, converseDatabase)
        {
            Title = "Login";
            PrivateKey = string.Empty;
            RecoveryPhrase = new List<string>(new string[12]);

            ContinueCommand = new DelegateCommand(ContinueCommandExecuted);
        }

        async void ContinueCommandExecuted()
        {
            var recoveryPhrase = string.Empty;
            if(UseRecoveryPhrase)
            {
                recoveryPhrase = String.Join(" ", RecoveryPhrase).Trim();
                if(string.IsNullOrWhiteSpace(recoveryPhrase))
                {
                    await _userDialogs.AlertAsync("Not a valid recovery phrase", "Failure", "Ok");
                    return;
                }
            }
            else if(string.IsNullOrWhiteSpace(PrivateKey) || PrivateKey.Length < 32)
            {
                await _userDialogs.AlertAsync("Not a valid private key", "Failure", "Ok");
                return;
            }

            try
            {
                _walletManager.LoadWalletAsync(UsePrivateKey ? PrivateKey : recoveryPhrase, UsePrivateKey);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await _userDialogs.AlertAsync("Not a valid private key", "Failure", "Ok");
                return;
            }

            var navParams = new NavigationParameters();
            navParams.Add("address", _walletManager.Wallet.Address);
            await _navigationService.NavigateAsync("ConfirmLoginPopupPage", navParams);
        }
    }
}
