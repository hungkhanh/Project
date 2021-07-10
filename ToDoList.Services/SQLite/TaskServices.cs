﻿using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using ToDoList.Model;
using ToDoList.Services.Database;

namespace ToDoList.Services.SQLite
{
    public class TaskServices : BaseService, ITaskServices
    {
        public TaskServices(Db db, IServiceManifest serviceManifest) : base(db, serviceManifest)
        {

        }

        public List<TaskIFM> GetTasks()
        {
            List<TaskIFM> tasks = new List<TaskIFM>();

            using (SqliteConnection db =
                new SqliteConnection(this.db.Database.GetDbConnection().ConnectionString))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT Id, BoardID, DateCreated, Title, Description, Category, ColumnIndex, ColorKey, Tags, DueDate, FinishDate, TimeDue, ReminderTime, StartDate from tblTasks order by ColumnIndex", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                // Query the db and get the tasks
                // Must match the exact table schemas
                while (query.Read())
                {
                   
                    TaskIFM row = new TaskIFM()
                    {
                        Id = Convert.ToInt32(query.GetString(0)),
                        BoardId = Convert.ToInt32(query.GetString(1)),
                        DateCreated = query.GetString(2),
                        Title = query.GetString(3),
                        Description = query.GetString(4),
                        Category = query.GetString(5),
                        ColumnIndex = Convert.ToInt32(query.GetValue(6) == DBNull.Value ? "0" : query.GetString(6)),
                        ColorKey = query.GetString(7),
                        Tags = query.GetString(8),
                        DueDate = (query.GetValue(9) == DBNull.Value ? "" : query.GetString(9)),
                        FinishDate = (query.GetValue(10) == DBNull.Value ? "" : query.GetString(10)),
                        TimeDue = (query.GetValue(11) == DBNull.Value ? "" : query.GetString(11)),
                        ReminderTime = (query.GetValue(12) == DBNull.Value ? "" : query.GetString(12)),
                        StartDate = (query.GetValue(13) == DBNull.Value ? "" : query.GetString(13))
                    };

                    tasks.Add(row);
                }
                db.Close();
            }
            return tasks;
        }

        public RowOpResult<TaskIFM> SaveTask(TaskIFM task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            RowOpResult<TaskIFM> result = new RowOpResult<TaskIFM>(task);

            ValidateTask(result);

            if (!result.Success)
                return result;

            using (SqliteConnection db = new SqliteConnection(this.db.Database.GetDbConnection().ConnectionString))
            {
                db.Open();

                try
                {
                    SqliteCommand command = new SqliteCommand { Connection = db };
                    command.Parameters.AddWithValue("@title", task.Title);
                    command.Parameters.AddWithValue("@desc", task.Description);
                    command.Parameters.AddWithValue("@categ", task.Category);
                    command.Parameters.AddWithValue("@colorKey", task.ColorKey);
                    command.Parameters.AddWithValue("@tags", task.Tags);
                    command.Parameters.AddWithValue("@columnIndex", task.ColumnIndex);
                    command.Parameters.AddWithValue("@dueDate", task.DueDate);
                    command.Parameters.AddWithValue("@finishDate", task.FinishDate);
                    command.Parameters.AddWithValue("@timeDue", task.TimeDue);
                    command.Parameters.AddWithValue("@reminderTime", task.ReminderTime);
                    command.Parameters.AddWithValue("@startDate", task.StartDate);

                    if (task.Id == 0)
                    {
                        // Insert a new row
                        command.Parameters.AddWithValue("@boardID", task.BoardId);
                        command.Parameters.AddWithValue("@dateCreated", task.DateCreated);
                        command.CommandText = "INSERT INTO tblTasks (BoardID,DateCreated,Title,Description,Category,ColorKey,Tags, ColumnIndex, DueDate, FinishDate, TimeDue, ReminderTime, StartDate) VALUES (" +
                            "@boardID, @dateCreated, @title, @desc, @categ, @colorKey, @tags, @columnIndex, @dueDate, @finishDate, @timeDue, @reminderTime, @startDate); ; SELECT last_insert_rowid();";
                        task.Id = Convert.ToInt32(command.ExecuteScalar());
                    }
                    else
                    {
                        // Update an existing row
                        command.Parameters.AddWithValue("@id", task.Id);
                        command.CommandText = "UPDATE tblTasks SET Title=@title, Description=@desc, Category=@categ, ColorKey=@colorKey, Tags=@tags, ColumnIndex=@columnIndex, DueDate=@dueDate, FinishDate=@finishDate, TimeDue=@timeDue, ReminderTime=@reminderTime, StartDate=@startDate WHERE Id=@id";
                        command.ExecuteNonQuery();
                    }

                    result.Success = true;
                }
                catch (Exception)
                {
                    result.Success = false;
                    throw;
                }
                finally
                {
                    db.Close();
                }
            }
            return result;
        }

        public RowOpResult DeleteTask(int id)
        {
            RowOpResult result = new RowOpResult();

            using (SqliteConnection db = new SqliteConnection(this.db.Database.GetDbConnection().ConnectionString))
            {
                db.Open();

                try
                {
                    SqliteCommand deleteCommand = new SqliteCommand
                    ("DELETE FROM tblTasks WHERE Id=@id", db);
                    deleteCommand.Parameters.AddWithValue("id", id);
                    deleteCommand.ExecuteNonQuery();
                    result.Success = true;
                }
                finally
                {
                    db.Close();
                }

                return result;
            }
        }

        public void UpdateColumnData(TaskIFM task)
        {
            using (SqliteConnection db =
                new SqliteConnection(this.db.Database.GetDbConnection().ConnectionString))
            {
                db.Open();

                // Update task column/category when dragged to new column/category
                SqliteCommand updateCommand = new SqliteCommand
                    ("UPDATE tblTasks SET Category=@category, ColumnIndex=@columnIndex WHERE Id=@id", db);
                updateCommand.Parameters.AddWithValue("@category", task.Category);
                updateCommand.Parameters.AddWithValue("@columnIndex", task.ColumnIndex);
                updateCommand.Parameters.AddWithValue("@id", task.Id);
                updateCommand.ExecuteNonQuery();

                db.Close();
            }
        }

        public void UpdateCardIndex(int iD, int currentCardIndex)
        {
            using (SqliteConnection db =
                new SqliteConnection(this.db.Database.GetDbConnection().ConnectionString))
            {
                db.Open();

                // Update card  index
                SqliteCommand updateCommand = new SqliteCommand
                    ("UPDATE tblTasks SET ColumnIndex=@columnIndex WHERE Id=@id", db);

                updateCommand.Parameters.AddWithValue("@id", iD);
                updateCommand.Parameters.AddWithValue("@columnIndex", currentCardIndex);
                updateCommand.ExecuteNonQuery();

                db.Close();
            }
        }
    }
}
