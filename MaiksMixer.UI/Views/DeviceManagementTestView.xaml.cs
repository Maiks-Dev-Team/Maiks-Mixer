using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MaiksMixer.Core.Communication;
using MaiksMixer.Core.Communication.Models;
using MaiksMixer.Core.Services;

namespace MaiksMixer.UI.Views
{
    /// <summary>
    /// Interaction logic for DeviceManagementTestView.xaml
    /// </summary>
    public partial class DeviceManagementTestView : UserControl
    {
        private readonly AudioDeviceService _deviceService;
        private readonly MockConnectionManager _mockConnectionManager;
        
        /// <summary>
        /// Initializes a new instance of the DeviceManagementTestView class.
        /// </summary>
        public DeviceManagementTestView()
        {
            InitializeComponent();
            
            // Create mock connection manager
            _mockConnectionManager = new MockConnectionManager();
            
            // Create device service with mock connection manager
            _deviceService = new AudioDeviceService(_mockConnectionManager);
            
            // Create device management view
            var deviceManagementView = new DeviceManagementView(_deviceService);
            DeviceManagementContainer.Content = deviceManagementView;
            
            // Add sample devices
            AddSampleDevices();
        }
        
        /// <summary>
        /// Adds sample devices to the mock connection manager.
        /// </summary>
        private void AddSampleDevices()
        {
            // Create sample physical devices
            var physicalDevices = new List<AudioDevice>
            {
                new AudioDevice
                {
                    Id = "physical1",
                    Name = "Focusrite Scarlett 2i2",
                    DeviceType = AudioDeviceType.PhysicalInterface,
                    IsInput = true,
                    IsOutput = true,
                    IsVirtual = false,
                    IsEnabled = true,
                    SampleRate = 48000,
                    BufferSize = 256,
                    InputChannels = 2,
                    OutputChannels = 2,
                    Status = AudioDeviceStatus.Online,
                    DriverType = "ASIO",
                    InputPorts = new List<AudioPort>
                    {
                        new AudioPort { Id = "physical1_in1", Name = "Input 1", IsInput = true, Channel = 0 },
                        new AudioPort { Id = "physical1_in2", Name = "Input 2", IsInput = true, Channel = 1 }
                    },
                    OutputPorts = new List<AudioPort>
                    {
                        new AudioPort { Id = "physical1_out1", Name = "Output 1", IsInput = false, Channel = 0 },
                        new AudioPort { Id = "physical1_out2", Name = "Output 2", IsInput = false, Channel = 1 }
                    }
                },
                new AudioDevice
                {
                    Id = "physical2",
                    Name = "Realtek HD Audio",
                    DeviceType = AudioDeviceType.WasapiDevice,
                    IsInput = true,
                    IsOutput = true,
                    IsVirtual = false,
                    IsEnabled = true,
                    SampleRate = 44100,
                    BufferSize = 512,
                    InputChannels = 1,
                    OutputChannels = 2,
                    Status = AudioDeviceStatus.Online,
                    DriverType = "WASAPI",
                    InputPorts = new List<AudioPort>
                    {
                        new AudioPort { Id = "physical2_in1", Name = "Microphone", IsInput = true, Channel = 0 }
                    },
                    OutputPorts = new List<AudioPort>
                    {
                        new AudioPort { Id = "physical2_out1", Name = "Speaker Left", IsInput = false, Channel = 0 },
                        new AudioPort { Id = "physical2_out2", Name = "Speaker Right", IsInput = false, Channel = 1 }
                    }
                },
                new AudioDevice
                {
                    Id = "physical3",
                    Name = "ASIO4ALL",
                    DeviceType = AudioDeviceType.AsioDevice,
                    IsInput = true,
                    IsOutput = true,
                    IsVirtual = false,
                    IsEnabled = true,
                    SampleRate = 48000,
                    BufferSize = 128,
                    InputChannels = 2,
                    OutputChannels = 2,
                    Status = AudioDeviceStatus.Offline,
                    DriverType = "ASIO",
                    InputPorts = new List<AudioPort>
                    {
                        new AudioPort { Id = "physical3_in1", Name = "Input 1", IsInput = true, Channel = 0 },
                        new AudioPort { Id = "physical3_in2", Name = "Input 2", IsInput = true, Channel = 1 }
                    },
                    OutputPorts = new List<AudioPort>
                    {
                        new AudioPort { Id = "physical3_out1", Name = "Output 1", IsInput = false, Channel = 0 },
                        new AudioPort { Id = "physical3_out2", Name = "Output 2", IsInput = false, Channel = 1 }
                    }
                }
            };
            
            // Create sample virtual devices
            var virtualDevices = new List<AudioDevice>
            {
                new AudioDevice
                {
                    Id = "virtual1",
                    Name = "Virtual Microphone",
                    DeviceType = AudioDeviceType.VirtualDevice,
                    IsInput = true,
                    IsOutput = false,
                    IsVirtual = true,
                    IsEnabled = true,
                    SampleRate = 48000,
                    BufferSize = 256,
                    InputChannels = 2,
                    OutputChannels = 0,
                    Status = AudioDeviceStatus.Online,
                    DriverType = "JACK",
                    InputPorts = new List<AudioPort>
                    {
                        new AudioPort { Id = "virtual1_in1", Name = "Input 1", IsInput = true, Channel = 0 },
                        new AudioPort { Id = "virtual1_in2", Name = "Input 2", IsInput = true, Channel = 1 }
                    }
                },
                new AudioDevice
                {
                    Id = "virtual2",
                    Name = "Virtual Speaker",
                    DeviceType = AudioDeviceType.VirtualDevice,
                    IsInput = false,
                    IsOutput = true,
                    IsVirtual = true,
                    IsEnabled = true,
                    SampleRate = 48000,
                    BufferSize = 256,
                    InputChannels = 0,
                    OutputChannels = 2,
                    Status = AudioDeviceStatus.Online,
                    DriverType = "JACK",
                    OutputPorts = new List<AudioPort>
                    {
                        new AudioPort { Id = "virtual2_out1", Name = "Output 1", IsInput = false, Channel = 0 },
                        new AudioPort { Id = "virtual2_out2", Name = "Output 2", IsInput = false, Channel = 1 }
                    }
                }
            };
            
            // Add devices to mock connection manager
            _mockConnectionManager.SetDevices(physicalDevices);
            _mockConnectionManager.SetDevices(virtualDevices);
            
            // Create sample connections
            var connections = new List<AudioConnection>
            {
                new AudioConnection
                {
                    Id = "conn1",
                    SourceId = "physical1_out1",
                    DestinationId = "virtual2_out1",
                    Status = ConnectionStatus.Connected,
                    Volume = 0.8
                },
                new AudioConnection
                {
                    Id = "conn2",
                    SourceId = "physical1_out2",
                    DestinationId = "virtual2_out2",
                    Status = ConnectionStatus.Connected,
                    Volume = 0.8
                },
                new AudioConnection
                {
                    Id = "conn3",
                    SourceId = "virtual1_in1",
                    DestinationId = "physical2_out1",
                    Status = ConnectionStatus.Muted,
                    Volume = 0.5
                }
            };
            
            // Add connections to mock connection manager
            _mockConnectionManager.SetConnections(connections);
        }
    }
    
    /// <summary>
    /// A mock connection manager for testing.
    /// </summary>
    public class MockConnectionManager : ConnectionManager
    {
        private readonly List<AudioDevice> _devices = new List<AudioDevice>();
        private readonly List<AudioConnection> _connections = new List<AudioConnection>();
        
        /// <summary>
        /// Initializes a new instance of the MockConnectionManager class.
        /// </summary>
        public MockConnectionManager() : base("MockConnectionManager")
        {
        }
        
        /// <summary>
        /// Adds devices to the mock connection manager.
        /// </summary>
        /// <param name="devices">The devices to add.</param>
        public void SetDevices(List<AudioDevice> devices)
        {
            _devices.AddRange(devices);
            
            // Raise device added events
            foreach (var device in devices)
            {
                RaiseDeviceEvent("DeviceAdded", device);
            }
        }
        
        /// <summary>
        /// Adds connections to the mock connection manager.
        /// </summary>
        /// <param name="connections">The connections to add.</param>
        public void SetConnections(List<AudioConnection> connections)
        {
            _connections.AddRange(connections);
            
            // Raise connection changed events
            foreach (var connection in connections)
            {
                RaiseConnectionEvent("ConnectionChanged", connection);
            }
        }
        
        /// <summary>
        /// Sends a command to the audio engine.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task<ResponseMessage> SendCommandAsync(CommandMessage command)
        {
            // Handle different commands
            switch (command.Command)
            {
                case "ListDevices":
                    return new ResponseMessage
                    {
                        Status = "Success",
                        Data = _devices
                    };
                
                case "GetRoutingMatrix":
                    return new ResponseMessage
                    {
                        Status = "Success",
                        Data = _connections
                    };
                
                case "CreateVirtualDevice":
                    if (command.Parameters.TryGetValue("name", out var nameObj) && nameObj is string name &&
                        command.Parameters.TryGetValue("inputChannels", out var inChObj) && inChObj is int inputChannels &&
                        command.Parameters.TryGetValue("outputChannels", out var outChObj) && outChObj is int outputChannels)
                    {
                        // Create new device
                        var device = new AudioDevice
                        {
                            Id = $"virtual{_devices.Count + 1}",
                            Name = name,
                            DeviceType = AudioDeviceType.VirtualDevice,
                            IsInput = inputChannels > 0,
                            IsOutput = outputChannels > 0,
                            IsVirtual = true,
                            IsEnabled = true,
                            SampleRate = 48000,
                            BufferSize = 256,
                            InputChannels = inputChannels,
                            OutputChannels = outputChannels,
                            Status = AudioDeviceStatus.Online,
                            DriverType = "JACK"
                        };
                        
                        // Add input ports
                        for (int i = 0; i < inputChannels; i++)
                        {
                            device.InputPorts.Add(new AudioPort
                            {
                                Id = $"{device.Id}_in{i + 1}",
                                Name = $"Input {i + 1}",
                                IsInput = true,
                                Channel = i
                            });
                        }
                        
                        // Add output ports
                        for (int i = 0; i < outputChannels; i++)
                        {
                            device.OutputPorts.Add(new AudioPort
                            {
                                Id = $"{device.Id}_out{i + 1}",
                                Name = $"Output {i + 1}",
                                IsInput = false,
                                Channel = i
                            });
                        }
                        
                        // Add to devices
                        _devices.Add(device);
                        
                        // Raise device added event
                        RaiseDeviceEvent("DeviceAdded", device);
                        
                        return new ResponseMessage
                        {
                            Status = "Success",
                            Data = device
                        };
                    }
                    
                    return new ResponseMessage
                    {
                        Status = "Error",
                        ErrorMessage = "Invalid parameters for CreateVirtualDevice command."
                    };
                
                case "RemoveVirtualDevice":
                    if (command.Parameters.TryGetValue("deviceId", out var deviceIdObj) && deviceIdObj is string deviceId)
                    {
                        // Find device
                        var device = _devices.Find(d => d.Id == deviceId);
                        if (device != null && device.IsVirtual)
                        {
                            // Remove device
                            _devices.Remove(device);
                            
                            // Raise device removed event
                            RaiseDeviceEvent("DeviceRemoved", device);
                            
                            return new ResponseMessage
                            {
                                Status = "Success"
                            };
                        }
                        
                        return new ResponseMessage
                        {
                            Status = "Error",
                            ErrorMessage = "Device not found or not a virtual device."
                        };
                    }
                    
                    return new ResponseMessage
                    {
                        Status = "Error",
                        ErrorMessage = "Invalid parameters for RemoveVirtualDevice command."
                    };
                
                case "SetRoute":
                    if (command.Parameters.TryGetValue("sourceId", out var sourceIdObj) && sourceIdObj is string sourceId &&
                        command.Parameters.TryGetValue("destinationId", out var destIdObj) && destIdObj is string destId &&
                        command.Parameters.TryGetValue("enabled", out var enabledObj) && enabledObj is bool enabled &&
                        command.Parameters.TryGetValue("volume", out var volumeObj) && volumeObj is double volume)
                    {
                        // Find existing connection
                        var connection = _connections.Find(c => c.SourceId == sourceId && c.DestinationId == destId);
                        
                        if (connection != null)
                        {
                            // Update connection
                            connection.Status = enabled ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;
                            connection.Volume = volume;
                            connection.UpdatedAt = DateTime.Now;
                        }
                        else if (enabled)
                        {
                            // Create new connection
                            connection = new AudioConnection
                            {
                                Id = $"conn{_connections.Count + 1}",
                                SourceId = sourceId,
                                DestinationId = destId,
                                Status = ConnectionStatus.Connected,
                                Volume = volume
                            };
                            
                            // Add to connections
                            _connections.Add(connection);
                        }
                        
                        // Raise connection changed event
                        if (connection != null)
                        {
                            RaiseConnectionEvent("ConnectionChanged", connection);
                        }
                        
                        return new ResponseMessage
                        {
                            Status = "Success"
                        };
                    }
                    
                    return new ResponseMessage
                    {
                        Status = "Error",
                        ErrorMessage = "Invalid parameters for SetRoute command."
                    };
                
                default:
                    return new ResponseMessage
                    {
                        Status = "Error",
                        ErrorMessage = $"Unknown command: {command.Command}"
                    };
            }
        }
        
        /// <summary>
        /// Raises a device event.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="device">The device.</param>
        private void RaiseDeviceEvent(string eventType, AudioDevice device)
        {
            var eventMessage = new EventMessage
            {
                EventType = eventType,
                Data = device
            };
            
            OnMessageReceived(new MessageReceivedEventArgs(eventMessage));
        }
        
        /// <summary>
        /// Raises a connection event.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="connection">The connection.</param>
        private void RaiseConnectionEvent(string eventType, AudioConnection connection)
        {
            var eventMessage = new EventMessage
            {
                EventType = eventType,
                Data = connection
            };
            
            OnMessageReceived(new MessageReceivedEventArgs(eventMessage));
        }
    }
}
