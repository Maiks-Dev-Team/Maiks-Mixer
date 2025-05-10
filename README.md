# MaiksMixer

A professional virtual audio mixer application with advanced routing capabilities and a modern user interface.

## Project Overview

MaiksMixer is a virtual audio mixer that allows users to route audio between physical and virtual devices, apply mixing controls, and manage audio configurations. The application consists of a C# WPF user interface that communicates with a C++ audio engine and drivers.

## Project Structure

The solution is organized into the following projects:

- **MaiksMixer.UI**: WPF user interface application with custom audio controls and visualizations
- **MaiksMixer.Core**: Core business logic, models, and shared components
- **MaiksMixer.Communication**: Communication layer between the C# UI and C++ audio engine
- **MaiksMixer.Tests**: Unit and integration tests for the application

## Key Features

- Audio routing between physical and virtual devices
- Channel strips with faders, meters, and controls
- Virtual audio device creation and management
- Configuration presets
- Real-time audio level visualization
- System tray integration

## Development Setup

### Prerequisites

- .NET 6.0+ SDK
- Visual Studio 2022 or later (recommended)
- C++ development tools (for the audio engine component)

### Getting Started

1. Clone the repository
2. Open the solution in Visual Studio
3. Build the solution
4. Run the MaiksMixer.UI project

## Communication Protocol

The UI communicates with the C++ audio engine using a combination of:

- Named pipes (`\\.\pipe\MaiksMixerPipe`)
- Memory-mapped files (`MaiksMixerSharedMemory`)
- C++/CLI bridge library

See [CommunicationProtocol.md](./CommunicationProtocol.md) for detailed information about the message formats and commands.

## Development Roadmap

See [checklist.md](./checklist.md) for the current development status and roadmap.

## License

[License information to be added]
