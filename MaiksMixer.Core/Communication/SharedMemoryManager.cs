using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MaiksMixer.Core.Logging;

namespace MaiksMixer.Core.Communication
{
    /// <summary>
    /// Manages memory-mapped file communication for efficient data sharing between processes.
    /// </summary>
    public class SharedMemoryManager : IDisposable
    {
        private const string DefaultMemoryName = "MaiksMixerSharedMemory";
        private const int DefaultBufferSize = 4096; // 4 KB
        
        private readonly string _memoryName;
        private readonly int _bufferSize;
        private MemoryMappedFile? _mappedFile;
        private Mutex? _mutex;
        private bool _isDisposed;
        
        /// <summary>
        /// Initializes a new instance of the SharedMemoryManager class.
        /// </summary>
        /// <param name="memoryName">The name of the memory-mapped file. Defaults to "MaiksMixerSharedMemory".</param>
        /// <param name="bufferSize">The size of the buffer in bytes. Defaults to 4096 (4 KB).</param>
        public SharedMemoryManager(string memoryName = DefaultMemoryName, int bufferSize = DefaultBufferSize)
        {
            _memoryName = memoryName;
            _bufferSize = bufferSize;
        }
        
        /// <summary>
        /// Initializes the shared memory.
        /// </summary>
        public void Initialize()
        {
            try
            {
                // Check if running on Windows
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    throw new PlatformNotSupportedException("Memory-mapped files are only supported on Windows.");
                }
                
                // Create or open the memory-mapped file
                _mappedFile = MemoryMappedFile.CreateOrOpen(_memoryName, _bufferSize);
                
                // Create or open the mutex for synchronization
                bool createdNew;
                _mutex = new Mutex(false, $"{_memoryName}_Mutex", out createdNew);
                
                LogManager.Info($"Shared memory initialized: {_memoryName}, Size: {_bufferSize} bytes");
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to initialize shared memory: {ex.Message}", ex);
                throw;
            }
        }
        
        /// <summary>
        /// Writes data to the shared memory.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <returns>True if the data was written successfully; otherwise, false.</returns>
        public bool WriteData(string data)
        {
            if (_mappedFile == null || _mutex == null)
            {
                LogManager.Error("Cannot write data: Shared memory not initialized");
                return false;
            }
            
            try
            {
                // Acquire the mutex to ensure exclusive access
                _mutex.WaitOne();
                
                try
                {
                    // Convert the data to bytes
                    byte[] buffer = Encoding.UTF8.GetBytes(data);
                    
                    // Check if the data fits in the buffer
                    if (buffer.Length > _bufferSize - 4)
                    {
                        LogManager.Error($"Data too large for shared memory buffer: {buffer.Length} bytes");
                        return false;
                    }
                    
                    // Create a view accessor
                    using (var accessor = _mappedFile.CreateViewAccessor())
                    {
                        // Write the data length as a 4-byte integer
                        accessor.Write(0, buffer.Length);
                        
                        // Write the data
                        accessor.WriteArray(4, buffer, 0, buffer.Length);
                    }
                    
                    LogManager.Info($"Data written to shared memory: {data.Length} characters");
                    return true;
                }
                finally
                {
                    // Release the mutex
                    _mutex.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to write data to shared memory: {ex.Message}", ex);
                return false;
            }
        }
        
        /// <summary>
        /// Reads data from the shared memory.
        /// </summary>
        /// <returns>The data read from the shared memory, or null if an error occurred.</returns>
        public string? ReadData()
        {
            if (_mappedFile == null || _mutex == null)
            {
                LogManager.Error("Cannot read data: Shared memory not initialized");
                return null;
            }
            
            try
            {
                // Acquire the mutex to ensure exclusive access
                _mutex.WaitOne();
                
                try
                {
                    // Create a view accessor
                    using (var accessor = _mappedFile.CreateViewAccessor())
                    {
                        // Read the data length
                        int dataLength = accessor.ReadInt32(0);
                        
                        // Check if the data length is valid
                        if (dataLength <= 0 || dataLength > _bufferSize - 4)
                        {
                            LogManager.Warn($"Invalid data length in shared memory: {dataLength}");
                            return null;
                        }
                        
                        // Read the data
                        byte[] buffer = new byte[dataLength];
                        accessor.ReadArray(4, buffer, 0, dataLength);
                        
                        // Convert the data to a string
                        string data = Encoding.UTF8.GetString(buffer);
                        
                        LogManager.Info($"Data read from shared memory: {data.Length} characters");
                        return data;
                    }
                }
                finally
                {
                    // Release the mutex
                    _mutex.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to read data from shared memory: {ex.Message}", ex);
                return null;
            }
        }
        
        /// <summary>
        /// Writes data to the shared memory asynchronously.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the data was written successfully.</returns>
        public Task<bool> WriteDataAsync(string data)
        {
            return Task.Run(() => WriteData(data));
        }
        
        /// <summary>
        /// Reads data from the shared memory asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the data read from the shared memory, or null if an error occurred.</returns>
        public Task<string?> ReadDataAsync()
        {
            return Task.Run(() => ReadData());
        }
        
        /// <summary>
        /// Disposes the shared memory manager.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Disposes the shared memory manager.
        /// </summary>
        /// <param name="disposing">Whether the method is being called from Dispose().</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;
            
            if (disposing)
            {
                // Dispose managed resources
                _mappedFile?.Dispose();
                _mutex?.Dispose();
                
                LogManager.Info("Shared memory disposed");
            }
            
            _isDisposed = true;
        }
    }
}
