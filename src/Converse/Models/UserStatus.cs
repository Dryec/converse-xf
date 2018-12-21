using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Converse.Models
{
    public class UserStatus : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [JsonProperty("message"), DefaultValue("")]
        public string Message { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
