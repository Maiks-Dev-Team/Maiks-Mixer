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
    /// Provides named pipe client functionality for inter-process communication.
    /// </summary>
    public class NamedPipeClient : IDisposable
    {
        private const string DefaultPipeName = "MaiksMixerPipe";
        private readonly string _pipeName;
        private readonly string _serverName;
        private NamedPipeClientStream? _pipeClient;
        private StreamWriter? _writer;
        private StreamReader? _reader;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _readTask;
        private bool _isConnected;
        private bool _isDisposed;

        /// <summary>
        /// Event raised when a message is received from the server.
        /// </summary>
        public event EventHandler<string>? MessageReceived;

        /// <summary>
        /// Event raised when the client connects to the server.
        /// </summary>
        public event EventHandler? Connected;

        /// <summary>
        /// Event raised when the client disconnects from the server.
        /// </summary>
        public event EventHandler? Disconnected;

        /// <summary>
        /// Gets a value indicating whether the client is connected to the server.
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// Initializes a new instance of the NamedPipeClient class.
        /// </summary>
        /// <param name="serverName">The name of the server. Defaults to ".".</param>
        /// <param name="pipeName">The name of the pipe. Defaults to "MaiksMixerPipe".</param>
        public NamedPipeClient(string serverName = ".", string pipeName = DefaultPipeName)
        {
            _serverName = serverName;
            _pipeName = pipeName;
            _isConnected = false;
        }

        /// <summary>
        /// Connects to the named pipe server.
        /// </summary>
        /// <param name="timeout">The timeout in milliseconds. Defaults to 5000 (5 seconds).</param>
        /// <returns>True if the connection was successful; otherwise, false.</returns>
        public async Task<bool> ConnectAsync(int timeout = 5000)
        {
            if (_isConnected)
                return true;

            try
            {
                // Create the pipe client
                _pipeClient = new NamedPipeClientStream(_serverName, _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                
                // Connect to the server
                await _pipeClient.ConnectAsync(timeout);
                
                // Create the reader and writer
                _writer = new StreamWriter(_pipeClient) { AutoFlush = true };
                _reader = new StreamReader(_pipeClient, Encoding.UTF8, false, 1024, true);
                
                // Start the read loop
                _cancellationTokenSource = new CancellationTokenSource();
                _readTask = Task.Run(ReadLoopAsync);
                
                _isConnected = true;
                LogManager.Info($"Connected to named pipe server: {_serverName}\\{_pipeName}");
                Connected?.Invoke(this, EventArgs.Empty);
                
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to connect to named pipe server: {ex.Message}", ex);
                await DisconnectAsync();
                return false;
            }
        }

        /// <summary>
        /// Disconnects from the named pipe server.
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (!_isConnected)
                return;

            try
            {
                _isConnected = false;
                
                // Cancel the read loop
                _cancellationTokenSource?.Cancel();
                
                if (_readTask != null)
                {
                    await Task.WhenAny(_readTask, Task.Delay(1000));
                }
                
                // Clean up resources
                _writer?.Dispose();
                _reader?.Dispose();
                _pipeClient?.Dispose();
                
                _writer = null;
                _reader = null;
                _pipeClient = null;
                
                LogManager.Info("Disconnected from named pipe server");
                Disconnected?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error disconnecting from named pipe server: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>True if the message was sent successfully; otherwise, false.</returns>
        public async Task<bool> SendMessageAsync(string message)
        {
            if (!_isConnected || _writer == null)
            {
                LogManager.Error("Cannot send message: Not connected to server");
                return false;
            }

            try
            {
                await _writer.WriteLineAsync(message);
                await _writer.FlushAsync();
                
                LogManager.Info($"Message sent: {message}");
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to send message: {ex.Message}", ex);
                await DisconnectAsync();
                return false;
            }
        }

        /// <summary>
        /// Continuously reads messages from the server.
        /// </summary>
        private async Task ReadLoopAsync()
        {
            if (_reader == null || _cancellationTokenSource == null)
                return;

            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    string? message = await _reader.ReadLineAsync();
                    
                    if (message == null)
                    {
                        // End of stream, server disconnected
                        break;
                    }
                    
                    LogManager.Info($"Message received: {message}");
                    MessageReceived?.Invoke(this, message);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation, do nothing
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error reading from pipe: {ex.Message}", ex);
            }
            finally
            {
                // Ensure we're disconnected
                if (_isConnected)
                {
                    await DisconnectAsync();
                }
            }
        }

        /// <summary>
        /// Disposes the named pipe client.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the named pipe client.
        /// </summary>
        /// <param name="disposing">Whether the method is being called from Dispose().</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                DisconnectAsync().Wait();
                _cancellationTokenSource?.Dispose();
            }

            _isDisposed = true;
        }
    }
}
