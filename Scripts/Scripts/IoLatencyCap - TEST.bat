@echo off
title IoLatencyCap (Test)
mode con: cols=85 lines=25
color a
:home
cls
echo ----------------------------------------
echo - 1) IoLatencyCap (KernelOS Default)   -
echo - 2) IoLatencyCap (Windows Default)    -
echo - 3) IoLatencyCap 14 (20ms)            -
echo - 4) IoLatencyCap 32 (50ms)            -
echo - 5) IoLatencyCap 1f4 (500ms)          -
echo - 6) IoLatencyCap 19 (25ms)            -
echo - 7) Enter manual value.               -
echo ----------------------------------------
echo.
echo --------------------------------------------
echo - IT IS RECOMMENDED TO READ THIS CAREFULLY -
echo --------------------------------------------
echo.
echo IoLatencyCap is a registry entry that controls how StorPort handles I/O requests 
echo that have not been completed within a specified period of time. If the value of 
echo IoLatencyCap is greater than 0, StorPort will hold incoming I/O requests in the 
echo queue when any I/O request sent to the miniport driver has not been completed in 
echo the specified time period. 
echo. 
echo This could potentially optimise system performance or do the opposite by preventing
echo the system from becoming saturated with too many pending I/O operations.
echo.
set /p iolc=Select an option: 
if %iolc% EQU 1 goto 50
if %iolc% EQU 2 goto 0
if %iolc% EQU 3 goto 14
if %iolc% EQU 4 goto 32
if %iolc% EQU 5 goto 1f4
if %iolc% EQU 6 goto 19
if %iolc% EQU 7 goto manual
goto home

:50
cls
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services" /v "IoLatencyCap" /t REG_DWORD /d "80" /f
echo Done, restart your computer and test it.
timeout /t 3 /nobreak
exit

:0
cls
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services" /v "IoLatencyCap" /t REG_DWORD /d "0" /f
echo Done, restart your computer and test it.
timeout /t 3 /nobreak
exit

:14
cls
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services" /v "IoLatencyCap" /t REG_DWORD /d "20" /f
echo Done, restart your computer and test it.
timeout /t 3 /nobreak
exit

:32
cls
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services" /v "IoLatencyCap" /t REG_DWORD /d "50" /f
echo Done, restart your computer and test it.
timeout /t 3 /nobreak
exit

:1f4
cls
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services" /v "IoLatencyCap" /t REG_DWORD /d "500" /f
echo Done, restart your computer and test it.
timeout /t 3 /nobreak
exit

:19
cls
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services" /v "IoLatencyCap" /t REG_DWORD /d "25" /f
echo Done, restart your computer and test it.
timeout /t 3 /nobreak
exit

:manual
cls
set /p value=Enter a value: 
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services" /v "IoLatencyCap" /t REG_DWORD /d "%value%" /f
echo Done, restart your computer and test it.
timeout /t 3 /nobreak
exit
