using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MaiksMixer.UI.Controls
{
    /// <summary>
    /// Interaction logic for ConnectionStatusIndicator.xaml
    /// </summary>
    public partial class ConnectionStatusIndicator : UserControl
    {
        private Storyboard? _pulseAnimation;
        private ConnectionStatus _currentStatus = ConnectionStatus.Disconnected;
        
        /// <summary>
        /// Event raised when the reconnect button is clicked.
        /// </summary>
        public event EventHandler? ReconnectRequested;
        
        /// <summary>
        /// Gets the current connection status.
        /// </summary>
        public ConnectionStatus Status => _currentStatus;
        
        /// <summary>
        /// Initializes a new instance of the ConnectionStatusIndicator class.
        /// </summary>
        public ConnectionStatusIndicator()
        {
            InitializeComponent();
            
            // Get the pulse animation
            _pulseAnimation = (Storyboard)Resources["PulseAnimation"];
            
            // Set initial status
            SetStatus(ConnectionStatus.Disconnected);
            
            // Start animation if connecting
            if (_currentStatus == ConnectionStatus.Connecting)
            {
                _pulseAnimation?.Begin();
            }
        }
        
        /// <summary>
        /// Sets the connection status.
        /// </summary>
        /// <param name="status">The new status.</param>
        /// <param name="errorMessage">Optional error message for error status.</param>
        public void SetStatus(ConnectionStatus status, string? errorMessage = null)
        {
            _currentStatus = status;
            
            // Update UI based on status
            switch (status)
            {
                case ConnectionStatus.Connected:
                    StatusEllipse.Fill = (SolidColorBrush)Resources["ConnectedBrush"];
                    StatusTextBlock.Text = "Connected";
                    ReconnectButton.Content = "Disconnect";
                    ReconnectButton.IsEnabled = true;
                    StopAnimation();
                    break;
                
                case ConnectionStatus.Disconnected:
                    StatusEllipse.Fill = (SolidColorBrush)Resources["DisconnectedBrush"];
                    StatusTextBlock.Text = "Disconnected";
                    ReconnectButton.Content = "Connect";
                    ReconnectButton.IsEnabled = true;
                    StopAnimation();
                    break;
                
                case ConnectionStatus.Connecting:
                    StatusEllipse.Fill = (SolidColorBrush)Resources["ConnectingBrush"];
                    StatusTextBlock.Text = "Connecting...";
                    ReconnectButton.Content = "Cancel";
                    ReconnectButton.IsEnabled = true;
                    StartAnimation();
                    break;
                
                case ConnectionStatus.Error:
                    StatusEllipse.Fill = (SolidColorBrush)Resources["ErrorBrush"];
                    StatusTextBlock.Text = string.IsNullOrEmpty(errorMessage) ? "Connection Error" : errorMessage;
                    ReconnectButton.Content = "Retry";
                    ReconnectButton.IsEnabled = true;
                    StopAnimation();
                    break;
            }
        }
        
        /// <summary>
        /// Starts the pulse animation.
        /// </summary>
        private void StartAnimation()
        {
            _pulseAnimation?.Begin();
        }
        
        /// <summary>
        /// Stops the pulse animation.
        /// </summary>
        private void StopAnimation()
        {
            _pulseAnimation?.Stop();
            StatusEllipse.Opacity = 1.0;
        }
        
        /// <summary>
        /// Handles the Click event of the reconnect button.
        /// </summary>
        private void ReconnectButton_Click(object sender, RoutedEventArgs e)
        {
            // Raise the reconnect requested event
            ReconnectRequested?.Invoke(this, EventArgs.Empty);
            
            // Update UI based on current status
            switch (_currentStatus)
            {
                case ConnectionStatus.Connected:
                    // If connected, show disconnecting
                    SetStatus(ConnectionStatus.Disconnected);
                    break;
                
                case ConnectionStatus.Disconnected:
                case ConnectionStatus.Error:
                    // If disconnected or error, show connecting
                    SetStatus(ConnectionStatus.Connecting);
                    break;
                
                case ConnectionStatus.Connecting:
                    // If connecting, cancel and show disconnected
                    SetStatus(ConnectionStatus.Disconnected);
                    break;
            }
        }
    }
    
    /// <summary>
    /// Represents the status of a connection.
    /// </summary>
    public enum ConnectionStatus
    {
        /// <summary>
        /// Connected.
        /// </summary>
        Connected,
        
        /// <summary>
        /// Disconnected.
        /// </summary>
        Disconnected,
        
        /// <summary>
        /// Connecting.
        /// </summary>
        Connecting,
        
        /// <summary>
        /// Error.
        /// </summary>
        Error
    }
}
