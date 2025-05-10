using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MaiksMixer.Core.Models
{
    /// <summary>
    /// Represents the type of audio device.
    /// </summary>
    public enum AudioDeviceType
    {
        /// <summary>
        /// Physical audio device.
        /// </summary>
        Physical,
        
        /// <summary>
        /// Virtual audio device.
        /// </summary>
        Virtual,
        
        /// <summary>
        /// JACK system device.
        /// </summary>
        Jack,
        
        /// <summary>
        /// ASIO device.
        /// </summary>
        Asio,
        
        /// <summary>
        /// WDM device.
        /// </summary>
        Wdm,
        
        /// <summary>
        /// WASAPI device.
        /// </summary>
        Wasapi,
        
        /// <summary>
        /// Unknown device type.
        /// </summary>
        Unknown
    }
    
    /// <summary>
    /// Represents the status of an audio device.
    /// </summary>
    public enum AudioDeviceStatus
    {
        /// <summary>
        /// Device is active and working properly.
        /// </summary>
        Active,
        
        /// <summary>
        /// Device is inactive but available.
        /// </summary>
        Inactive,
        
        /// <summary>
        /// Device is disconnected.
        /// </summary>
        Disconnected,
        
        /// <summary>
        /// Device has an error.
        /// </summary>
        Error,
        
        /// <summary>
        /// Device status is unknown.
        /// </summary>
        Unknown
    }
    
    /// <summary>
    /// Represents detailed information about an audio device.
    /// </summary>
    public class AudioDeviceInfo
    {
        /// <summary>
        /// Gets or sets the unique identifier for the device.
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the device.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the manufacturer of the device.
        /// </summary>
        public string Manufacturer { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the device.
        /// </summary>
        public AudioDeviceType DeviceType { get; set; }
        
        /// <summary>
        /// Gets or sets the status of the device.
        /// </summary>
        public AudioDeviceStatus Status { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the device is an input device.
        /// </summary>
        public bool IsInput { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the device is an output device.
        /// </summary>
        public bool IsOutput { get; set; }
        
        /// <summary>
        /// Gets or sets the number of input channels.
        /// </summary>
        public int InputChannels { get; set; }
        
        /// <summary>
        /// Gets or sets the number of output channels.
        /// </summary>
        public int OutputChannels { get; set; }
        
        /// <summary>
        /// Gets or sets the supported sample rates.
        /// </summary>
        public List<int> SupportedSampleRates { get; set; }
        
        /// <summary>
        /// Gets or sets the supported buffer sizes.
        /// </summary>
        public List<int> SupportedBufferSizes { get; set; }
        
        /// <summary>
        /// Gets or sets the current sample rate.
        /// </summary>
        public int CurrentSampleRate { get; set; }
        
        /// <summary>
        /// Gets or sets the current buffer size.
        /// </summary>
        public int CurrentBufferSize { get; set; }
        
        /// <summary>
        /// Gets or sets the bit depth.
        /// </summary>
        public int BitDepth { get; set; }
        
        /// <summary>
        /// Gets or sets the driver version.
        /// </summary>
        public string DriverVersion { get; set; }
        
        /// <summary>
        /// Gets or sets the device description.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the device is the default device.
        /// </summary>
        public bool IsDefault { get; set; }
        
        /// <summary>
        /// Gets or sets the device properties.
        /// </summary>
        public Dictionary<string, string> Properties { get; set; }
        
        /// <summary>
        /// Gets or sets the input ports.
        /// </summary>
        public List<AudioPortInfo> InputPorts { get; set; }
        
        /// <summary>
        /// Gets or sets the output ports.
        /// </summary>
        public List<AudioPortInfo> OutputPorts { get; set; }
        
        /// <summary>
        /// Gets or sets the last time the device was updated.
        /// </summary>
        [JsonIgnore]
        public DateTime LastUpdated { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the AudioDeviceInfo class.
        /// </summary>
        public AudioDeviceInfo()
        {
            Id = Guid.NewGuid().ToString();
            Name = "New Device";
            Manufacturer = "Unknown";
            DeviceType = AudioDeviceType.Unknown;
            Status = AudioDeviceStatus.Unknown;
            IsInput = false;
            IsOutput = false;
            InputChannels = 0;
            OutputChannels = 0;
            SupportedSampleRates = new List<int>();
            SupportedBufferSizes = new List<int>();
            CurrentSampleRate = 44100;
            CurrentBufferSize = 512;
            BitDepth = 24;
            DriverVersion = "Unknown";
            Description = "";
            IsDefault = false;
            Properties = new Dictionary<string, string>();
            InputPorts = new List<AudioPortInfo>();
            OutputPorts = new List<AudioPortInfo>();
            LastUpdated = DateTime.Now;
        }
        
        /// <summary>
        /// Creates a deep copy of this audio device info.
        /// </summary>
        /// <returns>A new AudioDeviceInfo instance with the same values.</returns>
        public AudioDeviceInfo Clone()
        {
            var clone = new AudioDeviceInfo
            {
                Id = this.Id,
                Name = this.Name,
                Manufacturer = this.Manufacturer,
                DeviceType = this.DeviceType,
                Status = this.Status,
                IsInput = this.IsInput,
                IsOutput = this.IsOutput,
                InputChannels = this.InputChannels,
                OutputChannels = this.OutputChannels,
                CurrentSampleRate = this.CurrentSampleRate,
                CurrentBufferSize = this.CurrentBufferSize,
                BitDepth = this.BitDepth,
                DriverVersion = this.DriverVersion,
                Description = this.Description,
                IsDefault = this.IsDefault,
                LastUpdated = this.LastUpdated
            };
            
            // Clone lists and dictionaries
            clone.SupportedSampleRates = new List<int>(this.SupportedSampleRates);
            clone.SupportedBufferSizes = new List<int>(this.SupportedBufferSizes);
            clone.Properties = new Dictionary<string, string>(this.Properties);
            
            // Clone ports
            clone.InputPorts = new List<AudioPortInfo>();
            foreach (var port in this.InputPorts)
            {
                clone.InputPorts.Add(port.Clone());
            }
            
            clone.OutputPorts = new List<AudioPortInfo>();
            foreach (var port in this.OutputPorts)
            {
                clone.OutputPorts.Add(port.Clone());
            }
            
            return clone;
        }
    }
}
