using System;
using Newtonsoft.Json;
using SQLite;

namespace Converse.Database.Models
{
    [Table("Groups")]
    public class Group
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int ID { get; set; }

        [Column("group_id")]
        public int GroupID { get; set; }

        [Column("address")]
        public string Address { get; set; }

        [Column("json")]
        public string Json { get; set; }

        public static Group FromGroupInfo(Converse.Models.GroupInfo group)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            return new Group { GroupID = group.GroupID, Address = group.TronAddress , Json = JsonConvert.SerializeObject(group) };
        }

        public Converse.Models.GroupInfo ToGroupInfo()
        {
            return JsonConvert.DeserializeObject<Converse.Models.GroupInfo>(Json);
        }
    }
}
