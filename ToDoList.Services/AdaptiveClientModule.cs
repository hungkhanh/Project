using System;
using System.Collections.Generic;
using System.Text;
using LeaderAnalytics.AdaptiveClient;
using LeaderAnalytics.AdaptiveClient.Utilities;
using LeaderAnalytics.AdaptiveClient.EntityFrameworkCore;
using ToDoList.Model;

namespace ToDoList.Services
{
    public class AdaptiveClientModule : IAdaptiveClientModule
    {
        public void Register(RegistrationHelper registrationHelper)
        {
            registrationHelper

                // Service SQLite
            .RegisterService<SQLite.BoardServices, IBoardServices>(EndPointType.DBMS, API_Name.ToDo, DatabaseProvider.SQLite)
            .RegisterService<SQLite.TaskServices, ITaskServices>(EndPointType.DBMS, API_Name.ToDo, DatabaseProvider.SQLite)

                // Service WebAPI
            .RegisterService<WebAPI.BoardServices, IBoardServices>(EndPointType.HTTP, API_Name.ToDo, DatabaseProvider.WebAPI)
            .RegisterService<WebAPI.TaskServices, ITaskServices>(EndPointType.HTTP, API_Name.ToDo, DatabaseProvider.WebAPI)

                // DbContexts
            .RegisterDbContext<Database.Db>(API_Name.ToDo)

                // Migration Contexts
            .RegisterMigrationContext<Database.Components.SQLite.Db_SQLite>(API_Name.ToDo, DatabaseProvider.SQLite)

                // Database Initializers
            .RegisterDatabaseInitializer<Database.Components.SQLite.DatabaseInitializer>(API_Name.ToDo, DatabaseProvider.SQLite)
                
                // Service Manifests
            .RegisterServiceManifest<ServiceManifest, IServiceManifest>(EndPointType.DBMS, API_Name.ToDo, DatabaseProvider.SQLite)
            .RegisterServiceManifest<ServiceManifest, IServiceManifest>(EndPointType.HTTP, API_Name.ToDo, DatabaseProvider.WebAPI)

                // EndPoint Validator
            .RegisterEndPointValidator<Database.Components.SQLite.EndPointValidator>(EndPointType.DBMS, DatabaseProvider.SQLite)
            .RegisterEndPointValidator<Http_EndPointValidator>(EndPointType.HTTP, DatabaseProvider.WebAPI)

                // DbContextOptions
            .RegisterDbContextOptions<Database.Components.SQLite.DbContextOptions_SQLite>(DatabaseProvider.SQLite);
        }
    }
}
