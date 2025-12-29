# Build C# Video Editor and Create Professional Windows Installer

param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Video Editor (C#) - Build Installer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if .NET SDK is installed
Write-Host "Checking .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK found: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ .NET SDK not found!" -ForegroundColor Red
    Write-Host "Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Step 1: Restoring NuGet packages..." -ForegroundColor Cyan
Write-Host "----------------------------------------" -ForegroundColor Cyan
dotnet restore VideoEditorCS.sln

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Package restore failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 2: Building project..." -ForegroundColor Cyan
Write-Host "----------------------------------------" -ForegroundColor Cyan
dotnet build VideoEditorCS.sln --configuration $Configuration --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

$exePath = "bin\$Configuration\net8.0-windows\VideoEditor.exe"
if (-not (Test-Path $exePath)) {
    Write-Host "❌ Executable not found at: $exePath" -ForegroundColor Red
    exit 1
}

$fullExePath = Resolve-Path $exePath
Write-Host ""
Write-Host "✅ Build successful!" -ForegroundColor Green
Write-Host "   Executable: $fullExePath" -ForegroundColor White
Write-Host "   Size: $([math]::Round((Get-Item $fullExePath).Length / 1MB, 2)) MB" -ForegroundColor White

Write-Host ""
Write-Host "Step 3: Building installer..." -ForegroundColor Cyan
Write-Host "----------------------------------------" -ForegroundColor Cyan

# Check if Inno Setup is installed
$innoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if (-not (Test-Path $innoSetupPath)) {
    $innoSetupPath = "C:\Program Files\Inno Setup 6\ISCC.exe"
}

if (-not (Test-Path $innoSetupPath)) {
    Write-Host ""
    Write-Host "⚠ Inno Setup not found!" -ForegroundColor Yellow
    Write-Host "Please install Inno Setup from: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Executable is ready at: $fullExePath" -ForegroundColor White
    exit 0
}

# Create installer directory
if (-not (Test-Path "installer")) {
    New-Item -ItemType Directory -Path "installer" | Out-Null
}

# Build installer
& $innoSetupPath "BuildInstaller.iss"

if (Test-Path "installer\VideoEditor-Setup.exe") {
    $installerPath = Resolve-Path "installer\VideoEditor-Setup.exe"
    Write-Host ""
    Write-Host "✅ Installer created successfully!" -ForegroundColor Green
    Write-Host "   Installer: $installerPath" -ForegroundColor White
    Write-Host "   Size: $([math]::Round((Get-Item $installerPath).Length / 1MB, 2)) MB" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "⚠ Installer build may have failed. Check installer directory." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Build Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📦 Files Created:" -ForegroundColor Cyan
Write-Host "   Executable: $fullExePath" -ForegroundColor White
if (Test-Path "installer\VideoEditor-Setup.exe") {
    Write-Host "   Installer: $(Resolve-Path 'installer\VideoEditor-Setup.exe')" -ForegroundColor White
}
Write-Host ""

