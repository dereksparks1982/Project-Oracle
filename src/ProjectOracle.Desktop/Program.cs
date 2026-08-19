using Avalonia;
using ProjectOracle;

namespace ProjectOracle.Desktop;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Any(arg => arg.Equals("--version", StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine(ProjectVersion.Display);
            return 0;
        }

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"Project Oracle could not start safely: {error.GetBaseException().Message}");
            return 2;
        }
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
