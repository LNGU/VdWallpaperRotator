using System.Text.Json;

namespace WallpaperRotator.Core;

public static class JsonStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    public static T LoadOrCreate<T>(string path) where T : new()
    {
        try
        {
            if (!File.Exists(path))
                return new T();

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json, Options) ?? new T();
        }
        catch
        {
            return new T();
        }
    }

    public static void Save<T>(string path, T value)
    {
        AppPaths.EnsureAppDataDir();
        var json = JsonSerializer.Serialize(value, Options);
        File.WriteAllText(path, json);
    }
}
