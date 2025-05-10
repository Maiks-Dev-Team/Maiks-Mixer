using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaiksMixer.Core.Communication;
using MaiksMixer.Core.Communication.Models;
using MaiksMixer.Core.Models;

namespace MaiksMixer.Core.Services
{
    /// <summary>
    /// Service for managing audio devices.
    /// </summary>
    public class AudioDeviceService
    {
        private readonly ConnectionManager _connectionManager;
        private readonly Dictionary<string, AudioDevice> _devices = new Dictionary<string, AudioDevice>();
        private readonly Dictionary<string, AudioConnection> _connections = new Dictionary<string, AudioConnection>();
        
        /// <summary>
        /// Event raised when devices are updated.
        /// </summary>
        public event EventHandler<EventArgs>? DevicesUpdated;
        
        /// <summary>
        /// Event raised when connections are updated.
        /// </summary>
        public event EventHandler<EventArgs>? ConnectionsUpdated;
        
        /// <summary>
        /// Event raised when audio levels are updated.
        /// </summary>
        public event EventHandler<AudioLevelEventArgs>? LevelUpdated;
        
        /// <summary>
        /// Initializes a new instance of the AudioDeviceService class.
        /// </summary>
        /// <param name="connectionManager">The connection manager to use for communication.</param>
        public AudioDeviceService(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            
            // Register for events from the connection manager
            _connectionManager.MessageReceived += ConnectionManager_MessageReceived;
        }
        
        /// <summary>
        /// Gets a list of all audio devices.
        /// </summary>
        /// <returns>A list of audio devices.</returns>
        public List<AudioDevice> GetDevices()
        {
            return _devices.Values.ToList();
        }
        
        /// <summary>
        /// Gets a list of input devices.
        /// </summary>
        /// <returns>A list of input devices.</returns>
        public List<AudioDevice> GetInputDevices()
        {
            return _devices.Values.Where(d => d.IsInput).ToList();
        }
        
        /// <summary>
        /// Gets a list of output devices.
        /// </summary>
        /// <returns>A list of output devices.</returns>
        public List<AudioDevice> GetOutputDevices()
        {
            return _devices.Values.Where(d => d.IsOutput).ToList();
        }
        
        /// <summary>
        /// Gets a list of virtual devices.
        /// </summary>
        /// <returns>A list of virtual devices.</returns>
        public List<AudioDevice> GetVirtualDevices()
        {
            return _devices.Values.Where(d => d.IsVirtual).ToList();
        }
        
        /// <summary>
        /// Gets a device by ID.
        /// </summary>
        /// <param name="deviceId">The device ID.</param>
        /// <returns>The device, or null if not found.</returns>
        public AudioDevice? GetDevice(string deviceId)
        {
            return _devices.TryGetValue(deviceId, out var device) ? device : null;
        }
        
        /// <summary>
        /// Gets a list of all audio connections.
        /// </summary>
        /// <returns>A list of audio connections.</returns>
        public List<AudioConnection> GetConnections()
        {
            return _connections.Values.ToList();
        }
        
        /// <summary>
        /// Gets connections for a specific source port.
        /// </summary>
        /// <param name="sourceId">The ID of the source port.</param>
        /// <returns>A list of connections from the specified source port.</returns>
        public List<AudioConnection> GetConnectionsFromSource(string sourceId)
        {
            return _connections.Values.Where(c => c.SourceId == sourceId).ToList();
        }
        
        /// <summary>
        /// Gets connections for a specific destination port.
        /// </summary>
        /// <param name="destinationId">The ID of the destination port.</param>
        /// <returns>A list of connections to the specified destination port.</returns>
        public List<AudioConnection> GetConnectionsToDestination(string destinationId)
        {
            return _connections.Values.Where(c => c.DestinationId == destinationId).ToList();
        }
        
        /// <summary>
        /// Gets a connection by ID.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <returns>The connection, or null if not found.</returns>
        public AudioConnection? GetConnection(string connectionId)
        {
            return _connections.TryGetValue(connectionId, out var connection) ? connection : null;
        }
        
        /// <summary>
        /// Refreshes the list of devices from the audio engine.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RefreshDevicesAsync()
        {
            try
            {
                // Create a command to list devices
                var command = new CommandMessage
                {
                    Command = "ListDevices",
                    Parameters = new Dictionary<string, object>()
                };
                
                // Send the command and wait for response
                var response = await _connectionManager.SendCommandAsync(command);
                
                // Process the response
                if (response.Status == "Success" && response.Data is List<object> deviceList)
                {
                    // Clear existing devices
                    _devices.Clear();
                    
                    // Add devices from response
                    foreach (var deviceObj in deviceList)
                    {
                        if (deviceObj is Dictionary<string, object> deviceDict)
                        {
                            var device = ConvertToAudioDevice(deviceDict);
                            if (device != null)
                            {
                                _devices[device.Id] = device;
                            }
                        }
                    }
                    
                    // Raise event
                    DevicesUpdated?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error refreshing devices: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Refreshes the list of connections from the audio engine.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RefreshConnectionsAsync()
        {
            try
            {
                // Create a command to get the routing matrix
                var command = new CommandMessage
                {
                    Command = "GetRoutingMatrix",
                    Parameters = new Dictionary<string, object>()
                };
                
                // Send the command and wait for response
                var response = await _connectionManager.SendCommandAsync(command);
                
                // Process the response
                if (response.Status == "Success" && response.Data is List<object> connectionList)
                {
                    // Clear existing connections
                    _connections.Clear();
                    
                    // Add connections from response
                    foreach (var connectionObj in connectionList)
                    {
                        if (connectionObj is Dictionary<string, object> connectionDict)
                        {
                            var connection = ConvertToAudioConnection(connectionDict);
                            if (connection != null)
                            {
                                _connections[connection.Id] = connection;
                            }
                        }
                    }
                    
                    // Raise event
                    ConnectionsUpdated?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error refreshing connections: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Creates a virtual device.
        /// </summary>
        /// <param name="name">The name of the device.</param>
        /// <param name="inputChannels">The number of input channels.</param>
        /// <param name="outputChannels">The number of output channels.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<bool> CreateVirtualDeviceAsync(string name, int inputChannels, int outputChannels)
        {
            try
            {
                // Create a command to create a virtual device
                var command = new CommandMessage
                {
                    Command = "CreateVirtualDevice",
                    Parameters = new Dictionary<string, object>
                    {
                        { "name", name },
                        { "inputChannels", inputChannels },
                        { "outputChannels", outputChannels }
                    }
                };
                
                // Send the command and wait for response
                var response = await _connectionManager.SendCommandAsync(command);
                
                // Check if the operation was successful
                if (response.Status == "Success")
                {
                    // Refresh devices to get the new device
                    await RefreshDevicesAsync();
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error creating virtual device: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Removes a virtual device.
        /// </summary>
        /// <param name="deviceId">The ID of the device to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<bool> RemoveVirtualDeviceAsync(string deviceId)
        {
            try
            {
                // Create a command to remove a virtual device
                var command = new CommandMessage
                {
                    Command = "RemoveVirtualDevice",
                    Parameters = new Dictionary<string, object>
                    {
                        { "deviceId", deviceId }
                    }
                };
                
                // Send the command and wait for response
                var response = await _connectionManager.SendCommandAsync(command);
                
                // Check if the operation was successful
                if (response.Status == "Success")
                {
                    // Refresh devices to update the list
                    await RefreshDevicesAsync();
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error removing virtual device: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Sets a connection between two ports.
        /// </summary>
        /// <param name="sourceId">The ID of the source port.</param>
        /// <param name="destinationId">The ID of the destination port.</param>
        /// <param name="isConnected">Whether the ports should be connected.</param>
        /// <param name="volume">The volume level for the connection (0.0 to 1.0).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SetConnectionAsync(string sourceId, string destinationId, bool isConnected, double volume = 1.0)
        {
            // Create the command message
            var command = new CommandMessage
            {
                MessageType = "Command",
                Command = isConnected ? "ConnectPorts" : "DisconnectPorts",
                Parameters = new Dictionary<string, object>
                {
                    { "sourceId", sourceId },
                    { "destinationId", destinationId },
                    { "volume", volume }
                }
            };
            
            // Send the command
            var response = await _connectionManager.SendCommandAsync(command);
            
            // Check for success
            if (response.Success)
            {
                // Update the connection in the local cache
                string connectionId = $"{sourceId}:{destinationId}";
                
                if (isConnected)
                {
                    // Add or update the connection
                    if (_connections.TryGetValue(connectionId, out var existingConnection))
                    {
                        existingConnection.Status = ConnectionStatus.Active;
                        existingConnection.Volume = volume;
                        existingConnection.UpdatedAt = DateTime.Now;
                    }
                    else
                    {
                        _connections[connectionId] = new AudioConnection
                        {
                            Id = connectionId,
                            SourceId = sourceId,
                            DestinationId = destinationId,
                            Status = ConnectionStatus.Active,
                            Volume = volume,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };
                    }
                }
                else
                {
                    // Remove the connection
                    _connections.Remove(connectionId);
                }
                
                // Raise the connections updated event
                ConnectionsUpdated?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                // Handle error
                throw new Exception($"Failed to set connection: {response.ErrorMessage}");
            }
        }
        
        /// <summary>
        /// Sets the properties of an audio channel.
        /// </summary>
        /// <param name="deviceId">The ID of the device.</param>
        /// <param name="portId">The ID of the port.</param>
        /// <param name="volume">The volume level (0.0 to 1.0).</param>
        /// <param name="pan">The pan position (-1.0 to 1.0).</param>
        /// <param name="isMuted">Whether the channel is muted.</param>
        /// <param name="isSolo">Whether the channel is soloed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SetChannelPropertiesAsync(string deviceId, string portId, double volume, double pan, bool isMuted, bool isSolo)
        {
            // Create the command message
            var command = new CommandMessage
            {
                MessageType = "Command",
                Command = "SetChannelProperties",
                Parameters = new Dictionary<string, object>
                {
                    { "deviceId", deviceId },
                    { "portId", portId },
                    { "volume", volume },
                    { "pan", pan },
                    { "muted", isMuted },
                    { "solo", isSolo }
                }
            };
            
            // Send the command
            var response = await _connectionManager.SendCommandAsync(command);
            
            // Check for success
            if (!response.Success)
            {
                // Handle error
                throw new Exception($"Failed to set channel properties: {response.ErrorMessage}");
            }
        }
        
        /// <summary>
        /// Gets the routing matrix.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<List<AudioConnectionInfo>> GetRoutingMatrixAsync()
        {
            // Create the command message
            var command = new CommandMessage
            {
                MessageType = "Command",
                Command = "GetRoutingMatrix",
                Parameters = new Dictionary<string, object>()
            };
            
            // Send the command
            var response = await _connectionManager.SendCommandAsync(command);
            
            // Check for success
            if (response.Success)
            {
                // Parse the routing matrix from the response
                if (response.Result is List<object> matrixList)
                {                    
                    var matrix = new List<AudioConnectionInfo>();
                    
                    foreach (var item in matrixList)
                    {
                        if (item is Dictionary<string, object> dict)
                        {
                            var connection = new AudioConnectionInfo
                            {
                                Id = dict.TryGetValue("id", out var id) ? id.ToString() ?? string.Empty : string.Empty,
                                SourceId = dict.TryGetValue("sourceId", out var sourceId) ? sourceId.ToString() ?? string.Empty : string.Empty,
                                DestinationId = dict.TryGetValue("destinationId", out var destId) ? destId.ToString() ?? string.Empty : string.Empty,
                                SourceName = dict.TryGetValue("sourceName", out var sourceName) ? sourceName.ToString() ?? string.Empty : string.Empty,
                                DestinationName = dict.TryGetValue("destinationName", out var destName) ? destName.ToString() ?? string.Empty : string.Empty,
                                IsConnected = dict.TryGetValue("isConnected", out var isConnected) && isConnected is bool connected && connected,
                                Volume = dict.TryGetValue("volume", out var volume) && volume is double vol ? vol : 1.0
                            };
                            
                            // Parse status
                            if (dict.TryGetValue("status", out var status) && status is string statusStr)
                            {
                                if (Enum.TryParse<ConnectionStatus>(statusStr, true, out var st))
                                {
                                    connection.Status = st;
                                }
                            }
                            
                            matrix.Add(connection);
                        }
                    }
                    
                    return matrix;
                }
            }
            
            // Return an empty list on error
            return new List<AudioConnectionInfo>();
        }
        
        /// <summary>
        /// Handles the MessageReceived event from the connection manager.
        /// </summary>
        private void ConnectionManager_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message.MessageType == "Event")
            {
                if (e.Message.EventType == "DevicesUpdated")
                {
                    // Refresh devices
                    Task.Run(RefreshDevicesAsync);
                }
                else if (e.Message.EventType == "ConnectionsUpdated")
                {
                    // Refresh connections
                    Task.Run(RefreshConnectionsAsync);
                }
                else if (e.Message.EventType == "LevelUpdated" && e.Message.Parameters != null)
                {
                    // Extract level data
                    if (e.Message.Parameters.TryGetValue("portId", out var portId) && portId is string portIdStr &&
                        e.Message.Parameters.TryGetValue("leftLevel", out var leftLevel) && leftLevel is double leftLevelVal &&
                        e.Message.Parameters.TryGetValue("rightLevel", out var rightLevel) && rightLevel is double rightLevelVal)
                    {
                        // Raise the level updated event
                        LevelUpdated?.Invoke(this, new AudioLevelEventArgs(portIdStr, leftLevelVal, rightLevelVal));
                    }
                }
            }
        }
        
        /// <summary>
        /// Converts a dictionary to an AudioDevice object.
        /// </summary>
        private AudioDevice? ConvertToAudioDevice(Dictionary<string, object> dict)
        {
            try
            {
                var device = new AudioDevice
                {
                    Id = dict.TryGetValue("id", out var id) ? id.ToString() ?? string.Empty : string.Empty,
                    Name = dict.TryGetValue("name", out var name) ? name.ToString() ?? string.Empty : string.Empty,
                    IsInput = dict.TryGetValue("isInput", out var isInput) && isInput is bool b1 && b1,
                    IsOutput = dict.TryGetValue("isOutput", out var isOutput) && isOutput is bool b2 && b2,
                    IsVirtual = dict.TryGetValue("isVirtual", out var isVirtual) && isVirtual is bool b3 && b3,
                    IsEnabled = dict.TryGetValue("isEnabled", out var isEnabled) && isEnabled is bool b4 && b4,
                    SampleRate = dict.TryGetValue("sampleRate", out var sampleRate) && sampleRate is int sr ? sr : 0,
                    BufferSize = dict.TryGetValue("bufferSize", out var bufferSize) && bufferSize is int bs ? bs : 0,
                    InputChannels = dict.TryGetValue("inputChannels", out var inputChannels) && inputChannels is int ic ? ic : 0,
                    OutputChannels = dict.TryGetValue("outputChannels", out var outputChannels) && outputChannels is int oc ? oc : 0,
                    DriverType = dict.TryGetValue("driverType", out var driverType) ? driverType.ToString() ?? string.Empty : string.Empty,
                    LastError = dict.TryGetValue("lastError", out var lastError) ? lastError.ToString() ?? string.Empty : string.Empty
                };
                
                // Parse device type
                if (dict.TryGetValue("deviceType", out var deviceType) && deviceType is string dtStr)
                {
                    if (Enum.TryParse<AudioDeviceType>(dtStr, true, out var dt))
                    {
                        device.DeviceType = dt;
                    }
                }
                
                // Parse status
                if (dict.TryGetValue("status", out var status) && status is string statusStr)
                {
                    if (Enum.TryParse<AudioDeviceStatus>(statusStr, true, out var st))
                    {
                        device.Status = st;
                    }
                }
                
                // Parse input ports
                if (dict.TryGetValue("inputPorts", out var inputPorts) && inputPorts is List<object> ipList)
                {
                    foreach (var portObj in ipList)
                    {
                        if (portObj is Dictionary<string, object> portDict)
                        {
                            var port = ConvertToAudioPort(portDict);
                            if (port != null)
                            {
                                port.IsInput = true;
                                device.InputPorts.Add(port);
                            }
                        }
                    }
                }
                
                // Parse output ports
                if (dict.TryGetValue("outputPorts", out var outputPorts) && outputPorts is List<object> opList)
                {
                    foreach (var portObj in opList)
                    {
                        if (portObj is Dictionary<string, object> portDict)
                        {
                            var port = ConvertToAudioPort(portDict);
                            if (port != null)
                            {
                                port.IsInput = false;
                                device.OutputPorts.Add(port);
                            }
                        }
                    }
                }
                
                // Parse properties
                if (dict.TryGetValue("properties", out var properties) && properties is Dictionary<string, object> propsDict)
                {
                    foreach (var kvp in propsDict)
                    {
                        device.Properties[kvp.Key] = kvp.Value.ToString() ?? string.Empty;
                    }
                }
                
                return device;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error converting to audio device: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Converts a dictionary to an AudioPort object.
        /// </summary>
        private AudioPort? ConvertToAudioPort(Dictionary<string, object> dict)
        {
            try
            {
                var port = new AudioPort
                {
                    Id = dict.TryGetValue("id", out var id) ? id.ToString() ?? string.Empty : string.Empty,
                    Name = dict.TryGetValue("name", out var name) ? name.ToString() ?? string.Empty : string.Empty,
                    IsInput = dict.TryGetValue("isInput", out var isInput) && isInput is bool b1 && b1,
                    Channel = dict.TryGetValue("channel", out var channel) && channel is int ch ? ch : 0,
                    IsConnected = dict.TryGetValue("isConnected", out var isConnected) && isConnected is bool b2 && b2
                };
                
                // Parse connections
                if (dict.TryGetValue("connections", out var connections) && connections is List<object> connList)
                {
                    foreach (var connObj in connList)
                    {
                        if (connObj is string connStr)
                        {
                            port.Connections.Add(connStr);
                        }
                    }
                }
                
                return port;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error converting to audio port: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Converts a dictionary to an AudioConnection object.
        /// </summary>
        private AudioConnection? ConvertToAudioConnection(Dictionary<string, object> dict)
        {
            try
            {
                var connection = new AudioConnection
                {
                    Id = dict.TryGetValue("id", out var id) ? id.ToString() ?? string.Empty : string.Empty,
                    SourceId = dict.TryGetValue("sourceId", out var sourceId) ? sourceId.ToString() ?? string.Empty : string.Empty,
                    DestinationId = dict.TryGetValue("destinationId", out var destId) ? destId.ToString() ?? string.Empty : string.Empty,
                    Volume = dict.TryGetValue("volume", out var volume) && volume is double vol ? vol : 1.0
                };
                
                // Parse status
                if (dict.TryGetValue("status", out var status) && status is string statusStr)
                {
                    if (Enum.TryParse<ConnectionStatus>(statusStr, true, out var st))
                    {
                        connection.Status = st;
                    }
                }
                
                // Parse timestamps
                if (dict.TryGetValue("createdAt", out var createdAt) && createdAt is string createdAtStr)
                {
                    if (DateTime.TryParse(createdAtStr, out var dt))
                    {
                        connection.CreatedAt = dt;
                    }
                }
                
                if (dict.TryGetValue("updatedAt", out var updatedAt) && updatedAt is string updatedAtStr)
                {
                    if (DateTime.TryParse(updatedAtStr, out var dt))
                    {
                        connection.UpdatedAt = dt;
                    }
                }
                
                return connection;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error converting to audio connection: {ex.Message}");
                return null;
            }
        }
    }
}
