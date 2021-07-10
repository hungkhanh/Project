using LeaderAnalytics.AdaptiveClient.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ToDoList.Services.Database.Components.SQLite
{
    public class Db_SQLite : Db, IMigrationContext
    {
        public Db_SQLite(DbContextOptions options) : base(options)
        {

        }
    }
}
