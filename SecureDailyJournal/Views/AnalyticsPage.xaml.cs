using SecureDailyJournal.ViewModels;

namespace SecureDailyJournal.Views;

public partial class AnalyticsPage : ContentPage
{
    public AnalyticsPage(AnalyticsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AnalyticsViewModel vm)
        {
            _ = vm.RefreshCommand.ExecuteAsync(null);
        }
    }
}
