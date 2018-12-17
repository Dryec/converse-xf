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
using Converse.Helpers;

namespace Converse.ViewModels
{
    public class ScrollEventArgs : EventArgs
    {
        public int Index { get; }
        public bool Animated { get; }
        public Syncfusion.ListView.XForms.ScrollToPosition ScrollPosition { get; set; }

        public ScrollEventArgs(int index, bool animated, Syncfusion.ListView.XForms.ScrollToPosition scrollToPosition)
        {
            Index = index;
            Animated = animated;
            ScrollPosition = scrollToPosition;
        }
    }

    public class ChatPageViewModel : ViewModelBase
    {

        public event EventHandler ScrollMessagesEvent;

        public ICommand LoadMoreCommand { get; set; }
        public ICommand SendMessageCommand { get; set; }

        public ChatEntry Chat { get; set; }
        //public UserInfo MySelf { get; set; }

        public ObservableCollection<ChatMessage> Messages { get; set; }
        public ChatMessage SelectedMessage { get; set; }

        public string Message { get; set; }
        public bool IsLoadingPreviousMessages { get; set; }
        public bool IsLoadingNewMessages { get; set; }

        public LoadMoreOption LoadMoreOption { get; set; }
        public double LastScrollY { get; set; }
        public double ScrollY { get; set; }
        public Size ScrollViewSize { get; set; }
        public double ScrollViewHeight { get; set; }


        public ChatPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, IFirebasePushNotification firebasePushNotification, IUserDialogs userDialogs,
                                    SyncServerConnection syncServer, TokenMessagesQueueService tokenMessagesQueueService, TronConnection tronConnection, WalletManager walletManager, ConverseDatabase converseDatabase)
            : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServer, tronConnection, walletManager, tokenMessagesQueueService, converseDatabase)
        {
            Message = string.Empty;
            Messages = new ObservableCollection<ChatMessage>();

            LoadMoreCommand = new DelegateCommand<SfListView>(LoadMoreCommandExecuted);
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

            Message = string.Empty;
        }

        void ChatPageViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ScrollY):
                    // Try Load previous messages if near top and scroll direction to top
                    /*if (ScrollY <= 10 && LastScrollY > ScrollY)
                    {
                        // TODO load previous messages [Disabled bc scroll issue, using Load More Command]
                        //Debug.WriteLine("Load More");
                        //await LoadPreviousMessages();
                    }*/
                    LastScrollY = ScrollY;
                    break;
                default:
                    break;
            }
        }

        async void LoadMoreCommandExecuted(SfListView obj)
        {
            await LoadPreviousMessages();
        }

        async Task LoadPreviousMessages()
        {
            if (IsLoadingPreviousMessages || Chat.ID == 0)
            {
                return;
            }

            var start = Messages.Count != 0 ? Messages.First().ID >= 11 ? (Messages.First().ID - 10) : 1 : 1;
            var end = Messages.Count != 0 ? Messages.First().ID - 1 : 0;

            if (end < start)
            {
                Debug.WriteLine("Loaded all");
                return;
            }
            Debug.WriteLine($"Load: {start} -> {end}");
            IsLoadingPreviousMessages = true;

            var dbMessages = await _database.ChatMessages.GetFromChatID(Chat.ID, start, end);

            var addedRows = 0;
            if (dbMessages.Count < end - start + 1)
            {
                Debug.WriteLine("Load from sync server");
                var messages = await _syncServer.GetMessagesAsync(Chat.ID, start, end);
                if (messages != null)
                {
                    foreach (var message in messages.Messages)
                    {
                        if (message.ID >= start && message.ID <= end)
                        {
                            message.IsSender = message.Sender.TronAddress == _walletManager.Wallet.Address;
                            Messages.Insert(0, message);
                            addedRows++;
                        }
                    }
                }
            }
            else
            {
                Debug.WriteLine("Loading from db");
                dbMessages.Reverse();
                foreach (var dbMessage in dbMessages)
                {
                    var message = dbMessage.ToChatMessage();
                    message.IsSender = message.Sender.TronAddress == _walletManager.Wallet.Address;
                    Messages.Insert(0, message);
                    addedRows++;
                }
            }

            if (Messages.First()?.ID <= 1)
            {
                LoadMoreOption = LoadMoreOption.None;
            }
            else
            {
                // Add 1 for the Load More Row
                addedRows++;
            }

            Debug.WriteLine(ScrollViewSize, "Scrolling Down");
            Device.BeginInvokeOnMainThread(() => ScrollMessagesEvent(this, new ScrollEventArgs(addedRows, false, Syncfusion.ListView.XForms.ScrollToPosition.Start)));


            IsLoadingPreviousMessages = false;
        }

        async Task LoadNewMessages()
        {
            if (IsLoadingNewMessages || Chat.ID == 0)
            {
                return;
            }

            IsLoadingNewMessages = true;

            // Update chat to get latest message
            var chat = await _syncServer.GetChatAsync(Chat.ID, _walletManager.Wallet.Address);
            if (chat == null)
            {
                IsLoadingNewMessages = false;
                return;
            }

            Chat = chat;

            var loadCount = 40;

            // TODO check
            var start = Messages.Count != 0 ? Messages.Last().ID + 1 : Chat.LastMessage != null ? Chat.LastMessage.ID - loadCount : 1;
            start = start <= 0 ? 1 : start;
            var end = Chat.LastMessage != null ? Chat.LastMessage.ID : 0;

            if (start > end)
            {
                IsLoadingNewMessages = false;
                return;
            }

            var needMore = false;
            if (end - start > loadCount)
            {
                needMore = true;
                end = start + loadCount;
            }

            var messages = await _syncServer.GetMessagesAsync(Chat.ID, start, end);
            if (messages != null)
            {
                var needScrollDown = ScrollY + ScrollViewHeight >= ScrollViewSize.Height - 80;
                foreach (var message in messages.Messages)
                {
                    message.IsSender = message.Sender.TronAddress == _walletManager.Wallet.Address;
                    Device.BeginInvokeOnMainThread(() => Messages.Add(message));
                }


                if (needScrollDown)
                {
                    //await Task.Delay(100);
                    Debug.WriteLine(ScrollViewSize, "Scrolling Down");
                    Device.BeginInvokeOnMainThread(() => ScrollMessagesEvent(this, new ScrollEventArgs(Messages.Count, true, Syncfusion.ListView.XForms.ScrollToPosition.End)));
                    //ScrollMessagesEvent(this, new ScrollEventArgs(Messages.Count, true));
                }

                if (needMore)
                {
                    await LoadNewMessages();
                }
            }
            IsLoadingNewMessages = false;
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
        }

        public override async void OnNavigatingTo(INavigationParameters parameters)
        {
            if (Chat != null && Chat.ID > 0)
            {
                // Already loaded
                return;
            }

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

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            _fcm.OnNotificationReceived += _fcm_OnNotificationReceived;

            if (Chat.ID > 0 && Messages.Count == 0)
            {
                var messages = await _database.ChatMessages.GetLatestFromChatID(Chat.ID, 10);

                var chatMessages = new List<ChatMessage>();
                foreach (var chatMessage in messages)
                {
                    var m = chatMessage.ToChatMessage();
                    m.IsSender = m.Sender.TronAddress == _walletManager.Wallet.Address;
                    chatMessages.Add(m);
                }
                Messages = new ObservableCollection<ChatMessage>(chatMessages);

                if (Messages.Count > 0 && Messages.First().ID > 1)
                {
                    LoadMoreOption = LoadMoreOption.Manual;
                }

                Device.BeginInvokeOnMainThread(() => ScrollMessagesEvent(this, new ScrollEventArgs(Messages.Count, false, Syncfusion.ListView.XForms.ScrollToPosition.End)));

            }

            if (Chat.ID > 0)
            {
                await LoadNewMessages();
            }
        }

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            _fcm.OnNotificationReceived -= _fcm_OnNotificationReceived;
        }

        async void _fcm_OnNotificationReceived(object source, FirebasePushNotificationDataEventArgs e)
        {
            if (Chat.ID == 0)
            {
                if (e.Data.ContainsKey("type") && e.Data.ContainsKey("data"))
                {
                    var tagObj = e.Data["type"];
                    var dataObj = e.Data["data"];
                    if (tagObj is string tag && tag == AppConstants.FCMTags.Message)
                    {
                        if (dataObj is string data)
                        {
                            try
                            {
                                var chatMessage = JsonConvert.DeserializeObject<ChatMessage>(data);
                                Chat.ID = chatMessage.ChatID;
                            }
                            catch (JsonException ex)
                            {
                                Debug.WriteLine(ex);
                            }
                        }
                    }
                }
            }

            await LoadNewMessages();
        }

    }
}
