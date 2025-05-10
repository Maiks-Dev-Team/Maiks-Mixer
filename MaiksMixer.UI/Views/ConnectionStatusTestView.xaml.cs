using System;
using System.Windows;
using System.Windows.Controls;
using MaiksMixer.UI.Controls;

namespace MaiksMixer.UI.Views
{
    /// <summary>
    /// Interaction logic for ConnectionStatusTestView.xaml
    /// </summary>
    public partial class ConnectionStatusTestView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the ConnectionStatusTestView class.
        /// </summary>
        public ConnectionStatusTestView()
        {
            InitializeComponent();
            
            // Add initial log entry
            LogEvent("Connection status indicator initialized.");
        }
        
        /// <summary>
        /// Handles the ReconnectRequested event of the connection status indicator.
        /// </summary>
        private void ConnectionStatusIndicator_ReconnectRequested(object sender, EventArgs e)
        {
            // Log the event
            LogEvent($"Reconnect requested. Current status: {ConnectionStatusIndicator.Status}");
            
            // In a real application, this would trigger a connection attempt to the audio engine
            // For example:
            // if (ConnectionStatusIndicator.Status == ConnectionStatus.Disconnected)
            // {
            //     _communicationService.Connect();
            // }
            // else if (ConnectionStatusIndicator.Status == ConnectionStatus.Connected)
            // {
            //     _communicationService.Disconnect();
            // }
        }
        
        /// <summary>
        /// Handles the Click event of the connected button.
        /// </summary>
        private void ConnectedButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectionStatusIndicator.SetStatus(ConnectionStatus.Connected);
            LogEvent("Set status to Connected.");
        }
        
        /// <summary>
        /// Handles the Click event of the disconnected button.
        /// </summary>
        private void DisconnectedButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectionStatusIndicator.SetStatus(ConnectionStatus.Disconnected);
            LogEvent("Set status to Disconnected.");
        }
        
        /// <summary>
        /// Handles the Click event of the connecting button.
        /// </summary>
        private void ConnectingButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectionStatusIndicator.SetStatus(ConnectionStatus.Connecting);
            LogEvent("Set status to Connecting...");
        }
        
        /// <summary>
        /// Handles the Click event of the error button.
        /// </summary>
        private void ErrorButton_Click(object sender, RoutedEventArgs e)
        {
            string errorMessage = ErrorMessageTextBox.Text;
            ConnectionStatusIndicator.SetStatus(ConnectionStatus.Error, errorMessage);
            LogEvent($"Set status to Error with message: {errorMessage}");
        }
        
        /// <summary>
        /// Logs an event to the event log text block.
        /// </summary>
        /// <param name="message">The message to log.</param>
        private void LogEvent(string message)
        {
            // Add timestamp
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string logEntry = $"[{timestamp}] {message}";
            
            // Add to log
            EventLogTextBlock.Text = logEntry + Environment.NewLine + EventLogTextBlock.Text;
            
            // Limit log length
            if (EventLogTextBlock.Text.Length > 1000)
            {
                EventLogTextBlock.Text = EventLogTextBlock.Text.Substring(0, 1000) + "...";
            }
        }
    }
}
