using System.Runtime.CompilerServices;
using TaskIt.Data;
using TaskIt.Mechanics;
using TaskIt.Mechanics.Models;

namespace TaskIt;

public partial class CalendarDayPage : ContentPage
{

	private readonly TaskContext _context;

	private List<UserTask> DaysTasks { get; set; }
	private DateTime _dateTime { get; set; }
	public CalendarDayPage(TaskContext context, DateTime date)
	{
		_dateTime = date;
		_context = context;
		DaysTasks = _context.GetTaskForDate(date);
		DaysTasks = DaysTasks.OrderByNextOccurance();
		InitializeComponent();
		dateLbl.Text = date.ToString("d");
		PopulateTask();
	}

	public void PopulateTask() {
		if (DaysTasks.Count < 1) { return; }
		// Add Time Slots
		var previous_t = _dateTime.Date;
		foreach (var task in DaysTasks) {
			// add free time slots
            if (task.NextOccurance > previous_t) {
                var t_end = task.NextOccurance;
                var t_start = previous_t;
                var free_frame = GenerateFreeSlot(t_start, t_end);
                taskStack.Add(free_frame);
            } 
			previous_t = task.NextOccurance;
			// add task slot
            var frame = GenerateTaskSlot(task);
			HorizontalStackLayout stack = (HorizontalStackLayout)frame.Content;
			// Make task slot clickable
            var btn = new TapGestureRecognizer()
            {
                Command = new Command<int>(task_clicked),
                CommandParameter = task.Id
            };
			stack.GestureRecognizers.Add(btn);
			// Add stack to frame content
			frame.Content = stack;
			// Add frame to main Task Stack
			taskStack.Add(frame);

			if (DaysTasks.IndexOf(task) == DaysTasks.Count -1) {
				var t_end = task.NextOccurance;
				var endOfDay = _dateTime.Date.AddDays(1).AddTicks(-1);
				if (task.NextOccurance < endOfDay) {
					var free_frame = GenerateFreeSlot(t_end, endOfDay);
					taskStack.Add(free_frame);
				}

			}
		}
	}

	public static Frame GenerateFreeSlot(DateTime start, DateTime end) {
		var frame = new Frame
		{
			BorderColor = Colors.Blue,
			CornerRadius = 5,
			BackgroundColor = Colors.LightBlue,
			Opacity = 80
		};

		var stack = new HorizontalStackLayout
		{
			Spacing = 40
		};
		var titleLbl = new Label
		{
			Text = $"Free Time:",
			TextColor = Colors.Black,
			HorizontalOptions = LayoutOptions.Start,
			VerticalOptions = LayoutOptions.Center,
			FontAttributes = FontAttributes.Bold
		};
		stack.Add(titleLbl);
		var dtLbl = new Label
		{
			Text = $"{start.ToString("h:mm tt")} - {end.ToString("h:mm tt")}",
			HorizontalOptions = LayoutOptions.End,
			VerticalOptions = LayoutOptions.Center
		};
		stack.Add(dtLbl);

		frame.Content = stack;
		return frame;
	}

	public static Frame GenerateTaskSlot(UserTask task) {

		// frame
		var frame = new Frame
		{
			BorderColor = task.IsRecurring ? Colors.Green : Colors.Red,
			CornerRadius = 5,
			BackgroundColor = task.IsRecurring ? Colors.GreenYellow : Colors.DarkRed,
			Opacity = 80
		};
		// stack
		var stack = new HorizontalStackLayout{
			Spacing = 40
		};
		 
		// label title
		var titleLbl = new Label
		{
			Text = task.Name,
			TextColor = Colors.Black,
			HorizontalOptions = LayoutOptions.Start,
			VerticalOptions = LayoutOptions.Center,
			FontAttributes = FontAttributes.Bold
		};
		stack.Add(titleLbl);
		// label time
		var dtLblTxt = task.EndDate.TimeOfDay > task.NextOccurance.TimeOfDay ? task.EndDate : task.NextOccurance.AddMinutes(30);
		var dtLbl = new Label
		{
			Text = $"{task.NextOccurance.ToString("h:mm tt")} - {task.EndDate.ToString("h:mm tt")}",
			HorizontalOptions = LayoutOptions.End,
			VerticalOptions = LayoutOptions.Center
		};
		stack.Add(dtLbl);

		frame.Content = stack;
		return frame;
	}

	private async void task_clicked(int id) {
		await Navigation.PushAsync(new TaskViewPage(id, _context));
	}


}