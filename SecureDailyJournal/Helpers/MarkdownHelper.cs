namespace SecureDailyJournal.Helpers;

/// <summary>
/// Utility class for markdown formatting operations
/// </summary>
public static class MarkdownHelper
{
    /// <summary>
    /// Wraps the selected text with markdown bold syntax
    /// </summary>
    public static string ApplyBold(string text, int cursorPosition, int selectionLength)
    {
        if (selectionLength > 0)
        {
            // Wrap selected text
            var before = text.Substring(0, cursorPosition);
            var selected = text.Substring(cursorPosition, selectionLength);
            var after = text.Substring(cursorPosition + selectionLength);
            return $"{before}**{selected}**{after}";
        }
        else
        {
            // Insert bold placeholder
            var before = text.Substring(0, cursorPosition);
            var after = text.Substring(cursorPosition);
            return $"{before}**bold text**{after}";
        }
    }

    /// <summary>
    /// Wraps the selected text with markdown italic syntax
    /// </summary>
    public static string ApplyItalic(string text, int cursorPosition, int selectionLength)
    {
        if (selectionLength > 0)
        {
            var before = text.Substring(0, cursorPosition);
            var selected = text.Substring(cursorPosition, selectionLength);
            var after = text.Substring(cursorPosition + selectionLength);
            return $"{before}*{selected}*{after}";
        }
        else
        {
            var before = text.Substring(0, cursorPosition);
            var after = text.Substring(cursorPosition);
            return $"{before}*italic text*{after}";
        }
    }

    /// <summary>
    /// Applies heading markdown syntax to the current line
    /// </summary>
    public static string ApplyHeading(string text, int cursorPosition, int level)
    {
        var prefix = new string('#', level) + " ";
        
        // Find the start of the current line
        var lineStart = text.LastIndexOf('\n', Math.Max(0, cursorPosition - 1)) + 1;
        
        // Check if line already has heading
        var currentLine = text.Substring(lineStart, cursorPosition - lineStart);
        if (currentLine.TrimStart().StartsWith("#"))
        {
            // Remove existing heading
            var hashCount = currentLine.TrimStart().TakeWhile(c => c == '#').Count();
            var afterHash = currentLine.TrimStart().Substring(hashCount).TrimStart();
            var before = text.Substring(0, lineStart);
            var after = text.Substring(cursorPosition);
            return $"{before}{prefix}{afterHash}{after}";
        }
        else
        {
            // Add heading
            var before = text.Substring(0, lineStart);
            var after = text.Substring(lineStart);
            return $"{before}{prefix}{after}";
        }
    }

    /// <summary>
    /// Applies list markdown syntax to the current line
    /// </summary>
    public static string ApplyList(string text, int cursorPosition, bool ordered)
    {
        var prefix = ordered ? "1. " : "- ";
        
        // Find the start of the current line
        var lineStart = text.LastIndexOf('\n', Math.Max(0, cursorPosition - 1)) + 1;
        
        var before = text.Substring(0, lineStart);
        var after = text.Substring(lineStart);
        
        // Check if already a list item
        var currentLine = after.Length > 0 ? after.Substring(0, Math.Min(3, after.IndexOf('\n') >= 0 ? after.IndexOf('\n') : after.Length)) : "";
        if (currentLine.TrimStart().StartsWith("- ") || currentLine.TrimStart().StartsWith("1. "))
        {
            // Already a list, don't add another prefix
            return text;
        }
        
        return $"{before}{prefix}{after}";
    }

    /// <summary>
    /// Inserts a markdown link at the cursor position
    /// </summary>
    public static string InsertLink(string text, int cursorPosition, int selectionLength, string linkText, string url)
    {
        if (selectionLength > 0)
        {
            // Use selected text as link text
            var before = text.Substring(0, cursorPosition);
            var selected = text.Substring(cursorPosition, selectionLength);
            var after = text.Substring(cursorPosition + selectionLength);
            return $"{before}[{selected}]({url}){after}";
        }
        else
        {
            // Insert link with provided text
            var before = text.Substring(0, cursorPosition);
            var after = text.Substring(cursorPosition);
            return $"{before}[{linkText}]({url}){after}";
        }
    }

    /// <summary>
    /// Renders basic markdown formatting for preview (simple implementation)
    /// </summary>
    public static string RenderMarkdownPreview(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
            return string.Empty;

        var result = markdown;

        // Note: This is a simplified renderer for display purposes
        // For full markdown support, consider using a library like Markdig
        
        // Bold: **text** (keep as is for now, MAUI Label doesn't support rich text natively)
        // Italic: *text* (keep as is)
        // Headings: # H1, ## H2, etc. (keep as is)
        // Lists: - item or 1. item (keep as is)
        // Links: [text](url) (keep as is)
        
        // For now, just return the markdown as-is
        // In a future enhancement, we could use a WebView with HTML rendering
        return result;
    }

    /// <summary>
    /// Gets the new cursor position after applying formatting
    /// </summary>
    public static int GetNewCursorPosition(int originalPosition, int selectionLength, MarkdownFormat format)
    {
        return format switch
        {
            MarkdownFormat.Bold => selectionLength > 0 ? originalPosition + 2 : originalPosition + 2,
            MarkdownFormat.Italic => selectionLength > 0 ? originalPosition + 1 : originalPosition + 1,
            MarkdownFormat.Heading1 => originalPosition + 2,
            MarkdownFormat.Heading2 => originalPosition + 3,
            MarkdownFormat.Heading3 => originalPosition + 4,
            MarkdownFormat.UnorderedList => originalPosition + 2,
            MarkdownFormat.OrderedList => originalPosition + 3,
            _ => originalPosition
        };
    }
}

/// <summary>
/// Markdown formatting types
/// </summary>
public enum MarkdownFormat
{
    Bold,
    Italic,
    Heading1,
    Heading2,
    Heading3,
    UnorderedList,
    OrderedList,
    Link
}
