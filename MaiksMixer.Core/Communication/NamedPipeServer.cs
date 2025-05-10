using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MaiksMixer.Core.Logging;

namespace MaiksMixer.Core.Communication
{
    /// <summary>
    /// Provides named pipe server functionality for inter-process communication.
    /// </summary>
    public class NamedPipeServer : IDisposable
    {
        private const string DefaultPipeName = "MaiksMixerPipe";
        private readonly string _pipeName;
        private readonly int _maxConnections;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private Task? _serverTask;
        private bool _isRunning;
        private bool _isDisposed;

        /// <summary>
        /// Event raised when a message is received from a client.
        /// </summary>
        public event EventHandler<string>? MessageReceived;

        /// <summary>
        /// Event raised when a client connects to the pipe.
        /// </summary>
        public event EventHandler<string>? ClientConnected;

        /// <summary>
        /// Event raised when a client disconnects from the pipe.
        /// </summary>
        public event EventHandler<string>? ClientDisconnected;

        /// <summary>
        /// Initializes a new instance of the NamedPipeServer class.
        /// </summary>
        /// <param name="pipeName">The name of the pipe. Defaults to "MaiksMixerPipe".</param>
        /// <param name="maxConnections">The maximum number of connections the server will accept. Defaults to 10.</param>
        public NamedPipeServer(string pipeName = DefaultPipeName, int maxConnections = 10)
        {
            _pipeName = pipeName;
            _maxConnections = maxConnections;
            _cancellationTokenSource = new CancellationTokenSource();
            _isRunning = false;
        }

        /// <summary>
        /// Starts the named pipe server.
        /// </summary>
        public void Start()
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _serverTask = Task.Run(RunServerAsync);
            LogManager.Info($"Named pipe server started with pipe name: {_pipeName}");
        }

        /// <summary>
        /// Stops the named pipe server.
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
                return;

            _isRunning = false;
            _cancellationTokenSource.Cancel();
            _serverTask?.Wait();
            LogManager.Info("Named pipe server stopped");
        }

        /// <summary>
        /// Sends a message to all connected clients.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public async Task SendMessageAsync(string message)
        {
            try
            {
                using var clientPipe = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out);
                await clientPipe.ConnectAsync(1000); // 1 second timeout
                
                using var writer = new StreamWriter(clientPipe);
                await writer.WriteLineAsync(message);
                await writer.FlushAsync();
                
                LogManager.Info($"Message sent to pipe: {message}");
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to send message to pipe: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Runs the server loop to accept and handle client connections.
        /// </summary>
        private async Task RunServerAsync()
        {
            while (_isRunning && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    // Create a new pipe instance for each client
                    using var serverPipe = new NamedPipeServerStream(
                        _pipeName,
                        PipeDirection.InOut,
                        _maxConnections,
                        PipeTransmissionMode.Message,
                        PipeOptions.Asynchronous);

                    // Wait for a client to connect
                    await serverPipe.WaitForConnectionAsync(_cancellationTokenSource.Token);
                    
                    // Get client info
                    string clientId = $"Client-{Guid.NewGuid()}";
                    LogManager.Info($"Client connected: {clientId}");
                    ClientConnected?.Invoke(this, clientId);

                    // Handle client communication in a separate task
                    _ = Task.Run(() => HandleClientAsync(serverPipe, clientId), _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    // Server is shutting down
                    break;
                }
                catch (Exception ex)
                {
                    LogManager.Error($"Error in named pipe server: {ex.Message}", ex);
                    
                    // Brief pause before retrying to prevent tight loop in case of persistent errors
                    await Task.Delay(1000, _cancellationTokenSource.Token);
                }
            }
        }

        /// <summary>
        /// Handles communication with a connected client.
        /// </summary>
        /// <param name="pipe">The pipe connected to the client.</param>
        /// <param name="clientId">The ID of the connected client.</param>
        private async Task HandleClientAsync(NamedPipeServerStream pipe, string clientId)
        {
            try
            {
                using var reader = new StreamReader(pipe, Encoding.UTF8, false, 1024, true);
                
                while (pipe.IsConnected && !_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    // Read message from client
                    string? message = await reader.ReadLineAsync();
                    
                    if (message == null)
                        break;
                    
                    LogManager.Info($"Message received from {clientId}: {message}");
                    MessageReceived?.Invoke(this, message);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error handling client {clientId}: {ex.Message}", ex);
            }
            finally
            {
                // Clean up
                if (pipe.IsConnected)
                {
                    pipe.Disconnect();
                }
                
                LogManager.Info($"Client disconnected: {clientId}");
                ClientDisconnected?.Invoke(this, clientId);
            }
        }

        /// <summary>
        /// Disposes the named pipe server.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the named pipe server.
        /// </summary>
        /// <param name="disposing">Whether the method is being called from Dispose().</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                Stop();
                _cancellationTokenSource.Dispose();
            }

            _isDisposed = true;
        }
    }
}
