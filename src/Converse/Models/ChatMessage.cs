using System;
using System.ComponentModel;

namespace Converse.Models
{
    public class ChatMessage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public User Sender { get; set; }

        public string Message { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
