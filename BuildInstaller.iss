[Setup]
AppName=Professional Video Editor (C#)
AppVersion=1.0.0
AppPublisher=Robert Foster
AppPublisherURL=https://github.com/devildog5x5/Video_Editor
AppSupportURL=https://github.com/devildog5x5/Video_Editor
AppUpdatesURL=https://github.com/devildog5x5/Video_Editor
DefaultDirName={autopf}\VideoEditorCSharp
DefaultGroupName=Video Editor (C#)
AllowNoIcons=yes
LicenseFile=
OutputDir=installer
OutputBaseFilename=VideoEditor-CSharp-Setup
SetupIconFile=
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
Source: "bin\Release\net8.0-windows\VideoEditor.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net8.0-windows\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion isreadme
; .NET 8.0 Runtime prerequisite - included in installer package
; Note: File must exist in prerequisites folder for installer to build
Source: "..\prerequisites\dotnet-runtime-8.0.0-win-x64.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Icons]
Name: "{group}\Video Editor (C#)"; Filename: "{app}\VideoEditor.exe"
Name: "{group}\{cm:UninstallProgram,Video Editor (C#)}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Video Editor (C#)"; Filename: "{app}\VideoEditor.exe"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\Video Editor (C#)"; Filename: "{app}\VideoEditor.exe"; Tasks: quicklaunchicon

[Run]
; Install .NET 8.0 Runtime if not already installed
Filename: "{tmp}\dotnet-runtime-8.0.0-win-x64.exe"; Parameters: "/install /quiet /norestart"; StatusMsg: "Installing .NET 8.0 Runtime..."; Flags: waituntilterminated; Check: ShouldInstallDotNet
Filename: "{app}\VideoEditor.exe"; Description: "{cm:LaunchProgram,Video Editor (C#)}"; Flags: nowait postinstall skipifsilent

[Code]
function IsDotNetInstalled: Boolean;
var
  Version: String;
begin
  Result := False;
  
  // Check for .NET 8.0 Desktop Runtime in the registry
  // First check: sharedfx registry key
  if RegKeyExists(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App\8.0.0') then
  begin
    Result := True;
    Exit;
  end;
  
  // Second check: Try to find any 8.x version
  if RegQueryStringValue(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', 'Version', Version) then
  begin
    if Copy(Version, 1, 1) = '8' then
    begin
      Result := True;
      Exit;
    end;
  end;
  
  // Third check: Check sharedhost version
  if RegQueryStringValue(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedhost', 'Version', Version) then
  begin
    if (Length(Version) >= 1) and (Version[1] >= '8') then
    begin
      Result := True;
      Exit;
    end;
  end;
  
  // Fourth check: Alternative registry location
  if RegKeyExists(HKLM, 'SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App\8.0.0') then
  begin
    Result := True;
    Exit;
  end;
end;

function ShouldInstallDotNet: Boolean;
begin
  // Install .NET if it's not already installed and the runtime file exists
  Result := (not IsDotNetInstalled) and FileExists(ExpandConstant('{tmp}\dotnet-runtime-8.0.0-win-x64.exe'));
end;

procedure InitializeWizard;
begin
  // Custom initialization if needed
end;

