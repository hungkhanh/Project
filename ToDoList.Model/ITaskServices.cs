using System;
using System.Collections.Generic;
using System.Text;

namespace ToDoList.Model
{
    public interface ITaskServices
    {
        RowOpResult<TaskIFM> SaveTask(TaskIFM task);
        RowOpResult DeleteTask(int id);
        List<TaskIFM> GetTasks();
        void UpdateCardIndex(int iD, int currentCardIndex);
        void UpdateColumnData(TaskIFM task);
        RowOpResult<TaskIFM> ValidateTask(RowOpResult<TaskIFM> result);
    }
}
