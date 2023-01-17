using Microsoft.EntityFrameworkCore;
using TaskIt.Data;
using TaskIt.Mechanics;
using TaskIt.Mechanics.Models;

namespace TaskIt;

public partial class NewTaskPage : ContentPage
{
	private readonly TaskContext _context;

	private Dictionary<string, TimeSpan> RecurringTaskSelection = new Dictionary<string, TimeSpan>()
	{
        {"Once a day", TimeSpan.FromHours(24) },
        {"Twice a day", TimeSpan.FromHours(12) },
		{"Every other day", TimeSpan.FromDays(2) },
		{"Once a week", TimeSpan.FromDays(7) },
		{"Once a Month", TimeSpan.FromDays(30) },
		{"Twice a Month", TimeSpan.FromDays(15) }
	};

	public NewTaskPage()
	{
		//_mainPage = mainPage;
		InitializeComponent();
		_context = new TaskContext();
		var selectionList = ToDoTaskUtils.RepeatIntervalSelection.Keys.ToList();
        RepeatInterval_entry.ItemsSource = selectionList;
		RepeatInterval_entry.SelectedIndex = 0;

		selectionList = ToDoTaskUtils.NotificationStartDateSelection.Keys.ToList();
		StartNotification_entry.ItemsSource = selectionList;

		selectionList = RecurringTaskSelection.Keys.ToList();
		RepeatTaskInterval_entry.ItemsSource = selectionList;

		IsRecurring.CheckedChanged += IsRecurring_Changed;

		_context.SaveChangesFailed += SaveChangesFailed_Event;
		
		// set defualt date & time selections
		TaskStartDate_entry.Date = DateTime.Now;
		TaskStartTime_entry.Time = DateTime.Now.TimeOfDay;
        TaskDueDate_entry.Date = DateTime.Now + TimeSpan.FromDays(1);
		TaskDueTime_entry.Time = DateTime.Now.TimeOfDay;
    }

	public async void CreateTaskClicked(object sender, EventArgs e) {
		// run input checks
		if (!RunInputChecks()) {
			return;		
		}

		string modelName = this.TaskName_entry.Text;
		string modelNotes = this.TaskNotes_entry.Text;
		bool isRecurring = this.IsRecurring.IsChecked;
		// Combine date and time pickers
		DateTime modelDueDate = this.TaskDueDate_entry.Date + TaskDueTime_entry.Time;
		DateTime modelStartDate = this.TaskStartDate_entry.Date + TaskStartTime_entry.Time;
		
		// generate notification id
        Random random = new Random();
        var notificationId = random.Next(1000, 9999);
		
		// get selected repeat interval for notification ** maybe add this to a PickerChanged Event and assign to variable to increase performance **
		var repeatIntervalSelectionVal = RepeatInterval_entry.SelectedItem.ToString();
		TimeSpan repeat = TimeSpan.Zero;
		foreach (var item in ToDoTaskUtils.RepeatIntervalSelection) { 
			if (item.Key == repeatIntervalSelectionVal)
			{
				repeat = item.Value;
				break;
			}
		}
		
		// Get selected start date for notification
		var notificationStartDateSelectionVal = StartNotification_entry.SelectedItem.ToString();
		TimeSpan start = TimeSpan.FromDays(1);
		foreach (var item in ToDoTaskUtils.NotificationStartDateSelection) { 
			if (item.Key == notificationStartDateSelectionVal)
			{
				start = item.Value;
				break;
			}
		}
		
		// Create task obj
        ToDoTask task = new ToDoTask()
		{
			Name = modelName,
			Notes = modelNotes,
			StartDate = modelStartDate,
			DueDate = isRecurring ? DateTime.MinValue : modelDueDate,
			NotificationId = notificationId,
			RecurringTask = isRecurring
		};
		// set task *NotificationStartDate* & *RecurringInterval* based on if the task is a recurring task
		 if (task.RecurringTask) {
			// Get value of selection from value dictionary
			task.RecurringInterval = RecurringTaskSelection.Where(m => m.Key == RepeatTaskInterval_entry.SelectedItem.ToString()).FirstOrDefault().Value;
			task.NextOccurance = task.StartDate + task.RecurringInterval;
            task.NotificationStartDate = task.StartDate - start;
			task.NotificationRepeatInterval = task.RecurringInterval;
        } else {
			task.NotificationRepeatInterval = repeat;
            task.NotificationStartDate = modelDueDate - start;
        }

		// Schedule notification
		await task.ScheduleNotificationAsync(); 
		// add task obj to db and save
		_context.ToDoTasks.Add(task);
		_context.SaveChanges();
		// verify task saved to db
		await VerifyTaskSaved(task);
		//return to previous page
		await Navigation.PopAsync();
	}

	private bool  RunInputChecks() {
		try {
			// Start date check
			var startDate = TaskStartDate_entry.Date + TaskStartTime_entry.Time;
			if (startDate < DateTime.Now) {
				DisplayAlert("Start Date Invalid", "The start date you have entered is in the past. Please enter a start date that is in the future.", "Okay");
				return false;
			}
			// Due date checked
			var dueDate = TaskDueDate_entry.Date + TaskDueTime_entry.Time;
			if (dueDate < DateTime.Now) {
				DisplayAlert("Due Date Invalid", "The due date you have entered is in the past. Please enter a due date that is in the future.", "Okay");
				return false;
			}
			// Title checked
			var title = TaskName_entry.Text;
			if (title.Length < 2) {
				DisplayAlert("Title Invalid", "You're title either has no characters or not enough characters, please enter a new title and try again", "Okay");
				return false;
			}
		} catch (NullReferenceException nre) {
			Console.WriteLine(nre.ToString());
            DisplayAlert("Title Invalid", "You're title either has no characters or not enough characters, please enter a new title and try again", "Okay");
            return false;
		} catch (Exception ex) {
			Console.WriteLine($"*!* Exception thrown in NewTaskPage => RunInputChecks; {ex.ToString()}");
			return false;
		}

		return true;
	}

	/// <summary>
	/// Verify that the task provided has been saved in DB. if not found in DB attempt to 
	///		add and commit asynchornously
	/// </summary>
	/// <param name="task">Task to verify saved</param>
	/// <returns></returns>
	private async Task VerifyTaskSaved(ToDoTask task) {
		if (!_context.ToDoTasks.Any(m => m.Id == task.Id)) {
			await _context.ToDoTasks.AddAsync(task);
			await _context.SaveChangesAsync();
		}
	}

	private void SaveChangesFailed_Event(object sender, EventArgs args) {
		Console.WriteLine("Failed to save changes...");
	}

    private void IsRecurring_Changed(object sender, CheckedChangedEventArgs e) {
        if (IsRecurring.IsChecked) {
            ShowRecurringTaskProperties();
        } else {
            ShowNonRecurringTaskProperties();
        }
    }

    private void ShowRecurringTaskProperties() {
		Dispatcher.Dispatch(() => {
            DueDate_lbl.IsVisible = false;
            DueDate_group.IsVisible = false;
            RepeatInterval_lbl.IsVisible = false;
            RepeatInterval_entry.IsVisible = false;
            RepeatTaskInterval_lbl.IsVisible = true;
            RepeatTaskInterval_entry.IsVisible = true;
        });
    }
    private void ShowNonRecurringTaskProperties() {
		Dispatcher.Dispatch(() => {
            DueDate_lbl.IsVisible = true;
            DueDate_group.IsVisible = true;
            RepeatInterval_lbl.IsVisible = true;
            RepeatInterval_entry.IsVisible = true;
            RepeatTaskInterval_lbl.IsVisible = false;
            RepeatTaskInterval_entry.IsVisible = false;
        });
    }
}