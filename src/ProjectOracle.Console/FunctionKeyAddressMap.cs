namespace ProjectOracle.ConsoleApp;

public static class FunctionKeyAddressMap
{
    public static string? ChannelKeyForFunctionKey(ConsoleKey key) => key switch
    {
        ConsoleKey.F1 => "oracle",
        ConsoleKey.F2 => "gaia",
        ConsoleKey.F3 => "adam",
        ConsoleKey.F4 => "sun",
        ConsoleKey.F5 => "moon",
        _ => null
    };
}
