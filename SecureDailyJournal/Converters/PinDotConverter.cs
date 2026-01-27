using System.Globalization;

namespace SecureDailyJournal.Converters;

/// <summary>
/// Converts PIN length to dot color for the PIN entry display
/// Parameter should be the index (0-5) of the dot
/// Returns filled color if PIN length > index, empty color otherwise
/// </summary>
public class PinDotConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int pinLength || parameter is not string indexStr || !int.TryParse(indexStr, out int index))
        {
            return Color.FromArgb("#E2E8F0"); // Empty dot color
        }
        
        // Check if this dot should be filled
        if (pinLength > index)
        {
            return Color.FromArgb("#6366F1"); // Filled dot color (indigo)
        }
        
        return Color.FromArgb("#E2E8F0"); // Empty dot color
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
