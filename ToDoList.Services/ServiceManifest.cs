using LeaderAnalytics.AdaptiveClient.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using ToDoList.Model;

namespace ToDoList.Services
{
    class ServiceManifest : ServiceManifestFactory, IServiceManifest
    {
        public ITaskServices TaskServices => Create<ITaskServices>();

        public IBoardServices BoardServices => Create<IBoardServices>();

        public IDatabaseServices DatabaseServices => Create<IDatabaseServices>();

    }
}
