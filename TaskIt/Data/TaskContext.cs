
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TaskIt.Mechanics.Models;
using static TaskIt.Mechanics.Utilities;

namespace TaskIt.Data
{
    public class TaskContext : DbContext 
    {
        public DbSet<UserTask> UserTasks { get; set; }
        public DbSet<Note> Notes { get; set; }

        public TaskContext() {
            SQLitePCL.Batteries_V2.Init();

            this.Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            // UserTask.Notification.SelectedDays
            modelBuilder.Entity<Recurring>()
                .Property(r => r.SelectedDays)
                .HasConversion(
                    v => string.Join(",", v.Select(e => e.ToString("D")).ToArray()),
                     v => v.Split(new[] { ',' })
                    .Select(e => Enum.Parse(typeof(DaysOfWeek), e))
                    .Cast<DaysOfWeek>()
                    .ToList()
              );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            // Set DbPath and db file name
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "taskContext.db3");
            // Tell EF to use sqlite and specified path ('Filename' is path from root)
            optionsBuilder.UseSqlite($"Filename={dbPath}");
        }

    }
}

