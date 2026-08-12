using ProjectOracle.Simulation;

namespace ProjectOracle.ConsoleApp;

/// <summary>
/// Owns the single reserved top terminal row used for in-world Time.
/// The conversation body scrolls beneath row 1. Clock refreshes save and restore
/// the input cursor so the live header never writes into the command buffer.
/// </summary>
public sealed class LiveWorldClockSurface : IDisposable
{
    private const string SaveCursor = "\u001b[s";
    private const string RestoreCursor = "\u001b[u";
    private const string ClearLine = "\u001b[2K";
    private const string ResetScrollRegion = "\u001b[r";

    private int _terminalHeight;
    private string? _lastHeader;
    private bool _active;
    private bool _disposed;

    public bool Active => _active;
    public static bool WritesToConversationBody => false;
    public static string PreTimeHeader => "In-world Time: Gaia has not yet created Time.";

    public void Begin(OracleSimulation simulation)
    {
        ArgumentNullException.ThrowIfNull(simulation);
        ThrowIfDisposed();

        if (!SupportsAnchoredHeader())
        {
            return;
        }

        _terminalHeight = Math.Max(3, SafeWindowHeight());
        _active = true;

        System.Console.Write("\u001b[2J\u001b[H");
        System.Console.Write($"\u001b[2;{_terminalHeight}r");
        System.Console.Write("\u001b[2;1H");
        Refresh(simulation, force: true);
    }

    public void Refresh(OracleSimulation simulation, bool force = false)
    {
        ArgumentNullException.ThrowIfNull(simulation);
        ThrowIfDisposed();

        string header = Describe(simulation);
        if (!force && header.Equals(_lastHeader, StringComparison.Ordinal))
        {
            return;
        }
        _lastHeader = header;

        if (!_active)
        {
            return;
        }

        System.Console.Write(SaveCursor);
        System.Console.Write("\u001b[1;1H");
        System.Console.Write(ClearLine);
        ConsoleTheme.WriteWorldTime(header);
        System.Console.Write(RestoreCursor);
    }

    public static string Describe(OracleSimulation simulation)
    {
        ArgumentNullException.ThrowIfNull(simulation);
        return simulation.InWorldTimeExists
            ? $"In-world Time: {simulation.Clock.Describe()}"
            : PreTimeHeader;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_active)
        {
            System.Console.Write(ResetScrollRegion);
            _active = false;
        }
    }

    private static bool SupportsAnchoredHeader()
    {
        if (System.Console.IsOutputRedirected) return false;
        string? term = Environment.GetEnvironmentVariable("TERM");
        return !string.Equals(term, "dumb", StringComparison.OrdinalIgnoreCase);
    }

    private static int SafeWindowHeight()
    {
        try
        {
            return System.Console.WindowHeight;
        }
        catch
        {
            return 24;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(LiveWorldClockSurface));
    }
}
