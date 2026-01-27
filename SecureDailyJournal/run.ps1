# SecureDailyJournal - Build and Run Script
# This script builds the project, registers the MSIX package, and launches the app

Write-Host "Building SecureDailyJournal..." -ForegroundColor Cyan
dotnet build -f net9.0-windows10.0.19041.0

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful! Registering app package..." -ForegroundColor Green
    Add-AppxPackage -Register ".\bin\Debug\net9.0-windows10.0.19041.0\win10-x64\AppxManifest.xml" -ForceApplicationShutdown -ForceUpdateFromAnyVersion
    
    Write-Host "Launching SecureDailyJournal..." -ForegroundColor Green
    Start-Process "shell:AppsFolder\$(Get-AppxPackage | Where-Object {$_.Name -like '*SecureDailyJournal*'} | Select-Object -ExpandProperty PackageFamilyName)!App"
    
    Write-Host "App launched successfully!" -ForegroundColor Green
} else {
    Write-Host "Build failed. Please check the errors above." -ForegroundColor Red
}
