using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskIt.Data;
using TaskIt.Mechanics.Models;
using TaskIt.Mechanics;

namespace TaskIt.Mechanics
{
    public static class ExtensionMethods
    {
        public static IOrderedEnumerable<KeyValuePair<TKey, TValue>> SortByValue<TKey, TValue>
        (this IDictionary<TKey, TValue> dictionary) where TValue : IComparable {
            return dictionary.OrderBy(kvp => kvp.Value);
        }

        /// <summary>
        /// Get List of ToDoTasks for a specified Date
        /// </summary>
        /// <param name="context">DataBase Context</param>
        /// <param name="date">Date to retreive list of ToDoTask for</param>
        /// <returns>List of ToDoTask for specified Date</returns>
        public static List<UserTask> GetTaskForDate(this TaskContext context, DateTime date) {

            // Get list of task for date
            var nonRecurringList = context.UserTasks
                .Include(m => m.NonRecurring) 
                .Where(m => m.StartDate.Date == date.Date && !m.Finished)
                .ToList();


            var recurringList = context.UserTasks
                .Include(m => m.Recurring)
                .Where(m => m.Recurring != null)
                .ToList();

            foreach (var task in recurringList.ToList()) {
                if (!task.Recurring.SelectedDays.Contains(date.DayOfWeek) && task.NextOccurance.Date != date.Date) {
                    recurringList.Remove(task);
                }
            }

            List<UserTask> list = new List<UserTask>();
            list.AddRange(recurringList);
            list.AddRange(nonRecurringList);
            // return list
            return list;
        }



    }
}






