using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Converse.Database.Models;
using Converse.Models;
using Protocol;
using SQLite;

namespace Converse.Database.Accessors
{
    public class UserAccessor : AccessorBase
    {
        public UserAccessor(SQLiteAsyncConnection database) : base(database)
        {
            _database.CreateTableAsync<User>().Wait();
        }

        public async Task<int> Insert(UserInfo userInfo)
        {
            var p = User.FromUserInfo(userInfo);
            if(p == null)
            {
                return -1;
            }
            return await _database.InsertAsync(p);
        }

        public async Task<int> Insert(User user)
        {
            if (user == null)
            {
                return -1;
            }
            return await _database.InsertAsync(user);
        }

        public async Task<int> Update(UserInfo userInfo)
        {
            var user = User.FromUserInfo(userInfo);
            if (user == null)
            {
                return -1;
            }
            var dbEntry = await _database.FindAsync<User>(c => c.UserID == user.UserID);
            if(dbEntry == null)
            {
                return await _database.InsertAsync(user);
            }

            return await _database.UpdateAsync(dbEntry);
        }

        public async Task<int> Update(User user)
        {
            if (user == null)
            {
                return -1;
            }
            return await _database.UpdateAsync(user);
        }

        public async Task<int> GetCount()
        {
            return await _database.Table<User>().CountAsync();
        }

        public async Task<User> GetFirst()
        {
            return await _database.Table<User>().FirstOrDefaultAsync();
        }

        public async Task<User> GetLast()
        {
            return await _database.Table<User>().OrderByDescending(p => p.ID).FirstOrDefaultAsync();
        }

        public async Task<List<User>> GetAll()
        {
            return await _database.Table<User>().ToListAsync();
        }

        public async Task<User> Get(int id)
        {
            return await _database.Table<User>().FirstOrDefaultAsync(p => p.ID == id);
        }

        public async Task<User> GetByUserID(int userId)
        {
            return await _database.Table<User>().FirstOrDefaultAsync(p => p.UserID == userId);
        }

        public async Task<int> Delete(int id)
        {
            return await _database.DeleteAsync<User>(id);
        }

        public async Task<int> DeleteByUserID(int userId)
        {
            var dbEntry = await _database.FindAsync<User>(c => c.UserID == userId);
            if (dbEntry != null)
            {
                return await _database.DeleteAsync<User>(dbEntry.ID);
            }
            return -1;
        }
    }
}
