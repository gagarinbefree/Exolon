$ErrorActionPreference = 'Stop'
$sourceMaps = Join-Path $PSScriptRoot '../../exolon/maps'
$maps = Join-Path $PSScriptRoot '../assets/maps'
New-Item -ItemType Directory -Force -Path $maps | Out-Null
Get-ChildItem -LiteralPath $sourceMaps -Filter '*.tmx' -File | Copy-Item -Destination $maps -Force
$scenes = Join-Path $PSScriptRoot '../scenes/zones'
New-Item -ItemType Directory -Force -Path $scenes | Out-Null
Get-ChildItem -LiteralPath $maps -Filter 'L??S??.tmx' -File | ForEach-Object {
    $scene = Join-Path $scenes ($_.BaseName + '.tscn')
    if (-not (Test-Path -LiteralPath $scene)) {
        $level = [int]$_.BaseName.Substring(1, 2)
        $screen = [int]$_.BaseName.Substring(4, 2)
        $zone = ($level - 1) * 25 + ($screen - 1)
        $contents = "[gd_scene load_steps=2 format=3]`n`n[ext_resource type=`"Script`" path=`"res://scripts/screens/FirstScreen.cs`" id=`"1`"]`n`n[node name=`"Zone$($_.BaseName)`" type=`"Node2D`"]`nscript = ExtResource(`"1`")`nInitialLevel = `"$($_.BaseName)`"`nInitialZoneCount = $zone`n"
        Set-Content -LiteralPath $scene -Value $contents -Encoding utf8NoBOM
    }
}
