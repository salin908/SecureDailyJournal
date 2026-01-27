using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureDailyJournal.Models;
using SecureDailyJournal.Services;
using System.Collections.ObjectModel;

namespace SecureDailyJournal.ViewModels;

public partial class JournalListViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;
    private int _currentPage = 1;
    private const int PageSize = 10;
    private bool _hasMoreItems = true;
    
    public JournalListViewModel(DatabaseService dbService)
    {
        _dbService = dbService;
    }
    
    public ObservableCollection<JournalEntry> Entries { get; } = new();
    
    [ObservableProperty]
    private string searchQuery = string.Empty;
    
    [ObservableProperty]
    private bool isRefreshing;
    
    [ObservableProperty]
    private bool isLoading;
    
    [ObservableProperty]
    private string currentFilter = "all";
    
    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        _currentPage = 1;
        _hasMoreItems = true;
        Entries.Clear();
        
        await LoadEntriesAsync();
        
        IsRefreshing = false;
    }
    
    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (IsLoading || !_hasMoreItems) return;
        
        _currentPage++;
        await LoadEntriesAsync();
    }
    
    [RelayCommand]
    private async Task SearchAsync()
    {
        _currentPage = 1;
        _hasMoreItems = true;
        Entries.Clear();
        
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            await LoadEntriesAsync();
        }
        else
        {
            IsLoading = true;
            var results = await _dbService.SearchEntriesAsync(SearchQuery);
            foreach (var entry in results)
            {
                Entries.Add(entry);
            }
            _hasMoreItems = false; // Search returns all results
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task FilterAsync(string filter)
    {
        CurrentFilter = filter;
        _currentPage = 1;
        _hasMoreItems = true;
        Entries.Clear();
        await LoadEntriesAsync();
    }
    
    [RelayCommand]
    private async Task FilterByMoodAsync(string mood)
    {
        CurrentFilter = mood;
        _currentPage = 1;
        Entries.Clear();
        
        IsLoading = true;
        var results = await _dbService.GetEntriesByMoodAsync(mood);
        foreach (var entry in results)
        {
            Entries.Add(entry);
        }
        _hasMoreItems = false;
        IsLoading = false;
    }
    
    [RelayCommand]
    private async Task OpenEntryAsync(JournalEntry entry)
    {
        // Navigate to journal page with the entry's date
        await Shell.Current.GoToAsync($"//main/journal?date={entry.EntryDateKey}");
    }
    
    private async Task LoadEntriesAsync()
    {
        if (IsLoading) return;
        
        IsLoading = true;
        
        var entries = await _dbService.GetEntriesPagedAsync(_currentPage, PageSize);
        
        if (entries.Count < PageSize)
        {
            _hasMoreItems = false;
        }
        
        foreach (var entry in entries)
        {
            Entries.Add(entry);
        }
        
        IsLoading = false;
    }
}
