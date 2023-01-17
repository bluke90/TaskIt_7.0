using TaskIt.Data;
using TaskIt.Mechanics;

namespace TaskIt;

public partial class CalendarViewPage : ContentPage
{
    // create a grid to hold the calendar
    private Grid calendarGrid;

    private readonly TaskContext _context;

    public CalendarViewPage()
	{
        _context = new TaskContext();
		InitializeComponent();
	}

    protected override void OnNavigatedTo(NavigatedToEventArgs args) {
        base.OnNavigatedTo(args);

        if (calendarGrid != null && calendarGrid.Children.Count > 0) {
            calendarGrid.Clear();
        }

        CreateCalendar();
    }

    private void CreateCalendar() {

        calendarGrid = new Grid
        {
            BackgroundColor = Color.FromArgb("#353535"),
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            RowSpacing = 1,
            ColumnSpacing = 1
        };

        // create columns for grid
        for (int i = 0; i <7; i++) {
            calendarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        // create rows for grid
        for (int i = 0; i < 6; i++) {
            calendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        }


        for (int i = 0; i < 7; i++) {
            var label = GetLabelForGridSpace(i);

            // Add label to grid         
            calendarGrid.Add(label, i, 0);
        }

        DateTime current_DateTime = DateTime.Now;
        int current_Month = current_DateTime.Month;
        int current_Year = current_DateTime.Year;
        DateTime firstDayOfMonth = new DateTime(current_Year, current_Month, 1);
        int firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
        int numDaysInMonth = DateTime.DaysInMonth(current_Year, current_Month);
        // Generate Grid Spaces
        for (int i = 1; i <= numDaysInMonth; i++) {
            var frame = new DateTime(current_Year, current_Month, i).Date == DateTime.Now.Date ? GetGridSpaceFrame(true) : GetGridSpaceFrame();
            var stack = new VerticalStackLayout();
            var label = new Label
            {
                Text = i.ToString(),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold
            };
            stack.Add(label);
            // Num task for date label
            int numTaskForDate = 0;
            numTaskForDate = _context.GetTaskForDate(new DateTime(current_Year, current_Month, i)).Count;
            if (numTaskForDate > 0) {
                var numTaskLbl = new Label
                {
                    Text = numTaskForDate > 9 ? "+9" : $"{numTaskForDate}",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Colors.GreenYellow,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 18,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                stack.Add(numTaskLbl);
            }
            var btn = new TapGestureRecognizer()
            {
                Command = new Command<DateTime>(Button_Tapped),
                CommandParameter = new DateTime(current_Year, current_Month, i)
            };
            stack.GestureRecognizers.Add(btn);

            frame.Content = stack;

            calendarGrid.Add(frame, (i + firstDayOfWeek - 1) % 7, (i + firstDayOfWeek - 1) / 7 + 1);
        }

        Content = calendarGrid;


    }

    private async void Button_Tapped(DateTime date) {
        await Navigation.PushAsync(new CalendarDayPage(_context, date));
    }

    // Create Label for grid space
    private static Label GetLabelForGridSpace(int dayIndex) {
        string[] _dayNames = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
        Label Label = new Label
        {
            Margin = new Thickness(1),
            Text = _dayNames[dayIndex],
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        }; 

        return Label;
    }

    // Create frame for grid space
    private static Frame GetGridSpaceFrame(bool currentDay = false) {
        var frame = new Frame
        {
            CornerRadius = 0,
            Margin = new Thickness(0),
            Padding = new Thickness(5,5),
            BorderColor = currentDay ? Colors.White : Colors.Black,
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        return frame;
    }
}