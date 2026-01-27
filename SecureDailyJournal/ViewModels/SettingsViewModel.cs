using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureDailyJournal.Services;

namespace SecureDailyJournal.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;
    private readonly SecurityService _securityService;
    private readonly PdfExportService _pdfService;
    
    public SettingsViewModel(DatabaseService dbService, SecurityService securityService, PdfExportService pdfService)
    {
        _dbService = dbService;
        _securityService = securityService;
        _pdfService = pdfService;
        
        ExportStartDate = DateTime.Today.AddMonths(-1);
        ExportEndDate = DateTime.Today;
        
        // Set dark mode based on current theme
        IsDarkMode = Application.Current?.RequestedTheme == AppTheme.Dark;
        
        StoragePath = Path.Combine(FileSystem.AppDataDirectory, "Journal.db3");
    }
    
    [ObservableProperty]
    private bool isDarkMode;
    
    [ObservableProperty]
    private DateTime exportStartDate;
    
    [ObservableProperty]
    private DateTime exportEndDate;
    
    [ObservableProperty]
    private string storagePath = "";
    
    partial void OnIsDarkModeChanged(bool value)
    {
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
        }
    }
    
    [RelayCommand]
    private async Task ChangePinAsync()
    {
        // Show dialog to get current and new PIN
        var currentPin = await Shell.Current.DisplayPromptAsync(
            "Change PIN", 
            "Enter your current PIN:",
            keyboard: Keyboard.Numeric,
            maxLength: 6);
        
        if (string.IsNullOrEmpty(currentPin)) return;
        
        var isValid = await _securityService.VerifyPinAsync(currentPin);
        if (!isValid)
        {
            await Shell.Current.DisplayAlert("Error", "Incorrect current PIN", "OK");
            return;
        }
        
        var newPin = await Shell.Current.DisplayPromptAsync(
            "Change PIN", 
            "Enter your new PIN (4-6 digits):",
            keyboard: Keyboard.Numeric,
            maxLength: 6);
        
        if (string.IsNullOrEmpty(newPin) || newPin.Length < 4)
        {
            await Shell.Current.DisplayAlert("Error", "PIN must be 4-6 digits", "OK");
            return;
        }
        
        var confirmPin = await Shell.Current.DisplayPromptAsync(
            "Change PIN", 
            "Confirm your new PIN:",
            keyboard: Keyboard.Numeric,
            maxLength: 6);
        
        if (newPin != confirmPin)
        {
            await Shell.Current.DisplayAlert("Error", "PINs do not match", "OK");
            return;
        }
        
        var success = await _securityService.ChangePinAsync(currentPin, newPin);
        if (success)
        {
            await Shell.Current.DisplayAlert("Success", "PIN changed successfully!", "OK");
        }
        else
        {
            await Shell.Current.DisplayAlert("Error", "Failed to change PIN", "OK");
        }
    }
    
    [RelayCommand]
    private async Task ExportPdfAsync()
    {
        try
        {
            var entries = await _dbService.GetEntriesByDateRangeAsync(ExportStartDate, ExportEndDate);
            
            if (entries.Count == 0)
            {
                await Shell.Current.DisplayAlert("No Entries", "No journal entries found in the selected date range.", "OK");
                return;
            }
            
            var filePath = await _pdfService.ExportToPdfAsync(entries, ExportStartDate, ExportEndDate);
            
            await Shell.Current.DisplayAlert("Export Complete", $"PDF saved to:\n{filePath}", "OK");
            
            // Try to open the file
            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(filePath)
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Export Failed", $"An error occurred: {ex.Message}", "OK");
        }
    }
}
