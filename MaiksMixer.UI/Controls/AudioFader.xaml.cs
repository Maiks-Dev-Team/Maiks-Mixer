using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MaiksMixer.UI.Controls
{
    /// <summary>
    /// Interaction logic for AudioFader.xaml
    /// </summary>
    public partial class AudioFader : UserControl
    {
        // Constants for dB conversion
        private const double MIN_DB = -60.0;
        private const double MAX_DB = 12.0;
        private const double UNITY_DB = 0.0;
        
        // Dependency properties
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(AudioFader),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));
        
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(AudioFader),
                new PropertyMetadata(MIN_DB, OnRangeChanged));
        
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(AudioFader),
                new PropertyMetadata(MAX_DB, OnRangeChanged));
        
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(AudioFader),
                new PropertyMetadata(Orientation.Vertical, OnOrientationChanged));
        
        public static readonly DependencyProperty TickFrequencyProperty =
            DependencyProperty.Register("TickFrequency", typeof(double), typeof(AudioFader),
                new PropertyMetadata(6.0, OnTickFrequencyChanged));
        
        public static readonly DependencyProperty ShowTicksProperty =
            DependencyProperty.Register("ShowTicks", typeof(bool), typeof(AudioFader),
                new PropertyMetadata(true, OnShowTicksChanged));
        
        public static readonly DependencyProperty ShowTickLabelsProperty =
            DependencyProperty.Register("ShowTickLabels", typeof(bool), typeof(AudioFader),
                new PropertyMetadata(false, OnShowTickLabelsChanged));
        
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(AudioFader),
                new PropertyMetadata(string.Empty, OnLabelChanged));
        
        /// <summary>
        /// Event raised when the value changes.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<double>? ValueChanged;
        
        /// <summary>
        /// Gets or sets the value in dB.
        /// </summary>
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        
        /// <summary>
        /// Gets or sets the minimum value in dB.
        /// </summary>
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        
        /// <summary>
        /// Gets or sets the maximum value in dB.
        /// </summary>
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        
        /// <summary>
        /// Gets or sets the orientation of the fader.
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        
        /// <summary>
        /// Gets or sets the tick frequency in dB.
        /// </summary>
        public double TickFrequency
        {
            get { return (double)GetValue(TickFrequencyProperty); }
            set { SetValue(TickFrequencyProperty, value); }
        }
        
        /// <summary>
        /// Gets or sets whether to show ticks.
        /// </summary>
        public bool ShowTicks
        {
            get { return (bool)GetValue(ShowTicksProperty); }
            set { SetValue(ShowTicksProperty, value); }
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
        /// Gets or sets the label for the fader.
        /// </summary>
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        
        /// <summary>
        /// Initializes a new instance of the AudioFader class.
        /// </summary>
        public AudioFader()
        {
            InitializeComponent();
            
            // Set initial values
            Minimum = MIN_DB;
            Maximum = MAX_DB;
            Value = UNITY_DB;
            
            // Initialize the fader
            Loaded += (s, e) => InitializeFader();
        }
        
        /// <summary>
        /// Initializes the fader.
        /// </summary>
        private void InitializeFader()
        {
            // Set slider properties
            FaderSlider.Minimum = Minimum;
            FaderSlider.Maximum = Maximum;
            FaderSlider.Value = Value;
            FaderSlider.TickFrequency = TickFrequency;
            FaderSlider.IsDirectionReversed = true; // Higher values at top for vertical orientation
            
            // Set orientation
            UpdateOrientation();
            
            // Create tick marks
            CreateTickMarks();
            
            // Set label
            FaderSlider.Tag = Label;
        }
        
        /// <summary>
        /// Updates the orientation of the fader.
        /// </summary>
        private void UpdateOrientation()
        {
            FaderSlider.Orientation = Orientation;
            
            if (Orientation == Orientation.Vertical)
            {
                FaderSlider.Template = (ControlTemplate)Resources["VerticalFaderTemplate"];
                FaderSlider.IsDirectionReversed = true;
            }
            else
            {
                FaderSlider.Template = (ControlTemplate)Resources["HorizontalFaderTemplate"];
                FaderSlider.IsDirectionReversed = false;
            }
            
            // Force template to apply
            FaderSlider.ApplyTemplate();
            
            // Create tick marks
            CreateTickMarks();
        }
        
        /// <summary>
        /// Creates tick marks for the fader.
        /// </summary>
        private void CreateTickMarks()
        {
            // Get the canvas from the template
            Canvas? tickCanvas = FaderSlider.Template.FindName("TickCanvas", FaderSlider) as Canvas;
            
            if (tickCanvas == null)
                return;
            
            tickCanvas.Children.Clear();
            
            if (!ShowTicks)
                return;
            
            double range = Maximum - Minimum;
            
            // Create tick marks at specified intervals
            for (double db = Minimum; db <= Maximum; db += TickFrequency)
            {
                // Calculate position
                double position = (db - Minimum) / range;
                
                // Create tick mark
                Line tick = new Line();
                tick.Stroke = new SolidColorBrush(Colors.Gray);
                tick.StrokeThickness = 1;
                
                if (Orientation == Orientation.Vertical)
                {
                    double y = (1.0 - position) * tickCanvas.ActualHeight;
                    tick.X1 = -5;
                    tick.Y1 = y;
                    tick.X2 = tickCanvas.ActualWidth + 5;
                    tick.Y2 = y;
                }
                else
                {
                    double x = position * tickCanvas.ActualWidth;
                    tick.X1 = x;
                    tick.Y1 = -5;
                    tick.X2 = x;
                    tick.Y2 = tickCanvas.ActualHeight + 5;
                }
                
                tickCanvas.Children.Add(tick);
                
                // Add label if enabled
                if (ShowTickLabels)
                {
                    TextBlock label = new TextBlock();
                    label.Text = db.ToString("+#;-#;0");
                    label.Foreground = new SolidColorBrush(Colors.LightGray);
                    label.FontSize = 8;
                    
                    if (Orientation == Orientation.Vertical)
                    {
                        Canvas.SetLeft(label, tickCanvas.ActualWidth + 2);
                        Canvas.SetTop(label, y - 6);
                    }
                    else
                    {
                        Canvas.SetLeft(label, x - 8);
                        Canvas.SetTop(label, tickCanvas.ActualHeight + 2);
                    }
                    
                    tickCanvas.Children.Add(label);
                }
            }
            
            // Add a special tick mark for unity gain (0 dB)
            if (Minimum < UNITY_DB && Maximum > UNITY_DB)
            {
                double position = (UNITY_DB - Minimum) / range;
                
                Line unityTick = new Line();
                unityTick.Stroke = new SolidColorBrush(Colors.White);
                unityTick.StrokeThickness = 1.5;
                
                if (Orientation == Orientation.Vertical)
                {
                    double y = (1.0 - position) * tickCanvas.ActualHeight;
                    unityTick.X1 = -8;
                    unityTick.Y1 = y;
                    unityTick.X2 = tickCanvas.ActualWidth + 8;
                    unityTick.Y2 = y;
                }
                else
                {
                    double x = position * tickCanvas.ActualWidth;
                    unityTick.X1 = x;
                    unityTick.Y1 = -8;
                    unityTick.X2 = x;
                    unityTick.Y2 = tickCanvas.ActualHeight + 8;
                }
                
                tickCanvas.Children.Add(unityTick);
                
                // Add unity label if enabled
                if (ShowTickLabels)
                {
                    TextBlock unityLabel = new TextBlock();
                    unityLabel.Text = "0";
                    unityLabel.Foreground = new SolidColorBrush(Colors.White);
                    unityLabel.FontSize = 9;
                    unityLabel.FontWeight = FontWeights.Bold;
                    
                    if (Orientation == Orientation.Vertical)
                    {
                        Canvas.SetLeft(unityLabel, tickCanvas.ActualWidth + 2);
                        Canvas.SetTop(unityLabel, (1.0 - position) * tickCanvas.ActualHeight - 6);
                    }
                    else
                    {
                        Canvas.SetLeft(unityLabel, position * tickCanvas.ActualWidth - 4);
                        Canvas.SetTop(unityLabel, tickCanvas.ActualHeight + 2);
                    }
                    
                    tickCanvas.Children.Add(unityLabel);
                }
            }
        }
        
        /// <summary>
        /// Updates the selection range to show the current value.
        /// </summary>
        private void UpdateSelectionRange()
        {
            // Get the selection range from the template
            Rectangle? selectionRange = FaderSlider.Template.FindName("PART_SelectionRange", FaderSlider) as Rectangle;
            
            if (selectionRange == null)
                return;
            
            double range = Maximum - Minimum;
            double position = (Value - Minimum) / range;
            
            // Adjust for orientation
            if (Orientation == Orientation.Vertical)
            {
                selectionRange.Height = position * FaderSlider.ActualHeight;
            }
            else
            {
                selectionRange.Width = position * FaderSlider.ActualWidth;
            }
        }
        
        /// <summary>
        /// Handles the ValueChanged event for the slider.
        /// </summary>
        private void FaderSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Update the Value property
            Value = e.NewValue;
            
            // Update the selection range
            UpdateSelectionRange();
            
            // Raise the ValueChanged event
            ValueChanged?.Invoke(this, new RoutedPropertyChangedEventArgs<double>(e.OldValue, e.NewValue));
        }
        
        /// <summary>
        /// Handles changes to the Value property.
        /// </summary>
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioFader fader)
            {
                // Update the slider value
                if (fader.FaderSlider != null)
                {
                    fader.FaderSlider.Value = (double)e.NewValue;
                }
            }
        }
        
        /// <summary>
        /// Handles changes to the Minimum or Maximum properties.
        /// </summary>
        private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioFader fader)
            {
                // Update the slider range
                if (fader.FaderSlider != null)
                {
                    fader.FaderSlider.Minimum = fader.Minimum;
                    fader.FaderSlider.Maximum = fader.Maximum;
                    
                    // Create tick marks
                    fader.CreateTickMarks();
                }
            }
        }
        
        /// <summary>
        /// Handles changes to the Orientation property.
        /// </summary>
        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioFader fader)
            {
                // Update the orientation
                fader.UpdateOrientation();
            }
        }
        
        /// <summary>
        /// Handles changes to the TickFrequency property.
        /// </summary>
        private static void OnTickFrequencyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioFader fader)
            {
                // Update the tick frequency
                if (fader.FaderSlider != null)
                {
                    fader.FaderSlider.TickFrequency = (double)e.NewValue;
                    
                    // Create tick marks
                    fader.CreateTickMarks();
                }
            }
        }
        
        /// <summary>
        /// Handles changes to the ShowTicks property.
        /// </summary>
        private static void OnShowTicksChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioFader fader)
            {
                // Create tick marks
                fader.CreateTickMarks();
            }
        }
        
        /// <summary>
        /// Handles changes to the ShowTickLabels property.
        /// </summary>
        private static void OnShowTickLabelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioFader fader)
            {
                // Create tick marks
                fader.CreateTickMarks();
            }
        }
        
        /// <summary>
        /// Handles changes to the Label property.
        /// </summary>
        private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioFader fader)
            {
                // Update the label
                if (fader.FaderSlider != null)
                {
                    fader.FaderSlider.Tag = e.NewValue;
                }
            }
        }
        
        /// <summary>
        /// Handles the SizeChanged event.
        /// </summary>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            
            // Create tick marks
            CreateTickMarks();
            
            // Update selection range
            UpdateSelectionRange();
        }
    }
}
