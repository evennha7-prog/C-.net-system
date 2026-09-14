# ==============================================================================
# PCCFPI STORE - Automated Build & Setup Installer Packager
# ==============================================================================
$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "  PCCFPI STORE - Building Installable Release & Setup   " -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan

# 1. Locate MSBuild and C# Compiler
$msbuildPaths = @(
    "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
)

$msbuild = $msbuildPaths | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $msbuild) {
    throw "MSBuild.exe was not found. Please ensure Visual Studio or .NET Build Tools is installed."
}

$cscPaths = @(
    "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Roslyn\csc.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe",
    "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
)

$csc = $cscPaths | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $csc) {
    throw "csc.exe (C# Compiler) was not found."
}

Write-Host "[1/6] Compilers detected:" -ForegroundColor Green
Write-Host "      MSBuild: $msbuild"
Write-Host "      CSC:     $csc"

# 2. Generate Multi-Resolution app.ico if not exists
$icoPath = Join-Path $scriptDir "assignment_code\icons\app.ico"
$pngPath = Join-Path $scriptDir "assignment_code\icons\pccfpi.png"

if ((-not (Test-Path $icoPath)) -and (Test-Path $pngPath)) {
    Write-Host "[2/6] Generating multi-resolution app.ico from $pngPath..." -ForegroundColor Yellow
    Add-Type -AssemblyName System.Drawing
    $sourceBmp = [System.Drawing.Bitmap]::FromFile($pngPath)
    $sizes = @(16, 32, 48, 64, 128, 256)
    $images = @()
    foreach ($size in $sizes) {
        $resized = new-object System.Drawing.Bitmap($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $g = [System.Drawing.Graphics]::FromImage($resized)
        $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
        $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $g.Clear([System.Drawing.Color]::Transparent)
        $g.DrawImage($sourceBmp, 0, 0, $size, $size)
        $g.Dispose()
        $images += $resized
    }
    $ms = New-Object System.IO.MemoryStream
    $bw = New-Object System.IO.BinaryWriter($ms)
    $bw.Write([UInt16]0)
    $bw.Write([UInt16]1)
    $bw.Write([UInt16]$images.Count)
    $pngStreams = @()
    foreach ($img in $images) {
        $pms = New-Object System.IO.MemoryStream
        $img.Save($pms, [System.Drawing.Imaging.ImageFormat]::Png)
        $pngStreams += $pms
    }
    $offset = 6 + ($images.Count * 16)
    for ($i = 0; $i -lt $images.Count; $i++) {
        $size = $sizes[$i]
        $bSize = if ($size -ge 256) { 0 } else { [byte]$size }
        $bw.Write([byte]$bSize)
        $bw.Write([byte]$bSize)
        $bw.Write([byte]0)
        $bw.Write([byte]0)
        $bw.Write([UInt16]1)
        $bw.Write([UInt16]32)
        $bw.Write([UInt32]$pngStreams[$i].Length)
        $bw.Write([UInt32]$offset)
        $offset += $pngStreams[$i].Length
    }
    for ($i = 0; $i -lt $images.Count; $i++) {
        $bytes = $pngStreams[$i].ToArray()
        $bw.Write($bytes)
        $pngStreams[$i].Dispose()
        $images[$i].Dispose()
    }
    $sourceBmp.Dispose()
    $bw.Flush()
    [System.IO.File]::WriteAllBytes($icoPath, $ms.ToArray())
    $bw.Dispose()
    $ms.Dispose()
}

# 3. Build Main Solution in Release Mode
Write-Host "[3/6] Building project in Release configuration..." -ForegroundColor Yellow
$projPath = Join-Path $scriptDir "assignment_code\assignment_code.csproj"
& $msbuild $projPath /p:Configuration=Release /t:Rebuild /verbosity:minimal /nologo
if ($LASTEXITCODE -ne 0) {
    throw "MSBuild failed with exit code $LASTEXITCODE"
}

# 4. Prepare Staging Folder & Compile Uninstaller
Write-Host "[4/6] Staging files & compiling uninstaller..." -ForegroundColor Yellow
$releaseDir = Join-Path $scriptDir "assignment_code\bin\Release"
$distDir = Join-Path $scriptDir "dist"
$stagingDir = Join-Path $scriptDir "installer\staging"

if (Test-Path $distDir) { Remove-Item $distDir -Recurse -Force }
if (Test-Path $stagingDir) { Remove-Item $stagingDir -Recurse -Force }
New-Item -ItemType Directory -Path $distDir -Force | Out-Null
New-Item -ItemType Directory -Path $stagingDir -Force | Out-Null

# Copy release output to staging
Copy-Item "$releaseDir\*" $stagingDir -Recurse -Force
# Exclude pdb files to keep payload small
Get-ChildItem -Path $stagingDir -Filter "*.pdb" -Recurse | Remove-Item -Force

# Compile Uninstaller
$uninstSrc = Join-Path $scriptDir "installer\UninstallProgram.cs"
$uninstExe = Join-Path $scriptDir "installer\uninstall.exe"
& $csc /target:winexe /win32icon:"$icoPath" /out:"$uninstExe" /r:System.Windows.Forms.dll,System.Drawing.dll,System.dll "$uninstSrc"
if ($LASTEXITCODE -ne 0) {
    throw "Failed to compile Uninstaller!"
}

# 5. Package Payload Zip & Compile Setup Installer EXE
Write-Host "[5/6] Compressing payload & compiling Setup Installer EXE..." -ForegroundColor Yellow
$payloadZip = Join-Path $scriptDir "installer\Payload.zip"
if (Test-Path $payloadZip) { Remove-Item $payloadZip -Force }

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($stagingDir, $payloadZip, [System.IO.Compression.CompressionLevel]::Optimal, $false)

$setupSrc = Join-Path $scriptDir "installer\SetupProgram.cs"
$setupExe = Join-Path $distDir "PCCFPI_Store_Setup.exe"

& $csc /target:winexe /win32icon:"$icoPath" /out:"$setupExe" `
    /resource:"$payloadZip,Payload.zip" `
    /resource:"$uninstExe,uninstall.exe" `
    /resource:"$icoPath,app.ico" `
    /r:System.IO.Compression.dll,System.IO.Compression.FileSystem.dll,System.Windows.Forms.dll,System.Drawing.dll,System.dll,Microsoft.CSharp.dll `
    "$setupSrc"

if ($LASTEXITCODE -ne 0) {
    throw "Failed to compile Setup Installer EXE!"
}

# 6. Create Portable ZIP & Documentation
Write-Host "[6/6] Creating Portable ZIP package and user guide..." -ForegroundColor Yellow

# Add launcher bat files to staging for portable users
$launchBat = @"
@echo off
start "" "%~dp0assignment_code.exe"
"@
$launchBat | Out-File (Join-Path $stagingDir "Launch_PCCFPI_Store.bat") -Encoding ASCII

$testDbBat = @"
@echo off
"%~dp0assignment_code.exe" test-db
pause
"@
$testDbBat | Out-File (Join-Path $stagingDir "Test_Database_Connection.bat") -Encoding ASCII

$portableZip = Join-Path $distDir "PCCFPI_Store_Portable_v1.0.0.zip"
[System.IO.Compression.ZipFile]::CreateFromDirectory($stagingDir, $portableZip, [System.IO.Compression.CompressionLevel]::Optimal, $false)

# Generate Readme
$readmeContent = @"
==============================================================================
 PCCFPI STORE - MODERN POS & STORE MANAGEMENT SYSTEM
 Version 1.0.0
==============================================================================

HOW TO INSTALL ON ANY OTHER WINDOWS COMPUTER:
------------------------------------------------------------------------------
Option 1: Graphical Setup Installer (Recommended)
  1. Copy 'PCCFPI_Store_Setup.exe' to the target computer (via USB or network).
  2. Double-click 'PCCFPI_Store_Setup.exe'.
  3. Click 'Install Now'.
  4. The installer will automatically:
     - Install the application into your user Programs directory (no admin needed).
     - Create a Desktop shortcut with the PCCFPI Store icon.
     - Create a Start Menu shortcut.
     - Register the uninstaller in Windows Settings / Control Panel.
  5. The store will launch immediately!

Option 2: Silent / Unattended Installation (Lab or Mass Deployment)
  Run in CMD or PowerShell as:
    PCCFPI_Store_Setup.exe /S

Option 3: Portable Zero-Install Run
  1. Extract 'PCCFPI_Store_Portable_v1.0.0.zip' to any folder or USB drive.
  2. Double-click 'assignment_code.exe' or 'Launch_PCCFPI_Store.bat'.

SYSTEM REQUIREMENTS:
------------------------------------------------------------------------------
- Windows 10 (version 1903 or later) or Windows 11 (Both include .NET 4.8 by default).
- For Windows 7 / 8 / 8.1: Microsoft .NET Framework 4.8 Runtime.
- Active Internet Connection (Cloud PostgreSQL Database on Aiven).

DEFAULT LOGIN CREDENTIALS:
------------------------------------------------------------------------------
- Admin:   admin@store.com   / Password: password
- Cashier: cashier@store.com / Password: password
- Manager: manager@store.com / Password: password

PCCFP INSTITUTE
==============================================================================
"@
$readmeContent | Out-File (Join-Path $distDir "README_INSTALL.txt") -Encoding UTF8

# Cleanup temporary files
if (Test-Path $stagingDir) { Remove-Item $stagingDir -Recurse -Force }
if (Test-Path $payloadZip) { Remove-Item $payloadZip -Force }
if (Test-Path $uninstExe) { Remove-Item $uninstExe -Force }

Write-Host "`n========================================================" -ForegroundColor Green
Write-Host " BUILD & PACKAGING COMPLETE!" -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Green
Write-Host "Deliverables created in: $distDir" -ForegroundColor Cyan
Get-ChildItem -Path $distDir | Select-Object Name, @{Name="Size (MB)";Expression={[math]::Round($_.Length/1MB, 2)}} | Format-Table -AutoSize
