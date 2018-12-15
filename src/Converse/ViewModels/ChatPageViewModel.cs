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
using Converse.Database;

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

        public ICommand SendMessageCommand { get; set; }

        public ChatEntry Chat { get; set; }
        public UserInfo MySelf { get; set; }
        //public UserInfo ChatPartner { get; set; }
        public ObservableCollection<ChatMessage> Messages { get; set; }
        public ChatMessage SelectedMessage { get; set; }

        public string Message { get; set; }

        public double LastScrollY { get; set; }
        public double ScrollY { get; set; }
        public Size ScrollViewSize { get; set; }
        public double ScrollViewHeight { get; set; }

        public ChatPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, IFirebasePushNotification firebasePushNotification, IUserDialogs userDialogs,
                                    SyncServerConnection syncServer, TokenMessagesQueueService tokenMessagesQueueService, TronConnection tronConnection, WalletManager walletManager, ConverseDatabase converseDatabase)
            : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServer, tronConnection, walletManager, tokenMessagesQueueService, converseDatabase)
        {
            MySelf = new UserInfo { Name = "Dave", TronAddress = "TERA14Y1HBpcEf1iCaDyeSpMady9MuDM2o" };
            //Title = ChatPartner.Name;

            Message = string.Empty;
            Messages = new ObservableCollection<ChatMessage>();

            SendMessageCommand = new DelegateCommand(SendMessage);

            PropertyChanged += ChatPageViewModel_PropertyChanged;
        }

        async void SendMessage()
        {
            await _tokenMessagesQueue.AddAsync(
                    _walletManager.Wallet.Address,
                    Chat.ChatPartner.TronAddress,
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

        async Task LoadNewMessages()
        {
            if (Chat.ID == 0)
            {
                return;
            }

            Chat = await _syncServer.GetChatAsync(Chat.ID, _walletManager.Wallet.Address);

            var start = Messages.Count != 0 ? Messages.Last().ID + 1 : 1;
            var end = Chat.LastMessage != null ? Chat.LastMessage.ID : 0;

            if (start > end)
            {
                return;
            }

            var messages = await _syncServer.GetMessagesAsync(Chat.ID, start, end);
            if (messages != null)
            {
                var needScrollDown = ScrollY + ScrollViewHeight >= ScrollViewSize.Height - 80;
                foreach (var message in messages.Messages)
                {
                    message.IsSender = message.Sender.TronAddress == _walletManager.Wallet.Address;
                    Messages.Add(message);
                }

                //Messages = new ObservableCollection<ChatMessage>(messages.Messages);

                if (needScrollDown)
                {
                    await Task.Delay(100);
                    Debug.WriteLine(ScrollViewSize, "Scrolling Down");
                    ScrollMessagesEvent(this, new ScrollEventArgs(Messages.Count, true));
                }
            }
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
        }

        public override async void OnNavigatingTo(INavigationParameters parameters)
        {
            UserInfo chatPartner = null;
            if (parameters.TryGetValue(KnownNavigationParameters.XamlParam, out object data)
                || parameters.TryGetValue("address", out data))
            {
                if (data is string address && WalletAddress.Decode58Check(address) != null)
                {
                    var user = await _database.Users.GetByAddress(address);
                    if (user != null)
                    {
                        chatPartner = user.ToUserInfo();
                    }
                    else
                    {
                        chatPartner = await _syncServer.GetUserAsync(address);
                    }
                }
                else if (data is ItemTappedEventArgs itemTappedEventArgs)
                {
                    if (itemTappedEventArgs.ItemData is ChatEntry chatEntry && chatEntry.Type == Enums.ChatType.Normal)
                    {
                        Chat = chatEntry;
                    }
                }
            }

            if (Chat == null && chatPartner != null)
            {
                Chat = new ChatEntry { ID = 0, ChatPartner = chatPartner };
            }

            if (Chat != null)
            {
                Title = Chat.ChatPartner.Name;
            }
            else
            {
                _userDialogs.Toast("not an user");
                await _navigationService.GoBackAsync();
            }
        }

        bool ru = false; // TODO remove
        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            if (Chat.ID > 0)
            {
                var messages = await _database.ChatMessages.GetFromChatID(Chat.ID, 3);

                var chatMessages = new List<ChatMessage>();
                foreach (var chatMessage in messages)
                {
                    var m = chatMessage.ToChatMessage();
                    m.IsSender = m.Sender.TronAddress == _walletManager.Wallet.Address;
                    chatMessages.Add(m);
                }
                Messages = new ObservableCollection<ChatMessage>(chatMessages);

                Device.StartTimer(new TimeSpan(0, 0, 0, 0, 100), () =>
                {
                    if (ru == false)
                    {
                        ru = true;
                        Device.BeginInvokeOnMainThread(async () =>
                           {
                               await LoadNewMessages();
                               ru = false;
                           }); 
                    }
                    return true;
                });
            }

            /*var messagesx = await _syncServer.GetMessagesAsync();

foreach (var message in messagesx.Messages)
{
    message.IsSender = message.Sender.TronAddress.Equals(MySelf.TronAddress);
}

messagesx.Messages.ForEach(Messages.Add);
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
