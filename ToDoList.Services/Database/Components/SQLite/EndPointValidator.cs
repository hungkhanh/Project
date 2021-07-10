using LeaderAnalytics.AdaptiveClient;
using LeaderAnalytics.AdaptiveClient.Utilities;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using ToDoList.Model;

namespace ToDoList.Services.Database.Components.SQLite
{
    public class EndPointValidator : IEndPointValidator
    {
        public virtual bool IsInterfaceAlive(IEndPointConfiguration endPoint)
        {
            bool res = true;

            using (SqliteConnection db = new SqliteConnection(endPoint.ConnectionString))
            {
                try
                {
                    db.Open();
                }
                catch (Exception)
                {
                    res = false;
                }
            }
            return res;
        }
    }
}
