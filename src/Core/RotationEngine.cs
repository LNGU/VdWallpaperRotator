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

    /// <summary>
    /// Gets multiple unique wallpapers for multi-monitor setups.
    /// Each monitor gets a different wallpaper.
    /// </summary>
    public IReadOnlyList<string> NextForMonitors(int monitorCount)
    {
        return NextUniqueWallpapers(monitorCount);
    }

    /// <summary>
    /// Gets a specific number of unique wallpapers for any use case.
    /// </summary>
    public IReadOnlyList<string> NextUniqueWallpapers(int count)
    {
        var list = _library.Wallpapers;
        if (list.Count == 0)
            return Array.Empty<string>();

        var result = new List<string>(count);

        if (_config.Mode == RotationMode.Random)
        {
            // Pick random unique indices (or allow repeats if fewer wallpapers than needed)
            var usedIndices = new HashSet<int>();
            for (int i = 0; i < count; i++)
            {
                int idx;
                if (usedIndices.Count < list.Count)
                {
                    // We can still pick unique wallpapers
                    do { idx = _rng.Next(list.Count); }
                    while (usedIndices.Contains(idx));
                    usedIndices.Add(idx);
                }
                else
                {
                    // More slots than wallpapers, allow repeats
                    idx = _rng.Next(list.Count);
                }
                result.Add(list[idx]);
            }
        }
        else
        {
            // Sequential: get consecutive wallpapers starting from saved position
            _state.DesktopIndices.TryGetValue(Guid.Empty, out var startIndex);
            
            for (int i = 0; i < count; i++)
            {
                var idx = (startIndex + i) % list.Count;
                result.Add(list[idx]);
            }
            
            // Save the next starting position
            _state.DesktopIndices[Guid.Empty] = (startIndex + count) % list.Count;
        }

        return result;
    }
}
