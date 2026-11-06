$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$source = Join-Path $PSScriptRoot '../../exolon'
$destination = Join-Path $PSScriptRoot '../assets'
$images = Join-Path $destination 'images'
$sound = Join-Path $destination 'sound'
$maps = Join-Path $destination 'maps'
New-Item -ItemType Directory -Force -Path $images, $sound, $maps | Out-Null
Get-ChildItem -LiteralPath (Join-Path $source 'images') -File | ForEach-Object {
    if ($_.Extension -ieq '.gif') {
        $image = [System.Drawing.Image]::FromFile($_.FullName)
        try { $image.Save((Join-Path $images ($_.BaseName + '.png')), [System.Drawing.Imaging.ImageFormat]::Png) }
        finally { $image.Dispose() }
    } elseif ($_.Extension -ieq '.png') {
        Copy-Item -LiteralPath $_.FullName -Destination $images -Force
    }
}
Get-ChildItem -LiteralPath (Join-Path $source 'sound') -Filter '*.ogg' -File | Copy-Item -Destination $sound -Force
& (Join-Path $PSScriptRoot 'import-melon-maps.ps1')
