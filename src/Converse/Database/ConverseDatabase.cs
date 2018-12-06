using System;
using Converse.Database.Accessors;
using SQLite;

namespace Converse.Database
{
    public class ConverseDatabase
    {
        SQLiteAsyncConnection _database { get; set; }
        public PendingTransactionsAccessor PendingTransactions { get; private set; }
        public ChatsAccessor Chats { get; private set; }

        public ConverseDatabase()
        {
        }

        public void Init(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);

            PendingTransactions = new PendingTransactionsAccessor(_database);
            Chats = new ChatsAccessor(_database);
        }
    }
}
