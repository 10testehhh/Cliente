@echo off
tittle Firewall Enabled - Disabled
mode con: cols=50 lines=10
color a
echo.
echo ----------------------------------
echo - 1) Firewall Enabled		 -
echo - 2) Firewall Disabled		 -
echo ----------------------------------
set /p frw=:
if %frw% EQU 1 goto e
if %frw% EQU 2 goto d

:e
reg add "HKEY_LOCAL_MACHINE\System\CurrentControlSet\Services\mpssvc" /v "Start" /t REG_DWORD /d "2" /f
reg add "HKEY_LOCAL_MACHINE\System\CurrentControlSet\Services\BFE" /v "Start" /t REG_DWORD /d "2" /f
echo Done, restart your computer.
timeout /t 3 /nobreak
exit

:d
reg add "HKEY_LOCAL_MACHINE\System\CurrentControlSet\Services\mpssvc" /v "Start" /t REG_DWORD /d "4" /f
reg add "HKEY_LOCAL_MACHINE\System\CurrentControlSet\Services\BFE" /v "Start" /t REG_DWORD /d "4" /f
echo Done, restart your computer.
timeout /t 3 /nobreak
exit