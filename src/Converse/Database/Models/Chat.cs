using System;
using Converse.Models;
using Newtonsoft.Json;
using SQLite;

namespace Converse.Database.Models
{
    [Table("Chats")]
    public class Chat
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int ID { get; set; }

        [Column("_id"), Unique]
        public int ChatID { get; set; }

        [Column("json")]
        public string Json{ get; set; }

        public static Chat FromChatEntry(ChatEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            return new Chat { ChatID = entry.ID, Json = JsonConvert.SerializeObject(entry) };
        }

        public ChatEntry ToChatEntry()
        {
            return JsonConvert.DeserializeObject<ChatEntry>(Json);
        }
    }
}
