@echo off
echo Optimizing Taskbar...



del /f /q "%localappdata%\Microsoft\Windows\Explorer\iconcache*"
del /f /q "%localappdata%\Microsoft\Windows\Explorer\thumbcache*"

:: Enable Transparency Effects (Maximum Transparency)
echo Enabling Taskbar Transparency (Maximized)...
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize" /v "EnableTransparency" /t REG_DWORD /d 1 /f
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize" /v "TransparencyEffect" /t REG_DWORD /d 1 /f

:: Enable or Ensure Full Transparency on Taskbar (Ensure taskbar transparency effect)
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize" /v "EnableBlurBehind" /t REG_DWORD /d 1 /f

:: Restart Windows Explorer
echo Restarting Windows Explorer...
start explorer.exe

:: Final Message
echo Taskbar optimization and maximum transparency enabled.
pause
