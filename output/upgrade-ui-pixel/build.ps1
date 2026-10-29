Add-Type -AssemblyName System.Drawing
$outDir = $PSScriptRoot
$navy = [System.Drawing.ColorTranslator]::FromHtml('#101522')
$edge = [System.Drawing.ColorTranslator]::FromHtml('#070B14')
$colors = @('#95A4BD','#B665ED','#42B5F5','#69E4AE')
$items = [System.Collections.Generic.List[object]]::new()
function New-PixelImage($w,$h) { return [System.Drawing.Bitmap]::new($w,$h,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb) }
function Save-Sprite($name,$bmp) {
  $bmp.Save((Join-Path $outDir "$name.png"),[System.Drawing.Imaging.ImageFormat]::Png)
  $items.Add(@{name=$name;image=$bmp})
}
function Frame($name,$w,$h,$hex,$pressed=$false) {
  $b = New-PixelImage $w $h
  $c = [System.Drawing.ColorTranslator]::FromHtml($hex)
  for($y=0;$y -lt $h;$y++) { for($x=0;$x -lt $w;$x++) {
    $d = [Math]::Min($y,$h-1-$y)
    $cut = if($d -eq 0){4}elseif($d -eq 1){2}elseif($d -eq 2){1}else{0}
    if($x -lt $cut -or $x -ge $w-$cut){continue}
    $innerD=[Math]::Min($x-$cut,$w-1-$cut-$x)
    $col=$navy
    if($d -eq 0 -or $innerD -eq 0){$col=$edge}
    elseif($d -eq 1 -or $innerD -eq 1){$col=$c}
    elseif($pressed){$col=[System.Drawing.ColorTranslator]::FromHtml('#19132B')}
    $b.SetPixel($x,$y,$col)
  }}
  Save-Sprite $name $b
}
$labels=@('neutral','epic','rare','common')
for($i=0;$i -lt 4;$i++){ Frame "card-$($labels[$i])" 80 128 $colors[$i] }
Frame 'button-normal' 96 24 $colors[0]
Frame 'button-hover' 96 24 $colors[1]
Frame 'button-pressed' 96 24 '#74419F' $true
for($i=0;$i -lt 4;$i++){
  Frame "icon-slot-$($labels[$i])" 24 24 $colors[$i]
  $c=[System.Drawing.ColorTranslator]::FromHtml($colors[$i])
  $b=New-PixelImage 7 7
  for($y=0;$y -lt 7;$y++){for($x=0;$x -lt 7;$x++){
    $b.SetPixel($x,$y,$(if($x -eq 0 -or $y -eq 0 -or $x -eq 6 -or $y -eq 6){$edge}elseif($i -eq 0){$navy}else{$c}))
  }}
  Save-Sprite "pip-$($labels[$i])" $b
  $b=New-PixelImage 9 9
  for($y=0;$y -lt 9;$y++){for($x=0;$x -lt 9;$x++){
    $d=[Math]::Abs($x-4)+[Math]::Abs($y-4)
    if($d -eq 3 -or $d -eq 4){$b.SetPixel($x,$y,$c)}
  }}
  Save-Sprite "diamond-$($labels[$i])" $b
  $b=New-PixelImage 64 7
  for($x=0;$x -lt 64;$x++){if($x -lt 28 -or $x -gt 35){$b.SetPixel($x,3,$c)}}
  for($y=0;$y -lt 7;$y++){for($x=28;$x -le 34;$x++){if(([Math]::Abs($x-31)+[Math]::Abs($y-3)) -eq 3){$b.SetPixel($x,$y,$c)}}}
  Save-Sprite "divider-$($labels[$i])" $b
}
$b=New-PixelImage 11 9
$c=[System.Drawing.ColorTranslator]::FromHtml($colors[0])
for($x=0;$x -le 8;$x++){ $b.SetPixel($x,4,$c) }
for($y=0;$y -lt 9;$y++){ $x=10-[Math]::Abs($y-4);$b.SetPixel($x,$y,$c);if($x -gt 6){$b.SetPixel($x-1,$y,$c)} }
Save-Sprite 'arrow-right' $b
$sheet=New-PixelImage 512 256
$g=[System.Drawing.Graphics]::FromImage($sheet)
$manifest=@();$sx=8;$sy=8;$rowH=0
foreach($item in $items){
  $b=$item.image
  if($sx+$b.Width+8 -gt 512){$sx=8;$sy+=$rowH+8;$rowH=0}
  $g.DrawImageUnscaled($b,$sx,$sy)
  $manifest+=@{name=$item.name;x=$sx;yTop=$sy;yUnity=256-$sy-$b.Height;width=$b.Width;height=$b.Height}
  $sx+=$b.Width+8;$rowH=[Math]::Max($rowH,$b.Height)
}
$g.Dispose()
$sheet.Save((Join-Path $outDir 'upgrade-ui-atlas.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$manifest | ConvertTo-Json | Set-Content -Encoding UTF8 (Join-Path $outDir 'sprite-rects.json')
$preview=New-PixelImage 1536 768
for($y=0;$y -lt 256;$y++){for($x=0;$x -lt 512;$x++){
  $c=$sheet.GetPixel($x,$y)
  for($dy=0;$dy -lt 3;$dy++){for($dx=0;$dx -lt 3;$dx++){$preview.SetPixel($x*3+$dx,$y*3+$dy,$c)}}
}}
$preview.Save((Join-Path $outDir 'preview-3x.png'),[System.Drawing.Imaging.ImageFormat]::Png)
Write-Output "Created $($items.Count) sprites, atlas, coordinates, and nearest-neighbor preview."
