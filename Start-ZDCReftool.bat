@echo off
title ZDC Reference Tool
echo Starting ZDC Reference Tool...
echo.
echo Once started, open your browser to: http://localhost:5063
echo Press Ctrl+C to stop the server.
echo.
dotnet run --project ZdcReference.csproj --launch-profile http
pause
