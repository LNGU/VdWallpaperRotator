namespace WallpaperRotator.Core;

public sealed class RotationEngine
{
    private readonly AppConfig _config;
    private readonly AppState _state;
    private readonly WallpaperLibrary _library;
    private readonly Random _rng = new();

    public RotationEngine(AppConfig config, AppState state, WallpaperLibrary library)
    {
        _config = config;
        _state = state;
        _library = library;
    }

    public string? NextForDesktop(Guid desktopId)
    {
        var list = _library.Wallpapers;
        if (list.Count == 0)
            return null;

        if (_config.Mode == RotationMode.Random)
        {
            var idx = _rng.Next(list.Count);
            _state.DesktopIndices[desktopId] = idx;
            return list[idx];
        }

        _state.DesktopIndices.TryGetValue(desktopId, out var current);
        var next = current + 1;
        if (next >= list.Count)
            next = 0;

        _state.DesktopIndices[desktopId] = next;
        return list[next];
    }
}
