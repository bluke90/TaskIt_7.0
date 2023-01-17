﻿//using Android.Text.Style;
//using AndroidX.Core.View;
using Microsoft.Maui.Controls.Xaml;
using TaskIt.Data;
using TaskIt.Mechanics.Models;
using TaskIt.Mechanics;

namespace TaskIt;

public partial class MainPage : ContentPage
{

	private readonly TaskContext _context;
	private List<string> FilterOptions = new List<string>()
	{
		"Start Date",
		"Due Date",
		"Title"
	};
	public MainPage()
	{
		 		
        _context = new TaskContext();
        InitializeComponent();
        FilterSelection.ItemsSource = FilterOptions;
		FilterSelection.SelectedIndexChanged += FilterSelection_SelectedIndexChanged;
		//PopulateTask();
	}

	private void FilterSelection_SelectedIndexChanged(object sender, EventArgs e) {
		PopulateTask();
	}

	protected override void OnNavigatedTo(NavigatedToEventArgs args) {
		base.OnNavigatedTo(args);
		PopulateTask();
	}

	private List<ToDoTask> Tasks { get; set; }

	public void PopulateTask() {
		// clear previous task
        if (Tasks != null) {
            Tasks.Clear();
        }
        TasksStack.Clear();
		// get task from db
		if (FilterSelection.SelectedItem == null) {
			Tasks = _context.ToDoTasks.Where(m=> m.Finished == false).Where(m => m.RecurringTask == false).ToList();
		} else {
			switch (FilterSelection.SelectedItem.ToString()) {
				case "Start Date":
					Tasks = _context.ToDoTasks.Where(m => m.Finished == false).Where(m => m.RecurringTask == false).OrderBy(m => m.StartDate).ToList();
					break;
				case "Due Date":
					Tasks = _context.ToDoTasks.Where(m => m.Finished == false).Where(m => m.RecurringTask == false).OrderBy(m => m.DueDate).ToList();
					break;
				case "Title":
					Tasks = _context.ToDoTasks.Where(m => m.Finished == false).Where(m => m.RecurringTask == false).OrderBy(m => m.Name).ToList();
					break;
				default:
					Tasks = _context.ToDoTasks.Where(m => m.Finished == false).Where(m => m.RecurringTask == false).ToList();
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
				Margin = new Thickness(4,0)
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
		} catch (NullReferenceException e) {
			PopulateTask();
			return;
		}
	}

	public async void NewTaskBtnClicked(object sender, EventArgs e) {
		//App.Current.MainPage = new NewTaskPage();
		await Navigation.PushAsync(new NewTaskPage());
		return;
	}

	public async void CompletedTaskButton_Tapped(object sender, EventArgs args) {
		await Navigation.PushAsync(new CompletedTaskPage(_context));
	}

}

