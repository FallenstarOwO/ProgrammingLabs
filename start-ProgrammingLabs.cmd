@echo off
setlocal
set "ROOT=%~dp0"
set "APP=%ROOT%publish\ProgrammingLabs.exe"
if exist "%APP%" (
    start "" "%APP%"
    exit /b
)

set "APP=%ROOT%ProgrammingLabs\bin\Release\net10.0-windows\ProgrammingLabs.exe"
if exist "%APP%" (
    start "" "%APP%"
    exit /b
)

echo Release build not found. Building...
dotnet build "%ROOT%ProgrammingLabs.sln" -c Release
if exist "%APP%" (
    start "" "%APP%"
) else (
    echo Build failed. Please check that .NET 10 SDK is installed.
    pause
)
