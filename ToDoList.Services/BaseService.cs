using Microsoft.Data.Sqlite;
using System;
using LeaderAnalytics.AdaptiveClient;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Text;
using ToDoList.Model;
using ToDoList.Services.Database;

namespace ToDoList.Services
{
    public abstract class BaseService
    {
        protected Db db { get; set; }
        protected IServiceManifest serviceManifest { get; set; }
        protected HttpClient httpClient { get; private set; }

        public BaseService(Db db, IServiceManifest serviceManifest)
        {
            this.db = db;
            this.serviceManifest = serviceManifest;
        }

        public BaseService(Func<IEndPointConfiguration> endPointFactory)
        {
            var endPoint = endPointFactory();

            if (endPoint.EndPointType == EndPointType.HTTP)
                httpClient = new HttpClient { BaseAddress = new Uri(endPoint.ConnectionString) };
        }

        public virtual RowOpResult<BoardIFM> ValidateBoard(RowOpResult<BoardIFM> result)
        {
            BoardIFM board = result.Entity;

            if (string.IsNullOrEmpty(board.Name))
                result.ErrorMessage = "Name is required.";
            else if (board.Name.Length > 100)
                result.ErrorMessage = "Name is too long.";
            if (string.IsNullOrEmpty(board.Notes))
                result.ErrorMessage = "Notes are required.";
            else if (board.Notes.Length > 1000)
                result.ErrorMessage = "Notes are too long.";

            result.Success = string.IsNullOrEmpty(result.ErrorMessage);
            return result;
        }

        public virtual RowOpResult<TaskIFM> ValidateTask(RowOpResult<TaskIFM> result)
        {
            TaskIFM task = result.Entity;

            // title, description, dueDate. finishDate, reminderTime, startDate can null

            if (string.IsNullOrEmpty(task.Title))
                task.Title = "";
            else if (task.Title.Length > 100)
                result.ErrorMessage = "Title is too long.";
            if (string.IsNullOrEmpty(task.Description))
                task.Description = "";
            if (string.IsNullOrEmpty(task.DueDate))
                task.DueDate = "";
            if (string.IsNullOrEmpty(task.FinishDate))
                task.FinishDate = "";
            if (string.IsNullOrEmpty(task.ReminderTime))
                task.ReminderTime = "";
            if (string.IsNullOrEmpty(task.StartDate))
                task.StartDate = "";

            result.Success = string.IsNullOrEmpty(result.ErrorMessage);
            return result;
        }

        public virtual async Task<T> Get<T>(string url)
        {
            string json = await httpClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public virtual async Task<TResponse> Post<TRequest, TResponse>(string url, TRequest requestObject)
        {
            TResponse response = default(TResponse);

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                string json = JsonConvert.SerializeObject(requestObject);
                using (StringContent stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;

                    using (var httpResponse = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        httpResponse.EnsureSuccessStatusCode();
                        response = JsonConvert.DeserializeObject<TResponse>(await httpResponse.Content.ReadAsStringAsync());
                    }
                }
            }
            return response;
        }
    }
}
