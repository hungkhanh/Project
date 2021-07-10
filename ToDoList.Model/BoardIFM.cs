using System;
using System.Collections.Generic;
using System.Text;

namespace ToDoList.Model
{
    public class BoardIFM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }

        public virtual ICollection<TaskIFM> Tasks { get; set; }
    }
}
