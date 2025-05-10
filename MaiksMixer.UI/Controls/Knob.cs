using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MaiksMixer.UI.Controls
{
    /// <summary>
    /// A custom control that represents a rotary knob.
    /// </summary>
    public class Knob : Control
    {
        private Ellipse _knobBackground;
        private Line _pointer;
        private TextBlock _valueDisplay;
        private Canvas _tickCanvas;
        private bool _isDragging;
        private Point _lastMousePosition;
        private double _dragStartValue;
        private Grid _mainGrid;

        static Knob()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Knob), new FrameworkPropertyMetadata(typeof(Knob)));
        }

        /// <summary>
        /// Initializes a new instance of the Knob class.
        /// </summary>
        public Knob()
        {
            // Create the control template
            CreateControlTemplate();
            
            // Register for mouse events
            MouseLeftButtonDown += Knob_MouseLeftButtonDown;
            MouseLeftButtonUp += Knob_MouseLeftButtonUp;
            MouseMove += Knob_MouseMove;
            MouseWheel += Knob_MouseWheel;
            
            // Set default values
            Minimum = 0.0;
            Maximum = 1.0;
            Value = 0.5;
            TickCount = 11;
            StartAngle = -135;
            EndAngle = 135;
            TickFrequency = 0.1;
            Background = new SolidColorBrush(Color.FromRgb(45, 45, 48));
            Foreground = Brushes.White;
            TickBrush = new SolidColorBrush(Color.FromRgb(63, 63, 70));
            PointerBrush = new SolidColorBrush(Color.FromRgb(0, 120, 215));
        }

        #region Dependency Properties

        /// <summary>
        /// Dependency property for the minimum value.
        /// </summary>
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                "Minimum",
                typeof(double),
                typeof(Knob),
                new PropertyMetadata(0.0, OnRangeChanged));

        /// <summary>
        /// Dependency property for the maximum value.
        /// </summary>
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                "Maximum",
                typeof(double),
                typeof(Knob),
                new PropertyMetadata(1.0, OnRangeChanged));

        /// <summary>
        /// Dependency property for the current value.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(double),
                typeof(Knob),
                new PropertyMetadata(0.5, OnValueChanged));

        /// <summary>
        /// Dependency property for the number of ticks.
        /// </summary>
        public static readonly DependencyProperty TickCountProperty =
            DependencyProperty.Register(
                "TickCount",
                typeof(int),
                typeof(Knob),
                new PropertyMetadata(11, OnTicksChanged));

        /// <summary>
        /// Dependency property for the start angle.
        /// </summary>
        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register(
                "StartAngle",
                typeof(double),
                typeof(Knob),
                new PropertyMetadata(-135.0, OnAngleChanged));

        /// <summary>
        /// Dependency property for the end angle.
        /// </summary>
        public static readonly DependencyProperty EndAngleProperty =
            DependencyProperty.Register(
                "EndAngle",
                typeof(double),
                typeof(Knob),
                new PropertyMetadata(135.0, OnAngleChanged));

        /// <summary>
        /// Dependency property for the tick frequency.
        /// </summary>
        public static readonly DependencyProperty TickFrequencyProperty =
            DependencyProperty.Register(
                "TickFrequency",
                typeof(double),
                typeof(Knob),
                new PropertyMetadata(0.1, OnTicksChanged));

        /// <summary>
        /// Dependency property for the tick brush.
        /// </summary>
        public static readonly DependencyProperty TickBrushProperty =
            DependencyProperty.Register(
                "TickBrush",
                typeof(Brush),
                typeof(Knob),
                new PropertyMetadata(Brushes.Gray, OnTicksChanged));

        /// <summary>
        /// Dependency property for the pointer brush.
        /// </summary>
        public static readonly DependencyProperty PointerBrushProperty =
            DependencyProperty.Register(
                "PointerBrush",
                typeof(Brush),
                typeof(Knob),
                new PropertyMetadata(Brushes.White, OnPointerBrushChanged));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, Math.Clamp(value, Minimum, Maximum));
        }

        /// <summary>
        /// Gets or sets the number of ticks.
        /// </summary>
        public int TickCount
        {
            get => (int)GetValue(TickCountProperty);
            set => SetValue(TickCountProperty, value);
        }

        /// <summary>
        /// Gets or sets the start angle.
        /// </summary>
        public double StartAngle
        {
            get => (double)GetValue(StartAngleProperty);
            set => SetValue(StartAngleProperty, value);
        }

        /// <summary>
        /// Gets or sets the end angle.
        /// </summary>
        public double EndAngle
        {
            get => (double)GetValue(EndAngleProperty);
            set => SetValue(EndAngleProperty, value);
        }

        /// <summary>
        /// Gets or sets the tick frequency.
        /// </summary>
        public double TickFrequency
        {
            get => (double)GetValue(TickFrequencyProperty);
            set => SetValue(TickFrequencyProperty, value);
        }

        /// <summary>
        /// Gets or sets the tick brush.
        /// </summary>
        public Brush TickBrush
        {
            get => (Brush)GetValue(TickBrushProperty);
            set => SetValue(TickBrushProperty, value);
        }

        /// <summary>
        /// Gets or sets the pointer brush.
        /// </summary>
        public Brush PointerBrush
        {
            get => (Brush)GetValue(PointerBrushProperty);
            set => SetValue(PointerBrushProperty, value);
        }

        #endregion

        #region Private Methods

        private void CreateControlTemplate()
        {
            // Create the main grid
            _mainGrid = new Grid();
            
            // Create the tick canvas
            _tickCanvas = new Canvas();
            
            // Create the knob background
            _knobBackground = new Ellipse
            {
                Fill = Background,
                Stroke = BorderBrush,
                StrokeThickness = 1
            };
            
            // Create the pointer
            _pointer = new Line
            {
                Stroke = PointerBrush,
                StrokeThickness = 2,
                X1 = 0,
                Y1 = 0,
                X2 = 0,
                Y2 = 0
            };
            
            // Create the value display
            _valueDisplay = new TextBlock
            {
                Foreground = Foreground,
                FontSize = 10,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            // Add the elements to the grid
            _mainGrid.Children.Add(_tickCanvas);
            _mainGrid.Children.Add(_knobBackground);
            _mainGrid.Children.Add(_pointer);
            _mainGrid.Children.Add(_valueDisplay);
            
            // Set the template
            Template = new ControlTemplate(typeof(Knob))
            {
                VisualTree = new FrameworkElementFactory(typeof(Grid))
            };
            
            // Set the content
            AddVisualChild(_mainGrid);
            AddLogicalChild(_mainGrid);
        }

        private void UpdateTicks()
        {
            if (_tickCanvas == null)
            {
                return;
            }
            
            // Clear existing ticks
            _tickCanvas.Children.Clear();
            
            // Calculate the center of the control
            double centerX = ActualWidth / 2;
            double centerY = ActualHeight / 2;
            
            // Calculate the radius of the tick circle
            double radius = Math.Min(ActualWidth, ActualHeight) / 2 - 2;
            
            // Calculate the angle step
            double angleRange = EndAngle - StartAngle;
            double angleStep = angleRange / (TickCount - 1);
            
            // Create the ticks
            for (int i = 0; i < TickCount; i++)
            {
                // Calculate the angle for this tick
                double angle = StartAngle + (i * angleStep);
                
                // Convert to radians
                double radians = angle * Math.PI / 180;
                
                // Calculate the tick start and end points
                double innerRadius = radius * 0.8;
                double outerRadius = radius;
                double startX = centerX + (innerRadius * Math.Cos(radians));
                double startY = centerY + (innerRadius * Math.Sin(radians));
                double endX = centerX + (outerRadius * Math.Cos(radians));
                double endY = centerY + (outerRadius * Math.Sin(radians));
                
                // Create the tick line
                var tick = new Line
                {
                    Stroke = TickBrush,
                    StrokeThickness = 1,
                    X1 = startX,
                    Y1 = startY,
                    X2 = endX,
                    Y2 = endY
                };
                
                // Add the tick to the canvas
                _tickCanvas.Children.Add(tick);
            }
        }

        private void UpdatePointer()
        {
            if (_pointer == null)
            {
                return;
            }
            
            // Calculate the center of the control
            double centerX = ActualWidth / 2;
            double centerY = ActualHeight / 2;
            
            // Calculate the radius of the pointer
            double radius = Math.Min(ActualWidth, ActualHeight) / 2 - 5;
            
            // Calculate the normalized value (0.0 to 1.0)
            double normalizedValue = (Value - Minimum) / (Maximum - Minimum);
            
            // Calculate the angle for the current value
            double angleRange = EndAngle - StartAngle;
            double angle = StartAngle + (normalizedValue * angleRange);
            
            // Convert to radians
            double radians = angle * Math.PI / 180;
            
            // Calculate the pointer end point
            double endX = centerX + (radius * Math.Cos(radians));
            double endY = centerY + (radius * Math.Sin(radians));
            
            // Update the pointer
            _pointer.X1 = centerX;
            _pointer.Y1 = centerY;
            _pointer.X2 = endX;
            _pointer.Y2 = endY;
            
            // Update the value display
            _valueDisplay.Text = Value.ToString("F1");
        }

        private void UpdateKnobSize()
        {
            if (_knobBackground == null)
            {
                return;
            }
            
            // Update the knob size
            _knobBackground.Width = ActualWidth;
            _knobBackground.Height = ActualHeight;
        }

        #endregion

        #region Event Handlers

        private void Knob_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Capture the mouse
            CaptureMouse();
            
            // Set the dragging flag
            _isDragging = true;
            
            // Store the current mouse position
            _lastMousePosition = e.GetPosition(this);
            
            // Store the current value
            _dragStartValue = Value;
            
            // Mark the event as handled
            e.Handled = true;
        }

        private void Knob_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Release the mouse capture
            ReleaseMouseCapture();
            
            // Clear the dragging flag
            _isDragging = false;
            
            // Mark the event as handled
            e.Handled = true;
        }

        private void Knob_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                // Get the current mouse position
                Point currentPosition = e.GetPosition(this);
                
                // Calculate the delta
                double deltaY = _lastMousePosition.Y - currentPosition.Y;
                
                // Calculate the sensitivity based on the range
                double range = Maximum - Minimum;
                double sensitivity = range / 100.0;
                
                // Calculate the new value
                double newValue = _dragStartValue + (deltaY * sensitivity);
                
                // Update the value
                Value = Math.Clamp(newValue, Minimum, Maximum);
                
                // Mark the event as handled
                e.Handled = true;
            }
        }

        private void Knob_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Calculate the sensitivity based on the range
            double range = Maximum - Minimum;
            double sensitivity = range / 100.0;
            
            // Calculate the delta
            double delta = e.Delta > 0 ? sensitivity : -sensitivity;
            
            // Calculate the new value
            double newValue = Value + delta;
            
            // Update the value
            Value = Math.Clamp(newValue, Minimum, Maximum);
            
            // Mark the event as handled
            e.Handled = true;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Gets the visual child at the specified index.
        /// </summary>
        /// <param name="index">The index of the visual child.</param>
        /// <returns>The visual child at the specified index.</returns>
        protected override Visual GetVisualChild(int index)
        {
            if (index != 0 || _mainGrid == null)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            
            return _mainGrid;
        }

        /// <summary>
        /// Gets the number of visual children.
        /// </summary>
        protected override int VisualChildrenCount => _mainGrid != null ? 1 : 0;

        /// <summary>
        /// Called when the control is rendered.
        /// </summary>
        /// <param name="drawingContext">The drawing context.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            
            // Update the knob size
            UpdateKnobSize();
            
            // Update the ticks
            UpdateTicks();
            
            // Update the pointer
            UpdatePointer();
        }

        /// <summary>
        /// Called when the size of the control changes.
        /// </summary>
        /// <param name="sizeInfo">The size information.</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            
            // Update the knob size
            UpdateKnobSize();
            
            // Update the ticks
            UpdateTicks();
            
            // Update the pointer
            UpdatePointer();
        }

        #endregion

        #region Static Methods

        private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Knob knob)
            {
                // Ensure the value is within the new range
                knob.Value = Math.Clamp(knob.Value, knob.Minimum, knob.Maximum);
                
                // Update the pointer
                knob.UpdatePointer();
            }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Knob knob)
            {
                // Update the pointer
                knob.UpdatePointer();
            }
        }

        private static void OnTicksChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Knob knob)
            {
                // Update the ticks
                knob.UpdateTicks();
            }
        }

        private static void OnAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Knob knob)
            {
                // Update the ticks
                knob.UpdateTicks();
                
                // Update the pointer
                knob.UpdatePointer();
            }
        }

        private static void OnPointerBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Knob knob && knob._pointer != null)
            {
                // Update the pointer brush
                knob._pointer.Stroke = knob.PointerBrush;
            }
        }

        #endregion
    }
}
