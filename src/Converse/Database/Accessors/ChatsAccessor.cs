using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Converse.Database.Models;
using Converse.Models;
using Protocol;
using SQLite;

namespace Converse.Database.Accessors
{
    public class ChatsAccessor : AccessorBase
    {
        public ChatsAccessor(SQLiteAsyncConnection database) : base(database)
        {
            _database.CreateTableAsync<Chat>().Wait();
        }

        public async Task<int> Insert(ChatEntry chatEntry)
        {
            var p = Chat.FromChatEntry(chatEntry);
            if(p == null)
            {
                return -1;
            }
            var insertedRows = await _database.InsertAsync(p);
            return insertedRows == 1 ? p.ID : -1;
        }

        public async Task<int> Insert(Chat chat)
        {
            if (chat == null)
            {
                return -1;
            }

            var insertedRows = await _database.InsertAsync(chat);
            return insertedRows == 1 ? chat.ID : -1;
        }

        public async Task<int> Update(ChatEntry chatEntry)
        {
            var chat = Chat.FromChatEntry(chatEntry);
            if (chat == null)
            {
                return -1;
            }
            var dbEntry = await _database.FindAsync<Chat>(c => c.ChatID == chat.ChatID);
            if(dbEntry == null)
            {
                return await _database.InsertAsync(chat);
            }

            chat.ID = dbEntry.ID;
            return await _database.UpdateAsync(chat);
        }

        public async Task<int> Update(Chat chat)
        {
            if (chat == null)
            {
                return -1;
            }
            return await _database.UpdateAsync(chat);
        }

        public async Task<int> GetCount()
        {
            return await _database.Table<Chat>().CountAsync();
        }

        public async Task<Chat> GetFirst()
        {
            return await _database.Table<Chat>().FirstOrDefaultAsync();
        }

        public async Task<Chat> GetLast()
        {
            return await _database.Table<Chat>().OrderByDescending(p => p.ID).FirstOrDefaultAsync();
        }

        public async Task<List<Chat>> GetAll()
        {
            return await _database.Table<Chat>().ToListAsync();
        }

        public async Task<Chat> Get(int id)
        {
            return await _database.Table<Chat>().FirstOrDefaultAsync(p => p.ID == id);
        }

        public async Task<Chat> GetByChatID(int id)
        {
            return await _database.Table<Chat>().FirstOrDefaultAsync(p => p.ChatID == id);
        }

        public async Task<int> Delete(int id)
        {
            return await _database.DeleteAsync<Chat>(id);
        }
    }
}
