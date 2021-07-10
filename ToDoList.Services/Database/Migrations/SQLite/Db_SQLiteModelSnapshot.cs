﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace ToDoList.Services.Database.Migrations.SQLite
{
    class Db_SQLiteModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel( ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity("ToDoList.Model.BoardIFM", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<string>("Name");

                b.Property<string>("Notes");

                b.HasKey("Id");

                b.ToTable("tblBoards");
            });

            modelBuilder.Entity("ToDoList.Model.TaskIFM", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<int>("BoardId");

                b.Property<string>("Category");

                b.Property<string>("ColorKey");

                b.Property<int>("ColumnIndex");

                b.Property<string>("DateCreated");

                b.Property<string>("Description");

                b.Property<string>("DueDate");

                b.Property<string>("FinishDate");

                b.Property<string>("ReminderTime");

                b.Property<string>("StartDate");

                b.Property<string>("Tags");

                b.Property<string>("TimeDue");

                b.Property<string>("Title");

                b.HasKey("Id");

                b.HasIndex("BoardId");

                b.ToTable("tblTasks");
            });

            modelBuilder.Entity("ToDoList.Model.TaskIFM", b =>
            {
                b.HasOne("ToDoList.Model.BoardIFM", "Board")
                    .WithMany("Tasks")
                    .HasForeignKey("BoardId")
                    .OnDelete(DeleteBehavior.Cascade);
            });
#pragma warning restore 612, 618
        }
    }
}
