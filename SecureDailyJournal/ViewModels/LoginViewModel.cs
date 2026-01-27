using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureDailyJournal.Services;

namespace SecureDailyJournal.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly SecurityService _securityService;
    
    public LoginViewModel(SecurityService securityService)
    {
        _securityService = securityService;
        _ = InitializeAsync();
    }
    
    [ObservableProperty] 
    private string pin = string.Empty;
    
    [ObservableProperty] 
    private string confirmPin = string.Empty;
    
    [ObservableProperty] 
    private bool isFirstTimeSetup;
    
    [ObservableProperty] 
    private string errorMessage = string.Empty;
    
    [ObservableProperty] 
    private bool showError;
    
    [ObservableProperty]
    private string headerText = "Enter PIN";
    
    [ObservableProperty]
    private bool showConfirmPin;

    private async Task InitializeAsync()
    {
        IsFirstTimeSetup = !await _securityService.IsPinSetAsync();
        if (IsFirstTimeSetup)
        {
            HeaderText = "Create Your PIN";
            ShowConfirmPin = true;
        }
        else
        {
            HeaderText = "Enter PIN to Unlock";
            ShowConfirmPin = false;
        }
    }
    
    [RelayCommand]
    private void AddDigit(string digit)
    {
        if (ShowConfirmPin && Pin.Length >= 6 && ConfirmPin.Length < 6)
        {
            ConfirmPin += digit;
        }
        else if (Pin.Length < 6)
        {
            Pin += digit;
        }
        ShowError = false;
    }
    
    [RelayCommand]
    private void Backspace()
    {
        if (ShowConfirmPin && ConfirmPin.Length > 0)
        {
            ConfirmPin = ConfirmPin[..^1];
        }
        else if (Pin.Length > 0)
        {
            Pin = Pin[..^1];
        }
    }
    
    [RelayCommand]
    private void Clear()
    {
        Pin = string.Empty;
        ConfirmPin = string.Empty;
        ShowError = false;
    }
    
    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (IsFirstTimeSetup)
        {
            await CreatePinAsync();
        }
        else
        {
            await VerifyPinAsync();
        }
    }
    
    private async Task CreatePinAsync()
    {
        if (Pin.Length < 4)
        {
            ErrorMessage = "PIN must be at least 4 digits";
            ShowError = true;
            return;
        }
        
        if (Pin != ConfirmPin)
        {
            ErrorMessage = "PINs do not match";
            ShowError = true;
            ConfirmPin = string.Empty;
            return;
        }
        
        var success = await _securityService.SetPinAsync(Pin);
        if (success)
        {
            await NavigateToMainAsync();
        }
        else
        {
            ErrorMessage = "Failed to create PIN";
            ShowError = true;
        }
    }
    
    private async Task VerifyPinAsync()
    {
        if (string.IsNullOrEmpty(Pin))
        {
            ErrorMessage = "Please enter your PIN";
            ShowError = true;
            return;
        }
        
        var isValid = await _securityService.VerifyPinAsync(Pin);
        if (isValid)
        {
            await NavigateToMainAsync();
        }
        else
        {
            ErrorMessage = "Incorrect PIN";
            ShowError = true;
            Pin = string.Empty;
        }
    }
    
    private static async Task NavigateToMainAsync()
    {
        await Shell.Current.GoToAsync("//main");
    }
}
