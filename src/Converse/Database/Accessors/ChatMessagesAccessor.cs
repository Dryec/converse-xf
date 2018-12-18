using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Converse.Database.Models;
using Converse.Models;
using Protocol;
using SQLite;

namespace Converse.Database.Accessors
{
    public class ChatMessagesAccessor : AccessorBase
    {
        public ChatMessagesAccessor(SQLiteAsyncConnection database) : base(database)
        {
            _database.CreateTableAsync<Database.Models.ChatMessage>().Wait();
        }

        public async Task<int> Insert(Converse.Models.ChatMessage message)
        {
            var p = Models.ChatMessage.FromChatMessage(message);
            if (p == null)
            {
                return -1;
            }
            var insertedRows = await _database.InsertAsync(p);
            return insertedRows == 1 ? p.ID : -1;
        }

        public async Task<int> Insert(Database.Models.ChatMessage message)
        {
            if (message == null)
            {
                return -1;
            }

            var insertedRows = await _database.InsertAsync(message);
            return insertedRows == 1 ? message.ID : -1;
        }

        public async Task<int> Update(Converse.Models.ChatMessage message)
        {
            var chatMessage = Database.Models.ChatMessage.FromChatMessage(message);
            if (chatMessage == null)
            {
                return -1;
            }
            var dbEntry = await _database.FindAsync<Database.Models.ChatMessage>(c => c.ChatID == chatMessage.ChatID && c.MessageID == chatMessage.MessageID);
            if (dbEntry == null)
            {
                try
                {
                    return await _database.InsertAsync(chatMessage);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }

            chatMessage.ID = dbEntry.ID;
            return await _database.UpdateAsync(chatMessage);
        }

        public async Task<int> Update(Converse.Models.ChatMessages messages)
        {
            var rowsCount = 0;
            if (messages != null)
            {
                foreach (var message in messages.Messages)
                {
                    rowsCount += await Update(message);
                }
            }
            return rowsCount;
        }


        public async Task<int> Update(Models.ChatMessage message)
        {
            if (message == null)
            {
                return -1;
            }
            return await _database.UpdateAsync(message);
        }

        public async Task<int> GetCount()
        {
            return await _database.Table<Models.ChatMessage>().CountAsync();
        }

        public async Task<Models.ChatMessage> GetFirst()
        {
            return await _database.Table<Models.ChatMessage>().FirstOrDefaultAsync();
        }

        public async Task<Models.ChatMessage> GetLast()
        {
            return await _database.Table<Models.ChatMessage>().OrderByDescending(p => p.ID).FirstOrDefaultAsync();
        }

        public async Task<List<Models.ChatMessage>> GetAll()
        {
            return await _database.Table<Models.ChatMessage>().ToListAsync();
        }

        public async Task<Models.ChatMessage> Get(int id)
        {
            return await _database.Table<Models.ChatMessage>().FirstOrDefaultAsync(p => p.ID == id);
        }

        public async Task<List<Models.ChatMessage>> GetAllFromChatID(int id)
        {
            return await _database.Table<Models.ChatMessage>().Where(p => p.ChatID == id).ToListAsync();
        }

        public async Task<List<Models.ChatMessage>> GetLatestFromChatID(int chatID, int amount)
        {
            var messages = await _database.Table<Models.ChatMessage>()
                                .Where(p => p.ChatID == chatID)
                                .OrderByDescending(p => p.MessageID)
                                .Take(amount)
                                .ToListAsync();
            messages.Reverse();
            return messages;
        }

        public async Task<List<Models.ChatMessage>> GetFromChatID(int id, int start, int end)
        {
            return await _database.Table<Models.ChatMessage>().Where(p => p.ChatID == id && (p.MessageID >= start && p.MessageID <= end)).ToListAsync();
        }

        public async Task<int> Delete(int id)
        {
            return await _database.DeleteAsync<Models.ChatMessage>(id);
        }

        public async Task<int> DeleteAll()
        {
            return await _database.DeleteAllAsync<Models.ChatMessage>();
        }
    }
}
