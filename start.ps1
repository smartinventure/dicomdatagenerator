# Builds and starts the DICOM Data Generator (Web API + web UI in one app).
# Usage:  ./start.ps1            (Release build, runs on http://localhost:5300)
#         ./start.ps1 -NoBrowser (don't open a browser)
[CmdletBinding()]
param(
    [switch]$NoBrowser
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$project = Join-Path $root 'src\DicomDataGenerator'
$url = 'http://localhost:5300'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error 'The .NET SDK (dotnet) was not found on PATH. Install .NET 8 SDK: https://dotnet.microsoft.com/download'
    exit 1
}

Write-Host '==> Building (Release)...' -ForegroundColor Cyan
dotnet build $project -c Release --nologo
if ($LASTEXITCODE -ne 0) { Write-Error 'Build failed.'; exit $LASTEXITCODE }

Write-Host "==> Starting on $url (Ctrl+C to stop)..." -ForegroundColor Green
if (-not $NoBrowser) {
    Start-Job -ScriptBlock { Start-Sleep -Seconds 3; Start-Process $using:url } | Out-Null
}

# Run without launchSettings so the URL is deterministic.
dotnet run --project $project -c Release --no-build --no-launch-profile --urls $url
