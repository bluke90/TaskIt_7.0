using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskIt.Data;
using TaskIt.Mechanics;

namespace TaskIt;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>() 
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
		builder.Services.AddDbContext<TaskContext>();
		builder.Services.AddScoped<TaskUpdateService>();
		builder.Services.AddSingleton<PeriodicUpdateService>();
		builder.Services.AddHostedService(provider => provider.GetRequiredService<PeriodicUpdateService>());
		
		//builder.Services.AddHostedService<PeriodicUpdateService>();
		

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
