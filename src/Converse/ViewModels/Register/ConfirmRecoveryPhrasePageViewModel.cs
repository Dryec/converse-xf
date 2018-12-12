using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using Converse.Tron;
using Converse.Services;
using Converse.TokenMessages;
using Converse.Helpers;
using Acr.UserDialogs;

namespace Converse.ViewModels.Register
{
    public class ConfirmRecoveryPhrasePageViewModel : ViewModelBase
    {
        TronConnection _tronConnection { get; }
        TokenMessagesQueueService _tokenMessagesQueueService { get; }
        IUserDialogs _userDialogs { get; }
        WalletManager _walletManager { get; }

        public List<string> RecoveryPhrase { get; private set; }
        public List<string> RecoveryPhraseConfirmation { get; private set; }

        public DelegateCommand ContinueCommand { get; private set; }

        public ConfirmRecoveryPhrasePageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, IUserDialogs userDialogs,
                                                    WalletManager walletManager, TronConnection tronConnection, TokenMessagesQueueService tokenMessagesQueueService)
                              : base(navigationService, pageDialogService, deviceService)
        {
            Title = "Recovery Phrase";
            _tronConnection = tronConnection;
            _tokenMessagesQueueService = tokenMessagesQueueService;
            _userDialogs = userDialogs;
            _walletManager = walletManager;

            RecoveryPhraseConfirmation = new List<string>(new string[12]);
            ContinueCommand = new DelegateCommand(Continue);
        }

        public override async void OnNavigatingTo(INavigationParameters parameters)
        {
            if (parameters.TryGetValue("RecoveryPhrase", out List<string> recoveryPhrase))
            {
                RecoveryPhrase = recoveryPhrase;
            }
            else
            {
                await _navigationService.GoBackAsync();
            }
        }

        public async void Continue()
        {
            if (IsBusy)
            {
                return;
            }

            if (true)//RecoveryPhraseConfirmation.Select(p => p.Trim()).SequenceEqual(RecoveryPhrase))
            {
                IsBusy = true;
                _userDialogs.ShowLoading();
                await Xamarin.Essentials.Clipboard.SetTextAsync(_walletManager.Wallet.Address);
                await _walletManager.SaveAsync();

                // Set Name
                var pendingId = await _tokenMessagesQueueService.AddAsync(
                    _walletManager.Wallet.Address,
                    AppConstants.PropertyAddress,
                    new ChangeNameTokenMessage { Name = _walletManager.Wallet.Name }
                );

                // Set Status to default
                await _tokenMessagesQueueService.AddAsync(
                 _walletManager.Wallet.Address,
                 AppConstants.PropertyAddress,
                 new ChangeStatusTokenMessage { Status = AppConstants.DefaultStatusMessage });

                // TODO Set Image if exist

                var result = await _tokenMessagesQueueService.WaitForAsync(pendingId);

                await _navigationService.NavigateAsync("/NavigationPage/MainPage");
                _userDialogs.HideLoading();
                IsBusy = false;
            }
            else
            {
                await _pageDialogService.DisplayAlertAsync("No match", "The entered Recovery Phrase does not match", "Ok");
            }
        }

    }
}
