namespace WallpaperRotator.Core;

public static class Logger
{
    private static readonly object Gate = new();

    public static void Info(string message) => Write("INFO", message);

    public static void Error(string message) => Write("ERROR", message);

    public static void Error(Exception ex, string message) => Write("ERROR", $"{message}\n{ex}");

    private static void Write(string level, string message)
    {
        try
        {
            AppPaths.EnsureAppDataDir();
            var line = $"{DateTimeOffset.Now:u} [{level}] {message}{Environment.NewLine}";
            lock (Gate)
                File.AppendAllText(AppPaths.LogPath, line);
        }
        catch
        {
            // ignore logging failures
        }
    }
}
