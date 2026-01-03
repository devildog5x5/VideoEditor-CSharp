# Professional Video Editor (C#)

A native Windows video editing application built with C# and WPF (.NET 8.0).

## Demo Video

A public domain sample video is included in the `Samples` folder for demonstration purposes:
- **File**: `Samples/sample_video.mp4`
- **Purpose**: Demonstration and testing of video editing features
- **Usage**: Import this video into the Video Editor to explore all features

## Features

- Timeline-based video editing
- Multi-track support
- Video effects (brightness, contrast, saturation, etc.)
- Audio controls (volume, speed, fade)
- 6 beautiful themes (Light, Dark, Ocean, Forest, Sunset, Midnight)
- Export to multiple formats (MP4, AVI, MOV)
- Project save/load functionality
- Modern WPF-based UI with smooth animations

## Requirements

- Windows 10/11 (64-bit)
- .NET 8.0 Desktop Runtime

## Building

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or later (recommended)

### Build Instructions

```bash
dotnet build --configuration Release
```

### Building Installer

The installer is built using Inno Setup. Run:

```powershell
.\BuildInstaller.ps1
```

Or manually compile `BuildInstaller.iss` using Inno Setup Compiler.

## License

See LICENSE file (if present) or refer to the main repository.
