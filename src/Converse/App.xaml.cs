using System;
using System.Threading.Tasks;
using Converse.Resources;
using Converse.Services;
using Converse.Views;
using Plugin.Multilingual;
using Prism;
using Prism.Ioc;
using Prism.Plugin.Popups;
using DryIoc;
using Prism.DryIoc;
using FFImageLoading.Helpers;
using Rg.Plugins.Popup.Contracts;
using Rg.Plugins.Popup.Services;
using BarcodeScanner;
using Prism.Logging;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Converse.ViewModels;
using Converse.Views.Register;
using Converse.ViewModels.Register;
using Converse.Tron;
using Converse.Database;
using Acr.UserDialogs;
using Plugin.FirebasePushNotification;
using Org.BouncyCastle.Security;
using Crypto;
using Client;
using System.Text;
using Org.BouncyCastle.Math;
using System.Linq;
using System.Diagnostics;
using Converse.Helpers;
using Converse.TokenMessages;
using Plugin.FirebasePushNotification.Abstractions;
using System.IO;
using Converse.ViewModels.Login;
using Converse.Views.Login;

namespace Converse
{
    public partial class App
    {
        /* 
         * NOTE: 
         * The Xamarin Forms XAML Previewer in Visual Studio uses System.Activator.CreateInstance.
         * This imposes a limitation in which the App class must have a default constructor. 
         * App(IPlatformInitializer initializer = null) cannot be handled by the Activator.
         */
        public App()
            : this(null)
        {
        }

        public App(IPlatformInitializer initializer)
            : this(initializer, false)
        {
        }

        public App(IPlatformInitializer initializer, bool setFormsDependencyResolver)
            : base(initializer, setFormsDependencyResolver)
        {
        }

        protected override async void OnInitialized()
        {
            InitializeComponent();
#if DEBUG
            // Handle Xamarin Form Logging events such as Binding Errors
            Log.Listeners.Add(new TraceLogListener());
#endif
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NTI3MjJAMzEzNjJlMzQyZTMwa0RuT0xkMzhFZ2daWFkyMXRpUk9xbkxkcEJndlU4dVI1UnlXd3NYWWxpUT0=");
            LogUnobservedTaskExceptions();
            AppResources.Culture = CrossMultilingual.Current.DeviceCultureInfo;

            /*{
                //var x1 = new ECKey();

                //var w1 = new WalletAddress(x1);
                //var dw1 = WalletAddress.Decode58Check(w1.ToString());

                var x = 0;
                while (true)
                {
                    var x2 = new ECKey();
                    var x2pub = x2.Pub.GetEncoded();
                    var sig = x2.Sign(Sha256.Hash(Encoding.UTF8.GetBytes("Test")));
                    var ecdsasig = new ECDSASignature(sig);

                    var recovered = ECKey.RecoverPubBytesFromSignature(ecdsasig, Sha256.Hash(Encoding.UTF8.GetBytes("Test")), false);
                    var comp = recovered.SequenceEqual(x2pub);
                    if(!comp)
                        break;
                    x++;
                    Debug.WriteLine("Try: " + x, "Sig");



                    var u1 = new ECKey();
                    var u2 = new ECKey();

                    var enc = u1.Encrypt(Encoding.UTF8.GetBytes("Das ist ein Test"), ECKey.GetPublicKeyParamsFromEncoded(u2.Pub.GetEncoded()));
                    var dec = u2.Decrypt(enc, ECKey.GetPublicKeyParamsFromEncoded(u1.Pub.GetEncoded()));
                    var decString = Encoding.UTF8.GetString(dec);
                }

                //var s1 = x1.GetSharedSecret(x2.GetPublicKeyParameters());
                //var s2 = x2.GetSharedSecret(x1.GetPublicKeyParameters());
                //var s3 = x2.GetSharedSecret(x2.GetPublicKeyParameters());

                //var eq = s1 == s2;
            }

    */
            CrossFirebasePushNotification.Current.Subscribe("test");

            CrossFirebasePushNotification.Current.OnTokenRefresh += FCM_OnTokenRefresh;
            //Handle notification when app is closed here
            CrossFirebasePushNotification.Current.OnNotificationReceived += (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine("Received", "Firebase");
           };
            CrossFirebasePushNotification.Current.OnNotificationOpened += (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine("Opened", "Firebase");
                foreach (var data in p.Data)
                {
                    System.Diagnostics.Debug.WriteLine($"{data.Key} : {data.Value}", "Firebase");
                }
            };
            CrossFirebasePushNotification.Current.OnNotificationAction += (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine("Action", "Firebase");

                if (!string.IsNullOrEmpty(p.Identifier))
                {
                    System.Diagnostics.Debug.WriteLine($"ActionId: {p.Identifier}", "Firebase");
                    foreach (var data in p.Data)
                    {
                        System.Diagnostics.Debug.WriteLine($"{data.Key} : {data.Value}", "Firebase");
                    }
                }
            };
            CrossFirebasePushNotification.Current.OnNotificationDeleted += (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine("Deleted", "Firebase");
            };

            await NavigationService.NavigateAsync("SplashScreenPage");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register the Popup Plugin Navigation Service
            containerRegistry.RegisterPopupNavigationService();

            // NOTE: Uses a Popup Page to contain the Scanner. You can optionally register 
            // the ContentPageBarcodeScannerService if you prefer a full screen approach.
            containerRegistry.RegisterSingleton<IBarcodeScannerService, PopupBarcodeScannerService>();
            containerRegistry.RegisterInstance<IUserDialogs>(UserDialogs.Instance);
            containerRegistry.RegisterInstance<IFirebasePushNotification>(CrossFirebasePushNotification.Current);
            containerRegistry.RegisterSingleton<TronConnection>();
            containerRegistry.RegisterSingleton<TokenMessagesQueueService>();
            containerRegistry.RegisterSingleton<ConverseDatabase>();
            containerRegistry.RegisterSingleton<WalletManager>();
            containerRegistry.RegisterSingleton<SyncServerConnection>();

            // Navigating to "TabbedPage?createTab=ViewA&createTab=ViewB&createTab=ViewC will generate a TabbedPage
            // with three tabs for ViewA, ViewB, & ViewC
            // Adding `selectedTab=ViewB` will set the current tab to ViewB
            containerRegistry.RegisterForNavigation<TabbedPage>();
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<MainPage, MainPageViewModel>();
            containerRegistry.RegisterForNavigation<SplashScreenPage, SplashScreenPageViewModel>();
            containerRegistry.RegisterForNavigation<WelcomePage, WelcomePageViewModel>();
            containerRegistry.RegisterForNavigation<LoginPage, LoginPageViewModel>();
            containerRegistry.RegisterForNavigation<ConfirmLoginPopupPage, ConfirmLoginPopupPageViewModel>();
            containerRegistry.RegisterForNavigation<RegisterInfoPage, RegisterInfoPageViewModel>();
            containerRegistry.RegisterForNavigation<RegisterPage, RegisterPageViewModel>();
            containerRegistry.RegisterForNavigation<ConfirmRecoveryPhrasePage, ConfirmRecoveryPhrasePageViewModel>();
            containerRegistry.RegisterForNavigation<ChatsOverviewPage, ChatsOverviewPageViewModel>();
            containerRegistry.RegisterForNavigation<ChatPage, ChatPageViewModel>();
            containerRegistry.RegisterForNavigation<SettingsPage, SettingsPageViewModel>();
            containerRegistry.RegisterForNavigation<AddChatOptionPopupPage, AddChatOptionPopupPageViewModel>();
            containerRegistry.RegisterForNavigation<QrCodePopupPage, QrCodePopupPageViewModel>();
            containerRegistry.RegisterForNavigation<UserPopupPage, UserPopupPageViewModel>();
            containerRegistry.RegisterForNavigation<ImagePopupPage, ImagePopupPageViewModel>();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle IApplicationLifecycle
            base.OnSleep();

            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle IApplicationLifecycle
            base.OnResume();

            // Handle when your app resumes
        }

        async void FCM_OnTokenRefresh(object source, Plugin.FirebasePushNotification.Abstractions.FirebasePushNotificationTokenEventArgs e)
        {

        }

        void LogUnobservedTaskExceptions()
        {
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                Container.Resolve<ILoggerFacade>().Log(e.Exception.ToString(), Category.Exception, Priority.None);
            };
        }
    }
}
