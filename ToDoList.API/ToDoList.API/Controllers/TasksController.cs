using LeaderAnalytics.AdaptiveClient;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ToDoList.Model;

namespace ToDoList.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private IAdaptiveClient<ITaskServices> serviceClient;

        public TasksController(IAdaptiveClient<ITaskServices> serviceClient) => this.serviceClient = serviceClient;

        [HttpGet]
        [Route("GetTasks")]
        public async Task<List<TaskIFM>> GetTasks() => serviceClient.Call(x => x.GetTasks());

        [HttpPost]
        [Route("SaveTask")]
        public async Task<RowOpResult<TaskIFM>> SaveTask(TaskIFM task) => serviceClient.Call(x => x.SaveTask(task));

        [HttpPost]
        [Route("DeleteTask")]
        public async Task<RowOpResult> DeleteTask(int taskID) => serviceClient.Call(x => x.DeleteTask(taskID));

        [HttpPost]
        [Route("UpdateColumnData")]
        public async Task UpdateColumnData(TaskIFM task) => serviceClient.Call(x => x.UpdateColumnData(task));

        [HttpPost]
        [Route("UpdateCardIndex")]
        public async Task UpdateCardIndex(JObject data) => serviceClient.Call(x => x.UpdateCardIndex(data["id"].ToObject<int>(), data["currentCardIndex"].ToObject<int>()));
    }
}
