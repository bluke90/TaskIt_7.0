using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskIt.Data;

namespace TaskIt.Mechanics.Models
{
    public class ToDoTask
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Notes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool Started { get; set; }
        public bool Finished { get; set; }
        // Notification properties
        public int NotificationId { get; set; }
        public TimeSpan NotificationRepeatInterval { get; set; }
        public DateTime NotificationStartDate { get; set; }
        // Recurring task properties
        public bool RecurringTask { get; set; }
        public TimeSpan RecurringInterval { get; set; }
        public DateTime NextOccurance { get; set; }
    }

    public static class ToDoTaskUtils
    {
        public static DateTime GetNextOccuranceOfTask(this ToDoTask task) {
            DateTime nextOccurance = task.StartDate + TimeSpan.FromSeconds(25);

            while (nextOccurance < DateTime.Now) {
                nextOccurance += task.RecurringInterval;
            }

            return nextOccurance;
        }

        public static List<ToDoTask> OrderByNextOccurance(this List<ToDoTask> listOfTask) {
            Dictionary<ToDoTask, DateTime> taskWithNextOccurance = new Dictionary<ToDoTask, DateTime>();

            foreach(var task in listOfTask) {
                taskWithNextOccurance.Add(task, (task.RecurringTask ? task.GetNextOccuranceOfTask(): task.DueDate));
            }

            var orderedEnum = taskWithNextOccurance.SortByValue();
            var orderedList = new List<ToDoTask>();
            foreach (var keyValue in orderedEnum) {
                orderedList.Add(keyValue.Key);
            }

            return orderedList;

        }


        public static async Task GenerateNotificationIdAsync(this ToDoTask task) {
            Random rand = new Random();
            bool idIsUnique = false;
            var id = 0;
            while (idIsUnique == false) {
                id = rand.Next(1000, 9999);
                if (!await NotificationService.CheckIfNotificationIdExistAsync(id)) { 
                    idIsUnique = true;
                    break;                
                }
            }
            task.NotificationId = id;
        }
        public static async Task<int> ScheduleNotificationAsync(this ToDoTask task)
        {
            await task.GenerateNotificationIdAsync();

            string title = $"To Do Task: {task.Name}";
            string msg = $"Task Due on {task.DueDate.ToString("g")}";
 
            var request = NotificationService.ScheduleNotification(task.NotificationId, title, msg, 42, task.NotificationStartDate, task.NotificationRepeatInterval);
            // -- Debug
            task.Debug_PrintNotificationInfoToConsole(request);
            return task.NotificationId;
        }

        public static async Task<NotificationRequest> GetNotificationAsync(this ToDoTask task) {
            var notification = await NotificationService.GetNotificationAsync(task.NotificationId);
            return notification;
        }

        public static Dictionary<string, TimeSpan> RepeatIntervalSelection = new Dictionary<string, TimeSpan>
        {
        {"None", TimeSpan.Zero },
        {"12 Hours", TimeSpan.FromHours(12) },
        {"Daily", TimeSpan.FromHours(24) },
        {"Twice a Week", TimeSpan.FromDays(4) },
        {"Weekly", TimeSpan.FromDays(7) }
        };

        public static Dictionary<string, TimeSpan> NotificationStartDateSelection = new Dictionary<string, TimeSpan>
        {
        {"On Notification Start Date", TimeSpan.Zero },
        {" 1 Hour before", TimeSpan.FromHours(1)},
        {" 2 Hours Before", TimeSpan.FromHours(2)},
        {"Day Before", TimeSpan.FromHours(24) },
        {"2 Days Before", TimeSpan.FromDays(2) },
        {"Week Before", TimeSpan.FromDays(7) },
        {"2 Weeks Before", TimeSpan.FromDays(14) },
        {"Month Before", TimeSpan.FromDays(30) }
        };

    }

    public static class DebugTaskUtils {
        public static async void PrintNotificationAsync(this ToDoTask task) {

            var notification = await NotificationService.GetNotificationAsync(task.NotificationId);
            try { 
                Console.WriteLine($"Notification Debug Details for NotificationID: {notification.NotificationId}"); 
            } catch (NullReferenceException ex) {
                Console.Write(ex.ToString());
                Console.WriteLine(" | Mechanics/Models/ToDoTask.cs - PrintNotificationAsync()");
            }
            Console.WriteLine($"Title: {notification.Title}");
            Console.WriteLine($"Description: {notification.Description}");
            Console.WriteLine($"Notify time: {notification.Schedule.NotifyTime.ToString()}");

        }
    }

}
