@echo off
setlocal
cd /d "%~dp0"
echo ========================================================
echo  PCCFPI STORE - BUILDING INSTALLER SETUP AND PACKAGES
echo ========================================================
powershell -ExecutionPolicy Bypass -NoProfile -File "%~dp0build_dist.ps1"
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Build failed with exit code %ERRORLEVEL%!
    pause
    exit /b %ERRORLEVEL%
)
echo.
echo Packaging finished successfully!
echo Check the 'dist' folder for PCCFPI_Store_Setup.exe.
pause
