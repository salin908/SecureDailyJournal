using CommunityToolkit.Maui;
using SecureDailyJournal.Services;
using SecureDailyJournal.ViewModels;
using SecureDailyJournal.Views;

namespace SecureDailyJournal;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register Services
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<SecurityService>();
        builder.Services.AddSingleton<PdfExportService>();
        
        // Register ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<JournalViewModel>();
        builder.Services.AddTransient<CalendarViewModel>();
        builder.Services.AddTransient<JournalListViewModel>();
        builder.Services.AddTransient<AnalyticsViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        
        // Register Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<CalendarPage>();
        builder.Services.AddTransient<JournalListPage>();
        builder.Services.AddTransient<AnalyticsPage>();
        builder.Services.AddTransient<SettingsPage>();

        return builder.Build();
    }
}
