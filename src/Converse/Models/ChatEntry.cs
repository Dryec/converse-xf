using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Converse.Enums;

namespace Converse.Models
{
    public class ChatEntry : INotifyPropertyChanged
    {
        // null if Type == Group
        public UserInfo ChatPartner { get; set; }

        // null if Type == Normal
        public GroupInfo GroupInfo { get; set; }

        public ChatType Type { get; set; }

        public ChatMessage LastMessage { get; set; }

        public bool HasUnreadMessages => UnreadMessagesCount > 0;

        public int UnreadMessagesCount { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}