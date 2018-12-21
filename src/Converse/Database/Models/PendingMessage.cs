using System;
using Client;
using Converse.TokenMessages;
using Google.Protobuf;
using Newtonsoft.Json;
using Protocol;
using SQLite;

namespace Converse.Database.Models
{
    [Table("PendingMessages")]
    public class PendingMessage
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int ID { get; set; }

        [Column("chat_id")]
        public int ChatID { get; set; }

        [Column("pending_id")]
        public int PendingID { get; set; }

        [Column("message")]
        public string Json { get; set; }

        public static PendingMessage FromExtendedMessage(int chatID, Converse.Models.ExtendedMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return new PendingMessage { ChatID = chatID, PendingID = message.PendingID, Json = JsonConvert.SerializeObject(message) };
        }

        public Converse.Models.ExtendedMessage ToExtendedMessage()
        {
            var msg = JsonConvert.DeserializeObject<Converse.Models.ExtendedMessage>(Json);
            msg.IsPending = true;
            return msg;
        }
    }
}
