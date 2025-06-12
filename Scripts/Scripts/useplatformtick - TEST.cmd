@echo off
color a
title useplatformtick
:home
cls
echo.
echo ------------------------------
echo - PLEASE READ THIS CAREFULLY -
echo ------------------------------
timeout /t 2 /nobreak >nul
echo IN NO CASE SHOULD YOU MODIFY THIS IF YOU DISABLE HPET IN BIOS, 
echo this results in PMT being used as timer and you will get DESYNC.
echo.
echo Enabling useplatformtick you will be using RTC instead of TSC; this results in higher accuracy/stability 
echo but will not be as fast as TSC.
echo Disabling useplatformtick will use TSC clock + TSC Tick, it's important not to have DESYNC when doing this (HPET On - BIOS).
echo.
echo.
echo --------------------------------------
echo - 1. Enable useplatformtick (RTC)    -
echo - 2. Disable useplatformtick (TSC)   -
echo --------------------------------------
set /p menu=Enter an option:
if %menu% EQU 1 goto e
if %menu% EQU 2 goto d
goto home

:e
bcdedit /set useplatformtick Yes
timeout /t 2 /nobreak >NUL 2>&1
exit

:d
bcdedit /set useplatformtick No
timeout /t 2 /nobreak >NUL 2>&1
exit