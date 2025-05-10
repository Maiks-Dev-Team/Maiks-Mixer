using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MaiksMixer.Core;
using MaiksMixer.UI.Services.Audio;

namespace MaiksMixer.UI.ViewModels
{
    /// <summary>
    /// ViewModel for JACK audio functionality
    /// </summary>
    public class JackAudioViewModel : ViewModelBase
    {
        private readonly IJackAudioService _jackAudioService;
        private bool _isInitialized;
        private bool _isActivated;
        private bool _isServerRunning;
        private int _sampleRate;
        private int _bufferSize;
        private float _cpuLoad;
        private string _statusMessage = "Not connected to JACK server";
        private ObservableCollection<JackPortViewModel> _ports = new();

        /// <summary>
        /// Gets or sets whether the JACK client is initialized
        /// </summary>
        public bool IsInitialized
        {
            get => _isInitialized;
            private set => SetProperty(ref _isInitialized, value);
        }

        /// <summary>
        /// Gets or sets whether the JACK client is activated
        /// </summary>
        public bool IsActivated
        {
            get => _isActivated;
            private set => SetProperty(ref _isActivated, value);
        }

        /// <summary>
        /// Gets or sets whether the JACK server is running
        /// </summary>
        public bool IsServerRunning
        {
            get => _isServerRunning;
            private set => SetProperty(ref _isServerRunning, value);
        }

        /// <summary>
        /// Gets or sets the sample rate from the JACK server
        /// </summary>
        public int SampleRate
        {
            get => _sampleRate;
            private set => SetProperty(ref _sampleRate, value);
        }

        /// <summary>
        /// Gets or sets the buffer size from the JACK server
        /// </summary>
        public int BufferSize
        {
            get => _bufferSize;
            private set => SetProperty(ref _bufferSize, value);
        }

        /// <summary>
        /// Gets or sets the CPU load from the JACK server
        /// </summary>
        public float CpuLoad
        {
            get => _cpuLoad;
            private set => SetProperty(ref _cpuLoad, value);
        }

        /// <summary>
        /// Gets or sets the status message
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// Gets the collection of JACK ports
        /// </summary>
        public ObservableCollection<JackPortViewModel> Ports
        {
            get => _ports;
            private set => SetProperty(ref _ports, value);
        }

        /// <summary>
        /// Command to initialize the JACK client
        /// </summary>
        public ICommand InitializeCommand { get; }

        /// <summary>
        /// Command to activate the JACK client
        /// </summary>
        public ICommand ActivateCommand { get; }

        /// <summary>
        /// Command to deactivate the JACK client
        /// </summary>
        public ICommand DeactivateCommand { get; }

        /// <summary>
        /// Command to refresh the port list
        /// </summary>
        public ICommand RefreshPortsCommand { get; }

        /// <summary>
        /// Creates a new instance of the JackAudioViewModel
        /// </summary>
        /// <param name="jackAudioService">The JACK audio service</param>
        public JackAudioViewModel(IJackAudioService jackAudioService)
        {
            _jackAudioService = jackAudioService ?? throw new ArgumentNullException(nameof(jackAudioService));
            
            // Set up commands
            InitializeCommand = new RelayCommand(Initialize, CanInitialize);
            ActivateCommand = new RelayCommand(Activate, CanActivate);
            DeactivateCommand = new RelayCommand(Deactivate, CanDeactivate);
            RefreshPortsCommand = new RelayCommand(RefreshPorts, CanRefreshPorts);
            
            // Set up event handlers
            _jackAudioService.ServerStatusChanged += OnServerStatusChanged;
            _jackAudioService.MeterUpdated += OnMeterUpdated;
        }

        /// <summary>
        /// Initializes the JACK client
        /// </summary>
        private void Initialize()
        {
            try
            {
                IsInitialized = _jackAudioService.Initialize("MaiksMixer");
                
                if (IsInitialized)
                {
                    StatusMessage = "JACK client initialized";
                    UpdateServerStatus();
                }
                else
                {
                    StatusMessage = "Failed to initialize JACK client";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error initializing JACK client: {ex.Message}";
            }
        }

        /// <summary>
        /// Determines whether the JACK client can be initialized
        /// </summary>
        private bool CanInitialize()
        {
            return !IsInitialized;
        }

        /// <summary>
        /// Activates the JACK client
        /// </summary>
        private void Activate()
        {
            try
            {
                // Create ports (2 inputs, 2 outputs for stereo)
                if (!_jackAudioService.CreatePorts(2, 2))
                {
                    StatusMessage = "Failed to create JACK ports";
                    return;
                }
                
                IsActivated = _jackAudioService.Activate();
                
                if (IsActivated)
                {
                    StatusMessage = "JACK client activated";
                    RefreshPorts();
                }
                else
                {
                    StatusMessage = "Failed to activate JACK client";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error activating JACK client: {ex.Message}";
            }
        }

        /// <summary>
        /// Determines whether the JACK client can be activated
        /// </summary>
        private bool CanActivate()
        {
            return IsInitialized && !IsActivated && IsServerRunning;
        }

        /// <summary>
        /// Deactivates the JACK client
        /// </summary>
        private void Deactivate()
        {
            try
            {
                IsActivated = !_jackAudioService.Deactivate();
                
                if (!IsActivated)
                {
                    StatusMessage = "JACK client deactivated";
                }
                else
                {
                    StatusMessage = "Failed to deactivate JACK client";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deactivating JACK client: {ex.Message}";
            }
        }

        /// <summary>
        /// Determines whether the JACK client can be deactivated
        /// </summary>
        private bool CanDeactivate()
        {
            return IsInitialized && IsActivated;
        }

        /// <summary>
        /// Refreshes the port list
        /// </summary>
        private void RefreshPorts()
        {
            try
            {
                var ports = _jackAudioService.GetPorts();
                Ports.Clear();
                
                foreach (var port in ports)
                {
                    Ports.Add(new JackPortViewModel(port));
                }
                
                StatusMessage = $"Found {Ports.Count} JACK ports";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error refreshing JACK ports: {ex.Message}";
            }
        }

        /// <summary>
        /// Determines whether the port list can be refreshed
        /// </summary>
        private bool CanRefreshPorts()
        {
            return IsInitialized && IsServerRunning;
        }

        /// <summary>
        /// Updates the server status
        /// </summary>
        private void UpdateServerStatus()
        {
            if (!IsInitialized) return;
            
            try
            {
                var status = _jackAudioService.GetServerStatus();
                IsServerRunning = status.IsRunning;
                SampleRate = status.SampleRate;
                BufferSize = status.BufferSize;
                CpuLoad = status.CpuLoad;
                
                if (IsServerRunning)
                {
                    StatusMessage = $"JACK server running at {SampleRate} Hz, {BufferSize} frames, {CpuLoad:F1}% CPU";
                }
                else
                {
                    StatusMessage = "JACK server not running";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error getting JACK server status: {ex.Message}";
            }
        }

        /// <summary>
        /// Handles the server status changed event
        /// </summary>
        private void OnServerStatusChanged(object? sender, bool isRunning)
        {
            IsServerRunning = isRunning;
            UpdateServerStatus();
        }

        /// <summary>
        /// Handles the meter update event
        /// </summary>
        private void OnMeterUpdated(object? sender, MeterUpdateEventArgs e)
        {
            // Update channel meter data in the UI
            // This would typically update a specific channel's meter display
        }
    }

    /// <summary>
    /// ViewModel for a JACK port
    /// </summary>
    public class JackPortViewModel : ViewModelBase
    {
        private readonly JackPort _port;

        /// <summary>
        /// Gets the port name
        /// </summary>
        public string Name => _port.Name;

        /// <summary>
        /// Gets the port type
        /// </summary>
        public string Type => _port.Type;

        /// <summary>
        /// Gets whether the port is an input port
        /// </summary>
        public bool IsInput => _port.IsInput;

        /// <summary>
        /// Gets whether the port is an output port
        /// </summary>
        public bool IsOutput => _port.IsOutput;

        /// <summary>
        /// Gets whether the port is a physical port
        /// </summary>
        public bool IsPhysical => _port.IsPhysical;

        /// <summary>
        /// Gets the list of connections to this port
        /// </summary>
        public ObservableCollection<string> Connections { get; }

        /// <summary>
        /// Creates a new instance of the JackPortViewModel
        /// </summary>
        /// <param name="port">The JACK port</param>
        public JackPortViewModel(JackPort port)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
            Connections = new ObservableCollection<string>(port.Connections);
        }
    }
}
