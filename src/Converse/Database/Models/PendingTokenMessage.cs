using System;
using Client;
using Converse.TokenMessages;
using Google.Protobuf;
using Newtonsoft.Json;
using Protocol;
using SQLite;

namespace Converse.Database.Models
{
    [Table("PendingTransactions")]
    public class PendingTokenMessage
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int ID { get; set; }

        [Column("sender")]
        public string Sender { get; set; }

        [Column("receiver")]
        public string Receiver { get; set; }

        [Column("data")]
        public string Data { get; set; }

        public static PendingTokenMessage Create(string sender, string receiver, TokenMessage tokenMessage)
        {
            if (WalletAddress.Decode58Check(sender) == null)
            {
                throw new ArgumentException("invalid address", nameof(sender));
            }
            if (WalletAddress.Decode58Check(receiver) == null)
            {
                throw new ArgumentException("invalid address", nameof(receiver));
            }
            if (tokenMessage == null)
            {
                throw new ArgumentNullException(nameof(tokenMessage));
            }

            return new PendingTokenMessage
            {
                Sender = sender,
                Receiver = receiver,
                Data = JsonConvert.SerializeObject(tokenMessage)
            };
        }
    }
}
