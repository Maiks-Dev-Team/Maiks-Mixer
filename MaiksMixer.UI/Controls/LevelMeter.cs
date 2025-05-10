using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MaiksMixer.UI.Controls
{
    /// <summary>
    /// A custom control that displays audio levels in a vertical or horizontal meter.
    /// </summary>
    public class LevelMeter : Control
    {
        private const int SegmentCount = 20;
        private readonly Rectangle[] _leftSegments;
        private readonly Rectangle[] _rightSegments;
        private Rectangle _leftPeakIndicator;
        private Rectangle _rightPeakIndicator;
        private Grid _mainGrid;

        static LevelMeter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LevelMeter), new FrameworkPropertyMetadata(typeof(LevelMeter)));
        }

        /// <summary>
        /// Initializes a new instance of the LevelMeter class.
        /// </summary>
        public LevelMeter()
        {
            _leftSegments = new Rectangle[SegmentCount];
            _rightSegments = new Rectangle[SegmentCount];
            
            // Create the control template
            CreateControlTemplate();
        }

        #region Dependency Properties

        /// <summary>
        /// Dependency property for the left channel level.
        /// </summary>
        public static readonly DependencyProperty LeftLevelProperty =
            DependencyProperty.Register(
                "LeftLevel",
                typeof(double),
                typeof(LevelMeter),
                new PropertyMetadata(0.0, OnLevelChanged));

        /// <summary>
        /// Dependency property for the right channel level.
        /// </summary>
        public static readonly DependencyProperty RightLevelProperty =
            DependencyProperty.Register(
                "RightLevel",
                typeof(double),
                typeof(LevelMeter),
                new PropertyMetadata(0.0, OnLevelChanged));

        /// <summary>
        /// Dependency property for the peak left level.
        /// </summary>
        public static readonly DependencyProperty PeakLeftLevelProperty =
            DependencyProperty.Register(
                "PeakLeftLevel",
                typeof(double),
                typeof(LevelMeter),
                new PropertyMetadata(0.0, OnPeakLevelChanged));

        /// <summary>
        /// Dependency property for the peak right level.
        /// </summary>
        public static readonly DependencyProperty PeakRightLevelProperty =
            DependencyProperty.Register(
                "PeakRightLevel",
                typeof(double),
                typeof(LevelMeter),
                new PropertyMetadata(0.0, OnPeakLevelChanged));

        /// <summary>
        /// Dependency property for the orientation of the meter.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                "Orientation",
                typeof(Orientation),
                typeof(LevelMeter),
                new PropertyMetadata(Orientation.Vertical, OnOrientationChanged));

        /// <summary>
        /// Dependency property for showing peak indicators.
        /// </summary>
        public static readonly DependencyProperty ShowPeakIndicatorsProperty =
            DependencyProperty.Register(
                "ShowPeakIndicators",
                typeof(bool),
                typeof(LevelMeter),
                new PropertyMetadata(true, OnShowPeakIndicatorsChanged));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the left channel level (0.0 to 1.0).
        /// </summary>
        public double LeftLevel
        {
            get => (double)GetValue(LeftLevelProperty);
            set => SetValue(LeftLevelProperty, Math.Clamp(value, 0.0, 1.0));
        }

        /// <summary>
        /// Gets or sets the right channel level (0.0 to 1.0).
        /// </summary>
        public double RightLevel
        {
            get => (double)GetValue(RightLevelProperty);
            set => SetValue(RightLevelProperty, Math.Clamp(value, 0.0, 1.0));
        }

        /// <summary>
        /// Gets or sets the peak left level (0.0 to 1.0).
        /// </summary>
        public double PeakLeftLevel
        {
            get => (double)GetValue(PeakLeftLevelProperty);
            set => SetValue(PeakLeftLevelProperty, Math.Clamp(value, 0.0, 1.0));
        }

        /// <summary>
        /// Gets or sets the peak right level (0.0 to 1.0).
        /// </summary>
        public double PeakRightLevel
        {
            get => (double)GetValue(PeakRightLevelProperty);
            set => SetValue(PeakRightLevelProperty, Math.Clamp(value, 0.0, 1.0));
        }

        /// <summary>
        /// Gets or sets the orientation of the meter.
        /// </summary>
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show peak indicators.
        /// </summary>
        public bool ShowPeakIndicators
        {
            get => (bool)GetValue(ShowPeakIndicatorsProperty);
            set => SetValue(ShowPeakIndicatorsProperty, value);
        }

        #endregion

        #region Private Methods

        private void CreateControlTemplate()
        {
            // Create the main grid
            _mainGrid = new Grid();
            
            // Create the left channel grid
            var leftGrid = new Grid
            {
                Margin = new Thickness(0, 0, 1, 0)
            };
            
            // Create the right channel grid
            var rightGrid = new Grid
            {
                Margin = new Thickness(1, 0, 0, 0)
            };
            
            // Add the grids to the main grid
            _mainGrid.Children.Add(leftGrid);
            _mainGrid.Children.Add(rightGrid);
            
            // Set up the grid layout based on orientation
            if (Orientation == Orientation.Vertical)
            {
                _mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
                _mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
                Grid.SetColumn(leftGrid, 0);
                Grid.SetColumn(rightGrid, 1);
                
                // Create segments for each channel
                for (int i = 0; i < SegmentCount; i++)
                {
                    // Calculate the color based on the segment position
                    var brush = GetSegmentBrush(i);
                    
                    // Create the left segment
                    _leftSegments[i] = new Rectangle
                    {
                        Fill = brush,
                        Margin = new Thickness(0, 1, 0, 1),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Height = 0
                    };
                    
                    // Create the right segment
                    _rightSegments[i] = new Rectangle
                    {
                        Fill = brush,
                        Margin = new Thickness(0, 1, 0, 1),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Height = 0
                    };
                    
                    // Add the segments to their respective grids
                    leftGrid.Children.Add(_leftSegments[i]);
                    rightGrid.Children.Add(_rightSegments[i]);
                }
                
                // Create peak indicators
                _leftPeakIndicator = new Rectangle
                {
                    Fill = Brushes.White,
                    Height = 2,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(0),
                    Visibility = ShowPeakIndicators ? Visibility.Visible : Visibility.Collapsed
                };
                
                _rightPeakIndicator = new Rectangle
                {
                    Fill = Brushes.White,
                    Height = 2,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(0),
                    Visibility = ShowPeakIndicators ? Visibility.Visible : Visibility.Collapsed
                };
                
                // Add the peak indicators to their respective grids
                leftGrid.Children.Add(_leftPeakIndicator);
                rightGrid.Children.Add(_rightPeakIndicator);
            }
            else // Horizontal orientation
            {
                _mainGrid.RowDefinitions.Add(new RowDefinition());
                _mainGrid.RowDefinitions.Add(new RowDefinition());
                Grid.SetRow(leftGrid, 0);
                Grid.SetRow(rightGrid, 1);
                
                // Create segments for each channel
                for (int i = 0; i < SegmentCount; i++)
                {
                    // Calculate the color based on the segment position
                    var brush = GetSegmentBrush(i);
                    
                    // Create the left segment
                    _leftSegments[i] = new Rectangle
                    {
                        Fill = brush,
                        Margin = new Thickness(1, 0, 1, 0),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Width = 0
                    };
                    
                    // Create the right segment
                    _rightSegments[i] = new Rectangle
                    {
                        Fill = brush,
                        Margin = new Thickness(1, 0, 1, 0),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Width = 0
                    };
                    
                    // Add the segments to their respective grids
                    leftGrid.Children.Add(_leftSegments[i]);
                    rightGrid.Children.Add(_rightSegments[i]);
                }
                
                // Create peak indicators
                _leftPeakIndicator = new Rectangle
                {
                    Fill = Brushes.White,
                    Width = 2,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Margin = new Thickness(0),
                    Visibility = ShowPeakIndicators ? Visibility.Visible : Visibility.Collapsed
                };
                
                _rightPeakIndicator = new Rectangle
                {
                    Fill = Brushes.White,
                    Width = 2,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Margin = new Thickness(0),
                    Visibility = ShowPeakIndicators ? Visibility.Visible : Visibility.Collapsed
                };
                
                // Add the peak indicators to their respective grids
                leftGrid.Children.Add(_leftPeakIndicator);
                rightGrid.Children.Add(_rightPeakIndicator);
            }
            
            // Set the template
            Template = new ControlTemplate(typeof(LevelMeter))
            {
                VisualTree = new FrameworkElementFactory(typeof(Grid))
            };
            
            // Set the content
            AddVisualChild(_mainGrid);
            AddLogicalChild(_mainGrid);
        }

        private Brush GetSegmentBrush(int segmentIndex)
        {
            // Calculate the normalized position (0.0 to 1.0)
            double position = (double)segmentIndex / SegmentCount;
            
            // Green for lower levels (0.0 to 0.6)
            if (position < 0.6)
            {
                return new SolidColorBrush(Color.FromRgb(0, 200, 0));
            }
            // Yellow for medium levels (0.6 to 0.8)
            else if (position < 0.8)
            {
                return new SolidColorBrush(Color.FromRgb(255, 200, 0));
            }
            // Red for high levels (0.8 to 1.0)
            else
            {
                return new SolidColorBrush(Color.FromRgb(200, 0, 0));
            }
        }

        private void UpdateLevels()
        {
            if (_leftSegments == null || _rightSegments == null)
            {
                return;
            }
            
            // Update segment visibility based on levels
            for (int i = 0; i < SegmentCount; i++)
            {
                // Calculate the threshold for this segment
                double threshold = (double)i / SegmentCount;
                
                // Update left channel segments
                if (LeftLevel >= threshold)
                {
                    // Show the segment
                    if (Orientation == Orientation.Vertical)
                    {
                        double segmentHeight = ActualHeight / SegmentCount;
                        _leftSegments[SegmentCount - i - 1].Height = segmentHeight - 2;
                    }
                    else
                    {
                        double segmentWidth = ActualWidth / SegmentCount;
                        _leftSegments[i].Width = segmentWidth - 2;
                    }
                }
                else
                {
                    // Hide the segment
                    if (Orientation == Orientation.Vertical)
                    {
                        _leftSegments[SegmentCount - i - 1].Height = 0;
                    }
                    else
                    {
                        _leftSegments[i].Width = 0;
                    }
                }
                
                // Update right channel segments
                if (RightLevel >= threshold)
                {
                    // Show the segment
                    if (Orientation == Orientation.Vertical)
                    {
                        double segmentHeight = ActualHeight / SegmentCount;
                        _rightSegments[SegmentCount - i - 1].Height = segmentHeight - 2;
                    }
                    else
                    {
                        double segmentWidth = ActualWidth / SegmentCount;
                        _rightSegments[i].Width = segmentWidth - 2;
                    }
                }
                else
                {
                    // Hide the segment
                    if (Orientation == Orientation.Vertical)
                    {
                        _rightSegments[SegmentCount - i - 1].Height = 0;
                    }
                    else
                    {
                        _rightSegments[i].Width = 0;
                    }
                }
            }
            
            // Update peak indicators
            if (ShowPeakIndicators)
            {
                if (Orientation == Orientation.Vertical)
                {
                    double leftPosition = ActualHeight * (1.0 - PeakLeftLevel);
                    double rightPosition = ActualHeight * (1.0 - PeakRightLevel);
                    
                    _leftPeakIndicator.SetValue(Canvas.BottomProperty, ActualHeight - leftPosition);
                    _rightPeakIndicator.SetValue(Canvas.BottomProperty, ActualHeight - rightPosition);
                }
                else
                {
                    double leftPosition = ActualWidth * PeakLeftLevel;
                    double rightPosition = ActualWidth * PeakRightLevel;
                    
                    _leftPeakIndicator.SetValue(Canvas.LeftProperty, leftPosition);
                    _rightPeakIndicator.SetValue(Canvas.LeftProperty, rightPosition);
                }
            }
        }

        private void UpdatePeakIndicators()
        {
            if (_leftPeakIndicator == null || _rightPeakIndicator == null)
            {
                return;
            }
            
            // Update peak indicator visibility
            _leftPeakIndicator.Visibility = ShowPeakIndicators ? Visibility.Visible : Visibility.Collapsed;
            _rightPeakIndicator.Visibility = ShowPeakIndicators ? Visibility.Visible : Visibility.Collapsed;
            
            // Update peak indicator positions
            if (ShowPeakIndicators)
            {
                if (Orientation == Orientation.Vertical)
                {
                    double leftPosition = ActualHeight * (1.0 - PeakLeftLevel);
                    double rightPosition = ActualHeight * (1.0 - PeakRightLevel);
                    
                    _leftPeakIndicator.SetValue(Canvas.BottomProperty, ActualHeight - leftPosition);
                    _rightPeakIndicator.SetValue(Canvas.BottomProperty, ActualHeight - rightPosition);
                }
                else
                {
                    double leftPosition = ActualWidth * PeakLeftLevel;
                    double rightPosition = ActualWidth * PeakRightLevel;
                    
                    _leftPeakIndicator.SetValue(Canvas.LeftProperty, leftPosition);
                    _rightPeakIndicator.SetValue(Canvas.LeftProperty, rightPosition);
                }
            }
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
            
            // Update the levels
            UpdateLevels();
            
            // Update the peak indicators
            UpdatePeakIndicators();
        }

        /// <summary>
        /// Called when the size of the control changes.
        /// </summary>
        /// <param name="sizeInfo">The size information.</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            
            // Update the levels
            UpdateLevels();
            
            // Update the peak indicators
            UpdatePeakIndicators();
        }

        #endregion

        #region Static Methods

        private static void OnLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LevelMeter meter)
            {
                meter.UpdateLevels();
            }
        }

        private static void OnPeakLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LevelMeter meter)
            {
                meter.UpdatePeakIndicators();
            }
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LevelMeter meter)
            {
                // Recreate the control template
                meter.CreateControlTemplate();
                
                // Update the levels
                meter.UpdateLevels();
                
                // Update the peak indicators
                meter.UpdatePeakIndicators();
            }
        }

        private static void OnShowPeakIndicatorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LevelMeter meter)
            {
                meter.UpdatePeakIndicators();
            }
        }

        #endregion
    }
}
