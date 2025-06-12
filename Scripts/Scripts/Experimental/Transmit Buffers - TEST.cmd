@echo off
setlocal
title Receive Buffers (TEST)
mode con: cols=140 lines=30
color a
cls
echo ------------------------------
echo -           MENU             -
echo ------------------------------
echo - 1) Transmit Buffers "512"  -
echo - 2) Transmit Buffers "1024" -
echo - 3) Transmit Buffers "2048" -
echo - 4) Transmit Buffers "4096" -
echo - 5) Enter manual value.     -
echo ------------------------------
echo.
echo --------------------------------------------
echo - IT IS RECOMMENDED TO READ THIS CAREFULLY -
echo --------------------------------------------
echo.
echo Receive Buffers is a network setting that determines the amount of memory allocated to store data that 
echo is waiting to be received over the network. The data being received over the network is stored in these buffers until it can be processed.
echo. 
echo By increasing the size of the receive buffer, you can allow your system to handle more data at once. 
echo This can result in an improvement in network performance, especially on high-speed networks. 
echo Receive buffers allow your system to handle data more efficiently, as it can store more data and process it when ready.
echo.
echo Increasing the size of the receive buffer means you are allocating more memory to store network data. 
echo This can result in less memory available for other operations on your system. 
echo If the receive buffers are too large, they can cause delays in data processing. 
echo This is because data can get stuck in the buffer waiting to be processed.
echo.
set /p rb=Select an option: 
set "x="

if %rb% EQU 1 (
    set "x=512"
) else if %rb% EQU 2 (
    set "x=1024"
) else if %rb% EQU 3 (
    set "x=2048"
) else if %rb% EQU 4 (
    set "x=4096"
) else if %rb% EQU 5 (
    set /p "x=Introduce the new value for Transmit Buffers: "
) else (
    echo Invalid option
    exit /b
)

cls
for /f %%i in ('wmic path Win32_NetworkAdapter get PNPDeviceID^| findstr /L "PCI\VEN_"') do (
    for /f "tokens=3" %%a in ('reg query "HKLM\SYSTEM\CurrentControlSet\Enum\%%i" /v "Driver" ^| find "{"') do (
        for /f "delims=" %%b in ('reg query "HKLM\SYSTEM\CurrentControlSet\Control\Class\%%a" /s /f "*TransmitBuffers" ^| findstr "HKEY"') do (
            reg.exe add "%%b" /v "*TransmitBuffers" /t REG_SZ /d "%x%" /f > NUL 2>&1
        )
    )
)

echo Done, restart your computer and test it.
timeout /t 3 /nobreak
exit /b
