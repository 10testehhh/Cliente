@echo off
title Change HopLimit - HotSpot Bypass
:: https://github.com/muhamaddarulhadi/Hotspot
:home
echo -----------------------------
echo - 1) Hoplimit bypass	 -
echo - 2) Hoplimit repeater	 -
echo - 3) Default		 -
echo -----------------------------
set /p chl=Select an option: 
if %chl% EQU 1 goto 1
if %chl% EQU 2 goto 2
if %chl% EQU 3 goto 3
goto home

:1
netsh int ipv4 set glob defaultcurhoplimit=65
netsh int ipv6 set glob defaultcurhoplimit=65
exit
:2
netsh int ipv4 set glob defaultcurhoplimit=64
netsh int ipv6 set glob defaultcurhoplimit=64
exit
:3
netsh int ipv4 set glob defaultcurhoplimit=128
netsh int ipv6 set glob defaultcurhoplimit=128
exit