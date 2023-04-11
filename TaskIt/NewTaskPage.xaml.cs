using Microsoft.EntityFrameworkCore;
using TaskIt.Data;
using TaskIt.Mechanics;
using TaskIt.Mechanics.Models;
using static TaskIt.Mechanics.Utilities;

namespace TaskIt;

public partial class NewTaskPage : ContentPage
{
	private readonly TaskContext _context;

	private UserTask UserTask { get; set; }

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

	public NewTaskPage()
	{
		//_mainPage = mainPage;
		InitializeComponent();
		_context = new TaskContext();
		// Populate RepeatInterval Selection
		var selectionList = ToDoTaskUtils.RepeatIntervalSelection.Keys.ToList();
        RepeatInterval_entry.ItemsSource = selectionList;
		RepeatInterval_entry.SelectedIndex = 0;
		// Populate NotificaitonStartDate Selection
		selectionList = ToDoTaskUtils.NotificationStartDateSelection.Keys.ToList();
		StartNotification_entry.ItemsSource = selectionList;
		// Populate RecurringTask Selection
		selectionList = RecurringTaskSelection.Keys.ToList();
		RepeatTaskInterval_entry.ItemsSource = selectionList;
		RepeatTaskInterval_entry.SelectedIndex = 0;
        
		AddDaysOfWeekSelection();

        // Set Event Handlers
        IsRecurring.CheckedChanged += IsRecurring_Changed;
		_context.SaveChangesFailed += SaveChangesFailed_Event;

        // Recurring options event handlers
        HasStartDate.CheckedChanged += StartDateOptionChanged;
        HasEndDate.CheckedChanged += EndDateOptionChanged;

        // set defualt date & time selections
        TaskStartDate_entry.Date = DateTime.Now;
		TaskStartTime_entry.Time = DateTime.Now.TimeOfDay;
        TaskDueDate_entry.Date = DateTime.Now + TimeSpan.FromDays(1);
		TaskDueTime_entry.Time = DateTime.Now.TimeOfDay;
    }
	
	// Main task creation function
	public async void CreateTaskClicked(object sender, EventArgs e) 
	{
		// run input checks
		if (!RunInputChecks()) {
			return;		
		}

		// Combine date and time pickers
		DateTime modelEndDate = !HasEndDate.IsChecked && IsRecurring.IsChecked ? DateTime.MinValue + TaskDueTime_entry.Time : this.TaskDueDate_entry.Date + TaskDueTime_entry.Time;
        DateTime modelStartDate = !HasStartDate.IsChecked && IsRecurring.IsChecked ? DateTime.Now + TaskStartTime_entry.Time : this.TaskStartDate_entry.Date + TaskStartTime_entry.Time;
				
		// get selected repeat interval for notification ** maybe add this to a PickerChanged Event and assign to variable to increase performance **
		var repeatIntervalSelectionVal = RepeatInterval_entry.SelectedItem.ToString();
		TimeSpan repeat = TimeSpan.Zero;
		foreach (var item in ToDoTaskUtils.RepeatIntervalSelection)
		{ 
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
        // Set Base Info
         UserTask = new UserTask()
        {
            Name = TaskName_entry.Text,
            Notes = TaskNotes_entry.Text,
            IsRecurring = IsRecurring.IsChecked,
			StartDate = modelStartDate,
			EndDate = modelEndDate
        };
		// Configure recurring and non-recurring
        if (UserTask.IsRecurring) {
			UserTask.BuildRecurring();
			if (RepeatTaskInterval_entry.SelectedIndex != 0) {
				UserTask.Recurring.RecurringInterval = RecurringTaskSelection.Where(m => m.Key == RepeatTaskInterval_entry.SelectedItem.ToString()).FirstOrDefault().Value;
            }
            UserTask.NextOccurance = UserTask.StartDate + UserTask.Recurring.RecurringInterval;
            UserTask.Notification.StartDate = UserTask.StartDate - start;
            UserTask.Notification.RepeatInterval = UserTask.Recurring.RecurringInterval;
			UserTask.Recurring.SelectedDays = SelectedDays;
			UserTask.EndDate = HasEndDate.IsChecked ? modelEndDate : (DateTime.MinValue + modelEndDate.TimeOfDay);
        } else {
			UserTask.BuildNonRecurring();
			UserTask.StartDate = modelStartDate + UserTask.Notification.RepeatInterval;
			UserTask.NextOccurance = UserTask.StartDate;
            UserTask.Notification.RepeatInterval = repeat;
            UserTask.Notification.StartDate = UserTask.Notification.RepeatInterval > TimeSpan.Zero ? modelStartDate + repeat : modelStartDate;
        }

        // Schedule notification
        await UserTask.GenerateNotificationIdAsync();
        await UserTask.ScheduleNotificationAsync(); 
		// add task obj to db and save
		_context.UserTasks.Add(UserTask);
		_context.SaveChanges();
		// verify task saved to db
		await VerifyTaskSaved(UserTask);
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
			// Exception handling
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
	private async Task VerifyTaskSaved(UserTask task) {
		// If task not saved, save again
		if (!_context.UserTasks.Any(m => m.Id == task.Id)) {
			await _context.UserTasks.AddAsync(task);
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

	private void AddDaysOfWeekSelection() {
		//build days of week selection
		int count = 0;
		foreach (var day in Enum.GetValues<DaysOfWeek>()) {
			count++;
			var frame = BuildDaySelectionFrame(day);

			if (count < 5) {
				days_col_top.Add(frame);
			} else { days_col_bottom.Add(frame); }

		}
	}

	private Frame BuildDaySelectionFrame(DaysOfWeek Day) {
        
		// Day Frame
		var frame = new Frame()
        {
            BorderColor = Colors.Black,
            BackgroundColor = Colors.White,
            CornerRadius = 0,
			Padding = new Thickness(12,14),
			BindingContext = Day
        };
		// Day Lable
        var dayLbl = new Label()
        {
            Text = Day.ToString(),
            TextColor = Colors.Black,
            FontAttributes = FontAttributes.Bold,
			FontSize = 14
        };
		// Content
        frame.Content = dayLbl;
		// Create Recognizer (Button) 
        var recognizer = new TapGestureRecognizer()
		{
			Command = new Command<Frame>(DaySelected),
			CommandParameter = frame
        };
        frame.GestureRecognizers.Add(recognizer);

		return frame;
    }

	public void DaySelected(Frame day) {
		
		switch (SelectedDays.Contains((DaysOfWeek)day.BindingContext)) {
			case true:
				SelectedDays.Remove((DaysOfWeek)day.BindingContext);
				day.BorderColor = Colors.Black;
                break;
			case false:
				SelectedDays.Add((DaysOfWeek)day.BindingContext);
				day.BorderColor = Colors.Blue;
				break;
		}
	}

    private void ShowRecurringTaskProperties() {
		Dispatcher.Dispatch(() => {
            DueDate_lbl.Text = "End Date";
            RepeatInterval_lbl.IsVisible = false;
            RepeatInterval_entry.IsVisible = false;
            RepeatTaskInterval_lbl.IsVisible = true;
            RepeatTaskInterval_entry.IsVisible = true;
			// days of week selection
			DaysOfWeek_lbl.IsVisible = true;
			days_col_top.IsVisible = true;
			days_col_bottom.IsVisible = true;
			// Recurring options
			RecurringOpt_1.IsVisible = true;
            RecurringOpt_2.IsVisible = true;
            TaskStartDate_entry.IsVisible = false;
            TaskDueDate_entry.IsVisible = false;
        });
    }
    private void ShowNonRecurringTaskProperties() {
		Dispatcher.Dispatch(() => {
            DueDate_lbl.Text = "Due Date";
            RepeatInterval_lbl.IsVisible = true;
            RepeatInterval_entry.IsVisible = true;
            RepeatTaskInterval_lbl.IsVisible = false;
            RepeatTaskInterval_entry.IsVisible = false;
            // days of week selection
            DaysOfWeek_lbl.IsVisible = false;
            days_col_top.IsVisible = false;
            days_col_bottom.IsVisible = false;
            // Recurring options
            RecurringOpt_1.IsVisible = false;
            RecurringOpt_2.IsVisible = false;
            TaskStartDate_entry.IsVisible = true;
            TaskDueDate_entry.IsVisible = true;
        });
    }

	private void StartDateOptionChanged(object sender, EventArgs args) {
		switch (HasStartDate.IsChecked) {
			case true:
				TaskStartDate_entry.IsVisible = true;
				break;
			case false:
                TaskStartDate_entry.IsVisible = false;
                break;
		}
		return;
	}

    private void EndDateOptionChanged(object sender, EventArgs args) {
        switch (HasEndDate.IsChecked) {
            case true:
				TaskDueDate_entry.IsVisible = true;
                break;
            case false:
                TaskDueDate_entry.IsVisible = false;
                break;
        }
        return;
    }

}