using System;
using Newtonsoft.Json;
using SQLite;

namespace Converse.Database.Models
{
    [Table("Users")]
    public class User
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int ID { get; set; }

        [Column("user_id"), Unique]
        public int UserID { get; set; }

        [Column("address"), Unique]
        public string Address { get; set; }

        [Column("json")]
        public string Json { get; set; }

        public static User FromUserInfo(Converse.Models.UserInfo user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return new User { UserID = user.UserID, Address = user.TronAddress , Json = JsonConvert.SerializeObject(user) };
        }

        public Converse.Models.UserInfo ToUserInfo()
        {
            return JsonConvert.DeserializeObject<Converse.Models.UserInfo>(Json);
        }
    }
}
