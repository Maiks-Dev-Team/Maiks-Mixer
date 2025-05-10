using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MaiksMixer.UI.Controls
{
    /// <summary>
    /// Interaction logic for ChannelStrip.xaml
    /// </summary>
    public partial class ChannelStrip : UserControl
    {
        private bool _isMuted = false;
        private bool _isSoloed = false;
        private double _volume = 0.0; // dB
        private double _pan = 0.0; // -1 (left) to 1 (right)
        private string _channelName = "Channel";
        
        #region Events
        
        /// <summary>
        /// Event raised when the volume changes.
        /// </summary>
        public event EventHandler<ChannelPropertyChangedEventArgs<double>>? VolumeChanged;
        
        /// <summary>
        /// Event raised when the pan changes.
        /// </summary>
        public event EventHandler<ChannelPropertyChangedEventArgs<double>>? PanChanged;
        
        /// <summary>
        /// Event raised when the mute state changes.
        /// </summary>
        public event EventHandler<ChannelPropertyChangedEventArgs<bool>>? MuteChanged;
        
        /// <summary>
        /// Event raised when the solo state changes.
        /// </summary>
        public event EventHandler<ChannelPropertyChangedEventArgs<bool>>? SoloChanged;
        
        /// <summary>
        /// Event raised when the settings button is clicked.
        /// </summary>
        public event EventHandler? SettingsRequested;
        
        /// <summary>
        /// Event raised when the routing button is clicked.
        /// </summary>
        public event EventHandler? RoutingRequested;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Gets or sets the channel ID.
        /// </summary>
        public string ChannelId { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Gets or sets the channel name.
        /// </summary>
        public string ChannelName
        {
            get => _channelName;
            set
            {
                _channelName = value;
                ChannelLabel.Text = value;
            }
        }
        
        /// <summary>
        /// Gets or sets the volume in dB.
        /// </summary>
        public double Volume
        {
            get => _volume;
            set
            {
                // Clamp value between min and max
                value = Math.Max(VolumeFader.Minimum, Math.Min(VolumeFader.Maximum, value));
                
                if (_volume != value)
                {
                    _volume = value;
                    VolumeFader.Value = value;
                    UpdateVolumeDisplay();
                    
                    // Raise event
                    VolumeChanged?.Invoke(this, new ChannelPropertyChangedEventArgs<double>(ChannelId, value));
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the pan value (-1 for left, 0 for center, 1 for right).
        /// </summary>
        public double Pan
        {
            get => _pan;
            set
            {
                // Clamp value between -1 and 1
                value = Math.Max(-1, Math.Min(1, value));
                
                if (_pan != value)
                {
                    _pan = value;
                    PanSlider.Value = value;
                    UpdatePanDisplay();
                    
                    // Raise event
                    PanChanged?.Invoke(this, new ChannelPropertyChangedEventArgs<double>(ChannelId, value));
                }
            }
        }
        
        /// <summary>
        /// Gets or sets whether the channel is muted.
        /// </summary>
        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                if (_isMuted != value)
                {
                    _isMuted = value;
                    MuteButton.Tag = value ? "Active" : null;
                    
                    // Raise event
                    MuteChanged?.Invoke(this, new ChannelPropertyChangedEventArgs<bool>(ChannelId, value));
                }
            }
        }
        
        /// <summary>
        /// Gets or sets whether the channel is soloed.
        /// </summary>
        public bool IsSoloed
        {
            get => _isSoloed;
            set
            {
                if (_isSoloed != value)
                {
                    _isSoloed = value;
                    SoloButton.Tag = value ? "Active" : null;
                    
                    // Raise event
                    SoloChanged?.Invoke(this, new ChannelPropertyChangedEventArgs<bool>(ChannelId, value));
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the peak level for the meter.
        /// </summary>
        public double PeakLevel
        {
            get => LevelMeter.PeakLevel;
            set => LevelMeter.PeakLevel = value;
        }
        
        /// <summary>
        /// Gets or sets the RMS level for the meter.
        /// </summary>
        public double RmsLevel
        {
            get => LevelMeter.RmsLevel;
            set => LevelMeter.RmsLevel = value;
        }
        
        #endregion
        
        /// <summary>
        /// Initializes a new instance of the ChannelStrip class.
        /// </summary>
        public ChannelStrip()
        {
            InitializeComponent();
            
            // Initialize UI
            UpdateVolumeDisplay();
            UpdatePanDisplay();
        }
        
        #region Event Handlers
        
        /// <summary>
        /// Handles the ValueChanged event of the volume fader.
        /// </summary>
        private void VolumeFader_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Volume = e.NewValue;
        }
        
        /// <summary>
        /// Handles the ValueChanged event of the pan slider.
        /// </summary>
        private void PanSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Pan = e.NewValue;
        }
        
        /// <summary>
        /// Handles the Click event of the mute button.
        /// </summary>
        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            IsMuted = !IsMuted;
        }
        
        /// <summary>
        /// Handles the Click event of the solo button.
        /// </summary>
        private void SoloButton_Click(object sender, RoutedEventArgs e)
        {
            IsSoloed = !IsSoloed;
        }
        
        /// <summary>
        /// Handles the Click event of the settings button.
        /// </summary>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsRequested?.Invoke(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// Handles the Click event of the routing button.
        /// </summary>
        private void RoutingButton_Click(object sender, RoutedEventArgs e)
        {
            RoutingRequested?.Invoke(this, EventArgs.Empty);
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Updates the volume display.
        /// </summary>
        private void UpdateVolumeDisplay()
        {
            // Format volume as dB with one decimal place
            VolumeDisplay.Text = $"{_volume:F1} dB";
            
            // If muted, show in red
            VolumeDisplay.Foreground = IsMuted ? Brushes.Red : Brushes.LightGray;
        }
        
        /// <summary>
        /// Updates the pan display.
        /// </summary>
        private void UpdatePanDisplay()
        {
            // Format pan display
            if (Math.Abs(_pan) < 0.05)
            {
                PanDisplay.Text = "C"; // Center
            }
            else if (_pan < 0)
            {
                int percent = (int)Math.Abs(_pan * 100);
                PanDisplay.Text = $"L {percent}%";
            }
            else
            {
                int percent = (int)(_pan * 100);
                PanDisplay.Text = $"R {percent}%";
            }
        }
        
        /// <summary>
        /// Updates the audio levels.
        /// </summary>
        /// <param name="peakLevel">The peak level in dB.</param>
        /// <param name="rmsLevel">The RMS level in dB.</param>
        public void UpdateLevels(double peakLevel, double rmsLevel)
        {
            LevelMeter.PeakLevel = peakLevel;
            LevelMeter.RmsLevel = rmsLevel;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Event arguments for channel property changes.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    public class ChannelPropertyChangedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the channel ID.
        /// </summary>
        public string ChannelId { get; }
        
        /// <summary>
        /// Gets the new property value.
        /// </summary>
        public T Value { get; }
        
        /// <summary>
        /// Initializes a new instance of the ChannelPropertyChangedEventArgs class.
        /// </summary>
        /// <param name="channelId">The channel ID.</param>
        /// <param name="value">The new property value.</param>
        public ChannelPropertyChangedEventArgs(string channelId, T value)
        {
            ChannelId = channelId;
            Value = value;
        }
    }
}
