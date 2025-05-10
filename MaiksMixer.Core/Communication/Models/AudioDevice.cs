using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MaiksMixer.Core.Communication.Models
{
    /// <summary>
    /// Represents an audio device in the system.
    /// </summary>
    public class AudioDevice
    {
        /// <summary>
        /// Gets or sets the unique identifier for the device.
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the name of the device.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the type of the device.
        /// </summary>
        public AudioDeviceType DeviceType { get; set; }
        
        /// <summary>
        /// Gets or sets whether the device is an input device.
        /// </summary>
        public bool IsInput { get; set; }
        
        /// <summary>
        /// Gets or sets whether the device is an output device.
        /// </summary>
        public bool IsOutput { get; set; }
        
        /// <summary>
        /// Gets or sets whether the device is a virtual device.
        /// </summary>
        public bool IsVirtual { get; set; }
        
        /// <summary>
        /// Gets or sets whether the device is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the sample rate of the device.
        /// </summary>
        public int SampleRate { get; set; }
        
        /// <summary>
        /// Gets or sets the buffer size of the device.
        /// </summary>
        public int BufferSize { get; set; }
        
        /// <summary>
        /// Gets or sets the number of input channels.
        /// </summary>
        public int InputChannels { get; set; }
        
        /// <summary>
        /// Gets or sets the number of output channels.
        /// </summary>
        public int OutputChannels { get; set; }
        
        /// <summary>
        /// Gets or sets the device status.
        /// </summary>
        public AudioDeviceStatus Status { get; set; } = AudioDeviceStatus.Offline;
        
        /// <summary>
        /// Gets or sets the list of input ports for the device.
        /// </summary>
        public List<AudioPort> InputPorts { get; set; } = new List<AudioPort>();
        
        /// <summary>
        /// Gets or sets the list of output ports for the device.
        /// </summary>
        public List<AudioPort> OutputPorts { get; set; } = new List<AudioPort>();
        
        /// <summary>
        /// Gets or sets the driver type for the device.
        /// </summary>
        public string DriverType { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets additional properties for the device.
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Gets or sets the last error message for the device.
        /// </summary>
        public string LastError { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the timestamp of the last update.
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
    
    /// <summary>
    /// Represents the type of an audio device.
    /// </summary>
    public enum AudioDeviceType
    {
        /// <summary>
        /// Unknown device type.
        /// </summary>
        Unknown,
        
        /// <summary>
        /// Physical audio interface.
        /// </summary>
        PhysicalInterface,
        
        /// <summary>
        /// Virtual audio device.
        /// </summary>
        VirtualDevice,
        
        /// <summary>
        /// JACK audio client.
        /// </summary>
        JackClient,
        
        /// <summary>
        /// ASIO device.
        /// </summary>
        AsioDevice,
        
        /// <summary>
        /// WASAPI device.
        /// </summary>
        WasapiDevice,
        
        /// <summary>
        /// DirectSound device.
        /// </summary>
        DirectSoundDevice
    }
    
    /// <summary>
    /// Represents the status of an audio device.
    /// </summary>
    public enum AudioDeviceStatus
    {
        /// <summary>
        /// Device is online and working properly.
        /// </summary>
        Online,
        
        /// <summary>
        /// Device is offline or not connected.
        /// </summary>
        Offline,
        
        /// <summary>
        /// Device is in an error state.
        /// </summary>
        Error,
        
        /// <summary>
        /// Device is busy or in use by another application.
        /// </summary>
        Busy,
        
        /// <summary>
        /// Device is initializing.
        /// </summary>
        Initializing
    }
    
    /// <summary>
    /// Represents an audio port on a device.
    /// </summary>
    public class AudioPort
    {
        /// <summary>
        /// Gets or sets the unique identifier for the port.
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the name of the port.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets whether the port is an input port.
        /// </summary>
        public bool IsInput { get; set; }
        
        /// <summary>
        /// Gets or sets the channel number.
        /// </summary>
        public int Channel { get; set; }
        
        /// <summary>
        /// Gets or sets whether the port is connected.
        /// </summary>
        public bool IsConnected { get; set; }
        
        /// <summary>
        /// Gets or sets the list of connections to other ports.
        /// </summary>
        public List<string> Connections { get; set; } = new List<string>();
    }
}
