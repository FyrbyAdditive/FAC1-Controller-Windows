using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using FAC1_Controller_Windows.Services;
using FAC1_Controller_Windows.Controllers;

namespace FAC1_Controller_Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private SerialPortManager? _serialPortManager;
        private ServoController? _servoController;
        private bool _disposed = false;
        private AppSettings _settings;

        // Status indicator colors
        private readonly SolidColorBrush _connectedBrush = new(Colors.LimeGreen);
        private readonly SolidColorBrush _disconnectedBrush = new(Colors.Red);

        public MainWindow()
        {
            InitializeComponent();
            
            // Load settings
            _settings = AppSettings.Load();
            
            InitializeControllers();
            ApplySettings();
            
            // Start auto-connection
            _ = Task.Run(AttemptAutoConnection);
        }

        private void InitializeControllers()
        {
            _serialPortManager = new SerialPortManager();
            _servoController = new ServoController(_serialPortManager);

            // Subscribe to events
            _serialPortManager.ConnectionStatusChanged += OnConnectionStatusChanged;
            _serialPortManager.StatusMessageChanged += OnStatusMessageChanged;
            _servoController.StatusMessageChanged += OnStatusMessageChanged;
            _servoController.PanServoStatusChanged += OnPanServoStatusChanged;
            _servoController.TiltServoStatusChanged += OnTiltServoStatusChanged;
        }

        private void ApplySettings()
        {
            // Apply always on top
            this.Topmost = _settings.AlwaysOnTop;
            if (AlwaysOnTopCheckBox != null)
            {
                AlwaysOnTopCheckBox.IsChecked = _settings.AlwaysOnTop;
            }

            // Apply axis inversions
            if (_servoController != null)
            {
                _servoController.InvertPanAxis = _settings.InvertPanAxis;
                _servoController.InvertTiltAxis = _settings.InvertTiltAxis;
            }

            // Update menu items
            if (InvertPanMenuItem != null)
            {
                InvertPanMenuItem.IsChecked = _settings.InvertPanAxis;
            }
            if (InvertTiltMenuItem != null)
            {
                InvertTiltMenuItem.IsChecked = _settings.InvertTiltAxis;
            }
        }

        private void SaveSettings()
        {
            _settings.Save();
        }



        private async Task AttemptAutoConnection()
        {
            if (_serialPortManager == null) return;

            await Task.Delay(1000); // Brief delay on startup
            
            UpdateStatus("Searching for USB-serial devices...");
            
            if (await _serialPortManager.AutoConnectAsync())
            {
                // Initialize servos after successful connection
                await _servoController?.InitializeServosAsync()!;
            }
        }

        #region Event Handlers

        private void OnConnectionStatusChanged(object? sender, bool connected)
        {
            Dispatcher.Invoke(() =>
            {
                UsbStatusIndicator.Fill = connected ? _connectedBrush : _disconnectedBrush;
                UsbStatusText.Text = connected ? 
                    $"USB: Connected ({_serialPortManager?.CurrentPortName})" : 
                    "USB: Disconnected";
                
                // Update servo status visibility and button states
                UpdateServoStatusVisibility();
                UpdateControlButtonStates();
            });
        }

        private void OnPanServoStatusChanged(object? sender, bool connected)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateServoStatusVisibility();
                PanStatusIndicator.Fill = connected ? _connectedBrush : _disconnectedBrush;
                PanStatusText.Text = connected ? 
                    "Pan Servo (ID 6): Connected" : 
                    "Pan Servo (ID 6): Disconnected";
                
                UpdateControlButtonStates();
            });
        }

        private void OnTiltServoStatusChanged(object? sender, bool connected)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateServoStatusVisibility();
                TiltStatusIndicator.Fill = connected ? _connectedBrush : _disconnectedBrush;
                TiltStatusText.Text = connected ? 
                    "Tilt Servo (ID 4): Connected" : 
                    "Tilt Servo (ID 4): Disconnected";
                
                UpdateControlButtonStates();
            });
        }

        private void OnStatusMessageChanged(object? sender, string message)
        {
            UpdateStatus(message);
        }

        #endregion

        #region Control Button Events

        private void PanLeftButton_Click(object sender, RoutedEventArgs e)
        {
            _servoController?.MovePanLeft();
        }

        private void PanRightButton_Click(object sender, RoutedEventArgs e)
        {
            _servoController?.MovePanRight();
        }

        private void TiltUpButton_Click(object sender, RoutedEventArgs e)
        {
            _servoController?.MoveTiltUp();
        }

        private void TiltDownButton_Click(object sender, RoutedEventArgs e)
        {
            _servoController?.MoveTiltDown();
        }

        private void CenterButton_Click(object sender, RoutedEventArgs e)
        {
            _servoController?.CenterCamera();
        }

        private async void ReconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_serialPortManager == null || _servoController == null) return;

            UpdateStatus("Reconnecting...");
            
            // Disconnect and reconnect
            _serialPortManager.Disconnect();
            await Task.Delay(500);
            
            if (await _serialPortManager.AutoConnectAsync())
            {
                await _servoController.InitializeServosAsync();
            }
        }

        #endregion

        #region Settings Events

        private void AlwaysOnTopCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            _settings.AlwaysOnTop = true;
            SaveSettings();
        }

        private void AlwaysOnTopCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.Topmost = false;
            _settings.AlwaysOnTop = false;
            SaveSettings();
        }



        #endregion

        #region Menu Events

        private void SetCenterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_servoController == null) return;

            var result = MessageBox.Show(
                "This will change the default center position of the servos.\n\n" +
                "If you click yes to continue, servo torque will be disabled so you can manually position the camera where you want the center to be, then click OK.\n\n" +
                "Continue?", 
                "Set Center Position", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Disable torque to allow manual positioning
                _servoController.DisableTorque();

                // Show instructions
                var positionResult = MessageBox.Show(
                    "Torque disabled. You can now manually move the camera.\n\n" +
                    "Position the camera at the desired center point, then click OK to save this position.\n\n" +
                    "Click Cancel to abort without saving.",
                    "Position Camera Manually",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Information);

                if (positionResult == MessageBoxResult.OK)
                {
                    // Calibrate at current position (writes to EEPROM)
                    _servoController.SetCenterPosition();
                }

                // Re-enable torque
                _servoController.EnableTorque();
            }
        }

        private void InvertPanMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_servoController != null)
            {
                _servoController.InvertPanAxis = InvertPanMenuItem.IsChecked;
                _settings.InvertPanAxis = InvertPanMenuItem.IsChecked;
                SaveSettings();
                UpdateStatus($"Pan axis inversion: {(InvertPanMenuItem.IsChecked ? "Enabled" : "Disabled")}");
            }
        }

        private void InvertTiltMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_servoController != null)
            {
                _servoController.InvertTiltAxis = InvertTiltMenuItem.IsChecked;
                _settings.InvertTiltAxis = InvertTiltMenuItem.IsChecked;
                SaveSettings();
                UpdateStatus($"Tilt axis inversion: {(InvertTiltMenuItem.IsChecked ? "Enabled" : "Disabled")}");
            }
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }

        #endregion

        #region UI Updates

        private void UpdateStatus(string message)
        {
            // Log to console instead of UI
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            Console.WriteLine($"[{timestamp}] {message}");
        }

        private void UpdateServoStatusVisibility()
        {
            var usbConnected = _serialPortManager?.IsConnected ?? false;
            var panConnected = _servoController?.IsPanServoConnected ?? false;
            var tiltConnected = _servoController?.IsTiltServoConnected ?? false;

            // Only show servo status if USB is connected AND at least one servo has issues
            var showPanStatus = usbConnected && !panConnected;
            var showTiltStatus = usbConnected && !tiltConnected;

            PanStatusIndicator.Visibility = showPanStatus ? Visibility.Visible : Visibility.Collapsed;
            PanStatusText.Visibility = showPanStatus ? Visibility.Visible : Visibility.Collapsed;
            
            TiltStatusIndicator.Visibility = showTiltStatus ? Visibility.Visible : Visibility.Collapsed;
            TiltStatusText.Visibility = showTiltStatus ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateControlButtonStates()
        {
            var panConnected = _servoController?.IsPanServoConnected ?? false;
            var tiltConnected = _servoController?.IsTiltServoConnected ?? false;
            var anyConnected = panConnected || tiltConnected;

            PanLeftButton.IsEnabled = panConnected;
            PanRightButton.IsEnabled = panConnected;
            TiltUpButton.IsEnabled = tiltConnected;
            TiltDownButton.IsEnabled = tiltConnected;
            CenterButton.IsEnabled = anyConnected;
        }



        #endregion

        #region Window Events

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Dispose();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_disposed)
            {
                _servoController?.Dispose();
                _serialPortManager?.Dispose();
                
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        ~MainWindow()
        {
            Dispose();
        }

        #endregion
    }
}