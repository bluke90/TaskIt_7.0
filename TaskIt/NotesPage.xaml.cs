using TaskIt.Data;

namespace TaskIt;

public partial class NotesPage : ContentPage
{
	private readonly TaskContext _context;

	public NotesPage()
	{
		_context = new TaskContext();
		InitializeComponent();
	}

	private async void newNoteClicked(object sender, EventArgs e)
	{
        await Navigation.PushAsync(new NewNotesPage(_context));
    }
}