@echo off
title TagleLabs Gestor SST
color 0A
echo ==========================================
echo      INICIANDO TAGLELABS GESTOR SST
echo ==========================================
echo.
echo Compilando y ejecutando la aplicacion...
echo.
cd /d "%~dp0"
dotnet run --project "TagleLabsGestorSST.UI\TagleLabsGestorSST.UI.csproj"
if %errorlevel% neq 0 (
    color 0C
    echo.
    echo [ERROR] Ocurrio un error al iniciar la aplicacion.
    pause
)
