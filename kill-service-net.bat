@echo off
title NET Background Processes Killer
echo ---------------------------------------------------------
echo [1/2] Dang kill cac tien trinh build va runtime (Giu lai Visual Studio)...
echo ---------------------------------------------------------

:: 1. Kill cac tien trinh dotnet, compiler va server (KHONG co devenv.exe)
taskkill /F /IM dotnet.exe /T 2>nul
taskkill /F /IM MSBuild.exe /T 2>nul
taskkill /F /IM VBCSCompiler.exe /T 2>nul
taskkill /F /IM iisexpress.exe /T 2>nul
taskkill /F /IM msvsmon.exe /T 2>nul
taskkill /F /IM testhost.exe /T 2>nul

:: 2. Kill cac ServiceHub chay ngam (thuong gay lock file am tham)
powershell -Command "Get-Process | Where-Object { $_.Name -like 'ServiceHub*' -or $_.Name -like 'testhost*' } | Stop-Process -Force -ErrorAction SilentlyContinue"

echo.
echo ---------------------------------------------------------
echo [2/2] Dang quet cac service dang chay tu thu muc bin...
echo ---------------------------------------------------------

:: 3. Kill chinh xac cac file .exe dang chay tu folder Debug/Release cua project
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "Get-Process | Where-Object { $_.Path -like '*\bin\Debug\*' -or $_.Path -like '*\bin\Release\*' } | Stop-Process -Force -ErrorAction SilentlyContinue"

echo.
echo ---------------------------------------------------------
echo Da don dep xong! Start lai service se khong bi loi.
echo ---------------------------------------------------------
pause
