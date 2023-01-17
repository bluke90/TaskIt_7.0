using Plugin.LocalNotification;
using TaskIt.Mechanics.Models;

namespace TaskIt.Mechanics
{
    public static class NotificationService
    {

        public static NotificationRequest ScheduleNotification(int notificationId, string title, string description, int badgeNumber, DateTime notifyTime, TimeSpan? repeatInterval = null, string subtitle = null) {

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

            if (repeatInterval != null) {
                request.Schedule.NotifyRepeatInterval = repeatInterval;
            }

            LocalNotificationCenter.Current.Show(request);
            return request;
        }

        public static async Task<NotificationRequest> GetNotificationAsync(int notificationId) {
            var notificationList = await LocalNotificationCenter.Current.GetPendingNotificationList();
            foreach (var notification in notificationList) {
                if (notification.NotificationId == notificationId) { return notification; }
            }
            return null;
        }

        public static async Task<bool> CheckIfNotificationIdExistAsync(int notificationId) {
            var notificationList = await LocalNotificationCenter.Current.GetPendingNotificationList();
            foreach (var notification in notificationList) {
                if (notification.NotificationId == notificationId) {
                    return true;
                }
            }
            return false;
        }
 
        public static void CancelNotification(int notificationId) {
            LocalNotificationCenter.Current.Cancel(notificationId);
        }

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
