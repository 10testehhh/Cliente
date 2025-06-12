@echo off
title NoLazyMode
mode con: cols=83 lines=20
color a
:home
cls
echo -----------------------------
echo - 1) NoLazyMode at 0 (OFF)  -
echo - 2) NoLazyMode at 1 (ON)   -	 
echo -----------------------------
echo.
echo --------------------------------------------
echo - IT IS RECOMMENDED TO READ THIS CAREFULLY -
echo --------------------------------------------
echo.
echo Activating NoLazyMode can generate higher cycle usage and become more 
echo aggressive in reducing idle time. MMCSS will operate in a more "real time" mode.
echo.
echo May increase FPS, reduce latencies, or the opposite. It has to be tested.
echo On the other hand, it can cause "Stuttering or Micro-Stuttering".
echo.
set /p nlm=Select an option: 
if %nlm% EQU 1 goto 0
if %nlm% EQU 2 goto 1
goto home

:0
cls
Reg.exe delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" /v "NoLazyMode" /f
echo Done, restart your computer and test it.
timeout /t 3 /nobreak
exit

:1
cls
Reg.exe add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" /v "NoLazyMode" /t REG_DWORD /d "1" /f
echo Done, restart your computer and test it.
timeout /t 3 /nobreak
exit
