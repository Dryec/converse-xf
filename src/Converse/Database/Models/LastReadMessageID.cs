using System;
using Newtonsoft.Json;
using SQLite;

namespace Converse.Database.Models
{
    [Table("LastReadMessageIDs")]
    public class LastReadMessageID
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int ID { get; set; }

        [Column("chat_id"), Unique]
        public int ChatID { get; set; }

        [Column("last_message_id")]
        public int LastReadID { get; set; }
    }
}
