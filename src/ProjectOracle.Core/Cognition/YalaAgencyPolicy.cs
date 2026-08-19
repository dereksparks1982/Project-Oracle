namespace ProjectOracle.Cognition;

/// <summary>
/// The in-world agency boundary for Yala. Soar may choose among these simulation
/// operators, but no Yala decision can acquire host shell, process, filesystem,
/// network, code-modification, or hidden Oracle capabilities.
/// </summary>
public static class YalaAgencyPolicy
{
    private static readonly HashSet<string> AllowedActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "observe",
        "reflect",
        "wait",
        "create-gaia",
        "command-gaia-time",
        "enact-cosmic-choice",
        "respond",
        "ask-speaker"
    };

    public static IReadOnlyCollection<string> Actions => AllowedActions;

    public static bool Allows(string action) =>
        !string.IsNullOrWhiteSpace(action) && AllowedActions.Contains(action.Trim());

    public static bool AllowsHostShell => false;
    public static bool AllowsHostProcessExecution => false;
    public static bool AllowsHostFileMutation => false;
    public static bool AllowsNetworkAccess => false;
    public static bool AllowsCodeModification => false;
    public static bool AllowsHiddenOracleKnowledge => false;

    public static void DemandAllowed(string action)
    {
        if (!Allows(action))
        {
            throw new InvalidOperationException($"Yala action '{action}' is outside the in-world agency sandbox.");
        }
    }
}
