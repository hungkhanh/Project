using LeaderAnalytics.AdaptiveClient.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ToDoList.Services.Database.Components.SQLite
{
    public class DbContextOptions_SQLite : IDbContextOptions
    {
        public DbContextOptions Options { get; set; }

        public DbContextOptions_SQLite(string connectionString)
        {
            DbContextOptionsBuilder builder = new DbContextOptionsBuilder();
            builder.UseSqlite(connectionString);
            Options = builder.Options;
        }
    }
}
