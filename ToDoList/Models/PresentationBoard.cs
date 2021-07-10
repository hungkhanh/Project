using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoList.Base;
using ToDoList.Model;

namespace ToDoList.Models
{
    public class PresentationBoard : Observable
    {
        private int _Id;
        private string _Name;
        private string _Notes;

        public int ID { get => _Id; set { _Id = value; OnPropertyChanged(); } }
        public string Name
        {
            get => _Name;
            set
            {
                if(_Name != value)
                {
                    _Name = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Notes
        {
            get => _Notes;
            set
            {
                if (_Notes != value)
                {
                    _Notes = value;
                    OnPropertyChanged();
                }
            }
        }


        private ObservableCollection<PresentationTask> _Tasks;
        public ObservableCollection<PresentationTask> Tasks
        {
            get => _Tasks;
            set
            {
                if (_Tasks != value)
                {
                    _Tasks = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<string> _TagsCollection;
        public ObservableCollection<string> TagsCollection
        {
            get => _TagsCollection;
            set
            {
                if (_TagsCollection != value)
                {
                    _TagsCollection = value;
                    OnPropertyChanged();
                }
            }
        }

        public PresentationBoard(BoardIFM dto)
        {
            ID = dto.Id;
            Name = dto.Name;
            Notes = dto.Notes;
            Tasks = new ObservableCollection<PresentationTask>();
            TagsCollection = new ObservableCollection<string>();
        }

        public BoardIFM To_BoardIFM()
        {
            return new BoardIFM
            {
                Id = ID,
                Name = Name,
                Notes = Notes
            };
        }
    }
}
