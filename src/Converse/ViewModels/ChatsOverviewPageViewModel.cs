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
using Converse.Enums;
using Converse.Services;
using Converse.Tron;
using Plugin.FirebasePushNotification.Abstractions;
using Acr.UserDialogs;
using Converse.Database;
using Xamarin.Forms;
using Converse.Helpers;
using System.Diagnostics;
using Google.Protobuf;
using Client;
using Protocol;

namespace Converse.ViewModels
{
    public class ChatsOverviewPageViewModel : ViewModelBase
    {
        public DelegateCommand<object> OpenChatCommand { get; private set; }
        public DelegateCommand UpdateChatEntriesCommand { get; private set; }
        public DelegateCommand DismissBandwidthWarningCommand { get; private set; }

        public ExtendedObservableCollection<ChatEntry> ChatEntries { get; private set; }
        public bool AreChatsEmpty { get; set; }

        public bool IsBandwidthWarningVisible { get; set; }
        public bool IsBandwidthWarningDismissed { get; set; }
        public DateTime LastCheckBandwidthTime { get; set; }

        public ChatsOverviewPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, IUserDialogs userDialogs, IFirebasePushNotification firebasePushNotification,
                                            SyncServerConnection syncServer, TronConnection tronConnection, WalletManager walletManager, TokenMessagesQueueService tokenMessagesQueueService, ConverseDatabase converseDatabase)
                                : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServer, tronConnection, walletManager, tokenMessagesQueueService, converseDatabase)
        {
            Title = "Converse";
            Icon = "logo_icon_white_32";
            OpenChatCommand = new DelegateCommand<object>(OpenChatCommandExcecuted);
            UpdateChatEntriesCommand = new DelegateCommand(UpdateChatEntriesCommandExcecuted);
            DismissBandwidthWarningCommand = new DelegateCommand(DismissBandwidthWarningCommandExcecuted);

            MessagingCenter.Subscribe<TokenMessagesQueueService>(this, AppConstants.MessagingService.BandwidthError, (p) => CheckFreeUsage());
        }

        async void OpenChatCommandExcecuted(object obj)
        {
            if (obj is Syncfusion.ListView.XForms.ItemTappedEventArgs tappedEventArgs)
            {
                if (tappedEventArgs.ItemData is ChatEntry chat)
                {
                    var navParams = new NavigationParameters();
                    navParams.Add("ChatEntry", chat);
                    switch (chat.Type)
                    {
                        case ChatType.Normal:
                            await _navigationService.NavigateAsync("ChatPage", navParams);
                            break;
                        case ChatType.Group:
                            await _navigationService.NavigateAsync("GroupChatPage", navParams);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        void DismissBandwidthWarningCommandExcecuted()
        {
            IsBandwidthWarningDismissed = true;
            IsBandwidthWarningVisible = false;
        }

        async void CheckFreeUsage()
        {
            if (IsBandwidthWarningDismissed || (LastCheckBandwidthTime.AddSeconds(15) >= DateTime.Now))
            {
                return;
            }

            LastCheckBandwidthTime = DateTime.Now;

            try
            {
                var account = await _tronConnection.Client.GetAccountAsync(new Protocol.Account
                {
                    Address = ByteString.CopyFrom(WalletAddress.Decode58Check(_walletManager.Wallet.Address))
                });
                var accountNet = await _tronConnection.Client.GetAccountNetAsync(new Protocol.Account
                {
                    Address = ByteString.CopyFrom(WalletAddress.Decode58Check(_walletManager.Wallet.Address))
                });

                if (account != null && !account.Address.IsEmpty)
                {
                    var isBandwidthAvailable = false;
                    var token = await _tronConnection.Client.GetAssetIssueByNameAsync(new BytesMessage { Value = ByteString.CopyFromUtf8(AppConstants.TokenName) });


                    if (token != null && !token.OwnerAddress.IsEmpty)
                    {
                        // Check account free usage
                        if (accountNet.AssetNetUsed.ContainsKey(AppConstants.TokenName))
                        {
                            isBandwidthAvailable = (token.PublicFreeAssetNetLimit - accountNet.AssetNetUsed[AppConstants.TokenName]) >= 1000;
                        }
                        else
                        {
                            isBandwidthAvailable = false;
                        }

                        if (isBandwidthAvailable)
                        {
                            // Check total token free bandwidth
                            isBandwidthAvailable = token.FreeAssetNetLimit - token.PublicFreeAssetNetUsage >= 2500;
                        }

                        // Check own account bandwidth and balance
                        if (!isBandwidthAvailable)
                        {
                            isBandwidthAvailable = (accountNet.NetLimit + accountNet.FreeNetLimit - accountNet.NetUsed - accountNet.FreeNetUsed) >= 1000 || account.Balance > 20 * 1000;
                        }

                        IsBandwidthWarningVisible = !isBandwidthAvailable && !IsBandwidthWarningDismissed;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        async void UpdateChatEntriesCommandExcecuted()
        {
            var chats = await _syncServer.GetChatsAsync(_walletManager.Wallet.Address);

            if (chats != null)
            {
                for (var i = ChatEntries.Count - 1; i >= 0; i--)
                {
                    var chat = ChatEntries[i];
                    var stillExist = false;
                    foreach (var chat2 in chats)
                    {
                        stillExist = (chat.ID == chat2.ID);
                        if (stillExist)
                            break;
                    }
                    if (!stillExist)
                    {
                        var updateTopic = $"{AppConstants.FCM.Topics.Update}_{(chat.Type == ChatType.Normal ? chat.ChatPartner.TronAddress : chat.GroupInfo.TronAddress)}";
                        _fcm.Unsubscribe(updateTopic);

                        await _database.Chats.DeleteByChatID(chat.ID);

                        ChatEntries.RemoveAt(i);
                    }
                }

                foreach (var chat in chats)
                {
                    var alreadyExist = false;
                    if (chat.LastMessage != null)
                    {
                        if (chat.Type == ChatType.Normal)
                        {
                            chat.LastMessage.Decrypt(_walletManager.Wallet, chat.ChatPartner.PublicKey);
                        }
                        else if (chat.Type == ChatType.Group)
                        {
                            var privKey = _walletManager.Wallet.Decrypt(chat.GroupInfo.PrivateKey, chat.GroupInfo.PublicKey);
                            chat.LastMessage.Decrypt(privKey, chat.LastMessage.Sender.PublicKey);
                        }
                    }

                    var dbLastReadMessageID = await _database.LastReadMessageIDs.GetByChatID(chat.ID);
                    chat.UpdateUnreadMessageCount(dbLastReadMessageID != null ? dbLastReadMessageID.LastReadID : 0);

                    for (var i = 0; i < ChatEntries.Count; i++)
                    {
                        if (chat.ID == ChatEntries[i].ID)
                        {
                            ChatEntries.RemoveAt(i);
                            ChatEntries.Insert(i, chat);
                            alreadyExist = true;
                        }
                    }

                    if (!alreadyExist)
                    {
                        AreChatsEmpty = false;
                        ChatEntries.Insert(0, chat);
                    }

                    var updateTopic = $"{AppConstants.FCM.Topics.Update}_{(chat.Type == ChatType.Normal ? chat.ChatPartner.TronAddress : chat.GroupInfo.TronAddress)}";
                    if (!_fcm.SubscribedTopics.Contains(updateTopic))
                    {
                        _fcm.Subscribe(updateTopic);
                    }
                    if (chat.Type == ChatType.Group)
                    {
                        var msgTopic = $"{AppConstants.FCM.Topics.Group}_{chat.GroupInfo.TronAddress}";
                        if (!_fcm.SubscribedTopics.Contains(msgTopic))
                        {
                            _fcm.Subscribe(msgTopic);
                        }
                    }
                }

                ChatEntries.Sort(false);
            }
        }

        public override async void OnNavigatingTo(INavigationParameters parameters)
        {
            if (parameters.GetNavigationMode() == NavigationMode.New)
            {
                var dbChats = await _database.Chats.GetAll();

                AreChatsEmpty = dbChats.Count == 0;

                var chatEntries = new List<ChatEntry>();
                foreach (var dbChat in dbChats)
                {
                    var chatEntry = dbChat.ToChatEntry();
                    if (chatEntry.LastMessage != null)
                    {
                        if (chatEntry.Type == ChatType.Normal)
                        {
                            chatEntry.LastMessage.Decrypt(_walletManager.Wallet, chatEntry.ChatPartner.PublicKey);
                        }
                        else if (chatEntry.Type == ChatType.Group)
                        {
                            var privKey = _walletManager.Wallet.Decrypt(chatEntry.GroupInfo.PrivateKey, chatEntry.GroupInfo.PublicKey);
                            chatEntry.LastMessage.Decrypt(privKey, chatEntry.LastMessage.Sender.PublicKey);
                        }
                    }

                    var dbLastReadMessageID = await _database.LastReadMessageIDs.GetByChatID(chatEntry.ID);
                    chatEntry.UpdateUnreadMessageCount(dbLastReadMessageID != null ? dbLastReadMessageID.LastReadID : 0);

                    chatEntries.Add(chatEntry);
                }
                ChatEntries = new ExtendedObservableCollection<ChatEntry>(chatEntries);
                ChatEntries.Sort(false);
            }
            else if (parameters.GetNavigationMode() == NavigationMode.Back) // Navigated back to this page
            {
                try
                {
                    foreach (var chatEntry in ChatEntries)
                    {
                        var dbLastReadMessageID = await _database.LastReadMessageIDs.GetByChatID(chatEntry.ID);
                        chatEntry.UpdateUnreadMessageCount(dbLastReadMessageID != null ? dbLastReadMessageID.LastReadID : 0);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }

            CheckFreeUsage();
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.GetNavigationMode() == NavigationMode.New)
            {
                _fcm.OnNotificationReceived += _fcm_OnNotificationReceived;

                UpdateChatEntriesCommand.Execute();
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
            // TODO improve to not update everything on any kind of message
            Device.BeginInvokeOnMainThread(UpdateChatEntriesCommand.Execute);
        }
    }
}
