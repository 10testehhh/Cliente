@echo off
color a
:home
cls
echo Select your language.
echo Selecciona tu idioma.
echo --------------
echo - 1. Spanish -
echo - 2. English -
echo --------------
set /p menu=:
if %menu% EQU 1 goto spanish
if %menu% EQU 2 goto english
goto home

:spanish
cls
echo ---------------------------------------
echo - SE RECOMIENDA LEER ESTO ATENTAMENTE -
echo ---------------------------------------
timeout /t 2 /nobreak >nul
echo Este script deshabilita el driver "Multimedia Class Scheduler".
echo Puede aumentar FPS, reducir latencias o todo lo contrario. Se tiene que testear.
echo PUEDES DESHABILITAR ESTO SI TENES STUTTERING O DIVERSOS PROBLEMAS EN TUS JUEGOS.
echo.
echo AL FINALIZAR EL SCRIPT SE REINICIARA TU PC.
echo.
echo --------------------------------
echo - 1. Habilitar MMCSS (Default) -
echo - 2. Deshabilitar MMCSS        -
echo --------------------------------
set /p menu2=:
if %menu2% EQU 1 goto e
if %menu2% EQU 2 goto d
goto spanish

:english
cls
echo ------------------------------
echo - PLEASE READ THIS CAREFULLY -
echo ------------------------------
timeout /t 2 /nobreak >nul
echo This script disables the "Multimedia Class Scheduler" driver.
echo It can increase FPS, reduce latencies or the opposite. It has to be tested.
echo YOU CAN DISABLE THIS IF YOU HAVE STUTTERING OR DIVERSE PROBLEMS IN YOUR GAMES.
echo.
echo AT THE END OF THE SCRIPT YOUR PC WILL RESTART.
echo.
echo -----------------------------
echo - 1. Enable MMCSS (Default) -
echo - 2. Disable MMCSS          -
echo -----------------------------
set /p menu2=:
if %menu2% EQU 1 goto e
if %menu2% EQU 2 goto d
goto english

:e
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\MMCSS" /v "Start" /t REG_DWORD /d "2" /f
shutdown -r -f -t 5 -c "Please wait until your PC restarts..."
timeout /t 2 /nobreak >NUL 2>&1
exit

:d
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\MMCSS" /v "Start" /t REG_DWORD /d "4" /f
shutdown -r -f -t 5 -c "Please wait until your PC restarts..."
timeout /t 2 /nobreak >NUL 2>&1
exit