// using Android.Provider;
using TaskIt.Data;
using TaskIt.Mechanics.Models;

namespace TaskIt;

public partial class NewNotesPage : ContentPage
{
	private readonly TaskContext _context;

	private List<ToDoTask> Tasks { get; set; }

	public NewNotesPage(TaskContext context)
	{
		_context = context;
		InitializeComponent();
		
		Tasks = _context.ToDoTasks.Where(m => m.Finished == false).ToList();
		var selectionListName = Tasks.Select(m => m.Name).ToList();
		var selectionListDue = Tasks.Select(m => m.DueDate).ToList();
		var selectionList = new List<string>();
		for (int i = 0; i < Tasks.Count; i++) {
			selectionList.Add($"{selectionListName[i]}  |  {selectionListDue[i].ToString("M/d/y h:mm tt")}");
		}
		RelatedTask.ItemsSource = selectionList;
	}

	private async void NoteComplete_clicked(object sender, EventArgs args) {
		CreateNote();
		await Navigation.PopAsync();
	}

	private async void CreateNote() {
		var title = noteTitle_entry.Text;
		var note = note_entry.Text;

		var task = GetSelectedTask();

		Note noteObj = new Note
		{
			Title = title,
			Details = note
		};

		if (task != null) { noteObj.ToDoTask = task; }

		_context.Notes.Add(noteObj);
		await _context.SaveChangesAsync();
	}

	private ToDoTask GetSelectedTask()
	{
		foreach(var task in Tasks)
		{
			if (task.Name == RelatedTask.SelectedItem.ToString())
			{
				return task;
			}
		}

		return null;
	}


}