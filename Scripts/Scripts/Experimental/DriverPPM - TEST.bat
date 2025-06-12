@echo off
title Drivers IntelPPM - AmdPPM
mode con: cols=75 lines=15
color a
:home
cls

set "PPM="
for /f "delims=" %%a in ('wmic cpu get name') do (
    echo %%a | findstr /i "AMD" >nul && set "PPM=AmdPPM"
    echo %%a | findstr /i "Intel" >nul && set "PPM=IntelPPM"
)

:drv
echo ----------------------------------------------------------------------
echo This is experimental, as you may get or have performance problems,   -
echo BSODs, possible causes are: Stuttering or Micro-Stuttering.          -
echo This can have great variability in any system.                       -
echo ----------------------------------------------------------------------
echo.
echo --------------------------------
echo - 1) %PPM% Enabled (Default)  -
echo - 2) %PPM% Disabled           -
echo --------------------------------
set /p option=:
if %option%==1 (
    cls
    reg add "HKLM\System\CurrentControlSet\Services\%PPM%" /v "Start" /t REG_DWORD /d "3" /f
) else if %option%==2 (
    cls
    reg add "HKLM\System\CurrentControlSet\Services\%PPM%" /v "Start" /t REG_DWORD /d "4" /f
    exit
) else (
    cls
    goto :drv
)
echo Done, restart your computer and test it.
timeout /t 3 /nobreak
exit
