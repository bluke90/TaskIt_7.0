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

        public async Task UpdateNextOccurances() {
            _logger.LogInformation($"[TaskUpdateService] Starting UpdateNextOccurance Task...");
            // Delay and init taskUpdated count
            await Task.Delay(100);
            int taskUpdated = 0;
            
            // Get list of recurring task
            var list = await _context.UserTasks.Where(m => m.IsRecurring == true).ToListAsync();
            
            // update recurringTask NextOccurance
            foreach (var task in list) {
                if (task.Recurring.NextOccurance < DateTime.Now) {     // If current date is past the stored NextOccurance THEN update
                    task.Recurring.NextOccurance = task.GetNextOccuranceOfTask();
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
