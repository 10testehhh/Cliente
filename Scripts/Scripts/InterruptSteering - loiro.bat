@echo off
color a
title Interrupt Steering Enable/Disable
:home
cls
echo.
echo ------------------------------
echo - PLEASE READ THIS CAREFULLY -
echo ------------------------------
timeout /t 2 /nobreak >nul
echo "Interrupt Steering" is a technique that efficiently distributes IRQs among CPU cores.
echo It can enhance performance and responsiveness in multi-core systems. You need to TEST it.
echo.
echo AT THE END OF THE SCRIPT YOUR PC WILL RESTART.
echo.
echo --------------------------------
echo - 1. Disable InterruptSteering -
echo - 2. Enable InterruptSteering  -
echo --------------------------------
set /p menu2=:
if %menu2% EQU 1 goto d
if %menu2% EQU 2 goto e
goto home

:d
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\kernel" /v "InterruptSteeringDisabled" /t REG_DWORD /d "1" /f
shutdown -r -f -t 5 -c "Please wait until your PC restarts..."
timeout /t 2 /nobreak >NUL 2>&1
exit

:e
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\kernel" /v "InterruptSteeringDisabled" /t REG_DWORD /d "0" /f
shutdown -r -f -t 5 -c "Please wait until your PC restarts..."
timeout /t 2 /nobreak >NUL 2>&1
exit