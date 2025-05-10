using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using MaiksMixer.Core.Models;
using MaiksMixer.Core.Services;

namespace MaiksMixer.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the device properties editor.
    /// </summary>
    public class DevicePropertiesViewModel : INotifyPropertyChanged
    {
        private AudioDeviceInfo _device;
        private AudioDeviceInfo _originalDevice;
        private ObservableCollection<KeyValuePair<string, string>> _deviceProperties;
        private bool _isVirtualDevice;
        private bool _canChangeSampleRate;
        private bool _canChangeBufferSize;
        private string _deviceTitle;
        
        /// <summary>
        /// Initializes a new instance of the DevicePropertiesViewModel class.
        /// </summary>
        /// <param name="device">The audio device to edit.</param>
        public DevicePropertiesViewModel(AudioDeviceInfo device)
        {
            _originalDevice = device ?? throw new ArgumentNullException(nameof(device));
            _device = _originalDevice.Clone();
            
            // Initialize commands
            ApplyChangesCommand = new RelayCommand(ApplyChanges);
            ResetChangesCommand = new RelayCommand(ResetChanges);
            EditPortCommand = new RelayCommand<AudioPortInfo>(EditPort);
            RemovePortCommand = new RelayCommand<AudioPortInfo>(RemovePort, CanRemovePort);
            RemoveDeviceCommand = new RelayCommand(RemoveDevice, CanRemoveDevice);
            
            // Set properties
            IsVirtualDevice = device.DeviceType == AudioDeviceType.Virtual;
            CanChangeSampleRate = IsVirtualDevice || device.DeviceType == AudioDeviceType.Jack || device.DeviceType == AudioDeviceType.Asio;
            CanChangeBufferSize = CanChangeSampleRate;
            DeviceTitle = $"Device Properties: {device.Name}";
            
            // Initialize device properties collection
            UpdateDeviceProperties();
        }
        
        #region Properties
        
        /// <summary>
        /// Gets or sets the audio device being edited.
        /// </summary>
        public AudioDeviceInfo Device
        {
            get => _device;
            set
            {
                _device = value;
                OnPropertyChanged();
                UpdateDeviceProperties();
            }
        }
        
        /// <summary>
        /// Gets the collection of device properties.
        /// </summary>
        public ObservableCollection<KeyValuePair<string, string>> DeviceProperties
        {
            get => _deviceProperties;
            private set
            {
                _deviceProperties = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the device is a virtual device.
        /// </summary>
        public bool IsVirtualDevice
        {
            get => _isVirtualDevice;
            set
            {
                _isVirtualDevice = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the sample rate can be changed.
        /// </summary>
        public bool CanChangeSampleRate
        {
            get => _canChangeSampleRate;
            set
            {
                _canChangeSampleRate = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the buffer size can be changed.
        /// </summary>
        public bool CanChangeBufferSize
        {
            get => _canChangeBufferSize;
            set
            {
                _canChangeBufferSize = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets the device title.
        /// </summary>
        public string DeviceTitle
        {
            get => _deviceTitle;
            set
            {
                _deviceTitle = value;
                OnPropertyChanged();
            }
        }
        
        #endregion
        
        #region Commands
        
        /// <summary>
        /// Gets the command to apply changes to the device.
        /// </summary>
        public ICommand ApplyChangesCommand { get; }
        
        /// <summary>
        /// Gets the command to reset changes to the device.
        /// </summary>
        public ICommand ResetChangesCommand { get; }
        
        /// <summary>
        /// Gets the command to edit a port.
        /// </summary>
        public ICommand EditPortCommand { get; }
        
        /// <summary>
        /// Gets the command to remove a port.
        /// </summary>
        public ICommand RemovePortCommand { get; }
        
        /// <summary>
        /// Gets the command to remove the device.
        /// </summary>
        public ICommand RemoveDeviceCommand { get; }
        
        #endregion
        
        #region Command Methods
        
        private void ApplyChanges()
        {
            try
            {
                // Update properties from the collection
                Device.Properties.Clear();
                foreach (var property in DeviceProperties)
                {
                    if (!string.IsNullOrWhiteSpace(property.Key))
                    {
                        Device.Properties[property.Key] = property.Value ?? string.Empty;
                    }
                }
                
                // In a real implementation, this would call the audio device service to update the device
                // var audioDeviceService = ServiceLocator.GetService<AudioDeviceService>();
                // audioDeviceService.UpdateDeviceAsync(Device);
                
                MessageBox.Show("Device properties updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Update the original device with the changes
                _originalDevice = Device.Clone();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying changes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void ResetChanges()
        {
            // Reset to the original device
            Device = _originalDevice.Clone();
            
            // Update the title
            DeviceTitle = $"Device Properties: {Device.Name}";
        }
        
        private void EditPort(AudioPortInfo port)
        {
            if (port == null)
            {
                return;
            }
            
            // In a real implementation, this would show a dialog to edit the port
            MessageBox.Show($"Editing port: {port.Name}", "Edit Port", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void RemovePort(AudioPortInfo port)
        {
            if (port == null)
            {
                return;
            }
            
            var result = MessageBox.Show(
                $"Are you sure you want to remove the port '{port.Name}'?",
                "Confirm Remove",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
                
            if (result == MessageBoxResult.Yes)
            {
                // Remove the port from the device
                if (port.PortType == AudioPortType.Input)
                {
                    Device.InputPorts.Remove(port);
                }
                else if (port.PortType == AudioPortType.Output)
                {
                    Device.OutputPorts.Remove(port);
                }
                
                // Update the device properties
                OnPropertyChanged(nameof(Device));
            }
        }
        
        private bool CanRemovePort(AudioPortInfo port)
        {
            return IsVirtualDevice && port != null;
        }
        
        private void RemoveDevice()
        {
            var result = MessageBox.Show(
                $"Are you sure you want to remove the device '{Device.Name}'? This action cannot be undone.",
                "Confirm Remove",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
                
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // In a real implementation, this would call the audio device service to remove the device
                    // var audioDeviceService = ServiceLocator.GetService<AudioDeviceService>();
                    // audioDeviceService.RemoveVirtualDeviceAsync(Device.Id);
                    
                    MessageBox.Show("Device removed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Close the window or navigate back
                    // TODO: Implement navigation back to device list
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error removing device: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private bool CanRemoveDevice()
        {
            return IsVirtualDevice;
        }
        
        #endregion
        
        #region Helper Methods
        
        private void UpdateDeviceProperties()
        {
            if (Device?.Properties == null)
            {
                DeviceProperties = new ObservableCollection<KeyValuePair<string, string>>();
                return;
            }
            
            DeviceProperties = new ObservableCollection<KeyValuePair<string, string>>(
                Device.Properties.Select(p => new KeyValuePair<string, string>(p.Key, p.Value)));
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
