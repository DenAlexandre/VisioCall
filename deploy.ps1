# VisioCall - Script de deploiement Android
# Usage: .\deploy.ps1 [-Release]

param(
    [switch]$Release
)

$ErrorActionPreference = "Stop"

$ADB = "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe"
$ProjectDir = "D:\GitHub\VisioCall"
$MauiProject = "VisioCall.Maui"
$ApkName = "VisioCall.apk"
$PhoneDest = "/sdcard/Download"

$Config = if ($Release) { "Release" } else { "Debug" }

Write-Host ""
Write-Host "===============================" -ForegroundColor Cyan
Write-Host "  VisioCall - Deploiement Android"
Write-Host "  Configuration: $Config"
Write-Host "===============================" -ForegroundColor Cyan
Write-Host ""

# 1. Build
Write-Host "[1/4] Build en cours..." -ForegroundColor Yellow
Set-Location $ProjectDir
dotnet publish $MauiProject -f net10.0-android -c $Config -p:EmbedAssembliesIntoApk=true --nologo -v quiet
if ($LASTEXITCODE -ne 0) { Write-Host "      Build ECHOUE !" -ForegroundColor Red; exit 1 }
Write-Host "      Build OK" -ForegroundColor Green

# 2. Renommer l'APK
Write-Host "[2/4] Renommage de l'APK..." -ForegroundColor Yellow
$ApkSource = Join-Path $ProjectDir "$MauiProject\bin\$Config\net10.0-android\publish\com.companyname.visiocall-Signed.apk"
$ApkOutput = Join-Path $ProjectDir $ApkName
Copy-Item $ApkSource $ApkOutput -Force
$Size = [math]::Round((Get-Item $ApkOutput).Length / 1MB, 1)
Write-Host "      $ApkName (${Size} Mo)" -ForegroundColor Green

# 3. Verifier le telephone
Write-Host "[3/4] Detection du telephone..." -ForegroundColor Yellow
$Devices = & $ADB devices 2>&1 | Select-String "device$"
if (-not $Devices) {
    Write-Host "      Aucun telephone detecte !" -ForegroundColor Red
    Write-Host "      L'APK est disponible ici : $ApkOutput"
    exit 1
}
$DeviceId = ($Devices -split "`t")[0]
Write-Host "      Telephone detecte : $DeviceId" -ForegroundColor Green

# 4. Copier sur le telephone
Write-Host "[4/4] Copie sur le telephone..." -ForegroundColor Yellow
& $ADB push $ApkOutput "$PhoneDest/$ApkName" 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) { Write-Host "      Copie ECHOUEE !" -ForegroundColor Red; exit 1 }
Write-Host "      Copie OK" -ForegroundColor Green

Write-Host ""
Write-Host "===============================" -ForegroundColor Cyan
Write-Host "  Deploiement termine !"
Write-Host "  -> Ouvrir Fichiers > Telechargements"
Write-Host "  -> Installer $ApkName"
Write-Host "===============================" -ForegroundColor Cyan
