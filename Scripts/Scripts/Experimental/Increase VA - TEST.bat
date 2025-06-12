@echo off
title Increase User VA
color a
:home
cls
echo.
echo ------------------
echo - 1. Increase VA -
echo - 2. Default VA  -
echo ------------------
echo.
set /p menu=:
if %menu% EQU 1 goto e
if %menu% EQU 2 goto d
goto home

:e
bcdedit /set IncreaseUserVA 3072
exit

:d
bcdedit /deletevalue IncreaseUserVA
exit




