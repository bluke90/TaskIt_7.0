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
        builder.RegisterServices();
        builder.RegisterPages();
		
        //builder.Services.AddHostedService<PeriodicUpdateService>();


#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	public static MauiAppBuilder RegisterServices(this MauiAppBuilder builder) {
        // Scoped
        builder.Services.AddScoped<TaskUpdateService>();

        // Singletons
        builder.Services.AddSingleton<PeriodicUpdateService>(); // Eventually get away from using BackgroundService from Extensions.Hosting
        builder.Services.AddSingleton<AppShell>();

		return builder;
    }

	public static MauiAppBuilder RegisterPages(this MauiAppBuilder builder) {
		// Singleton
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<CalendarViewPage>();
        builder.Services.AddSingleton<NotesPage>();
        builder.Services.AddSingleton<DailySchedulePage>();

		//Transient
        builder.Services.AddTransient<CalendarDayPage>();
        builder.Services.AddTransient<CompletedTaskPage>();
        builder.Services.AddTransient<EditTaskPage>();
        builder.Services.AddTransient<NewNotesPage>();
        builder.Services.AddTransient<TaskViewPage>();
        builder.Services.AddTransient<NewTaskPage>();


        return builder;
    }


}
