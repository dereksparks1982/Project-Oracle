using ProjectOracle.Simulation;

namespace ProjectOracle.ConsoleApp;

/// <summary>
/// Compatibility shell retained for source stability. Repair 2 removed this class from
/// the interactive input path; Repair 3 locks the matching acceptance assertion. It is terminal-silent and can never paint LIVE status.
/// </summary>
public sealed class LiveConsoleSurface
{
    public static bool VisibleStatusInBody => false;

    public void Refresh(OracleSimulation simulation, ConsoleInputLine input, bool force = false)
    {
        ArgumentNullException.ThrowIfNull(simulation);
        ArgumentNullException.ThrowIfNull(input);
    }

    public static bool MayPaintVisibleStatus(ConsoleInputLine input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return false;
    }
}
