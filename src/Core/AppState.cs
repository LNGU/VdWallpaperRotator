namespace WallpaperRotator.Core;

public sealed class AppState
{
    public Dictionary<Guid, int> DesktopIndices { get; set; } = new();
}
