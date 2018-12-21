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

namespace Converse.ViewModels
{
    public class ChatsOverviewPageViewModel : ViewModelBase
    {
        public DelegateCommand UpdateChatEntriesCommand { get; private set; }

        public ObservableCollection<ChatEntry> ChatEntries { get; private set; }

        public ChatsOverviewPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService, IUserDialogs userDialogs, IFirebasePushNotification firebasePushNotification,
                                            SyncServerConnection syncServer, TronConnection tronConnection, WalletManager walletManager, TokenMessagesQueueService tokenMessagesQueueService, ConverseDatabase converseDatabase)
                                : base(navigationService, pageDialogService, deviceService, firebasePushNotification, userDialogs, syncServer, tronConnection, walletManager, tokenMessagesQueueService, converseDatabase)
        {
            Title = "Converse";
            Icon = "logo_icon_white_32";
            UpdateChatEntriesCommand = new DelegateCommand(UpdateChatEntriesCommandExcecuted);
        }

        async void UpdateChatEntriesCommandExcecuted()
        {
            var chats = await _syncServer.GetChatsAsync(_walletManager.Wallet.Address);

            if (chats != null)
            {
                foreach (var chat in chats)
                {
                    var alreadyExist = false;
                    if (chat.LastMessage != null)
                    {
                        chat.LastMessage.Decrypt(_walletManager.Wallet, chat.ChatPartner.PublicKey);
                    }

                    var dbLastReadMessageID = await _database.LastReadMessageIDs.GetByChatID(chat.ID);
                    chat.UpdateUnreadMessageCount(dbLastReadMessageID != null ? dbLastReadMessageID.LastReadID : 0);

                    for (var i = 0; i < ChatEntries.Count; i++)
                    {
                        if (chat.ID == ChatEntries[i].ID)
                        {
                            ChatEntries[i] = chat;
                            alreadyExist = true;
                        }
                    }
                    if (!alreadyExist)
                    {
                        ChatEntries.Insert(0, chat);
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

                var chatEntries = new List<ChatEntry>();
                foreach (var dbChat in dbChats)
                {
                    var chatEntry = dbChat.ToChatEntry();
                    if (chatEntry.LastMessage != null)
                    {
                        chatEntry.LastMessage.Decrypt(_walletManager.Wallet, chatEntry.ChatPartner.PublicKey);
                    }

                    var dbLastReadMessageID = await _database.LastReadMessageIDs.GetByChatID(chatEntry.ID);
                    chatEntry.UpdateUnreadMessageCount(dbLastReadMessageID != null ? dbLastReadMessageID.LastReadID : 0);

                    chatEntries.Add(chatEntry);
                }
                ChatEntries = new ObservableCollection<ChatEntry>(chatEntries);
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
