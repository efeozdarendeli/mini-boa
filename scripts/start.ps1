<#
.SYNOPSIS
    MiniBoa'yı tek komutla başlatır: veritabanı → API → arayüz → tarayıcı.

.DESCRIPTION
    Proje kökündeki start.cmd'ye çift tıklayarak çalıştırılır (PowerShell betik izni gerekmez).
    API ve arayüz ayrı pencerelerde açılır; kapatmak için stop.cmd veya o pencerelerde Ctrl+C.

.PARAMETER Kiosk
    Tarayıcıyı adres çubuğu olmadan tam ekran (self-servis cihazı gibi) açar. Çıkmak için Alt+F4.
#>
param([switch]$Kiosk)

# Not: $ErrorActionPreference = 'Stop' kullanılmıyor. Windows PowerShell 5.1, docker gibi programların
# bilgi mesajlarını (stderr) çıktı yönlendirildiğinde hata sanıp script'i durdurabiliyor.
# Bunun yerine her adım kendi sonucunu ($LASTEXITCODE, Test-Url) kontrol eder ve gerekirse throw eder.
$root = Split-Path -Parent $PSScriptRoot
$apiUrl = 'http://localhost:5080/health'
$webUrl = 'http://localhost:5173'

function Wait-Until([string]$description, [int]$timeoutSeconds, [scriptblock]$condition) {
    Write-Host "    $description bekleniyor" -NoNewline
    for ($i = 0; $i -lt $timeoutSeconds; $i++) {
        if (& $condition) { Write-Host ' hazır.' -ForegroundColor Green; return }
        Write-Host '.' -NoNewline
        Start-Sleep -Seconds 1
    }
    throw "$description $timeoutSeconds saniye içinde hazır olmadı."
}

function Test-Url([string]$url) {
    try { (Invoke-WebRequest $url -UseBasicParsing -TimeoutSec 2).StatusCode -eq 200 } catch { $false }
}

function Test-Docker {
    docker info *> $null
    $LASTEXITCODE -eq 0
}

Write-Host '[1/4] Docker' -ForegroundColor Cyan
if (-not (Test-Docker)) {
    $dockerDesktop = 'C:\Program Files\Docker\Docker\Docker Desktop.exe'
    if (-not (Test-Path $dockerDesktop)) { throw 'Docker Desktop bulunamadı. Lütfen kurup tekrar deneyin.' }
    Start-Process $dockerDesktop
    Wait-Until 'Docker' 180 { Test-Docker }
}

Write-Host '[2/4] Veritabanı (PostgreSQL, port 5433)' -ForegroundColor Cyan
docker compose -f "$root\docker-compose.yml" up -d --wait
if ($LASTEXITCODE -ne 0) { throw 'Veritabanı başlatılamadı.' }

Write-Host '[3/4] API (http://localhost:5080)' -ForegroundColor Cyan
if (-not (Test-Url $apiUrl)) {
    Start-Process powershell -ArgumentList '-NoExit', '-Command',
        "`$host.UI.RawUI.WindowTitle = 'MiniBoa API'; Set-Location '$root\backend'; dotnet run --project MiniBoa.Communication"
    Wait-Until 'API' 120 { Test-Url $apiUrl }
}

Write-Host '[4/4] Arayüz (http://localhost:5173)' -ForegroundColor Cyan
if (-not (Test-Url $webUrl)) {
    # npm.cmd: bazı bilgisayarlarda PowerShell betik izni npm.ps1'i engeller; .cmd sürümü etkilenmez.
    Start-Process powershell -ArgumentList '-NoExit', '-Command',
        "`$host.UI.RawUI.WindowTitle = 'MiniBoa Arayüz'; Set-Location '$root\frontend'; if (-not (Test-Path node_modules)) { npm.cmd install }; npm.cmd run dev"
    Wait-Until 'Arayüz' 180 { Test-Url $webUrl }
}

if ($Kiosk) {
    Start-Process msedge -ArgumentList "--kiosk $webUrl --edge-kiosk-type=fullscreen --no-first-run"
} else {
    Start-Process $webUrl
}

Write-Host ''
Write-Host "MiniBoa hazır: $webUrl  (Demo: 12345678950 / 123456)" -ForegroundColor Green
Write-Host 'Kapatmak için: stop.cmd'
