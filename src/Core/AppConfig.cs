using System.Text.Json.Serialization;

namespace WallpaperRotator.Core;

public enum RotationMode
{
    Sequential = 0,
    Random = 1,
}

public sealed class AppConfig
{
    public string WallpaperRoot { get; set; } = @"C:\\Users\\lngu1\\OneDrive\\wallpapers";

    public int IntervalSeconds { get; set; } = 60;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RotationMode Mode { get; set; } = RotationMode.Sequential;

    public string[] Extensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp", ".bmp"];
}
