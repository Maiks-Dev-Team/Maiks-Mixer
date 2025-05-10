using System;
using System.Windows;
using System.Windows.Controls;
using MaiksMixer.UI.Controls;

namespace MaiksMixer.UI.Views
{
    /// <summary>
    /// Interaction logic for RoutingMatrixTestView.xaml
    /// </summary>
    public partial class RoutingMatrixTestView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the RoutingMatrixTestView class.
        /// </summary>
        public RoutingMatrixTestView()
        {
            InitializeComponent();
            
            // Add initial log entry
            LogEvent("Routing matrix initialized with sample data.");
        }
        
        /// <summary>
        /// Handles the ConnectionChanged event of the routing matrix.
        /// </summary>
        private void RoutingMatrix_ConnectionChanged(object sender, AudioConnectionEventArgs e)
        {
            // Log the connection change
            string statusText = e.Status switch
            {
                ConnectionStatus.Connected => "Connected",
                ConnectionStatus.Muted => "Muted",
                ConnectionStatus.Disconnected => "Disconnected",
                _ => "Unknown"
            };
            
            // Calculate volume in dB
            double volumeDb = 20 * Math.Log10(e.Volume);
            if (double.IsNegativeInfinity(volumeDb))
                volumeDb = -60;
            
            string logMessage = $"Connection changed: {e.SourceId} → {e.DestinationId}, Status: {statusText}, Volume: {volumeDb:F1} dB";
            LogEvent(logMessage);
            
            // In a real application, this would send a command to the audio engine to update the routing
            // For example:
            // var command = new CommandMessage("SetRoute", new
            // {
            //     sourceId = e.SourceId,
            //     destinationId = e.DestinationId,
            //     enabled = e.Status == ConnectionStatus.Connected,
            //     volume = e.Volume
            // });
            // await _communicationService.SendCommandAsync(command);
        }
        
        /// <summary>
        /// Logs an event to the event log text box.
        /// </summary>
        /// <param name="message">The message to log.</param>
        private void LogEvent(string message)
        {
            // Add timestamp
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string logEntry = $"[{timestamp}] {message}{Environment.NewLine}";
            
            // Add to log
            EventLogTextBox.AppendText(logEntry);
            
            // Scroll to end
            EventLogTextBox.ScrollToEnd();
        }
    }
}
