using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Converse.Database.Models;
using Converse.Models;
using Protocol;
using SQLite;

namespace Converse.Database.Accessors
{
    public class GroupsAccessor : AccessorBase
    {
        public GroupsAccessor(SQLiteAsyncConnection database) : base(database)
        {
            _database.CreateTableAsync<Group>().Wait();
        }

        public async Task<int> Insert(GroupInfo groupInfo)
        {
            var p = Group.FromGroupInfo(groupInfo);
            if(p == null)
            {
                return -1;
            }
            var insertedRows = await _database.InsertAsync(p);
            return insertedRows == 1 ? p.ID : -1;
        }

        public async Task<int> Insert(Group group)
        {
            if (group == null)
            {
                return -1;
            }
            var insertedRows = await _database.InsertAsync(group);
            return insertedRows == 1 ? group.ID : -1;
        }

        public async Task<int> Update(GroupInfo groupInfo)
        {
            var group = Group.FromGroupInfo(groupInfo);
            if (group == null)
            {
                return -1;
            }
            var dbEntry = await _database.FindAsync<Group>(c => c.Address == group.Address);
            if(dbEntry == null)
            {
                return await _database.InsertAsync(group);
            }

            group.ID = dbEntry.ID;
            return await _database.UpdateAsync(group);
        }

        public async Task<int> Update(Group group)
        {
            if (group == null)
            {
                return -1;
            }
            return await _database.UpdateAsync(group);
        }

        public async Task<int> GetCount()
        {
            return await _database.Table<Group>().CountAsync();
        }

        public async Task<Group> GetFirst()
        {
            return await _database.Table<Group>().FirstOrDefaultAsync();
        }

        public async Task<Group> GetLast()
        {
            return await _database.Table<Group>().OrderByDescending(p => p.ID).FirstOrDefaultAsync();
        }

        public async Task<List<Group>> GetAll()
        {
            return await _database.Table<Group>().ToListAsync();
        }

        public async Task<Group> Get(int id)
        {
            return await _database.Table<Group>().FirstOrDefaultAsync(p => p.ID == id);
        }

        public async Task<Group> GetByGroupID(int groupId)
        {
            return await _database.Table<Group>().FirstOrDefaultAsync(p => p.GroupID == groupId);
        }

        public async Task<int> Delete(int id)
        {
            return await _database.DeleteAsync<Group>(id);
        }

        public async Task<int> DeleteByGroupID(int groupId)
        {
            var dbEntry = await _database.FindAsync<Group>(c => c.GroupID == groupId);
            if (dbEntry != null)
            {
                return await _database.DeleteAsync<Group>(dbEntry.ID);
            }
            return -1;
        }
    }
}
