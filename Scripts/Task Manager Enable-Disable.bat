@echo off
title Task Manager Changer.
mode con: cols=50 lines=10
color a
echo.
echo ------------------------------
echo - 1) Enable Process Explorer -
echo - 2) Enable Task Manager     -
echo ------------------------------
set /p taskmgr=:
if %taskmgr% EQU 1 goto e
if %taskmgr% EQU 2 goto d

:e
Reg.exe add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\taskmgr.exe" /v "Debugger" /t REG_SZ /d "\"C:\WINDOWS\PROCESS EXPLORER.EXE\"" /f
echo Done.
timeout /t 3 /nobreak
exit

:d
Reg.exe delete "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\taskmgr.exe" /f
echo Done.
timeout /t 3 /nobreak
exit