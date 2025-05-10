using System;
using System.Threading;
using System.Threading.Tasks;
using MaiksMixer.Core.Communication.Models;
using MaiksMixer.Core.Logging;

namespace MaiksMixer.Core.Communication
{
    /// <summary>
    /// Manages connections and provides error recovery for communication between components.
    /// </summary>
    public class ConnectionManager : IDisposable
    {
        private readonly CommunicationService _communicationService;
        private readonly CommandDispatcher _commandDispatcher;
        private readonly EventDispatcher _eventDispatcher;
        private readonly Timer _heartbeatTimer;
        private readonly Timer _reconnectTimer;
        private readonly object _lockObject = new object();
        private bool _isDisposed;
        private bool _isConnected;
        private int _reconnectAttempts;
        private int _heartbeatSequence;
        private DateTime _lastHeartbeatReceived;
        private string _clientId;
        
        /// <summary>
        /// Gets the maximum number of reconnect attempts.
        /// </summary>
        public int MaxReconnectAttempts { get; set; } = 5;
        
        /// <summary>
        /// Gets or sets the reconnect interval in milliseconds.
        /// </summary>
        public int ReconnectInterval { get; set; } = 5000;
        
        /// <summary>
        /// Gets or sets the heartbeat interval in milliseconds.
        /// </summary>
        public int HeartbeatInterval { get; set; } = 10000;
        
        /// <summary>
        /// Gets or sets the heartbeat timeout in milliseconds.
        /// </summary>
        public int HeartbeatTimeout { get; set; } = 30000;
        
        /// <summary>
        /// Event raised when the connection status changes.
        /// </summary>
        public event EventHandler<bool>? ConnectionStatusChanged;
        
        /// <summary>
        /// Event raised when a reconnect attempt is made.
        /// </summary>
        public event EventHandler<int>? ReconnectAttempted;
        
        /// <summary>
        /// Initializes a new instance of the ConnectionManager class.
        /// </summary>
        /// <param name="communicationService">The communication service.</param>
        /// <param name="commandDispatcher">The command dispatcher.</param>
        /// <param name="eventDispatcher">The event dispatcher.</param>
        public ConnectionManager(
            CommunicationService communicationService,
            CommandDispatcher commandDispatcher,
            EventDispatcher eventDispatcher)
        {
            _communicationService = communicationService ?? throw new ArgumentNullException(nameof(communicationService));
            _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
            
            // Create timers but don't start them yet
            _heartbeatTimer = new Timer(SendHeartbeat, null, Timeout.Infinite, Timeout.Infinite);
            _reconnectTimer = new Timer(AttemptReconnect, null, Timeout.Infinite, Timeout.Infinite);
            
            // Generate a client ID
            _clientId = $"Client_{Guid.NewGuid().ToString().Substring(0, 8)}";
            
            // Wire up events
            _communicationService.ClientConnected += OnClientConnected;
            _communicationService.ClientDisconnected += OnClientDisconnected;
            _communicationService.CommandReceived += OnCommandReceived;
            _communicationService.EventReceived += OnEventReceived;
            
            // Register handlers for connection-related messages
            RegisterConnectionHandlers();
        }
        
        /// <summary>
        /// Starts the connection manager.
        /// </summary>
        public void Start()
        {
            try
            {
                LogManager.Info("Starting connection manager");
                
                // Start the communication service
                _communicationService.Start();
                
                // Start the heartbeat timer
                _heartbeatTimer.Change(HeartbeatInterval, HeartbeatInterval);
                
                // Set initial connection state
                SetConnectionState(true);
            }
            catch (Exception ex)
            {
                LogManager.Error("Failed to start connection manager", ex);
                SetConnectionState(false);
                StartReconnectTimer();
            }
        }
        
        /// <summary>
        /// Stops the connection manager.
        /// </summary>
        public void Stop()
        {
            try
            {
                LogManager.Info("Stopping connection manager");
                
                // Stop the timers
                _heartbeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _reconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);
                
                // Stop the communication service
                _communicationService.Stop();
                
                // Set connection state
                SetConnectionState(false);
            }
            catch (Exception ex)
            {
                LogManager.Error("Error stopping connection manager", ex);
            }
        }
        
        /// <summary>
        /// Sends a command to the server.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<bool> SendCommandAsync(CommandMessage command)
        {
            if (!_isConnected)
            {
                LogManager.Warn("Cannot send command: Not connected");
                return false;
            }
            
            try
            {
                string? json = MessageSerializer.Serialize(command);
                if (json != null)
                {
                    await _communicationService.SendMessageAsync(json);
                    LogManager.Info($"Command sent: {command.Command}");
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to send command: {ex.Message}", ex);
                HandleConnectionError(ex);
                return false;
            }
        }
        
        /// <summary>
        /// Sends an event to all connected clients.
        /// </summary>
        /// <param name="eventMessage">The event to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<bool> SendEventAsync(EventMessage eventMessage)
        {
            if (!_isConnected)
            {
                LogManager.Warn("Cannot send event: Not connected");
                return false;
            }
            
            try
            {
                string? json = MessageSerializer.Serialize(eventMessage);
                if (json != null)
                {
                    await _communicationService.SendMessageAsync(json);
                    LogManager.Info($"Event sent: {eventMessage.EventName}");
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to send event: {ex.Message}", ex);
                HandleConnectionError(ex);
                return false;
            }
        }
        
        /// <summary>
        /// Registers handlers for connection-related messages.
        /// </summary>
        private void RegisterConnectionHandlers()
        {
            // Register handler for heartbeat messages
            _commandDispatcher.RegisterHandler("Heartbeat", async (command) =>
            {
                if (command.Parameters is HeartbeatMessage heartbeat)
                {
                    // Update last heartbeat received time
                    _lastHeartbeatReceived = DateTime.Now;
                    
                    // Send a heartbeat response
                    var response = new HeartbeatMessage(_clientId, heartbeat.SequenceNumber);
                    await SendEventAsync(new EventMessage("HeartbeatResponse", response));
                }
            });
            
            // Register handler for heartbeat responses
            _eventDispatcher.RegisterHandler("HeartbeatResponse", async (eventMessage) =>
            {
                if (eventMessage.EventData is HeartbeatMessage heartbeat)
                {
                    // Update last heartbeat received time
                    _lastHeartbeatReceived = DateTime.Now;
                    LogManager.Debug($"Received heartbeat response from {heartbeat.ClientId}, sequence: {heartbeat.SequenceNumber}");
                }
                
                await Task.CompletedTask;
            });
        }
        
        /// <summary>
        /// Sends a heartbeat message to check connection health.
        /// </summary>
        /// <param name="state">The state object.</param>
        private async void SendHeartbeat(object? state)
        {
            if (!_isConnected)
                return;
            
            try
            {
                // Check if we've exceeded the heartbeat timeout
                if (_lastHeartbeatReceived != default && (DateTime.Now - _lastHeartbeatReceived).TotalMilliseconds > HeartbeatTimeout)
                {
                    LogManager.Warn($"Heartbeat timeout exceeded: {HeartbeatTimeout}ms");
                    HandleConnectionError(new TimeoutException("Heartbeat timeout exceeded"));
                    return;
                }
                
                // Send a heartbeat message
                var heartbeat = new HeartbeatMessage(_clientId, Interlocked.Increment(ref _heartbeatSequence));
                var message = new CommandMessage("Heartbeat", heartbeat);
                
                string? json = MessageSerializer.Serialize(message);
                if (json != null)
                {
                    await _communicationService.SendMessageAsync(json);
                    LogManager.Debug($"Heartbeat sent, sequence: {heartbeat.SequenceNumber}");
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error sending heartbeat: {ex.Message}", ex);
                HandleConnectionError(ex);
            }
        }
        
        /// <summary>
        /// Attempts to reconnect to the server.
        /// </summary>
        /// <param name="state">The state object.</param>
        private void AttemptReconnect(object? state)
        {
            if (_isConnected)
            {
                // Already connected, stop reconnect timer
                _reconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _reconnectAttempts = 0;
                return;
            }
            
            if (_reconnectAttempts >= MaxReconnectAttempts)
            {
                LogManager.Error($"Maximum reconnect attempts ({MaxReconnectAttempts}) reached");
                _reconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);
                return;
            }
            
            _reconnectAttempts++;
            LogManager.Info($"Attempting to reconnect (attempt {_reconnectAttempts} of {MaxReconnectAttempts})");
            ReconnectAttempted?.Invoke(this, _reconnectAttempts);
            
            try
            {
                // Stop the communication service
                _communicationService.Stop();
                
                // Wait a moment
                Thread.Sleep(500);
                
                // Start the communication service
                _communicationService.Start();
                
                // Set connection state
                SetConnectionState(true);
                
                // Reset reconnect attempts
                _reconnectAttempts = 0;
                
                // Stop reconnect timer
                _reconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);
                
                // Start heartbeat timer
                _heartbeatTimer.Change(HeartbeatInterval, HeartbeatInterval);
                
                LogManager.Info("Reconnected successfully");
            }
            catch (Exception ex)
            {
                LogManager.Error($"Reconnect attempt failed: {ex.Message}", ex);
                SetConnectionState(false);
                
                // Continue reconnect attempts if not reached max
                if (_reconnectAttempts < MaxReconnectAttempts)
                {
                    _reconnectTimer.Change(ReconnectInterval, Timeout.Infinite);
                }
            }
        }
        
        /// <summary>
        /// Starts the reconnect timer.
        /// </summary>
        private void StartReconnectTimer()
        {
            _reconnectAttempts = 0;
            _reconnectTimer.Change(0, Timeout.Infinite);
        }
        
        /// <summary>
        /// Handles connection errors.
        /// </summary>
        /// <param name="exception">The exception that caused the error.</param>
        private void HandleConnectionError(Exception exception)
        {
            LogManager.Error($"Connection error: {exception.Message}", exception);
            
            // Set connection state
            SetConnectionState(false);
            
            // Stop heartbeat timer
            _heartbeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
            
            // Start reconnect timer
            StartReconnectTimer();
        }
        
        /// <summary>
        /// Sets the connection state and raises the ConnectionStatusChanged event if the state has changed.
        /// </summary>
        /// <param name="isConnected">Whether the connection is established.</param>
        private void SetConnectionState(bool isConnected)
        {
            lock (_lockObject)
            {
                if (_isConnected != isConnected)
                {
                    _isConnected = isConnected;
                    ConnectionStatusChanged?.Invoke(this, _isConnected);
                    
                    if (_isConnected)
                    {
                        LogManager.Info("Connection established");
                        _lastHeartbeatReceived = DateTime.Now;
                    }
                    else
                    {
                        LogManager.Info("Connection lost");
                    }
                }
            }
        }
        
        /// <summary>
        /// Handles client connection events.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="clientId">The client ID.</param>
        private void OnClientConnected(object? sender, string clientId)
        {
            LogManager.Info($"Client connected: {clientId}");
            SetConnectionState(true);
        }
        
        /// <summary>
        /// Handles client disconnection events.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="clientId">The client ID.</param>
        private void OnClientDisconnected(object? sender, string clientId)
        {
            LogManager.Info($"Client disconnected: {clientId}");
            
            // Don't set connection state to false here, as we might still have other clients connected
            // The heartbeat mechanism will detect if we've lost connection to all clients
        }
        
        /// <summary>
        /// Handles command received events.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="command">The command.</param>
        private async void OnCommandReceived(object? sender, CommandMessage command)
        {
            try
            {
                // Dispatch the command to its handler
                await _commandDispatcher.DispatchCommandAsync(command);
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error dispatching command: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Handles event received events.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventMessage">The event.</param>
        private async void OnEventReceived(object? sender, EventMessage eventMessage)
        {
            try
            {
                // Dispatch the event to its handlers
                await _eventDispatcher.DispatchEventAsync(eventMessage);
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error dispatching event: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Disposes the connection manager.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Disposes the connection manager.
        /// </summary>
        /// <param name="disposing">Whether the method is being called from Dispose().</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;
            
            if (disposing)
            {
                // Stop the timers
                _heartbeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _reconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);
                
                // Dispose the timers
                _heartbeatTimer.Dispose();
                _reconnectTimer.Dispose();
                
                // Unwire events
                _communicationService.ClientConnected -= OnClientConnected;
                _communicationService.ClientDisconnected -= OnClientDisconnected;
                _communicationService.CommandReceived -= OnCommandReceived;
                _communicationService.EventReceived -= OnEventReceived;
            }
            
            _isDisposed = true;
        }
    }
}
