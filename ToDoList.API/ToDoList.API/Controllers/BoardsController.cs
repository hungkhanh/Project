using LeaderAnalytics.AdaptiveClient;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ToDoList.Model;

namespace ToDoList.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoardsController : ControllerBase
    {
        private IAdaptiveClient<IBoardServices> serviceClient;

        public BoardsController(IAdaptiveClient<IBoardServices> serviceClient) => this.serviceClient = serviceClient;

        [HttpGet]
        [Route("GetBoards")]
        public async Task<List<BoardIFM>> GetBoards() => serviceClient.Call(x => x.GetBoards());

        [HttpPost]
        [Route("SaveBoard")]
        public async Task<RowOpResult<BoardIFM>> SaveBoard(BoardIFM board) => serviceClient.Call(x => x.SaveBoard(board));

        [HttpPost]
        [Route("DeleteBoard")]
        public async Task<RowOpResult> DeleteBoard(int boardID) => serviceClient.Call(x => x.DeleteBoard(boardID));
    }
}
