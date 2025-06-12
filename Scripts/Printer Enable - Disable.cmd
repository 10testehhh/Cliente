@echo off
title Printer Enable - Disable
color a
echo.
echo ----------------------------------
echo - 1) Printer Enabled		  -
echo - 2) Printer Disabled		  -
echo ----------------------------------
set /p printer=:
if %printer% EQU 1 goto e
if %printer% EQU 2 goto d

:e
reg add "HKLM\SYSTEM\CurrentControlSet\Services\Spooler" /v "Start" /t REG_DWORD /d "2" /f >nul 2>&1
sc start Spooler >nul 2>&1
reg add "HKLM\SYSTEM\CurrentControlSet\Services\PrintNotify" /v "Start" /t REG_DWORD /d "3" /f >nul 2>&1
reg add "HKLM\SYSTEM\CurrentControlSet\Services\PrintWorkflowUserSvc" /v "Start" /t REG_DWORD /d "3" /f >nul 2>&1

dism /online /Enable-Feature /FeatureName:Printing-Foundation-InternetPrinting-Client >nul 2>&1
dism /online /Enable-Feature /FeatureName:Printing-Foundation-Features >nul 2>&1
echo Done, restart your computer.
timeout /t 3 /nobreak
exit

:d
sc stop Spooler >nul 2>&1
reg add "HKLM\SYSTEM\CurrentControlSet\Services\Spooler" /v "Start" /t REG_DWORD /d "4" /f >nul 2>&1
reg add "HKLM\SYSTEM\CurrentControlSet\Services\PrintNotify" /v "Start" /t REG_DWORD /d "4" /f >nul 2>&1
reg add "HKLM\SYSTEM\CurrentControlSet\Services\PrintWorkflowUserSvc" /v "Start" /t REG_DWORD /d "4" /f >nul 2>&1

dism /online /Disable-Feature /FeatureName:Printing-Foundation-InternetPrinting-Client >nul 2>&1
dism /online /Disable-Feature /FeatureName:Printing-Foundation-Features >nul 2>&1
echo Done, restart your computer.
timeout /t 3 /nobreak
exit