using Android.App;
using Plugin.LocalNotification.Json;
using TaskIt.Data;
using TaskIt.Mechanics;
using TaskIt.Mechanics.Models;

namespace TaskIt;

public partial class EditTaskPage : ContentPage
{
	private readonly TaskContext _context;

	private ToDoTask _task { get; set; }

	public EditTaskPage(TaskContext context, int taskId) {
        _context = context;
        _task = _context.ToDoTasks.FirstOrDefault(m => m.Id == taskId);
		InitializeComponent();
		PopulateData();

		RepeatInterval_entry.ItemsSource = ToDoTaskUtils.RepeatIntervalSelection.Keys.ToList();
		StartNotification_entry.ItemsSource = ToDoTaskUtils.NotificationStartDateSelection.Keys.ToList();
		foreach (var item in ToDoTaskUtils.RepeatIntervalSelection) {
			if (item.Value == _task.NotificationRepeatInterval) {
				RepeatInterval_entry.SelectedItem = item.Key;
			}
		}
        TimeSpan timespan = _task.DueDate - _task.NotificationStartDate;
        foreach (var item in ToDoTaskUtils.NotificationStartDateSelection) {
			if (item.Value == timespan) {
				StartNotification_entry.SelectedItem = item.Key;
			}
		}
	}

	private void PopulateData() {
		TaskName_entry.Text = _task.Name;
		TaskNotes_entry.Text = _task.Notes;
		TaskDueDate_entry.Date = _task.DueDate;
		TaskDueTime_entry.Time = _task.DueDate.TimeOfDay;
	}

	private async void doneClicked(object sender, EventArgs e) {
		var priorName = _task.Name;
		var priorNotes = _task.Notes;
		var priorDueDate = _task.DueDate;
		var priorRepeatInterval = _task.NotificationRepeatInterval;
		var priorNotificationStartDate = _task.NotificationStartDate;

		_task.Name = TaskName_entry.Text;
		_task.Notes = TaskNotes_entry.Text;
		_task.DueDate = TaskDueDate_entry.Date + TaskDueTime_entry.Time;

		bool newNotificationScheduled = false;
        if (_task.DueDate != priorDueDate) {
			await  RescheduleNotificationAsync(_task);
			newNotificationScheduled = true;
		}

		if(priorNotificationStartDate != _task.NotificationStartDate && !newNotificationScheduled) {
            await RescheduleNotificationAsync(_task);
            newNotificationScheduled = true;
        }

		if (priorRepeatInterval != _task.NotificationRepeatInterval && !newNotificationScheduled) {
            await RescheduleNotificationAsync(_task);
        }

        _context.SaveChanges();

        await Navigation.PopAsync();
	}

	/// <summary>
	///  Reschedules Notification for provided task obj
	/// </summary>
	/// <param name="task">Task object to reschedule notification for</param>
	/// <returns>New Notification ID</returns>
	private static async Task RescheduleNotificationAsync(ToDoTask task) { 
		NotificationService.CancelNotification(task.NotificationId);
		await task.ScheduleNotificationAsync();
		

    }
}