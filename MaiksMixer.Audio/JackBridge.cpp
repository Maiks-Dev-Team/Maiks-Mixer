#include "JackBridge.h"
#include <msclr/marshal_cppstd.h>
#include <string>
#include <vector>

// Include the native C++ headers
#include "cpp/emp/src/Bridge/MaiksMixerBridge.h"
#include "cpp/emp/src/JackClient/JackMixerClient.h"

using namespace System::Runtime::InteropServices;
using namespace msclr::interop;

namespace MaiksMixer {

    // Native meter data structure matching the one in C++ code
    struct NativeMeterData {
        float peak;
        float rms;
    };

    // Constructor
    JackBridge::JackBridge()
        : _nativeImpl(nullptr), _isInitialized(false), _isDisposed(false)
    {
        // Create the native implementation
        try {
            _nativeImpl = new emp::MaiksMixerBridge();
            
            // Register callbacks
            auto bridge = static_cast<emp::MaiksMixerBridge*>(_nativeImpl);
            bridge->SetServerStatusCallback(ServerStatusChangedCallback, this);
            bridge->SetMeterUpdateCallback(MeterUpdateCallback, this);
        }
        catch (const std::exception& ex) {
            throw gcnew System::Exception(gcnew String(ex.what()));
        }
    }

    // Finalizer
    JackBridge::~JackBridge()
    {
        this->!JackBridge();
    }

    // Destructor
    JackBridge::!JackBridge()
    {
        if (_isDisposed) return;

        if (_nativeImpl != nullptr) {
            try {
                auto bridge = static_cast<emp::MaiksMixerBridge*>(_nativeImpl);
                
                // Deactivate if initialized
                if (_isInitialized) {
                    bridge->Deactivate();
                }
                
                // Delete the native implementation
                delete bridge;
                _nativeImpl = nullptr;
            }
            catch (...) {
                // Ignore exceptions during cleanup
            }
        }

        _isDisposed = true;
    }

    // Initialize
    bool JackBridge::Initialize(String^ clientName)
    {
        if (_isDisposed) throw gcnew ObjectDisposedException("JackBridge");
        if (_isInitialized) return true;

        try {
            auto bridge = static_cast<emp::MaiksMixerBridge*>(_nativeImpl);
            std::string nativeClientName = marshal_as<std::string>(clientName);
            
            bool result = bridge->Initialize(nativeClientName);
            _isInitialized = result;
            return result;
        }
        catch (const std::exception& ex) {
            throw gcnew System::Exception(gcnew String(ex.what()));
        }
    }

    // Create Ports
    bool JackBridge::CreatePorts(int numInputs, int numOutputs)
    {
        if (_isDisposed) throw gcnew ObjectDisposedException("JackBridge");
        if (!_isInitialized) throw gcnew System::InvalidOperationException("JACK client is not initialized");

        try {
            auto bridge = static_cast<emp::MaiksMixerBridge*>(_nativeImpl);
            return bridge->CreatePorts(numInputs, numOutputs);
        }
        catch (const std::exception& ex) {
            throw gcnew System::Exception(gcnew String(ex.what()));
        }
    }

    // Activate
    bool JackBridge::Activate()
    {
        if (_isDisposed) throw gcnew ObjectDisposedException("JackBridge");
        if (!_isInitialized) throw gcnew System::InvalidOperationException("JACK client is not initialized");

        try {
            auto bridge = static_cast<emp::MaiksMixerBridge*>(_nativeImpl);
            return bridge->Activate();
        }
        catch (const std::exception& ex) {
            throw gcnew System::Exception(gcnew String(ex.what()));
        }
    }

    // Deactivate
    bool JackBridge::Deactivate()
    {
        if (_isDisposed) throw gcnew ObjectDisposedException("JackBridge");
        if (!_isInitialized) throw gcnew System::InvalidOperationException("JACK client is not initialized");

        try {
            auto bridge = static_cast<emp::MaiksMixerBridge*>(_nativeImpl);
            return bridge->Deactivate();
        }
        catch (const std::exception& ex) {
            throw gcnew System::Exception(gcnew String(ex.what()));
        }
    }

    // Set Channel Volume
    void JackBridge::SetChannelVolume(int channel, float volume)
    {
        if (_isDisposed) throw gcnew ObjectDisposedException("JackBridge");
        if (!_isInitialized) throw gcnew System::InvalidOperationException("JACK client is not initialized");

        try {
            auto bridge = static_cast<emp::MaiksMixerBridge*>(_nativeImpl);
            bridge->SetChannelVolume(channel, volume);
        }
        catch (const std::exception& ex) {
            throw gcnew System::Exception(gcnew String(ex.what()));
        }
    }

    // Set Channel Pan
    void JackBridge::SetChannelPan(int channel, float pan)
    {
        if (_isDisposed) throw gcnew ObjectDisposedException("JackBridge");
        if (!_isInitialized) throw gcnew System::InvalidOperationException("JACK client is not initialized");

        try {
            auto bridge = static_cast<emp::MaiksMixerBridge*>(_nativeImpl);
            bridge->SetChannelPan(channel, pan);
        }
        catch (const std::exception& ex) {
            throw gcnew System::Exception(gcnew String(ex.what()));
        }
    }

    // Set Channel Gain
    void JackBridge::SetChannelGain(int channel, float gainDB)
    {
        if (_isDisposed) throw gcnew ObjectDisposedException("JackBridge");
        if (!_isInitialized) throw gcnew System::InvalidOperationException("JACK client is not initialized");

        try {
            auto bridge = static_cast<emp::MaiksMixerBridge*>(_nativeImpl);
            bridge->SetChannelGain(channel, gainDB);
        }
        catch (const std::exception& ex) {
            throw gcnew System::Exception(gcnew String(ex.what()));
        }
    }

    // Set Channel Mute
    void JackBridge::SetChannelMute(int channel, bool mute)
    {
        if (_isDisposed) throw gcnew ObjectDisposedException("JackBridge");
        if (!_isInitialized) throw gcnew System::InvalidOperationException("JACK client is not initialized");

        try {
            auto bridge = static_cast<emp::MaiksMixerBridge*>(_nativeImpl);
            bridge->SetChannelMute(channel, mute);
        }
        catch (const std::exception& ex) {
            throw gcnew System::Exception(gcnew String(ex.what()));
        }
    }

    // Set Channel Solo
    void JackBridge::SetChannelSolo(int channel, bool solo)
    {
        if (_isDisposed) throw gcnew ObjectDisposedException("JackBridge");
        if (!_isInitialized) throw gcnew System::InvalidOperationException("JACK client is not initialized");

        try {
            auto bridge = static_cast<emp::MaiksMixerBridge*>(_nativeImpl);
            bridge->SetChannelSolo(channel, solo);
        }
        catch (const std::exception& ex) {
            throw gcnew System::Exception(gcnew String(ex.what()));
        }
    }

    // Get Sample Rate
    int JackBridge::GetSampleRate()
    {
        if (_isDisposed) throw gcnew ObjectDisposedException("JackBridge");
        if (!_isInitialized) throw gcnew System::InvalidOperationException("JACK client is not initialized");

        try {
            auto bridge = static_cast<emp::MaiksMixerBridge*>(_nativeImpl);
            return bridge->GetSampleRate();
        }
        catch (const std::exception& ex) {
            throw gcnew System::Exception(gcnew String(ex.what()));
        }
    }

    // Get Buffer Size
    int JackBridge::GetBufferSize()
    {
        if (_isDisposed) throw gcnew ObjectDisposedException("JackBridge");
        if (!_isInitialized) throw gcnew System::InvalidOperationException("JACK client is not initialized");

        try {
            auto bridge = static_cast<emp::MaiksMixerBridge*>(_nativeImpl);
            return bridge->GetBufferSize();
        }
        catch (const std::exception& ex) {
            throw gcnew System::Exception(gcnew String(ex.what()));
        }
    }

    // Get CPU Load
    float JackBridge::GetCpuLoad()
    {
        if (_isDisposed) throw gcnew ObjectDisposedException("JackBridge");
        if (!_isInitialized) throw gcnew System::InvalidOperationException("JACK client is not initialized");

        try {
            auto bridge = static_cast<emp::MaiksMixerBridge*>(_nativeImpl);
            return bridge->GetCpuLoad();
        }
        catch (const std::exception& ex) {
            throw gcnew System::Exception(gcnew String(ex.what()));
        }
    }

    // Is Server Running
    bool JackBridge::IsServerRunning()
    {
        if (_isDisposed) throw gcnew ObjectDisposedException("JackBridge");

        try {
            auto bridge = static_cast<emp::MaiksMixerBridge*>(_nativeImpl);
            return bridge->IsServerRunning();
        }
        catch (const std::exception& ex) {
            throw gcnew System::Exception(gcnew String(ex.what()));
        }
    }

    // Get Server Status
    System::Collections::Generic::Dictionary<String^, Object^>^ JackBridge::GetServerStatus()
    {
        if (_isDisposed) throw gcnew ObjectDisposedException("JackBridge");

        try {
            auto bridge = static_cast<emp::MaiksMixerBridge*>(_nativeImpl);
            auto status = bridge->GetServerStatus();

            auto result = gcnew System::Collections::Generic::Dictionary<String^, Object^>();
            result->Add("IsRunning", status.isRunning);
            result->Add("SampleRate", status.sampleRate);
            result->Add("BufferSize", status.bufferSize);
            result->Add("CpuLoad", status.cpuLoad);

            return result;
        }
        catch (const std::exception& ex) {
            throw gcnew System::Exception(gcnew String(ex.what()));
        }
    }

    // Connect Ports
    bool JackBridge::ConnectPorts(String^ sourcePort, String^ destPort)
    {
        if (_isDisposed) throw gcnew ObjectDisposedException("JackBridge");
        if (!_isInitialized) throw gcnew System::InvalidOperationException("JACK client is not initialized");

        try {
            auto bridge = static_cast<emp::MaiksMixerBridge*>(_nativeImpl);
            std::string nativeSourcePort = marshal_as<std::string>(sourcePort);
            std::string nativeDestPort = marshal_as<std::string>(destPort);
            
            return bridge->ConnectPorts(nativeSourcePort, nativeDestPort);
        }
        catch (const std::exception& ex) {
            throw gcnew System::Exception(gcnew String(ex.what()));
        }
    }

    // Disconnect Ports
    bool JackBridge::DisconnectPorts(String^ sourcePort, String^ destPort)
    {
        if (_isDisposed) throw gcnew ObjectDisposedException("JackBridge");
        if (!_isInitialized) throw gcnew System::InvalidOperationException("JACK client is not initialized");

        try {
            auto bridge = static_cast<emp::MaiksMixerBridge*>(_nativeImpl);
            std::string nativeSourcePort = marshal_as<std::string>(sourcePort);
            std::string nativeDestPort = marshal_as<std::string>(destPort);
            
            return bridge->DisconnectPorts(nativeSourcePort, nativeDestPort);
        }
        catch (const std::exception& ex) {
            throw gcnew System::Exception(gcnew String(ex.what()));
        }
    }

    // Get Port List
    array<String^>^ JackBridge::GetPortList(String^ portType, unsigned int flags)
    {
        if (_isDisposed) throw gcnew ObjectDisposedException("JackBridge");
        if (!_isInitialized) throw gcnew System::InvalidOperationException("JACK client is not initialized");

        try {
            auto bridge = static_cast<emp::MaiksMixerBridge*>(_nativeImpl);
            std::string nativePortType = marshal_as<std::string>(portType);
            
            auto nativePorts = bridge->GetPortList(nativePortType, flags);
            
            // Convert to managed array
            array<String^>^ result = gcnew array<String^>(nativePorts.size());
            for (size_t i = 0; i < nativePorts.size(); i++) {
                result[i] = gcnew String(nativePorts[i].c_str());
            }
            
            return result;
        }
        catch (const std::exception& ex) {
            throw gcnew System::Exception(gcnew String(ex.what()));
        }
    }

    // Get Ports
    System::Collections::Generic::List<System::Collections::Generic::Dictionary<String^, Object^>^>^ JackBridge::GetPorts()
    {
        if (_isDisposed) throw gcnew ObjectDisposedException("JackBridge");
        if (!_isInitialized) throw gcnew System::InvalidOperationException("JACK client is not initialized");

        try {
            auto bridge = static_cast<emp::MaiksMixerBridge*>(_nativeImpl);
            auto nativePorts = bridge->GetPorts();
            
            // Convert to managed list
            auto result = gcnew System::Collections::Generic::List<System::Collections::Generic::Dictionary<String^, Object^>^>();
            
            for (const auto& nativePort : nativePorts) {
                auto port = gcnew System::Collections::Generic::Dictionary<String^, Object^>();
                
                port->Add("Name", gcnew String(nativePort.name.c_str()));
                port->Add("Type", gcnew String(nativePort.type.c_str()));
                port->Add("Flags", (int)nativePort.flags);
                
                // Convert connections
                auto connections = gcnew System::Collections::Generic::List<String^>();
                for (const auto& conn : nativePort.connections) {
                    connections->Add(gcnew String(conn.c_str()));
                }
                
                port->Add("Connections", connections);
                port->Add("IsInput", (nativePort.flags & 1) != 0);
                port->Add("IsOutput", (nativePort.flags & 2) != 0);
                port->Add("IsPhysical", (nativePort.flags & 4) != 0);
                
                result->Add(port);
            }
            
            return result;
        }
        catch (const std::exception& ex) {
            throw gcnew System::Exception(gcnew String(ex.what()));
        }
    }

    // Callback for server status changes
    void JackBridge::ServerStatusChangedCallback(bool isRunning, void* userData)
    {
        JackBridge^ bridge = static_cast<JackBridge^>(GCHandle::FromIntPtr(IntPtr(userData)).Target);
        bridge->OnNativeServerStatusChanged(isRunning);
    }

    // Callback for meter updates
    void JackBridge::MeterUpdateCallback(int channel, const NativeMeterData& data, void* userData)
    {
        JackBridge^ bridge = static_cast<JackBridge^>(GCHandle::FromIntPtr(IntPtr(userData)).Target);
        bridge->OnNativeMeterUpdated(channel, data);
    }

    // Handle server status change
    void JackBridge::OnNativeServerStatusChanged(bool isRunning)
    {
        ServerStatusChanged(isRunning);
    }

    // Handle meter update
    void JackBridge::OnNativeMeterUpdated(int channel, const NativeMeterData& data)
    {
        MeterData^ meterData = gcnew MeterData();
        meterData->Peak = data.peak;
        meterData->Rms = data.rms;
        
        MeterUpdated(channel, meterData);
    }
}
