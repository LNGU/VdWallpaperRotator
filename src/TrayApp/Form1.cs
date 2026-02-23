using System.Diagnostics;
using Microsoft.Win32;
using WallpaperRotator.Core;
using WallpaperRotator.VirtDesktop;

namespace WallpaperRotator.TrayApp;

public partial class Form1 : Form
{
    private NotifyIcon? _tray;
    private ContextMenuStrip? _menu;
    private System.Windows.Forms.Timer? _timer;

    private AppConfig _config = new();
    private AppState _state = new();
    private WallpaperLibrary? _library;
    private RotationEngine? _engine;

    private VirtualDesktopService? _vd;

    private ToolStripMenuItem? _startItem;
    private ToolStripMenuItem? _stopItem;
    private ToolStripMenuItem? _startupItem;

    public Form1()
    {
        InitializeComponent();
        Load += OnLoad;
        Shown += (_, _) => Hide();
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        try
        {
            Logger.Info("OnLoad starting...");
            AppPaths.EnsureAppDataDir();

            _config = JsonStore.LoadOrCreate<AppConfig>(AppPaths.ConfigPath);
            _state = JsonStore.LoadOrCreate<AppState>(AppPaths.StatePath);
            Logger.Info($"Config loaded: folder={_config.WallpaperRoot}, interval={_config.IntervalSeconds}");

            _library = new WallpaperLibrary(_config);
            _library.Refresh();
            Logger.Info($"Library loaded: {_library.Wallpapers.Count} wallpapers");
            
            _engine = new RotationEngine(_config, _state, _library);

            try
            {
                _vd = new VirtualDesktopService();
                Logger.Info($"VirtualDesktopService initialized (Windows build {_vd.WindowsBuild})");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to initialize VirtualDesktopService");
                MessageBox.Show(ex.Message, "Virtual Desktop API not available", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            _timer = new System.Windows.Forms.Timer();
            _timer.Tick += (_, _) => RotateOnce();

            BuildTray();
            Logger.Info("Tray built");
            ApplyInterval();

            // Start immediately (can be changed later)
            StartRotation();
            Logger.Info("OnLoad complete, rotation started");

            // On first run, prompt user to set a wallpaper folder
            if (string.IsNullOrWhiteSpace(_config.WallpaperRoot) || !Directory.Exists(_config.WallpaperRoot))
            {
                Logger.Info("No valid wallpaper folder configured, prompting user");
                BeginInvoke(() =>
                {
                    var result = MessageBox.Show(
                        "No wallpaper folder is configured.\n\nWould you like to select a folder now?",
                        "Wallpaper Rotator Setup",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);
                    if (result == DialogResult.Yes)
                        SetWallpaperFolder();
                });
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "OnLoad failed");
            throw;
        }
    }

    private void BuildTray()
    {
        _menu = new ContextMenuStrip();

        _startItem = new ToolStripMenuItem("Start", null, (_, _) => StartRotation());
        _stopItem = new ToolStripMenuItem("Stop", null, (_, _) => StopRotation());

        var rotateNow = new ToolStripMenuItem("Rotate now", null, (_, _) => RotateOnce());

        var setFolder = new ToolStripMenuItem("Set wallpaper folder...", null, (_, _) => SetWallpaperFolder());
        var setInterval = new ToolStripMenuItem("Set interval (seconds)...", null, (_, _) => SetIntervalSeconds());

        var useGlobalMode = new ToolStripMenuItem("Use global wallpaper (compatibility)") { CheckOnClick = true };
        useGlobalMode.Checked = _config.WallpaperMode == WallpaperMode.Global;
        useGlobalMode.Click += (_, _) =>
        {
            _config.WallpaperMode = useGlobalMode.Checked ? WallpaperMode.Global : WallpaperMode.PerVirtualDesktop;
            JsonStore.Save(AppPaths.ConfigPath, _config);
            Logger.Info($"Wallpaper mode changed to: {_config.WallpaperMode}");
        };

        _startupItem = new ToolStripMenuItem("Launch at startup") { CheckOnClick = true };
        _startupItem.Checked = IsStartupEnabled();
        _startupItem.Click += (_, _) => ToggleStartup(_startupItem.Checked);

        var openLog = new ToolStripMenuItem("Open log", null, (_, _) => OpenLog());
        var exit = new ToolStripMenuItem("Exit", null, (_, _) => ExitApp());

        _menu.Items.AddRange([
            _startItem,
            _stopItem,
            new ToolStripSeparator(),
            rotateNow,
            new ToolStripSeparator(),
            setFolder,
            setInterval,
            useGlobalMode,
            new ToolStripSeparator(),
            _startupItem,
            openLog,
            new ToolStripSeparator(),
            exit,
        ]);

        // Load icon from app directory
        Icon? appIcon = null;
        var iconPath = Path.Combine(AppContext.BaseDirectory, "app.ico");
        if (File.Exists(iconPath))
        {
            try { appIcon = new Icon(iconPath); }
            catch { }
        }

        _tray = new NotifyIcon
        {
            Icon = appIcon ?? SystemIcons.Application,
            Visible = true,
            Text = "Vd Wallpaper Rotator",
            ContextMenuStrip = _menu,
        };

        _tray.MouseClick += (_, e) => { if (e.Button == MouseButtons.Left) RotateOnce(); };
    }

    private void ApplyInterval()
    {
        if (_timer == null)
            return;

        var seconds = _config.IntervalSeconds;
        if (seconds < 10)
            seconds = 10;

        _timer.Interval = checked(seconds * 1000);
    }

    private void StartRotation()
    {
        if (_timer == null)
            return;

        ApplyInterval();
        _timer.Start();
        UpdateMenuState();
    }

    private void StopRotation()
    {
        _timer?.Stop();
        UpdateMenuState();
    }

    private void UpdateMenuState()
    {
        var running = _timer?.Enabled == true;
        if (_startItem != null) _startItem.Enabled = !running;
        if (_stopItem != null) _stopItem.Enabled = running;
    }

    private void RotateOnce()
    {
        try
        {
            Logger.Info($"RotateOnce called (mode={_config.WallpaperMode})");

            if (_library == null || _engine == null)
            {
                Logger.Info($"RotateOnce skipped: library={_library != null}, engine={_engine != null}");
                return;
            }

            if (_library.Wallpapers.Count == 0)
            {
                _tray?.ShowBalloonTip(3000, "No wallpapers found", "Pick a folder with images first.", ToolTipIcon.Warning);
                return;
            }

            // Global mode: different wallpaper per physical monitor using official API
            if (_config.WallpaperMode == WallpaperMode.Global)
            {
                var monitorCount = VirtualDesktopService.GetMonitorCount();
                Logger.Info($"Global mode: {monitorCount} monitor(s) detected");

                var wallpapers = new List<string>();
                for (int i = 0; i < monitorCount; i++)
                {
                    // Use a unique GUID per monitor slot to track position in sequence
                    var monitorGuid = new Guid(i, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                    var next = _engine.NextForDesktop(monitorGuid);
                    if (next != null)
                    {
                        wallpapers.Add(next);
                        Logger.Info($"  Monitor {i}: {next}");
                    }
                }

                if (wallpapers.Count > 0)
                {
                    VirtualDesktopService.SetWallpaperGlobal(wallpapers);
                    Logger.Info("Global wallpapers set successfully");
                }

                JsonStore.Save(AppPaths.StatePath, _state);
                Logger.Info("RotateOnce completed successfully (global mode)");
                return;
            }

            // Per-virtual-desktop mode
            if (_vd == null)
            {
                Logger.Info("RotateOnce skipped: VirtualDesktopService not available");
                return;
            }

            var desktops = _vd.GetDesktops();
            Logger.Info($"Found {desktops.Count} virtual desktops");

            foreach (var d in desktops)
            {
                var next = _engine.NextForDesktop(d.Id);
                if (next == null)
                {
                    Logger.Info($"No wallpaper for desktop {d.Index}");
                    continue;
                }

                Logger.Info($"Setting wallpaper for desktop {d.Index} (id={d.Id}): {next}");
                Logger.Info($"  Current wallpaper before: {d.WallpaperPath}");
                
                try
                {
                    _vd.SetWallpaper(d.Index, next);
                    Logger.Info($"  SetWallpaper call succeeded");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"  SetWallpaper failed for desktop {d.Index}");
                    throw;
                }
            }

            JsonStore.Save(AppPaths.StatePath, _state);
            Logger.Info("RotateOnce completed successfully");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "RotateOnce failed");
            _tray?.ShowBalloonTip(5000, "Rotation failed", ex.Message, ToolTipIcon.Error);
        }
    }

    private void SetWallpaperFolder()
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "Select a root folder (subfolders will be scanned)",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false,
        };

        if (!string.IsNullOrWhiteSpace(_config.WallpaperRoot) && Directory.Exists(_config.WallpaperRoot))
            dlg.SelectedPath = _config.WallpaperRoot;

        if (dlg.ShowDialog(this) != DialogResult.OK)
            return;

        _config.WallpaperRoot = dlg.SelectedPath;
        JsonStore.Save(AppPaths.ConfigPath, _config);

        _library?.Refresh();
        RotateOnce();
    }

    private void SetIntervalSeconds()
    {
        var input = Microsoft.VisualBasic.Interaction.InputBox(
            "Enter rotation interval in seconds (min 10).",
            "Rotation interval",
            _config.IntervalSeconds.ToString());

        if (string.IsNullOrWhiteSpace(input))
            return;

        if (!int.TryParse(input, out var seconds))
        {
            MessageBox.Show("Please enter a valid integer.", "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _config.IntervalSeconds = seconds;
        JsonStore.Save(AppPaths.ConfigPath, _config);

        ApplyInterval();
    }

    private static bool IsStartupEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", writable: false);
            var val = key?.GetValue("VdWallpaperRotator") as string;
            return !string.IsNullOrWhiteSpace(val);
        }
        catch
        {
            return false;
        }
    }

    private void ToggleStartup(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            if (enable)
            {
                var exe = Application.ExecutablePath;
                key.SetValue("VdWallpaperRotator", '"' + exe + '"');
            }
            else
            {
                key.DeleteValue("VdWallpaperRotator", throwOnMissingValue: false);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ToggleStartup failed");
            MessageBox.Show(ex.Message, "Startup setting failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void OpenLog()
    {
        try
        {
            AppPaths.EnsureAppDataDir();
            if (!File.Exists(AppPaths.LogPath))
                File.WriteAllText(AppPaths.LogPath, "");
            Process.Start(new ProcessStartInfo(AppPaths.LogPath) { UseShellExecute = true });
        }
        catch { }
    }

    private void ExitApp()
    {
        _timer?.Stop();
        if (_tray != null)
        {
            _tray.Visible = false;
            _tray.Dispose();
        }
        Application.Exit();
    }
}

