@echo off
color a
title Nagle Algorithm
:home
cls
echo.
echo ------------------------------
echo - PLEASE READ THIS CAREFULLY -
echo ------------------------------
timeout /t 2 /nobreak >nul
echo Enabling or disabling the Nagle Algorithm can improve or hurt your connection. SHOULD BE TESTED
echo Enabling it, data is buffered until it's larger than the MSS size (value close to MTU), 
echo this can lead to higher latencies.
echo Disabling it no data is buffered but under load it can become congested and consequently lose internet connection.
echo.
echo.
echo ----------------------------------------
echo - 1. Enable Nagle Algorithm (Default)  -
echo - 2. Disable Nagle Algorithm           -
echo ----------------------------------------
set /p menu=Enter an option:
if %menu% EQU 1 goto e
if %menu% EQU 2 goto d
goto home

:e
for /f %%i in ('wmic path win32_networkadapter get GUID ^| findstr "{"') do (
    reg delete "HKLM\System\CurrentControlSet\services\Tcpip\Parameters\Interfaces\%%i" /v "TcpAckFrequency" /f
    reg delete "HKLM\System\CurrentControlSet\services\Tcpip\Parameters\Interfaces\%%i" /v "TcpDelAckTicks" /f
    reg delete "HKLM\System\CurrentControlSet\services\Tcpip\Parameters\Interfaces\%%i" /v "TCPNoDelay" /f
)
timeout /t 2 /nobreak >NUL 2>&1
exit

:d
for /f %%i in ('wmic path win32_networkadapter get GUID ^| findstr "{"') do (
	reg add "HKLM\System\CurrentControlSet\services\Tcpip\Parameters\Interfaces\%%i" /v "TcpAckFrequency" /t REG_DWORD /d "1" /f
	reg add "HKLM\System\CurrentControlSet\services\Tcpip\Parameters\Interfaces\%%i" /v "TcpDelAckTicks" /t REG_DWORD /d "0" /f
    reg add "HKLM\System\CurrentControlSet\services\Tcpip\Parameters\Interfaces\%%i" /v "TCPNoDelay" /t REG_DWORD /d "1" /f
)
timeout /t 2 /nobreak >NUL 2>&1
exit