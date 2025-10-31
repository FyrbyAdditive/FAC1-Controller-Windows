# Changelog

All notable changes to the FAC1 Controller Windows will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed
- **CRITICAL**: Calibration now properly saves center position to servo EEPROM using correct STS servo method (write 128 to torque enable register)
- Calibration now disables torque before allowing manual positioning, then re-enables it after
- Windows dark mode menu visibility - proper contrast and colors for all menu items
- Menu dropdown placement - menus now correctly drop down instead of opening to the right
- Menu border styling - eliminated light borders that were theme-sensitive

### Changed
- Calibration workflow now explicitly disables torque and shows instructions for manual camera positioning
- Improved calibration dialog messages to guide users through the process
- Removed ineffective angle limit manipulation from calibration (was not the correct method for STS servos)

## [1.0.0] - 2025-10-31

### Added
- Initial Windows port of FAC1 Controller from macOS version
- Native WPF application with modern dark theme
- Automatic USB-serial device detection and connection
- Pan and tilt servo control with configurable IDs (default: Pan=6, Tilt=4)
- Real-time connection status indicators for USB and individual servos
- Arrow button controls for pan (←→) and tilt (↑↓) movement
- Center camera button (●) for quick positioning
- Always-on-top window mode for convenient access
- Axis inversion settings for pan and tilt
- Center point calibration with offset adjustment sliders
- Real-time position display for both servos
- Status logging with timestamps
- Reconnect functionality for manual connection retry
- Comprehensive error handling and user feedback
- Self-contained executable build option
- Complete documentation (README, Quick Start, Build instructions)

### Technical Features
- Built with .NET 8.0 and WPF
- Uses Feetech Servo SDK for C# for servo communication
- Asynchronous operations for non-blocking UI
- Event-driven architecture with loose coupling
- Proper resource management and disposal
- Comprehensive status reporting

### Hardware Support
- STS series servos (STS3250 tested)
- USB-to-serial adapters
- Baud rate: 1,000,000 (configurable in code)
- Position range: 0-4095 (full 360°)
- Movement step: 100 units per button press
- Speed: 200, Acceleration: 50

### Documentation
- Complete README with features and usage
- Quick Start guide for rapid setup
- Build instructions for developers
- Troubleshooting guide
- Hardware connection diagrams

### Known Issues
- Application icon placeholder (needs proper .ico file)
- Servo IDs are hardcoded (can be changed in source code)

### Compatibility
- Windows 10 or later required
- .NET 8.0 runtime (or included in self-contained build)
- Compatible with original macOS version configuration

---

## Template for Future Releases

## [Unreleased]

### Added
### Changed
### Deprecated
### Removed
### Fixed
### Security