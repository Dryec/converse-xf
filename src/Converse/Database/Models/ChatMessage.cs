using System;
using Newtonsoft.Json;
using SQLite;

namespace Converse.Database.Models
{
    [Table("ChatMessages")]
    public class ChatMessage
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int ID { get; set; }

        [Column("chat_id")]
        public int ChatID { get; set; }

        [Column("message_id")]
        public int MessageID { get; set; }

        [Column("json")]
        public string Json { get; set; }

        public static ChatMessage FromChatMessage(Converse.Models.ChatMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return new ChatMessage { ChatID = message.ChatID, MessageID = message.ID, Json = JsonConvert.SerializeObject(message) };
        }

        public Converse.Models.ChatMessage ToChatMessage()
        {
            return JsonConvert.DeserializeObject<Converse.Models.ChatMessage>(Json);
        }
    }
}
