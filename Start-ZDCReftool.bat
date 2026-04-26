@echo off
title ZDC Reference Tool

:: Change to the directory where this batch file lives
cd /d "%~dp0"

echo ========================================
echo  ZDC Reference Tool
echo ========================================
echo.

:: Check for Node.js
where node >nul 2>&1
if errorlevel 1 (
    echo ERROR: Node.js is not installed.
    echo Please install it from https://nodejs.org and try again.
    pause
    exit /b 1
)

:: Check for .NET
where dotnet >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK is not installed.
    echo Please install it from https://dotnet.microsoft.com/download/dotnet/10.0 and try again.
    pause
    exit /b 1
)

:: Install npm packages if node_modules is missing
if not exist "node_modules\" (
    echo Installing Node.js packages for the first time...
    npm install
    if errorlevel 1 (
        echo ERROR: npm install failed. Try running this batch file as Administrator.
        pause
        exit /b 1
    )
    echo.
)

echo Starting server...
echo Once ready, open your browser to: http://localhost:5063
echo Press Ctrl+C to stop the server.
echo.

dotnet run --project ZdcReference.csproj --launch-profile http
pause
