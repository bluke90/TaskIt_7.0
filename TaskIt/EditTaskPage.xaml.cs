using Android.App;
using Plugin.LocalNotification.Json;
using TaskIt.Data;
using TaskIt.Mechanics;
using TaskIt.Mechanics.Models;

namespace TaskIt;

public partial class EditTaskPage : ContentPage
{
	private readonly TaskContext _context;

	private UserTask _task { get; set; }

	public EditTaskPage(TaskContext context, int taskId) {
        _context = context;
        _task = _context.UserTasks.FirstOrDefault(m => m.Id == taskId);
		InitializeComponent();
		PopulateData();

		RepeatInterval_entry.ItemsSource = ToDoTaskUtils.RepeatIntervalSelection.Keys.ToList();
		StartNotification_entry.ItemsSource = ToDoTaskUtils.NotificationStartDateSelection.Keys.ToList();
		foreach (var item in ToDoTaskUtils.RepeatIntervalSelection) {
			if (item.Value == _task.Notification.RepeatInterval) {
				RepeatInterval_entry.SelectedItem = item.Key;
			}
		}
        TimeSpan timespan = _task.EndDate - _task.Notification.StartDate;
        foreach (var item in ToDoTaskUtils.NotificationStartDateSelection) {
			if (item.Value == timespan) {
				StartNotification_entry.SelectedItem = item.Key;
			}
		}
	}

	private void PopulateData() {
		TaskName_entry.Text = _task.Name;
		TaskNotes_entry.Text = _task.Notes;
		TaskDueDate_entry.Date = _task.EndDate;
		TaskDueTime_entry.Time = _task.EndDate.TimeOfDay;
	}

	private async void doneClicked(object sender, EventArgs e) {
		var priorName = _task.Name;
		var priorNotes = _task.Notes;
		var priorDueDate = _task.EndDate;
		var priorRepeatInterval = _task.Notification.RepeatInterval;
		var priorNotificationStartDate = _task.Notification.StartDate;

		_task.Name = TaskName_entry.Text;
		_task.Notes = TaskNotes_entry.Text;
		_task.EndDate = TaskDueDate_entry.Date + TaskDueTime_entry.Time;

		bool newNotificationScheduled = false;
        if (_task.EndDate != priorDueDate) {
			await  _task.RescheduleTaskNotifcationAsync();
			newNotificationScheduled = true;
		}

		if(priorNotificationStartDate != _task.Notification.StartDate && !newNotificationScheduled) {
            await _task.RescheduleTaskNotifcationAsync();
            newNotificationScheduled = true;
        }

		if (priorRepeatInterval != _task.Notification.RepeatInterval && !newNotificationScheduled) {
            await _task.RescheduleTaskNotifcationAsync();
        }

        _context.SaveChanges();

        await Navigation.PopAsync();
	}

}