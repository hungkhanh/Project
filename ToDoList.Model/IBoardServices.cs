using System;
using System.Collections.Generic;
using System.Text;

namespace ToDoList.Model
{
    public interface IBoardServices
    {
        RowOpResult<BoardIFM> SaveBoard(BoardIFM board);
        RowOpResult DeleteBoard(int boardId);
        List<BoardIFM> GetBoards();
        RowOpResult<BoardIFM> ValidateBoard(RowOpResult<BoardIFM> result);
    }
}
