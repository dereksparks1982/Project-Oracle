using System.Text.Json;
using Avalonia;
using Avalonia.Controls;

namespace ProjectOracle.Desktop;

internal static class OracleWindowPlacementStore
{
    private sealed record Placement(double Width, double Height, int X, int Y);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    public static void Save(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        if (window.WindowState == WindowState.Minimized) return;
        if (!double.IsFinite(window.Width) || !double.IsFinite(window.Height)) return;
        if (window.Width <= 0 || window.Height <= 0) return;

        try
        {
            string path = PathForSettings();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            Placement placement = new(window.Width, window.Height, window.Position.X, window.Position.Y);
            File.WriteAllText(path, JsonSerializer.Serialize(placement, JsonOptions));
        }
        catch
        {
            // Window placement is convenience state. A failure must never stop a save or app shutdown.
        }
    }

    public static bool TryRestore(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        try
        {
            string path = PathForSettings();
            if (!File.Exists(path)) return false;
            Placement? placement = JsonSerializer.Deserialize<Placement>(File.ReadAllText(path), JsonOptions);
            if (placement is null || placement.Width <= 0 || placement.Height <= 0) return false;
            window.Width = placement.Width;
            window.Height = placement.Height;
            window.Position = new PixelPoint(placement.X, placement.Y);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string PathForSettings()
    {
        string localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localData)) localData = AppContext.BaseDirectory;
        return Path.Combine(localData, "ProjectOracle", "window-placement.json");
    }
}
