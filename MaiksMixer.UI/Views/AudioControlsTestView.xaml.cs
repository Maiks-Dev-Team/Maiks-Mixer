using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MaiksMixer.UI.Views
{
    /// <summary>
    /// Interaction logic for AudioControlsTestView.xaml
    /// </summary>
    public partial class AudioControlsTestView : UserControl
    {
        private readonly DispatcherTimer _levelUpdateTimer;
        private readonly Random _random = new Random();
        private bool _isLinkingFaders = false;
        
        /// <summary>
        /// Initializes a new instance of the AudioControlsTestView class.
        /// </summary>
        public AudioControlsTestView()
        {
            InitializeComponent();
            
            // Create a timer to simulate audio level changes
            _levelUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _levelUpdateTimer.Tick += LevelUpdateTimer_Tick;
            
            // Start the timer when the control is loaded
            Loaded += (s, e) => _levelUpdateTimer.Start();
            
            // Stop the timer when the control is unloaded
            Unloaded += (s, e) => _levelUpdateTimer.Stop();
            
            // Set up fader event handlers
            SetupFaderEventHandlers();
        }
        
        /// <summary>
        /// Sets up event handlers for the faders.
        /// </summary>
        private void SetupFaderEventHandlers()
        {
            VerticalFader1.ValueChanged += Fader_ValueChanged;
            VerticalFader2.ValueChanged += Fader_ValueChanged;
            VerticalFader3.ValueChanged += Fader_ValueChanged;
            HorizontalFader1.ValueChanged += Fader_ValueChanged;
            HorizontalFader2.ValueChanged += Fader_ValueChanged;
            HorizontalFader3.ValueChanged += Fader_ValueChanged;
        }
        
        /// <summary>
        /// Handles the Tick event of the level update timer.
        /// </summary>
        private void LevelUpdateTimer_Tick(object? sender, EventArgs e)
        {
            // Get the current peak and RMS levels from the sliders
            double basePeakLevel = PeakLevelSlider.Value;
            double baseRmsLevel = RmsLevelSlider.Value;
            
            // Add some random variation to simulate real audio
            double peakVariation = _random.NextDouble() * 6.0 - 3.0; // +/- 3 dB
            double rmsVariation = _random.NextDouble() * 4.0 - 2.0;  // +/- 2 dB
            
            // Update the vertical meters
            UpdateMeter(VerticalMeter1, basePeakLevel + peakVariation, baseRmsLevel + rmsVariation);
            UpdateMeter(VerticalMeter2, basePeakLevel + peakVariation * 0.8, baseRmsLevel + rmsVariation * 0.8);
            UpdateMeter(VerticalMeter3, basePeakLevel + peakVariation * 0.6, baseRmsLevel + rmsVariation * 0.6);
            
            // Update the horizontal meters
            UpdateMeter(HorizontalMeter1, basePeakLevel + peakVariation, baseRmsLevel + rmsVariation);
            UpdateMeter(HorizontalMeter2, basePeakLevel + peakVariation * 0.8, baseRmsLevel + rmsVariation * 0.8);
            UpdateMeter(HorizontalMeter3, basePeakLevel + peakVariation * 0.6, baseRmsLevel + rmsVariation * 0.6);
        }
        
        /// <summary>
        /// Updates a meter with the specified peak and RMS levels.
        /// </summary>
        /// <param name="meter">The meter to update.</param>
        /// <param name="peakLevel">The peak level in dB.</param>
        /// <param name="rmsLevel">The RMS level in dB.</param>
        private void UpdateMeter(Controls.AudioLevelMeter meter, double peakLevel, double rmsLevel)
        {
            // Clamp values to the meter's range
            peakLevel = Math.Max(-60.0, Math.Min(6.0, peakLevel));
            rmsLevel = Math.Max(-60.0, Math.Min(6.0, rmsLevel));
            
            // Ensure RMS level is not higher than peak level
            rmsLevel = Math.Min(rmsLevel, peakLevel);
            
            // Update the meter
            meter.PeakLevel = peakLevel;
            meter.RmsLevel = rmsLevel;
        }
        
        /// <summary>
        /// Handles the ValueChanged event of the peak level slider.
        /// </summary>
        private void PeakLevelSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Ensure RMS level is not higher than peak level
            if (RmsLevelSlider.Value > e.NewValue)
            {
                RmsLevelSlider.Value = e.NewValue;
            }
        }
        
        /// <summary>
        /// Handles the ValueChanged event of the RMS level slider.
        /// </summary>
        private void RmsLevelSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Ensure RMS level is not higher than peak level
            if (e.NewValue > PeakLevelSlider.Value)
            {
                RmsLevelSlider.Value = PeakLevelSlider.Value;
            }
        }
        
        /// <summary>
        /// Handles the ValueChanged event of the faders.
        /// </summary>
        private void Fader_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isLinkingFaders || !(sender is Controls.AudioFader sourceFader))
                return;
            
            // Update all other faders to match the value of the changed fader
            _isLinkingFaders = false; // Prevent recursion
            
            if (sender != VerticalFader1) VerticalFader1.Value = e.NewValue;
            if (sender != VerticalFader2) VerticalFader2.Value = e.NewValue;
            if (sender != VerticalFader3) VerticalFader3.Value = e.NewValue;
            if (sender != HorizontalFader1) HorizontalFader1.Value = e.NewValue;
            if (sender != HorizontalFader2) HorizontalFader2.Value = e.NewValue;
            if (sender != HorizontalFader3) HorizontalFader3.Value = e.NewValue;
            
            _isLinkingFaders = true;
        }
        
        /// <summary>
        /// Handles the Checked event of the link faders checkbox.
        /// </summary>
        private void LinkFadersCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _isLinkingFaders = true;
            
            // Set all faders to the value of the first fader
            double value = VerticalFader1.Value;
            VerticalFader2.Value = value;
            VerticalFader3.Value = value;
            HorizontalFader1.Value = value;
            HorizontalFader2.Value = value;
            HorizontalFader3.Value = value;
        }
        
        /// <summary>
        /// Handles the Unchecked event of the link faders checkbox.
        /// </summary>
        private void LinkFadersCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _isLinkingFaders = false;
        }
    }
}
