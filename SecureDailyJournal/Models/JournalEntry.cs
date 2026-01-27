using SQLite;

namespace SecureDailyJournal.Models;

public class JournalEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    public string PrimaryMood { get; set; } = "Neutral";
    public string SecondaryMoods { get; set; } = string.Empty;
    
    public string Tags { get; set; } = string.Empty;
    
    public string Category { get; set; } = "Personal";

    [Indexed(Unique = true)]
    public string EntryDateKey { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
    
    // Computed property for word count
    [Ignore]
    public int WordCount => Content?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
}
