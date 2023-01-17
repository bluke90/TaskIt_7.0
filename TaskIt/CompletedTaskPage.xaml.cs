using TaskIt.Data;
using TaskIt.Mechanics.Models;

namespace TaskIt;

public partial class CompletedTaskPage : ContentPage
{
	private readonly TaskContext _context;
	private List<ToDoTask> Tasks { get; set; }
    private List<string> FilterOptions = new List<string>()
    {
        "Start Date",
        "Due Date",
        "Title"
    };
    public CompletedTaskPage(TaskContext context)
	{
		_context = context;
		Tasks = _context.ToDoTasks.Where(m => m.Finished == true).ToList();
		InitializeComponent();
        FilterSelection.ItemsSource = FilterOptions;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args) {
        base.OnNavigatedTo(args);
        PopulateTask();
    }

    public void PopulateTask() {
        // clear previous task
        if (Tasks != null) {
            Tasks.Clear();
        }
        TasksStack.Clear();
        // get task from db
        if (FilterSelection.SelectedItem == null) {
            Tasks = _context.ToDoTasks.Where(m => m.Finished == true).ToList();
        } else {
            switch (FilterSelection.SelectedItem.ToString()) {
                case "Start Date":
                    Tasks = _context.ToDoTasks.Where(m => m.Finished == true).OrderBy(m => m.StartDate).ToList();
                    break;
                case "Due Date":
                    Tasks = _context.ToDoTasks.Where(m => m.Finished == true).OrderBy(m => m.DueDate).ToList();
                    break;
                case "Title":
                    Tasks = _context.ToDoTasks.Where(m => m.Finished == true).OrderBy(m => m.Name).ToList();
                    break;
                default:
                    Tasks = _context.ToDoTasks.Where(m => m.Finished == true).ToList();
                    break;
            }
        }
        // foreach task, create a display obj
        foreach (ToDoTask task in Tasks) {

            var frame = new Frame()
            {
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Color.FromRgba("#2B0B98"),
                BorderColor = Color.FromRgba("#3E8EED"),
                Margin = new Thickness(4, 0)
            };

            var stackLayout = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 20
            };

            var label_name = new Label()
            {
                Text = task.Name,
                VerticalOptions = LayoutOptions.Start,
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold
            };

            var label_due = new Label()
            {
                Text = $"{task.DueDate.ToString("M/d/y h:mm tt")}",
                VerticalOptions = LayoutOptions.End,
                TextColor = Colors.White
            };


            var button = new TapGestureRecognizer()
            {
                Command = new Command<int>(Button_Tapped),
                CommandParameter = task.Id
            };


            stackLayout.GestureRecognizers.Add(button);
            stackLayout.Add(label_name);
            stackLayout.Add(label_due);
            frame.Content = stackLayout;
            TasksStack.Add(frame);
        }

    }

    private async void Button_Tapped(int id) {
        //App.Current.MainPage = new TaskViewPage(id, _context);
        try {
            await Navigation.PushAsync(new TaskViewPage(id, _context));
        } catch (ArgumentNullException e) {
            return;
        }
    }

}