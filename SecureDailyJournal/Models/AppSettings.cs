using SQLite;

namespace SecureDailyJournal.Models;

public class AppSettings
{
    [PrimaryKey]
    public string Key { get; set; } = string.Empty;
    
    public string Value { get; set; } = string.Empty;
}
