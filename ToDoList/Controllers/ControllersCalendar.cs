﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoList.Assist;
using ToDoList.Base;
using ToDoList.Models;
using Windows.UI.Xaml;

namespace ToDoList.Controllers
{
    public class ControllersCalendar : Observable
    {

        private DispatcherTimer timer;
        private DateTime _currentTime;
        public DateTime CurrentTime
        {
            get => _currentTime;
            set
            {
                if (_currentTime != value)
                {
                    _currentTime = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTimeOffset _selectedDate;
        public DateTimeOffset SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<PresentationTask> _ScheduledTasks;
        public ObservableCollection<PresentationTask> ScheudledTasks
        {
            get => _ScheduledTasks;
            set
            {
                if (_ScheduledTasks != value)
                {
                    _ScheduledTasks = value;
                    OnPropertyChanged();
                    OnPropertyChanged("IsResultsVisible");
                }
            }
        }

        private bool _IsResultsVisible;
        public bool IsResultsVisible
        {
            get
            {
                return _ScheduledTasks.Count == 0;
            }
            set
            {
                _IsResultsVisible = value;
            }
        }

        public ControllersCalendar()
        {
            SelectedDate = DateTimeOffset.Now;
            CurrentTime = DateTime.Now;
            StartTimer();
        }


        public ObservableCollection<PresentationTask> GetAvailableTasks(PresentationBoard currentBoard)
        {
            ScheudledTasks = new ObservableCollection<PresentationTask>();
            
            if (currentBoard.Tasks != null && currentBoard.Tasks.Any())   // hack
                foreach (PresentationTask task in currentBoard.Tasks)
                {
                    if (!string.IsNullOrEmpty(task.DueDate))
                    {
                        var dueDate = task.DueDate.ToNullableDateTimeOffset();
                        if (dueDate.Value.Year == SelectedDate.Year &&
                            dueDate.Value.Month == SelectedDate.Month &&
                            dueDate.Value.Day == SelectedDate.Day)
                        {
                            ScheudledTasks.Add(task);
                        }
                    }
                }
            return new ObservableCollection<PresentationTask>(ScheudledTasks.OrderBy(x => x.TimeDue));
        }

        private void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_tick;
            timer.Start();
        }

        public void StopTimer()
        {
            timer.Stop();
        }

        private void timer_tick(object sender, object e)
        {
            CurrentTime = DateTime.Now;
        }
    }
}
