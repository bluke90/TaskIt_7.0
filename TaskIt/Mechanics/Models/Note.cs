using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskIt.Mechanics.Models
{
    public class Note
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
#nullable enable
        public ToDoTask? ToDoTask { get; set; }

    }
}
