using System;
using Google.Protobuf;
using Protocol;
using SQLite;

namespace Converse.Database.Models
{
    [Table("PendingTransactions")]
    public class PendingTransaction
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int ID { get; set; }

        [Column("raw")]
        public byte[] RawTransaction { get; set; }

        public Transaction ToTransaction()
        {
            if(RawTransaction == null)
            {
                return null;
            }
            return Transaction.Parser.ParseFrom(RawTransaction);
        }

        public static PendingTransaction FromTransaction(Transaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            return new PendingTransaction
            {
                RawTransaction = transaction.ToByteArray()
            };
        }
    }
}
