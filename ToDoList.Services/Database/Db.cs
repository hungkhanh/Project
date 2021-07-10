using LeaderAnalytics.AdaptiveClient.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using ToDoList.Model;

namespace ToDoList.Services.Database
{
    public class Db : DbContext
    {
        public DbSet<BoardIFM> Boards { get; set; }
        public DbSet<TaskIFM> Tasks { get; set; }

        public Db(Func<IDbContextOptions> dbContextOptionsFactory) : base(dbContextOptionsFactory().Options) { }

        public Db(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);
            mb.Entity<BoardIFM>().ToTable("tblBoards");
            mb.Entity<TaskIFM>().ToTable("tblTasks");
        }
    }
}
