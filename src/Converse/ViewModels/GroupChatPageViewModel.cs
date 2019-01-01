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
using System.Text;

namespace Converse.ViewModels
{

    public class GroupChatPageViewModel : ViewModelBase
    {
        public event EventHandler ScrollMessagesEvent;

        public ICommand InteractMessageCommand { get; set; }
        public ICommand LoadMoreCommand { get; set; }
        public ICommand SendMessageCommand { get; set; }

        public ChatEntry Chat { get; set; }
        public Wallet GroupWallet { get; set; }

        public ObservableCollection<ChatMessage> Messages { get; set; }
        public ChatMessage SelectedMessage { get; set; }

        public string Message { get; set; }
        public int MaxMessageLength => AppConstants.MaxMessageLength;
        public bool LoadedMessages { get; set; }
        public bool IsLoadingPreviousMessages { get; set; }
        public bool IsLoadingNewMessages { get; set; }

        public LoadMoreOption LoadMoreOption { get; set; }
        public double LastScrollY { get; set; }
        public double ScrollY { get; set; }
        public Size ScrollViewSize { get; set; }
        public double ScrollViewHeight { get; set; }
        public int LastVisibleIndex { get; set; }

        Random _randomPendingID;

        public GroupChatPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, Plugin.FirebasePushNotification.Abstractions.IFirebasePushNotification firebasePushNotification, Acr.UserDialogs.IUserDialogs userDialogs, Services.SyncServerConnection syncServerConnection, Services.TronConnection tronConnection, Tron.WalletManager walletManager, Services.TokenMessagesQueueService tokenMessagesQueueService, Database.ConverseDatabase converseDatabase) : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServerConnection, tronConnection, walletManager, tokenMessagesQueueService, converseDatabase)
        {
            GroupWallet = null;
            _randomPendingID = new Random();
            Message = string.Empty;
            Messages = new ObservableCollection<ChatMessage>();

            LoadMoreCommand = new DelegateCommand<SfListView>(LoadMoreCommandExecuted);
            SendMessageCommand = new DelegateCommand(SendMessage);
            InteractMessageCommand = new DelegateCommand<ItemHoldingEventArgs>(InteractMessageCommandExecuted);

            PropertyChanged += ChatPageViewModel_PropertyChanged;
        }


        async void InteractMessageCommandExecuted(ItemHoldingEventArgs e)
        {
            if (e.ItemData is ChatMessage message)
            {
                var action = await _userDialogs.ActionSheetAsync($"Message from {message.Sender?.Name}", "Cancel", null, null, "Open Chat", "Copy");

                switch (action)
                {
                    case "Open Chat":
                        var navParams = new NavigationParameters();
                        navParams.Add("user", message.Sender);
                        await _navigationService.NavigateAsync("ChatPage", navParams);
                        break;
                    case "Copy":
                        await Xamarin.Essentials.Clipboard.SetTextAsync(message.ExtendedMessage.Message);
                        _userDialogs.Toast("Copied");
                        break;
                    default:
                        break;
                }
            }
        }

        async void SendMessage()
        {
            var message = Message.Trim();
            Message = string.Empty;

            if (string.IsNullOrWhiteSpace(message) && message.Length <= MaxMessageLength)
            {
                return;
            }

            // Create Message and add to pending table
            var id = -1;
            ExtendedMessage extendedMessage = null;
            while (id == -1)
            {
                extendedMessage = new ExtendedMessage
                {
                    Message = message,
                    Timestamp = DateTime.Now,
                    PendingID = _randomPendingID.Next(0, 999999), // this will be checked on arrival of new messages to not load duplicates
                    IsPending = true
                };
                id = await _database.PendingMessages.Insert(Chat.ID, extendedMessage);
            }

            // Add to message queue
            await _tokenMessagesQueue.AddAsync(
                    _walletManager.Wallet.Address,
                    Chat.GroupInfo.TronAddress, // TODO Check public Key
                    new SendGroupMessageTokenMessage { Message = extendedMessage.Encrypt(_walletManager.Wallet, Chat.GroupInfo.PublicKey) }
                );

            // Add to messages
            var lastMessage = Messages.Count > 0 ? Messages.Last() : null;
            Messages.Add(new ChatMessage
            {
                ExtendedMessage = extendedMessage,
                Timestamp = DateTime.Now,
                IsSender = true,
                ChatID = Chat.ID,
                ID = lastMessage != null ? lastMessage.ID : 0
            });


            Device.BeginInvokeOnMainThread(() => ScrollMessagesEvent(this, new ScrollEventArgs(Messages.Count, false, Syncfusion.ListView.XForms.ScrollToPosition.Start)));
        }

        async void ChatPageViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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
                case nameof(LastVisibleIndex):
                    if (Chat != null && Chat.ID > 0)
                    {
                        var dbLastReadMessage = await _database.LastReadMessageIDs.GetByChatID(Chat.ID);
                        var lastReadID = dbLastReadMessage != null ? dbLastReadMessage.LastReadID : 0;

                        var index = LastVisibleIndex - (LoadMoreOption != LoadMoreOption.None ? 1 : 0);

                        if (index >= 0 && index < Messages.Count && Messages[index].ID > lastReadID)
                        {
                            await _database.LastReadMessageIDs.Update(Chat.ID, Messages[index].ID);
                        }
                    }
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
            if (LoadedMessages && (IsLoadingPreviousMessages || Chat.ID == 0))
            {
                return;
            }

            var loadCount = 40;

            var start = Messages.Count != 0 ? Messages.First().ID >= loadCount + 1 ? (Messages.First().ID - loadCount) : 1 : 1;
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
                            message.Decrypt(GroupWallet, message.Sender.PublicKey);
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
                    message.Decrypt(GroupWallet, message.Sender.PublicKey);
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
            if (LoadedMessages && (IsLoadingNewMessages || Chat.ID == 0))
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

            var pendingMessagesCount = await _database.PendingMessages.GetCount(Chat.ID);
            var messages = await _syncServer.GetMessagesAsync(Chat.ID, start, end);
            if (messages != null)
            {
                var needScrollDown = ScrollY + ScrollViewHeight >= ScrollViewSize.Height - 80;
                foreach (var message in messages.Messages)
                {
                    message.IsSender = message.Sender.TronAddress == _walletManager.Wallet.Address;
                    message.Decrypt(GroupWallet, message.Sender.PublicKey);

                    foreach (var msg in Messages)
                    {
                        if (msg.ExtendedMessage?.IsPending == true && message.ExtendedMessage?.PendingID == msg.ExtendedMessage?.PendingID)
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                try
                                {
                                    Messages.Remove(msg);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex);
                                }
                            });
                        }
                    }
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        Messages.Add(message);

                        if (message.IsSender && message.ExtendedMessage != null && pendingMessagesCount > 0)
                        {
                            // try deleting from table
                            await _database.PendingMessages.Delete(Chat.ID, message.ExtendedMessage.PendingID);

                            // delete from messages list
                            /*var moved = 0;
                            for (var i = 0; i < Messages.Count - moved; i++)
                            {
                                if (Messages[i].ExtendedMessage?.IsPending == true)
                                {
                                    moved++;
                                    Messages.Move(i, Messages.Count - 1);
                                }
                            }*/
                        }
                    });
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

        public override async void OnNavigatingTo(INavigationParameters parameters)
        {
            if (parameters.GetNavigationMode() == NavigationMode.New)
            {
                if (parameters.TryGetValue<ChatEntry>("ChatEntry", out var chat))
                {
                    if (chat != null && chat.Type == Enums.ChatType.Group)
                    {
                        Chat = chat;
                        Title = Chat.GroupInfo.Name;
                        if (chat.GroupInfo.PrivateKey != null)
                        {
                            var privKey = Chat.GroupInfo.IsPublic ? Chat.GroupInfo.PrivateKey : _walletManager.Wallet.Decrypt(chat.GroupInfo.PrivateKey, chat.GroupInfo.PublicKey);
                            if (privKey != null)
                            {
                                GroupWallet = new Wallet(privKey);
                            }
                        }
                    }

                    if (GroupWallet == null)
                    {
                        _userDialogs.Toast("not a group");
                        await _navigationService.GoBackAsync();
                    }
                }
            }
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.GetNavigationMode() == NavigationMode.New)
            {
                _fcm.OnNotificationReceived += _fcm_OnNotificationReceived;

                Task.Run(async () =>
                {
                    var chatMessages = new List<ChatMessage>();
                    if (Chat.ID > 0 && Messages.Count == 0)
                    {
                        var messages = await _database.ChatMessages.GetLatestFromChatID(Chat.ID, 20);
                        var pendingMessages = await _database.PendingMessages.GetAll(Chat.ID);

                        foreach (var chatMessage in messages)
                        {
                            var m = chatMessage.ToChatMessage();
                            m.IsSender = m.Sender.TronAddress == _walletManager.Wallet.Address;
                            m.Decrypt(GroupWallet, m.Sender.PublicKey);
                            chatMessages.Add(m);
                        }

                        // Remove duplicates TODO make duplicate entries impossible
                        chatMessages = chatMessages.GroupBy(x => x.ID).Select(z => z.Last()).ToList();

                        var lastMessage = chatMessages.Count > 0 ? chatMessages.Last() : null;
                        foreach (var pendingMessage in pendingMessages)
                        {
                            var extendedMessage = pendingMessage.ToExtendedMessage();

                            var isStillPending = true;
                            foreach (var message in chatMessages)
                            {
                                if (message.ExtendedMessage?.PendingID == pendingMessage.PendingID)
                                {
                                    isStillPending = false;
                                    break;
                                }
                            }
                            if (isStillPending)
                            {
                                chatMessages.Add(new ChatMessage
                                {
                                    ExtendedMessage = extendedMessage,
                                    Timestamp = extendedMessage.Timestamp,
                                    IsSender = true,
                                    ChatID = Chat.ID,
                                    ID = lastMessage != null ? lastMessage.ID : 0
                                });
                            }
                            else
                            {
                                await _database.PendingMessages.Delete(Chat.ID, pendingMessage.PendingID);
                            }
                        }
                    }

                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        Messages = new ObservableCollection<ChatMessage>(chatMessages);
                        if (Messages.Count > 0 && Messages.First().ID > 1)
                        {
                            LoadMoreOption = LoadMoreOption.Manual;
                        }
                        await Task.Delay(50);

                        var lastReadId = await _database.LastReadMessageIDs.GetByChatID(Chat.ID);
                        var scrollIndex = lastReadId != null ?
                                            lastReadId.LastReadID < Messages.Count && lastReadId.LastReadID >= 0 ?
                                                lastReadId.LastReadID : Messages.Count
                                            : Messages.Count;
                        ScrollMessagesEvent(this, new ScrollEventArgs(scrollIndex, false, Syncfusion.ListView.XForms.ScrollToPosition.End));
                        LoadedMessages = true;

                        if (Chat.ID > 0 && IsActive)
                        {
                            await LoadNewMessages();
                        }
                    });
                });
            }
        }

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            if (parameters.GetNavigationMode() == NavigationMode.Back)
            {
                _fcm.OnNotificationReceived -= _fcm_OnNotificationReceived;
            }
        }

        void _fcm_OnNotificationReceived(object source, FirebasePushNotificationDataEventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (Chat.ID == 0)
                {
                    if (e.Data.ContainsKey("type") && e.Data.ContainsKey("data"))
                    {
                        var tagObj = e.Data["type"];
                        var dataObj = e.Data["data"];
                        if (tagObj is string tag && tag == AppConstants.FCM.Types.Message)
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
            });
        }
    }
}
