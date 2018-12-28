using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Converse.Enums;
using Newtonsoft.Json;

namespace Converse.Models
{
    public class ChatEntry : INotifyPropertyChanged, IComparable
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

        public void UpdateUnreadMessageCount(int lastReadMessageID)
        {
            if (LastMessage != null)
            {
                UnreadMessagesCount = LastMessage.ID - lastReadMessageID;
            }
            else
            {
                UnreadMessagesCount = MessageCount;
            }
        }

        public int CompareTo(object obj)
        {
            if (obj is ChatEntry otherEntry)
            {
                try
                {
                    if (otherEntry.LastMessage == null)
                        return 1;
                    return LastMessage.Timestamp.CompareTo(otherEntry.LastMessage.Timestamp);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

            }
            return 0;
        }
    }
}