using System;
using SQLite;

namespace Converse.Database.Accessors
{
    public class AccessorBase
    {
        protected SQLiteAsyncConnection _database;
        public AccessorBase(SQLiteAsyncConnection database)
        {
            _database = database;
        }
    }
}
