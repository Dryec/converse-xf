using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Converse.Models
{
    public class GroupInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public string TronAddress { get; set; }
        
        public string Name { get; set; }
        
        public Uri ImageUri { get; set; }

        public List<UserInfo> Users { get; set; }
    }
}
