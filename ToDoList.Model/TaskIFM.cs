using System;
using System.Collections.Generic;
using System.Text;

namespace ToDoList.Model
{
    public class TaskIFM
    {
        public int Id { get; set; }
        public int BoardId { get; set; }           
        public string DateCreated { get; set; }
        public string DueDate { get; set; }
        public string StartDate { get; set; }
        public string FinishDate { get; set; }
        public string TimeDue { get; set; }
        public string ReminderTime { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int ColumnIndex { get; set; }
        public string ColorKey { get; set; }
        public string Tags { get; set; }

        public virtual BoardIFM Board { get; set; }
    }
}
