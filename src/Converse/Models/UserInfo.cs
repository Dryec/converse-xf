using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Converse.Models
{
    public class UserInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [JsonProperty("id")]
        public int UserID { get; set; }

        [JsonProperty("address")]
        public string TronAddress { get; set; }

        [JsonProperty("public_key")]
        public byte[] PublicKey { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image")]
        public string ImageUrl { get; set; }

        [JsonProperty("status")]
        public UserStatus Status { get; set; }
    }
}
