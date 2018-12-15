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
            UpdateChatEntriesCommand = new DelegateCommand(UpdateChatEntriesCommandExcecuted);

            /*for (var i = 0; i < 1; i++)
            {
                ChatEntries.Add(new ChatEntry
                {
                    ChatPartner = new UserInfo { Name = "Pascal", ImageUri = new Uri("https://www.tron-society.com/img/team/pascal.jpg") },
                    Type = ChatType.Normal,
                    LastMessage = new ChatMessage { Message = "Hey man! How are you today?", Timestamp = DateTime.Now.AddMinutes(-4) },
                    UnreadMessagesCount = 6
                });
                ChatEntries.Add(new ChatEntry
                {
                    ChatPartner = new UserInfo { Name = "Dave", ImageUri = new Uri("https://www.tron-society.com/img/team/dave.jpg") },
                    Type = ChatType.Normal,
                    LastMessage = new ChatMessage { Message = "See you on monday!", Timestamp = DateTime.Now.AddHours(-2) },
                    UnreadMessagesCount = 0
                });
                ChatEntries.Add(new ChatEntry
                {
                    GroupInfo = new GroupInfo { Name = "Tron Society", ImageUri = new Uri("https://www.tron-society.com/img/team/logo_en.png") },
                    Type = ChatType.Group,
                    LastMessage = new ChatMessage { Sender = new UserInfo { Name = "Padal" }, Message = "Any progress guys?", Timestamp = DateTime.Now.AddHours(-6) },
                    UnreadMessagesCount = 3
                });
                ChatEntries.Add(new ChatEntry
                {
                    ChatPartner = new UserInfo { Name = "Luca", ImageUri = new Uri("https://www.tron-society.com/img/team/luca.jpg") },
                    Type = ChatType.Normal,
                    LastMessage = new ChatMessage { Message = "Bye Bye", Timestamp = DateTime.Now.AddDays(-1) },
                    UnreadMessagesCount = 0
                });
                ChatEntries.Add(new ChatEntry
                {
                    ChatPartner = new UserInfo { Name = "Marius", ImageUri = new Uri("https://www.tron-society.com/img/team/marius.jpg") },
                    Type = ChatType.Normal,
                    LastMessage = new ChatMessage { Message = "Lol what the hell are you doin?", Timestamp = DateTime.Now.AddDays(-3) },
                    UnreadMessagesCount = 43
                });
                ChatEntries.Add(new ChatEntry
                {
                    ChatPartner = new UserInfo { Name = "Simon", ImageUri = new Uri("https://www.tron-society.com/img/team/simon.jpg") },
                    Type = ChatType.Normal,
                    LastMessage = new ChatMessage { Message = "When do you have time?", Timestamp = DateTime.Now.AddDays(-35) },
                    UnreadMessagesCount = 2
                });
            }*/
        }

        async void UpdateChatEntriesCommandExcecuted()
        {
            var chats = await _syncServer.GetChatsAsync(_walletManager.Wallet.Address);
            if (chats != null)
            {
                foreach (var chat in chats)
                {
                    var alreadyExist = false;
                    for (var i = 0; i < ChatEntries.Count; i++)
                    {
                        if (chat.ID == ChatEntries[i].ID)
                        {
                            ChatEntries[i] = chat;
                            alreadyExist = true;
                        }
                    }
                    if(!alreadyExist)
                    {
                        ChatEntries.Add(chat);
                    }
                }
            }
        }

        public override async void OnNavigatingTo(INavigationParameters parameters)
        {
            var dbChats = await _database.Chats.GetAll();

            var chatEntries = new List<ChatEntry>();
            foreach (var dbChat in dbChats)
            {
                var chatEntry = dbChat.ToChatEntry();
                chatEntries.Add(chatEntry);
            }
            ChatEntries = new ObservableCollection<ChatEntry>(chatEntries);
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            UpdateChatEntriesCommand.Execute();
        }
    }
}
