[Setup]
AppName=Vd Wallpaper Rotator
AppVersion=1.2.0
AppPublisher=
DefaultDirName={autopf}\VdWallpaperRotator
DefaultGroupName=Vd Wallpaper Rotator
UninstallDisplayIcon={app}\TrayApp.exe
Compression=lzma2
SolidCompression=yes
OutputDir=installer
OutputBaseFilename=VdWallpaperRotator-Setup-1.2.0
PrivilegesRequired=lowest
ArchitecturesInstallIn64BitMode=x64compatible

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
