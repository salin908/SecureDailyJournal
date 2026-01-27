using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureDailyJournal.Models;
using SecureDailyJournal.Services;
using SecureDailyJournal.Helpers;
using System.Collections.ObjectModel;

namespace SecureDailyJournal.ViewModels;

public partial class JournalViewModel : ObservableObject, IQueryAttributable
{
    private readonly DatabaseService _dbService;
    private JournalEntry? _currentEntry;
    
    public JournalViewModel(DatabaseService dbService)
    {
        _dbService = dbService;
        _ = InitializeAsync();
    }
    
    // Available moods for selection
    public List<MoodOption> AvailableMoods { get; } = new()
    {
        new("Happy", "😊"),
        new("Sad", "😢"),
        new("Anxious", "😰"),
        new("Calm", "😌"),
        new("Excited", "🤩"),
        new("Angry", "😠"),
        new("Neutral", "😐"),
        new("Grateful", "🙏"),
        new("Tired", "😴"),
        new("Energetic", "⚡")
    };
    
    public ObservableCollection<string> SelectedSecondaryMoods { get; } = new();
    public ObservableCollection<string> SelectedTags { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    
    // Common tags for quick selection
    public List<string> SuggestedTags { get; } = new()
    {
        "reflection", "gratitude", "goals", "memories", "dreams",
        "health", "work", "family", "travel", "creative"
    };

    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string content = string.Empty;
    [ObservableProperty] private string primaryMood = "Neutral";
    [ObservableProperty] private string selectedMoodEmoji = "😐";
    [ObservableProperty] private DateTime selectedDate = DateTime.Today;
    [ObservableProperty] private int currentStreak;
    [ObservableProperty] private bool isPreviewMode;
    [ObservableProperty] private bool hasExistingEntry;
    [ObservableProperty] private string newTag = string.Empty;
    [ObservableProperty] private Category? selectedCategory;
    [ObservableProperty] private int wordCount;
    [ObservableProperty] private string formattedDate = DateTime.Today.ToString("dddd, MMMM dd");
    [ObservableProperty] private int cursorPosition;
    [ObservableProperty] private int selectionLength;
    
    partial void OnContentChanged(string value)
    {
        WordCount = value?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
    }
    
    partial void OnSelectedDateChanged(DateTime value)
    {
        FormattedDate = value.ToString("dddd, MMMM dd");
        _ = LoadEntryForDateAsync();
    }
    
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("date", out var dateObj) && dateObj is string dateStr)
        {
            if (DateTime.TryParse(dateStr, out var date))
            {
                SelectedDate = date;
            }
        }
    }
    
    private async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        await LoadEntryForDateAsync();
        CurrentStreak = await _dbService.GetCurrentStreakAsync();
    }
    
    private async Task LoadCategoriesAsync()
    {
        var cats = await _dbService.GetCategoriesAsync();
        Categories.Clear();
        foreach (var cat in cats)
        {
            Categories.Add(cat);
        }
        SelectedCategory = Categories.FirstOrDefault();
    }
    
    private async Task LoadEntryForDateAsync()
    {
        var dateKey = SelectedDate.ToString("yyyy-MM-dd");
        _currentEntry = await _dbService.GetEntryByDateAsync(dateKey);
        
        if (_currentEntry != null)
        {
            Title = _currentEntry.Title;
            Content = _currentEntry.Content;
            PrimaryMood = _currentEntry.PrimaryMood;
            SelectedMoodEmoji = AvailableMoods.FirstOrDefault(m => m.Name == PrimaryMood)?.Emoji ?? "😐";
            HasExistingEntry = true;
            
            // Load secondary moods
            SelectedSecondaryMoods.Clear();
            if (!string.IsNullOrEmpty(_currentEntry.SecondaryMoods))
            {
                foreach (var mood in _currentEntry.SecondaryMoods.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    SelectedSecondaryMoods.Add(mood.Trim());
                }
            }
            
            // Load tags
            SelectedTags.Clear();
            if (!string.IsNullOrEmpty(_currentEntry.Tags))
            {
                foreach (var tag in _currentEntry.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    SelectedTags.Add(tag.Trim());
                }
            }
            
            // Load category
            SelectedCategory = Categories.FirstOrDefault(c => c.Name == _currentEntry.Category) ?? Categories.FirstOrDefault();
        }
        else
        {
            // Reset for new entry
            Title = string.Empty;
            Content = string.Empty;
            PrimaryMood = "Neutral";
            SelectedMoodEmoji = "😐";
            HasExistingEntry = false;
            SelectedSecondaryMoods.Clear();
            SelectedTags.Clear();
            SelectedCategory = Categories.FirstOrDefault();
        }
    }

    [RelayCommand]
    private void TogglePreview() => IsPreviewMode = !IsPreviewMode;
    
    [RelayCommand]
    private void SelectMood(MoodOption mood)
    {
        PrimaryMood = mood.Name;
        SelectedMoodEmoji = mood.Emoji;
    }
    
    [RelayCommand]
    private void ToggleSecondaryMood(string mood)
    {
        if (SelectedSecondaryMoods.Contains(mood))
            SelectedSecondaryMoods.Remove(mood);
        else if (SelectedSecondaryMoods.Count < 2) // Limit to 2 secondary moods
            SelectedSecondaryMoods.Add(mood);
    }

    [RelayCommand]
    private void ToggleTag(string tag)
    {
        if (SelectedTags.Contains(tag))
            SelectedTags.Remove(tag);
        else
            SelectedTags.Add(tag);
    }
    
    [RelayCommand]
    private void AddCustomTag()
    {
        if (!string.IsNullOrWhiteSpace(NewTag) && !SelectedTags.Contains(NewTag.Trim()))
        {
            SelectedTags.Add(NewTag.Trim().ToLower());
            NewTag = string.Empty;
        }
    }
    
    [RelayCommand]
    private void RemoveTag(string tag)
    {
        SelectedTags.Remove(tag);
    }

    [RelayCommand]
    private async Task SaveJournalAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            await Shell.Current.DisplayAlert("Missing Title", "Please add a title for your entry.", "OK");
            return;
        }
        
        var entry = new JournalEntry
        {
            Id = _currentEntry?.Id ?? 0,
            Title = Title,
            Content = Content,
            PrimaryMood = PrimaryMood,
            SecondaryMoods = string.Join(",", SelectedSecondaryMoods),
            Tags = string.Join(",", SelectedTags),
            Category = SelectedCategory?.Name ?? "Personal",
            EntryDateKey = SelectedDate.ToString("yyyy-MM-dd"),
            CreatedAt = _currentEntry?.CreatedAt ?? DateTime.Now
        };
        
        await _dbService.SaveEntryAsync(entry);
        _currentEntry = entry;
        HasExistingEntry = true;
        CurrentStreak = await _dbService.GetCurrentStreakAsync();
        
        await Shell.Current.DisplayAlert("Saved!", "Your journal entry has been saved.", "OK");
    }
    
    [RelayCommand]
    private async Task DeleteEntryAsync()
    {
        if (_currentEntry == null) return;
        
        var confirm = await Shell.Current.DisplayAlert(
            "Delete Entry", 
            "Are you sure you want to delete this entry? This cannot be undone.", 
            "Delete", 
            "Cancel");
        
        if (confirm)
        {
            await _dbService.DeleteEntryAsync(_currentEntry.Id);
            _currentEntry = null;
            
            // Reset the form
            Title = string.Empty;
            Content = string.Empty;
            PrimaryMood = "Neutral";
            SelectedMoodEmoji = "😐";
            HasExistingEntry = false;
            SelectedSecondaryMoods.Clear();
            SelectedTags.Clear();
            
            CurrentStreak = await _dbService.GetCurrentStreakAsync();
            await Shell.Current.DisplayAlert("Deleted", "Entry has been deleted.", "OK");
        }
    }
    
    [RelayCommand]
    private void GoToToday()
    {
        SelectedDate = DateTime.Today;
    }
    
    #region Markdown Formatting Commands
    
    [RelayCommand]
    private void ApplyBold()
    {
        Content = MarkdownHelper.ApplyBold(Content, CursorPosition, SelectionLength);
        CursorPosition = MarkdownHelper.GetNewCursorPosition(CursorPosition, SelectionLength, MarkdownFormat.Bold);
    }
    
    [RelayCommand]
    private void ApplyItalic()
    {
        Content = MarkdownHelper.ApplyItalic(Content, CursorPosition, SelectionLength);
        CursorPosition = MarkdownHelper.GetNewCursorPosition(CursorPosition, SelectionLength, MarkdownFormat.Italic);
    }
    
    [RelayCommand]
    private void ApplyHeading(string level)
    {
        if (int.TryParse(level, out var headingLevel) && headingLevel >= 1 && headingLevel <= 3)
        {
            Content = MarkdownHelper.ApplyHeading(Content, CursorPosition, headingLevel);
            var format = headingLevel switch
            {
                1 => MarkdownFormat.Heading1,
                2 => MarkdownFormat.Heading2,
                3 => MarkdownFormat.Heading3,
                _ => MarkdownFormat.Heading1
            };
            CursorPosition = MarkdownHelper.GetNewCursorPosition(CursorPosition, SelectionLength, format);
        }
    }
    
    [RelayCommand]
    private void ApplyList(string type)
    {
        var isOrdered = type?.ToLower() == "ordered";
        Content = MarkdownHelper.ApplyList(Content, CursorPosition, isOrdered);
        var format = isOrdered ? MarkdownFormat.OrderedList : MarkdownFormat.UnorderedList;
        CursorPosition = MarkdownHelper.GetNewCursorPosition(CursorPosition, SelectionLength, format);
    }
    
    [RelayCommand]
    private async Task InsertLinkAsync()
    {
        var linkText = "link text";
        if (SelectionLength > 0)
        {
            linkText = Content.Substring(CursorPosition, SelectionLength);
        }
        
        var url = await Shell.Current.DisplayPromptAsync(
            "Insert Link",
            "Enter the URL:",
            placeholder: "https://example.com");
        
        if (!string.IsNullOrWhiteSpace(url))
        {
            Content = MarkdownHelper.InsertLink(Content, CursorPosition, SelectionLength, linkText, url);
        }
    }
    
    #endregion
}

public record MoodOption(string Name, string Emoji);
