using System;
using System.ComponentModel;

namespace Converse.Models
{
    public class UserStatus : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Message { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
