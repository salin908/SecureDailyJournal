using SecureDailyJournal.ViewModels;

namespace SecureDailyJournal.Views;

public partial class MainPage : ContentPage
{
    public MainPage(JournalViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}