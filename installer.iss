; ============================================
; TagleLabs Gestor SST - Script de Instalador
; Inno Setup Script
; ============================================
; Para compilar: Instalar Inno Setup (https://jrsoftware.org/isinfo.php)
; y abrir este archivo con el compilador de Inno Setup
; ============================================

#define MyAppName "TagleLabs Gestor SST"
#define MyAppVersion "1.0.1"
#define MyAppPublisher "TagleLabs"
#define MyAppURL "https://taglelabs.cl"
#define MyAppExeName "TagleLabsGestorSST.UI.exe"
#define MyAppAssocName "Documento TagleLabs"
#define MyAppAssocExt ".taglelabs"
#define MyAppAssocKey StringChange(MyAppAssocName, " ", "") + MyAppAssocExt

[Setup]
; Identificador unico de la aplicacion
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; Carpeta de instalacion por defecto
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}

; Permisos
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

; Archivos de salida
OutputDir=Dist
OutputBaseFilename=TagleLabsGestorSST_Setup_v{#MyAppVersion}
; SetupIconFile=TagleLabsGestorSST.UI\Assets\icon.ico

; Compresion
Compression=lzma2/ultra64
SolidCompression=yes

; Estilo visual
WizardStyle=modern

; Mostrar licencia y readme (descomentar si existen los archivos)
; LicenseFile=LegalDocs\LICENCIA.txt
; InfoBeforeFile=LEEME.txt

; Permitir al usuario elegir directorio
DisableDirPage=no
DisableProgramGroupPage=yes

; Desinstalador
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "Crear acceso directo en el Escritorio"; GroupDescription: "Accesos directos:"

[Files]
; Todos los archivos de la aplicacion publicada
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Plantillas
Source: "Templates\*"; DestDir: "{app}\Templates"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist

[Icons]
; Menu Inicio
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "Gestion de Seguridad y Salud en el Trabajo"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

; Escritorio (si el usuario lo eligio)
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; Comment: "Gestion de Seguridad y Salud en el Trabajo"

[Run]
; Ejecutar la aplicacion al finalizar (opcional)
Filename: "{app}\{#MyAppExeName}"; Description: "Ejecutar {#MyAppName} ahora"; Flags: nowait postinstall skipifsilent shellexec

[Code]
// Ruta de datos del usuario en AppData
function GetAppDataFolder(): String;
begin
  Result := ExpandConstant('{localappdata}\TagleLabsGestorSST');
end;

// Verificar si hay una version anterior instalada
function InitializeSetup(): Boolean;
begin
  Result := True;
end;

// Mostrar mensaje de bienvenida personalizado
procedure InitializeWizard();
begin
  WizardForm.WelcomeLabel1.Caption := 'Bienvenido al instalador de TagleLabs Gestor SST';
  WizardForm.WelcomeLabel2.Caption := 
    'Este asistente le guiara en la instalacion de TagleLabs Gestor SST en su computador.' + #13#10 + #13#10 +
    'TagleLabs Gestor SST es un software de gestion de Seguridad y Salud en el Trabajo, ' +
    'disenado para cumplir con la normativa chilena vigente (DS44, Ley Karin 21.643).' + #13#10 + #13#10 +
    'Caracteristicas principales:' + #13#10 +
    '- Gestion de empresas y trabajadores' + #13#10 +
    '- Matriz de Riesgos (MIPER) con IA' + #13#10 +
    '- Generacion automatica de documentos' + #13#10 +
    '- Cumplimiento normativo DS44' + #13#10 + #13#10 +
    'Haga clic en Siguiente para continuar.';
end;

// DESINSTALACION: Preguntar si borrar datos del usuario
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  AppDataPath: String;
  DeleteData: Integer;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    AppDataPath := GetAppDataFolder();
    
    // Solo preguntar si la carpeta existe
    if DirExists(AppDataPath) then
    begin
      DeleteData := MsgBox(
        'Se ha desinstalado TagleLabs Gestor SST.' + #13#10 + #13#10 +
        'Desea ELIMINAR tambien los datos guardados?' + #13#10 + #13#10 +
        'Esto incluye:' + #13#10 +
        '- Base de datos (empresas, trabajadores, riesgos)' + #13#10 +
        '- Documentos generados' + #13#10 +
        '- Plantillas personalizadas' + #13#10 + #13#10 +
        'Ubicacion: ' + AppDataPath + #13#10 + #13#10 +
        'ADVERTENCIA: Esta accion NO se puede deshacer.',
        mbConfirmation, MB_YESNO);
      
      if DeleteData = IDYES then
      begin
        // Borrar toda la carpeta de datos
        DelTree(AppDataPath, True, True, True);
        MsgBox('Los datos han sido eliminados correctamente.', mbInformation, MB_OK);
      end
      else
      begin
        MsgBox(
          'Los datos se han conservado en:' + #13#10 + 
          AppDataPath + #13#10 + #13#10 +
          'Puede eliminarlos manualmente si lo desea.',
          mbInformation, MB_OK);
      end;
    end;
  end;
end;

[UninstallDelete]
; Solo limpiar archivos de la carpeta de instalacion (no AppData)
Type: filesandordirs; Name: "{app}\Outputs"
Type: filesandordirs; Name: "{app}\Templates"
