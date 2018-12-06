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
    public class ConfirmRecoveryPhrasePageViewModel : ViewModelBase
    {
        WalletManager _walletManager { get; }

        public List<string> RecoveryPhrase { get; private set; }
        public List<string> RecoveryPhraseConfirmation { get; private set; }

        public DelegateCommand ContinueCommand { get; private set; }

        public ConfirmRecoveryPhrasePageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, WalletManager walletManager) 
                              : base(navigationService, pageDialogService, deviceService)
        {
            Title = "Recovery Phrase";
            _walletManager = walletManager;

            RecoveryPhraseConfirmation = new List<string>(new string[12]);
            ContinueCommand = new DelegateCommand(Continue);
        }

        public override void OnNavigatingTo(INavigationParameters parameters)
        {
            if(parameters.TryGetValue(KnownNavigationParameters.XamlParam, out List<string> recoveryPhrase))
            {
                RecoveryPhrase = recoveryPhrase;
            }
        }

        public async void Continue()
        {
            if(true)//RecoveryPhraseConfirmation.Select(p => p.Trim()).SequenceEqual(RecoveryPhrase))
            {
                await _walletManager.SaveAsync();
                await _navigationService.NavigateAsync("/NavigationPage/MainPage");
            }
            else
            {
                await _pageDialogService.DisplayAlertAsync("No match", "The entered Recovery Phrase does not match", "Ok");
            }
        }

    }
}
