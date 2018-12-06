using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Converse.Models
{
    public class ChatMessage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string TransactionID { get; set; }

        public int ChatID { get; set; }

        public bool IsSender { get; set; }

        public UserInfo Sender { get; set; }

        public string Message { get; set; }

        public DateTime Timestamp { get; set; }

        public int PendingID { get; set; }
    }
}
