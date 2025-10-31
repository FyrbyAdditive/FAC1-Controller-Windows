#!/usr/bin/env pwsh
# Build script for FAC1 Controller Windows
# PowerShell script that works on Windows

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$SelfContained = $true,
    [switch]$SingleFile = $true,
    [switch]$Clean = $false
)

Write-Host "FAC1 Controller Windows Build Script" -ForegroundColor Green
Write-Host "===================================" -ForegroundColor Green

# Clean if requested
if ($Clean) {
    Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
    dotnet clean
    Remove-Item -Path "bin" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "obj" -Recurse -Force -ErrorAction SilentlyContinue
}

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
$restoreResult = dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Package restore failed!" -ForegroundColor Red
    exit 1
}

# Build project
Write-Host "Building project..." -ForegroundColor Yellow
$buildResult = dotnet build -c $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Publish self-contained executable
if ($SelfContained) {
    Write-Host "Publishing self-contained executable..." -ForegroundColor Yellow
    
    $publishArgs = @(
        "publish"
        "-c", $Configuration
        "-r", $Runtime
        "--self-contained", "true"
    )
    
    if ($SingleFile) {
        $publishArgs += "-p:PublishSingleFile=true"
    }
    
    $publishResult = & dotnet @publishArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Publish failed!" -ForegroundColor Red
        exit 1
    }
    
    $outputPath = "bin\$Configuration\net8.0-windows\$Runtime\publish"
    Write-Host "Self-contained executable created at: $outputPath" -ForegroundColor Green
    
    # Show file size
    $exePath = "$outputPath\FAC1-Controller-Windows.exe"
    if (Test-Path $exePath) {
        $fileSize = (Get-Item $exePath).Length
        $fileSizeMB = [math]::Round($fileSize / 1MB, 2)
        Write-Host "Executable size: $fileSizeMB MB" -ForegroundColor Cyan
    }
} else {
    Write-Host "Framework-dependent build completed." -ForegroundColor Green
    Write-Host "Run with: dotnet run -c $Configuration" -ForegroundColor Cyan
}

Write-Host "`nBuild completed successfully!" -ForegroundColor Green

# Show next steps
Write-Host "`nNext steps:" -ForegroundColor Yellow
if ($SelfContained) {
    Write-Host "1. Test the executable: .\bin\$Configuration\net8.0-windows\$Runtime\publish\FAC1-Controller-Windows.exe" -ForegroundColor White
    Write-Host "2. Copy the publish folder to distribute the application" -ForegroundColor White
} else {
    Write-Host "1. Run the application: dotnet run -c $Configuration" -ForegroundColor White
    Write-Host "2. Publish for distribution: .\build.ps1 -SelfContained" -ForegroundColor White
}
Write-Host "3. Check README.md for usage instructions" -ForegroundColor White