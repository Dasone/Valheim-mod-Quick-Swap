<#
.SYNOPSIS
    Removes the hot-reload copy of Quick Swap from BepInEx\scripts.

.DESCRIPTION
    Run this before launching with a -Release install, so BepInEx does not see two
    copies of the same plugin GUID.
#>
[CmdletBinding()]
param(
    [string]$BepInExProfileDir = (Join-Path $env:APPDATA 'r2modmanPlus-local\Valheim\profiles\Valheim 1.0\BepInEx')
)

$ErrorActionPreference = 'Stop'

$scripts = Join-Path $BepInExProfileDir 'scripts'
foreach ($name in 'QuickSwap.dll', 'QuickSwap.pdb') {
    $path = Join-Path $scripts $name
    if (Test-Path $path) {
        Remove-Item $path -Force
        Write-Host "Removed $path"
    }
}
