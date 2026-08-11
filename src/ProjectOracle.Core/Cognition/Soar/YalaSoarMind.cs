namespace ProjectOracle.Cognition.Soar;

public static class YalaSoarMind
{
    public const string BrainName = "Yala Soar Brain Slice 1";
    public const string Architecture = "Soar 9.6.5";

    public static YalaDecision Decide(YalaPerception perception)
    {
        ArgumentNullException.ThrowIfNull(perception);
        SoarRuntimePaths paths = SoarRuntimePaths.Discover();
        using SoarKernelHost host = new("yala", paths.YalaAgent);
        return host.Run(perception);
    }

    public static string ClassifyContactIntent(string message)
    {
        string text = (message ?? string.Empty).Trim().ToLowerInvariant();
        if (text.Contains("where") || text.Contains("location") || text.Contains("void"))
        {
            return "location";
        }
        if (text.Contains("who are you") || text.Contains("your name") || text.Contains("what are you"))
        {
            return "identity";
        }
        if (text.Contains("what did you") || text.Contains("what are you doing") || text.Contains("what have you done"))
        {
            return "action";
        }
        return "generic";
    }
}
