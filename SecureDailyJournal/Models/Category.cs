using SQLite;

namespace SecureDailyJournal.Models;

public class Category
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Color { get; set; } = "#6366F1"; // Default indigo color
    
    public string Icon { get; set; } = "📝";
}
