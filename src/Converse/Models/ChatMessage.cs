using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Converse.Tron;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities.Encoders;

namespace Converse.Models
{
    public class ChatMessages : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [JsonProperty("id")]
        public int ID { get; set; } // Chat ID

        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("end")]
        public int End { get; set; }

        [JsonProperty("messages")]
        public List<ChatMessage> Messages { get; set; }
    }

    public class ChatMessage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("trans_id")]
        public string TransactionID { get; set; }

        //public int MessageID { get; set; }

        [JsonProperty("chat_id")]
        public int ChatID { get; set; }

        [JsonProperty("is_sender")]
        public bool IsSender { get; set; }

        [JsonProperty("sender")]
        public UserInfo Sender { get; set; }

        [JsonProperty("message")] // Should be an encrypted ExtendedMessage
        public byte[] Message { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        //[JsonProperty("pending_id")]
        //public int PendingID { get; set; }

        [JsonIgnore]
        public ExtendedMessage ExtendedMessage { get; set; }

        public void Decrypt(byte[] privateKey, byte[] otherKey)
        {
            if(privateKey != null && privateKey.Length > 0)
            {
                var wallet = new Wallet(privateKey);
                Decrypt(wallet, otherKey);
            }
        }

        public void Decrypt(Wallet wallet, byte[] otherKey)
        {
            try
            {
                ExtendedMessage = JsonConvert.DeserializeObject<ExtendedMessage>(wallet.DecryptToString(Message, otherKey), new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Local });
            }
            catch (Exception ex)
            {
                try
                {
                    ExtendedMessage = new ExtendedMessage
                    {
                        Message = wallet.DecryptToString(Message, otherKey),
                        Timestamp = Timestamp
                    };
                }
                catch (Exception ex2)
                {
                    try
                    {
                        ExtendedMessage = JsonConvert.DeserializeObject<ExtendedMessage>(Encoding.UTF8.GetString(Message));
                    }
                    catch (Exception ex3)
                    {
                        ExtendedMessage = new ExtendedMessage
                        {
                            Message = "…could not decrypt…",
                            Timestamp = Timestamp
                        };
                        Debug.WriteLine(ex3);
                    }
                    Debug.WriteLine(ex2);
                }
                Debug.WriteLine(ex);
            }
        }
    }
}
