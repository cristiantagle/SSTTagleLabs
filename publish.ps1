# ============================================
# TagleLabs Gestor SST - Script de Publicacion
# ============================================
# Este script crea un paquete de distribucion self-contained
# que no requiere .NET runtime instalado en el equipo destino.
# ============================================

param(
    [string]$Version = "1.0.0",
    [string]$OutputDir = "Dist"
)

$ErrorActionPreference = "Stop"

# Colores para output
function Write-Step { param($msg) Write-Host ">>> $msg" -ForegroundColor Cyan }
function Write-Ok { param($msg) Write-Host "[OK] $msg" -ForegroundColor Green }
function Write-Err { param($msg) Write-Host "[ERROR] $msg" -ForegroundColor Red }

Write-Host ""
Write-Host "============================================" -ForegroundColor Magenta
Write-Host "  TagleLabs Gestor SST - Publicacion v$Version" -ForegroundColor Magenta
Write-Host "============================================" -ForegroundColor Magenta
Write-Host ""

# 1. Verificar dotnet
Write-Step "Verificando .NET SDK..."
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Err "No se encontro dotnet. Instala .NET 8 SDK."
    exit 1
}
Write-Ok "dotnet encontrado: $($dotnet.Source)"

# 2. Limpiar carpeta de salida
$distPath = Join-Path $PSScriptRoot $OutputDir
$appPath = Join-Path $distPath "TagleLabsGestorSST"

Write-Step "Limpiando carpeta de distribucion..."
if (Test-Path $distPath) {
    Remove-Item $distPath -Recurse -Force
}
New-Item -ItemType Directory -Path $appPath -Force | Out-Null
Write-Ok "Carpeta $OutputDir creada"

# 3. Build en Release
Write-Step "Compilando en modo Release..."
& dotnet build TagleLabsGestorSST.sln -c Release --nologo -v q
if ($LASTEXITCODE -ne 0) {
    Write-Err "Error en la compilacion"
    exit 1
}
Write-Ok "Compilacion exitosa"

# 4. Publish self-contained
Write-Step "Publicando aplicacion self-contained (Windows x64)..."
& dotnet publish TagleLabsGestorSST.UI/TagleLabsGestorSST.UI.csproj -c Release -r win-x64 --self-contained true -o $appPath --nologo -v q /p:PublishSingleFile=false /p:IncludeNativeLibrariesForSelfExtract=true

if ($LASTEXITCODE -ne 0) {
    Write-Err "Error en la publicacion"
    exit 1
}
Write-Ok "Publicacion completada"

# 5. Copiar archivos adicionales
Write-Step "Copiando archivos adicionales..."

# Templates
$templatesSource = Join-Path $PSScriptRoot "Templates"
$templatesDest = Join-Path $appPath "Templates"
if (Test-Path $templatesSource) {
    Copy-Item $templatesSource $templatesDest -Recurse -Force
    Write-Ok "Templates copiados"
}

# gemini.key (si existe en el proyecto UI)
$keySource = Join-Path $PSScriptRoot "TagleLabsGestorSST.UI\gemini.key"
if (Test-Path $keySource) {
    Copy-Item $keySource $appPath -Force
    Write-Ok "gemini.key copiado"
}

# Crear carpeta Outputs
$outputsPath = Join-Path $appPath "Outputs"
New-Item -ItemType Directory -Path $outputsPath -Force | Out-Null
Write-Ok "Carpeta Outputs creada"

# Copiar LEEME.txt
$leemeSource = Join-Path $PSScriptRoot "LEEME.txt"
if (Test-Path $leemeSource) {
    Copy-Item $leemeSource $appPath -Force
    Write-Ok "LEEME.txt copiado"
}

# 6. Crear archivo ZIP
Write-Step "Creando archivo ZIP..."
$zipName = "TagleLabsGestorSST_v$Version.zip"
$zipPath = Join-Path $distPath $zipName

if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Compress-Archive -Path $appPath -DestinationPath $zipPath -CompressionLevel Optimal
Write-Ok "ZIP creado: $zipName"

# 7. Resumen
Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  Publicacion completada exitosamente!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Archivos generados en: $distPath" -ForegroundColor Yellow
Write-Host ""
Write-Host "  [FOLDER] TagleLabsGestorSST/  (carpeta descomprimida)"
Write-Host "  [ZIP] $zipName   (paquete comprimido)"
Write-Host ""

# Mostrar tamano
$zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
Write-Host "Tamano del ZIP: $zipSize MB" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para distribuir, comparte el archivo ZIP." -ForegroundColor Gray
Write-Host "El usuario solo debe descomprimir y ejecutar TagleLabsGestorSST.UI.exe" -ForegroundColor Gray
Write-Host ""
