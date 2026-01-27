# How to Run SecureDailyJournal

## ✅ Quick Run (Recommended)

After building the project, run this command from the project directory:

```powershell
Add-AppxPackage -Register ".\bin\Debug\net9.0-windows10.0.19041.0\win10-x64\AppxManifest.xml" -ForceApplicationShutdown -ForceUpdateFromAnyVersion
```

Then launch the app:

```powershell
Start-Process "shell:AppsFolder\$(Get-AppxPackage | Where-Object {$_.Name -like '*SecureDailyJournal*'} | Select-Object -ExpandProperty PackageFamilyName)!App"
```

## 📝 Step-by-Step Instructions

### 1. Build the Project

From VS Code terminal or PowerShell:

```powershell
cd c:\Users\sures\OneDrive\Desktop\SecureDailyJournal\SecureDailyJournal
dotnet build -f net9.0-windows10.0.19041.0
```

### 2. Register the App Package

This step is required for MAUI Windows apps (MSIX packages):

```powershell
Add-AppxPackage -Register ".\bin\Debug\net9.0-windows10.0.19041.0\win10-x64\AppxManifest.xml" -ForceApplicationShutdown -ForceUpdateFromAnyVersion
```

### 3. Launch the App

```powershell
Start-Process "shell:AppsFolder\$(Get-AppxPackage | Where-Object {$_.Name -like '*SecureDailyJournal*'} | Select-Object -ExpandProperty PackageFamilyName)!App"
```

Or simply search for "SecureDailyJournal" in the Windows Start Menu.

## 🔄 After Making Code Changes

Every time you modify the code:

1. **Build**: `dotnet build -f net9.0-windows10.0.19041.0`
2. **Re-register**: `Add-AppxPackage -Register ".\bin\Debug\net9.0-windows10.0.19041.0\win10-x64\AppxManifest.xml" -ForceApplicationShutdown -ForceUpdateFromAnyVersion`
3. **Launch**: Use the Start Menu or the PowerShell command above

## 🚀 One-Line Run Script

Save this as `run.ps1` in the project directory:

```powershell
dotnet build -f net9.0-windows10.0.19041.0
if ($LASTEXITCODE -eq 0) {
    Add-AppxPackage -Register ".\bin\Debug\net9.0-windows10.0.19041.0\win10-x64\AppxManifest.xml" -ForceApplicationShutdown -ForceUpdateFromAnyVersion
    Start-Process "shell:AppsFolder\$(Get-AppxPackage | Where-Object {$_.Name -like '*SecureDailyJournal*'} | Select-Object -ExpandProperty PackageFamilyName)!App"
}
```

Then just run: `.\run.ps1`

## ℹ️ Why This Is Needed

.NET MAUI Windows apps are packaged as MSIX applications, which require registration with Windows before they can run. This is different from traditional .exe applications. The `Add-AppxPackage -Register` command tells Windows about your app without creating a full installer package.
