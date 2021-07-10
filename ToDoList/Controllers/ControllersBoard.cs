using LeaderAnalytics.AdaptiveClient;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Syncfusion.UI.Xaml.Kanban;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ToDoList.Assist;
using ToDoList.Base;
using ToDoList.Model;
using ToDoList.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ToDoList.Controllers
{
    public class ControllersBoard : Observable
    {
        private PresentationBoard _Board;
        public PresentationBoard Board
        {
            get => _Board;
            set
            {
                _Board = value;
                OnPropertyChanged();
            }
        }

        private PresentationTask _CurrentTask;
        public PresentationTask CurrentTask
        {
            get => _CurrentTask;
            set
            {
                _CurrentTask = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> SuggestedTagsCollection
        {
            get => _suggestedTagsCollection;
            set
            {
                _suggestedTagsCollection = value;
                OnPropertyChanged();
            }
        }

        private Brush _DueDateBackground;
        public Brush DueDateBackground
        {
            get => _DueDateBackground;
            set
            {
                if (_DueDateBackground != value)
                {
                    _DueDateBackground = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _paneTitle;
        private bool _isPointerEntered = false;
        private bool _isEditingTask;
        private string _currentCategory;
        private DispatcherTimer dateCheckTimer;
        private IAdaptiveClient<IServiceManifest> DataProvider;
        public ICommand NewTaskCommand { get; set; }
        public ICommand EditTaskCommand { get; set; }
        public ICommand SaveTaskCommand { get; set; }
        public ICommand DeleteTaskCommand { get; set; }
        public ICommand DeleteTagCommand { get; set; }
        public ICommand CancelEditCommand { get; set; }
        public ICommand RemoveScheduledNotificationCommand { get; set; }

        public ControllersBoard(PresentationBoard board, IAdaptiveClient<IServiceManifest> dataProvider, InAppNotification messagePump)
        {
            Board = board;
            DataProvider = dataProvider;
            MessagePump = messagePump;

            CurrentTask = new PresentationTask(new TaskIFM());
            NewTaskCommand = new RelayCommand<ColumnTag>(NewTaskCommandHandler, () => true);
            EditTaskCommand = new RelayCommand<int>(EditTaskCommandHandler, () => true);
            SaveTaskCommand = new RelayCommand(SaveTaskCommandHandler, () => true);
            DeleteTaskCommand = new RelayCommand<int>(DeleteTaskCommandHandler, () => true);
            DeleteTagCommand = new RelayCommand<string>(DeleteTagCommandHandler, () => true);
            CancelEditCommand = new RelayCommand(CancelEditCommandHandler, () => true);
            RemoveScheduledNotificationCommand = new RelayCommand(RemoveScheduledNotficationCommandHandler, () => true);

            DueDateBackground = (Application.Current.Resources["RegionBrush"] as AcrylicBrush);

            ColorKeys = new ObservableCollection<ComboBoxItem>();
            ColorKeys.Add(new ComboBoxItem { Content = "High" });
            ColorKeys.Add(new ComboBoxItem { Content = "Normal" });
            ColorKeys.Add(new ComboBoxItem { Content = "Low" });

            ReminderTimes = new ObservableCollection<ComboBoxItem>();
            ReminderTimes.Add(new ComboBoxItem { Content = "None" });
            ReminderTimes.Add(new ComboBoxItem { Content = "At Time of Due Date" });
            ReminderTimes.Add(new ComboBoxItem { Content = "5 Minutes Before" });
            ReminderTimes.Add(new ComboBoxItem { Content = "10 Minutes Before" });
            ReminderTimes.Add(new ComboBoxItem { Content = "15 Minutes Before" });
            ReminderTimes.Add(new ComboBoxItem { Content = "1 Hour Before" });


            if (Board.Tasks != null && board.Tasks.Any())
                foreach (PresentationTask task in Board.Tasks)
                {
                    task.ColorKeyComboBoxItem = GetComboBoxItemForColorKey(task.ColorKey);
                    task.ReminderTimeComboBoxItem = GetComboBoxItemForReminderTime(task.ReminderTime);
                }
        }

        #region Properties

        private ObservableCollection<ComboBoxItem> _ColorKeys;
        public ObservableCollection<ComboBoxItem> ColorKeys
        {
            get { return _ColorKeys; }
            set
            {
                _ColorKeys = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ComboBoxItem> _ReminderTimes;
        public ObservableCollection<ComboBoxItem> ReminderTimes
        {
            get { return _ReminderTimes; }
            set
            {
                _ReminderTimes = value;
                OnPropertyChanged();
            }
        }


        public string PaneTitle
        {
            get { return _paneTitle; }
            set
            {
                _paneTitle = value;
                OnPropertyChanged();
            }
        }


        public bool IsPointerEntered
        {
            get { return _isPointerEntered; }
            set
            {
                _isPointerEntered = value;
                OnPropertyChanged();
            }
        }

        public bool IsEditingTask
        {
            get { return _isEditingTask; }
            set { _isEditingTask = value; OnPropertyChanged(); }
        }

        public string CurrentCategory
        {
            get { return _currentCategory; }
            set
            {
                _currentCategory = value;
                OnPropertyChanged();
            }
        }

        public PresentationTask OriginalTask
        {
            get;
            set;
        }

        private InAppNotification MessagePump;
        private ObservableCollection<string> _suggestedTagsCollection;
        private const int MessageDuration = 3000;

        #endregion Properties

        #region CommandHandlers

        public void NewTaskCommandHandler(ColumnTag tag)
        {
            PaneTitle = "New Task";
            string category = tag?.Header?.ToString();
            CurrentTask = new PresentationTask(new TaskIFM() { Category = category }) { Board = Board, BoardId = Board.ID, ColorKeyComboBoxItem = ColorKeys[1], ReminderTimeComboBoxItem = ReminderTimes[0] };
            OriginalTask = null;
            IsEditingTask = true;
            InitializeSuggestedTags();
        }

        public void EditTaskCommandHandler(int taskID)
        {
            PaneTitle = "Edit Task";
            CurrentTask = Board.Tasks.First(x => x.ID == taskID);
            IsEditingTask = true;
            InitializeSuggestedTags();
            InitializeDateInformation();
            // clone a copy of CurrentTask so we can restore if user cancels
            OriginalTask = new PresentationTask(CurrentTask.To_TaskDTO());
        }

        public void SaveTaskCommandHandler()
        {
            IsEditingTask = false;

            if (dateCheckTimer != null)
                dateCheckTimer.Stop();

            if (CurrentTask == null)
                return;

            TaskIFM dto = CurrentTask.To_TaskDTO();
            dto.ColorKey = ((ComboBoxItem)CurrentTask.ColorKeyComboBoxItem)?.Content.ToString() ?? "Normal"; // hack
            dto.ReminderTime = ((ComboBoxItem)CurrentTask.ReminderTimeComboBoxItem)?.Content.ToString() ?? "None";

            bool isNew = dto.Id == 0;

            if (isNew)
            {
                dto.ColumnIndex = Board.Tasks?.Where(x => x.Category == dto.Category).Count() ?? 0;
                dto.DateCreated = DateTime.Now.ToString();
            }
            dto.Id = DataProvider.Call(x => x.TaskServices.SaveTask(dto)).Entity.Id;

            if (isNew)
            {
                CurrentTask.ID = dto.Id;
                CurrentTask.ColumnIndex = dto.ColumnIndex;
                Board.Tasks.Add(CurrentTask);
            }

            PrepareToastNotification();

            MessagePump.Show("Task was saved successfully", MessageDuration);
        }

        public void DeleteTaskCommandHandler(int taskID)
        {
            PresentationTask task = Board.Tasks.First(x => x.ID == taskID);
            RowOpResult result = DataProvider.Call(x => x.TaskServices.DeleteTask(taskID));

            if (result.Success)
            {
                Board.Tasks.Remove(task);
                CurrentTask = Board.Tasks.LastOrDefault();
                Toast.RemoveScheduledNotification(taskID.ToString());
                int startIndex = task.ColumnIndex;

                foreach (PresentationTask otherTask in Board.Tasks.Where(x => x.Category == task.Category && x.ColumnIndex > task.ColumnIndex).OrderBy(x => x.ColumnIndex))
                {
                    otherTask.ColumnIndex = startIndex++;
                    //otherTask.ColumnIndex -= 1;
                    UpdateCardIndex(otherTask.ID, otherTask.ColumnIndex);
                }
                MessagePump.Show("Task deleted from board successfully", MessageDuration);
            }
            else
                MessagePump.Show("Task failed to be deleted. Please try again or restart the application.", MessageDuration);
        }

        public void DeleteTagCommandHandler(string tag)
        {
            if (CurrentTask == null)
            {
                MessagePump.Show("Tag failed to be deleted.  CurrentTask is null. Please try again or restart the application.", MessageDuration);
                return;
            }
            CurrentTask.Tags.Remove(tag);
            MessagePump.Show("Tag deleted successfully", MessageDuration);
        }

        public void CancelEditCommandHandler()
        {
            IsEditingTask = false;

            if (dateCheckTimer != null)
                dateCheckTimer.Stop();

            if (OriginalTask == null)
                return;
            // roll back changes to CurrentTask
            else
            {
                int index = Board.Tasks.IndexOf(CurrentTask);
                Board.Tasks.Remove(CurrentTask);
                CurrentTask = new PresentationTask(OriginalTask.To_TaskDTO());
                Board.Tasks.Insert(index, CurrentTask);

                // Check if a toast notification was deleted
                if (OriginalTask.ReminderTime != "None")
                    PrepareToastNotification();

                // Reset combo box selected item since UWP Combobox doesn't bind correctly
                switch (OriginalTask.ColorKey)
                {
                    case "Low":
                        CurrentTask.ColorKeyComboBoxItem = ColorKeys[2];
                        break;
                    case "Normal":
                        CurrentTask.ColorKeyComboBoxItem = ColorKeys[1];
                        break;
                    case "High":
                        CurrentTask.ColorKeyComboBoxItem = ColorKeys[0];
                        break;
                }
                switch (OriginalTask.ReminderTime)
                {
                    case "None":
                        CurrentTask.ReminderTimeComboBoxItem = ReminderTimes[0];
                        break;
                    case "At Time of Due Date":
                        CurrentTask.ReminderTimeComboBoxItem = ReminderTimes[1];
                        break;
                    case "5 Minutes Before":
                        CurrentTask.ReminderTimeComboBoxItem = ReminderTimes[2];
                        break;
                    case "10 Minutes Before":
                        CurrentTask.ReminderTimeComboBoxItem = ReminderTimes[3];
                        break;
                    case "15 Minutes Before":
                        CurrentTask.ReminderTimeComboBoxItem = ReminderTimes[4];
                        break;
                    case "1 Hour Before":
                        CurrentTask.ReminderTimeComboBoxItem = ReminderTimes[5];
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion CommandHandlers

        #region Methods

        public bool AddTag(string tag)
        {
            bool result = false;

            if (CurrentTask == null)
            {
                MessagePump.Show("Tag failed to be added.  CurrentTask is null. Please try again or restart the application.", MessageDuration);
                return result;
            }

            if (CurrentTask.Tags.Contains(tag))
                MessagePump.Show("Tag already exists", 3000);
            else
            {
                CurrentTask.Tags.Add(tag);
                if (!Board.TagsCollection.Contains(tag))
                    Board.TagsCollection.Add(tag);
                SuggestedTagsCollection.Remove(tag);
                MessagePump.Show($"Tag {tag} added successfully", 3000);
                result = true;
            }
            return result;
        }

        private void InitializeSuggestedTags()
        {
            SuggestedTagsCollection = Board.TagsCollection;
            foreach (var tag in CurrentTask.Tags)
            {
                if (SuggestedTagsCollection.Contains(tag))
                {
                    SuggestedTagsCollection.Remove(tag);
                }
                else
                    SuggestedTagsCollection = Board.TagsCollection;
            }
        }

        private void InitializeDateInformation()
        {
            if (IsEditingTask)
            {
                StartDateCheckTimer();
                UpdateDateInformation();
            }
        }

        private void PrepareToastNotification()
        {            
            var dueDate = CurrentTask.DueDate.ToNullableDateTimeOffset();
            var timeDue = CurrentTask.TimeDue.ToNullableDateTimeOffset();
            var reminderTime = CurrentTask.ReminderTime;

            if (dueDate != null && timeDue != null && reminderTime != "None" && reminderTime != "")
            {
                DateTimeOffset taskDueDate = new DateTimeOffset(
                   dueDate.Value.Year, dueDate.Value.Month, dueDate.Value.Day,
                   timeDue.Value.Hour, timeDue.Value.Minute, timeDue.Value.Second,
                   timeDue.Value.Offset
                );

                var scheduledTime = taskDueDate;
                switch (reminderTime)
                {
                    case "At Time of Due Date":
                        break;
                    case "5 Minutes Before":
                        scheduledTime = taskDueDate.AddMinutes(-5);
                        break;
                    case "10 Minutes Before":
                        scheduledTime = taskDueDate.AddMinutes(-10);
                        break;
                    case "15 Minutes Before":
                        scheduledTime = taskDueDate.AddMinutes(-15);
                        break;
                    case "1 Hour Before":
                        scheduledTime = taskDueDate.AddHours(-1);
                        break;
                    default:
                        break;
                }
                Toast.ScheduleTaskDueNotification(CurrentTask.ID.ToString(), CurrentTask.Title,
                    CurrentTask.Description, scheduledTime, taskDueDate);
            }
            else if (reminderTime == "None")
                Toast.RemoveScheduledNotification(CurrentTask.ID.ToString());
        }

        private void RemoveScheduledNotficationCommandHandler()
        {
            Toast.RemoveScheduledNotification(CurrentTask.ID.ToString());
            CurrentTask.ReminderTimeComboBoxItem = ReminderTimes[0];
        }

        private void StartDateCheckTimer()
        {
            dateCheckTimer = new DispatcherTimer();
            dateCheckTimer.Interval = TimeSpan.FromMinutes(1);
            dateCheckTimer.Tick += Timer_Tick;
            dateCheckTimer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            UpdateDateInformation();
        }

        private void UpdateDateInformation()
        {
            DateTimeOffset? dateCreated = CurrentTask.DateCreated.ToNullableDateTimeOffset();
            DateTimeOffset? today = DateTimeOffset.Now;
            DateTimeOffset? startDate = CurrentTask.StartDate.ToNullableDateTimeOffset();
            DateTimeOffset? finishDate = CurrentTask.FinishDate.ToNullableDateTimeOffset();

            CheckIfPassedDueDate();

            if (startDate != null)
            {
                TimeSpan? ts = today - startDate;

                if (ts != null)
                    // Difference in days, hous, mins
                    CurrentTask.DaysWorkedOn = String.Format("{0} day(s)",
                        ts.Value.Days.ToString());
            }

            //  Update DaysSinceCreation
            if (dateCreated != null && today != null)
            {
                TimeSpan? ts = today - dateCreated;

                if (ts != null)
                    // Difference in days, hours, mins
                    CurrentTask.DaysSinceCreation = String.Format("{0}d, {1}hrs, {2}min",
                        ts.Value.Days.ToString(), ts.Value.Hours.ToString(), ts.Value.Minutes.ToString());
            }
        }

        public void SetDueDate(string dueDate)
        {
            if (CurrentTask == null)
                MessagePump.Show("Failed to set due date.  CurrentTask is null. Please try again or restart the application.", MessageDuration);

            CurrentTask.DueDate = dueDate;
            CheckIfPassedDueDate();
        }

        public void SetStartDate(string startDate)
        {
            if (CurrentTask == null)
                MessagePump.Show("Failed to set due date.  CurrentTask is null. Please try again or restart the application.", MessageDuration);

            CurrentTask.StartDate = startDate;

            // Update DaysWorkedOn binding
            DateTimeOffset? today = DateTimeOffset.Now;
            if (startDate.ToNullableDateTimeOffset() != null)
            {
                TimeSpan? ts = today - startDate.ToNullableDateTimeOffset();

                if (ts != null)
                    // Difference in days, hous, mins
                    CurrentTask.DaysWorkedOn = String.Format("{0} day(s)",
                        ts.Value.Days.ToString());
            }
        }

        public void SetFinishDate(string finishDate)
        {
            if (CurrentTask == null)
                MessagePump.Show("Failed to set due date.  CurrentTask is null. Please try again or restart the application.", MessageDuration);

            CurrentTask.FinishDate = finishDate;
        }

        public void SetTimeDue(string timeDue)
        {
            if (CurrentTask == null)
                MessagePump.Show("Failed to set time due.  CurrentTask is null. Please try again or restart the application.", MessageDuration);

            CurrentTask.TimeDue = timeDue;
            CheckIfPassedDueDate();
        }

        private ComboBoxItem GetComboBoxItemForColorKey(string colorKey) => ColorKeys.FirstOrDefault(x => x.Content.ToString() == colorKey);
        private ComboBoxItem GetComboBoxItemForReminderTime(string reminderTime) => ReminderTimes.FirstOrDefault(x => x.Content.ToString() == reminderTime);

        public void ShowInAppNotification(string message)
        {
            MessagePump.Show(message, MessageDuration);
        }

        public void CheckIfPassedDueDate()
        {
            var dueDate = CurrentTask.DueDate.ToNullableDateTimeOffset();
            if (!(dueDate == null))
            {
                var timeDue = CurrentTask.TimeDue.ToNullableDateTimeOffset();
                DateTimeOffset today = DateTimeOffset.Now;

                DateTimeOffset taskDueDate = new DateTimeOffset(
                  dueDate.Value.Year, dueDate.Value.Month, dueDate.Value.Day,
                  timeDue.Value.Hour, timeDue.Value.Minute, timeDue.Value.Second,
                  timeDue.Value.Offset
                );

                if (DateTimeOffset.Compare(taskDueDate, today) < 0)
                    DueDateBackground = new SolidColorBrush(Windows.UI.Colors.Red) { Opacity = 0.6 };
                else
                    DueDateBackground = (Application.Current.Resources["RegionBrush"] as AcrylicBrush);
            }
            else
                DueDateBackground = (Application.Current.Resources["RegionBrush"] as AcrylicBrush);
        }

        public void UpdateCardColumn(string targetCategory, PresentationTask selectedCardModel, int targetIndex)
        {
            TaskIFM task = selectedCardModel.To_TaskDTO();
            task.Category = targetCategory;
            task.ColumnIndex = targetIndex;
            DataProvider.Call(x => x.TaskServices.UpdateColumnData(task));
        }

        internal void UpdateCardIndex(int iD, int currentCardIndex)
        {
            DataProvider.Call(x => x.TaskServices.UpdateCardIndex(iD, currentCardIndex));
        }

        #endregion Methods

    }
}
