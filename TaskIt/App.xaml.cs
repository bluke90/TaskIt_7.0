using TaskIt.Mechanics;

namespace TaskIt;

public partial class App : Application
{
	// DI Instances
	private readonly PeriodicUpdateService _updateService;


	public App(PeriodicUpdateService updateService)
	{
		// Set DI Instances
		_updateService = updateService;

		// Start Services
		InitializeServices();

		// Proceed with startup
		InitializeComponent();
		MainPage = new AppShell();
	}
	/// <summary>
	/// Initialize Any Services that need to be started on app StartUp
	/// </summary>
	private void InitializeServices() {
		// Generate Cancellation Token
        var source = new CancellationTokenSource();
        var token = source.Token;

		// Initialize Services
		//		-- PeriodicUpdateService
        _updateService.IsEnabled = true;
        _updateService.StartAsync(token);

    }

}
