using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using Converse.Tron;
using System.Windows.Input;

namespace Converse.ViewModels.Register
{
    public class RegisterPageViewModel : ViewModelBase
    {
        WalletManager _walletManager { get; }

        public ICommand ContinueCommand { get; private set; }

        public Wallet Wallet { get; set; }
        public List<string> RecoveryPhrase { get; private set; }

        public RegisterPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, WalletManager walletManager)
                : base(navigationService, pageDialogService, deviceService)
        {
            Title = "Register";
            _walletManager = walletManager;

            ContinueCommand = new DelegateCommand(Continue);
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
            Wallet = _walletManager.CreateNewWalletAsync();
            RecoveryPhrase = Wallet.Mnemonic.Split(' ').Select(p => p.Trim()).ToList();

            IsBusy = false;
        }

        async void Continue()
        {
            if(string.IsNullOrWhiteSpace(Wallet.Name))
            {
                await _pageDialogService.DisplayAlertAsync("Invalid Name", "Please enter a name", "Ok");
                return;
            }

            var navParams = new NavigationParameters();
            navParams.Add("RecoveryPhrase", RecoveryPhrase);
            await _navigationService.NavigateAsync("ConfirmRecoveryPhrasePage", navParams);
        }

    }
}
