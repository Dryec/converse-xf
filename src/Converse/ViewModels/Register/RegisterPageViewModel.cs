using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using Converse.Tron;

namespace Converse.ViewModels.Register
{
    public class RegisterPageViewModel : ViewModelBase
    {
        WalletManager _walletManager { get; }

        public Wallet Wallet { get; private set; }
        public List<string> RecoveryPhrase { get; private set; }

        public RegisterPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, WalletManager walletManager)
                : base(navigationService, pageDialogService, deviceService)
        {
            Title = "Register";
            _walletManager = walletManager;
        }

        public override void OnNavigatingTo(INavigationParameters parameters)
        {
            IsBusy = true;
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            if (RecoveryPhrase != null)
            {
                IsBusy = false;
                return;
            }

            IsBusy = true;

            await Task.Delay(1500);
            Wallet = await _walletManager.CreateNewWalletAsync();
            RecoveryPhrase = Wallet.MnemonicSentence.Split(' ').Select(p => p.Trim()).ToList();

            IsBusy = false;
        }
    }
}
