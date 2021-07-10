using LeaderAnalytics.AdaptiveClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ToDoList.Services
{
    public static class ConnectionStringUtility
    {
        private static IConfigurationRoot config;

        private static void BuildConfig(string filePath)
        {
            ConfigurationBuilder configBuilder = new ConfigurationBuilder();

            string configFile = Path.Combine(filePath, "appsettings.Development.json");

            if (!File.Exists(configFile))
                throw new Exception($"File not found {configFile}.");

            configBuilder.AddJsonFile(configFile);
            config = configBuilder.Build();
        }

        public static string GetConnectionString(string filePath, string apiName, string providerName)
        {
            IEnumerable<IEndPointConfiguration> endPoints = EndPointUtilities.LoadEndPoints(filePath, false);
            return endPoints.First(x => x.API_Name == apiName && x.ProviderName == providerName).ConnectionString;
        }


        public static void PopulateConnectionStrings(string filePath, IEnumerable<IEndPointConfiguration> endPoints)
        {
            if (!(endPoints?.Any() ?? false))
                return;

            BuildConfig(filePath);
        }
    }
}
