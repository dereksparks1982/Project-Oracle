namespace ProjectOracle.ConsoleApp;

internal static class ConsoleTheme
{
    private const string Reset = "\u001b[0m";
    private const string Base = "\u001b[40m\u001b[38;5;46m";
    private const string Dim = "\u001b[38;5;35m";
    private const string Live = "\u001b[38;5;118m";
    private const string Prompt = "\u001b[38;5;220m";
    private const string Power = "\u001b[38;5;51m";
    private const string Adam = "\u001b[38;5;226m";
    private const string Record = "\u001b[38;5;159m";
    private const string Command = "\u001b[38;5;83m";

    public static bool Enabled =>
        !System.Console.IsOutputRedirected &&
        string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("NO_COLOR"));

    public static void ApplyBase()
    {
        if (Enabled)
        {
            System.Console.Write(Base);
        }
    }

    public static void ResetToShell()
    {
        if (Enabled)
        {
            System.Console.Write(Reset);
        }
    }

    public static void Write(string value)
    {
        if (Enabled)
        {
            System.Console.Write(Colorise(value));
            return;
        }

        System.Console.Write(value);
    }

    public static void WriteLine(string value = "")
    {
        if (Enabled)
        {
            System.Console.WriteLine(Colorise(value));
            return;
        }

        System.Console.WriteLine(value);
    }

    public static void WritePrompt(string value)
    {
        if (Enabled)
        {
            System.Console.Write($"{Prompt}{value}{Base}");
            return;
        }

        System.Console.Write(value);
    }

    public static string LiveLine(string value) => Enabled ? $"{Live}{Colorise(value)}{Base}" : value;

    public static string ClearLine => Enabled ? $"{Dim}\u001b[2K{Base}" : "\u001b[2K";

    private static string Colorise(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        string output = value;
        output = Replace(output, "WORLD RECORD", Record);
        output = Replace(output, "CREATOR RECORD", Record);
        output = Replace(output, "World Record", Record);
        output = Replace(output, "Creator Record", Record);
        output = Replace(output, "LIVE", Live);
        output = Replace(output, "<oracle>", Power);
        output = Replace(output, "<gaia>", Power);
        output = Replace(output, "<adam>", Adam);
        output = Replace(output, "<sun>", Power);
        output = Replace(output, "<moon>", Power);
        output = Replace(output, "Yala", Power);
        output = Replace(output, "Oracle", Power);
        output = Replace(output, "Adam", Adam);
        output = Replace(output, "Sol", Power);
        output = Replace(output, "Gaia", Power);
        output = Replace(output, "Aether", Power);
        output = Replace(output, "Thalassa", Power);
        output = Replace(output, "Luna", Power);
        output = Replace(output, "status", Command);
        output = Replace(output, "choices", Command);
        output = Replace(output, "plans", Command);
        output = Replace(output, "brain", Command);
        output = Replace(output, "creation", Command);
        output = Replace(output, "records world", Command);
        output = Replace(output, "records creator", Command);
        return output;
    }

    private static string Replace(string value, string token, string colour) =>
        value.Replace(token, $"{colour}{token}{Base}", StringComparison.Ordinal);
}
