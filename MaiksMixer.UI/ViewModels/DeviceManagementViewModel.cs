using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MaiksMixer.Core.Communication.Models;
using MaiksMixer.Core.Services;

namespace MaiksMixer.UI.ViewModels
{
    /// <summary>
    /// View model for the device management interface.
    /// </summary>
    public class DeviceManagementViewModel : INotifyPropertyChanged
    {
        private readonly AudioDeviceService _deviceService;
        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private AudioDevice? _selectedDevice;
        private bool _isCreatingDevice;
        private string _newDeviceName = "New Virtual Device";
        private int _newDeviceInputChannels = 2;
        private int _newDeviceOutputChannels = 2;
        
        /// <summary>
        /// Event raised when a property changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        
        /// <summary>
        /// Gets or sets the collection of devices.
        /// </summary>
        public ObservableCollection<AudioDevice> Devices { get; } = new ObservableCollection<AudioDevice>();
        
        /// <summary>
        /// Gets or sets the collection of physical devices.
        /// </summary>
        public ObservableCollection<AudioDevice> PhysicalDevices { get; } = new ObservableCollection<AudioDevice>();
        
        /// <summary>
        /// Gets or sets the collection of virtual devices.
        /// </summary>
        public ObservableCollection<AudioDevice> VirtualDevices { get; } = new ObservableCollection<AudioDevice>();
        
        /// <summary>
        /// Gets or sets whether the view model is loading data.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        
        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }
        
        /// <summary>
        /// Gets or sets the selected device.
        /// </summary>
        public AudioDevice? SelectedDevice
        {
            get => _selectedDevice;
            set => SetProperty(ref _selectedDevice, value);
        }
        
        /// <summary>
        /// Gets or sets whether the user is creating a new device.
        /// </summary>
        public bool IsCreatingDevice
        {
            get => _isCreatingDevice;
            set => SetProperty(ref _isCreatingDevice, value);
        }
        
        /// <summary>
        /// Gets or sets the name of the new device.
        /// </summary>
        public string NewDeviceName
        {
            get => _newDeviceName;
            set => SetProperty(ref _newDeviceName, value);
        }
        
        /// <summary>
        /// Gets or sets the number of input channels for the new device.
        /// </summary>
        public int NewDeviceInputChannels
        {
            get => _newDeviceInputChannels;
            set => SetProperty(ref _newDeviceInputChannels, value);
        }
        
        /// <summary>
        /// Gets or sets the number of output channels for the new device.
        /// </summary>
        public int NewDeviceOutputChannels
        {
            get => _newDeviceOutputChannels;
            set => SetProperty(ref _newDeviceOutputChannels, value);
        }
        
        /// <summary>
        /// Gets the command to refresh devices.
        /// </summary>
        public ICommand RefreshDevicesCommand { get; }
        
        /// <summary>
        /// Gets the command to create a virtual device.
        /// </summary>
        public ICommand CreateVirtualDeviceCommand { get; }
        
        /// <summary>
        /// Gets the command to remove a virtual device.
        /// </summary>
        public ICommand RemoveVirtualDeviceCommand { get; }
        
        /// <summary>
        /// Gets the command to show the device creation UI.
        /// </summary>
        public ICommand ShowCreateDeviceCommand { get; }
        
        /// <summary>
        /// Gets the command to cancel device creation.
        /// </summary>
        public ICommand CancelCreateDeviceCommand { get; }
        
        /// <summary>
        /// Initializes a new instance of the DeviceManagementViewModel class.
        /// </summary>
        /// <param name="deviceService">The device service to use.</param>
        public DeviceManagementViewModel(AudioDeviceService deviceService)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            
            // Initialize commands
            RefreshDevicesCommand = new RelayCommand(async _ => await RefreshDevicesAsync());
            CreateVirtualDeviceCommand = new RelayCommand(async _ => await CreateVirtualDeviceAsync(), _ => !string.IsNullOrWhiteSpace(NewDeviceName));
            RemoveVirtualDeviceCommand = new RelayCommand(async _ => await RemoveVirtualDeviceAsync(), _ => SelectedDevice != null && SelectedDevice.IsVirtual);
            ShowCreateDeviceCommand = new RelayCommand(_ => IsCreatingDevice = true);
            CancelCreateDeviceCommand = new RelayCommand(_ => IsCreatingDevice = false);
            
            // Register for events
            _deviceService.DevicesUpdated += DeviceService_DevicesUpdated;
            
            // Initial load
            _ = RefreshDevicesAsync();
        }
        
        /// <summary>
        /// Refreshes the list of devices.
        /// </summary>
        private async Task RefreshDevicesAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading devices...";
                
                await _deviceService.RefreshDevicesAsync();
                
                StatusMessage = $"Loaded {_deviceService.GetDevices().Count} devices.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading devices: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        /// <summary>
        /// Creates a virtual device.
        /// </summary>
        private async Task CreateVirtualDeviceAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = $"Creating virtual device '{NewDeviceName}'...";
                
                bool success = await _deviceService.CreateVirtualDeviceAsync(
                    NewDeviceName,
                    NewDeviceInputChannels,
                    NewDeviceOutputChannels);
                
                if (success)
                {
                    StatusMessage = $"Created virtual device '{NewDeviceName}'.";
                    IsCreatingDevice = false;
                    
                    // Reset form for next use
                    NewDeviceName = "New Virtual Device";
                    NewDeviceInputChannels = 2;
                    NewDeviceOutputChannels = 2;
                }
                else
                {
                    StatusMessage = $"Failed to create virtual device '{NewDeviceName}'.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error creating virtual device: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        /// <summary>
        /// Removes a virtual device.
        /// </summary>
        private async Task RemoveVirtualDeviceAsync()
        {
            if (SelectedDevice == null || !SelectedDevice.IsVirtual)
                return;
                
            try
            {
                IsLoading = true;
                StatusMessage = $"Removing virtual device '{SelectedDevice.Name}'...";
                
                bool success = await _deviceService.RemoveVirtualDeviceAsync(SelectedDevice.Id);
                
                if (success)
                {
                    StatusMessage = $"Removed virtual device '{SelectedDevice.Name}'.";
                    SelectedDevice = null;
                }
                else
                {
                    StatusMessage = $"Failed to remove virtual device '{SelectedDevice.Name}'.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error removing virtual device: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        /// <summary>
        /// Handles the DevicesUpdated event from the device service.
        /// </summary>
        private void DeviceService_DevicesUpdated(object? sender, EventArgs e)
        {
            // Update device collections on the UI thread
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateDeviceCollections();
            });
        }
        
        /// <summary>
        /// Updates the device collections.
        /// </summary>
        private void UpdateDeviceCollections()
        {
            // Clear existing collections
            Devices.Clear();
            PhysicalDevices.Clear();
            VirtualDevices.Clear();
            
            // Add devices to collections
            foreach (var device in _deviceService.GetDevices())
            {
                Devices.Add(device);
                
                if (device.IsVirtual)
                {
                    VirtualDevices.Add(device);
                }
                else
                {
                    PhysicalDevices.Add(device);
                }
            }
        }
        
        /// <summary>
        /// Sets a property value and raises the PropertyChanged event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">A reference to the property's backing field.</param>
        /// <param name="value">The new value of the property.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>True if the value was changed, false otherwise.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value))
                return false;
                
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        
        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    /// <summary>
    /// A command that relays its functionality to other objects.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;
        
        /// <summary>
        /// Event raised when the CanExecute state changes.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        
        /// <summary>
        /// Initializes a new instance of the RelayCommand class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        
        /// <summary>
        /// Determines whether this command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <returns>True if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }
        
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}
