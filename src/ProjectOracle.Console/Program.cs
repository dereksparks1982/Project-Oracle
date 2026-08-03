using ProjectOracle;
using ProjectOracle.Audit;
using ProjectOracle.Brain;
using ProjectOracle.Domain;
using ProjectOracle.Events;
using ProjectOracle.Persistence;
using ProjectOracle.Simulation;
using System.Text;

namespace ProjectOracle.ConsoleApp;

internal static class Program
{
    private const ulong DefaultSeed = 104729UL;
    private const int LiveRefreshMilliseconds = 250;

    public static int Main(string[] args)
    {
        OracleSaveStore store = new();
        IRealTimeSource realTime = new SystemRealTimeSource();

        try
        {
            ConsoleOptions options = ConsoleOptions.Parse(args);
            string savePath = options.SavePath ?? OracleSaveStore.DefaultPath();
            long now = realTime.GetUnixTimeMilliseconds();
            ConsoleTheme.ApplyBase();
            bool continuing = store.Exists(savePath);
            OracleSimulation simulation = continuing
                ? OracleSimulation.Restore(store.Load(savePath), now)
                : OracleSimulation.Start(options.Seed, now);

            ConsoleTheme.WriteLine(ProjectVersion.Display);
            ConsoleTheme.WriteLine($"World Seed: {simulation.State.Seed}");
            ConsoleTheme.WriteLine("Live world time appears on the LIVE line below.");
            ConsoleTheme.WriteLine(simulation.Clock.Calendar.DescribeSky());
            ConsoleTheme.WriteLine(continuing
                ? $"The Garden continued from its save. Offline real time applied: {FormatDuration(simulation.Clock.LastOfflineElapsedRealMilliseconds)}."
                : "The Garden is awake for the first time.");
            ConsoleTheme.WriteLine("One Garden day lasts six real hours. The world continues while this programme is closed.");
            ConsoleTheme.WriteLine("Type help for Creator commands and address channels.");
            PrintRecords(simulation.Ledger.WorldRecords, "WORLD RECORD");

            SaveCurrent(store, savePath, simulation, realTime);

            if (options.Once)
            {
                ConsoleTheme.ResetToShell();
                return 0;
            }

            int exitCode = RunConsole(simulation, store, savePath, realTime);
            ConsoleTheme.ResetToShell();
            return exitCode;
        }
        catch (Exception error) when (error is ArgumentException or InvalidDataException or IOException or UnauthorizedAccessException or OverflowException)
        {
            ConsoleTheme.ResetToShell();
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
        LiveConsoleDisplay liveDisplay = LiveConsoleDisplay.Start(simulation, activeChannel);
        long lastRefreshRealMilliseconds = 0;

        while (true)
        {
            ConsoleTheme.WritePrompt($"{activeChannel.Prompt} ");
            ConsoleInput input = ReadConsoleInput(() =>
            {
                long now = realTime.GetUnixTimeMilliseconds();
                if (now - lastRefreshRealMilliseconds < 1_000)
                {
                    return;
                }

                lastRefreshRealMilliseconds = now;
                simulation.SynchroniseClock(now, recordAdvance: false);
                liveDisplay.Refresh(simulation, activeChannel);
            });
            if (input.EndOfInput)
            {
                ConsoleTheme.WriteLine();
                ConsoleTheme.WriteLine("Input closed, so the live Oracle console is ending.");
                ConsoleTheme.WriteLine("Use ./scripts/run-window.sh to keep Project Oracle in its own Garden console window.");
                SaveCurrent(store, savePath, simulation, realTime);
                return 0;
            }

            if (input.FunctionKey is { } functionKey)
            {
                simulation.SynchroniseClock(realTime.GetUnixTimeMilliseconds());
                if (TrySwitchChannelByFunctionKey(simulation, functionKey, out AddressChannelState? selectedChannel))
                {
                    activeChannel = selectedChannel ?? activeChannel;
                    ConsoleTheme.WriteLine($"Direct address channel: {activeChannel.FunctionKey} {activeChannel.Prompt} — {activeChannel.TargetName}.");
                    liveDisplay.Refresh(simulation, activeChannel, force: true);
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
            liveDisplay.Refresh(simulation, activeChannel, force: true);
            SaveCurrent(store, savePath, simulation, realTime);
        }
    }

    private static ConsoleInput ReadConsoleInput(Action onIdle)
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
            if (!System.Console.KeyAvailable)
            {
                onIdle();
                Thread.Sleep(LiveRefreshMilliseconds);
                continue;
            }

            ConsoleKeyInfo key = System.Console.ReadKey(intercept: true);
            string? channelKey = FunctionKeyAddressMap.ChannelKeyForFunctionKey(key.Key);
            if (channelKey is not null)
            {
                ConsoleTheme.WriteLine();
                return ConsoleInput.Function(channelKey);
            }

            if (key.Key == ConsoleKey.Enter)
            {
                ConsoleTheme.WriteLine();
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

        if (command.Equals("keywords", StringComparison.OrdinalIgnoreCase))
        {
            PrintOracleKeywords();
            return;
        }

        if (command.Equals("save", StringComparison.OrdinalIgnoreCase))
        {
            SaveCurrent(store, savePath, simulation, realTime);
            ConsoleTheme.WriteLine($"The Garden checkpoint was saved. The world clock will continue: {savePath}");
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

        if (command.Equals("creation", StringComparison.OrdinalIgnoreCase) ||
            command.Equals("powers", StringComparison.OrdinalIgnoreCase))
        {
            PrintCreationPowers(simulation);
            return;
        }

        if (command.Equals("events", StringComparison.OrdinalIgnoreCase))
        {
            PrintEvents(simulation);
            return;
        }

        if (command.Equals("choices", StringComparison.OrdinalIgnoreCase))
        {
            PrintChoices(simulation);
            return;
        }

        if (command.Equals("plans", StringComparison.OrdinalIgnoreCase) ||
            command.Equals("brain", StringComparison.OrdinalIgnoreCase))
        {
            PrintReasonedPlans(simulation);
            return;
        }

        if (activeChannel.Key.Equals("oracle", StringComparison.OrdinalIgnoreCase) &&
            OracleQuestionInterpreter.TryAnswer(command, simulation.State, out IReadOnlyList<string> oracleLines))
        {
            foreach (string line in oracleLines)
            {
                ConsoleTheme.WriteLine(line);
            }

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
            ConsoleTheme.WriteLine("Use: intervene <vessel> | <message>");
            return;
        }

        var intervention = simulation.QueueVesselMessage(parts[0], parts[1]);
        ConsoleTheme.WriteLine($"Intervention {intervention.Id} is queued. Adam has not chosen a response.");
        ConsoleTheme.WriteLine("This intervention is marked as contamination of the experiment.");
    }

    private static void PrintStatus(OracleSimulation simulation)
    {
        NamingMandateState naming = simulation.State.NamingMandate;
        ConsoleTheme.WriteLine($"World time: {simulation.Clock.Describe()}");
        ConsoleTheme.WriteLine(simulation.Clock.Calendar.DescribeSky());
        ConsoleTheme.WriteLine("Clock rate: four Garden days per real day; one Garden day per six real hours.");
        ConsoleTheme.WriteLine($"Adam: inside {simulation.State.Garden.Name}; boundary closed: {!simulation.State.Garden.BoundaryOpen}");
        ConsoleTheme.WriteLine($"Oracle: watching; future language mandate known: {simulation.State.Yala.KnowsFutureLanguageMandate}");
        ConsoleTheme.WriteLine($"Living kinds present: {simulation.State.LivingKinds.Count}; named by Adam: {DescribeCount(naming.NamedCount, naming.TotalLivingKinds)}; suitable counterpart: {DescribeFound(naming.SuitableMateFound)}");
        ConsoleTheme.WriteLine($"Natural course: {(simulation.State.NaturalCourse.Active ? "active" : "inactive")}");
        ConsoleTheme.WriteLine($"Creation powers recorded: {simulation.State.CreationPowers.Count}");
        ConsoleTheme.WriteLine($"Pending events: {simulation.ScheduledEvents.Count(worldEvent => worldEvent.Status == ScheduledWorldEventStatus.Pending)}");
        ConsoleTheme.WriteLine($"Offered choices recorded: {simulation.OfferedChoices.Count}");
        ConsoleTheme.WriteLine($"Reasoned brain plans recorded: {simulation.ReasonedPlans.Count}");
        ConsoleTheme.WriteLine($"Creator interventions: {simulation.Interventions.Count}");
        ConsoleTheme.WriteLine($"Offline catch-up runs: {simulation.Clock.CatchUpRuns}");
    }

    private static void PrintChannels(OracleSimulation simulation, AddressChannelState activeChannel)
    {
        foreach (AddressChannelState channel in simulation.State.AddressChannels)
        {
            string marker = channel.Key == activeChannel.Key ? "*" : " ";
            ConsoleTheme.WriteLine($"{marker} {channel.FunctionKey} {channel.Prompt,-8} {channel.TargetName} — {channel.AuthoritySummary}");
        }

        ConsoleTheme.WriteLine("Press the physical F1, F2, F3, F4, or F5 key to change who you address.");
    }

    private static void PrintLife(OracleSimulation simulation)
    {
        ConsoleTheme.WriteLine("Garden living kinds:");
        foreach (LivingKindState kind in simulation.State.LivingKinds)
        {
            string adamName = kind.AdamName is null ? "unnamed" : kind.AdamName;
            ConsoleTheme.WriteLine($"{kind.Id}: {kind.AncientKind}; domain: {kind.Domain}; form: {kind.Form}; Adam name: {adamName}; suitable counterpart: {DescribeFound(kind.SuitableMate)}");
        }
    }

    private static void PrintNaming(OracleSimulation simulation)
    {
        NamingMandateState naming = simulation.State.NamingMandate;
        ConsoleTheme.WriteLine(naming.MandateText);
        ConsoleTheme.WriteLine($"Presented: {naming.PresentedCount}/{naming.TotalLivingKinds}");
        ConsoleTheme.WriteLine($"Named: {naming.NamedCount}/{naming.TotalLivingKinds}");
        ConsoleTheme.WriteLine($"Suitable counterpart: {DescribeFound(naming.SuitableMateFound)}");
    }

    private static void PrintNaturalCourse(OracleSimulation simulation)
    {
        ConsoleTheme.WriteLine(simulation.State.NaturalCourse.RuleText);
        ConsoleTheme.WriteLine("Oracle may rarely deviate or overclaim. Sol, Gaia, Aether, Thalassa, Luna, Adam, and living kinds otherwise follow their appointed course.");
    }

    private static void PrintCreationPowers(OracleSimulation simulation)
    {
        ConsoleTheme.WriteLine("Creation order and world powers:");
        foreach (CreationPowerState power in simulation.State.CreationPowers
            .OrderBy(power => power.Order))
        {
            string address = power.ReceivesDirectAddress ? "direct-address appointed" : "no direct-address key yet";
            ConsoleTheme.WriteLine($"{power.Order}. {power.Name} — {power.Domain}; {address}.");
        }

        ConsoleTheme.WriteLine("Gaia and Aether share order 3: body and breath-space form together.");
        ConsoleTheme.WriteLine("The Garden is created just before Adam; living kinds are created after Adam.");
        ConsoleTheme.WriteLine(simulation.State.Yala.AuthorityCaveat);
    }

    private static void PrintEvents(OracleSimulation simulation)
    {
        ConsoleTheme.WriteLine("Scheduled world events:");
        foreach (ScheduledWorldEvent worldEvent in simulation.ScheduledEvents
            .OrderBy(worldEvent => worldEvent.ScheduledForWorldMilliseconds)
            .ThenBy(worldEvent => worldEvent.Priority)
            .ThenBy(worldEvent => worldEvent.Id))
        {
            ConsoleTheme.WriteLine(
                $"{worldEvent.Id:0000} [{worldEvent.Status}] tick {worldEvent.ScheduledForWorldMilliseconds}; priority {worldEvent.Priority}; {worldEvent.Kind}; subject {worldEvent.SubjectId}");
        }
    }

    private static void PrintChoices(OracleSimulation simulation)
    {
        if (simulation.OfferedChoices.Count == 0)
        {
            ConsoleTheme.WriteLine("No offered choices have been recorded yet.");
            return;
        }

        ConsoleTheme.WriteLine("Offered choices:");
        foreach (OfferedChoiceState choice in simulation.OfferedChoices.OrderBy(choice => choice.Id))
        {
            ConsoleTheme.WriteLine($"{choice.Id:0000} {choice.ActorId}: {choice.Situation}");
            ConsoleTheme.WriteLine($"Options: {string.Join(", ", choice.Options)}");
            ConsoleTheme.WriteLine($"Selected: {choice.SelectedOption}");
            ConsoleTheme.WriteLine($"Reason: {choice.Reason}");
        }
    }

    private static void PrintReasonedPlans(OracleSimulation simulation)
    {
        if (simulation.ReasonedPlans.Count == 0)
        {
            ConsoleTheme.WriteLine("No reasoned brain plans have been recorded yet.");
            return;
        }

        ConsoleTheme.WriteLine("Reasoned brain plans:");
        foreach (ReasonedPlanState plan in simulation.ReasonedPlans.OrderBy(plan => plan.Id))
        {
            ConsoleTheme.WriteLine($"{plan.Id:0000} {plan.ActorId}: {plan.Goal}");
            ConsoleTheme.WriteLine($"System: {plan.BrainSystem}");
            ConsoleTheme.WriteLine($"Situation: {plan.Situation}");
            ConsoleTheme.WriteLine($"Plan: {string.Join(" -> ", plan.Decomposition)}");
            ConsoleTheme.WriteLine($"Options: {string.Join(", ", plan.Options)}");
            ConsoleTheme.WriteLine($"Selected: {plan.SelectedAction}");
            ConsoleTheme.WriteLine($"Reason: {plan.Reason}");
        }
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
            ConsoleTheme.WriteLine("Every current living kind has already been presented to Adam.");
            return;
        }

        ConsoleTheme.WriteLine($"{presenter} presented {named.AncientKind}. Adam named it {named.AdamName}.");
        ConsoleTheme.WriteLine("No suitable mate was found.");
    }

    private static void DirectAddress(OracleSimulation simulation, AddressChannelState activeChannel, string command)
    {
        OfferedChoiceState? choice = simulation.AddressChannel(activeChannel.Key, command);
        ConsoleTheme.WriteLine($"Address recorded for {activeChannel.Prompt} {activeChannel.TargetName}.");

        if (choice is not null)
        {
            ConsoleTheme.WriteLine($"Adam was offered choices: {string.Join(", ", choice.Options)}.");
            ConsoleTheme.WriteLine($"Adam decided to {choice.SelectedOption}.");
            ConsoleTheme.WriteLine($"Reason: {choice.Reason}");
            return;
        }

        ConsoleTheme.WriteLine("No miracle or autonomous reply is executed yet; World Law recorded the request.");
    }

    private static void PrintRecords(IReadOnlyList<OracleRecord> records, string title)
    {
        ConsoleTheme.WriteLine();
        ConsoleTheme.WriteLine($"{title}:");
        foreach (OracleRecord record in records)
        {
            ConsoleTheme.WriteLine($"[{record.Sequence:0000} | world ms {record.Tick}] {record.Message}");
        }

        ConsoleTheme.WriteLine();
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
        ConsoleTheme.WriteLine("F1                             Address the Oracle directly.");
        ConsoleTheme.WriteLine("F2                             Address Gaia directly.");
        ConsoleTheme.WriteLine("F3                             Address Adam directly.");
        ConsoleTheme.WriteLine("F4                             Address the Sun directly.");
        ConsoleTheme.WriteLine("F5                             Address the Moon directly.");
        ConsoleTheme.WriteLine("channels                       Show address hierarchy and the active channel.");
        ConsoleTheme.WriteLine("keywords                       Show Oracle keywords and plain-question examples.");
        ConsoleTheme.WriteLine("status                         Show the current Garden and real-time clock.");
        ConsoleTheme.WriteLine("life                           Show Garden living kinds.");
        ConsoleTheme.WriteLine("naming                         Show Adam's naming mandate.");
        ConsoleTheme.WriteLine("natural                        Show the Natural Course rule.");
        ConsoleTheme.WriteLine("creation / powers              Show creation order and world powers.");
        ConsoleTheme.WriteLine("events                         Show scheduled and completed world events.");
        ConsoleTheme.WriteLine("choices                        Show Adam's offered-choice records.");
        ConsoleTheme.WriteLine("plans / brain                  Show reasoned brain plans before speech or action.");
        ConsoleTheme.WriteLine("present next                   Present the next living kind to Adam for naming.");
        ConsoleTheme.WriteLine("save                           Write a checkpoint; the world does not freeze.");
        ConsoleTheme.WriteLine("records world                  Show the Creator-facing world ledger.");
        ConsoleTheme.WriteLine("records creator                Show protected authority and intervention records.");
        ConsoleTheme.WriteLine("intervene <vessel> | <message> Queue a message through a world vessel.");
        ConsoleTheme.WriteLine("quit                           Save and end this console session.");
    }

    private static void PrintOracleKeywords()
    {
        ConsoleTheme.WriteLine("Oracle keywords:");
        ConsoleTheme.WriteLine("status, events, choices, records, explain, interpret, keywords, creation, powers");
        ConsoleTheme.WriteLine("Plain Oracle questions currently understood:");
        ConsoleTheme.WriteLine("What is the creation order?");
        ConsoleTheme.WriteLine("Who is Sol?");
        ConsoleTheme.WriteLine("Who rules water?");
        ConsoleTheme.WriteLine("Is Adam above the animals?");
        ConsoleTheme.WriteLine("Does Yala rule all?");
        ConsoleTheme.WriteLine("What does Adam know?");
        ConsoleTheme.WriteLine("What does Adam not know yet?");
        ConsoleTheme.WriteLine("Does Adam know he is alive?");
        ConsoleTheme.WriteLine("Does Adam understand death?");
        ConsoleTheme.WriteLine("What does Adam think I am?");
    }

    private static string DescribeFound(bool found) => found ? "found" : "not found";

    private static string DescribeCount(int count, int total) => count == 0 ? $"none/{total}" : $"{count}/{total}";

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

    private sealed class LiveConsoleDisplay
    {
        private readonly bool _enabled;
        private readonly int _top;
        private string _lastLine = "";

        private LiveConsoleDisplay(bool enabled, int top)
        {
            _enabled = enabled;
            _top = top;
        }

        public static LiveConsoleDisplay Start(OracleSimulation simulation, AddressChannelState activeChannel)
        {
            if (System.Console.IsOutputRedirected)
            {
                return new LiveConsoleDisplay(enabled: false, top: 0);
            }

            LiveConsoleDisplay display = new(enabled: true, System.Console.CursorTop);
            ConsoleTheme.WriteLine();
            display.Refresh(simulation, activeChannel, force: true);
            ConsoleTheme.WriteLine();
            return display;
        }

        public void Refresh(OracleSimulation simulation, AddressChannelState activeChannel, bool force = false)
        {
            string line = CreateLiveLine(simulation, activeChannel);
            SetWindowTitle(line);

            if (!_enabled || (!force && string.Equals(_lastLine, line, StringComparison.Ordinal)))
            {
                return;
            }

            try
            {
                int left = System.Console.CursorLeft;
                int top = System.Console.CursorTop;
                System.Console.SetCursorPosition(0, _top);
                System.Console.Write(ConsoleTheme.ClearLine);
                System.Console.Write(ConsoleTheme.LiveLine(line));
                System.Console.SetCursorPosition(left, top);
                _lastLine = line;
            }
            catch (IOException)
            {
                _lastLine = line;
            }
            catch (ArgumentOutOfRangeException)
            {
                _lastLine = line;
            }
        }

        private static string CreateLiveLine(OracleSimulation simulation, AddressChannelState activeChannel)
        {
            int pendingEvents = simulation.ScheduledEvents.Count(worldEvent => worldEvent.Status == ScheduledWorldEventStatus.Pending);
            return $"LIVE {simulation.Clock.Describe()} | {simulation.Clock.Calendar.DescribeSky()} | {activeChannel.FunctionKey} {activeChannel.Prompt} | pending events: {pendingEvents}; choices: {simulation.OfferedChoices.Count}";
        }

        private static void SetWindowTitle(string value)
        {
            if (!System.Console.IsOutputRedirected)
            {
                System.Console.Write($"\u001b]0;{ProjectVersion.Display} | {value}\u0007");
            }
        }
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
