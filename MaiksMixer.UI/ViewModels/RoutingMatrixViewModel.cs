using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MaiksMixer.Core.Models;
using MaiksMixer.Core.Services;

namespace MaiksMixer.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the routing matrix view.
    /// </summary>
    public class RoutingMatrixViewModel : INotifyPropertyChanged
    {
        private readonly AudioDeviceService _audioDeviceService;
        private ObservableCollection<AudioPortInfo> _sourcePorts;
        private ObservableCollection<AudioPortInfo> _destinationPorts;
        private ObservableCollection<ObservableCollection<AudioConnectionInfo>> _connectionMatrix;
        private ObservableCollection<string> _sourceFilters;
        private ObservableCollection<string> _destinationFilters;
        private string _selectedSourceFilter;
        private string _selectedDestinationFilter;
        private bool _isLoading;
        private bool _isEditingConnection;
        private AudioConnectionInfo _editingConnection;
        private string _statusMessage;

        /// <summary>
        /// Initializes a new instance of the RoutingMatrixViewModel class.
        /// </summary>
        /// <param name="audioDeviceService">The audio device service to use.</param>
        public RoutingMatrixViewModel(AudioDeviceService audioDeviceService)
        {
            _audioDeviceService = audioDeviceService ?? throw new ArgumentNullException(nameof(audioDeviceService));
            
            // Initialize collections
            SourcePorts = new ObservableCollection<AudioPortInfo>();
            DestinationPorts = new ObservableCollection<AudioPortInfo>();
            ConnectionMatrix = new ObservableCollection<ObservableCollection<AudioConnectionInfo>>();
            SourceFilters = new ObservableCollection<string> { "All", "Physical", "Virtual" };
            DestinationFilters = new ObservableCollection<string> { "All", "Physical", "Virtual" };
            
            // Set default filters
            SelectedSourceFilter = "All";
            SelectedDestinationFilter = "All";
            
            // Initialize commands
            RefreshCommand = new RelayCommand(async () => await RefreshRoutingMatrixAsync());
            ClearAllCommand = new RelayCommand(async () => await ClearAllConnectionsAsync(), CanClearAllConnections);
            ToggleConnectionCommand = new RelayCommand<AudioConnectionInfo>(async (connection) => await ToggleConnectionAsync(connection));
            EditConnectionCommand = new RelayCommand<AudioConnectionInfo>(EditConnection);
            ApplyConnectionChangesCommand = new RelayCommand(async () => await ApplyConnectionChangesAsync(), CanApplyConnectionChanges);
            CancelEditConnectionCommand = new RelayCommand(CancelEditConnection);
            
            // Register for events from the audio device service
            _audioDeviceService.ConnectionsUpdated += AudioDeviceService_ConnectionsUpdated;
            
            // Load the routing matrix
            Task.Run(RefreshRoutingMatrixAsync);
        }

        #region Properties

        /// <summary>
        /// Gets the collection of source ports.
        /// </summary>
        public ObservableCollection<AudioPortInfo> SourcePorts
        {
            get => _sourcePorts;
            private set
            {
                _sourcePorts = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the collection of destination ports.
        /// </summary>
        public ObservableCollection<AudioPortInfo> DestinationPorts
        {
            get => _destinationPorts;
            private set
            {
                _destinationPorts = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the connection matrix.
        /// </summary>
        public ObservableCollection<ObservableCollection<AudioConnectionInfo>> ConnectionMatrix
        {
            get => _connectionMatrix;
            private set
            {
                _connectionMatrix = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the collection of source filters.
        /// </summary>
        public ObservableCollection<string> SourceFilters
        {
            get => _sourceFilters;
            private set
            {
                _sourceFilters = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the collection of destination filters.
        /// </summary>
        public ObservableCollection<string> DestinationFilters
        {
            get => _destinationFilters;
            private set
            {
                _destinationFilters = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the selected source filter.
        /// </summary>
        public string SelectedSourceFilter
        {
            get => _selectedSourceFilter;
            set
            {
                _selectedSourceFilter = value;
                OnPropertyChanged();
                Task.Run(RefreshRoutingMatrixAsync);
            }
        }

        /// <summary>
        /// Gets or sets the selected destination filter.
        /// </summary>
        public string SelectedDestinationFilter
        {
            get => _selectedDestinationFilter;
            set
            {
                _selectedDestinationFilter = value;
                OnPropertyChanged();
                Task.Run(RefreshRoutingMatrixAsync);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the view is loading.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a connection is being edited.
        /// </summary>
        public bool IsEditingConnection
        {
            get => _isEditingConnection;
            set
            {
                _isEditingConnection = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the connection being edited.
        /// </summary>
        public AudioConnectionInfo EditingConnection
        {
            get => _editingConnection;
            set
            {
                _editingConnection = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command to refresh the routing matrix.
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// Gets the command to clear all connections.
        /// </summary>
        public ICommand ClearAllCommand { get; }

        /// <summary>
        /// Gets the command to toggle a connection.
        /// </summary>
        public ICommand ToggleConnectionCommand { get; }

        /// <summary>
        /// Gets the command to edit a connection.
        /// </summary>
        public ICommand EditConnectionCommand { get; }

        /// <summary>
        /// Gets the command to apply connection changes.
        /// </summary>
        public ICommand ApplyConnectionChangesCommand { get; }

        /// <summary>
        /// Gets the command to cancel editing a connection.
        /// </summary>
        public ICommand CancelEditConnectionCommand { get; }

        #endregion

        #region Command Methods

        private async Task RefreshRoutingMatrixAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading routing matrix...";

                // Refresh devices and connections from the audio engine
                await _audioDeviceService.RefreshDevicesAsync();
                await _audioDeviceService.RefreshConnectionsAsync();

                // Get all devices
                var devices = _audioDeviceService.GetDevices();

                // Filter source ports
                var sourcePorts = new List<AudioPortInfo>();
                foreach (var device in devices)
                {
                    if (device.OutputPorts != null)
                    {
                        // Apply filter
                        if (SelectedSourceFilter == "All" ||
                            (SelectedSourceFilter == "Physical" && !device.IsVirtual) ||
                            (SelectedSourceFilter == "Virtual" && device.IsVirtual))
                        {
                            foreach (var port in device.OutputPorts)
                            {
                                var portInfo = new AudioPortInfo
                                {
                                    Id = port.Id,
                                    Name = $"{device.Name} - {port.Name}",
                                    DeviceId = device.Id,
                                    PortType = AudioPortType.Output,
                                    Channel = port.Channel,
                                    IsPhysical = !device.IsVirtual
                                };
                                sourcePorts.Add(portInfo);
                            }
                        }
                    }
                }

                // Filter destination ports
                var destinationPorts = new List<AudioPortInfo>();
                foreach (var device in devices)
                {
                    if (device.InputPorts != null)
                    {
                        // Apply filter
                        if (SelectedDestinationFilter == "All" ||
                            (SelectedDestinationFilter == "Physical" && !device.IsVirtual) ||
                            (SelectedDestinationFilter == "Virtual" && device.IsVirtual))
                        {
                            foreach (var port in device.InputPorts)
                            {
                                var portInfo = new AudioPortInfo
                                {
                                    Id = port.Id,
                                    Name = $"{device.Name} - {port.Name}",
                                    DeviceId = device.Id,
                                    PortType = AudioPortType.Input,
                                    Channel = port.Channel,
                                    IsPhysical = !device.IsVirtual
                                };
                                destinationPorts.Add(portInfo);
                            }
                        }
                    }
                }

                // Sort ports by name
                sourcePorts = sourcePorts.OrderBy(p => p.Name).ToList();
                destinationPorts = destinationPorts.OrderBy(p => p.Name).ToList();

                // Get all connections
                var connections = _audioDeviceService.GetConnections();

                // Create the connection matrix
                var matrix = new ObservableCollection<ObservableCollection<AudioConnectionInfo>>();
                for (int i = 0; i < sourcePorts.Count; i++)
                {
                    var row = new ObservableCollection<AudioConnectionInfo>();
                    for (int j = 0; j < destinationPorts.Count; j++)
                    {
                        var sourcePort = sourcePorts[i];
                        var destPort = destinationPorts[j];

                        // Check if a connection exists
                        var connection = connections.FirstOrDefault(c =>
                            c.SourceId == sourcePort.Id && c.DestinationId == destPort.Id);

                        if (connection != null)
                        {
                            // Connection exists
                            row.Add(new AudioConnectionInfo
                            {
                                Id = connection.Id,
                                SourceId = sourcePort.Id,
                                DestinationId = destPort.Id,
                                SourceName = sourcePort.Name,
                                DestinationName = destPort.Name,
                                IsConnected = true,
                                Volume = connection.Volume,
                                Status = connection.Status,
                                SourceRow = i,
                                DestinationColumn = j
                            });
                        }
                        else
                        {
                            // No connection exists
                            row.Add(new AudioConnectionInfo
                            {
                                SourceId = sourcePort.Id,
                                DestinationId = destPort.Id,
                                SourceName = sourcePort.Name,
                                DestinationName = destPort.Name,
                                IsConnected = false,
                                Volume = 1.0,
                                Status = ConnectionStatus.Inactive,
                                SourceRow = i,
                                DestinationColumn = j
                            });
                        }
                    }
                    matrix.Add(row);
                }

                // Update the UI on the UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SourcePorts = new ObservableCollection<AudioPortInfo>(sourcePorts);
                    DestinationPorts = new ObservableCollection<AudioPortInfo>(destinationPorts);
                    ConnectionMatrix = matrix;
                    StatusMessage = $"Loaded {sourcePorts.Count} sources, {destinationPorts.Count} destinations, {connections.Count} connections.";
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading routing matrix: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ClearAllConnectionsAsync()
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear all connections?",
                "Confirm Clear All",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    StatusMessage = "Clearing all connections...";

                    // Get all connections
                    var connections = _audioDeviceService.GetConnections();

                    // Disable all connections
                    foreach (var connection in connections)
                    {
                        await _audioDeviceService.SetConnectionAsync(
                            connection.SourceId,
                            connection.DestinationId,
                            false,
                            connection.Volume);
                    }

                    // Refresh the routing matrix
                    await RefreshRoutingMatrixAsync();

                    StatusMessage = "All connections cleared.";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error clearing connections: {ex.Message}";
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private bool CanClearAllConnections()
        {
            return !IsLoading && ConnectionMatrix != null && ConnectionMatrix.Count > 0;
        }

        private async Task ToggleConnectionAsync(AudioConnectionInfo connection)
        {
            if (connection == null)
            {
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = $"Toggling connection: {connection.SourceName} → {connection.DestinationName}...";

                // Toggle the connection
                bool newState = !connection.IsConnected;
                await _audioDeviceService.SetConnectionAsync(
                    connection.SourceId,
                    connection.DestinationId,
                    newState,
                    connection.Volume);

                // Update the connection in the matrix
                connection.IsConnected = newState;
                connection.Status = newState ? ConnectionStatus.Active : ConnectionStatus.Inactive;

                StatusMessage = $"Connection {(newState ? "enabled" : "disabled")}: {connection.SourceName} → {connection.DestinationName}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error toggling connection: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void EditConnection(AudioConnectionInfo connection)
        {
            if (connection == null)
            {
                return;
            }

            // Create a copy of the connection for editing
            EditingConnection = connection.Clone();
            IsEditingConnection = true;
        }

        private async Task ApplyConnectionChangesAsync()
        {
            if (EditingConnection == null)
            {
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = $"Updating connection: {EditingConnection.SourceName} → {EditingConnection.DestinationName}...";

                // Update the connection
                await _audioDeviceService.SetConnectionAsync(
                    EditingConnection.SourceId,
                    EditingConnection.DestinationId,
                    EditingConnection.IsConnected,
                    EditingConnection.Volume);

                // Find the connection in the matrix and update it
                var connection = ConnectionMatrix[EditingConnection.SourceRow][EditingConnection.DestinationColumn];
                connection.IsConnected = EditingConnection.IsConnected;
                connection.Volume = EditingConnection.Volume;
                connection.Status = EditingConnection.IsConnected ? ConnectionStatus.Active : ConnectionStatus.Inactive;

                StatusMessage = $"Connection updated: {EditingConnection.SourceName} → {EditingConnection.DestinationName}";
                IsEditingConnection = false;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating connection: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanApplyConnectionChanges()
        {
            return EditingConnection != null && !IsLoading;
        }

        private void CancelEditConnection()
        {
            IsEditingConnection = false;
            EditingConnection = null;
        }

        #endregion

        #region Event Handlers

        private void AudioDeviceService_ConnectionsUpdated(object sender, EventArgs e)
        {
            // Refresh the routing matrix when connections are updated
            Task.Run(RefreshRoutingMatrixAsync);
        }

        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// Event raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// Implementation of ICommand for relay commands.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// Initializes a new instance of the RelayCommand class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Event raised when the execution status changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        public void Execute(object parameter)
        {
            _execute();
        }
    }

    /// <summary>
    /// Implementation of ICommand for relay commands with a parameter.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        /// <summary>
        /// Initializes a new instance of the RelayCommand class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Event raised when the execution status changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            return parameter == null || _canExecute == null || _canExecute((T)parameter);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }
}
