param([string]$OutIco, [string]$OutPreview)
Add-Type -AssemblyName System.Drawing
$C = [System.Drawing.Color]

function RoundRect($x, $y, $w, $h, $r) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = 2 * $r
    $p.AddArc($x, $y, $d, $d, 180, 90); $p.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $p.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90); $p.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $p.CloseFigure(); $p
}

function Render([int]$n) {
    $bmp = New-Object System.Drawing.Bitmap $n, $n
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'
    $s = $n / 256.0
    $small = $n -le 24

    # dark tile
    $tile = RoundRect (6*$s) (6*$s) (244*$s) (244*$s) (52*$s)
    $g.FillPath([System.Drawing.SolidBrush]::new($C::FromArgb(255, 7, 9, 15)), $tile)
    $g.DrawPath([System.Drawing.Pen]::new($C::FromArgb(255, 18, 66, 95), [Math]::Max(1, 5*$s)), $tile)

    # ring: violet -> cyan, with soft glow
    $r = 84 * $s; $cx = 128 * $s
    $rect = [System.Drawing.RectangleF]::new($cx - $r, $cx - $r, 2*$r, 2*$r)
    $ringW = if ($small) { 30*$s } else { 16*$s }
    if (-not $small) {
        foreach ($k in 14..1) {
            $glow = [System.Drawing.Pen]::new($C::FromArgb(10, 0, 229, 255), ($ringW + $k*4*$s))
            $g.DrawEllipse($glow, $rect)
        }
    }
    $grad = [System.Drawing.Drawing2D.LinearGradientBrush]::new($rect, $C::FromArgb(255, 124, 77, 255), $C::FromArgb(255, 0, 229, 255), [single]45)
    $g.DrawEllipse([System.Drawing.Pen]::new($grad, $ringW), $rect)

    # glowing gamma symbol
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $fmt = New-Object System.Drawing.StringFormat
    $fmt.Alignment = 'Center'; $fmt.LineAlignment = 'Center'
    $em = if ($small) { 150*$s } else { 128*$s }
    $path.AddString([string][char]0x03B3, (New-Object System.Drawing.FontFamily 'Segoe UI'), 1, $em,
        [System.Drawing.RectangleF]::new(0, 8*$s, $n, $n), $fmt)
    if (-not $small) {
        foreach ($k in 10..1) {
            $glow = [System.Drawing.Pen]::new($C::FromArgb(12, 0, 229, 255), ($k*3*$s))
            $glow.LineJoin = 'Round'
            $g.DrawPath($glow, $path)
        }
    }
    $g.FillPath([System.Drawing.SolidBrush]::new($C::FromArgb(255, 234, 255, 255)), $path)
    $g.Dispose()
    $bmp
}

# 32-bit DIB entry: BITMAPINFOHEADER (double height) + bottom-up BGRA + empty AND mask
function Dib($b) {
    $n = $b.Width
    $data = $b.LockBits((New-Object System.Drawing.Rectangle 0, 0, $n, $n), 'ReadOnly', 'Format32bppArgb')
    $px = New-Object byte[] ($n * $n * 4)
    [System.Runtime.InteropServices.Marshal]::Copy($data.Scan0, $px, 0, $px.Length)
    $b.UnlockBits($data)
    $ms = New-Object System.IO.MemoryStream
    $w = New-Object System.IO.BinaryWriter $ms
    $w.Write([uint32]40); $w.Write([int32]$n); $w.Write([int32]($n * 2)); $w.Write([uint16]1); $w.Write([uint16]32)
    $w.Write([uint32]0); $w.Write([uint32]0); $w.Write([int32]0); $w.Write([int32]0); $w.Write([uint32]0); $w.Write([uint32]0)
    for ($y = $n - 1; $y -ge 0; $y--) { $w.Write($px, $y * $n * 4, $n * 4) }
    $w.Write((New-Object byte[] ([Math]::Ceiling($n / 32) * 4 * $n)))
    , $ms.ToArray()
}

# PNG only for 256 (what Windows expects); classic DIBs for the small sizes
$sizes = 16, 24, 32, 48, 64, 128, 256
$pngs = foreach ($n in $sizes) {
    $b = Render $n
    if ($n -eq 256) {
        $b.Save($OutPreview)
        $ms = New-Object System.IO.MemoryStream
        $b.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
        , $ms.ToArray()
    } else { , (Dib $b) }
}
$out = New-Object System.IO.BinaryWriter ([System.IO.File]::Create($OutIco))
$out.Write([uint16]0); $out.Write([uint16]1); $out.Write([uint16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $n = $sizes[$i]
    $out.Write([byte]($n % 256)); $out.Write([byte]($n % 256)); $out.Write([byte]0); $out.Write([byte]0)
    $out.Write([uint16]1); $out.Write([uint16]32)
    $out.Write([uint32]$pngs[$i].Length); $out.Write([uint32]$offset)
    $offset += $pngs[$i].Length
}
foreach ($p in $pngs) { $out.Write([byte[]]$p) }
$out.Close()
