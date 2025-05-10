using System;
using System.Threading.Tasks;
using MaiksMixer.Core.Communication.Models;
using MaiksMixer.Core.Logging;

namespace MaiksMixer.Core.Communication
{
    /// <summary>
    /// Service for managing inter-process communication.
    /// </summary>
    public class CommunicationService : IDisposable
    {
        private readonly NamedPipeServer _pipeServer;
        private readonly string _pipeName;
        private bool _isDisposed;

        /// <summary>
        /// Event raised when a command is received from a client.
        /// </summary>
        public event EventHandler<CommandMessage>? CommandReceived;
        
        /// <summary>
        /// Event raised when an event message is received from a client.
        /// </summary>
        public event EventHandler<EventMessage>? EventReceived;

        /// <summary>
        /// Event raised when a client connects to the server.
        /// </summary>
        public event EventHandler<string>? ClientConnected;

        /// <summary>
        /// Event raised when a client disconnects from the server.
        /// </summary>
        public event EventHandler<string>? ClientDisconnected;

        /// <summary>
        /// Initializes a new instance of the CommunicationService class.
        /// </summary>
        /// <param name="pipeName">The name of the pipe. Defaults to "MaiksMixerPipe".</param>
        public CommunicationService(string pipeName = "MaiksMixerPipe")
        {
            _pipeName = pipeName;
            _pipeServer = new NamedPipeServer(pipeName);
            
            // Wire up events
            _pipeServer.MessageReceived += OnMessageReceived;
            _pipeServer.ClientConnected += OnClientConnected;
            _pipeServer.ClientDisconnected += OnClientDisconnected;
        }

        /// <summary>
        /// Starts the communication service.
        /// </summary>
        public void Start()
        {
            try
            {
                _pipeServer.Start();
                LogManager.Info($"Communication service started with pipe name: {_pipeName}");
            }
            catch (Exception ex)
            {
                LogManager.Error("Failed to start communication service", ex);
                throw;
            }
        }

        /// <summary>
        /// Stops the communication service.
        /// </summary>
        public void Stop()
        {
            try
            {
                _pipeServer.Stop();
                LogManager.Info("Communication service stopped");
            }
            catch (Exception ex)
            {
                LogManager.Error("Error stopping communication service", ex);
            }
        }

        /// <summary>
        /// Sends a status update to all connected clients.
        /// </summary>
        /// <param name="status">The status to send.</param>
        public async Task SendStatusUpdateAsync(StatusMessage status)
        {
            try
            {
                string? json = MessageSerializer.Serialize(status);
                if (json != null)
                {
                    await _pipeServer.SendMessageAsync(json);
                    LogManager.Info($"Status update sent: {status.Status}");
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to send status update: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Sends an event message to all connected clients.
        /// </summary>
        /// <param name="eventMessage">The event message to send.</param>
        public async Task SendEventAsync(EventMessage eventMessage)
        {
            try
            {
                string? json = MessageSerializer.Serialize(eventMessage);
                if (json != null)
                {
                    await _pipeServer.SendMessageAsync(json);
                    LogManager.Info($"Event sent: {eventMessage.EventName}");
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to send event: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Handles messages received from clients.
        /// </summary>
        private void OnMessageReceived(object? sender, string message)
        {
            try
            {
                // Try to determine the message type
                if (message.Contains("\"messageType\":") || message.Contains("\"MessageType\":"))
                {
                    // Try to deserialize as a base message to get the type
                    if (MessageSerializer.TryDeserialize<Message>(message, out var baseMessage) && baseMessage != null)
                    {
                        switch (baseMessage.MessageType)
                        {
                            case "CommandMessage":
                                HandleCommandMessage(message);
                                break;
                                
                            case "EventMessage":
                                HandleEventMessage(message);
                                break;
                                
                            default:
                                LogManager.Warn($"Received message with unknown type: {baseMessage.MessageType}");
                                break;
                        }
                    }
                    else
                    {
                        LogManager.Warn($"Failed to determine message type: {message}");
                    }
                }
                else if (message.Contains("\"command\":") || message.Contains("\"Command\":"))
                {
                    // Try to deserialize as a command message
                    HandleCommandMessage(message);
                }
                else
                {
                    LogManager.Warn($"Received message with unknown format: {message}");
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error processing message: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Handles a command message.
        /// </summary>
        /// <param name="message">The JSON message.</param>
        private void HandleCommandMessage(string message)
        {
            var command = MessageSerializer.Deserialize<CommandMessage>(message);
            
            if (command != null && !string.IsNullOrEmpty(command.Command))
            {
                LogManager.Info($"Command received: {command.Command}");
                CommandReceived?.Invoke(this, command);
            }
            else
            {
                LogManager.Warn($"Received invalid command message format: {message}");
            }
        }
        
        /// <summary>
        /// Handles an event message.
        /// </summary>
        /// <param name="message">The JSON message.</param>
        private void HandleEventMessage(string message)
        {
            var eventMessage = MessageSerializer.Deserialize<EventMessage>(message);
            
            if (eventMessage != null && !string.IsNullOrEmpty(eventMessage.EventName))
            {
                LogManager.Info($"Event received: {eventMessage.EventName}");
                EventReceived?.Invoke(this, eventMessage);
            }
            else
            {
                LogManager.Warn($"Received invalid event message format: {message}");
            }
        }

        /// <summary>
        /// Handles client connection events.
        /// </summary>
        private void OnClientConnected(object? sender, string clientId)
        {
            LogManager.Info($"Client connected: {clientId}");
            ClientConnected?.Invoke(this, clientId);
        }

        /// <summary>
        /// Handles client disconnection events.
        /// </summary>
        private void OnClientDisconnected(object? sender, string clientId)
        {
            LogManager.Info($"Client disconnected: {clientId}");
            ClientDisconnected?.Invoke(this, clientId);
        }

        /// <summary>
        /// Disposes the communication service.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the communication service.
        /// </summary>
        /// <param name="disposing">Whether the method is being called from Dispose().</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                _pipeServer.MessageReceived -= OnMessageReceived;
                _pipeServer.ClientConnected -= OnClientConnected;
                _pipeServer.ClientDisconnected -= OnClientDisconnected;
                _pipeServer.Dispose();
            }

            _isDisposed = true;
        }
    }

    /// <summary>
    /// Represents a command message sent from a client.
    /// </summary>
    public class CommandMessage
    {
        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        public string Command { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parameters for the command.
        /// </summary>
        public object? Parameters { get; set; }
    }

    /// <summary>
    /// Represents a status message sent to clients.
    /// </summary>
    public class StatusMessage
    {
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the data associated with the status.
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the status.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
