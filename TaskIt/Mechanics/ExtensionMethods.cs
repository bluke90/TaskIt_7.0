using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskIt.Data;
using TaskIt.Mechanics.Models;

namespace TaskIt.Mechanics
{
    public static class ExtensionMethods
    {
        public static IOrderedEnumerable<KeyValuePair<TKey, TValue>> SortByValue<TKey, TValue>
        (this IDictionary<TKey, TValue> dictionary) where TValue : IComparable {
            return dictionary.OrderBy(kvp => kvp.Value);
        }

        public static List<ToDoTask> GetTaskForDate(this TaskContext context, DateTime date) {
            
            // Get list of task for date
            var list = context.ToDoTasks
                .Where(m => m.DueDate.Date == date.Date || m.NextOccurance.Date == date.Date)
                .ToList();
            // return list
            return list;
        }



    }
}






