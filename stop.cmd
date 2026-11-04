@echo off
rem MiniBoa'yi kapatir. "stop.cmd -ResetData" demo verisini de sifirlar.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\stop.ps1" %*
pause
