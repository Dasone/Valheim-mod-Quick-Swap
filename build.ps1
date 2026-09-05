<#
.SYNOPSIS
    Builds Quick Swap and deploys it into the Valheim BepInEx profile.

.DESCRIPTION
    Debug   -> BepInEx\scripts\                       (hot-reloadable via ScriptEngine)
    Release -> BepInEx\plugins\Samuel-QuickSwap\      (permanent install)

.PARAMETER Release
    Build and install the permanent copy instead of the hot-reload copy.

.PARAMETER Watch
    Rebuild on every source change. Combined with ScriptEngine's file watcher this
    reloads the mod in the running game a second or two after you save.

.EXAMPLE
    .\build.ps1
    .\build.ps1 -Watch
    .\build.ps1 -Release
#>
[CmdletBinding()]
param(
    [switch]$Release,
    [switch]$Watch
)

$ErrorActionPreference = 'Stop'

$project       = Join-Path $PSScriptRoot 'src\QuickSwap\QuickSwap.csproj'
$configuration = if ($Release) { 'Release' } else { 'Debug' }

if ($Watch) {
    if ($Release) { throw 'Use -Watch with the Debug configuration; -Release is a one-shot install.' }
    Write-Host 'Watching for changes. Save a file to rebuild; ScriptEngine reloads it in-game.' -ForegroundColor Cyan
    Write-Host 'Press Ctrl+C to stop.' -ForegroundColor DarkGray
    dotnet watch --project $project build -c $configuration
    return
}

dotnet build $project -c $configuration
if ($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE." }

if ($Release) {
    Write-Host ''
    Write-Host 'Installed to BepInEx\plugins. Remove BepInEx\scripts\QuickSwap.dll before' -ForegroundColor Yellow
    Write-Host 'launching, or BepInEx will refuse the second copy as a duplicate GUID.'    -ForegroundColor Yellow
}
