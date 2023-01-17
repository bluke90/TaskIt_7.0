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
    public class TaskUpdateService
    {
        private readonly TaskContext _context;
        private readonly ILogger<TaskUpdateService> _logger;

        public TaskUpdateService(ILogger<TaskUpdateService> logger, TaskContext context) {
            _logger = logger;
            _context = context;
        }

        public async Task UpdateNextOccurances() {

            await Task.Delay(100);
            int taskUpdated = 0;
            
            // Get list of recurring task
            var list = await _context.ToDoTasks.Where(m => m.RecurringTask == true).ToListAsync();
            
            // update recurringTask NextOccurance
            foreach (var task in list) {
                if (task.NextOccurance < DateTime.Now) { 
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
