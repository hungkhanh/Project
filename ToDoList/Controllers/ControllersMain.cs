using LeaderAnalytics.AdaptiveClient;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ToDoList.Base;
using ToDoList.Model;
using ToDoList.Models;
using Windows.UI.Xaml.Controls;

namespace ToDoList.Controllers
{
    public class ControllersMain : Observable
    {
        public Func<PresentationBoard, InAppNotification, ControllersBoard> controllersMainFactory;
        private IAdaptiveClient<IServiceManifest> dataProvider;
        public ICommand NewBoardCommand { get; set; }
        public ICommand EditBoardCommand { get; set; }
        public ICommand SaveBoardCommand { get; set; }
        public ICommand CancelSaveBoardCommand { get; set; }
        public ICommand DeleteBoardCommand { get; set; }

        #region Properties

        private ObservableCollection<ControllersBoard> _boardList;
        public ObservableCollection<ControllersBoard> BoardList
        {
            get
            {
                return _boardList;
            }
            set
            {
                _boardList = value;
                OnPropertyChanged();
            }
        }


        private ControllersBoard _CurrentBoard;
        public ControllersBoard CurrentBoard
        {
            get
            {
                return _CurrentBoard;
            }
            set
            {
                _CurrentBoard = value;
                OnPropertyChanged();
            }

        }
        
        private string _BoardEditorTitle;
        public string BoardEditorTitle
        {
            get => _BoardEditorTitle;
            set
            {
                _BoardEditorTitle = value;
                OnPropertyChanged();
            }
        }

        internal void SetCurrentBoardOnClose()
        {
            if (BoardList.Count == 0)
                CurrentBoard = null; 
            else
            {
                CurrentBoard = null;
                CurrentBoard = TmpBoard;
            }
        }

        private Frame navigationFrame { get; set; }
        private InAppNotification messagePump;
        private const int MessageDuration = 3000;

        private ControllersBoard TmpBoard;
        #endregion Properties

        public ControllersMain(Func<PresentationBoard, InAppNotification, ControllersBoard> controllersMainFactory, IAdaptiveClient<IServiceManifest> dataProvider, Frame navigationFrame, InAppNotification messagePump)
        {
            this.navigationFrame = navigationFrame;
            this.messagePump = messagePump;
            PropertyChanged += ControllersMain_PropertyChanged;
            NewBoardCommand = new RelayCommand(NewBoardCommandHandler, () => true);
            EditBoardCommand = new RelayCommand(EditBoardCommandHandler, () => CurrentBoard != null);
            SaveBoardCommand = new RelayCommand(SaveBoardCommandHandler, () => true);
            CancelSaveBoardCommand = new RelayCommand(CancelSaveBoardCommandHandler, () => true);
            DeleteBoardCommand = new RelayCommand(DeleteBoardCommandHandler, () => CurrentBoard != null);
            this.dataProvider = dataProvider;
            this.controllersMainFactory = controllersMainFactory;
            BoardList = new ObservableCollection<ControllersBoard>();
            List<BoardIFM> boardDTOs = dataProvider.Call(x => x.BoardServices.GetBoards());

            foreach (BoardIFM dto in boardDTOs)
            {
                PresentationBoard presBoard = new PresentationBoard(dto);

                if (dto.Tasks?.Any() ?? false)
                    foreach (TaskIFM taskDTO in dto.Tasks.OrderBy(x => x.ColumnIndex))
                    {
                        presBoard.Tasks.Add(new PresentationTask(taskDTO));

                        // Fill TagsCollection on Board for AutoSuggestBox
                        foreach (var tag in taskDTO.Tags.Split(','))
                            if (!string.IsNullOrEmpty(tag) && !presBoard.TagsCollection.Contains(tag))
                                presBoard.TagsCollection.Add(tag);
                    }

                BoardList.Add(controllersMainFactory(presBoard, messagePump));
            }

            if (BoardList.Any())
                CurrentBoard = BoardList.First();
            else
                CurrentBoard = null;
        }
        
        private void ControllersMain_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentBoard))
            {
                if (CurrentBoard == null)
                    navigationFrame.Navigate(typeof(Views.NoBoardsMessageView));
                else
                    navigationFrame.Navigate(typeof(Views.BoardView), CurrentBoard);
            }
        }


        public void NewBoardCommandHandler()
        {
            BoardEditorTitle = "New Board Editor";
            ControllersBoard newBoard = controllersMainFactory(new PresentationBoard(new BoardIFM()), messagePump);
            TmpBoard = CurrentBoard;        
            if (TmpBoard != null)
            {
                OldBoardName = TmpBoard.Board.Name;
                OldBoardNotes = TmpBoard.Board.Notes;
            }
            CurrentBoard = null;              
            CurrentBoard = newBoard;
           
        }

        public void EditBoardCommandHandler()
        {
            TmpBoard = CurrentBoard;
            OldBoardName = TmpBoard.Board.Name;
            OldBoardNotes = TmpBoard.Board.Notes;
            BoardEditorTitle = "Edit Board";
        }

        public void SaveBoardCommandHandler()
        {
            if (CurrentBoard.Board == null)
                return;
            if (string.IsNullOrEmpty(CurrentBoard.Board.Name))
                return;
            if (string.IsNullOrEmpty(CurrentBoard.Board.Notes))
                return;


            BoardIFM dto = CurrentBoard.Board.To_BoardIFM();
            bool isNew = dto.Id == 0;
            RowOpResult<BoardIFM> result = null;
            // Add board to db and collection
            result = dataProvider.Call(x => x.BoardServices.SaveBoard(dto));
            messagePump.Show(result.Success ? "Board was saved successfully." : result.ErrorMessage, MessageDuration);
            if (isNew && result.Success)
            {
                CurrentBoard.Board.ID = result.Entity.Id;
                BoardList.Add(CurrentBoard);
            }

        }
        
        private string _OldBoardName;
        public string OldBoardName
        {
            get => _OldBoardName;
            set
            {
                _OldBoardName = value;
                OnPropertyChanged();
            }
        }

        private string _OldBoardNotes;
        public string OldBoardNotes
        {
            get => _OldBoardNotes;
            set
            {
                _OldBoardNotes = value;
                OnPropertyChanged();
            }
        }

        public void CancelSaveBoardCommandHandler()
        {
            CurrentBoard.Board.Name = "";
            CurrentBoard.Board.Notes = "";
            CurrentBoard = null;
            CurrentBoard = TmpBoard;

            if (CurrentBoard != null)
            {
                CurrentBoard.Board.Name = OldBoardName;
                CurrentBoard.Board.Notes = OldBoardNotes;
            }
        }

        public void DeleteBoardCommandHandler()
        {
            if (CurrentBoard == null)
                return;

            dataProvider.Call(x => x.BoardServices.DeleteBoard(CurrentBoard.Board.ID));
            BoardList.Remove(CurrentBoard);
            CurrentBoard.Board.Name = ""; 
            CurrentBoard.Board.Notes = ""; 

            CurrentBoard = null;
            CurrentBoard = BoardList.LastOrDefault();
        }
    }
}
