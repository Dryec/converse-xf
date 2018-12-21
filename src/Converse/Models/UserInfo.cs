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

        [JsonProperty("name"), DefaultValue("")]
        public string Name { get; set; }

        [JsonProperty("image"), DefaultValue("baseline_person_grayish_48")]
        public string ImageUrl { get; set; }

        [JsonProperty("status"), DefaultValue(default(UserStatus))]
        public UserStatus Status { get; set; }
    }
}
