using System;
using System.Collections.Generic;
using MaiksMixer.Core;

namespace MaiksMixer.Audio
{
    /// <summary>
    /// Service for interacting with the JACK audio system through the C++/CLI bridge
    /// </summary>
    public class JackAudioService : IDisposable
    {
        private readonly global::MaiksMixer.JackBridge _jackBridge;
        private bool _isInitialized;
        private bool _isDisposed;

        /// <summary>
        /// Event raised when the JACK server status changes
        /// </summary>
        public event EventHandler<bool>? ServerStatusChanged;

        /// <summary>
        /// Event raised when meter data is updated for a channel
        /// </summary>
        public event EventHandler<MeterUpdateEventArgs>? MeterUpdated;

        /// <summary>
        /// Creates a new instance of the JackAudioService
        /// </summary>
        public JackAudioService()
        {
            _jackBridge = new global::MaiksMixer.JackBridge();
            _jackBridge.ServerStatusChanged += OnServerStatusChanged;
            _jackBridge.MeterUpdated += OnMeterUpdated;
        }

        /// <summary>
        /// Initializes the JACK client with the specified name
        /// </summary>
        /// <param name="clientName">Name to register with the JACK server</param>
        /// <returns>True if initialization was successful, false otherwise</returns>
        public bool Initialize(string clientName)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioService));
            if (_isInitialized) return true;

            _isInitialized = _jackBridge.Initialize(clientName);
            return _isInitialized;
        }

        /// <summary>
        /// Creates input and output ports for the JACK client
        /// </summary>
        /// <param name="numInputs">Number of input ports to create</param>
        /// <param name="numOutputs">Number of output ports to create</param>
        /// <returns>True if port creation was successful, false otherwise</returns>
        public bool CreatePorts(int numInputs, int numOutputs)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioService));
            if (!_isInitialized) throw new InvalidOperationException("JACK client is not initialized");

            return _jackBridge.CreatePorts(numInputs, numOutputs);
        }

        /// <summary>
        /// Activates the JACK client
        /// </summary>
        /// <returns>True if activation was successful, false otherwise</returns>
        public bool Activate()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioService));
            if (!_isInitialized) throw new InvalidOperationException("JACK client is not initialized");

            return _jackBridge.Activate();
        }

        /// <summary>
        /// Deactivates the JACK client
        /// </summary>
        /// <returns>True if deactivation was successful, false otherwise</returns>
        public bool Deactivate()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioService));
            if (!_isInitialized) throw new InvalidOperationException("JACK client is not initialized");

            return _jackBridge.Deactivate();
        }

        /// <summary>
        /// Sets the volume for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="volume">Volume value (0.0 - 1.0)</param>
        public void SetChannelVolume(int channel, float volume)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioService));
            if (!_isInitialized) throw new InvalidOperationException("JACK client is not initialized");

            _jackBridge.SetChannelVolume(channel, volume);
        }

        /// <summary>
        /// Sets the pan for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="pan">Pan value (0.0 left, 0.5 center, 1.0 right)</param>
        public void SetChannelPan(int channel, float pan)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioService));
            if (!_isInitialized) throw new InvalidOperationException("JACK client is not initialized");

            _jackBridge.SetChannelPan(channel, pan);
        }

        /// <summary>
        /// Sets the gain for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="gainDB">Gain value in dB</param>
        public void SetChannelGain(int channel, float gainDB)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioService));
            if (!_isInitialized) throw new InvalidOperationException("JACK client is not initialized");

            _jackBridge.SetChannelGain(channel, gainDB);
        }

        /// <summary>
        /// Sets the mute state for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="mute">Mute state</param>
        public void SetChannelMute(int channel, bool mute)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioService));
            if (!_isInitialized) throw new InvalidOperationException("JACK client is not initialized");

            _jackBridge.SetChannelMute(channel, mute);
        }

        /// <summary>
        /// Sets the solo state for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="solo">Solo state</param>
        public void SetChannelSolo(int channel, bool solo)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioService));
            if (!_isInitialized) throw new InvalidOperationException("JACK client is not initialized");

            _jackBridge.SetChannelSolo(channel, solo);
        }

        /// <summary>
        /// Gets the meter data for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <returns>MeterData object with peak and RMS values</returns>
        public MeterData GetChannelMeter(int channel)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioService));
            if (!_isInitialized) throw new InvalidOperationException("JACK client is not initialized");

            var nativeMeterData = _jackBridge.GetChannelMeter(channel);
            return new MeterData
            {
                Peak = nativeMeterData.Peak,
                Rms = nativeMeterData.Rms
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
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioService));
            if (!_isInitialized) throw new InvalidOperationException("JACK client is not initialized");

            return _jackBridge.ConnectPorts(sourcePort, destPort);
        }

        /// <summary>
        /// Disconnects two JACK ports
        /// </summary>
        /// <param name="sourcePort">Source port name</param>
        /// <param name="destPort">Destination port name</param>
        /// <returns>True if disconnection was successful, false otherwise</returns>
        public bool DisconnectPorts(string sourcePort, string destPort)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioService));
            if (!_isInitialized) throw new InvalidOperationException("JACK client is not initialized");

            return _jackBridge.DisconnectPorts(sourcePort, destPort);
        }

        /// <summary>
        /// Gets a list of all available JACK ports
        /// </summary>
        /// <param name="portType">Port type filter (e.g., "audio")</param>
        /// <param name="flags">Port flags filter</param>
        /// <returns>List of port names</returns>
        public List<string> GetPortList(string portType, uint flags)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioService));
            if (!_isInitialized) throw new InvalidOperationException("JACK client is not initialized");

            var nativePortList = _jackBridge.GetPortList(portType, flags);
            var portList = new List<string>();
            
            foreach (var port in nativePortList)
            {
                portList.Add(port);
            }
            
            return portList;
        }

        /// <summary>
        /// Gets a list of all available JACK ports
        /// </summary>
        /// <returns>List of JackPort objects</returns>
        public List<JackPort> GetPorts()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioService));
            if (!_isInitialized) throw new InvalidOperationException("JACK client is not initialized");

            var allPorts = _jackBridge.GetPortList("", 0);
            var result = new List<JackPort>();

            foreach (var portName in allPorts)
            {
                var port = new JackPort
                {
                    Name = portName,
                    Type = "audio", // This would need to be determined from the actual port
                    IsInput = portName.Contains("input"),
                    IsOutput = portName.Contains("output"),
                    IsPhysical = portName.Contains("system"),
                    Connections = new List<string>()
                };
                
                result.Add(port);
            }

            return result;
        }

        /// <summary>
        /// Gets the sample rate from the JACK server
        /// </summary>
        /// <returns>Sample rate in Hz</returns>
        public int GetSampleRate()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioService));
            if (!_isInitialized) throw new InvalidOperationException("JACK client is not initialized");

            return _jackBridge.GetSampleRate();
        }

        /// <summary>
        /// Gets the buffer size from the JACK server
        /// </summary>
        /// <returns>Buffer size in frames</returns>
        public int GetBufferSize()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioService));
            if (!_isInitialized) throw new InvalidOperationException("JACK client is not initialized");

            return _jackBridge.GetBufferSize();
        }

        /// <summary>
        /// Gets the CPU load from the JACK server
        /// </summary>
        /// <returns>CPU load as a percentage (0.0 - 100.0)</returns>
        public float GetCpuLoad()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioService));
            if (!_isInitialized) throw new InvalidOperationException("JACK client is not initialized");

            return _jackBridge.GetCpuLoad();
        }

        /// <summary>
        /// Checks if the JACK server is running
        /// </summary>
        /// <returns>True if running, false otherwise</returns>
        public bool IsServerRunning()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioService));
            if (!_isInitialized) return false;

            return _jackBridge.IsServerRunning();
        }

        /// <summary>
        /// Gets the current JACK server status
        /// </summary>
        /// <returns>JackServerStatus object with server information</returns>
        public JackServerStatus GetServerStatus()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JackAudioService));
            if (!_isInitialized) throw new InvalidOperationException("JACK client is not initialized");

            var nativeStatus = _jackBridge.GetServerStatus();
            return new JackServerStatus
            {
                IsRunning = nativeStatus.IsRunning,
                SampleRate = nativeStatus.SampleRate,
                BufferSize = nativeStatus.BufferSize,
                CpuLoad = nativeStatus.CpuLoad
            };
        }

        /// <summary>
        /// Handles the server status changed event from the native bridge
        /// </summary>
        private void OnServerStatusChanged(bool isRunning)
        {
            ServerStatusChanged?.Invoke(this, isRunning);
        }

        /// <summary>
        /// Handles the meter update event from the native bridge
        /// </summary>
        private void OnMeterUpdated(int channel, global::MaiksMixer.MeterData data)
        {
            MeterUpdated?.Invoke(this, new MeterUpdateEventArgs(channel, new MeterData
            {
                Peak = data.Peak,
                Rms = data.Rms
            }));
        }

        /// <summary>
        /// Disposes the JackAudioService
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            if (_isInitialized)
            {
                try
                {
                    _jackBridge.Deactivate();
                }
                catch
                {
                    // Ignore exceptions during cleanup
                }
            }

            _jackBridge.ServerStatusChanged -= OnServerStatusChanged;
            _jackBridge.MeterUpdated -= OnMeterUpdated;

            // The finalizer in the C++/CLI bridge will handle the native resources
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Represents meter data for a channel
    /// </summary>
    public class MeterData
    {
        /// <summary>
        /// Peak level (0.0 - 1.0)
        /// </summary>
        public float Peak { get; set; }

        /// <summary>
        /// RMS level (0.0 - 1.0)
        /// </summary>
        public float Rms { get; set; }
    }

    /// <summary>
    /// Represents JACK port information
    /// </summary>
    public class JackPort
    {
        /// <summary>
        /// Port name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Port type (e.g., "audio")
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Whether the port is an input port
        /// </summary>
        public bool IsInput { get; set; }

        /// <summary>
        /// Whether the port is an output port
        /// </summary>
        public bool IsOutput { get; set; }

        /// <summary>
        /// Whether the port is a physical port
        /// </summary>
        public bool IsPhysical { get; set; }

        /// <summary>
        /// List of connections to this port
        /// </summary>
        public List<string> Connections { get; set; } = new List<string>();
    }

    /// <summary>
    /// Represents JACK server status
    /// </summary>
    public class JackServerStatus
    {
        /// <summary>
        /// Whether the JACK server is running
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// Sample rate in Hz
        /// </summary>
        public int SampleRate { get; set; }

        /// <summary>
        /// Buffer size in frames
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// CPU load as a percentage (0.0 - 100.0)
        /// </summary>
        public float CpuLoad { get; set; }
    }

    /// <summary>
    /// Event arguments for meter updates
    /// </summary>
    public class MeterUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// Channel index
        /// </summary>
        public int Channel { get; }

        /// <summary>
        /// Meter data
        /// </summary>
        public MeterData MeterData { get; }

        /// <summary>
        /// Creates a new instance of MeterUpdateEventArgs
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="meterData">Meter data</param>
        public MeterUpdateEventArgs(int channel, MeterData meterData)
        {
            Channel = channel;
            MeterData = meterData;
        }
    }
}
