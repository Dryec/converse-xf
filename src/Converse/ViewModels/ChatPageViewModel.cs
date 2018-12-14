using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using System.Collections.ObjectModel;
using Converse.Models;
using Converse.TokenMessages;
using Newtonsoft.Json;
using Converse.Services;
using Xamarin.Forms;
using System.Diagnostics;
using System.Windows.Input;
using Syncfusion.ListView.XForms;
using Converse.Tron;
using Plugin.FirebasePushNotification.Abstractions;
using Client;
using Acr.UserDialogs;
using ItemTappedEventArgs = Syncfusion.ListView.XForms.ItemTappedEventArgs;

namespace Converse.ViewModels
{
    public class ScrollEventArgs : EventArgs
    {
        public int Index { get; }
        public bool Animated { get; }

        public ScrollEventArgs(int index, bool animated)
        {
            Index = index;
            Animated = animated;
        }
    }

    public class ChatPageViewModel : ViewModelBase
    {

        public event EventHandler ScrollMessagesEvent;

        public ICommand LoadMoreCommand { get; set; }
        public ICommand SendMessageCommand { get; set; }

        public UserInfo MySelf { get; set; }
        public UserInfo ChatPartner { get; set; }
        public ObservableCollection<ChatMessage> Messages { get; set; }
        public ChatMessage SelectedMessage { get; set; }

        public string Message { get; set; }

        public double LastScrollY { get; set; }
        public double ScrollY { get; set; }
        public Size ScrollViewSize { get; set; }
        public double ScrollViewHeight { get; set; }

        public ChatPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, IFirebasePushNotification firebasePushNotification, IUserDialogs userDialogs,
                                    SyncServerConnection syncServer, TokenMessagesQueueService tokenMessagesQueueService, TronConnection tronConnection, WalletManager walletManager)
            : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServer, tronConnection, walletManager, tokenMessagesQueueService)
        {
            MySelf = new UserInfo { Name = "Dave", TronAddress = "TERA14Y1HBpcEf1iCaDyeSpMady9MuDM2o" };
            //Title = ChatPartner.Name;

            Message = string.Empty;
            Messages = new ObservableCollection<ChatMessage>();

            LoadMoreCommand = new DelegateCommand<SfListView>(LoadMoreMessages);
            SendMessageCommand = new DelegateCommand(SendMessage);

            PropertyChanged += ChatPageViewModel_PropertyChanged;

            /*
            for (var i = 0; i < 10; i++)
            {
                Messages.Add(new ChatMessage
                {
                    Message = "Hey!",
                    Sender = MySelf,
                    IsSender = true, Timestamp = DateTime.Now.AddDays(-2).AddSeconds(-854)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "Hey! What's up?",
                    Sender = ChatPartner,
                    Timestamp = DateTime.Now.AddDays(-1).AddSeconds(-263)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "Just wanted to ask you something about TRON, hope you have some time?",
                    Sender = MySelf,
                    IsSender = true,
                    Timestamp = DateTime.Now.AddMinutes(-35)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "My question is about Tron SRs, what are they and how do I vote for them?",
                    Sender = MySelf,
                    IsSender = true,
                    Timestamp = DateTime.Now.AddMinutes(-34)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "Sure I've time for you! Tron SRs represent the network, there are a total of 27 elected SRs which are handling the block production. To vote for them just open your wallet, freeze some TRX and search for you SR to distribute your votes.",
                    Sender = ChatPartner,
                    Timestamp = DateTime.Now.AddMinutes(-28)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "Awesome thanks! I'll have a look",
                    Sender = MySelf,
                    IsSender = true,
                    Timestamp = DateTime.Now.AddMinutes(-26)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "Your welcome!",
                    Sender = ChatPartner,
                    Timestamp = DateTime.Now.AddMinutes(-25)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "If you want to know more - feel free to ask",
                    Sender = ChatPartner,
                    Timestamp = DateTime.Now.AddMinutes(-12)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "Do you have a website?",
                    Sender = MySelf,
                    IsSender = true,
                    Timestamp = DateTime.Now.AddMinutes(-5)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "Sure",
                    Sender = ChatPartner,
                    Timestamp = DateTime.Now.AddMinutes(-4)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "https://tron-society.com",
                    Sender = ChatPartner,
                    Timestamp = DateTime.Now.AddMinutes(-4)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "Thanks I'll check it out",
                    Sender = MySelf,
                    IsSender = true,
                    Timestamp = DateTime.Now.AddMinutes(-2)
                });
                Messages.Add(new ChatMessage
                {
                    Message = "and I think I'll also vote for you!\ud83d\ude0c",
                    Sender = MySelf,
                    IsSender = true,
                    Timestamp = DateTime.Now.AddMinutes(-1)
                });
            }*/
        }

        async void SendMessage()
        {
            await _tokenMessagesQueueService.AddAsync(
                    _walletManager.Wallet.Address,
                    ChatPartner.TronAddress,
                    new SendMessageTokenMessage { Message = Message }
                );
            /*var messageToken = new SendMessageTokenMessage { Message = Message};

            var transaction = await _tronConnection.CreateTransactionFromTokenMessageAsync(messageToken, _walletManager.Wallet.Address, ChatPartner.TronAddress);

            _walletManager.Wallet.SignTransaction(transaction.Transaction);

            var result = await _tronConnection.Client.BroadcastTransactionAsync(transaction.Transaction);*/
        }

        void ChatPageViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ScrollY):
                    // Try Load previous messages if near top and scroll direction to top
                    if (ScrollY <= 500 && LastScrollY > ScrollY)
                    {
                        // TODO load previous messages
                        Debug.WriteLine("Load More");
                    }
                    LastScrollY = ScrollY;
                    break;
                default:
                    break;
            }
        }

        void LoadMoreMessages(SfListView obj)
        {
            Debug.WriteLine("LoadMore");
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
        }

        public override async void OnNavigatingTo(INavigationParameters parameters)
        {
            if (parameters.TryGetValue(KnownNavigationParameters.XamlParam, out object data)
                || parameters.TryGetValue("address", out data))
            {
                if (data is string address)
                {
                    // TODO
                    ChatPartner = await _syncServerConnection.GetUserAsync(address);
                }
                else if (data is ItemTappedEventArgs itemTappedEventArgs)
                {
                    if (itemTappedEventArgs.ItemData is ChatEntry chatEntry && chatEntry.Type == Enums.ChatType.Normal)
                    {
                        ChatPartner = chatEntry.ChatPartner;
                    }
                }

                if (ChatPartner != null)
                {
                    return;
                }
            }

            _userDialogs.Toast("not an user");
            await _navigationService.GoBackAsync();
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            /*var messages = await _syncServer.GetChatMessagesAsync();

            foreach (var message in messages.Messages)
            {
                message.IsSender = message.Sender.TronAddress.Equals(MySelf.TronAddress);
            }

            messages.Messages.ForEach(Messages.Add);
            //Messages = new ObservableCollection<ChatMessage>(messages.Messages);

            //await Task.Delay(1);
            Device.BeginInvokeOnMainThread(() => ScrollMessagesEvent(this, new ScrollEventArgs(Messages.Count, false)));

            var random = new Random();
            while (true)
            {
                var lastHeight = ScrollViewSize.Height;
                Messages.Add(new ChatMessage
                {
                    Message = WaffleGenerator.WaffleEngine.Text(1, false), //random.Next(2, int.MaxValue).ToString(),
                    Sender = MySelf,
                    IsSender = true,
                    Timestamp = DateTime.Now.AddMinutes(-1)
                });
                Debug.WriteLine(ScrollViewSize, "Before");

                Device.BeginInvokeOnMainThread(() =>
                {
                    Debug.WriteLine(ScrollViewSize, "After");
                    if (ScrollY+ScrollViewHeight >= ScrollViewSize.Height - (ScrollViewSize.Height - lastHeight) - 80)
                    {
                        Debug.WriteLine(ScrollViewSize, "Scrolling Down");
                        ScrollMessagesEvent(this, new ScrollEventArgs(Messages.Count, true));
                    }
                });
                await Task.Delay(1500);
            }*/
        }
    }
}
