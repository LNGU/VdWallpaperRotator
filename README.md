# What's New
v1.4.0 - Clearer rotation modes: Physical Monitors vs Virtual Desktops (no more confusing hybrid)

# Vd Wallpaper Rotator

WinForms tray app that rotates wallpapers on **Windows 11 (22H2+ / build 22621+)**.

## Install

Download and run the latest `VdWallpaperRotator-Setup-x.x.x.exe` from the [Releases](../../releases) page.

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

Output: `installer\VdWallpaperRotator-Setup-x.x.x.exe`

## Use
- Right-click the tray icon to:
  - set the wallpaper folder (subfolders are scanned)
  - set the rotation interval (seconds)
  - enable/disable launch at startup
  - select rotation mode (see below)
- Left-click the tray icon to rotate immediately.

### Rotation Modes

| Mode | Description | Best For |
|------|-------------|----------|
| **Physical Monitors** (default) | Different wallpaper on each monitor. Same wallpaper stays when switching virtual desktops. Uses official Windows API (reliable). | Multi-monitor setups |
| **Virtual Desktops** | Different wallpaper on each virtual desktop. Same wallpaper on all monitors within each VD. Uses undocumented API (may break on some Windows builds). | Single monitor with multiple VDs |

Select your mode from the tray menu:
- ✓ Mode: Physical monitors
- ✓ Mode: Virtual desktops

**Note:** You can only use one mode at a time. Windows doesn't support per-monitor AND per-virtual-desktop wallpapers simultaneously.

## Config files
Stored under:
- `%AppData%\VdWallpaperRotator\config.json`
- `%AppData%\VdWallpaperRotator\state.json`
- `%AppData%\VdWallpaperRotator\app.log` (for troubleshooting)

## Startup
The "Launch at startup" toggle uses:
- `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` (value: `VdWallpaperRotator`)

## Notes
- Virtual Desktops mode uses **undocumented Windows COM interfaces** (dependency-free, but can break after Windows updates).
- If Virtual Desktops mode stops working after a Windows update, switch to Physical Monitors mode.
