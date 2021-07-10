using LeaderAnalytics.AdaptiveClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ToDoList.Model;

namespace ToDoList.Services.WebAPI
{
    public class TaskServices : BaseService, ITaskServices
    {
        public TaskServices(Func<IEndPointConfiguration> endPointFactory) : base(endPointFactory) { }

        public List<TaskIFM> GetTasks() => Task.Run(() => Get<List<TaskIFM>>("Tasks/GetTasks")).Result;

        public RowOpResult<TaskIFM> SaveTask(TaskIFM task) => Task.Run(() => Post<TaskIFM, RowOpResult<TaskIFM>>("Tasks/SaveTask", task)).Result;

        public RowOpResult DeleteTask(int id) => Task.Run(() => Post<int, RowOpResult>("Tasks/DeleteTask", id)).Result;

        public void UpdateColumnData(TaskIFM task)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, "Tasks/UpdateColumnData"))
            {
                string json = JsonConvert.SerializeObject(task);
                using (StringContent stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;

                    using (var httpResponse = httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result)
                    {
                        httpResponse.EnsureSuccessStatusCode();
                    }
                }
            }
        }

        public void UpdateCardIndex(int iD, int currentCardIndex)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, "Tasks/UpdateCardIndex"))
            {
                string json = JsonConvert.SerializeObject(new { id = iD, currentCardIndex = currentCardIndex });
                using (StringContent stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;

                    using (var httpResponse = httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result)
                    {
                        httpResponse.EnsureSuccessStatusCode();
                    }
                }
            }
        }
    }
}
