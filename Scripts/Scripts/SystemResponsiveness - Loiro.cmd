@echo off
color a
title SystemResponsiveness
:home
cls
echo.
echo ------------------------------
echo - PLEASE READ THIS CAREFULLY -
echo ------------------------------
timeout /t 2 /nobreak >nul
echo The SystemResponsiveness value determines the percentage of resources allocated to low priority tasks by the CPU.
echo This value is used by the Multimedia Class Scheduler.
echo.
echo A higher value increases NDIS.sys DPCs but may eliminate stuttering in some cases, a lower value (minimum 10/a hex) may increase performance.
echo The value 64 hex (100dec) disables MMCSS.
echo.
echo.
echo ------------------------------------------------
echo - 1. Default Windows SystemResponsiveness (14) -
echo - 2. Minimum SystemResponsiveness value (a)    -
echo - 3. Disable MMCSS - SystemResponsiveness (64) -
echo - 4. Custom SystemResponsiveness Value         -
echo ------------------------------------------------
set /p menu=Enter an option:
if %menu% EQU 1 goto a
if %menu% EQU 2 goto b
if %menu% EQU 3 goto c
if %menu% EQU 4 goto d
goto home

:a
Reg.exe add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" /v "SystemResponsiveness" /t REG_DWORD /d "20" /f
timeout /t 2 /nobreak >NUL 2>&1
exit

:b
Reg.exe add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" /v "SystemResponsiveness" /t REG_DWORD /d "10" /f
timeout /t 2 /nobreak >NUL 2>&1
exit

:c
Reg.exe add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" /v "SystemResponsiveness" /t REG_DWORD /d "100" /f
timeout /t 2 /nobreak >NUL 2>&1
exit

:d
:input
echo You need to write int values.
set /p value=Enter custom value (integers):
if %value% lss 10 (
    echo Value is too small. Please enter a value greater than or equal to 10.
    goto input
) else if %value% gtr 100 (
    echo Value is too large. Please enter a value less than or equal to 100.
    goto input
) else (
    Reg.exe add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" /v "SystemResponsiveness" /t REG_DWORD /d "%value%" /f
)
timeout /t 2 /nobreak >NUL 2>&1
exit