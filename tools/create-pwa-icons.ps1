Add-Type -AssemblyName System.Drawing
$root = Split-Path -Parent $PSScriptRoot
$output = Join-Path $root 'Exolon.web/wwwroot'
$title = [System.Drawing.Bitmap]::FromFile((Join-Path $root 'assets/images/title.png'))
$sprites = [System.Drawing.Bitmap]::FromFile((Join-Path $root 'assets/images/vitorc2.png'))
try {
    $master = [System.Drawing.Bitmap]::new(512, 512)
    $graphics = [System.Drawing.Graphics]::FromImage($master)
    try {
        $graphics.Clear([System.Drawing.Color]::Black)
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
        $graphics.DrawImage($title, [System.Drawing.Rectangle]::new(68, 100, 376, 94))
        $graphics.DrawImage($sprites, [System.Drawing.Rectangle]::new(184, 208, 144, 192), 0, 0, 48, 64, [System.Drawing.GraphicsUnit]::Pixel)
    } finally { $graphics.Dispose() }
    try {
        foreach ($name in @('pwa-512.png', 'pwa-maskable-512.png')) {
            $master.Save((Join-Path $output $name), [System.Drawing.Imaging.ImageFormat]::Png)
        }
        foreach ($size in @(192, 180)) {
            $icon = [System.Drawing.Bitmap]::new($size, $size)
            $canvas = [System.Drawing.Graphics]::FromImage($icon)
            try {
                $canvas.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
                $canvas.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
                $canvas.DrawImage($master, [System.Drawing.Rectangle]::new(0, 0, $size, $size))
                $icon.Save((Join-Path $output "pwa-$size.png"), [System.Drawing.Imaging.ImageFormat]::Png)
            } finally { $canvas.Dispose(); $icon.Dispose() }
        }
    } finally { $master.Dispose() }
} finally { $title.Dispose(); $sprites.Dispose() }
