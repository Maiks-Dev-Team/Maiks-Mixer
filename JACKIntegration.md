# JACK Audio Integration for MaiksMixer

## Overview

This document outlines the integration of JACK Audio Connection Kit with the MaiksMixer application. JACK will serve as the core audio routing and processing engine, providing low-latency, professional-grade audio capabilities.

## Installation and Configuration

### Installation Details

- JACK2 version 1.9.21 is installed at `C:\Program Files\JACK2`
- Available drivers: PortAudio, WinMME, Dummy

### Working Configuration

The following command successfully starts the JACK server with proper audio device configuration:

```bash
"C:\Program Files\JACK2\jackd.exe" -d portaudio -P "Windows WDM-KS::Speakers (USB PnP Audio Device)" -C "Windows WDM-KS::Microphone (USB PnP Audio Device)" -r 48000 -p 1024
```

#### Configuration Parameters

- Driver: PortAudio (`-d portaudio`)
- Playback Device: `Windows WDM-KS::Speakers (USB PnP Audio Device)` (`-P`)
- Capture Device: `Windows WDM-KS::Microphone (USB PnP Audio Device)` (`-C`)
- Sample Rate: 48000 Hz (`-r 48000`)
- Buffer Size: 1024 frames (`-p 1024`)

### Detected ASIO Drivers

- ASIO4ALL v2
- JackRouter
- Realtek ASIO
- Synchronous Audio Router
- Voicemeeter AUX Virtual ASIO
- Voicemeeter Insert Virtual ASIO
- Voicemeeter Potato Insert Virtual ASIO
- Voicemeeter VAIO3 Virtual ASIO
- Voicemeeter Virtual ASIO

### Troubleshooting

If JACK fails to start, check for these common issues:

1. Another JACK server might already be running (use `Stop-Process -Name 'jackd' -Force` to stop it)
2. Audio device might be in use by another application
3. VoiceMeeter might be causing conflicts with the selected audio devices
4. Try different buffer sizes if you experience audio glitches (512, 1024, or 2048)

### Starting JACK Automatically

Consider creating a batch file with the working configuration command to easily start JACK, or use QjackCtl to save these settings for quick access.

## JACK Architecture

### Components

1. **JACK Server**
   - Core audio processing daemon
   - Manages audio connections between clients
   - Handles buffer sizes, sample rates, and audio processing

2. **JACK Clients**
   - Applications that connect to the JACK server
   - Can create input and output ports
   - Process audio in real-time through callbacks

3. **JackRouter**
   - ASIO driver for Windows
   - Routes audio between Windows applications and JACK
   - Appears as a standard audio device to Windows applications

4. **JACK Control**
   - GUI for configuring and monitoring JACK server
   - Can be replaced by our custom UI

## Integration Strategy

### C++ JACK Client (B:\Projects\C++\MaiksMixer)

1. **Core JACK Client Implementation**
   - Create a JACK client that registers with the JACK server
   - Register input and output ports for routing
   - Implement the JACK process callback for audio processing
   - Handle JACK server events (shutdown, xrun, etc.)

2. **Audio Processing**
   - Implement volume, pan, and gain controls within the JACK process callback
   - Ensure sample-accurate processing
   - Implement metering and analysis

3. **Port Management**
   - Create, connect, and disconnect ports dynamically
   - Monitor port connections and status
   - Handle port registration/unregistration events

### C# WPF UI Integration (B:\Projects\C#\MaiksMixer)

1. **C++/CLI Bridge**
   - Create a managed wrapper around the JACK C++ client
   - Expose JACK functionality to the C# UI
   - Handle marshaling of data between managed and unmanaged code

2. **JACK Server Management**
   - Start/stop the JACK server from the UI
   - Configure JACK server parameters (sample rate, buffer size, etc.)
   - Monitor JACK server status and health

3. **UI Components**
   - Visualize JACK connections as a routing matrix
   - Display real-time audio levels from JACK ports
   - Provide controls for JACK client parameters

## JACK-Specific Commands

### Get JACK Status

```json
{
  "messageType": "Command",
  "messageId": "uuid-string",
  "timestamp": "2025-05-09T20:08:46+02:00",
  "payload": {
    "command": "GetJackStatus"
  }
}
```

#### Get JACK Status Response

```json
{
  "messageType": "Response",
  "messageId": "uuid-string",
  "inResponseTo": "original-message-id",
  "timestamp": "2025-05-09T20:08:46+02:00",
  "payload": {
    "success": true,
    "status": {
      "running": true,
      "sampleRate": 48000,
      "bufferSize": 256,
      "cpuLoad": 0.12,
      "xruns": 0,
      "latency": 5.3
    }
  }
}
```

### List JACK Ports

```json
{
  "messageType": "Command",
  "messageId": "uuid-string",
  "timestamp": "2025-05-09T20:08:46+02:00",
  "payload": {
    "command": "ListJackPorts",
    "portType": "audio",
    "direction": "input",
    "flags": ["physical", "terminal"]
  }
}
```

#### List JACK Ports Response

```json
{
  "messageType": "Response",
  "messageId": "uuid-string",
  "inResponseTo": "original-message-id",
  "timestamp": "2025-05-09T20:08:46+02:00",
  "payload": {
    "success": true,
    "ports": [
      {
        "name": "system:capture_1",
        "flags": ["physical", "terminal", "input"],
        "type": "audio",
        "connections": ["MaiksMixer:input_1"]
      },
      {
        "name": "system:capture_2",
        "flags": ["physical", "terminal", "input"],
        "type": "audio",
        "connections": ["MaiksMixer:input_2"]
      }
    ]
  }
}
```

### Connect JACK Ports

```json
{
  "messageType": "Command",
  "messageId": "uuid-string",
  "timestamp": "2025-05-09T20:08:46+02:00",
  "payload": {
    "command": "ConnectJackPorts",
    "sourcePort": "system:capture_1",
    "destinationPort": "MaiksMixer:input_1"
  }
}
```

### Disconnect JACK Ports

```json
{
  "messageType": "Command",
  "messageId": "uuid-string",
  "timestamp": "2025-05-09T20:08:46+02:00",
  "payload": {
    "command": "DisconnectJackPorts",
    "sourcePort": "system:capture_1",
    "destinationPort": "MaiksMixer:input_1"
  }
}
```

### Start JACK Server

```json
{
  "messageType": "Command",
  "messageId": "uuid-string",
  "timestamp": "2025-05-09T20:08:46+02:00",
  "payload": {
    "command": "StartJackServer",
    "sampleRate": 48000,
    "bufferSize": 256,
    "periods": 2,
    "priority": "high"
  }
}
```

### Stop JACK Server

```json
{
  "messageType": "Command",
  "messageId": "uuid-string",
  "timestamp": "2025-05-09T20:08:46+02:00",
  "payload": {
    "command": "StopJackServer"
  }
}
```

### Implementation Details C++ JACK Client Example

```cpp
#include <jack/jack.h>
#include <iostream>
#include <vector>
#include <string>
#include <mutex>

class JackMixerClient {
private:
    jack_client_t* client;
    std::vector<jack_port_t*> inputPorts;
    std::vector<jack_port_t*> outputPorts;
    std::mutex portMutex;
    
    // Audio processing parameters
    struct ChannelParams {
        float volume;
        float pan;
        float gain;
        bool mute;
        bool solo;
    };
    std::vector<ChannelParams> channelParams;
    
    // Metering data
    struct MeterData {
        float peak;
        float rms;
    };
    std::vector<MeterData> meterData;
    
    static int processCallback(jack_nframes_t nframes, void* arg) {
        JackMixerClient* client = static_cast<JackMixerClient*>(arg);
        return client->process(nframes);
    }
    
    int process(jack_nframes_t nframes) {
        // Get input/output buffers
        std::vector<float*> inBuffers;
        std::vector<float*> outBuffers;
        
        for (auto port : inputPorts) {
            inBuffers.push_back(static_cast<float*>(jack_port_get_buffer(port, nframes)));
        }
        
        for (auto port : outputPorts) {
            outBuffers.push_back(static_cast<float*>(jack_port_get_buffer(port, nframes)));
        }
        
        // Clear output buffers
        for (auto buffer : outBuffers) {
            memset(buffer, 0, sizeof(float) * nframes);
        }
        
        // Process audio (apply volume, pan, etc.)
        for (size_t i = 0; i < inputPorts.size(); i++) {
            if (i >= channelParams.size()) continue;
            
            const auto& params = channelParams[i];
            if (params.mute) continue;
            
            // Calculate levels for metering
            float peak = 0.0f;
            float sumSquares = 0.0f;
            
            for (jack_nframes_t j = 0; j < nframes; j++) {
                float sample = inBuffers[i][j] * params.gain * params.volume;
                
                // Update metering
                float absSample = std::abs(sample);
                peak = std::max(peak, absSample);
                sumSquares += sample * sample;
                
                // Apply panning (simple linear pan for now)
                if (outBuffers.size() >= 2) {
                    outBuffers[0][j] += sample * (1.0f - params.pan);
                    outBuffers[1][j] += sample * params.pan;
                } else if (outBuffers.size() == 1) {
                    outBuffers[0][j] += sample;
                }
            }
            
            // Update meter data
            if (i < meterData.size()) {
                meterData[i].peak = peak;
                meterData[i].rms = std::sqrt(sumSquares / nframes);
            }
        }
        
        return 0;
    }
    
    static void jackShutdownCallback(void* arg) {
        JackMixerClient* client = static_cast<JackMixerClient*>(arg);
        client->handleJackShutdown();
    }
    
    void handleJackShutdown() {
        std::cerr << "JACK server shutdown!" << std::endl;
        // Notify the UI that JACK has shutdown
    }

public:
    JackMixerClient(const std::string& clientName) : client(nullptr) {
        // Open client
        jack_status_t status;
        client = jack_client_open(clientName.c_str(), JackNullOption, &status);
        
        if (client == nullptr) {
            throw std::runtime_error("Failed to create JACK client");
        }
        
        // Set callbacks
        jack_set_process_callback(client, processCallback, this);
        jack_on_shutdown(client, jackShutdownCallback, this);
    }
    
    ~JackMixerClient() {
        if (client) {
            jack_client_close(client);
        }
    }
    
    void createPorts(int numInputs, int numOutputs) {
        std::lock_guard<std::mutex> lock(portMutex);
        
        // Create input ports
        for (int i = 0; i < numInputs; i++) {
            std::string portName = "input_" + std::to_string(i + 1);
            jack_port_t* port = jack_port_register(client, portName.c_str(), 
                                                  JACK_DEFAULT_AUDIO_TYPE, 
                                                  JackPortIsInput, 0);
            if (port == nullptr) {
                throw std::runtime_error("Failed to create input port");
            }
            inputPorts.push_back(port);
            
            // Initialize channel parameters
            ChannelParams params = { 1.0f, 0.5f, 1.0f, false, false };
            channelParams.push_back(params);
            
            // Initialize metering data
            MeterData meter = { 0.0f, 0.0f };
            meterData.push_back(meter);
        }
        
        // Create output ports
        for (int i = 0; i < numOutputs; i++) {
            std::string portName = "output_" + std::to_string(i + 1);
            jack_port_t* port = jack_port_register(client, portName.c_str(), 
                                                  JACK_DEFAULT_AUDIO_TYPE, 
                                                  JackPortIsOutput, 0);
            if (port == nullptr) {
                throw std::runtime_error("Failed to create output port");
            }
            outputPorts.push_back(port);
        }
    }
    
    bool activate() {
        return (jack_activate(client) == 0);
    }
    
    bool deactivate() {
        return (jack_deactivate(client) == 0);
    }
    
    bool connectPorts(const std::string& src, const std::string& dst) {
        return (jack_connect(client, src.c_str(), dst.c_str()) == 0);
    }
    
    bool disconnectPorts(const std::string& src, const std::string& dst) {
        return (jack_disconnect(client, src.c_str(), dst.c_str()) == 0);
    }
    
    void setChannelVolume(int channel, float volume) {
        if (channel >= 0 && channel < static_cast<int>(channelParams.size())) {
            channelParams[channel].volume = volume;
        }
    }
    
    void setChannelPan(int channel, float pan) {
        if (channel >= 0 && channel < static_cast<int>(channelParams.size())) {
            channelParams[channel].pan = pan;
        }
    }
    
    void setChannelMute(int channel, bool mute) {
        if (channel >= 0 && channel < static_cast<int>(channelParams.size())) {
            channelParams[channel].mute = mute;
        }
    }
    
    MeterData getChannelMeter(int channel) {
        if (channel >= 0 && channel < static_cast<int>(meterData.size())) {
            return meterData[channel];
        }
        return { 0.0f, 0.0f };
    }
    
    int getSampleRate() {
        return jack_get_sample_rate(client);
    }
    
    int getBufferSize() {
        return jack_get_buffer_size(client);
    }
    
    float getCpuLoad() {
        return jack_cpu_load(client);
    }
};
```

### C++/CLI Bridge Example

```cpp
// MaiksMixerBridge.h
#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

namespace MaiksMixer {
    
    public ref class JackPort {
    public:
        property String^ Name;
        property String^ Type;
        property bool IsInput;
        property bool IsOutput;
        property bool IsPhysical;
        property List<String^>^ Connections;
    };
    
    public ref class JackStatus {
    public:
        property bool IsRunning;
        property int SampleRate;
        property int BufferSize;
        property double CpuLoad;
        property int Xruns;
        property double Latency;
    };
    
    public ref class MeterData {
    public:
        property float Peak;
        property float Rms;
    };
    
    public ref class JackBridge {
    public:
        JackBridge();
        ~JackBridge();
        !JackBridge(); // Finalizer
        
        // JACK client management
        bool Connect(String^ clientName);
        bool Disconnect();
        bool IsConnected();
        
        // Port management
        bool CreatePorts(int numInputs, int numOutputs);
        array<JackPort^>^ GetPorts();
        bool ConnectPorts(String^ source, String^ destination);
        bool DisconnectPorts(String^ source, String^ destination);
        
        // Channel parameters
        void SetChannelVolume(int channel, float volume);
        void SetChannelPan(int channel, float pan);
        void SetChannelMute(int channel, bool mute);
        void SetChannelSolo(int channel, bool solo);
        
        // Metering
        MeterData^ GetChannelMeter(int channel);
        
        // JACK status
        JackStatus^ GetStatus();
        
        // JACK server management
        bool StartJackServer(int sampleRate, int bufferSize, int periods, String^ priority);
        bool StopJackServer();
        
    private:
        // Pointer to the native C++ implementation
        void* _nativeClient;
    };
}
```

C# JACK Service Example

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace MaiksMixer
{
    public class JackService : IDisposable
    {
        private readonly JackBridge _bridge;
        private readonly Timer _meterUpdateTimer;
        private readonly Timer _statusUpdateTimer;
        
        public event EventHandler<MeterUpdateEventArgs> MeterUpdated;
        public event EventHandler<JackStatusEventArgs> StatusUpdated;
        public event EventHandler<JackConnectionEventArgs> ConnectionChanged;
        
        public JackService()
        {
            _bridge = new JackBridge();
            
            _meterUpdateTimer = new Timer(33); // ~30fps
            _meterUpdateTimer.Elapsed += OnMeterUpdateTimer;
            
            _statusUpdateTimer = new Timer(1000); // 1 second
            _statusUpdateTimer.Elapsed += OnStatusUpdateTimer;
        }
        
        public async Task<bool> ConnectAsync(string clientName)
        {
            return await Task.Run(() => {
                bool result = _bridge.Connect(clientName);
                if (result)
                {
                    _meterUpdateTimer.Start();
                    _statusUpdateTimer.Start();
                }
                return result;
            });
        }
        
        public async Task<bool> DisconnectAsync()
        {
            _meterUpdateTimer.Stop();
            _statusUpdateTimer.Stop();
```
