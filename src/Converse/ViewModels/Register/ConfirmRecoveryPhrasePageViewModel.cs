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
using Plugin.FirebasePushNotification.Abstractions;
using Converse.Database;
using Org.BouncyCastle.Utilities.Encoders;

namespace Converse.ViewModels.Register
{
    public class ConfirmRecoveryPhrasePageViewModel : ViewModelBase
    {
        public List<string> RecoveryPhrase { get; private set; }
        public List<string> RecoveryPhraseConfirmation { get; private set; }

        public DelegateCommand ContinueCommand { get; private set; }

        public ConfirmRecoveryPhrasePageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, IUserDialogs userDialogs,
                                                    IFirebasePushNotification firebasePushNotification, WalletManager walletManager, SyncServerConnection syncServerConnection, TronConnection tronConnection, TokenMessagesQueueService tokenMessagesQueueService, ConverseDatabase converseDatabase)
                              : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServerConnection, tronConnection, walletManager, tokenMessagesQueueService, converseDatabase)
        {
            Title = "Recovery Phrase";

            RecoveryPhraseConfirmation = new List<string>(new string[12]);
            for (var i = 0; i < RecoveryPhraseConfirmation.Count; i++)
            {
                RecoveryPhraseConfirmation[i] = string.Empty;
            }
            ContinueCommand = new DelegateCommand(Continue);
        }

        public override async void OnNavigatingTo(INavigationParameters parameters)
        {
            if (parameters.TryGetValue("RecoveryPhrase", out List<string> recoveryPhrase))
            {
                RecoveryPhrase = recoveryPhrase;
            }

            if(RecoveryPhrase == null || RecoveryPhrase.Count < 12)
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

                var wallet = _walletManager.Wallet;

                _fcm.Subscribe($"{AppConstants.FCM.Topics.Update}_{wallet.Address}");

                // Set firebase token to property address
                var pendingId = await _tokenMessagesQueue.AddAsync(
                    wallet.Address,
                    AppConstants.PropertyAddress,
                    new AddDeviceIdTokenMessage
                    {
                        DeviceID = wallet.Encrypt(_fcm.Token, AppConstants.PropertyAddressPublicKey)
                    }
                );

                // Set Name
                await _tokenMessagesQueue.AddAsync(
                    wallet.Address,
                    AppConstants.PropertyAddress,
                    new ChangeNameTokenMessage { Name = wallet.Encrypt(wallet.Name, AppConstants.PropertyAddressPublicKey) }
                );

                // Set Status to default
                await _tokenMessagesQueue.AddAsync(
                                     wallet.Address,
                                     AppConstants.PropertyAddress,
                                     new ChangeStatusTokenMessage { Status = wallet.Encrypt(AppConstants.DefaultStatusMessage, AppConstants.PropertyAddressPublicKey) });

                // Set Image if exist
                if (_walletManager.Wallet.ProfileImageUrl != null && Uri.IsWellFormedUriString(wallet.ProfileImageUrl, UriKind.Absolute))
                {
                    await _tokenMessagesQueue.AddAsync(
                                         wallet.Address,
                                         AppConstants.PropertyAddress,
                                         new ChangeImageTokenMessage { ImageUrl = wallet.Encrypt(wallet.ProfileImageUrl, AppConstants.PropertyAddressPublicKey) });
                }

                await _walletManager.SaveAsync();

                var result = await _tokenMessagesQueue.WaitForAsync(pendingId);


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
