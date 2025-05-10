# MaiksMixer Communication Protocol

## Overview

This document defines the communication protocol between the C++ audio engine/JACK integration (located at `B:\Projects\C++\MaiksMixer`) and the C# user interface (located at `B:\Projects\C#\MaiksMixer`). The protocol enables bidirectional communication for audio routing, device management, and mixer control.

The system uses JACK Audio Connection Kit as the core audio routing and processing engine, with a custom communication layer to connect the C# WPF UI to the C++ JACK client.

## Communication Methods

### 1. Inter-Process Communication (IPC)

The primary communication method will be through a named pipe or memory-mapped file:

- **Named Pipe**: `\\.\pipe\MaiksMixerPipe`
- **Memory-Mapped File**: `MaiksMixerSharedMemory`

### 2. C++/CLI Bridge Library

A bridge library (`MaiksMixerBridge.dll`) will be created to facilitate direct method calls between C++ and C#:

- C++ exports functions using `extern "C"` for C# P/Invoke
- C++/CLI wrapper provides .NET-friendly interface

## Message Format

All messages follow a standard JSON format:

```json
{
  "messageType": "string",
  "timestamp": "ISO8601 timestamp",
  "messageId": "UUID",
  "payload": {}
}
```

### Message Types

1. **Command**: Request from UI to audio engine
2. **Response**: Reply from audio engine to UI
3. **Event**: Asynchronous notification from audio engine
4. **Heartbeat**: Connection health check

## Audio Routing Commands

### Set Audio Route

```json
{
  "messageType": "Command",
  "messageId": "uuid-string",
  "timestamp": "2025-05-09T19:16:40+02:00",
  "payload": {
    "command": "SetRoute",
    "sourceId": "input-device-id",
    "destinationId": "output-device-id",
    "enabled": true,
    "volume": 0.75
  }
}
```

### Get Routing Matrix

```json
{
  "messageType": "Command",
  "messageId": "uuid-string",
  "timestamp": "2025-05-09T19:16:40+02:00",
  "payload": {
    "command": "GetRoutingMatrix"
  }
}
```

## Device Management

### Create Virtual Device

```json
{
  "messageType": "Command",
  "messageId": "uuid-string",
  "timestamp": "2025-05-09T19:16:40+02:00",
  "payload": {
    "command": "CreateVirtualDevice",
    "deviceType": "input", // or "output"
    "deviceName": "MaiksMixer Virtual Input",
    "channelCount": 2,
    "sampleRate": 48000,
    "bitDepth": 24
  }
}
```

### Remove Virtual Device

```json
{
  "messageType": "Command",
  "messageId": "uuid-string",
  "timestamp": "2025-05-09T19:16:40+02:00",
  "payload": {
    "command": "RemoveVirtualDevice",
    "deviceId": "device-id"
  }
}
```

### List Available Devices

```json
{
  "messageType": "Command",
  "messageId": "uuid-string",
  "timestamp": "2025-05-09T19:16:40+02:00",
  "payload": {
    "command": "ListDevices"
  }
}
```

## Mixer Controls

### Set Channel Properties

```json
{
  "messageType": "Command",
  "messageId": "uuid-string",
  "timestamp": "2025-05-09T19:16:40+02:00",
  "payload": {
    "command": "SetChannelProperties",
    "channelId": "channel-id",
    "properties": {
      "volume": 0.8,
      "pan": 0.2,
      "mute": false,
      "solo": false,
      "gain": 3.0
    }
  }
}
```

## Events

### Audio Level Update

```json
{
  "messageType": "Event",
  "messageId": "uuid-string",
  "timestamp": "2025-05-09T19:16:40+02:00",
  "payload": {
    "event": "LevelUpdate",
    "channelId": "channel-id",
    "peakLevel": -12.5,
    "rmsLevel": -18.2
  }
}
```

### Device Added/Removed

```json
{
  "messageType": "Event",
  "messageId": "uuid-string",
  "timestamp": "2025-05-09T19:16:40+02:00",
  "payload": {
    "event": "DeviceChanged",
    "changeType": "added", // or "removed"
    "deviceId": "device-id",
    "deviceName": "Device Name",
    "deviceType": "input" // or "output"
  }
}
```

## Error Handling

Error responses follow this format:

```json
{
  "messageType": "Response",
  "messageId": "uuid-string",
  "inResponseTo": "original-message-id",
  "timestamp": "2025-05-09T19:16:40+02:00",
  "payload": {
    "success": false,
    "errorCode": 1001,
    "errorMessage": "Device not found"
  }
}
```

## Implementation Notes

### C++ Side (Audio Engine)

1. Implement a message queue for processing commands
2. Create a worker thread for handling IPC
3. Use JSON library (e.g., nlohmann/json) for message parsing
4. Implement command handlers for each message type

### C# Side (UI)

1. Create an IPC client wrapper class
2. Implement async message handling
3. Use System.Text.Json for message serialization/deserialization
4. Create event-based system for handling audio engine events

## Performance Considerations

1. Use binary format for high-frequency messages (e.g., level meters)
2. Batch small updates when possible
3. Consider using shared memory for audio level data
4. Implement throttling for UI updates (e.g., limit meter updates to 30fps)

## Security Considerations

1. Validate all incoming messages
2. Implement message authentication for sensitive operations
3. Sanitize all inputs before processing
