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
using Plugin.Media;
using Acr.UserDialogs;
using Plugin.Media.Abstractions;
using Xamarin.Forms;
using Firebase.Storage;
using NETCore.Encrypt;
using System.Diagnostics;
using Plugin.FirebasePushNotification.Abstractions;
using Converse.Services;
using Converse.Database;

namespace Converse.ViewModels.Register
{
    public class RegisterPageViewModel : ViewModelBase
    {
        public ICommand ContinueCommand { get; private set; }
        public ICommand SelectProfilePictureCommand { get; private set; }

        public Wallet Wallet { get; set; }
        public List<string> RecoveryPhrase { get; private set; }

        public RegisterPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, IFirebasePushNotification firebasePushNotification,
                                       IUserDialogs userDialogs, SyncServerConnection syncServerConnection, TronConnection tronConnection, WalletManager walletManager, TokenMessagesQueueService tokenMessagesQueueService, ConverseDatabase converseDatabase)
                : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServerConnection, tronConnection, walletManager, tokenMessagesQueueService, converseDatabase)
        {
            Title = "Register";
            ContinueCommand = new DelegateCommand(ContinueCommandExecuted);
            SelectProfilePictureCommand = new DelegateCommand(SelectProfilePictureCommandExecuted);

            Wallet = _walletManager.CreateNewWalletAsync();
            Wallet.ProfileImageUrl = "baseline_person_grayish_48";
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

            await Task.Delay(1000);

            RecoveryPhrase = Wallet.Mnemonic.Split(' ').Select(p => p.Trim()).ToList();

            IsBusy = false;
        }

        async void ContinueCommandExecuted()
        {
            if (string.IsNullOrWhiteSpace(Wallet.Name))
            {
                await _pageDialogService.DisplayAlertAsync("Invalid Name", "Please enter a name", "Ok");
                return;
            }
            if (string.IsNullOrWhiteSpace(Wallet.ProfileImageUrl) || !Uri.IsWellFormedUriString(Wallet.ProfileImageUrl, UriKind.Absolute))
            {
                var res = await _userDialogs.ConfirmAsync("No profile picture, do you want to continue without?", "Profile Picture", "Yes", "No");
                if (!res)
                    return;
            }

            var navParams = new NavigationParameters();
            navParams.Add("RecoveryPhrase", RecoveryPhrase);
            await _navigationService.NavigateAsync("ConfirmRecoveryPhrasePage", navParams);
        }

        async void SelectProfilePictureCommandExecuted()
        {
            await CrossMedia.Current.Initialize();
            try
            {
                var selection = await _userDialogs.ActionSheetAsync("Profile Picture", "", null, null, "Take Photo", "Select from Gallery", "Clear");

                _userDialogs.ShowLoading();
                MediaFile file = null;
                if (selection.Equals("Take Photo"))
                {
                    if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                    {
                        _userDialogs.Toast("No camera available");
                        return;
                    }

                    file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                    {
                        SaveToAlbum = true,
                        Directory = "Converse",
                        PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                        MaxWidthHeight = 1024,
                        CompressionQuality = 50
                    });
                }
                else if (selection.Equals("Select from Gallery"))
                {
                    if (!CrossMedia.Current.IsPickPhotoSupported)
                    {
                        _userDialogs.Toast("Not supported");
                        return;
                    }
                    file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                    {
                        PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                        MaxWidthHeight = 1024,
                        CompressionQuality = 50
                    });
                }
                else if(selection.Equals("Clear"))
                {
                    Wallet.ProfileImageUrl = "baseline_person_grayish_48";
                }

                if (file != null)
                {
                    var stream = file.GetStream();
                    var storedImageUrl = await new FirebaseStorage("converse-8a53c.appspot.com")
                                                    .Child("users")
                                                    .Child(_walletManager.Wallet.Address)
                                                    .Child("images")
                                                    .Child(EncryptProvider.Md5(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"), MD5Length.L16)
                                                                                + $".{file.Path.Split('.').Last()}")
                                                    .PutAsync(stream);
                    if(!string.IsNullOrWhiteSpace(storedImageUrl))
                    {
                        Wallet.ProfileImageUrl = storedImageUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                _userDialogs.Toast("Image upload failed");
            }
            _userDialogs.HideLoading();
        }

    }
}
