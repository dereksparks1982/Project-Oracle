namespace ProjectOracle.ConsoleApp;

internal static class ConsoleTheme
{
    private const string Reset = "\u001b[0m";
    private const string Base = "\u001b[40m\u001b[38;5;46m";
    private const string Dim = "\u001b[38;5;35m";
    private const string Live = "\u001b[38;5;118m";
    private const string Prompt = "\u001b[38;5;220m";
    private const string Power = "\u001b[38;5;51m";
    private const string Record = "\u001b[38;5;159m";
    private const string Command = "\u001b[38;5;83m";

    public static bool Enabled => !System.Console.IsOutputRedirected && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("NO_COLOR"));
    public static void ApplyBase() { if (Enabled) System.Console.Write(Base); }
    public static void ResetToShell() { if (Enabled) System.Console.Write(Reset); }
    public static void WriteLine(string value = "") => System.Console.WriteLine(Enabled ? Colorise(value) : value);
    public static void WritePrompt(string value) => System.Console.Write(Enabled ? $"{Prompt}{value}{Base}" : value);
    public static void WriteWorldTime(string value) => System.Console.Write(Enabled ? $"{Live}{value}{Base}" : value);
    public static string LiveLine(string value) => Enabled ? $"{Live}{Colorise(value)}{Base}" : value;
    public static string ClearLine => Enabled ? $"{Dim}\u001b[2K{Base}" : "\u001b[2K";

    private static string Colorise(string value)
    {
        string output = value;
        foreach ((string token, string colour) in new[]
        {
            ("WORLD RECORD", Record), ("ORACLE RECORD", Record), ("World Record", Record), ("Oracle Record", Record),
            ("LIVE", Live), ("Monad", Power), ("Wisdom", Power), ("Yala", Power), ("Gaia", Power),
            ("Terra", Power), ("Aether", Power), ("Sol", Power), ("Thalassa", Power), ("Luna", Power),
            ("status", Command), ("creation", Command), ("records world", Command), ("records oracle", Command)
        })
        {
            output = output.Replace(token, $"{colour}{token}{Base}", StringComparison.Ordinal);
        }
        return output;
    }
}
