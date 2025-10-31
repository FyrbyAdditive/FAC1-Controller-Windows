# FAC1 Controller - Windows Installer

## Building the Installer

### Prerequisites
- .NET 8.0 SDK
- WiX Toolset v4+ (installed via `dotnet tool install --global wix`)

### Build Steps

1. **Build the application in Release mode:**
   ```powershell
   dotnet build -c Release
   ```

2. **Build the installer:**
   ```powershell
   cd Installer
   wix build Product.wxs -arch x64 -out ..\bin\Release\FAC1-Controller-Setup.msi
   ```

   Or use the provided build script:
   ```powershell
   .\build-installer.ps1
   ```

3. **The installer will be created at:**
   ```
   bin\Release\FAC1-Controller-Setup.msi
   ```

## Installer Details

### Package Information
- **Name:** FAC1 Pan & Tilt Controller
- **Version:** 1.0.0.0
- **Manufacturer:** Fyrby Additive Manufacturing & Engineering
- **Copyright:** Copyright 2025, Timothy Ellis, Fyrby Additive Manufacturing & Engineering

### Installation Contents
- Main application executable and dependencies
- FeetechServoSDK library
- System.IO.Ports library
- FAME logo (150x150px)
- Start Menu shortcut

### Installation Location
- Default: `C:\Program Files\FAC1 Controller\`
- Start Menu: `FAC1 Controller`

### Uninstallation
The installer creates proper Add/Remove Programs entries with:
- Application icon
- Help link to GitHub repository
- Full description with copyright information

Users can uninstall via:
- Windows Settings → Apps → Installed apps
- Control Panel → Programs and Features

## Distribution

The generated MSI file can be distributed directly. No additional files are required for installation.

### System Requirements
- Windows 10/11 (64-bit)
- .NET 8.0 Runtime (included in installer dependencies)
- USB-serial adapter for FAC1 Camera connection
