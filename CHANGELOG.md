# Changelog

All notable changes to this project will be documented in this file.

## [1.3.1] - 2026-02-23

### Changed
- Global mode now sets **different wallpapers per physical monitor** instead of the same image on all monitors

## [1.3.0] - 2026-02-23

### Added
- **Global wallpaper mode**: New "Use global wallpaper (compatibility)" option in tray menu for systems where per-virtual-desktop wallpaper doesn't work
- Better diagnostic logging with Windows build info and per-step status

### Fixed
- Removed verification check that could cause silent failures on some Windows builds
- Improved error handling in SetWallpaper

## [1.2.0] - 2026-02-23

### Fixed
- Rotation not working on fresh install due to hardcoded wallpaper folder path
- App now prompts user to select wallpaper folder on first run

### Added
- Detailed logging in rotation process for easier troubleshooting

## [1.1.0] - 2026-02-18

### Added
- Left-click tray icon to rotate wallpaper immediately
- Automated releases via GitHub Actions

### Changed
- Updated README with installation instructions

## [1.0.0] - 2026-02-18

### Added
- Initial release
- Per-virtual-desktop wallpaper rotation
- System tray application
- Configurable rotation interval
- Launch at startup option
- Windows 11 22H2+ support (build 22621+)
