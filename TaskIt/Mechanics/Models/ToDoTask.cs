using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskIt.Data;
//using Xamarin.Google.Crypto.Tink.DAead;
using static TaskIt.Mechanics.Utilities;

namespace TaskIt.Mechanics.Models
{


    public class UserTask {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public bool IsRecurring { get; set; }
        public bool Finished { get; set; }

        public Recurring Recurring { get; set; }
        public NonRecurring NonRecurring { get; set; }
        public Notification Notification { get; set; }

#nullable enable
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class Recurring
    {
        public int Id { get; set; }
        public TimeSpan RecurringInterval { get; set; }
        public DaysOfWeek SelectedDays { get; set; }
        public int RepeatOnSelectedDay { get; set; }
        public DateTime NextOccurance { get; set; }
    }

    public class NonRecurring
    {
        public int Id { get; set; }
        public bool Started { get; set; }
    }

    public class Notification {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public TimeSpan RepeatInterval { get; set; }
    }

    public static class UserTaskExtensions 
    {
        public static void BuildRecurring(this UserTask task,
                                                        TimeSpan Recurring_Interval = default(TimeSpan),
                                                        DaysOfWeek Days = DaysOfWeek.None,
                                                        int RepeatOnSelectedDay = 0)
        {
            // Build Recurring Task Instance
            task.Recurring = new Recurring()
            {
                RecurringInterval = Recurring_Interval,
                SelectedDays = Days,
                RepeatOnSelectedDay = RepeatOnSelectedDay
            };
            // Build Task Notification Instance
            task.Notification = new Notification { };
            // Set Appropriate Properties
            task.IsRecurring = true;
        }

        public static void BuildNonRecurring(this UserTask task, bool Started = false, bool Finished = false) 
        {
            // Build NonRecurring Task Instance
            task.NonRecurring = new NonRecurring
            {
                Started = Started
            };
            // Build Task Notification Instance
            task.Notification = new Notification { };
            // Set Appropriate Properties
            task.IsRecurring = false;
        }  
    
    }

    // ======================= Start Utils/Extensions ========================
    public static class ToDoTaskUtils
    {
        /// <summary>
        /// Get the date for the next time a recurring task is supposed to occure
        /// </summary>
        /// <param name="task">ToDoTask Instance</param>
        /// <returns>DateTime of next occurance</returns>
        public static DateTime GetNextOccuranceOfTask(this UserTask task)
        {

            // Init the next occurance var; add 25sec to account for UI refreash period
            DateTime nextOccurance = task.StartDate + TimeSpan.FromSeconds(25);

            // Continue to add the repeat interval to nextOccurance until it is greater than the current time
            while (nextOccurance < DateTime.Now) {
                nextOccurance += task.Recurring.RecurringInterval;
            }
            return nextOccurance;
        }

        /// <summary>
        /// Order a list of ToDoTask in assending order, for recurring task use nextOccurance, for non-recurring task use DueDate
        /// </summary>
        /// <param name="listOfTask"> List to reorder </param>
        /// <returns> A list of ToDoTask ordered by NextOccurance/DueDate </returns>
        public static List<UserTask> OrderByNextOccurance(this List<UserTask> listOfTask)
        {
            // create dictionary for toDoTask and NextOccurance
            Dictionary<UserTask, DateTime> taskWithNextOccurance = new Dictionary<UserTask, DateTime>();
            
            // add task to dictionary
            foreach(var task in listOfTask) {
                taskWithNextOccurance.Add(task, task.IsRecurring ? task.GetNextOccuranceOfTask(): task.EndDate);
            }

            // get a reordered Enumerable and copy the keys<ToDoTask> to a list
            var orderedEnum = taskWithNextOccurance.SortByValue();
            var orderedList = new List<UserTask>();
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
        public static async Task GenerateNotificationIdAsync(this UserTask task)
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
            task.Notification.Id = id;
        }

        /// <summary>
        /// Asynchronously Schedule a LocalNotification for the refrenced ToDoTask
        /// </summary>
        /// <param name="task">ToDoTask to schedule notification for</param>
        /// <returns>The Notification ID</returns>
        public static async Task<int> ScheduleNotificationAsync(this UserTask  task)
        {
            // Generate a unique notification id for the task
            await task.GenerateNotificationIdAsync();

            // Create the title and message for the notification
            string title = $"To Do Task: {task.Name}";
            string msg = $"Task Due on {task.EndDate.ToString("g")}";

            // Schedule the notification with the generated id, title, message, and task details
            var request = NotificationService.ScheduleNotification(task.Notification.Id, title, msg, 42, task.Notification.StartDate, task.Notification.RepeatInterval);
            
            // -- Debug: Prints the notification information to the console 
            task.Debug_PrintNotificationInfoToConsole(request);
            return task.Notification.Id;
        }

        /// <summary>
        /// Get the NotificationRequest Instance for refrenced task
        /// </summary>
        /// <param name="task">ToDoTask instance to get notification for</param>
        /// <returns>Notification Request for specified ToDoTask</returns>
        public static async Task<NotificationRequest> GetNotificationAsync(this UserTask task) 
        {
            var notification = await NotificationService.GetNotificationAsync(task.Notification.Id);
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
            { "15 Minutes Before", TimeSpan.FromMinutes(15)},
            { "30 Minutes Before", TimeSpan.FromMinutes(30)},
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
        public static async void PrintNotificationAsync(this UserTask task) {

            var notification = await NotificationService.GetNotificationAsync(task.Notification.Id);
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
