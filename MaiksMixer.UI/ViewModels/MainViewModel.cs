using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MaiksMixer.Core;
using MaiksMixer.UI.ViewModels;

namespace MaiksMixer.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the main window of the application.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private string _statusMessage = string.Empty;
        private string _engineStatus = string.Empty;
        private string _sampleRate = string.Empty;
        private string _bufferSize = string.Empty;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            // Initialize properties
            StatusMessage = "Ready";
            EngineStatus = "Disconnected";
            SampleRate = "48000 Hz";
            BufferSize = "256";

            // Initialize commands
            ExitCommand = new RelayCommand(ExecuteExit);
            NewConfigurationCommand = new RelayCommand(ExecuteNewConfiguration);
            OpenConfigurationCommand = new RelayCommand(ExecuteOpenConfiguration);
            SaveConfigurationCommand = new RelayCommand(ExecuteSaveConfiguration);
            SaveConfigurationAsCommand = new RelayCommand(ExecuteSaveConfigurationAs);
            ManageDevicesCommand = new RelayCommand(ExecuteManageDevices);
            RefreshDevicesCommand = new RelayCommand(ExecuteRefreshDevices);
            ShowPreferencesCommand = new RelayCommand(ExecuteShowPreferences);
            ShowAboutCommand = new RelayCommand(ExecuteShowAbout);

            // Initialize collections
            InputDevices = new ObservableCollection<string>
            {
                "Microphone",
                "Line In",
                "Virtual Input 1"
            };

            OutputDevices = new ObservableCollection<string>
            {
                "Speakers",
                "Headphones",
                "Virtual Output 1"
            };

            MixerChannels = new ObservableCollection<ChannelViewModel>
            {
                new ChannelViewModel { Name = "Channel 1", Volume = 0.75, IsMuted = false, IsSoloed = false },
                new ChannelViewModel { Name = "Channel 2", Volume = 0.8, IsMuted = false, IsSoloed = false }
            };
        }

        #region Properties

        /// <summary>
        /// Gets or sets the status message displayed in the status bar.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// Gets or sets the engine connection status.
        /// </summary>
        public string EngineStatus
        {
            get => _engineStatus;
            set => SetProperty(ref _engineStatus, value);
        }

        /// <summary>
        /// Gets or sets the current sample rate.
        /// </summary>
        public string SampleRate
        {
            get => _sampleRate;
            set => SetProperty(ref _sampleRate, value);
        }

        /// <summary>
        /// Gets or sets the current buffer size.
        /// </summary>
        public string BufferSize
        {
            get => _bufferSize;
            set => SetProperty(ref _bufferSize, value);
        }

        /// <summary>
        /// Gets the collection of input devices.
        /// </summary>
        public ObservableCollection<string> InputDevices { get; }

        /// <summary>
        /// Gets the collection of output devices.
        /// </summary>
        public ObservableCollection<string> OutputDevices { get; }

        /// <summary>
        /// Gets the collection of mixer channels.
        /// </summary>
        public ObservableCollection<ChannelViewModel> MixerChannels { get; }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command to exit the application.
        /// </summary>
        public ICommand ExitCommand { get; }

        /// <summary>
        /// Gets the command to create a new configuration.
        /// </summary>
        public ICommand NewConfigurationCommand { get; }

        /// <summary>
        /// Gets the command to open an existing configuration.
        /// </summary>
        public ICommand OpenConfigurationCommand { get; }

        /// <summary>
        /// Gets the command to save the current configuration.
        /// </summary>
        public ICommand SaveConfigurationCommand { get; }

        /// <summary>
        /// Gets the command to save the current configuration with a new name.
        /// </summary>
        public ICommand SaveConfigurationAsCommand { get; }

        /// <summary>
        /// Gets the command to manage virtual devices.
        /// </summary>
        public ICommand ManageDevicesCommand { get; }

        /// <summary>
        /// Gets the command to refresh the device list.
        /// </summary>
        public ICommand RefreshDevicesCommand { get; }

        /// <summary>
        /// Gets the command to show the preferences dialog.
        /// </summary>
        public ICommand ShowPreferencesCommand { get; }

        /// <summary>
        /// Gets the command to show the about dialog.
        /// </summary>
        public ICommand ShowAboutCommand { get; }

        #endregion

        #region Command Handlers

        private void ExecuteExit(object parameter)
        {
            // TODO: Implement application exit logic
            System.Windows.Application.Current.Shutdown();
        }

        private void ExecuteNewConfiguration(object parameter)
        {
            // TODO: Implement new configuration logic
            StatusMessage = "Creating new configuration...";
        }

        private void ExecuteOpenConfiguration(object parameter)
        {
            // TODO: Implement open configuration logic
            StatusMessage = "Opening configuration...";
        }

        private void ExecuteSaveConfiguration(object parameter)
        {
            // TODO: Implement save configuration logic
            StatusMessage = "Saving configuration...";
        }

        private void ExecuteSaveConfigurationAs(object parameter)
        {
            // TODO: Implement save configuration as logic
            StatusMessage = "Saving configuration as...";
        }

        private void ExecuteManageDevices(object parameter)
        {
            // TODO: Implement manage devices logic
            StatusMessage = "Managing virtual devices...";
        }

        private void ExecuteRefreshDevices(object parameter)
        {
            // TODO: Implement refresh devices logic
            StatusMessage = "Refreshing device list...";
        }

        private void ExecuteShowPreferences(object parameter)
        {
            // TODO: Implement show preferences logic
            StatusMessage = "Showing preferences...";
        }

        private void ExecuteShowAbout(object parameter)
        {
            // TODO: Implement show about logic
            StatusMessage = "Showing about dialog...";
        }

        #endregion
    }
}
