using SecureDailyJournal.ViewModels;

namespace SecureDailyJournal.Views;

public partial class JournalListPage : ContentPage
{
    public JournalListPage(JournalListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is JournalListViewModel vm)
        {
            _ = vm.RefreshCommand.ExecuteAsync(null);
        }
    }
}
