using ProjectOracle;
using ProjectOracle.Audit;
using ProjectOracle.Persistence;
using ProjectOracle.Simulation;

namespace ProjectOracle.ConsoleApp;

internal static class Program
{
    private const ulong DefaultSeed = 104729UL;

    public static int Main(string[] args)
    {
        OracleSaveStore store = new();
        IRealTimeSource realTime = new SystemRealTimeSource();

        try
        {
            ConsoleOptions options = ConsoleOptions.Parse(args);
            string savePath = options.SavePath ?? OracleSaveStore.DefaultPath();
            long now = realTime.GetUnixTimeMilliseconds();
            bool continuing = store.Exists(savePath);
            OracleSimulation simulation = continuing
                ? OracleSimulation.Restore(store.Load(savePath), now)
                : OracleSimulation.Start(options.Seed, now);

            System.Console.WriteLine(ProjectVersion.Display);
            System.Console.WriteLine($"Run seed: {simulation.State.Seed}");
            System.Console.WriteLine($"World time: {simulation.Clock.Describe()}");
            System.Console.WriteLine(simulation.Clock.Calendar.DescribeSky());
            System.Console.WriteLine(continuing
                ? $"The Garden continued from its save. Offline real time applied: {FormatDuration(simulation.Clock.LastOfflineElapsedRealMilliseconds)}."
                : "The Garden is awake for the first time.");
            System.Console.WriteLine("One Garden day lasts six real hours. The world continues while this programme is closed.");
            System.Console.WriteLine("Type help for Creator commands.");
            PrintRecords(simulation.Ledger.WorldRecords, "WORLD RECORD");

            SaveCurrent(store, savePath, simulation, realTime);

            if (options.Once)
            {
                return 0;
            }

            return RunConsole(simulation, store, savePath, realTime);
        }
        catch (Exception error) when (error is ArgumentException or InvalidDataException or IOException or UnauthorizedAccessException or OverflowException)
        {
            System.Console.Error.WriteLine($"Project Oracle could not continue safely: {error.Message}");
            return 2;
        }
    }

    private static int RunConsole(
        OracleSimulation simulation,
        OracleSaveStore store,
        string savePath,
        IRealTimeSource realTime)
    {
        while (true)
        {
            System.Console.Write("oracle> ");
            string? input = System.Console.ReadLine();
            if (input is null)
            {
                System.Console.WriteLine();
                System.Console.WriteLine("Input closed, so the live Oracle console is ending.");
                System.Console.WriteLine("Use ./scripts/run-window.sh to keep Project Oracle in its own Garden console window.");
                SaveCurrent(store, savePath, simulation, realTime);
                return 0;
            }

            string command = input.Trim();
            if (command.Length == 0)
            {
                continue;
            }

            simulation.SynchroniseClock(realTime.GetUnixTimeMilliseconds());

            if (command.Equals("quit", StringComparison.OrdinalIgnoreCase) ||
                command.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                SaveCurrent(store, savePath, simulation, realTime);
                return 0;
            }

            ExecuteCommand(simulation, store, savePath, realTime, command);
            SaveCurrent(store, savePath, simulation, realTime);
        }
    }

    private static void ExecuteCommand(
        OracleSimulation simulation,
        OracleSaveStore store,
        string savePath,
        IRealTimeSource realTime,
        string command)
    {
        if (command.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            PrintHelp();
            return;
        }

        if (command.Equals("status", StringComparison.OrdinalIgnoreCase))
        {
            PrintStatus(simulation);
            return;
        }

        if (command.Equals("save", StringComparison.OrdinalIgnoreCase))
        {
            SaveCurrent(store, savePath, simulation, realTime);
            System.Console.WriteLine($"The Garden checkpoint was saved. The world clock will continue: {savePath}");
            return;
        }

        if (command.Equals("records world", StringComparison.OrdinalIgnoreCase))
        {
            PrintRecords(simulation.Ledger.WorldRecords, "WORLD RECORD");
            return;
        }

        if (command.Equals("records creator", StringComparison.OrdinalIgnoreCase))
        {
            PrintRecords(simulation.Ledger.CreatorRecords, "CREATOR RECORD");
            return;
        }

        if (command.StartsWith("intervene ", StringComparison.OrdinalIgnoreCase))
        {
            Intervene(simulation, command[10..]);
            return;
        }

        System.Console.WriteLine("That command is not recognised. Type help to see the available commands.");
    }

    private static void Intervene(OracleSimulation simulation, string value)
    {
        string[] parts = value.Split('|', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || parts.Any(string.IsNullOrWhiteSpace))
        {
            System.Console.WriteLine("Use: intervene <vessel> | <message>");
            return;
        }

        var intervention = simulation.QueueVesselMessage(parts[0], parts[1]);
        System.Console.WriteLine($"Intervention {intervention.Id} is queued. Adam has not chosen a response.");
        System.Console.WriteLine("This intervention is marked as contamination of the experiment.");
    }

    private static void PrintStatus(OracleSimulation simulation)
    {
        System.Console.WriteLine($"World time: {simulation.Clock.Describe()}");
        System.Console.WriteLine(simulation.Clock.Calendar.DescribeSky());
        System.Console.WriteLine("Clock rate: four Garden days per real day; one Garden day per six real hours.");
        System.Console.WriteLine($"Adam: inside {simulation.State.Garden.Name}; boundary closed: {!simulation.State.Garden.BoundaryOpen}");
        System.Console.WriteLine($"Oracle: watching; future language mandate known: {simulation.State.Yala.KnowsFutureLanguageMandate}");
        System.Console.WriteLine($"Pending Creator interventions: {simulation.Interventions.Count}");
        System.Console.WriteLine($"Offline catch-up runs: {simulation.Clock.CatchUpRuns}");
    }

    private static void PrintRecords(IReadOnlyList<OracleRecord> records, string title)
    {
        System.Console.WriteLine();
        System.Console.WriteLine($"{title}:");
        foreach (OracleRecord record in records)
        {
            System.Console.WriteLine($"[{record.Sequence:0000} | world ms {record.Tick}] {record.Message}");
        }

        System.Console.WriteLine();
    }

    private static void SaveCurrent(
        OracleSaveStore store,
        string path,
        OracleSimulation simulation,
        IRealTimeSource realTime)
    {
        long now = realTime.GetUnixTimeMilliseconds();
        simulation.SynchroniseClock(now);
        store.Save(path, simulation.CreateSnapshot(now));
    }

    private static string FormatDuration(long milliseconds)
    {
        TimeSpan duration = TimeSpan.FromMilliseconds(milliseconds);
        return $"{(long)duration.TotalHours}h {duration.Minutes}m {duration.Seconds}s";
    }

    private static void PrintHelp()
    {
        System.Console.WriteLine("status                         Show the current Garden and real-time clock.");
        System.Console.WriteLine("save                           Write a checkpoint; the world does not freeze.");
        System.Console.WriteLine("records world                  Show what inhabitants may know.");
        System.Console.WriteLine("records creator                Show protected Creator truth.");
        System.Console.WriteLine("intervene <vessel> | <message> Queue a message through a world vessel.");
        System.Console.WriteLine("quit                           Save and end this console session.");
    }

    private sealed record ConsoleOptions(ulong Seed, string? SavePath, bool Once)
    {
        public static ConsoleOptions Parse(string[] args)
        {
            ulong seed = DefaultSeed;
            string? savePath = null;
            bool once = false;

            for (int index = 0; index < args.Length; index++)
            {
                string argument = args[index];
                if (argument.Equals("--once", StringComparison.OrdinalIgnoreCase))
                {
                    once = true;
                    continue;
                }

                if (argument.Equals("--seed", StringComparison.OrdinalIgnoreCase))
                {
                    if (++index >= args.Length || !ulong.TryParse(args[index], out seed))
                    {
                        throw new ArgumentException("--seed must be followed by a whole number from 0 to 18,446,744,073,709,551,615.");
                    }

                    continue;
                }

                if (argument.Equals("--save", StringComparison.OrdinalIgnoreCase))
                {
                    if (++index >= args.Length || string.IsNullOrWhiteSpace(args[index]))
                    {
                        throw new ArgumentException("--save must be followed by a save-file path.");
                    }

                    savePath = args[index];
                    continue;
                }

                throw new ArgumentException($"Unknown start option: {argument}");
            }

            return new ConsoleOptions(seed, savePath, once);
        }
    }
}
