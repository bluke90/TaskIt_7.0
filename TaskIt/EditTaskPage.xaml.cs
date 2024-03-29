using Microsoft.EntityFrameworkCore;
using Plugin.LocalNotification.Json;
using TaskIt.Data;
using TaskIt.Mechanics;
using TaskIt.Mechanics.Models;
using static TaskIt.Mechanics.Utilities;

namespace TaskIt;

public partial class EditTaskPage : ContentPage
{
	private readonly TaskContext _context;

	private UserTask _task { get; set; }

    private Dictionary<string, TimeSpan> RecurringTaskSelection = new Dictionary<string, TimeSpan>()
    {
        {"None", TimeSpan.MinValue },
        {"Once a day", TimeSpan.FromHours(24) },
        {"Twice a day", TimeSpan.FromHours(12) },
        {"Every other day", TimeSpan.FromDays(2) },
        {"Once a week", TimeSpan.FromDays(7) },
        {"Once a Month", TimeSpan.FromDays(30) },
        {"Twice a Month", TimeSpan.FromDays(15) }
    };

    private List<DaysOfWeek> SelectedDays = new List<DaysOfWeek>();

    public EditTaskPage(TaskContext context, int taskId) {
        _context = context;
        _task = _context.UserTasks.Include(m => m.Notification).FirstOrDefault(m => m.Id == taskId);
		InitializeComponent();
		PopulateData();

        // Populate RepeatInterval Selection
        var selectionList = ToDoTaskUtils.RepeatIntervalSelection.Keys.ToList();
        RepeatInterval_entry.ItemsSource = selectionList;
		RepeatInterval_entry.SelectedItem = object.ReferenceEquals(_task.Notification.RepeatInterval, null) ? 0 : ToDoTaskUtils.RepeatIntervalSelection.Where(m => m.Value == _task.Notification.RepeatInterval).First().Key;
        // Populate NotificaitonStartDate Selection
        selectionList = ToDoTaskUtils.NotificationStartDateSelection.Keys.ToList();
        StartNotification_entry.ItemsSource = selectionList;
        TimeSpan timespan = _task.EndDate - _task.Notification.StartDate;
        foreach (var item in ToDoTaskUtils.NotificationStartDateSelection) {
			if (item.Value == timespan) {
				StartNotification_entry.SelectedItem = item.Key;
			}
		}
        // Populate RecurringTask Selection
        // selectionList = RecurringTaskSelection.Keys.ToList();
        // RepeatTaskInterval_entry.ItemsSource = selectionList;
        // RepeatTaskInterval_entry.SelectedIndex = 0;
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