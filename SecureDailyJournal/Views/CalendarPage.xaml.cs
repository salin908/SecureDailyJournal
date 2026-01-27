using SecureDailyJournal.ViewModels;

namespace SecureDailyJournal.Views;

public partial class CalendarPage : ContentPage
{
    public CalendarPage(CalendarViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        
        // Initialize calendar data asynchronously
        _ = viewModel.InitializeAsync();
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CalendarViewModel vm)
        {
            _ = vm.RefreshDataCommand.ExecuteAsync(null);
        }
    }
}
