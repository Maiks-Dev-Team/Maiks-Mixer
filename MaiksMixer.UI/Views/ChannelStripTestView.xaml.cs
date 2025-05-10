using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MaiksMixer.UI.Controls;

namespace MaiksMixer.UI.Views
{
    /// <summary>
    /// Interaction logic for ChannelStripTestView.xaml
    /// </summary>
    public partial class ChannelStripTestView : UserControl
    {
        private readonly DispatcherTimer _audioSimulationTimer;
        private readonly Random _random = new Random();
        private int _channelCounter = 5; // Start at 5 since we already have 4 channels
        private double _simulationLevel = -20.0;
        
        /// <summary>
        /// Initializes a new instance of the ChannelStripTestView class.
        /// </summary>
        public ChannelStripTestView()
        {
            InitializeComponent();
            
            // Initialize audio simulation timer
            _audioSimulationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // Update 10 times per second
            };
            _audioSimulationTimer.Tick += AudioSimulationTimer_Tick;
            
            // Add initial log entry
            LogEvent("Channel strip test view initialized.");
            LogEvent("Tip: Toggle 'Simulate Audio' to see dynamic level meters in action.");
        }
        
        #region Event Handlers
        
        /// <summary>
        /// Handles the VolumeChanged event of a channel.
        /// </summary>
        private void Channel_VolumeChanged(object sender, ChannelPropertyChangedEventArgs<double> e)
        {
            if (sender is ChannelStrip channel)
            {
                LogEvent($"Volume changed on {channel.ChannelName} ({e.ChannelId}): {e.Value:F1} dB");
            }
        }
        
        /// <summary>
        /// Handles the PanChanged event of a channel.
        /// </summary>
        private void Channel_PanChanged(object sender, ChannelPropertyChangedEventArgs<double> e)
        {
            if (sender is ChannelStrip channel)
            {
                string panText;
                if (Math.Abs(e.Value) < 0.05)
                {
                    panText = "Center";
                }
                else if (e.Value < 0)
                {
                    int percent = (int)Math.Abs(e.Value * 100);
                    panText = $"Left {percent}%";
                }
                else
                {
                    int percent = (int)(e.Value * 100);
                    panText = $"Right {percent}%";
                }
                
                LogEvent($"Pan changed on {channel.ChannelName} ({e.ChannelId}): {panText}");
            }
        }
        
        /// <summary>
        /// Handles the MuteChanged event of a channel.
        /// </summary>
        private void Channel_MuteChanged(object sender, ChannelPropertyChangedEventArgs<bool> e)
        {
            if (sender is ChannelStrip channel)
            {
                LogEvent($"Mute {(e.Value ? "enabled" : "disabled")} on {channel.ChannelName} ({e.ChannelId})");
            }
        }
        
        /// <summary>
        /// Handles the SoloChanged event of a channel.
        /// </summary>
        private void Channel_SoloChanged(object sender, ChannelPropertyChangedEventArgs<bool> e)
        {
            if (sender is ChannelStrip channel)
            {
                LogEvent($"Solo {(e.Value ? "enabled" : "disabled")} on {channel.ChannelName} ({e.ChannelId})");
                
                // In a real application, we would solo this channel and mute all others
                if (e.Value)
                {
                    LogEvent($"  (Other channels would be automatically muted)");
                }
            }
        }
        
        /// <summary>
        /// Handles the SettingsRequested event of a channel.
        /// </summary>
        private void Channel_SettingsRequested(object sender, EventArgs e)
        {
            if (sender is ChannelStrip channel)
            {
                LogEvent($"Settings requested for {channel.ChannelName} ({channel.ChannelId})");
                
                // In a real application, this would open a settings dialog
                LogEvent($"  (Would open settings dialog for this channel)");
            }
        }
        
        /// <summary>
        /// Handles the RoutingRequested event of a channel.
        /// </summary>
        private void Channel_RoutingRequested(object sender, EventArgs e)
        {
            if (sender is ChannelStrip channel)
            {
                LogEvent($"Routing requested for {channel.ChannelName} ({channel.ChannelId})");
                
                // In a real application, this would open the routing matrix
                LogEvent($"  (Would open routing matrix focused on this channel)");
            }
        }
        
        /// <summary>
        /// Handles the Checked event of the simulate audio toggle button.
        /// </summary>
        private void SimulateAudioToggle_Checked(object sender, RoutedEventArgs e)
        {
            _audioSimulationTimer.Start();
            LogEvent("Audio simulation started.");
        }
        
        /// <summary>
        /// Handles the Unchecked event of the simulate audio toggle button.
        /// </summary>
        private void SimulateAudioToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            _audioSimulationTimer.Stop();
            LogEvent("Audio simulation stopped.");
            
            // Reset all meters to minimum
            foreach (var child in ChannelStripContainer.Children)
            {
                if (child is ChannelStrip channel)
                {
                    channel.UpdateLevels(-60, -60);
                }
            }
        }
        
        /// <summary>
        /// Handles the ValueChanged event of the simulation level slider.
        /// </summary>
        private void SimulationLevelSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _simulationLevel = e.NewValue;
        }
        
        /// <summary>
        /// Handles the Click event of the add channel button.
        /// </summary>
        private void AddChannel_Click(object sender, RoutedEventArgs e)
        {
            string channelName = $"Channel {_channelCounter++}";
            
            // Create new channel strip
            var channelStrip = new ChannelStrip
            {
                ChannelName = channelName,
                ChannelId = Guid.NewGuid().ToString(),
                Margin = new Thickness(5, 0, 5, 0)
            };
            
            // Add event handlers
            channelStrip.VolumeChanged += Channel_VolumeChanged;
            channelStrip.PanChanged += Channel_PanChanged;
            channelStrip.MuteChanged += Channel_MuteChanged;
            channelStrip.SoloChanged += Channel_SoloChanged;
            channelStrip.SettingsRequested += Channel_SettingsRequested;
            channelStrip.RoutingRequested += Channel_RoutingRequested;
            
            // Add to container
            ChannelStripContainer.Children.Add(channelStrip);
            
            LogEvent($"Added new channel: {channelName}");
        }
        
        /// <summary>
        /// Handles the Tick event of the audio simulation timer.
        /// </summary>
        private void AudioSimulationTimer_Tick(object sender, EventArgs e)
        {
            // Update all channel meters with simulated audio levels
            foreach (var child in ChannelStripContainer.Children)
            {
                if (child is ChannelStrip channel)
                {
                    // Skip muted channels
                    if (channel.IsMuted)
                    {
                        channel.UpdateLevels(-60, -60);
                        continue;
                    }
                    
                    // Generate random audio levels based on the simulation level
                    double baseLevel = _simulationLevel;
                    
                    // Add some randomness to make it look more realistic
                    double peakVariation = _random.NextDouble() * 10.0;
                    double rmsVariation = _random.NextDouble() * 6.0;
                    
                    double peakLevel = baseLevel + peakVariation;
                    double rmsLevel = baseLevel - rmsVariation;
                    
                    // Ensure RMS is always lower than peak
                    rmsLevel = Math.Min(rmsLevel, peakLevel - 3);
                    
                    // Apply channel volume
                    peakLevel += channel.Volume;
                    rmsLevel += channel.Volume;
                    
                    // Clamp values
                    peakLevel = Math.Max(-60, Math.Min(0, peakLevel));
                    rmsLevel = Math.Max(-60, Math.Min(0, rmsLevel));
                    
                    // Update channel meter
                    channel.UpdateLevels(peakLevel, rmsLevel);
                }
            }
        }
        
        #endregion
        
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
            
            // Limit log length
            if (EventLogTextBox.Text.Length > 5000)
            {
                EventLogTextBox.Text = EventLogTextBox.Text.Substring(0, 5000) + "...";
            }
        }
    }
}
