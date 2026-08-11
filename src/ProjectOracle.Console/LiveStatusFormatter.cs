using ProjectOracle.Simulation;

namespace ProjectOracle.ConsoleApp;

public static class LiveStatusFormatter
{
    public static string Format(OracleSimulation simulation, int availableWidth)
    {
        ArgumentNullException.ThrowIfNull(simulation);
        if (availableWidth <= 0)
        {
            return string.Empty;
        }

        long decisions = simulation.State.YalaCognition?.DecisionCount ?? 0;
        string full;
        if (!simulation.InWorldTimeExists)
        {
            full = $"LIVE | Yala: {simulation.State.Yala.Location} | In-world Time: not yet created | Soar decisions: {decisions}";
        }
        else if (!simulation.State.Cosmic!.LowerWorldEstablished)
        {
            full = $"LIVE | Yala: {simulation.State.Yala.Location} | Time: {simulation.Clock.Describe()} | Lower world: not yet established | Soar decisions: {decisions}";
        }
        else
        {
            CalendarSnapshot calendar = simulation.Clock.Calendar;
            full = $"LIVE {calendar.DescribeDateAndTime()} | Sun: {calendar.SolarPhase} | Moon: {calendar.LunarPhase} | Soar decisions: {decisions}";
        }

        return Fit(full, availableWidth);
    }

    private static string Fit(string value, int availableWidth)
    {
        if (value.Length <= availableWidth)
        {
            return value;
        }
        if (availableWidth <= 3)
        {
            return new string('.', availableWidth);
        }
        return $"{value[..(availableWidth - 3)]}...";
    }
}
