
using TaskIt.Data;
using TaskIt.Mechanics.Models;

namespace TaskIt;

public partial class TaskViewPage : ContentPage
{
	private readonly TaskContext _context;
	private ToDoTask _task { get; set; }
	public TaskViewPage(int taskId, TaskContext context)
	{
		_context = context;
		_task = _context.ToDoTasks.FirstOrDefault(m => m.Id == taskId);
		InitializeComponent();
		PopulateData();

		if (_task.RecurringTask) {
			dueDate_frame.IsVisible = false;
			nextDate_frame.IsVisible = true;
		}

		if (_task.RecurringTask || _task.Finished) {
			Dispatcher.Dispatch(() => {
                complete_btn.IsVisible = false;
                delete_btn.IsVisible = true;
            });
		}

	}

	/// <summary>
	/// Populate labels on screen with details from task obj
	/// </summary>
	private void PopulateData() {
		name_label.Text = _task.Name;
		notes_label.Text = _task.Notes;
		startDate_label.Text = $"{_task.StartDate.ToShortDateString()} {_task.StartDate.ToShortTimeString()}";
		dueDate_label.Text = $"{_task.DueDate.ToShortDateString()} {_task.DueDate.ToShortTimeString()}";
		nextDate_label.Text = $"{_task.NextOccurance.ToShortDateString()} {_task.NextOccurance.ToShortTimeString()}";
	}

    private async void deleteClicked(object sender, EventArgs e) {
        // Verify deletion before performing task
        bool confirmed = await DisplayAlert("Delete Task", "Are you sure you would like to delete this task? (you will not be able to recover after deletion", "Yes", "Cancel");

        if (confirmed) {
            _context.ToDoTasks.Remove(_task);
            await _context.SaveChangesAsync();
        }
        ReturnToMainPage();
    }

    private async void editClicked(object sender, EventArgs e) {
        await Navigation.PushAsync(new EditTaskPage(_context, _task.Id));
	}

	private async void completedClicked(object sender, EventArgs e) {
		bool confirmed = await DisplayAlert("Complete Task", "Are you sure you would like to mark this task as completed?", "Yes", "No");

		if (confirmed) {
			_task.Finished = true;
            await _context.SaveChangesAsync();
        }
		ReturnToMainPage();
	}


    private void doneClicked(object sender, EventArgs e) {
        ReturnToMainPage();
    }

	private void startTaskClicked(object sender, EventArgs e)
	{
		throw new NotImplementedException();
	}

    private async void ReturnToMainPage() {
		//App.Current.MainPage = new NavigationPage(new MainPage());
		//App.Current.MainPage = new AppShell();
		//await Shell.Current.GoToAsync("..");
		await Navigation.PopAsync();
    }
}