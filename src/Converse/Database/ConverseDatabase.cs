using System;
using Converse.Database.Accessors;
using SQLite;

namespace Converse.Database
{
    public class ConverseDatabase
    {
        SQLiteAsyncConnection _database { get; set; }
        public PendingTokenMessagesAccessor PendingTokenMessages { get; private set; }
        public UserAccessor Users { get; private set; }
        public GroupsAccessor Groups { get; private set; }
        public ChatsAccessor Chats { get; private set; }
        public ChatMessagesAccessor ChatMessages { get; private set; }

        public ConverseDatabase()
        {
        }

        public void Init(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);

            PendingTokenMessages = new PendingTokenMessagesAccessor(_database);
            Chats = new ChatsAccessor(_database);
            Users = new UserAccessor(_database);
            Groups = new GroupsAccessor(_database);
            ChatMessages = new ChatMessagesAccessor(_database);
        }
    }
}
