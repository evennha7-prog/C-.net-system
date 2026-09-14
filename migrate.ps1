# ========================================================
#  PCCFPI STORE - PostgreSQL Database Migration Script
# ========================================================

$ErrorActionPreference = "Stop"

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host " PCCFPI STORE - Database Migration Runner" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host ""

$baseDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $baseDir

$binDir = Join-Path $baseDir "assignment_code\bin\Debug"
$exePath = Join-Path $binDir "assignment_code.exe"

if (!(Test-Path $exePath)) {
    Write-Host "Building project first..." -ForegroundColor Yellow
    & "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" "assignment_code\assignment_code.csproj" /p:Configuration=Debug /v:m
}

[System.IO.Directory]::SetCurrentDirectory($binDir)

[System.AppDomain]::CurrentDomain.add_AssemblyResolve([System.ResolveEventHandler]{
    param($s, $e)
    $shortName = $e.Name.Split(',')[0].Trim()
    $file = Join-Path $binDir ($shortName + ".dll")
    if (Test-Path $file) {
        return [System.Reflection.Assembly]::LoadFrom($file)
    }
    return $null
})

$asm = [System.Reflection.Assembly]::LoadFrom($exePath)
$envLoader = $asm.GetType('assignment_code.Services.EnvLoader')
$loadMethod = $envLoader.GetMethod('Load', [Type[]]@([string]))
if ($loadMethod) { $loadMethod.Invoke($null, @($null)) }

$migrator = $asm.GetType('assignment_code.Services.Database.DatabaseMigrator')
$res = $migrator.GetMethod('Migrate', [Type[]]@([bool])).Invoke($null, @($true))

if ($res.Success) {
    Write-Host "Status: SUCCESS" -ForegroundColor Green
    Write-Host "Time:   $($res.ElapsedMilliseconds)ms" -ForegroundColor Green
    Write-Host "Info:   $($res.Message)" -ForegroundColor Green
} else {
    Write-Host "Status: FAILED" -ForegroundColor Red
    Write-Host "Error:  $($res.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Migration Steps:" -ForegroundColor Cyan
foreach ($step in $res.ExecutedSteps) {
    Write-Host "  $step" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================================" -ForegroundColor Cyan
