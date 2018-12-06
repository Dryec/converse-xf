using System;
using System.Threading.Tasks;
using Converse.Database.Models;
using Protocol;
using SQLite;

namespace Converse.Database.Accessors
{
    public class PendingTransactionsAccessor : AccessorBase
    {
        public PendingTransactionsAccessor(SQLiteAsyncConnection database) : base(database)
        {
            _database.CreateTableAsync<PendingTransaction>().Wait();
        }

        public async Task<int> Insert(Transaction transaction)
        {
            var p = PendingTransaction.FromTransaction(transaction);
            if(p == null)
            {
                return -1;
            }
            return await _database.InsertAsync(p);
        }

        public async Task<int> Insert(PendingTransaction transaction)
        {
            if(transaction == null)
            {
                return -1;
            }
            return await _database.InsertAsync(transaction);
        }

        public async Task<int> GetCount()
        {
            return await _database.Table<PendingTransaction>().CountAsync();
        }

        public async Task<PendingTransaction> GetFirst()
        {
            return await _database.Table<PendingTransaction>().FirstOrDefaultAsync();
        }

        public async Task<PendingTransaction> GetLast()
        {
            return await _database.Table<PendingTransaction>().OrderByDescending(p => p.ID).FirstOrDefaultAsync();
        }

        public async Task<PendingTransaction> Get(int id)
        {
            return await _database.Table<PendingTransaction>().FirstOrDefaultAsync(p => p.ID == id);
        }

        public async Task<int> Delete(int id)
        {
            return await _database.DeleteAsync<PendingTransaction>(id);
        }
    }
}
