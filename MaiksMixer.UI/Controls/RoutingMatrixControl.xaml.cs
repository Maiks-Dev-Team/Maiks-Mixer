using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MaiksMixer.UI.Controls
{
    /// <summary>
    /// Interaction logic for RoutingMatrixControl.xaml
    /// </summary>
    public partial class RoutingMatrixControl : UserControl
    {
        // Constants for UI layout
        private const int HEADER_HEIGHT = 30;
        private const int HEADER_WIDTH = 150;
        private const int CELL_SIZE = 30;
        private const double LINE_THICKNESS = 2.0;
        
        // Data structures for the routing matrix
        private List<AudioDevice> _inputDevices = new List<AudioDevice>();
        private List<AudioDevice> _outputDevices = new List<AudioDevice>();
        private Dictionary<string, AudioConnection> _connections = new Dictionary<string, AudioConnection>();
        
        // UI elements for the matrix
        private Dictionary<string, Button> _connectionButtons = new Dictionary<string, Button>();
        private Dictionary<string, Line> _connectionLines = new Dictionary<string, Line>();
        
        // Currently selected connection for the popup
        private string _selectedConnectionId = string.Empty;
        
        /// <summary>
        /// Event raised when a connection is changed.
        /// </summary>
        public event EventHandler<AudioConnectionEventArgs>? ConnectionChanged;
        
        /// <summary>
        /// Initializes a new instance of the RoutingMatrixControl class.
        /// </summary>
        public RoutingMatrixControl()
        {
            InitializeComponent();
            
            // Initialize the matrix with some sample data
            Loaded += (s, e) => InitializeMatrix();
        }
        
        /// <summary>
        /// Initializes the routing matrix with sample data.
        /// </summary>
        private void InitializeMatrix()
        {
            // Create sample input devices
            _inputDevices = new List<AudioDevice>
            {
                new AudioDevice { Id = "input1", Name = "Microphone", Type = DeviceType.Physical, Direction = DeviceDirection.Input },
                new AudioDevice { Id = "input2", Name = "Line In", Type = DeviceType.Physical, Direction = DeviceDirection.Input },
                new AudioDevice { Id = "input3", Name = "Virtual Input 1", Type = DeviceType.Virtual, Direction = DeviceDirection.Input },
                new AudioDevice { Id = "input4", Name = "Virtual Input 2", Type = DeviceType.Virtual, Direction = DeviceDirection.Input }
            };
            
            // Create sample output devices
            _outputDevices = new List<AudioDevice>
            {
                new AudioDevice { Id = "output1", Name = "Speakers", Type = DeviceType.Physical, Direction = DeviceDirection.Output },
                new AudioDevice { Id = "output2", Name = "Headphones", Type = DeviceType.Physical, Direction = DeviceDirection.Output },
                new AudioDevice { Id = "output3", Name = "Virtual Output 1", Type = DeviceType.Virtual, Direction = DeviceDirection.Output },
                new AudioDevice { Id = "output4", Name = "Virtual Output 2", Type = DeviceType.Virtual, Direction = DeviceDirection.Output }
            };
            
            // Create sample connections
            _connections = new Dictionary<string, AudioConnection>
            {
                { "input1-output1", new AudioConnection { SourceId = "input1", DestinationId = "output1", Status = ConnectionStatus.Connected, Volume = 0.75 } },
                { "input1-output2", new AudioConnection { SourceId = "input1", DestinationId = "output2", Status = ConnectionStatus.Muted, Volume = 0.5 } },
                { "input2-output3", new AudioConnection { SourceId = "input2", DestinationId = "output3", Status = ConnectionStatus.Connected, Volume = 1.0 } },
                { "input3-output4", new AudioConnection { SourceId = "input3", DestinationId = "output4", Status = ConnectionStatus.Connected, Volume = 0.8 } }
            };
            
            // Build the matrix UI
            BuildMatrixUI();
        }
        
        /// <summary>
        /// Builds the matrix UI.
        /// </summary>
        private void BuildMatrixUI()
        {
            // Clear existing UI
            MatrixGrid.Children.Clear();
            MatrixGrid.RowDefinitions.Clear();
            MatrixGrid.ColumnDefinitions.Clear();
            ConnectionLinesCanvas.Children.Clear();
            _connectionButtons.Clear();
            _connectionLines.Clear();
            
            // Set up row and column definitions
            MatrixGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(HEADER_HEIGHT) });
            MatrixGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(CELL_SIZE * _inputDevices.Count) });
            
            MatrixGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(HEADER_WIDTH) });
            MatrixGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(CELL_SIZE * _outputDevices.Count) });
            
            // Add corner header
            TextBlock cornerHeader = new TextBlock
            {
                Text = "Inputs \\ Outputs",
                Style = (Style)Resources["HeaderStyle"],
                TextAlignment = TextAlignment.Center
            };
            Grid.SetRow(cornerHeader, 0);
            Grid.SetColumn(cornerHeader, 0);
            MatrixGrid.Children.Add(cornerHeader);
            
            // Add output device headers
            for (int i = 0; i < _outputDevices.Count; i++)
            {
                AudioDevice device = _outputDevices[i];
                
                // Create header with rotation
                TextBlock header = new TextBlock
                {
                    Text = device.Name,
                    Style = (Style)Resources["HeaderStyle"],
                    LayoutTransform = new RotateTransform(-45),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                
                // Add tooltip with device details
                ToolTip tooltip = new ToolTip
                {
                    Content = $"ID: {device.Id}\nType: {device.Type}\nDirection: {device.Direction}"
                };
                header.ToolTip = tooltip;
                
                // Position in grid
                Grid.SetRow(header, 0);
                Grid.SetColumn(header, 1);
                
                // Add to canvas with absolute positioning
                Canvas headerCanvas = new Canvas
                {
                    Width = CELL_SIZE,
                    Height = HEADER_HEIGHT
                };
                Canvas.SetLeft(header, i * CELL_SIZE + CELL_SIZE / 2 - header.ActualWidth / 2);
                Canvas.SetTop(header, 0);
                headerCanvas.Children.Add(header);
                
                Grid.SetRow(headerCanvas, 0);
                Grid.SetColumn(headerCanvas, 1);
                MatrixGrid.Children.Add(headerCanvas);
            }
            
            // Add input device labels
            for (int i = 0; i < _inputDevices.Count; i++)
            {
                AudioDevice device = _inputDevices[i];
                
                // Create label
                TextBlock label = new TextBlock
                {
                    Text = device.Name,
                    Style = (Style)Resources["DeviceLabelStyle"]
                };
                
                // Add tooltip with device details
                ToolTip tooltip = new ToolTip
                {
                    Content = $"ID: {device.Id}\nType: {device.Type}\nDirection: {device.Direction}"
                };
                label.ToolTip = tooltip;
                
                // Position in grid
                Canvas labelCanvas = new Canvas
                {
                    Width = HEADER_WIDTH,
                    Height = CELL_SIZE
                };
                Canvas.SetLeft(label, 0);
                Canvas.SetTop(label, i * CELL_SIZE + CELL_SIZE / 2 - label.ActualHeight / 2);
                labelCanvas.Children.Add(label);
                
                Grid.SetRow(labelCanvas, 1);
                Grid.SetColumn(labelCanvas, 0);
                MatrixGrid.Children.Add(labelCanvas);
            }
            
            // Add connection cells
            for (int i = 0; i < _inputDevices.Count; i++)
            {
                AudioDevice inputDevice = _inputDevices[i];
                
                for (int j = 0; j < _outputDevices.Count; j++)
                {
                    AudioDevice outputDevice = _outputDevices[j];
                    string connectionId = $"{inputDevice.Id}-{outputDevice.Id}";
                    
                    // Create connection button
                    Button connectionButton = new Button
                    {
                        Style = (Style)Resources["ConnectionNodeStyle"],
                        Tag = _connections.ContainsKey(connectionId) ? _connections[connectionId].Status.ToString() : "Disconnected"
                    };
                    
                    // Set up event handler
                    connectionButton.Click += (s, e) => ConnectionButton_Click(connectionId);
                    
                    // Position in canvas
                    Canvas.SetLeft(connectionButton, j * CELL_SIZE + (CELL_SIZE - connectionButton.Width) / 2);
                    Canvas.SetTop(connectionButton, i * CELL_SIZE + (CELL_SIZE - connectionButton.Height) / 2);
                    
                    // Add to canvas
                    ConnectionLinesCanvas.Children.Add(connectionButton);
                    
                    // Store button reference
                    _connectionButtons[connectionId] = connectionButton;
                    
                    // Add connection line if connected
                    if (_connections.ContainsKey(connectionId) && _connections[connectionId].Status != ConnectionStatus.Disconnected)
                    {
                        DrawConnectionLine(connectionId);
                    }
                }
            }
        }
        
        /// <summary>
        /// Draws a connection line for the specified connection.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        private void DrawConnectionLine(string connectionId)
        {
            if (!_connections.ContainsKey(connectionId) || !_connectionButtons.ContainsKey(connectionId))
                return;
            
            // Get the connection and button
            AudioConnection connection = _connections[connectionId];
            Button button = _connectionButtons[connectionId];
            
            // Get the source and destination indices
            int sourceIndex = _inputDevices.FindIndex(d => d.Id == connection.SourceId);
            int destIndex = _outputDevices.FindIndex(d => d.Id == connection.DestinationId);
            
            if (sourceIndex == -1 || destIndex == -1)
                return;
            
            // Calculate line positions
            double x1 = Canvas.GetLeft(button) + button.Width / 2;
            double y1 = Canvas.GetTop(button) + button.Height / 2;
            
            // Create line
            Line line = new Line
            {
                X1 = x1 - CELL_SIZE / 2,
                Y1 = y1,
                X2 = x1 + CELL_SIZE / 2,
                Y2 = y1,
                StrokeThickness = LINE_THICKNESS,
                Stroke = connection.Status == ConnectionStatus.Connected ? 
                         new SolidColorBrush(Colors.Green) : 
                         new SolidColorBrush(Colors.Red),
                Opacity = connection.Volume
            };
            
            // Add to canvas
            ConnectionLinesCanvas.Children.Add(line);
            
            // Store line reference
            _connectionLines[connectionId] = line;
        }
        
        /// <summary>
        /// Updates the connection status and UI.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <param name="status">The new status.</param>
        /// <param name="volume">The new volume.</param>
        private void UpdateConnection(string connectionId, ConnectionStatus status, double volume)
        {
            // Parse the connection ID
            string[] parts = connectionId.Split('-');
            if (parts.Length != 2)
                return;
            
            string sourceId = parts[0];
            string destId = parts[1];
            
            // Update or create the connection
            if (_connections.ContainsKey(connectionId))
            {
                _connections[connectionId].Status = status;
                _connections[connectionId].Volume = volume;
            }
            else
            {
                _connections[connectionId] = new AudioConnection
                {
                    SourceId = sourceId,
                    DestinationId = destId,
                    Status = status,
                    Volume = volume
                };
            }
            
            // Update the UI
            if (_connectionButtons.ContainsKey(connectionId))
            {
                _connectionButtons[connectionId].Tag = status.ToString();
            }
            
            // Update or create the connection line
            if (status == ConnectionStatus.Disconnected)
            {
                if (_connectionLines.ContainsKey(connectionId))
                {
                    ConnectionLinesCanvas.Children.Remove(_connectionLines[connectionId]);
                    _connectionLines.Remove(connectionId);
                }
            }
            else
            {
                if (_connectionLines.ContainsKey(connectionId))
                {
                    ConnectionLinesCanvas.Children.Remove(_connectionLines[connectionId]);
                    _connectionLines.Remove(connectionId);
                }
                
                DrawConnectionLine(connectionId);
            }
            
            // Raise the ConnectionChanged event
            ConnectionChanged?.Invoke(this, new AudioConnectionEventArgs
            {
                SourceId = sourceId,
                DestinationId = destId,
                Status = status,
                Volume = volume
            });
        }
        
        /// <summary>
        /// Handles the Click event of a connection button.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        private void ConnectionButton_Click(string connectionId)
        {
            // Store the selected connection ID
            _selectedConnectionId = connectionId;
            
            // Parse the connection ID
            string[] parts = connectionId.Split('-');
            if (parts.Length != 2)
                return;
            
            string sourceId = parts[0];
            string destId = parts[1];
            
            // Get the input and output devices
            AudioDevice? inputDevice = _inputDevices.FirstOrDefault(d => d.Id == sourceId);
            AudioDevice? outputDevice = _outputDevices.FirstOrDefault(d => d.Id == destId);
            
            if (inputDevice == null || outputDevice == null)
                return;
            
            // Set the connection title
            ConnectionTitleTextBlock.Text = $"{inputDevice.Name} → {outputDevice.Name}";
            
            // Set the connection status
            if (_connections.ContainsKey(connectionId))
            {
                AudioConnection connection = _connections[connectionId];
                ConnectionStatusComboBox.SelectedIndex = (int)connection.Status;
                VolumeSlider.Value = connection.Volume;
            }
            else
            {
                ConnectionStatusComboBox.SelectedIndex = (int)ConnectionStatus.Disconnected;
                VolumeSlider.Value = 0.75;
            }
            
            // Update the volume value text
            UpdateVolumeValueText();
            
            // Show the popup
            ConnectionDetailsPopup.IsOpen = true;
        }
        
        /// <summary>
        /// Updates the volume value text.
        /// </summary>
        private void UpdateVolumeValueText()
        {
            double volumeDb = 20 * Math.Log10(VolumeSlider.Value);
            if (double.IsNegativeInfinity(volumeDb))
                volumeDb = -60;
            
            VolumeValueTextBlock.Text = $"Volume: {volumeDb:F1} dB";
        }
        
        /// <summary>
        /// Handles the Click event of the refresh button.
        /// </summary>
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // In a real application, this would refresh the routing matrix from the audio engine
            // For now, just rebuild the UI
            BuildMatrixUI();
        }
        
        /// <summary>
        /// Handles the SelectionChanged event of the view mode combo box.
        /// </summary>
        private void ViewModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // In a real application, this would filter the devices and connections based on the selected view mode
            // For now, just rebuild the UI
            BuildMatrixUI();
        }
        
        /// <summary>
        /// Handles the SelectionChanged event of the connection status combo box.
        /// </summary>
        private void ConnectionStatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Enable/disable the volume slider based on the selected status
            bool isConnected = ConnectionStatusComboBox.SelectedIndex == (int)ConnectionStatus.Connected;
            VolumeSlider.IsEnabled = isConnected;
        }
        
        /// <summary>
        /// Handles the ValueChanged event of the volume slider.
        /// </summary>
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Update the volume value text
            UpdateVolumeValueText();
        }
        
        /// <summary>
        /// Handles the Click event of the apply button.
        /// </summary>
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected status and volume
            ConnectionStatus status = (ConnectionStatus)ConnectionStatusComboBox.SelectedIndex;
            double volume = VolumeSlider.Value;
            
            // Update the connection
            UpdateConnection(_selectedConnectionId, status, volume);
            
            // Close the popup
            ConnectionDetailsPopup.IsOpen = false;
        }
        
        /// <summary>
        /// Handles the Click event of the cancel button.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the popup without making changes
            ConnectionDetailsPopup.IsOpen = false;
        }
        
        /// <summary>
        /// Updates the routing matrix with the specified devices and connections.
        /// </summary>
        /// <param name="inputDevices">The input devices.</param>
        /// <param name="outputDevices">The output devices.</param>
        /// <param name="connections">The connections.</param>
        public void UpdateMatrix(List<AudioDevice> inputDevices, List<AudioDevice> outputDevices, Dictionary<string, AudioConnection> connections)
        {
            _inputDevices = inputDevices;
            _outputDevices = outputDevices;
            _connections = connections;
            
            BuildMatrixUI();
        }
    }
    
    /// <summary>
    /// Represents an audio device.
    /// </summary>
    public class AudioDevice
    {
        /// <summary>
        /// Gets or sets the device ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the device name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the device type.
        /// </summary>
        public DeviceType Type { get; set; }
        
        /// <summary>
        /// Gets or sets the device direction.
        /// </summary>
        public DeviceDirection Direction { get; set; }
    }
    
    /// <summary>
    /// Represents an audio connection.
    /// </summary>
    public class AudioConnection
    {
        /// <summary>
        /// Gets or sets the source device ID.
        /// </summary>
        public string SourceId { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the destination device ID.
        /// </summary>
        public string DestinationId { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the connection status.
        /// </summary>
        public ConnectionStatus Status { get; set; }
        
        /// <summary>
        /// Gets or sets the connection volume (0.0 to 1.0).
        /// </summary>
        public double Volume { get; set; }
    }
    
    /// <summary>
    /// Represents the type of an audio device.
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// Physical device.
        /// </summary>
        Physical,
        
        /// <summary>
        /// Virtual device.
        /// </summary>
        Virtual
    }
    
    /// <summary>
    /// Represents the direction of an audio device.
    /// </summary>
    public enum DeviceDirection
    {
        /// <summary>
        /// Input device.
        /// </summary>
        Input,
        
        /// <summary>
        /// Output device.
        /// </summary>
        Output
    }
    
    /// <summary>
    /// Represents the status of an audio connection.
    /// </summary>
    public enum ConnectionStatus
    {
        /// <summary>
        /// Connected.
        /// </summary>
        Connected,
        
        /// <summary>
        /// Muted.
        /// </summary>
        Muted,
        
        /// <summary>
        /// Disconnected.
        /// </summary>
        Disconnected
    }
    
    /// <summary>
    /// Event arguments for the ConnectionChanged event.
    /// </summary>
    public class AudioConnectionEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the source device ID.
        /// </summary>
        public string SourceId { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the destination device ID.
        /// </summary>
        public string DestinationId { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the connection status.
        /// </summary>
        public ConnectionStatus Status { get; set; }
        
        /// <summary>
        /// Gets or sets the connection volume.
        /// </summary>
        public double Volume { get; set; }
    }
}
