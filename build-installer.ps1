# Build script for FAC1 Controller Installer
# Run this after building the main application in Release mode

Write-Host "Building FAC1 Controller Installer..." -ForegroundColor Green

# Ensure Release build exists
$releasePath = ".\bin\Release\net8.0-windows"
if (-not (Test-Path $releasePath)) {
    Write-Host "Error: Release build not found. Please build in Release mode first." -ForegroundColor Red
    Write-Host "Run: dotnet build -c Release" -ForegroundColor Yellow
    exit 1
}

# Build the installer
Push-Location Installer
try {
    Write-Host "Compiling WiX source..." -ForegroundColor Cyan
    wix build Product.wxs -arch x64 -ext WixToolset.UI.wixext -out ..\bin\Release\FAC1-Controller-Setup.msi
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nInstaller created successfully!" -ForegroundColor Green
        Write-Host "Location: .\bin\Release\FAC1-Controller-Setup.msi" -ForegroundColor Yellow
    } else {
        Write-Host "`nInstaller build failed." -ForegroundColor Red
    }
} finally {
    Pop-Location
}
