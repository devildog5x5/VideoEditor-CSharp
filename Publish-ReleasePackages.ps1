# Build self-contained portable zips for each supported Windows runtime.
# This project uses WPF; macOS/Linux packages are not produced here.

param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$version = (dotnet msbuild VideoEditorCS.csproj -getProperty:Version -nologo).Trim()
Write-Host "Package version: $version" -ForegroundColor Cyan

Write-Host "Restoring..." -ForegroundColor Yellow
dotnet restore VideoEditorCS.sln
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

foreach ($rid in @("win-x64", "win-arm64")) {
    Write-Host "Publishing $rid..." -ForegroundColor Yellow
    $outDir = "dist/publish-$rid"
    if (Test-Path $outDir) { Remove-Item -Recurse -Force $outDir }
    dotnet publish VideoEditorCS.csproj `
        --configuration $Configuration `
        -r $rid `
        --self-contained true `
        -o $outDir `
        --no-restore
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    $zip = "dist/VideoEditor-$version-$rid.zip"
    if (Test-Path $zip) { Remove-Item -Force $zip }
    New-Item -ItemType Directory -Force -Path "dist" | Out-Null
    Compress-Archive -Path "$outDir\*" -DestinationPath $zip
    Write-Host "  -> $zip" -ForegroundColor Green
}

Write-Host "Done. Zips are under dist\" -ForegroundColor Green
