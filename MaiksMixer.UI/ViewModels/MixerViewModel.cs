using System;
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
    /// ViewModel for the mixer view.
    /// </summary>
    public class MixerViewModel : INotifyPropertyChanged
    {
        private readonly AudioDeviceService _audioDeviceService;
        private ObservableCollection<MixerChannelViewModel> _channels;
        private ObservableCollection<string> _deviceFilters;
        private string _selectedDeviceFilter;
        private bool _isLoading;
        private string _statusMessage;

        /// <summary>
        /// Initializes a new instance of the MixerViewModel class.
        /// </summary>
        /// <param name="audioDeviceService">The audio device service to use.</param>
        public MixerViewModel(AudioDeviceService audioDeviceService)
        {
            _audioDeviceService = audioDeviceService ?? throw new ArgumentNullException(nameof(audioDeviceService));
            
            // Initialize collections
            Channels = new ObservableCollection<MixerChannelViewModel>();
            DeviceFilters = new ObservableCollection<string> { "All", "Physical", "Virtual" };
            
            // Set default filter
            SelectedDeviceFilter = "All";
            
            // Initialize commands
            RefreshCommand = new RelayCommand(async () => await RefreshChannelsAsync());
            ResetAllCommand = new RelayCommand(ResetAllChannels, CanResetAllChannels);
            
            // Register for events from the audio device service
            _audioDeviceService.DevicesUpdated += AudioDeviceService_DevicesUpdated;
            
            // Load the mixer channels
            Task.Run(RefreshChannelsAsync);
        }

        #region Properties

        /// <summary>
        /// Gets the collection of mixer channels.
        /// </summary>
        public ObservableCollection<MixerChannelViewModel> Channels
        {
            get => _channels;
            private set
            {
                _channels = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the collection of device filters.
        /// </summary>
        public ObservableCollection<string> DeviceFilters
        {
            get => _deviceFilters;
            private set
            {
                _deviceFilters = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the selected device filter.
        /// </summary>
        public string SelectedDeviceFilter
        {
            get => _selectedDeviceFilter;
            set
            {
                _selectedDeviceFilter = value;
                OnPropertyChanged();
                Task.Run(RefreshChannelsAsync);
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
        /// Gets the command to refresh the mixer channels.
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// Gets the command to reset all mixer channels.
        /// </summary>
        public ICommand ResetAllCommand { get; }

        #endregion

        #region Command Methods

        private async Task RefreshChannelsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading mixer channels...";

                // Refresh devices from the audio engine
                await _audioDeviceService.RefreshDevicesAsync();

                // Get all devices
                var devices = _audioDeviceService.GetDevices();

                // Filter devices
                if (SelectedDeviceFilter != "All")
                {
                    bool isVirtual = SelectedDeviceFilter == "Virtual";
                    devices = devices.Where(d => d.IsVirtual == isVirtual).ToList();
                }

                // Create channel view models for each device port
                var channelViewModels = new ObservableCollection<MixerChannelViewModel>();
                foreach (var device in devices)
                {
                    // Add output ports (for monitoring)
                    if (device.OutputPorts != null)
                    {
                        foreach (var port in device.OutputPorts)
                        {
                            var channelViewModel = new MixerChannelViewModel(_audioDeviceService, device, port);
                            channelViewModels.Add(channelViewModel);
                        }
                    }

                    // Add input ports (for recording)
                    if (device.InputPorts != null)
                    {
                        foreach (var port in device.InputPorts)
                        {
                            var channelViewModel = new MixerChannelViewModel(_audioDeviceService, device, port);
                            channelViewModels.Add(channelViewModel);
                        }
                    }
                }

                // Update the UI on the UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Channels = channelViewModels;
                    StatusMessage = $"Loaded {channelViewModels.Count} mixer channels.";
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading mixer channels: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ResetAllChannels()
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset all mixer channels to default settings?",
                "Confirm Reset All",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Reset all channels
                foreach (var channel in Channels)
                {
                    // Use the channel's reset command
                    channel.ResetCommand.Execute(null);
                }

                StatusMessage = "All mixer channels reset to default settings.";
            }
        }

        private bool CanResetAllChannels()
        {
            return !IsLoading && Channels != null && Channels.Count > 0;
        }

        #endregion

        #region Event Handlers

        private void AudioDeviceService_DevicesUpdated(object sender, EventArgs e)
        {
            // Refresh the mixer channels when devices are updated
            Task.Run(RefreshChannelsAsync);
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
}
