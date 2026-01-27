using System.Security.Cryptography;
using System.Text;

namespace SecureDailyJournal.Services;

public class SecurityService
{
    private readonly DatabaseService _dbService;
    private const string PIN_KEY = "user_pin_hash";
    private const string PIN_SALT_KEY = "user_pin_salt";
    
    public SecurityService(DatabaseService dbService)
    {
        _dbService = dbService;
    }
    
    /// <summary>
    /// Checks if a PIN has been set for the application
    /// </summary>
    public async Task<bool> IsPinSetAsync()
    {
        var pinHash = await _dbService.GetSettingAsync(PIN_KEY);
        return !string.IsNullOrEmpty(pinHash);
    }
    
    /// <summary>
    /// Sets a new PIN for the application
    /// </summary>
    public async Task<bool> SetPinAsync(string pin)
    {
        if (string.IsNullOrWhiteSpace(pin) || pin.Length < 4)
            return false;
            
        // Generate a random salt
        var salt = GenerateSalt();
        var hash = HashPin(pin, salt);
        
        await _dbService.SaveSettingAsync(PIN_SALT_KEY, salt);
        await _dbService.SaveSettingAsync(PIN_KEY, hash);
        
        return true;
    }
    
    /// <summary>
    /// Verifies the provided PIN against the stored hash
    /// </summary>
    public async Task<bool> VerifyPinAsync(string pin)
    {
        if (string.IsNullOrWhiteSpace(pin))
            return false;
            
        var storedHash = await _dbService.GetSettingAsync(PIN_KEY);
        var storedSalt = await _dbService.GetSettingAsync(PIN_SALT_KEY);
        
        if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(storedSalt))
            return false;
            
        var hash = HashPin(pin, storedSalt);
        return hash == storedHash;
    }
    
    /// <summary>
    /// Changes the PIN to a new value after verifying the old PIN
    /// </summary>
    public async Task<bool> ChangePinAsync(string oldPin, string newPin)
    {
        if (!await VerifyPinAsync(oldPin))
            return false;
            
        return await SetPinAsync(newPin);
    }
    
    /// <summary>
    /// Resets the PIN (requires app reinstall or manual DB deletion in production)
    /// </summary>
    public async Task ResetPinAsync()
    {
        await _dbService.DeleteSettingAsync(PIN_KEY);
        await _dbService.DeleteSettingAsync(PIN_SALT_KEY);
    }
    
    private static string GenerateSalt()
    {
        var saltBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }
    
    private static string HashPin(string pin, string salt)
    {
        var combined = pin + salt;
        var bytes = Encoding.UTF8.GetBytes(combined);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
