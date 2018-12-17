using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using FFImageLoading.Forms;
using Plugin.Media;
using System.IO;
using Xamarin.Essentials;

namespace Converse.ViewModels
{
    public class ImagePopupPageViewModel : ViewModelBase
    {
        public DelegateCommand<CachedImage> SaveCommand { get; private set; }

        public string Source { get; set; }

        public ImagePopupPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, Plugin.FirebasePushNotification.Abstractions.IFirebasePushNotification firebasePushNotification, Acr.UserDialogs.IUserDialogs userDialogs, Services.SyncServerConnection syncServerConnection, Services.TronConnection tronConnection, Tron.WalletManager walletManager, Services.TokenMessagesQueueService tokenMessagesQueueService, Database.ConverseDatabase converseDatabase) : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServerConnection, tronConnection, walletManager, tokenMessagesQueueService, converseDatabase)
        {
            SaveCommand = new DelegateCommand<CachedImage>(SaveCommandExecuted);
        }

        public override void OnNavigatingTo(INavigationParameters parameters)
        {
            if (parameters.TryGetValue(KnownNavigationParameters.XamlParam, out string source)
                || parameters.TryGetValue("source", out source))
            {
                Source = source;
            }
        }

        async void SaveCommandExecuted(CachedImage image)
        {
            // TODO Save to Gallery
            var png = await image.GetImageAsPngAsync();
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            var fileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
            var fullPath = Path.Combine(path, fileName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            File.WriteAllBytes(fullPath, png);

            _userDialogs.Toast("Saved");
        }
    }
}
