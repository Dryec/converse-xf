using System;
using System.ComponentModel;

namespace Converse.Models
{
    public class User : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string TronAddress { get; set; }

        public string Name { get; set; }

        public Uri ImageUri { get; set; }

        public UserStatus Status { get; set; }
    }
}
