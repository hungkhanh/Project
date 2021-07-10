using LeaderAnalytics.AdaptiveClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ToDoList.Model;

namespace ToDoList.Services.WebAPI
{
    public class BoardServices : BaseService, IBoardServices
    {
        public BoardServices(Func<IEndPointConfiguration> endPointFactory) : base(endPointFactory) { }

        public List<BoardIFM> GetBoards() => Task.Run(() => Get<List<BoardIFM>>("Boards/GetBoards")).Result;

        public RowOpResult<BoardIFM> SaveBoard(BoardIFM board) => Task.Run(() => Post<BoardIFM, RowOpResult<BoardIFM>>("Boards/SaveBoard", board)).Result;

        public RowOpResult DeleteBoard(int boardID) => Task.Run(() => Post<int, RowOpResult>("Boards/DeleteBoard", boardID)).Result;
    }
}
