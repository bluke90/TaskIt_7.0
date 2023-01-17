using TaskIt.Mechanics;

namespace TaskIt;

public partial class App : Application
{
	private readonly PeriodicUpdateService _updateService;
	private CancellationTokenSource source { get; set; }
	private CancellationToken token { get; set; }


	public App(PeriodicUpdateService updateService)
	{
		source = new CancellationTokenSource();
		token = source.Token;
		_updateService = updateService;
		_updateService.IsEnabled = true;
		_updateService.StartAsync(token);

		InitializeComponent();

		MainPage = new AppShell();
	}
}
