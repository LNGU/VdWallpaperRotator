using System.Text.Json.Serialization;

namespace WallpaperRotator.Core;

public enum RotationMode
{
    Sequential = 0,
    Random = 1,
}

public enum WallpaperMode
{
    /// <summary>Different wallpaper per virtual desktop (undocumented API, may not work on all Windows builds)</summary>
    PerVirtualDesktop = 0,
    /// <summary>Same wallpaper across all virtual desktops (official API, more reliable)</summary>
    Global = 1,
}

public sealed class AppConfig
{
    public string WallpaperRoot { get; set; } = "";

    public int IntervalSeconds { get; set; } = 60;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RotationMode Mode { get; set; } = RotationMode.Sequential;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WallpaperMode WallpaperMode { get; set; } = WallpaperMode.PerVirtualDesktop;

    public string[] Extensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp", ".bmp"];
}
