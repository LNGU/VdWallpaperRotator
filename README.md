# Vd Wallpaper Rotator

WinForms tray app that rotates **per-virtual-desktop** wallpapers on **Windows 11 (22H2+ / build 22621+)**.

## Install

Download and run `VdWallpaperRotator-Setup-1.0.0.exe` from the [Releases](../../releases) page.

The installer will:
- Install to `%LocalAppData%\Programs\VdWallpaperRotator`
- Optionally add to Windows startup
- Launch the app after installation

## Build from source

```powershell
cd C:\projects\wallpaperRotator
dotnet build .\WallpaperRotator.sln -c Release
dotnet run --project .\src\TrayApp\TrayApp.csproj
```

### Create installer

Requires [Inno Setup 6](https://jrsoftware.org/isinfo.php).

```powershell
dotnet publish .\src\TrayApp\TrayApp.csproj -c Release -r win-x64 --self-contained -o publish
iscc installer.iss
```

Output: `installer\VdWallpaperRotator-Setup-1.0.0.exe`

## Use
- Right-click the tray icon to:
  - set the wallpaper folder (subfolders are scanned)
  - set the rotation interval (seconds)
  - enable/disable launch at startup
- Double-click the tray icon to rotate immediately.

## Config files
Stored under:
- `%AppData%\VdWallpaperRotator\config.json`
- `%AppData%\VdWallpaperRotator\state.json`

## Startup
The “Launch at startup” toggle uses:
- `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` (value: `VdWallpaperRotator`)

## Notes
- Per-desktop wallpaper is implemented via **undocumented Windows COM interfaces** (dependency-free, but can break after Windows updates).

