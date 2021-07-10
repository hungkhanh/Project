using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using ToDoList.Model;
using ToDoList.Services.Database;

namespace ToDoList.Services.SQLite
{
    public class DatabaseServices : BaseService, IDatabaseServices
    {
        public DatabaseServices(Db db, IServiceManifest serviceManifest) : base(db, serviceManifest)
        {

        }

        public void BackupDB(string destPath)
        {
            using (SqliteConnection db =
               new SqliteConnection(this.db.Database.GetDbConnection().ConnectionString))
            {
                db.Open();

                // db.BackupDatabase();

                db.Close();
            }
        }
    }
}
