using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ToDoList.Model;
using System.Linq;
using ToDoList.Services.Database;
using Microsoft.EntityFrameworkCore;

namespace ToDoList.Services.SQLite
{
    public class BoardServices : BaseService, IBoardServices
    {
        public BoardServices(Db db, IServiceManifest serviceManifest) : base(db, serviceManifest)
        {
        }

        public void BackupDB(string destPath)
        {
            using (SqliteConnection db =
               new SqliteConnection(this.db.Database.GetDbConnection().ConnectionString))
            {
                db.Open();

                // Backup Db. Save db localhost to destPath
                //db.BackupDatabase()

                db.Close();
            }
        }

        public List<BoardIFM> GetBoards()
        {
            List<BoardIFM> boards = new List<BoardIFM>();
            List<TaskIFM> tasks = serviceManifest.TaskServices.GetTasks();

            using (SqliteConnection db =
                new SqliteConnection(this.db.Database.GetDbConnection().ConnectionString))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT Id,Name,Notes from tblBoards", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    BoardIFM row = new BoardIFM()
                    {
                        Id = Convert.ToInt32(query.GetString(0)),
                        Name = query.GetString(1),
                        Notes = query.GetString(2),
                    };
                    boards.Add(row);
                }
                db.Close();
            }

            foreach (BoardIFM board in boards)
                board.Tasks = tasks.Where(x => x.BoardId == board.Id).ToList();

            return boards;
        }

        public RowOpResult<BoardIFM> SaveBoard(BoardIFM board)
        {
            RowOpResult<BoardIFM> result = new RowOpResult<BoardIFM>(board);

            ValidateBoard(result);

            if (!result.Success)
                return result;

            using (SqliteConnection db = new SqliteConnection(this.db.Database.GetDbConnection().ConnectionString))
            {
                try
                {
                    db.Open();
                    SqliteCommand command = new SqliteCommand { Connection = db };
                    command.Parameters.AddWithValue("@boardName", board.Name);
                    command.Parameters.AddWithValue("@boardNotes", board.Notes);

                    if (board.Id == 0)
                    {
                        command.CommandText = "INSERT INTO tblBoards (Name,Notes) VALUES (@boardName, @boardNotes); ; SELECT last_insert_rowid();";
                        board.Id = Convert.ToInt32(command.ExecuteScalar());
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@boardId", board.Id);
                        command.CommandText = "UPDATE tblBoards SET Name=@boardName,Notes=@boardNotes WHERE Id=@boardId";
                        command.ExecuteNonQuery();

                    }
                    result.Success = true;
                }
                finally
                {
                    db.Close();
                }
            }
            return result;
        }

        public RowOpResult DeleteBoard(int boardId)
        {
            RowOpResult result = new RowOpResult();

            // Delete task from db
            using (SqliteConnection db =
                new SqliteConnection(this.db.Database.GetDbConnection().ConnectionString))
            {
                db.Open();

                try
                {
                    SqliteCommand deleteCommand = new SqliteCommand
                   ("DELETE FROM tblTasks WHERE BoardID=@boardId", db);
                    deleteCommand.Parameters.AddWithValue("boardId", boardId);
                    deleteCommand.ExecuteNonQuery();

                    deleteCommand = new SqliteCommand
                   ("DELETE FROM tblBoards WHERE Id=@id", db);
                    deleteCommand.Parameters.AddWithValue("id", boardId);
                    deleteCommand.ExecuteNonQuery();

                    result.Success = true;
                    return result;
                }

                finally
                {
                    db.Close();
                }
            }
        }
    }
}
