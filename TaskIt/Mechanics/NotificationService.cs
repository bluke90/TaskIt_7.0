using Plugin.LocalNotification;
using TaskIt.Mechanics.Models;

namespace TaskIt.Mechanics
{
    /// <summary>
    /// Service for scheduling Notifications
    /// </summary>
    public static class NotificationService
    {
        /// <summary>
        /// Schedule local notification for ToDoTask
        /// </summary>
        /// <param name="notificationId">Randomly generated notificationId</param>
        /// <param name="title">Notification Title</param>
        /// <param name="description">Notification Description</param>
        /// <param name="badgeNumber">Notification Badge Number</param>
        /// <param name="notifyTime">Time for the first notification to be delivered</param>
        /// <param name="repeatInterval">How often the notification repeats after the notify time</param>
        /// <param name="subtitle">Notification Subtitle</param>
        /// <returns>Scheduled NotificationRequest </returns>
        public static NotificationRequest ScheduleNotification(int notificationId, string title, string description, int badgeNumber, DateTime notifyTime, TimeSpan? repeatInterval = null, string subtitle = null) {
            // Create request
            var request = new NotificationRequest()
            {
                NotificationId = notificationId,
                Title = title,
                Subtitle = subtitle,
                Description = description,
                BadgeNumber = badgeNumber,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = notifyTime
                }
            };
            // Set Repeat interval if not null
            if (repeatInterval != null) {
                request.Schedule.NotifyRepeatInterval = repeatInterval;
            }
            // Schedule Notification
            LocalNotificationCenter.Current.Show(request);
            return request;
        }

        /// <summary>
        /// Retreive currently queued notification based on notification ID
        /// </summary>
        /// <param name="notificationId">Notification ID assigned at creation </param>
        /// <returns>Requested Notification. If non found, return NULL </returns>
        public static async Task<NotificationRequest> GetNotificationAsync(int notificationId) {
            // Get list of pending notifications
            var notificationList = await LocalNotificationCenter.Current.GetPendingNotificationList();
            // Iterate through notifications and if matching notification found, return NotificationRequest
            foreach (var notification in notificationList) {
                if (notification.NotificationId == notificationId) { return notification; }
            }
            return null;
        }

        /// <summary>
        /// Check if notification exist in pending notifications
        /// </summary>
        /// <param name="notificationId">ID for requested notification</param>
        /// <returns>Notification Exist => TRUE; Notification Not Found => FALSE; </returns>
        public static async Task<bool> CheckIfNotificationIdExistAsync(int notificationId) {
            // Get list of pending notifications
            var notificationList = await LocalNotificationCenter.Current.GetPendingNotificationList();
            // Iterate through notifications and if matching notification found, return TRUE
            foreach (var notification in notificationList) {
                if (notification.NotificationId == notificationId) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Cancel a pending Notification
        /// </summary>
        /// <param name="notificationId">Id of notification for deletion request </param>
        public static void CancelNotification(int notificationId) {
            LocalNotificationCenter.Current.Cancel(notificationId);
        }

        /// <summary>
        /// Debug only - Method for writing Task Data and Task Notification Data to console
        /// </summary>
        /// <param name="task">ToDoTask object</param>
        /// <param name="request">Relevent NotificationRequest object</param>
        public static void Debug_PrintNotificationInfoToConsole(this ToDoTask task, NotificationRequest request) {
            // print info from task obj
            Console.WriteLine($"Task[{task.Name}:{task.Id}] Notification Info: ");
            Console.WriteLine($"    From Task Obj:");
            Console.WriteLine($"    ---- StartDate => {task.StartDate.ToString()} | NotificationStartDate => {task.NotificationStartDate.ToString()}");
            Console.WriteLine($"    ---- DueDate => {task.DueDate.ToString()}");
            Console.WriteLine($"    ---- NotificationRepeatInterval => {task.NotificationRepeatInterval.ToString()}");
            Console.WriteLine($"    ---- RecurringTask => {task.RecurringTask.ToString()}");
            Console.WriteLine($"    ---- RecurringInterval => {task.RecurringInterval.ToString()}");
            // print info from request obj
            Console.WriteLine($"    From Request Obj:");
            Console.WriteLine($"    ---- NotificationId => {request.NotificationId}");
            Console.WriteLine($"    ---- Schedule.NotifyTime => {request.Schedule.NotifyTime.ToString()} | Schedule.NotifyRepeatInterval => {request.Schedule.NotifyRepeatInterval.ToString()}");
            Console.WriteLine("---------------------------------------------- End Task Notification Debug Info ----------------------------------------------");
        }

    }
}
