//using Android.Icu.Math;
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
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "taskContext.db3");

            optionsBuilder.UseSqlite($"Filename={dbPath}");
        }

    }
}

