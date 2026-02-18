namespace WallpaperRotator.Core;

public sealed class WallpaperLibrary
{
    private readonly AppConfig _config;

    public WallpaperLibrary(AppConfig config)
    {
        _config = config;
    }

    public IReadOnlyList<string> Wallpapers { get; private set; } = Array.Empty<string>();

    public void Refresh()
    {
        if (string.IsNullOrWhiteSpace(_config.WallpaperRoot) || !Directory.Exists(_config.WallpaperRoot))
        {
            Wallpapers = Array.Empty<string>();
            return;
        }

        var allowed = new HashSet<string>(_config.Extensions.Select(e => e.ToLowerInvariant()));

        var files = Directory.EnumerateFiles(_config.WallpaperRoot, "*.*", SearchOption.AllDirectories)
            .Where(p => allowed.Contains(Path.GetExtension(p).ToLowerInvariant()))
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Wallpapers = files;
    }
}
