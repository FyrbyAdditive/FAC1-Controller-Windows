using System;
using System.Threading.Tasks;
using Feetech.ServoSDK;
using FAC1_Controller_Windows.Services;

namespace FAC1_Controller_Windows.Controllers
{
    public class ServoController : IDisposable
    {
        private readonly SerialPortManager _serialPortManager;
        private bool _disposed = false;

        // Servo configuration - matching the macOS version
        public const byte PanServoId = 6;
        public const byte TiltServoId = 4;
        public const ushort MovementStep = 100;
        public const ushort ServoSpeed = 200;
        public const byte ServoAcceleration = 50;
        public const ushort MinPosition = 0;
        public const ushort MaxPosition = 4095;
        public const ushort CenterPosition = MaxPosition / 2; // 2047

        // Current servo positions
        public ushort PanPosition { get; private set; } = CenterPosition;
        public ushort TiltPosition { get; private set; } = CenterPosition;

        // Calibration offsets
        public short PanCenterOffset { get; set; } = 0;
        public short TiltCenterOffset { get; set; } = 0;

        // Axis inversion flags
        public bool InvertPanAxis { get; set; } = false;
        public bool InvertTiltAxis { get; set; } = false;

        // Connection status
        public bool IsPanServoConnected { get; private set; } = false;
        public bool IsTiltServoConnected { get; private set; } = false;

        // Events
        public event EventHandler<string>? StatusMessageChanged;
        public event EventHandler<bool>? PanServoStatusChanged;
        public event EventHandler<bool>? TiltServoStatusChanged;

        public ServoController(SerialPortManager serialPortManager)
        {
            _serialPortManager = serialPortManager ?? throw new ArgumentNullException(nameof(serialPortManager));
            _serialPortManager.ConnectionStatusChanged += OnConnectionStatusChanged;
        }

        /// <summary>
        /// Initialize servos and check their connection status
        /// </summary>
        public async Task InitializeServosAsync()
        {
            await Task.Run(() => InitializeServos());
        }

        public void InitializeServos()
        {
            if (!_serialPortManager.IsConnected)
            {
                OnStatusMessageChanged("No serial connection available");
                return;
            }

            OnStatusMessageChanged("Initializing servos...");

            // Check pan servo connection
            IsPanServoConnected = TestAndInitializeServo(PanServoId, "Pan");
            OnPanServoStatusChanged(IsPanServoConnected);

            // Check tilt servo connection  
            IsTiltServoConnected = TestAndInitializeServo(TiltServoId, "Tilt");
            OnTiltServoStatusChanged(IsTiltServoConnected);

            if (IsPanServoConnected || IsTiltServoConnected)
            {
                OnStatusMessageChanged($"Servos initialized - Pan: {(IsPanServoConnected ? "OK" : "FAIL")}, Tilt: {(IsTiltServoConnected ? "OK" : "FAIL")}");
                
                // Read current positions
                ReadCurrentPositions();
            }
            else
            {
                OnStatusMessageChanged("No servos found - Check connections and IDs");
            }
        }

        private bool TestAndInitializeServo(byte servoId, string servoName)
        {
            if (_serialPortManager.PacketHandler == null || _serialPortManager.PortHandler == null)
                return false;

            try
            {
                // Test communication with ping
                var (modelNumber, result, error) = _serialPortManager.PacketHandler.Ping(_serialPortManager.PortHandler, servoId);
                
                if (result != CommResult.Success || !error.IsEmpty())
                {
                    OnStatusMessageChanged($"{servoName} servo (ID {servoId}): Not responding");
                    return false;
                }

                OnStatusMessageChanged($"{servoName} servo (ID {servoId}): Connected (Model: {modelNumber})");

                // Enable torque
                EnableServoTorque(servoId);

                // Set servo parameters
                SetServoParameters(servoId);

                return true;
            }
            catch (Exception ex)
            {
                OnStatusMessageChanged($"{servoName} servo error: {ex.Message}");
                return false;
            }
        }

        private void EnableServoTorque(byte servoId)
        {
            try
            {
                var (result, error) = _serialPortManager.PacketHandler!.Write1ByteTxRx(
                    _serialPortManager.PortHandler!, servoId, ControlTableAddress.TorqueEnable, 1);

                if (result != CommResult.Success || !error.IsEmpty())
                {
                    OnStatusMessageChanged($"Warning: Failed to enable torque for servo {servoId}");
                }
            }
            catch (Exception ex)
            {
                OnStatusMessageChanged($"Error enabling torque for servo {servoId}: {ex.Message}");
            }
        }

        private void SetServoParameters(byte servoId)
        {
            try
            {
                // Set movement speed
                _serialPortManager.PacketHandler!.Write2ByteTxRx(
                    _serialPortManager.PortHandler!, servoId, ControlTableAddress.GoalSpeed, ServoSpeed);

                // Set acceleration
                _serialPortManager.PacketHandler!.Write1ByteTxRx(
                    _serialPortManager.PortHandler!, servoId, ControlTableAddress.GoalAcc, ServoAcceleration);
            }
            catch (Exception ex)
            {
                OnStatusMessageChanged($"Warning: Failed to set parameters for servo {servoId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Move pan servo left
        /// </summary>
        public void MovePanLeft()
        {
            if (!IsPanServoConnected) return;

            var newPosition = CalculateNewPosition(PanPosition, -MovementStep, InvertPanAxis);
            MovePanToPosition(newPosition);
        }

        /// <summary>
        /// Move pan servo right
        /// </summary>
        public void MovePanRight()
        {
            if (!IsPanServoConnected) return;

            var newPosition = CalculateNewPosition(PanPosition, MovementStep, InvertPanAxis);
            MovePanToPosition(newPosition);
        }

        /// <summary>
        /// Move tilt servo up
        /// </summary>
        public void MoveTiltUp()
        {
            if (!IsTiltServoConnected) return;

            var newPosition = CalculateNewPosition(TiltPosition, MovementStep, InvertTiltAxis);
            MoveTiltToPosition(newPosition);
        }

        /// <summary>
        /// Move tilt servo down
        /// </summary>
        public void MoveTiltDown()
        {
            if (!IsTiltServoConnected) return;

            var newPosition = CalculateNewPosition(TiltPosition, -MovementStep, InvertTiltAxis);
            MoveTiltToPosition(newPosition);
        }

        /// <summary>
        /// Center both servos
        /// </summary>
        public void CenterCamera()
        {
            var panCenter = (ushort)Math.Clamp(CenterPosition + PanCenterOffset, MinPosition, MaxPosition);
            var tiltCenter = (ushort)Math.Clamp(CenterPosition + TiltCenterOffset, MinPosition, MaxPosition);

            if (IsPanServoConnected)
                MovePanToPosition(panCenter);

            if (IsTiltServoConnected)
                MoveTiltToPosition(tiltCenter);

            OnStatusMessageChanged("Camera centered");
        }

        /// <summary>
        /// Disable torque on both servos to allow manual positioning
        /// </summary>
        public void DisableTorque()
        {
            if (_serialPortManager.PacketHandler == null || _serialPortManager.PortHandler == null)
                return;

            OnStatusMessageChanged("Disabling torque - you can now move the camera manually");

            if (IsPanServoConnected)
            {
                var (result, error) = _serialPortManager.PacketHandler.Write1ByteTxRx(
                    _serialPortManager.PortHandler, PanServoId, ControlTableAddress.TorqueEnable, 0);
                
                if (result == CommResult.Success && error.IsEmpty())
                {
                    OnStatusMessageChanged("Pan servo torque disabled");
                }
            }

            if (IsTiltServoConnected)
            {
                var (result, error) = _serialPortManager.PacketHandler.Write1ByteTxRx(
                    _serialPortManager.PortHandler, TiltServoId, ControlTableAddress.TorqueEnable, 0);
                
                if (result == CommResult.Success && error.IsEmpty())
                {
                    OnStatusMessageChanged("Tilt servo torque disabled");
                }
            }
        }

        /// <summary>
        /// Enable torque on both servos
        /// </summary>
        public void EnableTorque()
        {
            if (_serialPortManager.PacketHandler == null || _serialPortManager.PortHandler == null)
                return;

            OnStatusMessageChanged("Enabling torque...");

            if (IsPanServoConnected)
            {
                EnableServoTorque(PanServoId);
            }

            if (IsTiltServoConnected)
            {
                EnableServoTorque(TiltServoId);
            }

            OnStatusMessageChanged("Torque enabled");
        }

        /// <summary>
        /// Set current position as center for both servos (writes to EEPROM)
        /// This uses the STS servo calibration method: writing 128 to torque enable register
        /// saves the current position as center (2048) to EEPROM
        /// </summary>
        public void SetCenterPosition()
        {
            if (!_serialPortManager.IsConnected)
            {
                OnStatusMessageChanged("Cannot set center - no serial connection");
                return;
            }

            if (_serialPortManager.PacketHandler == null || _serialPortManager.PortHandler == null)
            {
                OnStatusMessageChanged("Cannot set center - no packet handler available");
                return;
            }

            OnStatusMessageChanged("Calibrating servos - writing current position to EEPROM...");

            bool panSet = false, tiltSet = false;

            // For STS servos, write 128 to torque enable register to calibrate current position to 2048
            // This is the proper way to calibrate - don't use the offset register
            if (IsPanServoConnected)
            {
                var (result, error) = _serialPortManager.PacketHandler.Write1ByteTxRx(
                    _serialPortManager.PortHandler, PanServoId, ControlTableAddress.TorqueEnable, 128);
                
                panSet = (result == CommResult.Success);
                OnStatusMessageChanged($"Pan calibration: {(panSet ? "OK" : "FAIL")}");
            }

            if (IsTiltServoConnected)
            {
                var (result, error) = _serialPortManager.PacketHandler.Write1ByteTxRx(
                    _serialPortManager.PortHandler, TiltServoId, ControlTableAddress.TorqueEnable, 128);
                
                tiltSet = (result == CommResult.Success);
                OnStatusMessageChanged($"Tilt calibration: {(tiltSet ? "OK" : "FAIL")}");
            }

            if (panSet || tiltSet)
            {
                // Reset offsets since we've updated the servo's internal center
                PanCenterOffset = 0;
                TiltCenterOffset = 0;
                OnStatusMessageChanged("Calibration complete - current position saved as center");
            }
            else
            {
                OnStatusMessageChanged("Calibration failed - no servos available");
            }
        }

        /// <summary>
        /// Reset center position to factory default (2047)
        /// This will move both servos to center position and then calibrate
        /// </summary>
        public void ResetCenterPosition()
        {
            if (!_serialPortManager.IsConnected)
            {
                OnStatusMessageChanged("Cannot reset center - no serial connection");
                return;
            }

            OnStatusMessageChanged("Resetting to factory center position...");

            // Move both servos to center position (2047)
            if (IsPanServoConnected)
            {
                MovePanToPosition(CenterPosition);
            }

            if (IsTiltServoConnected)
            {
                MoveTiltToPosition(CenterPosition);
            }

            // Wait for movement to complete
            System.Threading.Thread.Sleep(1000);

            // Calibrate at this position
            SetCenterPosition();

            OnStatusMessageChanged("Factory center position restored");
        }

        /// <summary>
        /// Move pan servo to specific position
        /// </summary>
        public void MovePanToPosition(ushort position)
        {
            if (!IsPanServoConnected || _serialPortManager.PacketHandler == null || _serialPortManager.PortHandler == null)
                return;

            position = (ushort)Math.Clamp(position, MinPosition, MaxPosition);

            try
            {
                var (result, error) = _serialPortManager.PacketHandler.Write2ByteTxRx(
                    _serialPortManager.PortHandler, PanServoId, ControlTableAddress.GoalPosition, position);

                if (result == CommResult.Success && error.IsEmpty())
                {
                    PanPosition = position;
                }
                else
                {
                    OnStatusMessageChanged($"Pan move failed: {result.GetDescription()}");
                }
            }
            catch (Exception ex)
            {
                OnStatusMessageChanged($"Pan move error: {ex.Message}");
            }
        }

        /// <summary>
        /// Move tilt servo to specific position
        /// </summary>
        public void MoveTiltToPosition(ushort position)
        {
            if (!IsTiltServoConnected || _serialPortManager.PacketHandler == null || _serialPortManager.PortHandler == null)
                return;

            position = (ushort)Math.Clamp(position, MinPosition, MaxPosition);

            try
            {
                var (result, error) = _serialPortManager.PacketHandler.Write2ByteTxRx(
                    _serialPortManager.PortHandler, TiltServoId, ControlTableAddress.GoalPosition, position);

                if (result == CommResult.Success && error.IsEmpty())
                {
                    TiltPosition = position;
                }
                else
                {
                    OnStatusMessageChanged($"Tilt move failed: {result.GetDescription()}");
                }
            }
            catch (Exception ex)
            {
                OnStatusMessageChanged($"Tilt move error: {ex.Message}");
            }
        }

        /// <summary>
        /// Read current servo positions
        /// </summary>
        public void ReadCurrentPositions()
        {
            if (_serialPortManager.PacketHandler == null || _serialPortManager.PortHandler == null)
                return;

            try
            {
                if (IsPanServoConnected)
                {
                    var (panPos, result, error) = _serialPortManager.PacketHandler.Read2ByteTxRx(
                        _serialPortManager.PortHandler, PanServoId, ControlTableAddress.PresentPosition);
                    
                    if (result == CommResult.Success && error.IsEmpty())
                    {
                        PanPosition = panPos;
                    }
                }

                if (IsTiltServoConnected)
                {
                    var (tiltPos, result, error) = _serialPortManager.PacketHandler.Read2ByteTxRx(
                        _serialPortManager.PortHandler, TiltServoId, ControlTableAddress.PresentPosition);
                    
                    if (result == CommResult.Success && error.IsEmpty())
                    {
                        TiltPosition = tiltPos;
                    }
                }
            }
            catch (Exception ex)
            {
                OnStatusMessageChanged($"Position read error: {ex.Message}");
            }
        }

        private ushort CalculateNewPosition(ushort currentPosition, int step, bool inverted)
        {
            if (inverted)
                step = -step;

            var newPosition = currentPosition + step;
            return (ushort)Math.Clamp(newPosition, MinPosition, MaxPosition);
        }

        private void OnConnectionStatusChanged(object? sender, bool connected)
        {
            if (!connected)
            {
                // Reset servo connection status when serial connection is lost
                IsPanServoConnected = false;
                IsTiltServoConnected = false;
                OnPanServoStatusChanged(false);
                OnTiltServoStatusChanged(false);
                OnStatusMessageChanged("Serial connection lost - Servos disconnected");
            }
        }

        private void OnStatusMessageChanged(string message)
        {
            StatusMessageChanged?.Invoke(this, message);
        }

        private void OnPanServoStatusChanged(bool connected)
        {
            PanServoStatusChanged?.Invoke(this, connected);
        }

        private void OnTiltServoStatusChanged(bool connected)
        {
            TiltServoStatusChanged?.Invoke(this, connected);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _serialPortManager.ConnectionStatusChanged -= OnConnectionStatusChanged;
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        ~ServoController()
        {
            Dispose();
        }
    }
}