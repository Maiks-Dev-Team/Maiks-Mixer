using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MaiksMixer.UI.Controls
{
    /// <summary>
    /// Interaction logic for AudioLevelMeter.xaml
    /// </summary>
    public partial class AudioLevelMeter : UserControl
    {
        // Constants for meter scaling
        private const double MIN_DB = -60.0;
        private const double MAX_DB = 6.0;
        private const double PEAK_HOLD_TIME_MS = 1500;
        private const double PEAK_FALL_RATE_DB_SEC = 12.0;
        private const double RMS_FALL_RATE_DB_SEC = 20.0;
        
        // Animation-related fields
        private DateTime _lastUpdateTime = DateTime.Now;
        private double _currentPeakDb = MIN_DB;
        private double _currentRmsDb = MIN_DB;
        private double _peakHoldValue = MIN_DB;
        private DateTime _peakHoldTime = DateTime.MinValue;
        
        // Dependency properties
        public static readonly DependencyProperty PeakLevelProperty =
            DependencyProperty.Register("PeakLevel", typeof(double), typeof(AudioLevelMeter),
                new PropertyMetadata(-60.0, OnLevelChanged));
        
        public static readonly DependencyProperty RmsLevelProperty =
            DependencyProperty.Register("RmsLevel", typeof(double), typeof(AudioLevelMeter),
                new PropertyMetadata(-60.0, OnLevelChanged));
        
        public static readonly DependencyProperty TickIntervalProperty =
            DependencyProperty.Register("TickInterval", typeof(double), typeof(AudioLevelMeter),
                new PropertyMetadata(6.0, OnTickIntervalChanged));
        
        public static readonly DependencyProperty ShowTickLabelsProperty =
            DependencyProperty.Register("ShowTickLabels", typeof(bool), typeof(AudioLevelMeter),
                new PropertyMetadata(false, OnShowTickLabelsChanged));
        
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(AudioLevelMeter),
                new PropertyMetadata(Orientation.Vertical, OnOrientationChanged));
        
        /// <summary>
        /// Gets or sets the peak level in dB.
        /// </summary>
        public double PeakLevel
        {
            get { return (double)GetValue(PeakLevelProperty); }
            set { SetValue(PeakLevelProperty, value); }
        }
        
        /// <summary>
        /// Gets or sets the RMS level in dB.
        /// </summary>
        public double RmsLevel
        {
            get { return (double)GetValue(RmsLevelProperty); }
            set { SetValue(RmsLevelProperty, value); }
        }
        
        /// <summary>
        /// Gets or sets the tick interval in dB.
        /// </summary>
        public double TickInterval
        {
            get { return (double)GetValue(TickIntervalProperty); }
            set { SetValue(TickIntervalProperty, value); }
        }
        
        /// <summary>
        /// Gets or sets whether to show tick labels.
        /// </summary>
        public bool ShowTickLabels
        {
            get { return (bool)GetValue(ShowTickLabelsProperty); }
            set { SetValue(ShowTickLabelsProperty, value); }
        }
        
        /// <summary>
        /// Gets or sets the orientation of the meter.
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        
        /// <summary>
        /// Initializes a new instance of the AudioLevelMeter class.
        /// </summary>
        public AudioLevelMeter()
        {
            InitializeComponent();
            
            // Set up animation
            CompositionTarget.Rendering += OnCompositionTargetRendering;
            
            // Initialize the meter
            Loaded += (s, e) => InitializeMeter();
        }
        
        /// <summary>
        /// Initializes the meter.
        /// </summary>
        private void InitializeMeter()
        {
            // Create tick marks
            CreateTickMarks();
            
            // Set initial levels
            UpdateMeterDisplay(_currentPeakDb, _currentRmsDb);
        }
        
        /// <summary>
        /// Creates tick marks for the meter.
        /// </summary>
        private void CreateTickMarks()
        {
            TickMarksCanvas.Children.Clear();
            
            if (ActualHeight <= 0 || ActualWidth <= 0)
                return;
            
            double range = MAX_DB - MIN_DB;
            
            // Create tick marks at specified intervals
            for (double db = MIN_DB; db <= MAX_DB; db += TickInterval)
            {
                // Skip the very bottom tick if it's at the minimum
                if (Math.Abs(db - MIN_DB) < 0.01)
                    continue;
                
                // Calculate position
                double position = 1.0 - (db - MIN_DB) / range;
                
                // Create tick mark
                Line tick = new Line();
                tick.Stroke = new SolidColorBrush(Colors.Gray);
                tick.StrokeThickness = 1;
                
                if (Orientation == Orientation.Vertical)
                {
                    double y = position * ActualHeight;
                    tick.X1 = 0;
                    tick.Y1 = y;
                    tick.X2 = ActualWidth;
                    tick.Y2 = y;
                }
                else
                {
                    double x = position * ActualWidth;
                    tick.X1 = x;
                    tick.Y1 = 0;
                    tick.X2 = x;
                    tick.Y2 = ActualHeight;
                }
                
                TickMarksCanvas.Children.Add(tick);
                
                // Add label if enabled
                if (ShowTickLabels)
                {
                    TextBlock label = new TextBlock();
                    label.Text = db.ToString("+#;-#;0");
                    label.Foreground = new SolidColorBrush(Colors.LightGray);
                    label.FontSize = 8;
                    
                    if (Orientation == Orientation.Vertical)
                    {
                        Canvas.SetLeft(label, ActualWidth + 2);
                        Canvas.SetTop(label, y - 6);
                    }
                    else
                    {
                        Canvas.SetLeft(label, x - 8);
                        Canvas.SetTop(label, ActualHeight + 2);
                    }
                    
                    TickMarksCanvas.Children.Add(label);
                }
            }
        }
        
        /// <summary>
        /// Handles the CompositionTarget.Rendering event.
        /// </summary>
        private void OnCompositionTargetRendering(object? sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            double deltaTime = (now - _lastUpdateTime).TotalSeconds;
            _lastUpdateTime = now;
            
            if (deltaTime <= 0 || deltaTime > 1.0)
                return;
            
            // Apply fall rates
            bool changed = false;
            
            // Handle peak hold
            if (_peakHoldTime != DateTime.MinValue && (now - _peakHoldTime).TotalMilliseconds > PEAK_HOLD_TIME_MS)
            {
                _peakHoldValue = Math.Max(_peakHoldValue - PEAK_FALL_RATE_DB_SEC * deltaTime, _currentPeakDb);
                
                if (_peakHoldValue <= _currentPeakDb)
                {
                    _peakHoldTime = DateTime.MinValue;
                    _peakHoldValue = _currentPeakDb;
                }
                
                changed = true;
            }
            
            // Apply fall rates to current levels
            double targetPeakDb = PeakLevel;
            double targetRmsDb = RmsLevel;
            
            if (_currentPeakDb > targetPeakDb)
            {
                _currentPeakDb = Math.Max(_currentPeakDb - PEAK_FALL_RATE_DB_SEC * deltaTime, targetPeakDb);
                changed = true;
            }
            else if (_currentPeakDb < targetPeakDb)
            {
                _currentPeakDb = targetPeakDb;
                
                // Update peak hold if new peak is higher
                if (_currentPeakDb > _peakHoldValue)
                {
                    _peakHoldValue = _currentPeakDb;
                    _peakHoldTime = now;
                }
                
                changed = true;
            }
            
            if (_currentRmsDb > targetRmsDb)
            {
                _currentRmsDb = Math.Max(_currentRmsDb - RMS_FALL_RATE_DB_SEC * deltaTime, targetRmsDb);
                changed = true;
            }
            else if (_currentRmsDb < targetRmsDb)
            {
                _currentRmsDb = targetRmsDb;
                changed = true;
            }
            
            // Update display if needed
            if (changed)
            {
                UpdateMeterDisplay(_currentPeakDb, _currentRmsDb);
            }
        }
        
        /// <summary>
        /// Updates the meter display.
        /// </summary>
        /// <param name="peakDb">The peak level in dB.</param>
        /// <param name="rmsDb">The RMS level in dB.</param>
        private void UpdateMeterDisplay(double peakDb, double rmsDb)
        {
            // Clamp values to range
            peakDb = Math.Max(MIN_DB, Math.Min(MAX_DB, peakDb));
            rmsDb = Math.Max(MIN_DB, Math.Min(MAX_DB, rmsDb));
            
            double range = MAX_DB - MIN_DB;
            double peakPosition = (peakDb - MIN_DB) / range;
            double rmsPosition = (rmsDb - MIN_DB) / range;
            double peakHoldPosition = (_peakHoldValue - MIN_DB) / range;
            
            if (Orientation == Orientation.Vertical)
            {
                // Update RMS level rectangle
                RmsLevelRect.Height = rmsPosition * ActualHeight;
                
                // Update peak indicator
                if (peakHoldPosition > 0.001)
                {
                    PeakIndicator.Visibility = Visibility.Visible;
                    Canvas.SetTop(PeakIndicator, ActualHeight * (1.0 - peakHoldPosition));
                    PeakIndicator.Width = ActualWidth;
                }
                else
                {
                    PeakIndicator.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                // Update RMS level rectangle
                RmsLevelRect.Width = rmsPosition * ActualWidth;
                
                // Update peak indicator
                if (peakHoldPosition > 0.001)
                {
                    PeakIndicator.Visibility = Visibility.Visible;
                    Canvas.SetLeft(PeakIndicator, ActualWidth * peakHoldPosition);
                    PeakIndicator.Height = ActualHeight;
                }
                else
                {
                    PeakIndicator.Visibility = Visibility.Collapsed;
                }
            }
        }
        
        /// <summary>
        /// Handles changes to the level properties.
        /// </summary>
        private static void OnLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // No need to do anything here, as the rendering event will handle updates
        }
        
        /// <summary>
        /// Handles changes to the tick interval property.
        /// </summary>
        private static void OnTickIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioLevelMeter meter)
            {
                meter.CreateTickMarks();
            }
        }
        
        /// <summary>
        /// Handles changes to the show tick labels property.
        /// </summary>
        private static void OnShowTickLabelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioLevelMeter meter)
            {
                meter.CreateTickMarks();
            }
        }
        
        /// <summary>
        /// Handles changes to the orientation property.
        /// </summary>
        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioLevelMeter meter)
            {
                meter.UpdateLayout();
                meter.CreateTickMarks();
                
                // Adjust the layout based on orientation
                if ((Orientation)e.NewValue == Orientation.Horizontal)
                {
                    // Horizontal orientation
                    meter.RmsLevelRect.VerticalAlignment = VerticalAlignment.Stretch;
                    meter.RmsLevelRect.HorizontalAlignment = HorizontalAlignment.Left;
                    
                    meter.PeakIndicator.VerticalAlignment = VerticalAlignment.Stretch;
                    meter.PeakIndicator.HorizontalAlignment = HorizontalAlignment.Left;
                    meter.PeakIndicator.Width = 2;
                    meter.PeakIndicator.Height = double.NaN;
                }
                else
                {
                    // Vertical orientation
                    meter.RmsLevelRect.VerticalAlignment = VerticalAlignment.Bottom;
                    meter.RmsLevelRect.HorizontalAlignment = HorizontalAlignment.Stretch;
                    
                    meter.PeakIndicator.VerticalAlignment = VerticalAlignment.Top;
                    meter.PeakIndicator.HorizontalAlignment = HorizontalAlignment.Stretch;
                    meter.PeakIndicator.Height = 2;
                    meter.PeakIndicator.Width = double.NaN;
                }
                
                meter.UpdateMeterDisplay(meter._currentPeakDb, meter._currentRmsDb);
            }
        }
        
        /// <summary>
        /// Handles the SizeChanged event.
        /// </summary>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            CreateTickMarks();
            UpdateMeterDisplay(_currentPeakDb, _currentRmsDb);
        }
    }
}
