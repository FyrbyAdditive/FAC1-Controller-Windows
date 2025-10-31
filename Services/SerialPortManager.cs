using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using Feetech.ServoSDK;

namespace FAC1_Controller_Windows.Services
{
    public class SerialPortManager : IDisposable
    {
        private PortHandler? _portHandler;
        private IPacketHandler? _packetHandler;
        private string? _currentPortName;
        private bool _isConnected = false;
        private bool _disposed = false;

        public event EventHandler<bool>? ConnectionStatusChanged;
        public event EventHandler<string>? StatusMessageChanged;

        public bool IsConnected => _isConnected;
        public string? CurrentPortName => _currentPortName;
        public PortHandler? PortHandler => _portHandler;
        public IPacketHandler? PacketHandler => _packetHandler;

        // Constants matching the macOS version
        public const uint DefaultBaudRate = 1000000;
        public const int ConnectionTimeoutMs = 5000;

        public SerialPortManager()
        {
            // Initialize packet handler for STS series servos (protocol 0)
            _packetHandler = PacketHandlerFactory.CreatePacketHandler(0);
        }

        /// <summary>
        /// Get all available serial ports that might be USB-serial adapters
        /// </summary>
        public static List<string> GetAvailableSerialPorts()
        {
            var allPorts = SerialPort.GetPortNames().ToList();
            var usbPorts = new List<string>();

            foreach (var port in allPorts)
            {
                try
                {
                    // Try to get more information about the port
                    // Windows COM ports for USB devices typically show up as COM1, COM2, etc.
                    // We'll include all available ports and let the user/auto-detection figure it out
                    usbPorts.Add(port);
                }
                catch
                {
                    // If we can't access port info, skip it
                    continue;
                }
            }

            return usbPorts.OrderBy(p => p).ToList();
        }

        /// <summary>
        /// Automatically find and connect to the first available USB-serial port
        /// </summary>
        public async Task<bool> AutoConnectAsync()
        {
            var availablePorts = GetAvailableSerialPorts();
            
            OnStatusMessageChanged($"Scanning {availablePorts.Count} available ports...");

            foreach (var port in availablePorts)
            {
                OnStatusMessageChanged($"Trying port {port}...");
                
                if (await ConnectToPortAsync(port))
                {
                    OnStatusMessageChanged($"Connected to {port}");
                    return true;
                }
                
                // Brief delay between attempts
                await Task.Delay(100);
            }

            OnStatusMessageChanged("No suitable ports found");
            return false;
        }

        /// <summary>
        /// Connect to a specific serial port
        /// </summary>
        public async Task<bool> ConnectToPortAsync(string portName)
        {
            return await Task.Run(() => ConnectToPort(portName));
        }

        /// <summary>
        /// Synchronous connection to a specific port
        /// </summary>
        public bool ConnectToPort(string portName)
        {
            try
            {
                // Disconnect any existing connection
                Disconnect();

                // Create new port handler
                _portHandler = new PortHandler(portName);

                // Try to open the port
                _portHandler.OpenPort();
                _portHandler.SetBaudRate(DefaultBaudRate);

                _currentPortName = portName;
                _isConnected = true;

                OnConnectionStatusChanged(true);
                OnStatusMessageChanged($"Connected to {portName} at {DefaultBaudRate} baud");

                return true;
            }
            catch (Exception ex)
            {
                OnStatusMessageChanged($"Failed to connect to {portName}: {ex.Message}");
                
                // Clean up on failure
                _portHandler?.ClosePort();
                _portHandler = null;
                _currentPortName = null;
                _isConnected = false;

                return false;
            }
        }

        /// <summary>
        /// Test if we can communicate with a servo on the connected port
        /// </summary>
        public bool TestServoConnection(byte servoId)
        {
            if (!_isConnected || _portHandler == null || _packetHandler == null)
                return false;

            try
            {
                var (modelNumber, result, error) = _packetHandler.Ping(_portHandler, servoId);
                return result == CommResult.Success && error.IsEmpty();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Disconnect from the current port
        /// </summary>
        public void Disconnect()
        {
            if (_portHandler != null)
            {
                try
                {
                    _portHandler.ClosePort();
                }
                catch
                {
                    // Ignore errors during cleanup
                }
                finally
                {
                    _portHandler = null;
                }
            }

            _currentPortName = null;
            _isConnected = false;

            OnConnectionStatusChanged(false);
            OnStatusMessageChanged("Disconnected");
        }

        /// <summary>
        /// Check if the current connection is still valid
        /// </summary>
        public bool IsConnectionValid()
        {
            if (!_isConnected || _portHandler == null)
                return false;

            try
            {
                // Try to check if port is still open
                // This is a simple way to verify the connection
                return _portHandler != null;
            }
            catch
            {
                return false;
            }
        }

        private void OnConnectionStatusChanged(bool connected)
        {
            ConnectionStatusChanged?.Invoke(this, connected);
        }

        private void OnStatusMessageChanged(string message)
        {
            StatusMessageChanged?.Invoke(this, message);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Disconnect();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        ~SerialPortManager()
        {
            Dispose();
        }
    }
}