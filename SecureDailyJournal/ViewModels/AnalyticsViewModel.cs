using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureDailyJournal.Services;
using System.Collections.ObjectModel;

namespace SecureDailyJournal.ViewModels;

public partial class AnalyticsViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;
    
    public AnalyticsViewModel(DatabaseService dbService)
    {
        _dbService = dbService;
    }
    
    [ObservableProperty]
    private int totalEntries;
    
    [ObservableProperty]
    private int currentStreak;
    
    [ObservableProperty]
    private int longestStreak;
    
    [ObservableProperty]
    private int averageWordCount;
    
    public ObservableCollection<MoodStat> MoodStats { get; } = new();
    public ObservableCollection<TagStat> TopTags { get; } = new();
    public ObservableCollection<WeekdayStat> WeekdayStats { get; } = new();
    
    [RelayCommand]
    private async Task RefreshAsync()
    {
        var entries = await _dbService.GetAllEntriesAsync();
        
        // Basic stats
        TotalEntries = entries.Count;
        CurrentStreak = await _dbService.GetCurrentStreakAsync();
        LongestStreak = await _dbService.GetLongestStreakAsync();
        
        // Average word count
        if (entries.Count > 0)
        {
            var totalWords = entries.Sum(e => e.Content?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length ?? 0);
            AverageWordCount = totalWords / entries.Count;
        }
        
        // Mood distribution
        MoodStats.Clear();
        var moodGroups = entries.GroupBy(e => e.PrimaryMood)
            .Select(g => new { Mood = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(5);
        
        var maxMoodCount = moodGroups.Any() ? moodGroups.Max(g => g.Count) : 1;
        foreach (var group in moodGroups)
        {
            MoodStats.Add(new MoodStat
            {
                MoodEmoji = GetMoodEmoji(group.Mood),
                MoodName = group.Mood,
                Count = group.Count,
                BarWidth = (double)group.Count / maxMoodCount * 200
            });
        }
        
        // Top tags
        TopTags.Clear();
        var allTags = entries.SelectMany(e => e.Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [])
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .GroupBy(t => t)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(8);
        
        foreach (var tag in allTags)
        {
            TopTags.Add(new TagStat { Name = tag.Name, Count = tag.Count });
        }
        
        // Weekday distribution
        WeekdayStats.Clear();
        var weekdayGroups = entries.GroupBy(e => 
        {
            if (DateTime.TryParse(e.EntryDateKey, out var date))
                return date.DayOfWeek;
            return DayOfWeek.Sunday;
        })
        .ToDictionary(g => g.Key, g => g.Count());
        
        var maxDayCount = weekdayGroups.Any() ? weekdayGroups.Values.Max() : 1;
        var days = new[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, 
                          DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
        
        foreach (var day in days)
        {
            var count = weekdayGroups.GetValueOrDefault(day, 0);
            WeekdayStats.Add(new WeekdayStat
            {
                DayName = day.ToString()[..3],
                Count = count,
                BarHeight = Math.Max(10, (double)count / maxDayCount * 80)
            });
        }
    }
    
    private static string GetMoodEmoji(string mood) => mood?.ToLower() switch
    {
        "happy" => "😊",
        "sad" => "😢",
        "anxious" => "😰",
        "calm" => "😌",
        "excited" => "🤩",
        "angry" => "😠",
        "neutral" => "😐",
        "grateful" => "🙏",
        "tired" => "😴",
        "energetic" => "⚡",
        _ => "📝"
    };
}

public class MoodStat
{
    public string MoodEmoji { get; set; } = "";
    public string MoodName { get; set; } = "";
    public int Count { get; set; }
    public double BarWidth { get; set; }
}

public class TagStat
{
    public string Name { get; set; } = "";
    public int Count { get; set; }
}

public class WeekdayStat
{
    public string DayName { get; set; } = "";
    public int Count { get; set; }
    public double BarHeight { get; set; }
}
