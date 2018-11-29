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

namespace Converse.ViewModels
{
    public class ChatsOverviewPageViewModel : ViewModelBase
    {
        public ObservableCollection<ChatEntry> ChatEntries { get; private set; }

        public ChatsOverviewPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IDeviceService deviceService) : base(navigationService, pageDialogService, deviceService)
        {
            Title = "Converse";

            ChatEntries = new ObservableCollection<ChatEntry>();

            ChatEntries.Add(new ChatEntry
            {
                ChatPartner = new User { Name = "Pascal", ImageUri = new Uri("https://www.tron-society.com/img/team/pascal.jpg") },
                Type = ChatType.Normal,
                LastMessage = new ChatMessage { Message = "Hey man! How are you today?" }, UnreadMessagesCount = 6
            });
            ChatEntries.Add(new ChatEntry
            {
                ChatPartner = new User { Name = "Dave", ImageUri = new Uri("https://www.tron-society.com/img/team/dave.jpg") },
                Type = ChatType.Normal,
                LastMessage = new ChatMessage { Message = "See you on monday!" },
                UnreadMessagesCount = 0
            });
            ChatEntries.Add(new ChatEntry
            {
                ChatPartner = new User { Name = "Luca", ImageUri = new Uri("https://www.tron-society.com/img/team/luca.jpg") },
                Type = ChatType.Normal,
                LastMessage = new ChatMessage { Message = "Bye Bye" },
                UnreadMessagesCount = 0
            });
            ChatEntries.Add(new ChatEntry
            {
                ChatPartner = new User { Name = "Marius", ImageUri = new Uri("https://www.tron-society.com/img/team/marius.jpg") },
                Type = ChatType.Normal,
                LastMessage = new ChatMessage { Message = "Lol what the hell are you doin?" },
                UnreadMessagesCount = 43
            });
            ChatEntries.Add(new ChatEntry
            {
                ChatPartner = new User { Name = "Simon", ImageUri = new Uri("https://www.tron-society.com/img/team/simon.jpg") },
                Type = ChatType.Normal,
                LastMessage = new ChatMessage { Message = "When do you have time?" },
                UnreadMessagesCount = 2
            });
        }
    }
}
