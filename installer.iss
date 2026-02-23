[Setup]
AppId={{B8F4E2A1-7D3C-4E5F-9A2B-1C6D8E0F3A5B}
AppName=Vd Wallpaper Rotator
AppVersion=1.4.0
AppPublisher=
DefaultDirName={autopf}\VdWallpaperRotator
DefaultGroupName=Vd Wallpaper Rotator
UninstallDisplayIcon={app}\TrayApp.exe
Compression=lzma2
SolidCompression=yes
OutputDir=installer
OutputBaseFilename=VdWallpaperRotator-Setup-1.4.0
PrivilegesRequired=lowest
ArchitecturesInstallIn64BitMode=x64compatible
CloseApplications=yes
CloseApplicationsFilter=TrayApp.exe
RestartApplications=yes

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\Vd Wallpaper Rotator"; Filename: "{app}\TrayApp.exe"
Name: "{group}\Uninstall Vd Wallpaper Rotator"; Filename: "{uninstallexe}"
Name: "{userstartup}\Vd Wallpaper Rotator"; Filename: "{app}\TrayApp.exe"; Tasks: startup

[Tasks]
Name: "startup"; Description: "Start automatically when Windows starts"; GroupDescription: "Additional options:"

[Run]
Filename: "{app}\TrayApp.exe"; Description: "Launch Vd Wallpaper Rotator"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
var
  ResultCode: Integer;
begin
  // Kill running instance before upgrade
  Exec('taskkill.exe', '/F /IM TrayApp.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := True;
end;
