
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskIt.Mechanics.Models;

namespace TaskIt.Data
{
    public class TaskContext : DbContext 
    {
        public DbSet<ToDoTask> ToDoTasks { get; set; }
        public DbSet<Note> Notes { get; set; }

        public TaskContext() {
            SQLitePCL.Batteries_V2.Init();

            this.Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            // Set DbPath and db file name
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "taskContext.db3");
            // Tell EF to use sqlite and specified path ('Filename' is path from root)
            optionsBuilder.UseSqlite($"Filename={dbPath}");
        }

    }
}

