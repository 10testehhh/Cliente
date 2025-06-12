@echo off
tittle Bluetooth Enabled - Disabled
mode con: cols=50 lines=10
color a
echo.
echo ----------------------------------
echo - 1) Bluetooth Enabled		 -
echo - 2) Bluetooth Disabled		 -
echo ----------------------------------
set /p blth=:
if %blth% EQU 1 goto e
if %blth% EQU 2 goto d

:e
::services
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\bthserv" /v "Start" /t REG_DWORD /d "3" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BTAGService" /v "Start" /t REG_DWORD /d "3" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BluetoothUserService" /v "Start" /t REG_DWORD /d "3" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BthAvctpSvc" /v "Start" /t REG_DWORD /d "3" /f
::drivers
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\RFCOMM" /v "Start" /t REG_DWORD /d "3" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BthEnum" /v "Start" /t REG_DWORD /d "3" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BthLEEnum" /v "Start" /t REG_DWORD /d "3" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BthA2dp" /v "Start" /t REG_DWORD /d "3" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BthHFEnum" /v "Start" /t REG_DWORD /d "3" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BthMini" /v "Start" /t REG_DWORD /d "3" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BTHMODEM" /v "Start" /t REG_DWORD /d "3" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BTHPORT" /v "Start" /t REG_DWORD /d "3" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BTHUSB" /v "Start" /t REG_DWORD /d "3" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\HidBth" /v "Start" /t REG_DWORD /d "3" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Microsoft_Bluetooth_AvrcpTransport" /v "Start" /t REG_DWORD /d "3" /f
echo Done, restart your computer.
timeout /t 3 /nobreak
exit

:d
::services
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\bthserv" /v "Start" /t REG_DWORD /d "4" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BTAGService" /v "Start" /t REG_DWORD /d "4" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BluetoothUserService" /v "Start" /t REG_DWORD /d "4" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BthAvctpSvc" /v "Start" /t REG_DWORD /d "4" /f
::drivers
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\RFCOMM" /v "Start" /t REG_DWORD /d "4" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BthEnum" /v "Start" /t REG_DWORD /d "4" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BthLEEnum" /v "Start" /t REG_DWORD /d "4" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BthA2dp" /v "Start" /t REG_DWORD /d "4" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BthHFEnum" /v "Start" /t REG_DWORD /d "4" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BthMini" /v "Start" /t REG_DWORD /d "4" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BTHMODEM" /v "Start" /t REG_DWORD /d "4" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BTHPORT" /v "Start" /t REG_DWORD /d "4" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BTHUSB" /v "Start" /t REG_DWORD /d "4" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\HidBth" /v "Start" /t REG_DWORD /d "4" /f
Reg.exe add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Microsoft_Bluetooth_AvrcpTransport" /v "Start" /t REG_DWORD /d "4" /f
echo Done, restart your computer.
timeout /t 3 /nobreak
exit