# FAC1 Controller Windows - Project Summary

## Overview
Successfully created a native Windows port of the FAC1 Controller macOS application using C# and WPF. The application provides full-featured pan and tilt servo control for the FAC1 Camera system.

## Project Structure
```
FAC1 Controller Windows/
├── Controllers/
│   └── ServoController.cs          # High-level servo control with EEPROM calibration
├── Services/
│   └── SerialPortManager.cs        # USB-serial connection management
├── Installer/
│   ├── Product.wxs                 # WiX installer definition
│   ├── License.rtf                 # License agreement for installer
│   └── README.md                   # Installer build instructions
├── MainWindow.xaml                 # Main UI layout (280x400px, fixed width)
├── MainWindow.xaml.cs              # UI event handling and logic
├── AboutWindow.xaml                # About dialog with FAME logo
├── AboutWindow.xaml.cs             # About dialog logic
├── App.xaml                        # Application resources and button styles
├── App.xaml.cs                     # Application entry point
├── AppSettings.cs                  # Settings persistence (JSON)
├── FAME-150-150.png                # Application logo (from macOS version)
├── build-installer.ps1             # PowerShell script to build MSI
├── FAC1-Controller-Windows.csproj  # Project configuration
└── README.md                       # Complete documentation
```

## Features Implemented

### Core Functionality
✅ **Automatic USB-Serial Detection**: Scans and connects to available COM ports
✅ **Dual Servo Control**: Independent control of pan (ID 6) and tilt (ID 4) servos
✅ **Connection Status**: Real-time indicators for USB and individual servo connections at bottom
✅ **Movement Controls**: Color-coded circular buttons (green pan, blue tilt, orange center)
✅ **Center Positioning**: One-click camera centering button

### Advanced Features
✅ **Always on Top**: Checkbox for window always-on-top mode (persisted)
✅ **Axis Inversion**: Menu options for pan and tilt axis direction (persisted)
✅ **EEPROM Calibration**: Set center position that persists to servo EEPROM (STS protocol: write 128 to torque enable)
✅ **Settings Persistence**: JSON-based storage of user preferences in %APPDATA%
✅ **Compact UI**: Fixed 280x400px window with minimal, focused interface
✅ **About Dialog**: Professional about page with FAME logo and copyright info

### Installation & Distribution
✅ **MSI Installer**: WiX-based installer with UI, license agreement, and proper Add/Remove Programs integration
✅ **Start Menu Integration**: Installer creates Start Menu shortcut
✅ **Uninstall Support**: Full uninstall capability via Windows Settings
✅ **Logo Integration**: FAME-150-150.png logo included in installer and About dialog

### Technical Implementation
✅ **Asynchronous Operations**: Non-blocking UI during connection attempts
✅ **Event-Driven Architecture**: Loose coupling between components via events
✅ **Resource Management**: Proper disposal of serial resources with IDisposable
✅ **Error Handling**: Console-based logging for debugging
✅ **Modern UI**: Dark theme (#1E1E1E) with custom button styles and hover effects
✅ **Theme-Aware Buttons**: Dark backgrounds (#2A2A2A) slightly lighter than window background

## Servo Configuration (Matching macOS Version)
- **Pan Servo ID**: 6 (hardcoded)
- **Tilt Servo ID**: 4 (hardcoded)  
- **Baud Rate**: 1,000,000
- **Protocol**: STS series (STS3250)
- **Movement Step**: 100 units per button press
- **Speed**: 200
- **Acceleration**: 50
- **Position Range**: 0 - 4095 (full 360°)

## Dependencies
- **.NET 8.0**: Modern C# language features and performance
- **System.IO.Ports**: Windows serial communication (NuGet package)
- **WPF**: Native Windows UI framework
- **Feetech Servo SDK for C#**: Servo control protocol implementation (project reference to ../feetech-servo-sdk-csharp)
- **WiX Toolset 6.0**: MSI installer creation (global tool)

## Build Outputs
- **MSI Installer**: ~410 KB professional installer package
- **Framework-dependent**: ~200 KB (requires .NET 8.0 runtime)
- **Self-contained**: ~147 MB (includes .NET runtime)

## Key Classes

### SerialPortManager
- Handles USB-serial device detection and connection
- Manages PortHandler and PacketHandler instances
- Provides connection status events
- Implements auto-discovery of suitable COM ports

### ServoController  
- High-level servo control abstraction
- Manages individual servo connections and status
- Implements movement commands (pan left/right, tilt up/down)
- **EEPROM Calibration**: SetCenterPosition() writes 128 to torque enable register for STS servos
- DisableTorque() and EnableTorque() for manual positioning during calibration
- Axis inversion support for pan and tilt

### AppSettings
- JSON-based settings persistence
- Stores: InvertPanAxis, InvertTiltAxis, AlwaysOnTop
- Location: %APPDATA%\FAC1-Controller\settings.json
- Automatic save on setting changes
- Load on application startup

### MainWindow
- WPF user interface with modern dark theme
- Fixed 280x400px window with ResizeMode=CanMinimize
- Menu bar with Calibration and Help menus
- Color-coded circular buttons with Viewbox centering
- Event handling for buttons, checkboxes, and menu items
- Settings integration with AppSettings

### AboutWindow
- Professional about dialog (400x380px)
- Displays FAME-150-150.png logo
- Shows version, copyright, author, and company information
- Matches macOS AboutViewController layout

## Platform Compatibility
- **Windows 10** or later (WPF requirement)
- **USB-to-serial adapters** (any Windows-compatible)

## Development Approach
1. **Analysis**: Studied macOS Swift version for feature parity and UI design
2. **Architecture**: Clean separation of concerns with Services/Controllers/Settings pattern
3. **UI Design**: Native WPF with compact dark theme matching macOS aesthetics
4. **Iterative Refinement**: Multiple UI iterations for button styling, layout, and theming
5. **Settings Persistence**: JSON-based configuration for user preferences
6. **Professional Distribution**: WiX MSI installer with proper Windows integration
7. **Documentation**: Comprehensive README and installer documentation

## Success Criteria Met
✅ **Feature Parity**: All macOS functionality ported to Windows
✅ **Native Experience**: True Windows application using WPF with proper installers
✅ **Professional Quality**: Modern UI, comprehensive documentation, MSI installer
✅ **Easy Distribution**: MSI installer with UI and Add/Remove Programs integration
✅ **Maintainable Code**: Clean architecture with separation of concerns
✅ **Settings Persistence**: User preferences saved across sessions
✅ **EEPROM Calibration**: Proper STS servo calibration that persists to hardware
✅ **Visual Consistency**: UI styling matches macOS version with color-coded controls

## Completed Enhancements
✅ **Settings Persistence**: JSON-based storage in %APPDATA%
✅ **MSI Installer**: WiX-based professional installer with UI
✅ **Logo Integration**: FAME logo in About dialog and installer
✅ **Compact UI**: Minimal 280x400px fixed-width window
✅ **Color-Coded Buttons**: Green pan, blue tilt, orange center
✅ **Dark Theme**: Consistent dark UI with theme-aware button backgrounds

## Deployment Ready
The application is complete and ready for:
- **End User Installation**: MSI installer with Start Menu integration
- **Portable Distribution**: Self-contained executable option
- **Source Code Sharing**: Complete source with documentation and build scripts
- **Commercial Use**: Professional-grade implementation with proper copyright notices

This Windows port successfully brings the FAC1 Camera control experience to Windows users with full feature parity, native platform integration, and professional installer packaging.