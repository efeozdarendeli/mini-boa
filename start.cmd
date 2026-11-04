@echo off
rem MiniBoa'yi baslatir. Cift tiklayin ya da "start.cmd -Kiosk" ile tam ekran acin.
rem -ExecutionPolicy Bypass sadece bu calistirma icindir; bilgisayarin ayarlarini degistirmez.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\start.ps1" %*
pause
