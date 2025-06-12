@echo off
color a
title Enable/Disable TextInputHost
:home
cls
echo ----------------------------
echo - 1. Enable TextInputHost  -
echo - 2. Disable TextInputHost -
echo ----------------------------
echo.
set /p tih=:
if %tih% EQU 1 goto e
if %tih% EQU 2 goto d
goto home

:e
PowerRun.exe /SW:0 Reg.exe del "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\TextInputHost.exe" /f
PowerRun.exe /SW:0 Reg.exe del "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\WindowsInternal.ComposableShell.Experiences.TextInput.InputApp.exe" /f
exit

:d
PowerRun.exe /SW:0 Reg.exe add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\TextInputHost.exe" /v Debugger /d "%SystemRoot%\system32\systray.exe" /f
PowerRun.exe /SW:0 taskkill.exe /im "TextInputHost.exe" /t /f
PowerRun.exe /SW:0 Reg.exe add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\WindowsInternal.ComposableShell.Experiences.TextInput.InputApp.exe" /v Debugger /d "%SystemRoot%\system32\systray.exe" /f
PowerRun.exe /SW:0 taskkill.exe /im "WindowsInternal.ComposableShell.Experiences.TextInput.InputApp.exe" /t /f 
PowerRun.exe /SW:0 Reg.exe add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\WindowsInternal.ComposableShell.Experiences.TextInput.InputApp.exe" /v Debugger /f
exit