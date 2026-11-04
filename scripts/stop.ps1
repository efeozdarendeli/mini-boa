<#
.SYNOPSIS
    MiniBoa'yı kapatır: arayüz ve API durdurulur, veritabanı konteyneri durdurulur (veri silinmez).

.PARAMETER ResetData
    Veritabanını da siler; bir sonraki başlatmada demo verisi (seed) baştan yüklenir.
#>
param([switch]$ResetData)

$root = Split-Path -Parent $PSScriptRoot

foreach ($port in 5173, 5080) {
    $owners = Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue |
        Select-Object -ExpandProperty OwningProcess -Unique
    foreach ($processId in $owners) {
        $name = (Get-Process -Id $processId -ErrorAction SilentlyContinue).ProcessName
        Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
        Write-Host "Port $port durduruldu ($name)."
    }
}

docker info *> $null
if ($LASTEXITCODE -eq 0) {
    if ($ResetData) {
        docker compose -f "$root\docker-compose.yml" down -v
        Write-Host 'Veritabanı silindi; bir sonraki başlatmada demo verisi yeniden yüklenecek.'
    } else {
        docker compose -f "$root\docker-compose.yml" stop
        Write-Host 'Veritabanı durduruldu (veriler korunuyor).'
    }
} else {
    Write-Host 'Docker çalışmıyor; veritabanı zaten kapalı.'
}
