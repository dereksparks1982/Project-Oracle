using ProjectOracle;
using ProjectOracle.Audit;
using ProjectOracle.Domain;
using ProjectOracle.Persistence;
using ProjectOracle.Simulation;
using System.Text;

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
            System.Console.WriteLine($"World Seed: {simulation.State.Seed}");
            System.Console.WriteLine($"World time: {simulation.Clock.Describe()}");
            System.Console.WriteLine(simulation.Clock.Calendar.DescribeSky());
            System.Console.WriteLine(continuing
                ? $"The Garden continued from its save. Offline real time applied: {FormatDuration(simulation.Clock.LastOfflineElapsedRealMilliseconds)}."
                : "The Garden is awake for the first time.");
            System.Console.WriteLine("One Garden day lasts six real hours. The world continues while this programme is closed.");
            System.Console.WriteLine("Type help for Creator commands and address channels.");
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
        AddressChannelState activeChannel = simulation.State.AddressChannels.First(channel => channel.Key == "oracle");

        while (true)
        {
            System.Console.Write($"{activeChannel.Prompt} ");
            ConsoleInput input = ReadConsoleInput();
            if (input.EndOfInput)
            {
                System.Console.WriteLine();
                System.Console.WriteLine("Input closed, so the live Oracle console is ending.");
                System.Console.WriteLine("Use ./scripts/run-window.sh to keep Project Oracle in its own Garden console window.");
                SaveCurrent(store, savePath, simulation, realTime);
                return 0;
            }

            if (input.FunctionKey is { } functionKey)
            {
                simulation.SynchroniseClock(realTime.GetUnixTimeMilliseconds());
                if (TrySwitchChannelByFunctionKey(simulation, functionKey, out AddressChannelState? selectedChannel))
                {
                    activeChannel = selectedChannel ?? activeChannel;
                    System.Console.WriteLine($"Direct address channel: {activeChannel.FunctionKey} {activeChannel.Prompt} — {activeChannel.TargetName}.");
                    SaveCurrent(store, savePath, simulation, realTime);
                    continue;
                }
            }

            string command = input.Command.Trim();
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

            ExecuteCommand(simulation, store, savePath, realTime, activeChannel, command);
            SaveCurrent(store, savePath, simulation, realTime);
        }
    }

    private static ConsoleInput ReadConsoleInput()
    {
        if (System.Console.IsInputRedirected)
        {
            string? redirected = System.Console.ReadLine();
            return redirected is null
                ? ConsoleInput.End()
                : ConsoleInput.CommandText(redirected);
        }

        StringBuilder line = new();
        while (true)
        {
            ConsoleKeyInfo key = System.Console.ReadKey(intercept: true);
            string? channelKey = FunctionKeyAddressMap.ChannelKeyForFunctionKey(key.Key);
            if (channelKey is not null)
            {
                System.Console.WriteLine();
                return ConsoleInput.Function(channelKey);
            }

            if (key.Key == ConsoleKey.Enter)
            {
                System.Console.WriteLine();
                return ConsoleInput.CommandText(line.ToString());
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (line.Length > 0)
                {
                    line.Remove(line.Length - 1, 1);
                    System.Console.Write("\b \b");
                }

                continue;
            }

            if (!char.IsControl(key.KeyChar))
            {
                line.Append(key.KeyChar);
                System.Console.Write(key.KeyChar);
            }
        }
    }

    private static void ExecuteCommand(
        OracleSimulation simulation,
        OracleSaveStore store,
        string savePath,
        IRealTimeSource realTime,
        AddressChannelState activeChannel,
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

        if (command.Equals("channels", StringComparison.OrdinalIgnoreCase))
        {
            PrintChannels(simulation, activeChannel);
            return;
        }

        if (command.Equals("life", StringComparison.OrdinalIgnoreCase))
        {
            PrintLife(simulation);
            return;
        }

        if (command.Equals("naming", StringComparison.OrdinalIgnoreCase))
        {
            PrintNaming(simulation);
            return;
        }

        if (command.Equals("natural", StringComparison.OrdinalIgnoreCase))
        {
            PrintNaturalCourse(simulation);
            return;
        }

        if (command.Equals("present next", StringComparison.OrdinalIgnoreCase))
        {
            PresentNextLivingKind(simulation, activeChannel);
            return;
        }

        if (command.StartsWith("intervene ", StringComparison.OrdinalIgnoreCase))
        {
            Intervene(simulation, command[10..]);
            return;
        }

        DirectAddress(simulation, activeChannel, command);
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
        NamingMandateState naming = simulation.State.NamingMandate;
        System.Console.WriteLine($"World time: {simulation.Clock.Describe()}");
        System.Console.WriteLine(simulation.Clock.Calendar.DescribeSky());
        System.Console.WriteLine("Clock rate: four Garden days per real day; one Garden day per six real hours.");
        System.Console.WriteLine($"Adam: inside {simulation.State.Garden.Name}; boundary closed: {!simulation.State.Garden.BoundaryOpen}");
        System.Console.WriteLine($"Oracle: watching; future language mandate known: {simulation.State.Yala.KnowsFutureLanguageMandate}");
        System.Console.WriteLine($"Living kinds: {simulation.State.LivingKinds.Count}; named by Adam: {naming.NamedCount}/{naming.TotalLivingKinds}; suitable mate found: {naming.SuitableMateFound}");
        System.Console.WriteLine($"Natural course: {(simulation.State.NaturalCourse.Active ? "active" : "inactive")}");
        System.Console.WriteLine($"Pending Creator interventions: {simulation.Interventions.Count}");
        System.Console.WriteLine($"Offline catch-up runs: {simulation.Clock.CatchUpRuns}");
    }

    private static void PrintChannels(OracleSimulation simulation, AddressChannelState activeChannel)
    {
        foreach (AddressChannelState channel in simulation.State.AddressChannels)
        {
            string marker = channel.Key == activeChannel.Key ? "*" : " ";
            System.Console.WriteLine($"{marker} {channel.FunctionKey} {channel.Prompt,-8} {channel.TargetName} — {channel.AuthoritySummary}");
        }

        System.Console.WriteLine("Press the physical F1, F2, F3, F4, or F5 key to change who you address.");
    }

    private static void PrintLife(OracleSimulation simulation)
    {
        System.Console.WriteLine("Garden living kinds:");
        foreach (LivingKindState kind in simulation.State.LivingKinds)
        {
            string adamName = kind.AdamName is null ? "unnamed" : kind.AdamName;
            System.Console.WriteLine($"{kind.Id}: {kind.AncientKind}; domain: {kind.Domain}; form: {kind.Form}; Adam name: {adamName}; suitable mate: {kind.SuitableMate}");
        }
    }

    private static void PrintNaming(OracleSimulation simulation)
    {
        NamingMandateState naming = simulation.State.NamingMandate;
        System.Console.WriteLine(naming.MandateText);
        System.Console.WriteLine($"Presented: {naming.PresentedCount}/{naming.TotalLivingKinds}");
        System.Console.WriteLine($"Named: {naming.NamedCount}/{naming.TotalLivingKinds}");
        System.Console.WriteLine($"Suitable mate found: {naming.SuitableMateFound}");
    }

    private static void PrintNaturalCourse(OracleSimulation simulation)
    {
        System.Console.WriteLine(simulation.State.NaturalCourse.RuleText);
        System.Console.WriteLine("Oracle may rarely deviate. Gaia, Sun, Moon, Adam, and living kinds otherwise follow their planned course.");
    }

    private static void PresentNextLivingKind(OracleSimulation simulation, AddressChannelState activeChannel)
    {
        string presenter = activeChannel.Key switch
        {
            "gaia" => "Gaia",
            "oracle" => "The Oracle",
            "sun" => "The Sun's light",
            "moon" => "The Moon's sign",
            _ => "The Garden"
        };

        LivingKindState? named = simulation.PresentNextLivingKindToAdam(presenter);
        if (named is null)
        {
            System.Console.WriteLine("Every current living kind has already been presented to Adam.");
            return;
        }

        System.Console.WriteLine($"{presenter} presented {named.AncientKind}. Adam named it {named.AdamName}.");
        System.Console.WriteLine("No suitable mate was found.");
    }

    private static void DirectAddress(OracleSimulation simulation, AddressChannelState activeChannel, string command)
    {
        simulation.AddressChannel(activeChannel.Key, command);
        System.Console.WriteLine($"Address recorded for {activeChannel.Prompt} {activeChannel.TargetName}.");

        if (activeChannel.Key == "adam")
        {
            System.Console.WriteLine("Adam has heard the address, but no autonomous response engine exists yet.");
            return;
        }

        System.Console.WriteLine("No miracle or autonomous reply is executed yet; World Law recorded the request.");
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
        System.Console.WriteLine("F1                             Address the Oracle directly.");
        System.Console.WriteLine("F2                             Address Gaia directly.");
        System.Console.WriteLine("F3                             Address Adam directly.");
        System.Console.WriteLine("F4                             Address the Sun directly.");
        System.Console.WriteLine("F5                             Address the Moon directly.");
        System.Console.WriteLine("channels                       Show address hierarchy and the active channel.");
        System.Console.WriteLine("status                         Show the current Garden and real-time clock.");
        System.Console.WriteLine("life                           Show Garden living kinds.");
        System.Console.WriteLine("naming                         Show Adam's naming mandate.");
        System.Console.WriteLine("natural                        Show the Natural Course rule.");
        System.Console.WriteLine("present next                   Present the next living kind to Adam for naming.");
        System.Console.WriteLine("save                           Write a checkpoint; the world does not freeze.");
        System.Console.WriteLine("records world                  Show what inhabitants may know.");
        System.Console.WriteLine("records creator                Show protected Creator truth.");
        System.Console.WriteLine("intervene <vessel> | <message> Queue a message through a world vessel.");
        System.Console.WriteLine("quit                           Save and end this console session.");
    }

    private static bool TrySwitchChannelByFunctionKey(
        OracleSimulation simulation,
        string channelKey,
        out AddressChannelState? selectedChannel)
    {
        selectedChannel = simulation.State.AddressChannels.FirstOrDefault(channel => channel.Key == channelKey);
        return selectedChannel is not null;
    }

    private sealed record ConsoleInput(string Command, string? FunctionKey, bool EndOfInput)
    {
        public static ConsoleInput CommandText(string command) => new(command, null, false);

        public static ConsoleInput Function(string channelKey) => new("", channelKey, false);

        public static ConsoleInput End() => new("", null, true);
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
