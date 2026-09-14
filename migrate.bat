@echo off
echo ========================================================
echo  PCCFPI STORE - PostgreSQL Database Migration
echo ========================================================
echo.

set EXE_PATH=assignment_code\bin\Debug\assignment_code.exe

if not exist "%EXE_PATH%" (
    echo Building project...
    "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" assignment_code\assignment_code.csproj /p:Configuration=Debug /v:m
)

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$base = (Resolve-Path 'assignment_code\bin\Debug').Path;" ^
    "[System.IO.Directory]::SetCurrentDirectory($base);" ^
    "[System.AppDomain]::CurrentDomain.add_AssemblyResolve([System.ResolveEventHandler]{ param($s, $e) $shortName = $e.Name.Split(',')[0].Trim(); $file = Join-Path $base ($shortName + '.dll'); if (Test-Path $file) { return [System.Reflection.Assembly]::LoadFrom($file); } return $null; });" ^
    "$asm = [System.Reflection.Assembly]::LoadFrom((Join-Path $base 'assignment_code.exe'));" ^
    "$envLoader = $asm.GetType('assignment_code.Services.EnvLoader');" ^
    "$loadMethod = $envLoader.GetMethod('Load', [Type[]]@([string]));" ^
    "if ($loadMethod) { $loadMethod.Invoke($null, @($null)); }" ^
    "$migrator = $asm.GetType('assignment_code.Services.Database.DatabaseMigrator');" ^
    "$res = $migrator.GetMethod('Migrate', [Type[]]@([bool])).Invoke($null, @($true));" ^
    "Write-Output ('Status:  ' + $(if ($res.Success) { 'SUCCESS' } else { 'FAILED' }));" ^
    "Write-Output ('Time:    ' + $res.ElapsedMilliseconds + 'ms');" ^
    "Write-Output ('Message: ' + $res.Message);" ^
    "Write-Output '';" ^
    "Write-Output 'Migration Details:';" ^
    "foreach ($step in $res.ExecutedSteps) { Write-Output ('  ' + $step); }"

echo.
echo ========================================================
