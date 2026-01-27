using QuestPDF.Fluent;
using QuestPDF.Helpers;
using SecureDailyJournal.Models;
using QuestContainer = QuestPDF.Infrastructure.IContainer;

namespace SecureDailyJournal.Services;

public class PdfExportService
{
    public PdfExportService()
    {
        // Set QuestPDF license (Community license is free)
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }
    
    public async Task<string> ExportToPdfAsync(List<JournalEntry> entries, DateTime startDate, DateTime endDate)
    {
        var fileName = $"Journal_{startDate:yyyyMMdd}_to_{endDate:yyyyMMdd}.pdf";
        var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
        
        await Task.Run(() =>
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11));
                    
                    page.Header().Element(ComposeHeader);
                    page.Content().Element(c => ComposeContent(c, entries));
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf(filePath);
        });
        
        return filePath;
    }
    
    private void ComposeHeader(QuestContainer container)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Secure Daily Journal")
                        .FontSize(24)
                        .Bold()
                        .FontColor("#6366F1");
                    
                    col.Item().Text($"Exported on {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(10)
                        .FontColor("#9CA3AF");
                });
            });
            
            column.Item().PaddingTop(10).LineHorizontal(1).LineColor("#E5E7EB");
        });
    }
    
    private void ComposeContent(QuestContainer container, List<JournalEntry> entries)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Spacing(20);
            
            foreach (var entry in entries.OrderByDescending(e => e.EntryDateKey))
            {
                column.Item().Element(c => ComposeEntry(c, entry));
            }
            
            if (!entries.Any())
            {
                column.Item().Text("No journal entries found for the selected date range.")
                    .FontSize(12)
                    .FontColor("#9CA3AF")
                    .Italic();
            }
        });
    }
    
    private void ComposeEntry(QuestContainer container, JournalEntry entry)
    {
        container.Border(1).BorderColor("#E5E7EB").Padding(15).Column(column =>
        {
            column.Spacing(8);
            
            // Header with date and mood
            column.Item().Row(row =>
            {
                row.RelativeItem().Text(entry.Title)
                    .FontSize(16)
                    .Bold()
                    .FontColor("#4338CA");
                
                row.ConstantItem(100).AlignRight().Text(GetMoodEmoji(entry.PrimaryMood))
                    .FontSize(20);
            });
            
            // Date
            column.Item().Text($"Date: {FormatDate(entry.EntryDateKey)}")
                .FontSize(10)
                .FontColor("#9CA3AF");
            
            // Content
            column.Item().PaddingTop(5).Text(entry.Content)
                .FontSize(11)
                .LineHeight(1.5f);
            
            // Tags (if any)
            if (!string.IsNullOrWhiteSpace(entry.Tags))
            {
                column.Item().PaddingTop(5).Row(row =>
                {
                    row.AutoItem().Text("Tags: ").FontSize(10);
                    row.RelativeItem().Text(entry.Tags)
                        .FontSize(9)
                        .FontColor("#6366F1");
                });
            }
            
            // Secondary moods (if any)
            if (!string.IsNullOrWhiteSpace(entry.SecondaryMoods))
            {
                column.Item().Text($"Also feeling: {entry.SecondaryMoods}")
                    .FontSize(9)
                    .FontColor("#9CA3AF")
                    .Italic();
            }
        });
    }
    
    private void ComposeFooter(QuestContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Page ").FontSize(9).FontColor("#9CA3AF");
            text.CurrentPageNumber().FontSize(9).FontColor("#9CA3AF");
            text.Span(" of ").FontSize(9).FontColor("#9CA3AF");
            text.TotalPages().FontSize(9).FontColor("#9CA3AF");
        });
    }
    
    private static string GetMoodEmoji(string? mood) => mood?.ToLower() switch
    {
        "happy" => "Happy",
        "sad" => "Sad",
        "anxious" => "Anxious",
        "calm" => "Calm",
        "excited" => "Excited",
        "angry" => "Angry",
        "neutral" => "Neutral",
        "grateful" => "Grateful",
        "tired" => "Tired",
        "energetic" => "Energetic",
        _ => "Entry"
    };
    
    private static string FormatDate(string dateKey)
    {
        if (DateTime.TryParse(dateKey, out var date))
        {
            return date.ToString("dddd, MMMM dd, yyyy");
        }
        return dateKey;
    }
}

