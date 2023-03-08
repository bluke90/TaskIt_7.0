using Microsoft.EntityFrameworkCore;
using TaskIt.Data;
using TaskIt.Mechanics.Models;

namespace TaskIt;

public partial class DailySchedulePage : ContentPage
{
    private readonly TaskContext _context;

    private List<UserTask> Tasks { get; set; }

    private List<TimeSpan> NonOccupiedTimes { get; set; }

    // Create timeline

    private List<string> FilterOptions = new List<string>()
    {
        "Start Date",
        "Title",
        "Order by occurance"
    };

    public DailySchedulePage(TaskContext context) {
        _context = context;
        NonOccupiedTimes  = new List<TimeSpan>();
        
        InitializeComponent();

        FilterSelection.ItemsSource = FilterOptions;
        FilterSelection.SelectedIndexChanged += FilterSelection_SelectedIndexChanged;
        FilterSelection.SelectedIndex = 2;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args) {
        base.OnNavigatedTo(args);
        PopulateTask();
    }

    private void FilterSelection_SelectedIndexChanged(object sender, EventArgs e) {
        PopulateTask();
    }

    private async void PopulateTask() {
        // clear stack before repopulating
        if (Tasks != null) {
            Tasks.Clear();
        }
        RecurringTask_stack.Clear();

        // Get & Sort Task
        switch (FilterSelection.SelectedItem.ToString()) {
            case "Start Date":
                Tasks = await _context.UserTasks.Where(m => m.Finished == false).Where(m => m.IsRecurring == true).OrderBy(m => m.StartDate).ToListAsync();
                break;
            case "Order by occurance":
                Tasks = await _context.UserTasks.Where(m => m.Finished == false).Where(m => m.IsRecurring == true).ToListAsync();
                Tasks = Tasks.OrderByNextOccurance();
                break;
            case "Title":
                Tasks = await _context.UserTasks.Where(m => m.Finished == false).Where(m => m.IsRecurring == true).OrderBy(m => m.Name).ToListAsync();
                break;
        }

        // foreach task, create a display obj
        foreach (UserTask task in Tasks) {

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

            var label_nextOcurrance = new Label()
            {
                Text = $"{task.GetNextOccuranceOfTask().ToString("M/d/y h:mm tt")}",
                VerticalOptions = LayoutOptions.End,
                TextColor = Colors.White
            };

            var button = new TapGestureRecognizer()
            {
                Command = new Command<int>(Button_Tapped),
                CommandParameter = task.Id
            };

            // Build Stacklayout
            stackLayout.GestureRecognizers.Add(button);
            stackLayout.Add(label_name);
            stackLayout.Add(label_nextOcurrance);
            // set frame content to stacklayout
            frame.Content = stackLayout;
            // add frame to recurring task stack
            RecurringTask_stack.Add(frame);
        }
    }

    private async void Button_Tapped(int id) {
        //App.Current.MainPage = new TaskViewPage(id, _context);
        try {
            await Navigation.PushAsync(new TaskViewPage(id, _context));
        } catch (NullReferenceException e) {
            PopulateTask();
            return;
        }
    }
}