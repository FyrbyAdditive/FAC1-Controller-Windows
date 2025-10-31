# FAC1 Controller for Windows - Quick Start

This guide will help you get the FAC1 Controller running on Windows quickly.

## Prerequisites

1. **Hardware Setup**:
   - FAC1 Camera with pan/tilt servos
   - USB-to-serial adapter
   - Power supply for servos (typically 6-12V)
   - Windows 10 or later

2. **Software Requirements**:
   - .NET 8.0 Runtime (if not using self-contained build)

## Building from Source (Optional)

If you want to build the application yourself:

1. **Install Prerequisites**:
   - .NET 8.0 SDK
   - Git

2. **Clone the repositories**:
   ```powershell
   # Create a parent folder for both projects
   mkdir C:\Projects
   cd C:\Projects
   
   # Clone the main application
   git clone https://github.com/FyrbyAdditive/FAC1-Controller-Windows.git
   
   # Clone the Feetech SDK dependency
   git clone https://github.com/FyrbyAdditive/feetech-servo-sdk-csharp.git
   
   # Build and run
   cd FAC1-Controller-Windows
   dotnet build -c Release
   dotnet run -c Release
   ```

   The project references the SDK from `../feetech-servo-sdk-csharp/FeetechServoSDK/FeetechServoSDK.csproj`

## Quick Setup Steps

### Step 1: Hardware Connection
1. Connect your servos to power supply
2. Connect servos to each other via daisy chain (data line)
3. Connect USB-to-serial adapter to your computer
4. Connect data line from servos to USB-to-serial adapter:
   - **Ground**: Connect servo GND to adapter GND
   - **Data**: Connect servo data line to adapter TX/RX (depends on adapter type)
   - **Power**: Servos should have separate power supply

### Step 2: Servo Configuration
**IMPORTANT**: The servos ship with the same ID and need to be configured:

1. **Pan Servo**: Must be programmed to ID **6**
2. **Tilt Servo**: Must be programmed to ID **4**

Use the Feetech SDK tools to change servo IDs before using this controller.

### Step 3: Run the Application
1. Extract the application files to a folder
2. Double-click `FAC1-Controller-Windows.exe`
3. The app will automatically search for COM ports

### Step 4: First Connection
1. Power on your servos
2. The application will show connection status:
   - **USB**: Should show "Connected (COMx)"
   - **Pan Servo (ID 6)**: Should show "Connected" 
   - **Tilt Servo (ID 4)**: Should show "Connected"

3. If servos don't connect:
   - Check servo IDs are correct (6 for pan, 4 for tilt)
   - Verify power supply to servos
   - Click "Reconnect" button
   - Check status messages for detailed error information

### Step 5: Test Movement
1. Use the arrow buttons to test movement:
   - **←** and **→** for pan (left/right)
   - **↑** and **↓** for tilt (up/down)
   - **●** to center both servos

2. If movement is backwards:
   - Check "Invert Pan Axis" or "Invert Tilt Axis" in settings

### Step 6: Calibration
1. Use the center button **●** to move to center position
2. If not properly centered:
   - Adjust "Pan Offset" and "Tilt Offset" sliders in settings
   - Click center button again to test

## Troubleshooting

### No COM Port Detected
- Check Device Manager → Ports (COM & LPT)
- Install USB-to-serial driver if needed
- Try different USB port

### Servos Not Responding
- Verify servo IDs using Feetech SDK ping tool
- Check power supply (6-12V, sufficient current)
- Verify data connections (GND and data line)
- Ensure baud rate is 1,000,000 (default)

### Permission Errors
- Run as Administrator if needed
- Close other applications that might use COM ports

## Common Connection Issues

| Issue | Solution |
|-------|----------|
| "USB: Disconnected" | Check USB cable and adapter drivers |
| "Pan Servo: Not responding" | Verify servo ID is set to 6 |
| "Tilt Servo: Not responding" | Verify servo ID is set to 7 |
| Movement backwards | Enable axis inversion in settings |
| Not centered properly | Adjust offset calibration sliders |

## Support

For issues:
1. Check status messages in the application
2. Verify hardware connections
3. Test servos with Feetech SDK examples first
4. Check servo IDs and configuration

## Next Steps

Once working:
- Adjust center point calibration for your camera mount
- Enable "Always on Top" for convenient control
- Save your settings (settings persist between sessions)