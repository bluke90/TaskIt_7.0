using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskIt.Data;
using TaskIt.Mechanics.Models;

namespace TaskIt.Mechanics
{
    /// <summary>
    /// Service for performing any TaskObj Updates needed - See PeriodicUpdateService.cs
    /// </summary>
    public class TaskUpdateService
    {
        private readonly TaskContext _context;
        private readonly ILogger<TaskUpdateService> _logger;

        public TaskUpdateService(ILogger<TaskUpdateService> logger, TaskContext context) {
            _logger = logger;
            _context = context;
        }

        public async Task UpdateNotifications() {
            _logger.LogInformation("[TaskUpdate Service] Starting UpdateNotifications Task...");
            // Delay
            await Task.Delay(100);

            // Get Recurring Task
            var list = await _context.UserTasks.Include(m=>m.Notification).Where(m => m.IsRecurring == true).ToListAsync();

            // Update recurring notifications for selected days
            foreach (var task in list) {
                if (DateTime.Now > task.Notification.LastScheduleUpdate + TimeSpan.FromDays(7)) {
                    var week2 = task.Notification.LastScheduleUpdate + TimeSpan.FromDays(14);
                    var weekdifference = DateTime.Now - week2;
                    await task.ScheduleNotificationAsync(weekdifference);
                }
            }

        }

        public async Task UpdateNextOccurances() {
            _logger.LogInformation($"[TaskUpdateService] Starting UpdateNextOccurance Task...");
            // Delay and init taskUpdated count
            await Task.Delay(100);
            int taskUpdated = 0;
            
            // Get list of recurring task
            var list = await _context.UserTasks.Include(m => m.Recurring).Where(m => m.IsRecurring == true).ToListAsync();
            
            // update recurringTask NextOccurance
            foreach (var task in list) {
                if (task.NextOccurance < DateTime.Now) {     // If current date is past the stored NextOccurance THEN update
                    task.NextOccurance = task.GetNextOccuranceOfTask();
                    taskUpdated++;
                }
            }
            
            // Save Changes to DB
            await _context.SaveChangesAsync();

            _logger.LogInformation($"[TaskUpdateService] Completed UpdateNextOccurance. {taskUpdated} Tasks were updated");
            return;
        }

    }
}
