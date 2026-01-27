using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureDailyJournal.Models;
using SecureDailyJournal.Services;

namespace SecureDailyJournal.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;

    public MainViewModel()
    {
        _dbService = new DatabaseService();
    }

    [ObservableProperty] string title;
    [ObservableProperty] string content;
    [ObservableProperty] string selectedMood;

    [RelayCommand]
    async Task SaveJournal()
    {
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(SelectedMood))
        {
            await Shell.Current.DisplayAlert("Missing Info", "Please add a title and select a mood.", "OK");
            return;
        }

        var entry = new JournalEntry
        {
            Title = Title,
            Content = Content,
            PrimaryMood = SelectedMood,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        try
        {
            await _dbService.SaveEntryAsync(entry);
            await Shell.Current.DisplayAlert("Success", "Daily entry saved!", "OK");
            // Clear fields after save
            Title = string.Empty;
            Content = string.Empty;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}