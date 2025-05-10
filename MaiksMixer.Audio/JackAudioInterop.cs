using System;
using System.Runtime.InteropServices;

namespace MaiksMixer.Audio
{
    /// <summary>
    /// Provides native interop for JACK audio functionality
    /// </summary>
    public static class JackAudioInterop
    {
        // The name of the native DLL
        private const string DllName = "MaiksMixerNative";

        #region Native Methods

        /// <summary>
        /// Initializes the JACK client
        /// </summary>
        /// <param name="clientName">Name of the JACK client</param>
        /// <returns>True if successful, false otherwise</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Initialize([MarshalAs(UnmanagedType.LPStr)] string clientName);

        /// <summary>
        /// Creates input and output ports
        /// </summary>
        /// <param name="numInputs">Number of input ports to create</param>
        /// <param name="numOutputs">Number of output ports to create</param>
        /// <returns>True if successful, false otherwise</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool CreatePorts(int numInputs, int numOutputs);

        /// <summary>
        /// Activates the JACK client
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Activate();

        /// <summary>
        /// Deactivates the JACK client
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Deactivate();

        /// <summary>
        /// Sets the volume for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="volume">Volume value (0.0 - 1.0)</param>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetChannelVolume(int channel, float volume);

        /// <summary>
        /// Sets the pan for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="pan">Pan value (0.0 left, 0.5 center, 1.0 right)</param>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetChannelPan(int channel, float pan);

        /// <summary>
        /// Sets the gain for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="gainDB">Gain value in dB</param>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetChannelGain(int channel, float gainDB);

        /// <summary>
        /// Sets the mute state for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="mute">Mute state</param>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetChannelMute(int channel, [MarshalAs(UnmanagedType.I1)] bool mute);

        /// <summary>
        /// Sets the solo state for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="solo">Solo state</param>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetChannelSolo(int channel, [MarshalAs(UnmanagedType.I1)] bool solo);

        /// <summary>
        /// Gets the sample rate from the JACK server
        /// </summary>
        /// <returns>Sample rate in Hz</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetSampleRate();

        /// <summary>
        /// Gets the buffer size from the JACK server
        /// </summary>
        /// <returns>Buffer size in frames</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetBufferSize();

        /// <summary>
        /// Gets the CPU load from the JACK server
        /// </summary>
        /// <returns>CPU load as a percentage (0.0 - 100.0)</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float GetCpuLoad();

        /// <summary>
        /// Checks if the JACK server is running
        /// </summary>
        /// <returns>True if running, false otherwise</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsServerRunning();

        /// <summary>
        /// Connects two JACK ports
        /// </summary>
        /// <param name="sourcePort">Source port name</param>
        /// <param name="destPort">Destination port name</param>
        /// <returns>True if connection was successful, false otherwise</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool ConnectPorts(
            [MarshalAs(UnmanagedType.LPStr)] string sourcePort, 
            [MarshalAs(UnmanagedType.LPStr)] string destPort);

        /// <summary>
        /// Disconnects two JACK ports
        /// </summary>
        /// <param name="sourcePort">Source port name</param>
        /// <param name="destPort">Destination port name</param>
        /// <returns>True if disconnection was successful, false otherwise</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool DisconnectPorts(
            [MarshalAs(UnmanagedType.LPStr)] string sourcePort, 
            [MarshalAs(UnmanagedType.LPStr)] string destPort);

        #endregion

        #region Callback Delegates and Events

        // Define delegate types for callbacks from native code
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ServerStatusChangedCallback([MarshalAs(UnmanagedType.I1)] bool isRunning);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void MeterUpdateCallback(int channel, float peak, float rms);

        // Events that will be raised when callbacks are received
        public static event EventHandler<bool>? ServerStatusChanged;
        public static event EventHandler<(int Channel, float Peak, float RMS)>? MeterUpdated;

        // Callback instances that will be passed to native code
        private static readonly ServerStatusChangedCallback _serverStatusChangedCallback = OnServerStatusChanged;
        private static readonly MeterUpdateCallback _meterUpdateCallback = OnMeterUpdate;

        // Methods to register callbacks with native code
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void RegisterServerStatusCallback(ServerStatusChangedCallback callback);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void RegisterMeterUpdateCallback(MeterUpdateCallback callback);

        // Static constructor to register callbacks when the class is first used
        static JackAudioInterop()
        {
            try
            {
                RegisterServerStatusCallback(_serverStatusChangedCallback);
                RegisterMeterUpdateCallback(_meterUpdateCallback);
            }
            catch (Exception ex)
            {
                // Log the exception - in a real app you'd use proper logging
                Console.WriteLine($"Error registering callbacks: {ex.Message}");
            }
        }

        // Callback methods that will be called from native code
        private static void OnServerStatusChanged([MarshalAs(UnmanagedType.I1)] bool isRunning)
        {
            ServerStatusChanged?.Invoke(null, isRunning);
        }

        private static void OnMeterUpdate(int channel, float peak, float rms)
        {
            MeterUpdated?.Invoke(null, (channel, peak, rms));
        }

        #endregion
    }
}
