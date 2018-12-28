using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using Converse.Models;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Firebase.Storage;
using NETCore.Encrypt;
using Converse.Tron;
using Converse.TokenMessages;
using Converse.Helpers;

namespace Converse.ViewModels
{
    public class CreateGroupPopupPageViewModel : ViewModelBase
    {
        public DelegateCommand SelectProfilePictureCommand { get; private set; }
        public DelegateCommand CreateCommand { get; private set; }

        public GroupInfo Group { get; set; }

        public CreateGroupPopupPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, Plugin.FirebasePushNotification.Abstractions.IFirebasePushNotification firebasePushNotification, Acr.UserDialogs.IUserDialogs userDialogs, Services.SyncServerConnection syncServerConnection, Services.TronConnection tronConnection, Tron.WalletManager walletManager, Services.TokenMessagesQueueService tokenMessagesQueueService, Database.ConverseDatabase converseDatabase) : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServerConnection, tronConnection, walletManager, tokenMessagesQueueService, converseDatabase)
        {
            Group = new GroupInfo();
            SelectProfilePictureCommand = new DelegateCommand(SelectProfilePictureCommandExecuted);
            CreateCommand = new DelegateCommand(CreateCommandExecuted);
        }

        async void CreateCommandExecuted()
        {
            Group.Name = Group.Name.Trim();
            Group.Description = Group.Description.Trim();
            var isDescriptionSet = !string.IsNullOrWhiteSpace(Group.Description);
            var isImageSet = (!string.IsNullOrWhiteSpace(Group.ImageUrl)) && Uri.IsWellFormedUriString(Group.ImageUrl, UriKind.Absolute);

            if (string.IsNullOrWhiteSpace(Group.Name))
            {
                await _userDialogs.AlertAsync("Enter a name", "Invalid Name", "Ok");
                return;
            }
            if (!isImageSet)
            {
                var res = await _userDialogs.ConfirmAsync("No group picture, do you want to continue without?", "Group Picture", "Yes", "No");
                if (!res)
                    return;
            }
            if (string.IsNullOrWhiteSpace(Group.Description))
            {
                var res = await _userDialogs.ConfirmAsync("No description, do you want to continue without?", "Group Description", "Yes", "No");
                if (!res)
                    return;
            }

            _userDialogs.ShowLoading();

            var wallet = _walletManager.Wallet;
            var groupWallet = new Wallet();

            var message = new CreateGroupTokenMessage
            {
                Name = wallet.Encrypt(Group.Name, AppConstants.PropertyAddressPublicKey),

                Description = isDescriptionSet ? wallet.Encrypt(Group.Description, AppConstants.PropertyAddressPublicKey) : null,

                ImageUrl = isImageSet ? wallet.Encrypt(Group.ImageUrl, AppConstants.PropertyAddressPublicKey) : null,

                GroupAddress = wallet.Encrypt(groupWallet.Address, AppConstants.PropertyAddressPublicKey),

                PublicKey = wallet.Encrypt(groupWallet.PublicKey, AppConstants.PropertyAddressPublicKey),

                PrivateKey = Group.IsPublic ? wallet.Encrypt(groupWallet.PrivateKey, AppConstants.PropertyAddressPublicKey) 
                                            : wallet.Encrypt(groupWallet.PrivateKey, groupWallet.PublicKey),

                IsPublic = Group.IsPublic
            };

            var pendingId = await _tokenMessagesQueue.AddAsync(wallet.Address, AppConstants.PropertyAddress, message);

            // Send init message
            await _tokenMessagesQueue.AddAsync(
                    _walletManager.Wallet.Address,
                    groupWallet.Address,
                    new SendGroupMessageTokenMessage { Message = new ExtendedMessage {
                        Message = "- Group Created -",
                        Timestamp = DateTime.Now,
                    }.Encrypt(_walletManager.Wallet, groupWallet.PublicKey) }
                );

            var waitResult = await _tokenMessagesQueue.WaitForAsync(pendingId);

            _fcm.Subscribe($"{AppConstants.FCM.Topics.Group}_{groupWallet.Address}");

            if (waitResult)
            {
                await _userDialogs.AlertAsync("Your group is pending and will appear soon in your chat overview", "Group Pending", "Ok");
            }
            else
            {
                await _userDialogs.AlertAsync("Could not send your group request yet, it will be automatically send when connection is etablished and appear then in your chat overview.", "Group Pending", "Ok");
            }

            _userDialogs.HideLoading();
            await _navigationService.GoBackAsync();
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
                    Group.ImageUrl = "baseline_group_grayish_48";
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
                        Group.ImageUrl = storedImageUrl;
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
