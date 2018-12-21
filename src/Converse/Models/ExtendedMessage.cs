using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Converse.Tron;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities.Encoders;

namespace Converse.Models
{
    public class ExtendedMessage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [JsonProperty("msg")]
        public string Message { get; set; }

        [JsonProperty("img", NullValueHandling = NullValueHandling.Ignore)]
        public string ImageUrl { get; set; }

        [JsonProperty("tmsp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("pid")]
        public int PendingID { get; set; }

        [JsonIgnore]
        public bool IsPending { get; set; }

        public byte[] Encrypt(Wallet wallet, byte[] otherKey)
        {
            return (wallet.Encrypt(JsonConvert.SerializeObject(this, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc }), otherKey));
        }
    }
}
