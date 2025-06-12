@echo off
title LUA Enabled - Disabled
mode con: cols=50 lines=10
color a
echo.
echo ----------------------------------
echo - 1) LUA Enabled		 -
echo - 2) LUA Disabled (DEFAULT KOS)	 -
echo ----------------------------------
set /p wifi=:
if %wifi% EQU 1 goto e
if %wifi% EQU 2 goto d

:e
Reg.exe add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System" /v "EnableLUA" /t REG_DWORD /d "1" /f
Reg.exe add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System" /v "FilterAdministratorToken" /t REG_DWORD /d "1" /f
echo Done, restart your computer.
timeout /t 3 /nobreak
exit

:d
Reg.exe add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System" /v "EnableLUA" /t REG_DWORD /d "0" /f
Reg.exe add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System" /v "FilterAdministratorToken" /t REG_DWORD /d "0" /f
echo Done, restart your computer.
timeout /t 3 /nobreak
exit