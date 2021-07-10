using Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace ToDoList.Services
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
        }
    }
}
