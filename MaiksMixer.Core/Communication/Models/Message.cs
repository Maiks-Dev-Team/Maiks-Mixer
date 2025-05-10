using System;
using System.Text.Json.Serialization;

namespace MaiksMixer.Core.Communication.Models
{
    /// <summary>
    /// Base class for all messages exchanged between components.
    /// </summary>
    public abstract class Message
    {
        /// <summary>
        /// Gets or sets the message type.
        /// </summary>
        public string MessageType { get; set; }
        
        /// <summary>
        /// Gets or sets the timestamp of the message.
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Gets or sets the message ID.
        /// </summary>
        public string MessageId { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the Message class.
        /// </summary>
        protected Message()
        {
            MessageType = GetType().Name;
            Timestamp = DateTime.Now;
            MessageId = Guid.NewGuid().ToString();
        }
    }
    
    /// <summary>
    /// Represents a command message sent from a client to the server.
    /// </summary>
    public class CommandMessage : Message
    {
        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        public string Command { get; set; }
        
        /// <summary>
        /// Gets or sets the parameters for the command.
        /// </summary>
        public object? Parameters { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the CommandMessage class.
        /// </summary>
        public CommandMessage() : base()
        {
            Command = string.Empty;
        }
        
        /// <summary>
        /// Initializes a new instance of the CommandMessage class with the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public CommandMessage(string command) : base()
        {
            Command = command;
        }
        
        /// <summary>
        /// Initializes a new instance of the CommandMessage class with the specified command and parameters.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters for the command.</param>
        public CommandMessage(string command, object parameters) : base()
        {
            Command = command;
            Parameters = parameters;
        }
    }
    
    /// <summary>
    /// Represents a response message sent from the server to clients.
    /// </summary>
    public class ResponseMessage : Message
    {
        /// <summary>
        /// Gets or sets the ID of the message this is in response to.
        /// </summary>
        public string InResponseTo { get; set; }
        
        /// <summary>
        /// Gets or sets whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Gets or sets the error code if the operation failed.
        /// </summary>
        public int? ErrorCode { get; set; }
        
        /// <summary>
        /// Gets or sets the error message if the operation failed.
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// Gets or sets the response data if the operation succeeded.
        /// </summary>
        public object? Data { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the ResponseMessage class.
        /// </summary>
        public ResponseMessage() : base()
        {
            InResponseTo = string.Empty;
            Success = true;
        }
        
        /// <summary>
        /// Initializes a new instance of the ResponseMessage class with the specified in-response-to ID.
        /// </summary>
        /// <param name="inResponseTo">The ID of the message this is in response to.</param>
        public ResponseMessage(string inResponseTo) : base()
        {
            InResponseTo = inResponseTo;
            Success = true;
        }
        
        /// <summary>
        /// Initializes a new instance of the ResponseMessage class with the specified in-response-to ID and success flag.
        /// </summary>
        /// <param name="inResponseTo">The ID of the message this is in response to.</param>
        /// <param name="success">Whether the operation was successful.</param>
        public ResponseMessage(string inResponseTo, bool success) : base()
        {
            InResponseTo = inResponseTo;
            Success = success;
        }
        
        /// <summary>
        /// Initializes a new instance of the ResponseMessage class with the specified in-response-to ID, success flag, and data.
        /// </summary>
        /// <param name="inResponseTo">The ID of the message this is in response to.</param>
        /// <param name="success">Whether the operation was successful.</param>
        /// <param name="data">The response data if the operation succeeded.</param>
        public ResponseMessage(string inResponseTo, bool success, object data) : base()
        {
            InResponseTo = inResponseTo;
            Success = success;
            Data = data;
        }
        
        /// <summary>
        /// Initializes a new instance of the ResponseMessage class with the specified in-response-to ID, error code, and error message.
        /// </summary>
        /// <param name="inResponseTo">The ID of the message this is in response to.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorMessage">The error message.</param>
        public ResponseMessage(string inResponseTo, int errorCode, string errorMessage) : base()
        {
            InResponseTo = inResponseTo;
            Success = false;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
    }
    
    /// <summary>
    /// Represents an event message sent from the server to clients.
    /// </summary>
    public class EventMessage : Message
    {
        /// <summary>
        /// Gets or sets the event name.
        /// </summary>
        public string EventName { get; set; }
        
        /// <summary>
        /// Gets or sets the event data.
        /// </summary>
        public object? EventData { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the EventMessage class.
        /// </summary>
        public EventMessage() : base()
        {
            EventName = string.Empty;
        }
        
        /// <summary>
        /// Initializes a new instance of the EventMessage class with the specified event name.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        public EventMessage(string eventName) : base()
        {
            EventName = eventName;
        }
        
        /// <summary>
        /// Initializes a new instance of the EventMessage class with the specified event name and data.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="eventData">The event data.</param>
        public EventMessage(string eventName, object eventData) : base()
        {
            EventName = eventName;
            EventData = eventData;
        }
    }
    
    /// <summary>
    /// Represents a heartbeat message for connection health checking.
    /// </summary>
    public class HeartbeatMessage : Message
    {
        /// <summary>
        /// Gets or sets the client ID.
        /// </summary>
        public string ClientId { get; set; }
        
        /// <summary>
        /// Gets or sets the sequence number.
        /// </summary>
        public int SequenceNumber { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the HeartbeatMessage class.
        /// </summary>
        public HeartbeatMessage() : base()
        {
            ClientId = string.Empty;
        }
        
        /// <summary>
        /// Initializes a new instance of the HeartbeatMessage class with the specified client ID.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        public HeartbeatMessage(string clientId) : base()
        {
            ClientId = clientId;
        }
        
        /// <summary>
        /// Initializes a new instance of the HeartbeatMessage class with the specified client ID and sequence number.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="sequenceNumber">The sequence number.</param>
        public HeartbeatMessage(string clientId, int sequenceNumber) : base()
        {
            ClientId = clientId;
            SequenceNumber = sequenceNumber;
        }
    }
    
    /// <summary>
    /// Specific message payloads for audio routing commands
    /// </summary>
    public class AudioRoutingPayloads
    {
        /// <summary>
        /// Payload for setting an audio route
        /// </summary>
        public class SetRoutePayload
        {
            /// <summary>
            /// Gets or sets the source device ID.
            /// </summary>
            public string SourceId { get; set; } = string.Empty;
            
            /// <summary>
            /// Gets or sets the destination device ID.
            /// </summary>
            public string DestinationId { get; set; } = string.Empty;
            
            /// <summary>
            /// Gets or sets whether the route is enabled.
            /// </summary>
            public bool Enabled { get; set; } = true;
            
            /// <summary>
            /// Gets or sets the volume level (0.0 to 1.0).
            /// </summary>
            public float Volume { get; set; } = 1.0f;
        }
        
        /// <summary>
        /// Payload for the routing matrix response
        /// </summary>
        public class RoutingMatrixPayload
        {
            /// <summary>
            /// Gets or sets the list of audio routes.
            /// </summary>
            public AudioRoute[] Routes { get; set; } = Array.Empty<AudioRoute>();
            
            /// <summary>
            /// Represents an audio route between two devices.
            /// </summary>
            public class AudioRoute
            {
                /// <summary>
                /// Gets or sets the source device ID.
                /// </summary>
                public string SourceId { get; set; } = string.Empty;
                
                /// <summary>
                /// Gets or sets the destination device ID.
                /// </summary>
                public string DestinationId { get; set; } = string.Empty;
                
                /// <summary>
                /// Gets or sets whether the route is enabled.
                /// </summary>
                public bool Enabled { get; set; }
                
                /// <summary>
                /// Gets or sets the volume level (0.0 to 1.0).
                /// </summary>
                public float Volume { get; set; }
            }
        }
    }
    
    /// <summary>
    /// Specific message payloads for device management
    /// </summary>
    public class DeviceManagementPayloads
    {
        /// <summary>
        /// Payload for creating a virtual device
        /// </summary>
        public class CreateVirtualDevicePayload
        {
            /// <summary>
            /// Gets or sets the device type (input or output).
            /// </summary>
            public string DeviceType { get; set; } = string.Empty;
            
            /// <summary>
            /// Gets or sets the device name.
            /// </summary>
            public string DeviceName { get; set; } = string.Empty;
            
            /// <summary>
            /// Gets or sets the number of channels.
            /// </summary>
            public int ChannelCount { get; set; } = 2;
            
            /// <summary>
            /// Gets or sets the sample rate.
            /// </summary>
            public int SampleRate { get; set; } = 48000;
            
            /// <summary>
            /// Gets or sets the bit depth.
            /// </summary>
            public int BitDepth { get; set; } = 24;
        }
        
        /// <summary>
        /// Payload for removing a virtual device
        /// </summary>
        public class RemoveVirtualDevicePayload
        {
            /// <summary>
            /// Gets or sets the device ID.
            /// </summary>
            public string DeviceId { get; set; } = string.Empty;
        }
        
        /// <summary>
        /// Payload for the device list response
        /// </summary>
        public class DeviceListPayload
        {
            /// <summary>
            /// Gets or sets the list of audio devices.
            /// </summary>
            public AudioDevice[] Devices { get; set; } = Array.Empty<AudioDevice>();
            
            /// <summary>
            /// Represents an audio device.
            /// </summary>
            public class AudioDevice
            {
                /// <summary>
                /// Gets or sets the device ID.
                /// </summary>
                public string DeviceId { get; set; } = string.Empty;
                
                /// <summary>
                /// Gets or sets the device name.
                /// </summary>
                public string DeviceName { get; set; } = string.Empty;
                
                /// <summary>
                /// Gets or sets the device type (input or output).
                /// </summary>
                public string DeviceType { get; set; } = string.Empty;
                
                /// <summary>
                /// Gets or sets whether the device is virtual.
                /// </summary>
                public bool IsVirtual { get; set; }
                
                /// <summary>
                /// Gets or sets the number of channels.
                /// </summary>
                public int ChannelCount { get; set; }
                
                /// <summary>
                /// Gets or sets the sample rate.
                /// </summary>
                public int SampleRate { get; set; }
                
                /// <summary>
                /// Gets or sets the bit depth.
                /// </summary>
                public int BitDepth { get; set; }
            }
        }
    }
    
    /// <summary>
    /// Specific message payloads for mixer controls
    /// </summary>
    public class MixerControlPayloads
    {
        /// <summary>
        /// Payload for setting channel properties
        /// </summary>
        public class ChannelPropertiesPayload
        {
            /// <summary>
            /// Gets or sets the channel ID.
            /// </summary>
            public string ChannelId { get; set; } = string.Empty;
            
            /// <summary>
            /// Gets or sets the channel properties.
            /// </summary>
            public ChannelProperties Properties { get; set; } = new ChannelProperties();
            
            /// <summary>
            /// Represents the properties of an audio channel.
            /// </summary>
            public class ChannelProperties
            {
                /// <summary>
                /// Gets or sets the volume level (0.0 to 1.0).
                /// </summary>
                public float Volume { get; set; } = 1.0f;
                
                /// <summary>
                /// Gets or sets the pan position (-1.0 to 1.0).
                /// </summary>
                public float Pan { get; set; } = 0.0f;
                
                /// <summary>
                /// Gets or sets whether the channel is muted.
                /// </summary>
                public bool Mute { get; set; } = false;
                
                /// <summary>
                /// Gets or sets whether the channel is soloed.
                /// </summary>
                public bool Solo { get; set; } = false;
                
                /// <summary>
                /// Gets or sets the gain level in dB.
                /// </summary>
                public float Gain { get; set; } = 0.0f;
            }
        }
    }
    
    /// <summary>
    /// Specific message payloads for events
    /// </summary>
    public class EventPayloads
    {
        /// <summary>
        /// Payload for audio level update events
        /// </summary>
        public class LevelUpdatePayload
        {
            /// <summary>
            /// Gets or sets the channel ID.
            /// </summary>
            public string ChannelId { get; set; } = string.Empty;
            
            /// <summary>
            /// Gets or sets the peak level in dB.
            /// </summary>
            public float PeakLevel { get; set; }
            
            /// <summary>
            /// Gets or sets the RMS level in dB.
            /// </summary>
            public float RmsLevel { get; set; }
        }
        
        /// <summary>
        /// Payload for device change events
        /// </summary>
        public class DeviceChangedPayload
        {
            /// <summary>
            /// Gets or sets the change type (added or removed).
            /// </summary>
            public string ChangeType { get; set; } = string.Empty;
            
            /// <summary>
            /// Gets or sets the device ID.
            /// </summary>
            public string DeviceId { get; set; } = string.Empty;
            
            /// <summary>
            /// Gets or sets the device name.
            /// </summary>
            public string DeviceName { get; set; } = string.Empty;
            
            /// <summary>
            /// Gets or sets the device type (input or output).
            /// </summary>
            public string DeviceType { get; set; } = string.Empty;
        }
    }
    
    /// <summary>
    /// Specific message payloads for JACK-specific commands
    /// </summary>
    public class JackCommandPayloads
    {
        /// <summary>
        /// Payload for JACK status response
        /// </summary>
        public class JackStatusPayload
        {
            /// <summary>
            /// Gets or sets whether JACK is running.
            /// </summary>
            public bool Running { get; set; }
            
            /// <summary>
            /// Gets or sets the sample rate.
            /// </summary>
            public int SampleRate { get; set; }
            
            /// <summary>
            /// Gets or sets the buffer size.
            /// </summary>
            public int BufferSize { get; set; }
            
            /// <summary>
            /// Gets or sets the CPU load.
            /// </summary>
            public float CpuLoad { get; set; }
            
            /// <summary>
            /// Gets or sets the number of xruns.
            /// </summary>
            public int Xruns { get; set; }
            
            /// <summary>
            /// Gets or sets the latency in milliseconds.
            /// </summary>
            public float Latency { get; set; }
        }
        
        /// <summary>
        /// Payload for listing JACK ports
        /// </summary>
        public class ListJackPortsPayload
        {
            /// <summary>
            /// Gets or sets the port type.
            /// </summary>
            public string PortType { get; set; } = string.Empty;
            
            /// <summary>
            /// Gets or sets the port direction.
            /// </summary>
            public string Direction { get; set; } = string.Empty;
            
            /// <summary>
            /// Gets or sets the port flags.
            /// </summary>
            public string[] Flags { get; set; } = Array.Empty<string>();
        }
        
        /// <summary>
        /// Payload for connecting JACK ports
        /// </summary>
        public class ConnectJackPortsPayload
        {
            /// <summary>
            /// Gets or sets the source port.
            /// </summary>
            public string SourcePort { get; set; } = string.Empty;
            
            /// <summary>
            /// Gets or sets the destination port.
            /// </summary>
            public string DestinationPort { get; set; } = string.Empty;
        }
        
        /// <summary>
        /// Payload for starting the JACK server
        /// </summary>
        public class StartJackServerPayload
        {
            /// <summary>
            /// Gets or sets the sample rate.
            /// </summary>
            public int SampleRate { get; set; } = 48000;
            
            /// <summary>
            /// Gets or sets the buffer size.
            /// </summary>
            public int BufferSize { get; set; } = 1024;
            
            /// <summary>
            /// Gets or sets the number of periods.
            /// </summary>
            public int Periods { get; set; } = 2;
            
            /// <summary>
            /// Gets or sets the priority.
            /// </summary>
            public string Priority { get; set; } = "high";
        }
    }
}
