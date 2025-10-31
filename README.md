# FAC1 Controller for Windows

A native Windows WPF application for controlling pan and tilt servos for the [FAC1 Camera](https://www.printables.com/model/1463101-fac1-fyrby-additive-camera-1/).

This is a Windows port of the [macOS FAC1 Controller](https://github.com/FyrbyAdditive/FAC1-Controller-macOS), built using C# and WPF with the [Feetech Servo SDK for C#](https://github.com/FyrbyAdditive/feetech-servo-sdk-csharp).

![FAC1 Camera](https://github.com/FyrbyAdditive/FAC1-Controller-Windows/raw/refs/heads/main/Media/FAC1-Camera.png)

## Features

- **Automatic USB-Serial Detection**: Automatically finds and connects to USB-serial devices
- **Dual Servo Control**: Controls pan and tilt servos (hardcoded IDs: pan=6, tilt=4)
- **Connection Status**: Real-time status indicators for USB and individual servos
- **Calibration**: Calibrate the center point of the servos with EEPROM persistence
- **Axis Inversion**: Toggle pan and tilt axis directions (persisted across sessions)
- **Always on Top**: Optional window mode to keep the controller always visible
- **Compact Interface**: Minimal 280x400px fixed-width window with dark theme
- **Color-Coded Controls**: Green pan buttons, blue tilt buttons, orange center button
- **Settings Persistence**: Saves axis inversions and window preferences
- **MSI Installer**: Professional installer package with uninstall support

![alt text](https://github.com/FyrbyAdditive/FAC1-Controller-Windows/raw/refs/heads/main/Media/FAC1-Controller-Banner.png "FAC1 Camera Controller")

## Requirements

- Windows 10 or later
- .NET 8.0 Runtime
- USB-to-serial adapter
- [FAC1 Camera](https://www.printables.com/model/1463101-fac1-fyrby-additive-camera-1/) with pan/tilt servos

## Installation

### Option 1: MSI Installer (Recommended)
1. Download `FAC1-Controller-Setup.msi` from the [Releases page](https://github.com/FyrbyAdditive/FAC1-Controller-Windows/releases)
2. Double-click the installer and follow the setup wizard
3. The application will be installed to `C:\Program Files\FAC1 Controller\`
4. Launch from Start Menu: **FAC1 Controller**

### Option 2: Portable Executable
1. Download the latest release ZIP from the [Releases page](https://github.com/FyrbyAdditive/FAC1-Controller-Windows/releases)
2. Extract to your desired location
3. Run `FAC1-Controller-Windows.exe`

### Option 3: Build from Source
1. **Clone the repository and dependencies**:
   ```powershell
   # Clone the main project
   git clone https://github.com/FyrbyAdditive/FAC1-Controller-Windows.git
   cd FAC1-Controller-Windows
   
   # Clone the Feetech SDK dependency in the parent directory
   cd ..
   git clone https://github.com/FyrbyAdditive/feetech-servo-sdk-csharp.git
   cd FAC1-Controller-Windows
   ```

2. **Build and run**:
   ```powershell
   dotnet build -c Release
   dotnet run -c Release
   ```

   The project expects the Feetech SDK to be in `../feetech-servo-sdk-csharp/` relative to the project directory.

## Usage

### First Launch
1. Connect your USB-to-serial adapter
2. Power on your servos
3. Launch the application
4. The app will automatically search for and connect to available USB-serial ports
5. Once connected, servo status will be displayed

### Controls
- **↑ (Up Arrow)**: Tilt up
- **↓ (Down Arrow)**: Tilt down
- **● (Center Button)**: Center camera
- **← (Left Arrow)**: Pan left
- **→ (Right Arrow)**: Pan right

### Configuration
- **Always on Top**: Check to keep the window always visible above other applications
- **Reconnect**: Manually reconnect to the USB-serial port

### Calibration Menu
- **Set Center Position**: Disables torque so you can manually position the camera, then saves the current location as the new center point (writes value 128 to torque enable register for STS servo EEPROM calibration)
- **Invert Pan Axis**: Reverses pan servo direction (setting persisted across sessions)
- **Invert Tilt Axis**: Reverses tilt servo direction (setting persisted across sessions)

**Note**: During calibration, servo torque is automatically disabled to allow manual positioning. The torque is re-enabled after calibration is complete. The center position is permanently saved to the servo's EEPROM.

## Servo Configuration

The application uses the following servo settings:

- **Pan Servo ID**: 6 (hardcoded)
- **Tilt Servo ID**: 4 (hardcoded)
- **Baud Rate**: 1,000,000
- **Protocol**: STS series (STS3250)
- **Movement Step**: 100 units per button press
- **Speed**: 200
- **Acceleration**: 50
- **Position Range**: 0 - 4095 (full 360°)

Servo IDs can be changed in `Controllers/ServoController.cs` if needed.

## Troubleshooting

### Application doesn't connect
1. Check that servos are powered on
2. Verify USB-serial adapter is connected and recognized by Windows
3. Check Device Manager for COM port availability
4. Make sure servo IDs match the configured values
5. Click "Reconnect" to retry connection

### Servos don't respond
1. Verify servo IDs are correct (default: pan=6, tilt=4)
2. Check power supply to servos
3. Ensure baud rate matches your servo configuration (default: 1,000,000)
4. Try running the Feetech SDK ping example to test basic connectivity
5. Note that servos may ship with the same ID and need to be reprogrammed

### COM port access denied
On Windows, make sure:
1. No other applications are using the COM port
2. Your user account has permission to access serial ports
3. The COM port driver is properly installed

## Building for Distribution

### Build MSI Installer (Recommended)

1. **Install WiX Toolset**:
   ```powershell
   dotnet tool install --global wix
   ```

2. **Build the application**:
   ```powershell
   dotnet build -c Release
   ```

3. **Build the installer**:
   ```powershell
   .\build-installer.ps1
   ```
   
   Or manually:
   ```powershell
   cd Installer
   wix build Product.wxs -arch x64 -ext WixToolset.UI.wixext -out ..\bin\Release\FAC1-Controller-Setup.msi
   ```

4. **The MSI installer will be created at**:
   ```
   bin\Release\FAC1-Controller-Setup.msi
   ```

### Build Self-Contained Executable

To create a standalone executable without installer:

```powershell
# Build self-contained Windows executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# The executable will be created in:
# bin/Release/net8.0-windows/win-x64/publish/FAC1-Controller-Windows.exe
```

## Development

### Dependencies
- .NET 8.0 SDK
- System.IO.Ports NuGet package
- Feetech Servo SDK for C# (project reference)

### Architecture
The application follows a clean architecture pattern:
- **Services Layer**: `SerialPortManager` handles low-level serial communication
- **Controllers Layer**: `ServoController` provides high-level servo control and calibration
- **UI Layer**: WPF windows with XAML for layout and C# code-behind
- **Settings Layer**: `AppSettings` handles JSON-based configuration persistence

### Key Features
- **Asynchronous Operations**: Non-blocking UI during connection attempts
- **Event-Driven Architecture**: Loose coupling between components via events
- **Settings Persistence**: JSON configuration stored in `%APPDATA%\FAC1-Controller\settings.json`
- **Error Handling**: Comprehensive error handling with console logging
- **Resource Management**: Proper disposal of serial port resources via IDisposable
- **EEPROM Calibration**: Uses STS servo protocol (write 128 to torque enable) for permanent center position storage

## Credits

This project uses the [Feetech Servo SDK for C#](https://github.com/FyrbyAdditive/feetech-servo-sdk-csharp).

Based on the macOS version: [FAC1-Controller-macOS](https://github.com/FyrbyAdditive/FAC1-Controller-macOS)

Copyright 2025, Timothy Ellis, Fyrby Additive Manufacturing & Engineering

## License

This project is licensed under the same terms as the original macOS version.
