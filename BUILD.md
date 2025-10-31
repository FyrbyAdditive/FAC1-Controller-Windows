# Build Instructions for FAC1 Controller Windows

## Prerequisites

1. **Development Environment**:
   - Windows 10/11
   - .NET 8.0 SDK or later
   - Visual Studio 2022 (recommended) or VS Code with C# extension
   - Git (for cloning dependencies)

2. **Clone Repository with Dependencies**:
   ```powershell
   # Clone the main project
   git clone https://github.com/FyrbyAdditive/FAC1-Controller-Windows.git
   cd FAC1-Controller-Windows
   
   # Clone the Feetech SDK dependency in the parent directory
   cd ..
   git clone https://github.com/FyrbyAdditive/feetech-servo-sdk-csharp.git
   cd FAC1-Controller-Windows
   ```

   **Directory Structure**:
   ```
   parent-folder/
   ├── FAC1-Controller-Windows/
   │   ├── Controllers/
   │   ├── Services/
   │   └── FAC1-Controller-Windows.csproj
   └── feetech-servo-sdk-csharp/
       └── FeetechServoSDK/
           └── FeetechServoSDK.csproj
   ```

## Building the Application

### Method 1: Using .NET CLI (Recommended)

1. **Clone or extract the project**:
   ```powershell
   cd "C:\Users\timel\VSCode\FAC1 Controller Windows"
   ```

2. **Restore dependencies**:
   ```powershell
   dotnet restore
   ```

3. **Build for development**:
   ```powershell
   dotnet build
   ```

4. **Run in development mode**:
   ```powershell
   dotnet run
   ```

### Method 2: Release Build

1. **Build optimized release**:
   ```powershell
   dotnet build -c Release
   ```

2. **Run release build**:
   ```powershell
   dotnet run -c Release
   ```

### Method 3: Self-Contained Executable

To create a standalone executable that doesn't require .NET runtime:

```powershell
# Windows x64 (most common)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Windows x86 (32-bit)
dotnet publish -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true

# Windows ARM64
dotnet publish -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true
```

The executable will be created at:
`bin\Release\net8.0-windows\win-x64\publish\FAC1-Controller-Windows.exe`

### Method 4: Framework-Dependent

To create a smaller executable that requires .NET runtime:

```powershell
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

## Build Configurations

### Debug Build
- Includes debugging symbols
- Not optimized
- Larger file size
- Useful for development

```powershell
dotnet build -c Debug
```

### Release Build
- Optimized for performance
- Smaller file size
- No debugging symbols
- Ready for distribution

```powershell
dotnet build -c Release
```

## Troubleshooting Build Issues

### Common Issues

1. **SDK Reference Error**:
   ```
   Error: Project reference '../feetech-servo-sdk-csharp/FeetechServoSDK/FeetechServoSDK.csproj' not found
   ```
   
   **Solution**: Ensure the Feetech SDK is in the correct relative path, or update the project reference in the .csproj file.

2. **Missing .NET SDK**:
   ```
   Error: 'dotnet' is not recognized
   ```
   
   **Solution**: Install .NET 8.0 SDK from https://dotnet.microsoft.com/download

3. **Package Restore Fails**:
   ```
   Error: Package restore failed
   ```
   
   **Solution**: 
   ```powershell
   dotnet restore --force
   dotnet clean
   dotnet build
   ```

4. **WPF Not Found**:
   ```
   Error: UseWPF requires Windows
   ```
   
   **Solution**: This is a Windows-only application. Build on Windows with .NET 8.0-windows target framework.

### Performance Optimization

For best performance in release builds:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -p:TrimMode=link
```

**Note**: Trimming may cause issues with reflection-based code. Test thoroughly.

## Building with Visual Studio

1. **Open the project**:
   - Open `FAC1-Controller-Windows.csproj` in Visual Studio 2022

2. **Select configuration**:
   - Choose "Debug" or "Release" from the toolbar

3. **Build the project**:
   - Build → Build Solution (Ctrl+Shift+B)

4. **Run the application**:
   - Debug → Start Without Debugging (Ctrl+F5)

## Packaging for Distribution

### Option 1: Portable Executable
The single-file publish creates a portable .exe that can be distributed without installation.

### Option 2: Installer Package
For a professional installer, use tools like:
- **WiX Toolset**: Create MSI installers
- **Inno Setup**: Create lightweight installers
- **NSIS**: Create custom installers

### Option 3: Microsoft Store Package
Convert to MSIX for Microsoft Store distribution:

```powershell
# Install MSIX Packaging Tool from Microsoft Store
# Follow Microsoft's MSIX packaging guidelines
```

## Development Workflow

1. **Make changes to source code**
2. **Test in debug mode**:
   ```powershell
   dotnet run
   ```

3. **Build release for testing**:
   ```powershell
   dotnet build -c Release
   ```

4. **Create distribution package**:
   ```powershell
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```

## Output Locations

- **Debug builds**: `bin\Debug\net8.0-windows\`
- **Release builds**: `bin\Release\net8.0-windows\`
- **Published apps**: `bin\Release\net8.0-windows\win-x64\publish\`

## File Sizes (Approximate)

- **Framework-dependent**: ~200 KB
- **Self-contained**: ~150 MB (includes .NET runtime)
- **Trimmed self-contained**: ~50-80 MB (optimized)

Choose based on your distribution needs and target audience.