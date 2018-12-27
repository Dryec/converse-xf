using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Converse.Models
{
    public class GroupInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [JsonProperty("id"), DefaultValue(0)]
        public int GroupID { get; set; }

        [JsonProperty("address"), DefaultValue("")]
        public string TronAddress { get; set; }

        [JsonProperty("private_key")]
        public byte[] PrivateKey { get; set; }

        [JsonProperty("public_key")]
        public byte[] PublicKey { get; set; }

        [JsonProperty("name"), DefaultValue("")]
        public string Name { get; set; }

        [JsonProperty("description"), DefaultValue("")]
        public string Description { get; set; }

        [JsonProperty("image"), DefaultValue("baseline_group_grayish_48")]
        public string ImageUrl { get; set; }

        [JsonProperty("is_public"), DefaultValue(false)]
        public bool IsPublic { get; set; }

        [JsonIgnore]
        public bool IsPrivate => !IsPublic;

        [JsonProperty("users")]
        public List<UserInfo> Users { get; set; }

        public GroupInfo()
        {
            GroupID = 0;
            TronAddress = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            ImageUrl = "baseline_group_grayish_48";
            Users = new List<UserInfo>();
        }
    }
}
