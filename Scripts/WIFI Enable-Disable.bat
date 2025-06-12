@echo off
title Wifi Enabled - Disabled
mode con: cols=50 lines=10
color a
echo.
echo ----------------------------------
echo - 1) WIFI Enabled		 -
echo - 2) WIFI Disabled		 -
echo ----------------------------------
set /p wifi=:
if %wifi% EQU 1 goto e
if %wifi% EQU 2 goto d

:e
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WlanSvc" /v "Start" /t REG_DWORD /d "2" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vwififlt" /v "Start" /t REG_DWORD /d "1" /f
echo Done, restart your computer.
timeout /t 3 /nobreak
exit

:d
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WlanSvc" /v "Start" /t REG_DWORD /d "4" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\vwififlt" /v "Start" /t REG_DWORD /d "4" /f
echo Done, restart your computer.
timeout /t 3 /nobreak
exit