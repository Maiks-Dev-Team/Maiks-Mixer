using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MaiksMixer.Core.Models
{
    /// <summary>
    /// Represents the type of audio port.
    /// </summary>
    public enum AudioPortType
    {
        /// <summary>
        /// Audio input port.
        /// </summary>
        Input,
        
        /// <summary>
        /// Audio output port.
        /// </summary>
        Output,
        
        /// <summary>
        /// MIDI input port.
        /// </summary>
        MidiInput,
        
        /// <summary>
        /// MIDI output port.
        /// </summary>
        MidiOutput,
        
        /// <summary>
        /// Control port.
        /// </summary>
        Control,
        
        /// <summary>
        /// Unknown port type.
        /// </summary>
        Unknown
    }
    
    /// <summary>
    /// Represents the status of an audio port.
    /// </summary>
    public enum AudioPortStatus
    {
        /// <summary>
        /// Port is active and working properly.
        /// </summary>
        Active,
        
        /// <summary>
        /// Port is inactive but available.
        /// </summary>
        Inactive,
        
        /// <summary>
        /// Port is connected to another port.
        /// </summary>
        Connected,
        
        /// <summary>
        /// Port has an error.
        /// </summary>
        Error,
        
        /// <summary>
        /// Port status is unknown.
        /// </summary>
        Unknown
    }
    
    /// <summary>
    /// Represents detailed information about an audio port.
    /// </summary>
    public class AudioPortInfo
    {
        /// <summary>
        /// Gets or sets the unique identifier for the port.
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the port.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the port.
        /// </summary>
        public AudioPortType PortType { get; set; }
        
        /// <summary>
        /// Gets or sets the status of the port.
        /// </summary>
        public AudioPortStatus Status { get; set; }
        
        /// <summary>
        /// Gets or sets the device ID that this port belongs to.
        /// </summary>
        public string DeviceId { get; set; }
        
        /// <summary>
        /// Gets or sets the channel number.
        /// </summary>
        public int Channel { get; set; }
        
        /// <summary>
        /// Gets or sets the port description.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the port is a physical port.
        /// </summary>
        public bool IsPhysical { get; set; }
        
        /// <summary>
        /// Gets or sets the port properties.
        /// </summary>
        public Dictionary<string, string> Properties { get; set; }
        
        /// <summary>
        /// Gets or sets the connections to other ports.
        /// </summary>
        public List<string> Connections { get; set; }
        
        /// <summary>
        /// Gets or sets the last time the port was updated.
        /// </summary>
        [JsonIgnore]
        public DateTime LastUpdated { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the AudioPortInfo class.
        /// </summary>
        public AudioPortInfo()
        {
            Id = Guid.NewGuid().ToString();
            Name = "New Port";
            PortType = AudioPortType.Unknown;
            Status = AudioPortStatus.Unknown;
            DeviceId = string.Empty;
            Channel = 0;
            Description = string.Empty;
            IsPhysical = true;
            Properties = new Dictionary<string, string>();
            Connections = new List<string>();
            LastUpdated = DateTime.Now;
        }
        
        /// <summary>
        /// Creates a deep copy of this audio port info.
        /// </summary>
        /// <returns>A new AudioPortInfo instance with the same values.</returns>
        public AudioPortInfo Clone()
        {
            var clone = new AudioPortInfo
            {
                Id = this.Id,
                Name = this.Name,
                PortType = this.PortType,
                Status = this.Status,
                DeviceId = this.DeviceId,
                Channel = this.Channel,
                Description = this.Description,
                IsPhysical = this.IsPhysical,
                LastUpdated = this.LastUpdated
            };
            
            // Clone lists and dictionaries
            clone.Properties = new Dictionary<string, string>(this.Properties);
            clone.Connections = new List<string>(this.Connections);
            
            return clone;
        }
    }
}
