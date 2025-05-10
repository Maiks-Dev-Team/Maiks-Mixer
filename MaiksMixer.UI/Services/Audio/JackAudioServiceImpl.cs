using System;
using System.Collections.Generic;

namespace MaiksMixer.UI.Services.Audio
{
    /// <summary>
    /// Implementation of the IJackAudioService interface
    /// </summary>
    public class JackAudioServiceImpl : IJackAudioService, IDisposable
    {
        private bool _isDisposed = false;
        private bool _isInitialized = false;
        private bool _isActivated = false;
        private bool _isServerRunning = false;
        private float _cpuLoad = 0.0f;
        private int _sampleRate = 48000;
        private int _bufferSize = 1024;
        private string _clientName = string.Empty;
        private List<JackPort> _inputPorts = new List<JackPort>();
        private List<JackPort> _outputPorts = new List<JackPort>();
        private Dictionary<int, float> _channelVolumes = new Dictionary<int, float>();
        private Dictionary<int, float> _channelPans = new Dictionary<int, float>();

        /// <summary>
        /// Event raised when the JACK server status changes
        /// </summary>
        public event EventHandler<bool>? ServerStatusChanged;

        /// <summary>
        /// Event raised when meter data is updated for a channel
        /// </summary>
        public event EventHandler<MeterUpdateEventArgs>? MeterUpdated;

        /// <summary>
        /// Initializes a new instance of the JackAudioServiceImpl class
        /// </summary>
        public JackAudioServiceImpl()
        {
            // Initialize with default values
            _isServerRunning = false;
            _isInitialized = false;
            _isActivated = false;
        }

        /// <summary>
        /// Initializes the JACK client
        /// </summary>
        /// <param name="clientName">Name of the JACK client</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool Initialize(string clientName)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioServiceImpl));
            
            // Mock implementation - simulate successful initialization
            _clientName = clientName;
            _isInitialized = true;
            _isServerRunning = true;
            
            // Raise the server status changed event
            ServerStatusChanged?.Invoke(this, _isServerRunning);
            
            return true;
        }

        /// <summary>
        /// Creates input and output ports
        /// </summary>
        /// <param name="numInputs">Number of input ports to create</param>
        /// <param name="numOutputs">Number of output ports to create</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool CreatePorts(int numInputs, int numOutputs)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioServiceImpl));
            if (!_isInitialized) return false;
            
            // Mock implementation - create mock ports
            _inputPorts.Clear();
            _outputPorts.Clear();
            
            for (int i = 0; i < numInputs; i++)
            {
                _inputPorts.Add(new JackPort
                {
                    Name = $"input_{i+1}",
                    Type = "audio",
                    Flags = 1 // Input flag
                });
            }
            
            for (int i = 0; i < numOutputs; i++)
            {
                _outputPorts.Add(new JackPort
                {
                    Name = $"output_{i+1}",
                    Type = "audio",
                    Flags = 2 // Output flag
                });
            }
            
            return true;
        }

        /// <summary>
        /// Activates the JACK client
        /// </summary>
        /// <returns>True if activation was successful, false otherwise</returns>
        public bool Activate()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioServiceImpl));
            if (!_isInitialized) return false;
            
            // Mock implementation - simulate successful activation
            _isActivated = true;
            
            // Start a timer to simulate meter updates
            StartMeterUpdateTimer();
            
            _isServerRunning = true;
            ServerStatusChanged?.Invoke(this, true);
            return true;
        }
        
        private System.Timers.Timer? _meterUpdateTimer;
        
        private void StartMeterUpdateTimer()
        {
            _meterUpdateTimer = new System.Timers.Timer(100); // 100ms interval
            _meterUpdateTimer.Elapsed += (sender, e) => {
                // Generate random meter data for each input port
                Random random = new Random();
                for (int i = 0; i < _inputPorts.Count; i++)
                {
                    var meterData = new MeterData
                    {
                        Peak = (float)(random.NextDouble() * 0.8),
                        RMS = (float)(random.NextDouble() * 0.5)
                    };
                    
                    MeterUpdated?.Invoke(this, new MeterUpdateEventArgs(i, meterData));
                }
            };
            _meterUpdateTimer.Start();
        }

        /// <summary>
        /// Deactivates the JACK client
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        public bool Deactivate()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioServiceImpl));
            if (!_isActivated) return false;
            
            // Mock implementation - simulate successful deactivation
            _isActivated = false;
            
            // Stop the meter update timer
            if (_meterUpdateTimer != null)
            {
                _meterUpdateTimer.Stop();
                _meterUpdateTimer.Dispose();
                _meterUpdateTimer = null;
            }
            
            ServerStatusChanged?.Invoke(this, false);
            return true;
        }

        /// <summary>
        /// Sets the volume for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="volume">Volume value (0.0 - 1.0)</param>
        public void SetChannelVolume(int channel, float volume)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioServiceImpl));
            if (volume < 0.0f || volume > 1.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be between 0.0 and 1.0");
            }
            _channelVolumes[channel] = volume;
        }

        /// <summary>
        /// Sets the pan for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="pan">Pan value (0.0 left, 0.5 center, 1.0 right)</param>
        public void SetChannelPan(int channel, float pan)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioServiceImpl));
            if (pan < 0.0f || pan > 1.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(pan), "Pan must be between 0.0 and 1.0");
            }
            _channelPans[channel] = pan;
        }

        /// <summary>
        /// Sets the gain for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="gainDB">Gain value in dB</param>
        public void SetChannelGain(int channel, float gainDB)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioServiceImpl));
            // Mock implementation - no actual functionality needed for now
        }

        /// <summary>
        /// Sets the mute state for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="mute">Mute state</param>
        public void SetChannelMute(int channel, bool mute)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioServiceImpl));
            // Mock implementation - no actual functionality needed for now
        }

        /// <summary>
        /// Sets the solo state for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="solo">Solo state</param>
        public void SetChannelSolo(int channel, bool solo)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioServiceImpl));
            // Mock implementation - no actual functionality needed for now
        }

        /// <summary>
        /// Gets the meter data for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <returns>MeterData object with peak and RMS values</returns>
        public MeterData GetChannelMeter(int channel)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioServiceImpl));
            
            // Mock implementation - generate random meter data
            Random random = new Random();
            return new MeterData
            {
                Peak = (float)(random.NextDouble() * 0.8),
                RMS = (float)(random.NextDouble() * 0.5)
            };
        }

        /// <summary>
        /// Connects two JACK ports
        /// </summary>
        /// <param name="sourcePort">Source port name</param>
        /// <param name="destPort">Destination port name</param>
        /// <returns>True if connection was successful, false otherwise</returns>
        public bool ConnectPorts(string sourcePort, string destPort)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioServiceImpl));
            if (!_isActivated) return false;
            
            // Mock implementation - simulate successful connection
            // Find the source and destination ports in our lists
            var source = _inputPorts.FirstOrDefault(p => p.Name == sourcePort) ?? 
                         _outputPorts.FirstOrDefault(p => p.Name == sourcePort);
            var dest = _inputPorts.FirstOrDefault(p => p.Name == destPort) ?? 
                       _outputPorts.FirstOrDefault(p => p.Name == destPort);
            
            if (source != null && dest != null)
            {
                // Add the connection to the source port's connections list
                if (!source.Connections.Contains(destPort))
                {
                    source.Connections.Add(destPort);
                }
                
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Disconnects two JACK ports
        /// </summary>
        /// <param name="sourcePort">Source port name</param>
        /// <param name="destPort">Destination port name</param>
        /// <returns>True if disconnection was successful, false otherwise</returns>
        public bool DisconnectPorts(string sourcePort, string destPort)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioServiceImpl));
            if (!_isActivated) return false;
            
            // Mock implementation - simulate successful disconnection
            // Find the source port in our lists
            var source = _inputPorts.FirstOrDefault(p => p.Name == sourcePort) ?? 
                         _outputPorts.FirstOrDefault(p => p.Name == sourcePort);
            
            if (source != null)
            {
                // Remove the connection from the source port's connections list
                if (source.Connections.Contains(destPort))
                {
                    source.Connections.Remove(destPort);
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Gets a list of all available JACK ports
        /// </summary>
        /// <param name="portType">Port type filter (e.g., "audio")</param>
        /// <param name="flags">Port flags filter</param>
        /// <returns>List of port names</returns>
        public List<string> GetPortList(string portType, uint flags)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioServiceImpl));
            
            // Mock implementation - return a list of port names based on our internal ports
            List<string> portNames = new List<string>();
            
            // Add input ports if they match the filter
            if (portType == "audio" && (flags & 1) != 0)
            {
                portNames.AddRange(_inputPorts.Where(p => p.Name != null).Select(p => p.Name!));
            }
            
            // Add output ports if they match the filter
            if (portType == "audio" && (flags & 2) != 0)
            {
                portNames.AddRange(_outputPorts.Where(p => p.Name != null).Select(p => p.Name!));
            }
            
            // Add some mock system ports for testing
            if (portType == "audio")
            {
                portNames.Add("system:playback_1");
                portNames.Add("system:playback_2");
                portNames.Add("system:capture_1");
                portNames.Add("system:capture_2");
            }
            
            return portNames;
        }

        /// <summary>
        /// Gets a list of all available JACK ports
        /// </summary>
        /// <returns>List of JackPort objects</returns>
        public List<JackPort> GetPorts()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioServiceImpl));
            
            // Mock implementation - return a combined list of our internal ports plus some system ports
            List<JackPort> ports = new List<JackPort>();
            
            // Add our input and output ports
            ports.AddRange(_inputPorts);
            ports.AddRange(_outputPorts);
            
            // Add some mock system ports for testing
            ports.Add(new JackPort("system:playback_1", "audio", 2));
            ports.Add(new JackPort("system:playback_2", "audio", 2));
            ports.Add(new JackPort("system:capture_1", "audio", 1));
            ports.Add(new JackPort("system:capture_2", "audio", 1));
            
            return ports;
        }

        /// <summary>
        /// Gets the sample rate from the JACK server
        /// </summary>
        /// <returns>Sample rate in Hz</returns>
        public int GetSampleRate()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioServiceImpl));
            return _sampleRate;
        }

        /// <summary>
        /// Gets the buffer size from the JACK server
        /// </summary>
        /// <returns>Buffer size in frames</returns>
        public int GetBufferSize()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioServiceImpl));
            return _bufferSize;
        }

        /// <summary>
        /// Gets the CPU load from the JACK server
        /// </summary>
        /// <returns>CPU load as a percentage (0.0 - 100.0)</returns>
        public float GetCpuLoad()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioServiceImpl));
            return _cpuLoad;
        }

        /// <summary>
        /// Checks if the JACK server is running
        /// </summary>
        /// <returns>True if running, false otherwise</returns>
        public bool IsServerRunning()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioServiceImpl));
            return _isServerRunning;
        }

        /// <summary>
        /// Gets the current JACK server status
        /// </summary>
        /// <returns>JackServerStatus object with server information</returns>
        public JackServerStatus GetServerStatus()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioServiceImpl));
            return new JackServerStatus
            {
                IsRunning = _isServerRunning,
                SampleRate = _sampleRate,
                BufferSize = _bufferSize,
                CpuLoad = _cpuLoad
            };
        }

        /// <summary>
        /// Handles the server status changed event from the JackAudioService
        /// </summary>
        private void OnServerStatusChanged(object sender, bool isRunning)
        {
            ServerStatusChanged?.Invoke(this, isRunning);
        }

        /// <summary>
        /// Handles the meter update event from the JackAudioService
        /// </summary>
        private void OnMeterUpdated(object sender, MeterUpdateEventArgs e)
        {
            MeterUpdated?.Invoke(this, e);
        }

        /// <summary>
        /// Disposes the JackAudioServiceImpl instance
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            
            // Clean up any resources
            if (_isActivated)
            {
                Deactivate();
            }
            
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
