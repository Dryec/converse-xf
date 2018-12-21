using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Converse.Database.Models;
using Converse.TokenMessages;
using Protocol;
using SQLite;

namespace Converse.Database.Accessors
{
    public class LastReadMessageIDsAccessor : AccessorBase
    {
        public LastReadMessageIDsAccessor(SQLiteAsyncConnection database) : base(database)
        {
            _database.CreateTableAsync<LastReadMessageID>().Wait();
        }

        public async Task<int> Insert(int chatID, int lastReadMessageID)
        {
            var p = new LastReadMessageID { ChatID = chatID, LastReadID = lastReadMessageID };
            var insertedRows = await _database.InsertAsync(p);
            return insertedRows == 1 ? p.ID : -1;
        }

        public async Task<int> Update(int chatID, int lastReadMessageID)
        {
            var p = new LastReadMessageID { ChatID = chatID, LastReadID = lastReadMessageID };

            var dbEntry = await _database.FindAsync<LastReadMessageID>(c => c.ChatID == p.ChatID);
            if (dbEntry == null)
            {
                try
                {
                    return await _database.InsertAsync(p);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    return 0;
                }
            }
            p.ID = dbEntry.ID;
            return await _database.UpdateAsync(p);
        }

        public async Task<int> GetCount()
        {
            return await _database.Table<LastReadMessageID>().CountAsync();
        }

        public async Task<LastReadMessageID> GetFirst()
        {
            return await _database.Table<LastReadMessageID>().FirstOrDefaultAsync();
        }

        public async Task<LastReadMessageID> GetLast()
        {
            return await _database.Table<LastReadMessageID>().OrderByDescending(p => p.ID).FirstOrDefaultAsync();
        }

        public async Task<LastReadMessageID> Get(int id)
        {
            return await _database.Table<LastReadMessageID>().FirstOrDefaultAsync(p => p.ID == id);
        }

        public async Task<LastReadMessageID> GetByChatID(int id)
        {
            return await _database.Table<LastReadMessageID>().FirstOrDefaultAsync(p => p.ChatID == id);
        }

        public async Task<int> Delete(int id)
        {
            return await _database.DeleteAsync<LastReadMessageID>(id);
        }
    }
}
