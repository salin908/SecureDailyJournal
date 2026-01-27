# Running SecureDailyJournal from VS Code

This guide explains how to build and run the SecureDailyJournal .NET MAUI application from Visual Studio Code using the `dotnet` CLI.

## Prerequisites

1. **Install .NET 9 SDK** (if not already installed)
   - Download from: https://dotnet.microsoft.com/download/dotnet/9.0
   
2. **Install .NET MAUI Workload**
   ```powershell
   dotnet workload install maui
   ```

3. **VS Code Extensions** (recommended)
   - C# Dev Kit
   - .NET MAUI Extension (optional)

## Building the Project

Navigate to the project directory:
```powershell
cd c:\Users\sures\OneDrive\Desktop\SecureDailyJournal\SecureDailyJournal
```

### Build for Windows

```powershell
dotnet build -f net9.0-windows10.0.19041.0
```

### Build for specific configuration

```powershell
# Debug build
dotnet build -f net9.0-windows10.0.19041.0 -c Debug

# Release build
dotnet build -f net9.0-windows10.0.19041.0 -c Release
```

## Running the Project

### Run on Windows

```powershell
dotnet run -f net9.0-windows10.0.19041.0
```

### Run with specific configuration

```powershell
# Run in Debug mode
dotnet run -f net9.0-windows10.0.19041.0 -c Debug

# Run in Release mode
dotnet run -f net9.0-windows10.0.19041.0 -c Release
```

## Cleaning the Project

If you encounter build issues, clean the project first:

```powershell
# Clean build artifacts
dotnet clean

# Then rebuild
dotnet build -f net9.0-windows10.0.19041.0
```

## Common Issues

### Issue: "The project needs to be deployed"
This typically happens with MAUI Windows apps. Try:
```powershell
dotnet build -f net9.0-windows10.0.19041.0 -c Debug
dotnet run -f net9.0-windows10.0.19041.0 -c Debug
```

### Issue: Splash screen errors
The project has been cleaned of Visual Studio-specific configurations. If you still encounter splash screen issues, ensure the splash image exists at:
`Resources\Splash\splash.png`

### Issue: Package restore errors
```powershell
dotnet restore
dotnet build -f net9.0-windows10.0.19041.0
```

## VS Code Tasks (Optional)

You can create a `.vscode/tasks.json` file in the project root for quick access to build/run commands:

```json
{
    "version": "2.0.0",
    "tasks": [
        {
            "label": "build-windows",
            "command": "dotnet",
            "type": "process",
            "args": [
                "build",
                "${workspaceFolder}/SecureDailyJournal/SecureDailyJournal.csproj",
                "-f",
                "net9.0-windows10.0.19041.0"
            ],
            "problemMatcher": "$msCompile"
        },
        {
            "label": "run-windows",
            "command": "dotnet",
            "type": "process",
            "args": [
                "run",
                "--project",
                "${workspaceFolder}/SecureDailyJournal/SecureDailyJournal.csproj",
                "-f",
                "net9.0-windows10.0.19041.0"
            ],
            "problemMatcher": "$msCompile"
        }
    ]
}
```

Then you can run tasks from VS Code using `Ctrl+Shift+P` → "Tasks: Run Task" → select "build-windows" or "run-windows".

## Notes

- All Visual Studio 2026-specific files have been removed (.sln, .user, .vs folder)
- The project is now optimized for command-line builds using `dotnet` CLI
- You can still open this project in Visual Studio if needed - it will regenerate the solution file automatically
