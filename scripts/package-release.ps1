# Builds the release zip into dist\EmilyIsAwayTooAccess.zip.
# Usage (from the repo root):
#   powershell -ExecutionPolicy Bypass -File scripts\package-release.ps1
# The zip mirrors the game folder, so extracting it into the game folder installs both mods
# (BepInEx 5 x86 itself is installed separately, in the game folder and in EmilyOnline,
# by the installer or by hand):
#   BepInEx/plugins/EmilyIsAwayTooAccess/              the Emily is Away Too mod + Prism
#   EmilyOnline/BepInEx/plugins/EmilyOnlineAccess/      the Emily Online mod + Prism
#   EmilyIsAwayTooAccess-README.md, EmilyIsAwayTooAccess-CHANGELOG.md

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

# SkipDeploy: a release build doesn't touch the local game install.
# DevTools=false: never ship the private test drivers, even on a machine that has them.
$outGame = Join-Path $root "src\EmilyIsAwayTooAccess\bin\ReleasePackage"
$outOnline = Join-Path $root "src\EmilyOnlineAccess\bin\ReleasePackage"
dotnet build src\EmilyIsAwayTooAccess -c Release -p:SkipDeploy=true -p:DevTools=false -o $outGame
if ($LASTEXITCODE -ne 0) { throw "Build failed (Emily is Away Too Access)." }
dotnet build src\EmilyOnlineAccess -c Release -p:SkipDeploy=true -p:DevTools=false -o $outOnline
if ($LASTEXITCODE -ne 0) { throw "Build failed (Emily Online Access)." }

$game = "BepInEx/plugins/EmilyIsAwayTooAccess"
$online = "EmilyOnline/BepInEx/plugins/EmilyOnlineAccess"
# Source path (relative to the repo root) -> path inside the zip.
$files = [ordered]@{
    "src\EmilyIsAwayTooAccess\bin\ReleasePackage\EmilyIsAwayTooAccess.dll" = "$game/EmilyIsAwayTooAccess.dll"
    "src\EmilyOnlineAccess\bin\ReleasePackage\EmilyOnlineAccess.dll"       = "$online/EmilyOnlineAccess.dll"
    "README.md"                                                         = "EmilyIsAwayTooAccess-README.md"
    "CHANGELOG.md"                                                      = "EmilyIsAwayTooAccess-CHANGELOG.md"
}
foreach ($plugin in @($game, $online)) {
    $files["prism\prism.dll|$plugin"] = "$plugin/prism.dll"
    $files["prism\LICENSE|$plugin"] = "$plugin/Prism/LICENSE"
    $files["prism\NOTICE|$plugin"] = "$plugin/Prism/NOTICE"
}
$folders = [ordered]@{
    "prism\LICENSES|$game"   = "$game/Prism/LICENSES"
    "prism\LICENSES|$online" = "$online/Prism/LICENSES"
}

$dist = Join-Path $root "dist"
New-Item -ItemType Directory -Path $dist -Force | Out-Null
$zip = Join-Path $dist "EmilyIsAwayTooAccess.zip"
if (Test-Path $zip) { Remove-Item $zip -Force }

# Write entries with forward slashes (Windows PowerShell's ZipFile writes backslashes).
# A key may carry "|something" to allow the same source twice; it's cut off here.
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::Open($zip, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($key in $files.Keys) {
        $full = Join-Path $root ($key.Split('|')[0])
        if (-not (Test-Path $full)) { throw "Missing release file: $full" }
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $full, $files[$key]) | Out-Null
    }
    foreach ($key in $folders.Keys) {
        $full = Join-Path $root ($key.Split('|')[0])
        if (-not (Test-Path $full)) { throw "Missing release folder: $full" }
        foreach ($item in Get-ChildItem $full -Recurse -File) {
            $name = $folders[$key] + "/" + $item.FullName.Substring($full.Length + 1).Replace([char]92, [char]47)
            [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $item.FullName, $name) | Out-Null
        }
    }
} finally {
    $archive.Dispose()
}
Remove-Item $outGame, $outOnline -Recurse -Force
Write-Host "Built: $zip"
