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
        // Generic Properties
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
        /// <summary>
        /// Get the date for the next time a recurring task is supposed to occure
        /// </summary>
        /// <param name="task">ToDoTask Instance</param>
        /// <returns>DateTime of next occurance</returns>
        public static DateTime GetNextOccuranceOfTask(this ToDoTask task)
        {
            // Init the next occurance var; add 25sec to account for UI refreash period
            DateTime nextOccurance = task.StartDate + TimeSpan.FromSeconds(25);

            // Continue to add the repeat interval to nextOccurance until it is greater than the current time
            while (nextOccurance < DateTime.Now) {
                nextOccurance += task.RecurringInterval;
            }
            return nextOccurance;
        }

        /// <summary>
        /// Order a list of ToDoTask in assending order, for recurring task use nextOccurance, for non-recurring task use DueDate
        /// </summary>
        /// <param name="listOfTask"> List to reorder </param>
        /// <returns> A list of ToDoTask ordered by NextOccurance/DueDate </returns>
        public static List<ToDoTask> OrderByNextOccurance(this List<ToDoTask> listOfTask)
        {
            // create dictionary for toDoTask and NextOccurance
            Dictionary<ToDoTask, DateTime> taskWithNextOccurance = new Dictionary<ToDoTask, DateTime>();
            
            // add task to dictionary
            foreach(var task in listOfTask) {
                taskWithNextOccurance.Add(task, (task.RecurringTask ? task.GetNextOccuranceOfTask(): task.DueDate));
            }

            // get a reordered Enumerable and copy the keys<ToDoTask> to a list
            var orderedEnum = taskWithNextOccurance.SortByValue();
            var orderedList = new List<ToDoTask>();
            foreach (var keyValue in orderedEnum) {
                orderedList.Add(keyValue.Key);
            }
            return orderedList;
        }

        /// <summary>
        /// Generate a random NotificationID and verify it doesnt match any currently pending notifications
        /// </summary>
        /// <param name="task">ToDoTask to set the notificationId for</param>
        /// <returns>Task Object</returns>
        public static async Task GenerateNotificationIdAsync(this ToDoTask task)
        {
            // Initialize the id variable and random number generator
            Random rand = new Random();
            int id;

            // Define the upper limit of the id range
            const int upperLimit = 10000; 

            do {
                // Generate a random integer between 1 and upperLimit
                id = rand.Next(1, upperLimit);
                // Check if the generated id already exists in pending notifications
            } while (await NotificationService.CheckIfNotificationIdExistAsync(id));

            // Set task.NotificationId
            task.NotificationId = id;
        }

        /// <summary>
        /// Asynchronously Schedule a LocalNotification for the refrenced ToDoTask
        /// </summary>
        /// <param name="task">ToDoTask to schedule notification for</param>
        /// <returns>The Notification ID</returns>
        public static async Task<int> ScheduleNotificationAsync(this ToDoTask task)
        {
            // Generate a unique notification id for the task
            await task.GenerateNotificationIdAsync();

            // Create the title and message for the notification
            string title = $"To Do Task: {task.Name}";
            string msg = $"Task Due on {task.DueDate.ToString("g")}";

            // Schedule the notification with the generated id, title, message, and task details
            var request = NotificationService.ScheduleNotification(task.NotificationId, title, msg, 42, task.NotificationStartDate, task.NotificationRepeatInterval);
            
            // -- Debug: Prints the notification information to the console 
            task.Debug_PrintNotificationInfoToConsole(request);
            return task.NotificationId;
        }

        /// <summary>
        /// Get the NotificationRequest Instance for refrenced task
        /// </summary>
        /// <param name="task">ToDoTask instance to get notification for</param>
        /// <returns>Notification Request for specified ToDoTask</returns>
        public static async Task<NotificationRequest> GetNotificationAsync(this ToDoTask task) 
        {
            var notification = await NotificationService.GetNotificationAsync(task.NotificationId);
            return notification;
        }

        /// <summary>
        /// Dictionary for Repeat Interval DropDown Selection list [Key=StringExpression; Value=TimeSpanExpression]
        /// </summary>
        public static Dictionary<string, TimeSpan> RepeatIntervalSelection = new Dictionary<string, TimeSpan>
        {
        {"None", TimeSpan.Zero },
        {"12 Hours", TimeSpan.FromHours(12) },
        {"Daily", TimeSpan.FromHours(24) },
        {"Twice a Week", TimeSpan.FromDays(4) },
        {"Weekly", TimeSpan.FromDays(7) }
        };
        /// <summary>
        /// Dictionary for selecting the start date for notifications to start [Key=StringExpression; Value=TimeSpanExpression]
        /// </summary>
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
