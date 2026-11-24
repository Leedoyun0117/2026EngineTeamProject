# 사용: powershell -File Tools/PixelStyle/generate_gear.ps1
# gear_pixel_0..3.png (40x40)를 Assets/LDY/Sprites/PixelStyle/Gear 에 쓴다. .meta는 건드리지 않는다.
Add-Type -AssemblyName System.Drawing
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$src = Get-Content (Join-Path $PSScriptRoot 'GearSpriteGenerator.cs') -Raw
Add-Type -TypeDefinition $src -ReferencedAssemblies System.Drawing
$out = Join-Path $root 'Assets/LDY/Sprites/PixelStyle/Gear'
[PixelStyle.GearSheetWriter]::Write($out, (New-Object PixelStyle.GearShape), 4)
Write-Output "written to $out"
