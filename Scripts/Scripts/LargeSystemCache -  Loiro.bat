@echo off
title LargeSystemCache Tweak - TEST
color a
:home
cls
echo.
echo -----------------------------------------
echo - 1. Enable LargeSystemCache            -
echo - 2. Disable LargeSystemCache (Default) -
echo -----------------------------------------
echo.
set /p lsc=:
if %lsc% EQU 1 goto 1
if %lsc% EQU 2 goto 2
goto home

:1
Reg.exe add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v "LargeSystemCache" /t REG_DWORD /d "1" /f
exit
:2
Reg.exe add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v "LargeSystemCache" /t REG_DWORD /d "0" /f
exit