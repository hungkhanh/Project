using System;
using System.Collections.Generic;
using System.Text;

namespace ToDoList.Model
{
    public interface IServiceManifest
    {
        ITaskServices TaskServices { get; }
        IBoardServices BoardServices { get; }
        IDatabaseServices DatabaseServices { get; }
    }
}
