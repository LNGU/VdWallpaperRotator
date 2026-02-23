# What's New
v1.3.3 - Global mode now rotates ALL monitors AND virtual desktops with unique wallpapers

# Vd Wallpaper Rotator

WinForms tray app that rotates **per-virtual-desktop** wallpapers on **Windows 11 (22H2+ / build 22621+)**.

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
  - toggle compatibility mode (see below)
- Left-click the tray icon to rotate immediately.

### Compatibility Mode
If wallpaper rotation doesn't work on your system (logs show success but wallpaper doesn't change), enable **"Use global wallpaper (compatibility)"** in the tray menu.

| Mode | Description |
|------|-------------|
| **Per-Virtual-Desktop** (default) | Different wallpaper on each virtual desktop. Uses undocumented Windows API. |
| **Global** (compatibility) | Different wallpaper per physical monitor AND per virtual desktop. Uses hybrid approach with official API for reliable visual updates. |

**When to use compatibility mode:**
- Wallpaper doesn't change despite successful log entries
- You have multiple physical monitors and per-VD mode doesn't work
- You're on a very recent Windows 11 build (26100+) where the undocumented API may have changed

## Config files
Stored under:
- `%AppData%\VdWallpaperRotator\config.json`
- `%AppData%\VdWallpaperRotator\state.json`
- `%AppData%\VdWallpaperRotator\app.log` (for troubleshooting)

## Startup
The "Launch at startup" toggle uses:
- `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` (value: `VdWallpaperRotator`)

## Notes
- Per-desktop wallpaper is implemented via **undocumented Windows COM interfaces** (dependency-free, but can break after Windows updates).
- If per-VD mode stops working after a Windows update, use compatibility mode as a fallback.
