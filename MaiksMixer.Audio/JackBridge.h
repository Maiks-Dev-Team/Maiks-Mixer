#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;

namespace MaiksMixer {

    // Forward declarations for native types
    struct NativeMeterData;

    /// <summary>
    /// Represents meter data for a channel
    /// </summary>
    public ref class MeterData
    {
    public:
        /// <summary>
        /// Peak level (0.0 - 1.0)
        /// </summary>
        property float Peak;

        /// <summary>
        /// RMS level (0.0 - 1.0)
        /// </summary>
        property float Rms;
    };

    /// <summary>
    /// Bridge class for interacting with the JACK audio system
    /// </summary>
    public ref class JackBridge
    {
    private:
        // Pointer to the native implementation
        void* _nativeImpl;
        bool _isInitialized;
        bool _isDisposed;

    public:
        /// <summary>
        /// Event raised when the JACK server status changes
        /// </summary>
        event Action<bool>^ ServerStatusChanged;

        /// <summary>
        /// Event raised when meter data is updated for a channel
        /// </summary>
        event Action<int, MeterData^>^ MeterUpdated;

        /// <summary>
        /// Creates a new instance of the JackBridge
        /// </summary>
        JackBridge();

        /// <summary>
        /// Finalizer
        /// </summary>
        ~JackBridge();

        /// <summary>
        /// Destructor
        /// </summary>
        !JackBridge();

        /// <summary>
        /// Initializes the JACK client with the specified name
        /// </summary>
        /// <param name="clientName">Name to register with the JACK server</param>
        /// <returns>True if initialization was successful, false otherwise</returns>
        bool Initialize(String^ clientName);

        /// <summary>
        /// Creates input and output ports for the JACK client
        /// </summary>
        /// <param name="numInputs">Number of input ports to create</param>
        /// <param name="numOutputs">Number of output ports to create</param>
        /// <returns>True if port creation was successful, false otherwise</returns>
        bool CreatePorts(int numInputs, int numOutputs);

        /// <summary>
        /// Activates the JACK client
        /// </summary>
        /// <returns>True if activation was successful, false otherwise</returns>
        bool Activate();

        /// <summary>
        /// Deactivates the JACK client
        /// </summary>
        /// <returns>True if deactivation was successful, false otherwise</returns>
        bool Deactivate();

        /// <summary>
        /// Sets the volume for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="volume">Volume value (0.0 - 1.0)</param>
        void SetChannelVolume(int channel, float volume);

        /// <summary>
        /// Sets the pan for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="pan">Pan value (0.0 left, 0.5 center, 1.0 right)</param>
        void SetChannelPan(int channel, float pan);

        /// <summary>
        /// Sets the gain for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="gainDB">Gain value in dB</param>
        void SetChannelGain(int channel, float gainDB);

        /// <summary>
        /// Sets the mute state for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="mute">Mute state</param>
        void SetChannelMute(int channel, bool mute);

        /// <summary>
        /// Sets the solo state for a channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <param name="solo">Solo state</param>
        void SetChannelSolo(int channel, bool solo);

        /// <summary>
        /// Gets the sample rate from the JACK server
        /// </summary>
        /// <returns>Sample rate in Hz</returns>
        int GetSampleRate();

        /// <summary>
        /// Gets the buffer size from the JACK server
        /// </summary>
        /// <returns>Buffer size in frames</returns>
        int GetBufferSize();

        /// <summary>
        /// Gets the CPU load from the JACK server
        /// </summary>
        /// <returns>CPU load as a percentage (0.0 - 100.0)</returns>
        float GetCpuLoad();

        /// <summary>
        /// Checks if the JACK server is running
        /// </summary>
        /// <returns>True if running, false otherwise</returns>
        bool IsServerRunning();

        /// <summary>
        /// Gets the current JACK server status
        /// </summary>
        /// <returns>JackServerStatus object with server information</returns>
        System::Collections::Generic::Dictionary<String^, Object^>^ GetServerStatus();

        /// <summary>
        /// Connects two JACK ports
        /// </summary>
        /// <param name="sourcePort">Source port name</param>
        /// <param name="destPort">Destination port name</param>
        /// <returns>True if connection was successful, false otherwise</returns>
        bool ConnectPorts(String^ sourcePort, String^ destPort);

        /// <summary>
        /// Disconnects two JACK ports
        /// </summary>
        /// <param name="sourcePort">Source port name</param>
        /// <param name="destPort">Destination port name</param>
        /// <returns>True if disconnection was successful, false otherwise</returns>
        bool DisconnectPorts(String^ sourcePort, String^ destPort);

        /// <summary>
        /// Gets a list of all available JACK ports
        /// </summary>
        /// <param name="portType">Port type filter (e.g., "audio")</param>
        /// <param name="flags">Port flags filter</param>
        /// <returns>List of port names</returns>
        array<String^>^ GetPortList(String^ portType, unsigned int flags);

        /// <summary>
        /// Gets a list of all available JACK ports
        /// </summary>
        /// <returns>List of JackPort objects</returns>
        System::Collections::Generic::List<System::Collections::Generic::Dictionary<String^, Object^>^>^ GetPorts();

    private:
        // Callback methods for native events
        void OnNativeServerStatusChanged(bool isRunning);
        void OnNativeMeterUpdated(int channel, const NativeMeterData& data);

        // Static callback functions that will be passed to native code
        static void ServerStatusChangedCallback(bool isRunning, void* userData);
        static void MeterUpdateCallback(int channel, const NativeMeterData& data, void* userData);
    };
}
