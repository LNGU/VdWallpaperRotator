namespace WallpaperRotator.Core;

public static class AppPaths
{
    public static string AppDataDir
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VdWallpaperRotator");

    public static string ConfigPath => Path.Combine(AppDataDir, "config.json");

    public static string StatePath => Path.Combine(AppDataDir, "state.json");

    public static string LogPath => Path.Combine(AppDataDir, "app.log");

    public static void EnsureAppDataDir() => Directory.CreateDirectory(AppDataDir);
}
