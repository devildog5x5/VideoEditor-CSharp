# Professional Video Editor (C#)

A comprehensive video editing application built with C# and WPF, featuring multiple themes and professional editing capabilities.

## Features

- ✅ **Video Import** - Support for MP4, AVI, MOV, MKV, and more
- ✅ **Timeline Editing** - Multi-track timeline with video clips
- ✅ **Cut & Trim** - Precise video cutting and trimming
- ✅ **Multiple Themes** - 6 beautiful themes (Light, Dark, Ocean, Forest, Sunset, Midnight)
- ✅ **Effects** - Brightness, Contrast, Saturation adjustments
- ✅ **Audio Control** - Volume and speed adjustments
- ✅ **Export** - Export to MP4, AVI, MOV formats
- ✅ **Professional UI** - Modern WPF interface

## Requirements

- .NET 8.0 Runtime (included in installer)
- Windows 10/11
- FFmpeg (for video processing)

## Building

### Build Executable
```powershell
dotnet build VideoEditorCS.sln --configuration Release
```

### Build Installer
```powershell
.\BuildInstaller.ps1
```

## File Locations

### Executable
**Location:** `bin\Release\net8.0-windows\VideoEditor.exe`

**Full Path:**
```
C:\Users\rober\Documents\GitHub\Video_Editor\VideoEditorCS\bin\Release\net8.0-windows\VideoEditor.exe
```

### Windows Installer
**Location:** `installer\VideoEditor-Setup.exe`

**Full Path:**
```
C:\Users\rober\Documents\GitHub\Video_Editor\VideoEditorCS\installer\VideoEditor-Setup.exe
```

## Themes

The application includes 6 professional themes:
- **Light** - Bright, clean interface
- **Dark** - Easy on the eyes (default)
- **Ocean** 🌊 - Calming blue tones
- **Forest** 🌲 - Natural green tones
- **Sunset** 🌅 - Warm orange tones
- **Midnight** 🌙 - Deep purple tones

Change themes from the toolbar dropdown.

## Usage

1. **Import Video** - Click "Import Video" or use File menu
2. **Add to Timeline** - Drag videos from library to timeline
3. **Edit** - Select clips and adjust properties
4. **Export** - Click "Export Video" when ready

## Both Versions Available

This repository contains both:
- **Python Version** - Original Python/PyQt6 implementation
- **C# Version** - New C#/WPF implementation (this version)

Both are fully functional and can be built independently.

