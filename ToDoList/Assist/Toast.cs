using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace ToDoList.Assist
{
    public static class Toast
    {
       
        public static void ScheduleTaskDueNotification(string taskId, string taskTitle, string taskDescription, DateTimeOffset scheduledAlarmTime, DateTimeOffset taskDueDate)
        {
            if (scheduledAlarmTime > DateTime.Now.AddSeconds(5))
            {

                var toastContent = ConstructToastContent(taskTitle, taskDescription, taskDueDate);

                var scheduledNotif = new ScheduledToastNotification(toastContent.GetXml(), scheduledAlarmTime);
                scheduledNotif.Tag = taskId;
                ToastNotificationManager.CreateToastNotifier().AddToSchedule(scheduledNotif);
            }
        }

        public static void RemoveScheduledNotification(string tag)
        {
            var scheduledNotifs = ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();
            foreach (var notif in scheduledNotifs)
            {

                if (notif.Tag == tag)
                    ToastNotificationManager.CreateToastNotifier().RemoveFromSchedule(notif);
            }
        }

        
        private static ToastContent ConstructToastContent(string taskTitle, string taskDescription, DateTimeOffset alarmTime)
        {
            return new ToastContent()
            {
               
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                            {
                                new AdaptiveText()
                                { // Toast Header Title
                                    Text = "You have a task due"
                                },
                                new AdaptiveText()
                                { // Title of Task & Description
                                    Text = ((taskTitle != null) ? taskTitle : "") + "\n" +
                                           ((taskDescription != null) ? taskDescription : "")
                                },
                                new AdaptiveText()
                                { // Task Due Date
                                    Text = "Due " + alarmTime.ToString("t") + ", " + alarmTime.Date.ToShortDateString()
                                },

                            },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = "ms-appx:///Assets/Square44x44Logo.scale-200.png",
                        },
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Inputs =
                        {
                            new ToastSelectionBox("snoozeTime")
                            {
                                DefaultSelectionBoxItemId = "15",
                                Items =
                                {
                                    new ToastSelectionBoxItem("1", "1 minute"),
                                    new ToastSelectionBoxItem("15", "15 minutes"),
                                    new ToastSelectionBoxItem("60", "1 hour"),
                                    new ToastSelectionBoxItem("240", "4 hours"),
                                    new ToastSelectionBoxItem("1440", "1 day")
                                }
                            }
                        },
                    Buttons =
                        {
                            new ToastButtonSnooze()
                            {
                                SelectionBoxId = "snoozeTime"
                            },
                            new ToastButtonDismiss()
                        }
                },
                Launch = "action=viewEvent&eventId=1983",
                Scenario = ToastScenario.Reminder
            };
        }
    }
}
