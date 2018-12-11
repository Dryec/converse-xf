using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Converse.Database.Models;
using Converse.TokenMessages;
using Protocol;
using SQLite;

namespace Converse.Database.Accessors
{
    public class PendingTokenMessagesAccessor : AccessorBase
    {
        public PendingTokenMessagesAccessor(SQLiteAsyncConnection database) : base(database)
        {
            _database.CreateTableAsync<PendingTokenMessage>().Wait();
        }

        public async Task<int> Insert(string sender, string receiver, TokenMessage tokenMessage)
        {
            var p = PendingTokenMessage.Create(sender, receiver, tokenMessage);
            if(p == null)
            {
                return -1;
            }
            var insertedRows = await _database.InsertAsync(p);
            return insertedRows == 1 ? p.ID : -1;
        }

        public async Task<int> Insert(PendingTokenMessage pendingTokenMessage)
        {
            if(pendingTokenMessage == null)
            {
                return -1;
            }
            var insertedRows = await _database.InsertAsync(pendingTokenMessage);
            return insertedRows == 1 ? pendingTokenMessage.ID : -1;
        }

        public async Task<int> GetCount()
        {
            return await _database.Table<PendingTokenMessage>().CountAsync();
        }

        public async Task<PendingTokenMessage> GetFirst()
        {
            return await _database.Table<PendingTokenMessage>().FirstOrDefaultAsync();
        }

        public async Task<PendingTokenMessage> GetFirst(string sender)
        {
            return await _database.Table<PendingTokenMessage>().FirstOrDefaultAsync(p => p.Sender == sender);
        }

        public async Task<PendingTokenMessage> GetLast()
        {
            return await _database.Table<PendingTokenMessage>().OrderByDescending(p => p.ID).FirstOrDefaultAsync();
        }

        public async Task<PendingTokenMessage> Get(int id)
        {
            return await _database.Table<PendingTokenMessage>().FirstOrDefaultAsync(p => p.ID == id);
        }

        public async Task<List<PendingTokenMessage>> GetByReceiver(string receiver)
        {
            return await _database.Table<PendingTokenMessage>().Where(p => p.Receiver == receiver).ToListAsync();
        }

        public async Task<int> Delete(int id)
        {
            return await _database.DeleteAsync<PendingTokenMessage>(id);
        }
    }
}
