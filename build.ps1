$dotnet = $null
$cmd = Get-Command dotnet -ErrorAction SilentlyContinue
if ($cmd) {
    $dotnet = $cmd.Source
}
if (-not $dotnet) {
    $fallback = "C:\Program Files\dotnet\dotnet.exe"
    if (Test-Path $fallback) { $dotnet = $fallback }
}
if (-not $dotnet) {
    Write-Error "No se encontró el ejecutable dotnet. Instala .NET 8 SDK y agrega a PATH."
    exit 1
}

Write-Host "Usando dotnet en: $dotnet" -ForegroundColor Yellow

Write-Host "Restaurando paquetes..." -ForegroundColor Cyan
& $dotnet restore

Write-Host "Compilando solución..." -ForegroundColor Cyan
& $dotnet build TagleLabsGestorSST.sln -c Debug

Write-Host "Ejecutando pruebas..." -ForegroundColor Cyan
& $dotnet test TagleLabsGestorSST.Tests/TagleLabsGestorSST.Tests.csproj

Write-Host "Listo. Para ejecutar la app:" -ForegroundColor Green
Write-Host "$dotnet run --project TagleLabsGestorSST.UI"
