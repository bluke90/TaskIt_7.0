using System.Runtime.CompilerServices;
using TaskIt.Data;
using TaskIt.Mechanics;
using TaskIt.Mechanics.Models;

namespace TaskIt;

public partial class CalendarDayPage : ContentPage
{

	private readonly TaskContext _context;

	private List<ToDoTask> DaysTasks { get; set; }

	public CalendarDayPage(TaskContext context, DateTime date)
	{
		_context = context;
		DaysTasks = _context.GetTaskForDate(date);
		DaysTasks = DaysTasks.OrderByNextOccurance();
		InitializeComponent();
		dateLbl.Text = date.ToString("d");
		PopulateTask();
	}

	public void PopulateTask() {
		if (DaysTasks.Count < 1) { return; }

		foreach (var task in DaysTasks) {
			var frame = GenerateTaskSlot(task);
			HorizontalStackLayout stack = (HorizontalStackLayout)frame.Content;
            var btn = new TapGestureRecognizer()
            {
                Command = new Command<int>(task_clicked),
                CommandParameter = task.Id
            };
			stack.GestureRecognizers.Add(btn);
			frame.Content = stack;
			taskStack.Add(frame);
		}
	}


	public static Frame GenerateTaskSlot(ToDoTask task) {

		// frame
		var frame = new Frame
		{
			BorderColor = task.RecurringTask ? Colors.Green : Colors.Red,
			CornerRadius = 5,
			BackgroundColor = task.RecurringTask ? Colors.GreenYellow : Colors.DarkRed,
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
		var dtLbl = new Label
		{
			Text = task.RecurringTask ? task.GetNextOccuranceOfTask().ToString() : task.DueDate.ToString(),
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