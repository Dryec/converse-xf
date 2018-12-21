using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Converse.Database.Models;
using Converse.Models;
using Converse.TokenMessages;
using Protocol;
using SQLite;

namespace Converse.Database.Accessors
{
    public class PendingMessagesAccessor : AccessorBase
    {
        public PendingMessagesAccessor(SQLiteAsyncConnection database) : base(database)
        {
            _database.CreateTableAsync<PendingMessage>().Wait();
        }

        public async Task<int> Insert(int chatID, ExtendedMessage message)
        {
            var p = PendingMessage.FromExtendedMessage(chatID, message);
            if(p == null || await _database.Table<PendingMessage>().Where(f => f.ChatID == p.ChatID && f.PendingID == p.PendingID).CountAsync() > 0)
            {
                return -1;
            }
            var insertedRows = await _database.InsertAsync(p);
            return insertedRows == 1 ? p.ID : -1;
        }

        public async Task<int> GetCount()
        {
            return await _database.Table<PendingMessage>().CountAsync();
        }

        public async Task<int> GetCount(int chatID)
        {
            return await _database.Table<PendingMessage>().Where(p => p.ChatID == chatID).CountAsync();
        }

        public async Task<PendingMessage> GetFirst()
        {
            return await _database.Table<PendingMessage>().FirstOrDefaultAsync();
        }

        public async Task<PendingMessage> GetLast()
        {
            return await _database.Table<PendingMessage>().OrderByDescending(p => p.ID).FirstOrDefaultAsync();
        }

        public async Task<PendingMessage> Get(int id)
        {
            return await _database.Table<PendingMessage>().FirstOrDefaultAsync(p => p.ID == id);
        }

        public async Task<List<PendingMessage>> GetAll(int chatId)
        {
            return await _database.Table<PendingMessage>().Where(p => p.ChatID == chatId).ToListAsync();
        }

        public async Task<int> Delete(int id)
        {
            return await _database.DeleteAsync<PendingMessage>(id);
        }

        public async Task<int> Delete(int chatID, int pendingID)
        {
            return await _database.Table<PendingMessage>().Where(p => p.ChatID == chatID && p.PendingID == pendingID).DeleteAsync();
        }
    }
}
