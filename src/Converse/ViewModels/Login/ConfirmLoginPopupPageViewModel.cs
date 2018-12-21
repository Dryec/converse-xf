using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using Converse.Models;
using Converse.Helpers;
using Converse.TokenMessages;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Firebase.Storage;
using NETCore.Encrypt;

namespace Converse.ViewModels.Login
{
    public class ConfirmLoginPopupPageViewModel : ViewModelBase
    {
        public DelegateCommand ConfirmCommand { get; private set; }
        public DelegateCommand SelectProfilePictureCommand { get; private set; }

        public UserInfo User { get; set; }

        public ConfirmLoginPopupPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, Plugin.FirebasePushNotification.Abstractions.IFirebasePushNotification firebasePushNotification, Acr.UserDialogs.IUserDialogs userDialogs, Services.SyncServerConnection syncServerConnection, Services.TronConnection tronConnection, Tron.WalletManager walletManager, Services.TokenMessagesQueueService tokenMessagesQueueService, Database.ConverseDatabase converseDatabase) : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServerConnection, tronConnection, walletManager, tokenMessagesQueueService, converseDatabase)
        {
            User = new UserInfo
            {
                Name = string.Empty,
                ImageUrl = "baseline_person_grayish_48"
            };
            ConfirmCommand = new DelegateCommand(ConfirmCommandExecuted);
            SelectProfilePictureCommand = new DelegateCommand(SelectProfilePictureCommandExecuted);
        }

        public override async void OnNavigatingTo(INavigationParameters parameters)
        {
            if (parameters.TryGetValue(KnownNavigationParameters.XamlParam, out string address)
                || parameters.TryGetValue("address", out address))
            {
                User.TronAddress = address;
                var user = await _syncServer.GetUserAsync(address);
                if (user != null)
                {
                    User = user;
                }
            }
            else
            {
                await _navigationService.GoBackAsync();
            }
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
                else if (selection.Equals("Clear"))
                {
                    User.ImageUrl = "baseline_person_grayish_48";
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
                    if (!string.IsNullOrWhiteSpace(storedImageUrl))
                    {
                        User.ImageUrl = storedImageUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                _userDialogs.Toast("Image upload failed");
            }
            _userDialogs.HideLoading();
        }

        public async void ConfirmCommandExecuted()
        {
            if (IsBusy)
            {
                return;
            }

            var wallet = _walletManager.Wallet;

            wallet.Name = User.Name.Trim();
            wallet.ProfileImageUrl = User.ImageUrl;

            if (string.IsNullOrWhiteSpace(wallet.Name))
            {
                await _userDialogs.AlertAsync("Enter a name", "Invalid Name", "Ok");
                return;
            }
            if (string.IsNullOrWhiteSpace(wallet.ProfileImageUrl) || !Uri.IsWellFormedUriString(wallet.ProfileImageUrl, UriKind.Absolute))
            {
                var res = await _userDialogs.ConfirmAsync("No profile picture, do you want to continue without?", "Profile Picture", "Yes", "No");
                if (!res)
                    return;
            }

            IsBusy = true;
            _userDialogs.ShowLoading();


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

            if (User.Status == null)
            {
                // Set Status to default
                await _tokenMessagesQueue.AddAsync(
                                     wallet.Address,
                                     AppConstants.PropertyAddress,
                                     new ChangeStatusTokenMessage { Status = wallet.Encrypt(AppConstants.DefaultStatusMessage, AppConstants.PropertyAddressPublicKey) });
            }
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

    }
}
