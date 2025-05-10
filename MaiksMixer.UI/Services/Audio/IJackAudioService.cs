using System;
using System.Collections.Generic;

namespace MaiksMixer.UI.Services.Audio
{
    /// <summary>
    /// Event arguments for meter updates
    /// </summary>
    public class MeterUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the channel index
        /// </summary>
        public int Channel { get; }

        /// <summary>
        /// Gets the meter data
        /// </summary>
        public MeterData MeterData { get; }

        /// <summary>
        /// Initializes a new instance of the MeterUpdateEventArgs class
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="meterData">Meter data</param>
        public MeterUpdateEventArgs(int channel, MeterData meterData)
        {
            Channel = channel;
            MeterData = meterData;
        }
    }

    /// <summary>
    /// Represents meter data for an audio channel
    /// </summary>
    public class MeterData
    {
        /// <summary>
        /// Gets or sets the peak level
        /// </summary>
        public float Peak { get; set; }

        /// <summary>
        /// Gets or sets the RMS level
        /// </summary>
        public float RMS { get; set; }
    }

    /// <summary>
    /// Represents a JACK port
    /// </summary>
    public class JackPort
    {
        /// <summary>
        /// Gets or sets the port name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the port type
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the port flags
        /// </summary>
        public uint Flags { get; set; }

        /// <summary>
        /// Gets or sets the connections for this port
        /// </summary>
        public List<string> Connections { get; set; } = new List<string>();
        
        /// <summary>
        /// Initializes a new instance of the JackPort class
        /// </summary>
        public JackPort()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the JackPort class with specified values
        /// </summary>
        public JackPort(string name, string type, uint flags)
        {
            Name = name;
            Type = type;
            Flags = flags;
        }
    }

    /// <summary>
    /// Represents the status of the JACK server
    /// </summary>
    public class JackServerStatus
    {
        /// <summary>
        /// Gets or sets whether the server is running
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// Gets or sets the sample rate
        /// </summary>
        public int SampleRate { get; set; }

        /// <summary>
        /// Gets or sets the buffer size
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// Gets or sets the CPU load
        /// </summary>
        public float CpuLoad { get; set; }
    }

    /// <summary>
    /// Interface for JACK audio service
    /// </summary>
    public interface IJackAudioService
    {
        /// <summary>
        /// Event raised when the JACK server status changes
        /// </summary>
        event EventHandler<bool> ServerStatusChanged;

        /// <summary>
        /// Event raised when meter data is updated for a channel
        /// </summary>
        event EventHandler<MeterUpdateEventArgs> MeterUpdated;

        /// <summary>
        /// Initializes the JACK client with the specified name
        /// </summary>
        /// <param name="clientName">Name to register with the JACK server</param>
        /// <returns>True if initialization was successful, false otherwise</returns>
        bool Initialize(string clientName);

        /// <summary>
        /// Creates input and output ports for the JACK client
        /// </summary>
        /// <param name="numInputs">Number of input ports to create</param>
        /// <param name="numOutputs">Number of output ports to create</param>
        /// <returns>True if port creation was successful, false otherwise</returns>
        bool CreatePorts(int numInputs, int numOutputs);

        /// <summary>
        /// Activates the JACK client
        /// </summary>
        /// <returns>True if activation was successful, false otherwise</returns>
        bool Activate();

        /// <summary>
        /// Deactivates the JACK client
        /// </summary>
        /// <returns>True if deactivation was successful, false otherwise</returns>
        bool Deactivate();

        /// <summary>
        /// Sets the volume for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="volume">Volume value (0.0 - 1.0)</param>
        void SetChannelVolume(int channel, float volume);

        /// <summary>
        /// Sets the pan for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="pan">Pan value (0.0 left, 0.5 center, 1.0 right)</param>
        void SetChannelPan(int channel, float pan);

        /// <summary>
        /// Sets the gain for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="gainDB">Gain value in dB</param>
        void SetChannelGain(int channel, float gainDB);

        /// <summary>
        /// Sets the mute state for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="mute">Mute state</param>
        void SetChannelMute(int channel, bool mute);

        /// <summary>
        /// Sets the solo state for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="solo">Solo state</param>
        void SetChannelSolo(int channel, bool solo);

        /// <summary>
        /// Gets the meter data for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <returns>MeterData object with peak and RMS values</returns>
        MeterData GetChannelMeter(int channel);

        /// <summary>
        /// Connects two JACK ports
        /// </summary>
        /// <param name="sourcePort">Source port name</param>
        /// <param name="destPort">Destination port name</param>
        /// <returns>True if connection was successful, false otherwise</returns>
        bool ConnectPorts(string sourcePort, string destPort);

        /// <summary>
        /// Disconnects two JACK ports
        /// </summary>
        /// <param name="sourcePort">Source port name</param>
        /// <param name="destPort">Destination port name</param>
        /// <returns>True if disconnection was successful, false otherwise</returns>
        bool DisconnectPorts(string sourcePort, string destPort);

        /// <summary>
        /// Gets a list of all available JACK ports
        /// </summary>
        /// <param name="portType">Port type filter (e.g., "audio")</param>
        /// <param name="flags">Port flags filter</param>
        /// <returns>List of port names</returns>
        List<string> GetPortList(string portType, uint flags);

        /// <summary>
        /// Gets a list of all available JACK ports
        /// </summary>
        /// <returns>List of JackPort objects</returns>
        List<JackPort> GetPorts();

        /// <summary>
        /// Gets the sample rate from the JACK server
        /// </summary>
        /// <returns>Sample rate in Hz</returns>
        int GetSampleRate();

        /// <summary>
        /// Gets the buffer size from the JACK server
        /// </summary>
        /// <returns>Buffer size in frames</returns>
        int GetBufferSize();

        /// <summary>
        /// Gets the CPU load from the JACK server
        /// </summary>
        /// <returns>CPU load as a percentage (0.0 - 100.0)</returns>
        float GetCpuLoad();

        /// <summary>
        /// Checks if the JACK server is running
        /// </summary>
        /// <returns>True if running, false otherwise</returns>
        bool IsServerRunning();

        /// <summary>
        /// Gets the current JACK server status
        /// </summary>
        /// <returns>JackServerStatus object with server information</returns>
        JackServerStatus GetServerStatus();
    }
}
