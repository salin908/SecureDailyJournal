using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureDailyJournal.Models;
using SecureDailyJournal.Services;
using System.Collections.ObjectModel;

namespace SecureDailyJournal.ViewModels;

public partial class CalendarViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;
    
    public CalendarViewModel(DatabaseService dbService)
    {
        _dbService = dbService;
        _currentMonth = DateTime.Today;
        _ = RefreshDataAsync();
    }
    
    public ObservableCollection<CalendarDay> CalendarDays { get; } = new();
    
    [ObservableProperty]
    private DateTime _currentMonth;
    
    [ObservableProperty]
    private string currentMonthYear = DateTime.Today.ToString("MMMM yyyy");
    
    [ObservableProperty]
    private int currentStreak;
    
    [ObservableProperty]
    private int longestStreak;
    
    [ObservableProperty]
    private JournalEntry? selectedEntry;
    
    [ObservableProperty]
    private bool hasSelectedEntry;
    
    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        await LoadCalendarDaysAsync();
        CurrentStreak = await _dbService.GetCurrentStreakAsync();
        LongestStreak = await _dbService.GetLongestStreakAsync();
    }
    
    [RelayCommand]
    private async Task PreviousMonthAsync()
    {
        CurrentMonth = CurrentMonth.AddMonths(-1);
        CurrentMonthYear = CurrentMonth.ToString("MMMM yyyy");
        await LoadCalendarDaysAsync();
    }
    
    [RelayCommand]
    private async Task NextMonthAsync()
    {
        CurrentMonth = CurrentMonth.AddMonths(1);
        CurrentMonthYear = CurrentMonth.ToString("MMMM yyyy");
        await LoadCalendarDaysAsync();
    }
    
    [RelayCommand]
    private async Task SelectDateAsync(DateTime? date)
    {
        if (date == null) return;
        
        var dateKey = date.Value.ToString("yyyy-MM-dd");
        SelectedEntry = await _dbService.GetEntryByDateAsync(dateKey);
        HasSelectedEntry = SelectedEntry != null;
        
        // Update calendar to show selection
        foreach (var day in CalendarDays)
        {
            day.IsSelected = day.Date?.Date == date.Value.Date;
        }
    }
    
    [RelayCommand]
    private async Task EditEntryAsync()
    {
        if (SelectedEntry != null)
        {
            // Navigate to journal page with the selected date
            await Shell.Current.GoToAsync($"//main/journal?date={SelectedEntry.EntryDateKey}");
        }
    }
    
    private async Task LoadCalendarDaysAsync()
    {
        CalendarDays.Clear();
        
        var firstDay = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(CurrentMonth.Year, CurrentMonth.Month);
        var startDayOfWeek = (int)firstDay.DayOfWeek;
        
        // Get entries for this month
        var entriesThisMonth = await _dbService.GetDatesWithEntriesAsync(CurrentMonth.Year, CurrentMonth.Month);
        var entryDates = new HashSet<string>(entriesThisMonth);
        
        // Add empty days for padding
        for (int i = 0; i < startDayOfWeek; i++)
        {
            CalendarDays.Add(new CalendarDay { DayNumber = "", IsCurrentMonth = false });
        }
        
        // Add days of the month
        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(CurrentMonth.Year, CurrentMonth.Month, day);
            var dateKey = date.ToString("yyyy-MM-dd");
            var isToday = date.Date == DateTime.Today;
            var hasEntry = entryDates.Contains(dateKey);
            
            CalendarDays.Add(new CalendarDay
            {
                Date = date,
                DayNumber = day.ToString(),
                IsCurrentMonth = true,
                IsToday = isToday,
                HasEntry = hasEntry,
                BackgroundColor = isToday ? Color.FromArgb("#6366F1") : Color.FromArgb("#00000000"),
                TextColor = isToday ? Colors.White : (Application.Current?.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#F1F5F9") : Color.FromArgb("#1E293B")),
                FontAttributes = isToday ? FontAttributes.Bold : FontAttributes.None
            });
        }
    }
}

public partial class CalendarDay : ObservableObject
{
    public DateTime? Date { get; set; }
    public string DayNumber { get; set; } = "";
    public bool IsCurrentMonth { get; set; }
    public bool IsToday { get; set; }
    public bool HasEntry { get; set; }
    
    [ObservableProperty]
    private bool isSelected;
    
    [ObservableProperty]
    private Color backgroundColor = Colors.Transparent;
    
    [ObservableProperty]
    private Color textColor = Colors.Black;
    
    public FontAttributes FontAttributes { get; set; } = FontAttributes.None;
}
