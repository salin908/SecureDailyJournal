using SQLite;
using SecureDailyJournal.Models;

namespace SecureDailyJournal.Services;

public class DatabaseService
{
    SQLiteAsyncConnection? _db;

    async Task Init()
    {
        if (_db is not null) return;
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "Journal.db3");
        _db = new SQLiteAsyncConnection(dbPath);
        await _db.CreateTableAsync<JournalEntry>();
        await _db.CreateTableAsync<AppSettings>();
        await _db.CreateTableAsync<Category>();
        
        // Seed default categories if none exist
        var categories = await _db.Table<Category>().CountAsync();
        if (categories == 0)
        {
            await _db.InsertAllAsync(new List<Category>
            {
                new() { Name = "Personal", Color = "#6366F1", Icon = "👤" },
                new() { Name = "Work", Color = "#F59E0B", Icon = "💼" },
                new() { Name = "Health", Color = "#10B981", Icon = "🏃" },
                new() { Name = "Travel", Color = "#3B82F6", Icon = "✈️" },
                new() { Name = "Ideas", Color = "#EC4899", Icon = "💡" }
            });
        }
    }

    #region Journal Entry Methods
    
    public async Task<int> SaveEntryAsync(JournalEntry entry)
    {
        await Init();
        entry.UpdatedAt = DateTime.Now;
        
        // Check if entry already exists for this date
        var existing = await GetEntryByDateAsync(entry.EntryDateKey);
        if (existing != null)
        {
            entry.Id = existing.Id;
            entry.CreatedAt = existing.CreatedAt;
            return await _db!.UpdateAsync(entry);
        }
        
        entry.CreatedAt = DateTime.Now;
        return await _db!.InsertAsync(entry);
    }

    public async Task<JournalEntry?> GetEntryByDateAsync(string dateKey)
    {
        await Init();
        return await _db!.Table<JournalEntry>().Where(x => x.EntryDateKey == dateKey).FirstOrDefaultAsync();
    }

    public async Task<List<JournalEntry>> GetAllEntriesAsync()
    {
        await Init();
        return await _db!.Table<JournalEntry>().OrderByDescending(x => x.CreatedAt).ToListAsync();
    }
    
    public async Task<List<JournalEntry>> GetEntriesPagedAsync(int page, int pageSize = 10)
    {
        await Init();
        return await _db!.Table<JournalEntry>()
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<int> GetTotalEntriesCountAsync()
    {
        await Init();
        return await _db!.Table<JournalEntry>().CountAsync();
    }

    public async Task<List<JournalEntry>> GetEntriesByDateRangeAsync(DateTime start, DateTime end)
    {
        await Init();
        var startKey = start.ToString("yyyy-MM-dd");
        var endKey = end.ToString("yyyy-MM-dd");
        return await _db!.Table<JournalEntry>()
            .Where(x => string.Compare(x.EntryDateKey, startKey) >= 0 && string.Compare(x.EntryDateKey, endKey) <= 0)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<JournalEntry>> SearchEntriesAsync(string query)
    {
        await Init();
        var lowerQuery = query.ToLower();
        return await _db!.Table<JournalEntry>()
            .Where(v => v.Title.ToLower().Contains(lowerQuery) || 
                       v.Content.ToLower().Contains(lowerQuery) ||
                       v.Tags.ToLower().Contains(lowerQuery))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<List<JournalEntry>> GetEntriesByMoodAsync(string mood)
    {
        await Init();
        return await _db!.Table<JournalEntry>()
            .Where(x => x.PrimaryMood == mood || x.SecondaryMoods.Contains(mood))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<List<JournalEntry>> GetEntriesByTagAsync(string tag)
    {
        await Init();
        return await _db!.Table<JournalEntry>()
            .Where(x => x.Tags.Contains(tag))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> DeleteEntryAsync(int id)
    {
        await Init();
        return await _db!.DeleteAsync<JournalEntry>(id);
    }

    public async Task<int> GetCurrentStreakAsync()
    {
        await Init();
        var entries = await _db!.Table<JournalEntry>().OrderByDescending(x => x.EntryDateKey).ToListAsync();
        if (entries.Count == 0) return 0;

        int streak = 0;
        DateTime expectedDate = DateTime.Today;

        foreach (var entry in entries)
        {
            var entryDate = DateTime.ParseExact(entry.EntryDateKey, "yyyy-MM-dd", null);
            if (entryDate.Date == expectedDate)
            {
                streak++;
                expectedDate = expectedDate.AddDays(-1);
            }
            else if (entryDate.Date < expectedDate) 
            {
                // If there's no entry for today but there is for yesterday, start counting from yesterday
                if (streak == 0 && entryDate.Date == DateTime.Today.AddDays(-1))
                {
                    streak = 1;
                    expectedDate = entryDate.Date.AddDays(-1);
                }
                else break;
            }
        }
        return streak;
    }
    
    public async Task<int> GetLongestStreakAsync()
    {
        await Init();
        var entries = await _db!.Table<JournalEntry>().OrderBy(x => x.EntryDateKey).ToListAsync();
        if (entries.Count == 0) return 0;

        int longestStreak = 1;
        int currentStreak = 1;
        
        for (int i = 1; i < entries.Count; i++)
        {
            var prevDate = DateTime.ParseExact(entries[i - 1].EntryDateKey, "yyyy-MM-dd", null);
            var currDate = DateTime.ParseExact(entries[i].EntryDateKey, "yyyy-MM-dd", null);
            
            if ((currDate - prevDate).Days == 1)
            {
                currentStreak++;
                longestStreak = Math.Max(longestStreak, currentStreak);
            }
            else
            {
                currentStreak = 1;
            }
        }
        
        return longestStreak;
    }
    
    public async Task<List<string>> GetDatesWithEntriesAsync(int year, int month)
    {
        await Init();
        var startKey = $"{year:D4}-{month:D2}-01";
        var endKey = $"{year:D4}-{month:D2}-31";
        
        var entries = await _db!.Table<JournalEntry>()
            .Where(x => string.Compare(x.EntryDateKey, startKey) >= 0 && string.Compare(x.EntryDateKey, endKey) <= 0)
            .ToListAsync();
            
        return entries.Select(e => e.EntryDateKey).ToList();
    }
    
    #endregion
    
    #region Category Methods
    
    public async Task<List<Category>> GetCategoriesAsync()
    {
        await Init();
        return await _db!.Table<Category>().ToListAsync();
    }
    
    public async Task<int> SaveCategoryAsync(Category category)
    {
        await Init();
        if (category.Id != 0)
            return await _db!.UpdateAsync(category);
        return await _db!.InsertAsync(category);
    }
    
    #endregion

    #region Settings Methods
    
    public async Task<string?> GetSettingAsync(string key)
    {
        await Init();
        var setting = await _db!.Table<AppSettings>().Where(x => x.Key == key).FirstOrDefaultAsync();
        return setting?.Value;
    }
    
    public async Task SaveSettingAsync(string key, string value)
    {
        await Init();
        var existing = await _db!.Table<AppSettings>().Where(x => x.Key == key).FirstOrDefaultAsync();
        if (existing != null)
        {
            existing.Value = value;
            await _db!.UpdateAsync(existing);
        }
        else
        {
            await _db!.InsertAsync(new AppSettings { Key = key, Value = value });
        }
    }
    
    public async Task DeleteSettingAsync(string key)
    {
        await Init();
        await _db!.Table<AppSettings>().DeleteAsync(x => x.Key == key);
    }
    
    #endregion
}
