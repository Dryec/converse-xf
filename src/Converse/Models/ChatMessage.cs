using System;
using System.ComponentModel;

namespace Converse.Models
{
    public class ChatMessage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSender { get; set; }

        public UserInfo Sender { get; set; }

        public string Message { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
