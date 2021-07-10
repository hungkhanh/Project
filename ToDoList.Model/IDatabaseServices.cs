using System;
using System.Collections.Generic;
using System.Text;

namespace ToDoList.Model
{
    public interface IDatabaseServices
    {
        void BackupDB(string destPath);
    }
}
