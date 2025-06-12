@echo off
color a
:home
cls
echo.
echo ------------------------------
echo - PLEASE READ THIS CAREFULLY -
echo ------------------------------
timeout /t 2 /nobreak >nul
echo If you disable IDLE in the powerplan then your CPU will be in CState 0.
echo You may have higher temps, lower performance (or not) and you will also have
echo a visual bug in Task Manager where the CPU will always be displayed at 100%.
echo.
echo -------------------
echo - 1. Enable IDLE  -
echo - 2. Disable IDLE -
echo -------------------
set /p menu2=:
if %menu2% EQU 1 goto e
if %menu2% EQU 2 goto d
goto home

:d
powercfg -setacvalueindex scheme_current sub_processor 5d76a2ca-e8c0-402f-a133-2158492d58ad 0 & powercfg -setactive scheme_current
timeout /t 2 /nobreak >NUL 2>&1
exit

:e
powercfg -setacvalueindex scheme_current sub_processor 5d76a2ca-e8c0-402f-a133-2158492d58ad 1 & powercfg -setactive scheme_current
timeout /t 2 /nobreak >NUL 2>&1
exit