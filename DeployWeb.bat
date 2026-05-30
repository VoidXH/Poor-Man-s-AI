@echo off
setlocal

if not exist "WebLocation.txt" (
    echo ERROR: WebLocation.txt not found.
    echo Please create WebLocation.txt and add the target directory path.
    pause
    exit /b 1
)

set /p WebLocation=<WebLocation.txt

if not defined WebLocation (
    echo ERROR: WebLocation.txt is empty.
    echo Please add the target directory path.
    pause
    exit /b 1
)

if not exist "%WebLocation%" (
    echo ERROR: Target directory "%WebLocation%" does not exist.
    pause
    exit /b 1
)

echo Deploying changed web files to: %WebLocation%
echo.

robocopy "src_web" "%WebLocation%" /E /XF "__*.*" "*.jpg" /XO /R:0 /W:0

echo.
echo Deployment complete.
