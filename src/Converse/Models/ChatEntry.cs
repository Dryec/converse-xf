using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Converse.Enums;
using Newtonsoft.Json;

namespace Converse.Models
{
    public class ChatEntry : INotifyPropertyChanged
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("messages_count")]
        public int MessageCount { get; set; }

        // null if Type == Group
        [JsonProperty("chat_partner")]
        public UserInfo ChatPartner { get; set; }

        // null if Type == Normal
        [JsonProperty("group_info")]
        public GroupInfo GroupInfo { get; set; }

        [JsonProperty("type")]
        public ChatType Type { get; set; }

        [JsonProperty("last_message")]
        public ChatMessage LastMessage { get; set; }

        public bool HasUnreadMessages => UnreadMessagesCount > 0;

        public int UnreadMessagesCount { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}