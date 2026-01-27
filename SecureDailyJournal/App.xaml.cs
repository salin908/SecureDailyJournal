namespace SecureDailyJournal;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        // Start with AppShell which handles navigation
        MainPage = new AppShell();
    }
}