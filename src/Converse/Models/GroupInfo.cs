using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Converse.Models
{
    public class GroupInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [JsonProperty("id")]
        public int GroupID { get; set; }

        [JsonProperty("address")]
        public string TronAddress { get; set; }

        [JsonProperty("priv_key")]
        public byte[] PrivateKey { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image")]
        public Uri ImageUri { get; set; }

        [JsonProperty("users")]
        public List<UserInfo> Users { get; set; }
    }
}
