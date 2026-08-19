using ProjectOracle;
using ProjectOracle.Audit;
using ProjectOracle.Brain;
using ProjectOracle.Cognition.Soar;
using ProjectOracle.Domain;
using ProjectOracle.Events;
using ProjectOracle.Interventions;
using ProjectOracle.Observation;
using ProjectOracle.Persistence;
using ProjectOracle.Simulation;
using System.Diagnostics;

namespace ProjectOracle.ConsoleApp;

internal static class Program
{
    private const ulong DefaultSeed = 104729UL;
    private const int LiveRefreshMilliseconds = 100;

    public static int Main(string[] args)
    {
        try
        {
            ConsoleOptions options = ConsoleOptions.Parse(args);
            if (!options.TerminalChild && !options.Once && DesktopConsoleBootstrap.TryRelaunchInTerminal(args))
            {
                return 0;
            }

            OracleSaveStore store = new();
            IRealTimeSource realTime = new SystemRealTimeSource();
            string savePath = options.SavePath ?? OracleSaveStore.DefaultPath();
            long now = realTime.GetUnixTimeMilliseconds();
            bool continuing = store.Exists(savePath);
            using OracleSimulation simulation = continuing
                ? OracleSimulation.Restore(store.Load(savePath), now, savePath)
                : OracleSimulation.Start(options.Seed, now, savePath);

            ConsoleTheme.ApplyBase();
            using LiveWorldClockSurface worldClockSurface = new();
            worldClockSurface.Begin(simulation);
            PrintBanner(simulation, continuing, worldClockSurface.Active);

            if (options.Once)
            {
                YalaDecision decision = simulation.TryRunYalaAutonomousStep(now, force: true)
                    ?? throw new InvalidOperationException("Yala Soar smoke step did not run.");
                ConsoleTheme.WriteLine($"SOAR SMOKE PASS: Yala selected {decision.Action} via {decision.Source}.");
                SaveCurrent(store, savePath, simulation, realTime);
                ConsoleTheme.ResetToShell();
                return 0;
            }

            SaveCurrent(store, savePath, simulation, realTime);
            int exitCode = RunConsole(simulation, store, savePath, realTime, worldClockSurface);
            ConsoleTheme.ResetToShell();
            return exitCode;
        }
        catch (Exception error)
        {
            ConsoleTheme.ResetToShell();
            System.Console.Error.WriteLine($"Project Oracle could not continue safely: {Unwrap(error).Message}");
            return 2;
        }
    }

    private static void PrintBanner(OracleSimulation simulation, bool continuing, bool anchoredWorldClockActive)
    {
        if (!anchoredWorldClockActive)
        {
            ConsoleTheme.WriteLine(LiveWorldClockSurface.Describe(simulation));
        }
        ConsoleTheme.WriteLine(ProjectVersion.Display);
        ConsoleTheme.WriteLine($"World Seed: {simulation.State.Seed}");
        ConsoleTheme.WriteLine(continuing ? "Existing world state restored." : "Fresh v0.0.26 experimental world started at Yala's pre-Time Void state.");
        ConsoleTheme.WriteLine("Yala cognition: Soar 9.6.5 Brain Slice 9 with deliberation, planning, investigations, counterfactuals, memory, and bounded in-world agency.");
        ConsoleTheme.WriteLine("Ctrl+Y enters persistent Yala conversation mode. Escape returns to the normal system prompt.");
        ConsoleTheme.WriteLine("Type help for system-console commands and direct-call syntax.");
        PrintRecords(simulation.Ledger.WorldRecords, "WORLD RECORD");
    }

    private static int RunConsole(
        OracleSimulation simulation,
        OracleSaveStore store,
        string savePath,
        IRealTimeSource realTime,
        LiveWorldClockSurface worldClockSurface)
    {
        long lastRefresh = 0;
        ConsoleConversationMode conversationMode = new();
        while (true)
        {
            ConsoleInput input = ReadConsoleInput(line =>
            {
                long now = realTime.GetUnixTimeMilliseconds();
                if (now - lastRefresh < 250)
                {
                    return;
                }
                lastRefresh = now;
                simulation.SynchroniseClock(now, recordAdvance: false);
                simulation.TryRunYalaAutonomousStep(now);
                worldClockSurface.Refresh(simulation);

                // Autonomous Yala questions are delivered only while the command buffer is
                // empty. If Derek is typing, the question remains pending until a safe prompt.
                if (line.IsEmpty && simulation.TryTakePendingYalaUtterance(out string? utterance) && !string.IsNullOrWhiteSpace(utterance))
                {
                    System.Console.WriteLine();
                    ConsoleTheme.WriteLine($"Yala: {utterance}");
                    WriteInteractivePrompt(conversationMode, line);
                }
            }, conversationMode);

            if (input.EndOfInput)
            {
                SaveCurrent(store, savePath, simulation, realTime);
                return 0;
            }

            string command = input.Command.Trim();
            if (command.Length == 0)
            {
                continue;
            }
            long now = realTime.GetUnixTimeMilliseconds();
            simulation.SynchroniseClock(now, recordAdvance: false);
            worldClockSurface.Refresh(simulation);
            if (command.Equals("quit", StringComparison.OrdinalIgnoreCase) || command.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                SaveCurrent(store, savePath, simulation, realTime);
                return 0;
            }

            ExecuteCommand(simulation, store, savePath, realTime, command, now);
            SaveCurrent(store, savePath, simulation, realTime);
            worldClockSurface.Refresh(simulation);
        }
    }

    private static ConsoleInput ReadConsoleInput(Action<ConsoleInputLine> onIdle, ConsoleConversationMode conversationMode)
    {
        if (System.Console.IsInputRedirected)
        {
            string? redirected = System.Console.ReadLine();
            return redirected is null ? ConsoleInput.End() : ConsoleInput.CommandText(redirected);
        }

        ArgumentNullException.ThrowIfNull(conversationMode);
        ConsoleInputLine line = new();
        WriteInteractivePrompt(conversationMode, line);

        while (true)
        {
            if (!System.Console.KeyAvailable)
            {
                onIdle(line);
                Thread.Sleep(LiveRefreshMilliseconds);
                continue;
            }

            ConsoleKeyInfo key = System.Console.ReadKey(intercept: true);
            bool control = (key.Modifiers & ConsoleModifiers.Control) != 0;
            if (control && key.Key == ConsoleKey.Y)
            {
                conversationMode.EnterYala();
                RedrawInteractivePrompt(conversationMode, line);
                continue;
            }
            if (key.Key == ConsoleKey.Escape)
            {
                conversationMode.Escape(line);
                RedrawInteractivePrompt(conversationMode, line);
                continue;
            }
            if (key.Key == ConsoleKey.Enter)
            {
                System.Console.WriteLine();
                return ConsoleInput.CommandText(conversationMode.BuildCommand(line.Text));
            }
            if (key.Key == ConsoleKey.Backspace)
            {
                if (line.Backspace())
                {
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

    private static void WriteInteractivePrompt(ConsoleConversationMode conversationMode, ConsoleInputLine line)
    {
        ConsoleTheme.WritePrompt(conversationMode.Prompt);
        if (!line.IsEmpty) System.Console.Write(line.Text);
    }

    private static void RedrawInteractivePrompt(ConsoleConversationMode conversationMode, ConsoleInputLine line)
    {
        System.Console.Write("\r");
        System.Console.Write(ConsoleTheme.ClearLine);
        WriteInteractivePrompt(conversationMode, line);
    }

    private static void ExecuteCommand(
        OracleSimulation simulation,
        OracleSaveStore store,
        string savePath,
        IRealTimeSource realTime,
        string command,
        long now)
    {
        if (command.StartsWith('('))
        {
            ExecuteDirectCall(simulation, command, now);
            return;
        }
        if (command.Equals("help", StringComparison.OrdinalIgnoreCase)) { PrintHelp(); return; }
        if (command.Equals("status", StringComparison.OrdinalIgnoreCase)) { PrintStatus(simulation); return; }
        if (command.Equals("save", StringComparison.OrdinalIgnoreCase))
        {
            SaveCurrent(store, savePath, simulation, realTime);
            ConsoleTheme.WriteLine($"Checkpoint saved: {savePath}");
            return;
        }
        if (command.Equals("records world", StringComparison.OrdinalIgnoreCase)) { PrintRecords(simulation.Ledger.WorldRecords, "WORLD RECORD"); return; }
        if (command.Equals("records oracle", StringComparison.OrdinalIgnoreCase)) { PrintRecords(simulation.Ledger.OracleRecords, "ORACLE RECORD"); return; }
        if (command.Equals("calls", StringComparison.OrdinalIgnoreCase)) { PrintDirectCallTargets(simulation); return; }
        if (command.Equals("creation", StringComparison.OrdinalIgnoreCase) || command.Equals("powers", StringComparison.OrdinalIgnoreCase)) { PrintCreationPowers(simulation); return; }
        if (command.Equals("brain", StringComparison.OrdinalIgnoreCase)) { PrintYalaBrain(simulation); return; }
        if (command.Equals("events", StringComparison.OrdinalIgnoreCase)) { PrintEvents(simulation); return; }
        if (command.Equals("choices", StringComparison.OrdinalIgnoreCase)) { PrintChoices(simulation); return; }
        if (command.Equals("plans", StringComparison.OrdinalIgnoreCase)) { PrintReasonedPlans(simulation); return; }
        if (command.Equals("observations", StringComparison.OrdinalIgnoreCase)) { PrintObservations(simulation); return; }
        if (command.Equals("attention", StringComparison.OrdinalIgnoreCase)) { PrintAttention(simulation); return; }
        if (command.Equals("present next", StringComparison.OrdinalIgnoreCase)) { PresentNextLivingKind(simulation); return; }
        if (command.StartsWith("intervene ", StringComparison.OrdinalIgnoreCase)) { Intervene(simulation, command[10..]); return; }

        if (OracleQuestionInterpreter.TryAnswer(command, simulation.State, simulation.Observations, out IReadOnlyList<string> lines))
        {
            foreach (string line in lines) ConsoleTheme.WriteLine(line);
            return;
        }

        ConsoleTheme.WriteLine("Oracle system console did not recognise that command. Use (Yala <message> to contact Yala, or type help.");
    }

    private static void ExecuteDirectCall(OracleSimulation simulation, string command, long now)
    {
        if (!DirectCallParser.TryParse(command, simulation.State.DirectCallTargets, out DirectCall? call, out string? error) || call is null)
        {
            ConsoleTheme.WriteLine(error ?? "Direct call could not be resolved.");
            return;
        }

        if (call.Target.Key.Equals("yala", StringComparison.OrdinalIgnoreCase))
        {
            YalaDirectReply reply = simulation.CallYala(call.Message, now);
            ConsoleTheme.WriteLine($"Yala: {reply.Reply}");
            return;
        }

        OfferedChoiceState? choice = simulation.CallEntity(call.Target.Key, call.Message);
        ConsoleTheme.WriteLine($"Unplaced contact reached {call.Target.TargetName}. Oracle identity was not revealed.");
        if (choice is not null)
        {
            ConsoleTheme.WriteLine($"{call.Target.TargetName} selected: {choice.SelectedOption}");
        }
        else
        {
            ConsoleTheme.WriteLine("This being does not yet have an autonomous reply brain in v0.0.26.");
        }
    }

    private static void PrintStatus(OracleSimulation simulation)
    {
        CosmicState cosmic = simulation.State.Cosmic!;
        ConsoleTheme.WriteLine($"Yala: {simulation.State.Yala.Location}; sex: {simulation.State.Yala.Sex}; knows Oracle exists: {simulation.State.Yala.KnowsOfOracle}");
        ConsoleTheme.WriteLine($"Gaia created: {cosmic.GaiaCreated}");
        ConsoleTheme.WriteLine($"In-world Time created by Gaia: {cosmic.TimeCreated}");
        ConsoleTheme.WriteLine(simulation.InWorldTimeExists ? $"World time: {simulation.Clock.Describe()}" : "World time: does not yet exist");
        ConsoleTheme.WriteLine($"Lower world established: {cosmic.LowerWorldEstablished}");
        ConsoleTheme.WriteLine($"Yala decisions: {simulation.State.YalaCognition?.DecisionCount ?? 0}");
        ConsoleTheme.WriteLine($"Last Yala action: {simulation.State.YalaCognition?.LastAction ?? "none"}");
        ConsoleTheme.WriteLine($"Last Yala result: {simulation.State.YalaCognition?.LastResult ?? "none"}");
        ConsoleTheme.WriteLine($"Oracle interventions: {simulation.Interventions.Count}");
    }

    private static void PrintDirectCallTargets(OracleSimulation simulation)
    {
        ConsoleTheme.WriteLine("Current in-world direct-call targets:");
        foreach (DirectCallTargetState target in simulation.State.DirectCallTargets.Where(target => target.ReceivesDirectCall))
        {
            ConsoleTheme.WriteLine($"{target.Prompt} <message>  {target.TargetName}  {target.AuthoritySummary}");
        }
        ConsoleTheme.WriteLine("Oracle is not an in-world target. The console itself is Oracle's system interface.");
    }

    private static void PrintCreationPowers(OracleSimulation simulation)
    {
        ConsoleTheme.WriteLine("Current settled in-world order:");
        foreach (CreationPowerState power in simulation.State.CreationPowers.OrderBy(power => power.Order))
        {
            ConsoleTheme.WriteLine($"{power.Order}. {power.Name}: {power.Domain}. {power.AuthoritySummary}");
        }
        ConsoleTheme.WriteLine(ProjectOracle.Lore.OracleLore.PrimeSimulationLaw);
    }

    private static void PrintYalaBrain(OracleSimulation simulation)
    {
        YalaCognitionState cognition = simulation.State.YalaCognition ?? WorldDefaults.CreateInitialYalaCognition();
        YalaDriveState drives = cognition.Drives ?? WorldDefaults.CreateInitialDrives();
        ConsoleTheme.WriteLine($"Brain: {YalaSoarMind.BrainName} ({YalaSoarMind.Architecture})");
        ConsoleTheme.WriteLine($"Decisions: {cognition.DecisionCount}; conversations: {cognition.ConversationCount}");
        ConsoleTheme.WriteLine($"Last action: {cognition.LastAction ?? "none"}");
        ConsoleTheme.WriteLine($"Last result: {cognition.LastResult ?? "none"}");
        ConsoleTheme.WriteLine($"Drives: curiosity {drives.Curiosity}, caution {drives.Caution}, authority {drives.Authority}, companionship {drives.Companionship}, comfort {drives.Comfort}, uncertainty {drives.Uncertainty}");
        ConsoleTheme.WriteLine($"Contacts: {cognition.Contacts?.Count ?? 0}; beliefs/claims: {cognition.Beliefs?.Count ?? 0}; structured episodes: {cognition.Episodes?.Count ?? 0}");
        ConsoleTheme.WriteLine($"Dialogue turns: {cognition.Dialogue?.Count ?? 0}; relationships: {cognition.Relationships?.Count ?? 0}; questions: {cognition.Questions?.Count ?? 0}; temporal events: {cognition.TemporalEvents?.Count ?? 0}; goals: {cognition.Goals?.Count ?? 0}");
        ConsoleTheme.WriteLine($"Self-action memories: {cognition.ActionMemory?.Count ?? 0}; knowledge gaps: {cognition.KnowledgeGaps?.Count ?? 0}; learned word claims: {cognition.LearnedLexicon?.Count ?? 0}; base lexicon: {ProjectOracle.Cognition.Language.YalaLexicon.BuiltInCount}");
        YalaQuestionState? pendingQuestion = (cognition.Questions ?? []).Where(item => !item.Asked).OrderByDescending(item => item.Priority).FirstOrDefault();
        if (pendingQuestion is not null) ConsoleTheme.WriteLine($"Highest pending question: {pendingQuestion.Text}");
        YalaGoalState? activeGoal = (cognition.Goals ?? []).Where(item => item.Status == "active").OrderByDescending(item => item.Priority).FirstOrDefault();
        if (activeGoal is not null) ConsoleTheme.WriteLine($"Highest active goal: {activeGoal.Goal} | {activeGoal.Reason}");
        SoarMemoryDiagnostics diagnostics = simulation.GetYalaMemoryDiagnostics();
        ConsoleTheme.WriteLine($"Soar semantic memory: {diagnostics.SemanticNodes} node(s), {diagnostics.SemanticEdges} edge(s); episodic time: {diagnostics.EpisodicTime}");
        ConsoleTheme.WriteLine("Recent remembered state:");
        foreach (string memory in cognition.Memory.TakeLast(12)) ConsoleTheme.WriteLine($"  {memory}");
    }

    private static void Intervene(OracleSimulation simulation, string value)
    {
        string[] parts = value.Split('|', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || parts.Any(string.IsNullOrWhiteSpace))
        {
            ConsoleTheme.WriteLine("Use: intervene <vessel> | <message>");
            return;
        }
        OracleIntervention intervention = simulation.QueueVesselMessage(parts[0], parts[1]);
        ConsoleTheme.WriteLine($"Intervention {intervention.Id} queued. The vessel does not know Oracle's identity unless explicitly revealed later.");
    }

    private static void PresentNextLivingKind(OracleSimulation simulation)
    {
        LivingKindState? named = simulation.PresentNextLivingKindToAdam("The Garden");
        ConsoleTheme.WriteLine(named is null
            ? "The Adam naming scaffold is not active in this world state, or every kind is already named."
            : $"Adam named {named.AncientKind} as {named.AdamName}.");
    }

    private static void PrintEvents(OracleSimulation simulation)
    {
        if (simulation.ScheduledEvents.Count == 0) { ConsoleTheme.WriteLine("No scheduled world events."); return; }
        foreach (ScheduledWorldEvent item in simulation.ScheduledEvents.OrderBy(item => item.Id))
            ConsoleTheme.WriteLine($"{item.Id:0000} [{item.Status}] {item.Kind} at world ms {item.ScheduledForWorldMilliseconds}");
    }

    private static void PrintChoices(OracleSimulation simulation)
    {
        if (simulation.OfferedChoices.Count == 0) { ConsoleTheme.WriteLine("No offered choices recorded."); return; }
        foreach (OfferedChoiceState item in simulation.OfferedChoices.OrderBy(item => item.Id))
            ConsoleTheme.WriteLine($"{item.Id:0000} {item.ActorId}: {item.SelectedOption} | {item.Reason}");
    }

    private static void PrintReasonedPlans(OracleSimulation simulation)
    {
        if (simulation.ReasonedPlans.Count == 0) { ConsoleTheme.WriteLine("No Adam plans recorded in the current world."); return; }
        foreach (ReasonedPlanState item in simulation.ReasonedPlans.OrderBy(item => item.Id))
            ConsoleTheme.WriteLine($"{item.Id:0000} {item.ActorId}: {item.SelectedAction} via {item.BrainSystem}");
    }

    private static void PrintObservations(OracleSimulation simulation)
    {
        if (simulation.Observations.Count == 0) { ConsoleTheme.WriteLine("No observation records are active in the current world."); return; }
        foreach (ObservationState item in simulation.Observations.OrderBy(item => item.Id))
            ConsoleTheme.WriteLine($"{item.Id:0000} {item.ObserverName} observed {item.SubjectName}: {item.Detail}");
    }

    private static void PrintAttention(OracleSimulation simulation)
    {
        if (simulation.AttentionStates.Count == 0) { ConsoleTheme.WriteLine("No attention records are active in the current world."); return; }
        foreach (AttentionState item in simulation.AttentionStates.OrderBy(item => item.ActorName))
            ConsoleTheme.WriteLine($"{item.ActorName} -> {item.TargetName}: {item.Focus}");
    }

    private static void PrintRecords(IReadOnlyList<OracleRecord> records, string title)
    {
        ConsoleTheme.WriteLine();
        ConsoleTheme.WriteLine($"{title}:");
        foreach (OracleRecord record in records) ConsoleTheme.WriteLine($"[{record.Sequence:0000} | world ms {record.Tick}] {record.Message}");
        ConsoleTheme.WriteLine();
    }

    private static void SaveCurrent(OracleSaveStore store, string path, OracleSimulation simulation, IRealTimeSource realTime)
    {
        long now = realTime.GetUnixTimeMilliseconds();
        simulation.SynchroniseClock(now, recordAdvance: false);
        store.Save(path, simulation.CreateSnapshot(now));
    }

    private static void PrintHelp()
    {
        ConsoleTheme.WriteLine("(Yala <message>                 Contact Yala; Yala perceives an unplaced source, not Oracle.");
        ConsoleTheme.WriteLine("(Monad <message>                Contact Monad from the system console.");
        ConsoleTheme.WriteLine("(Wisdom <message>               Contact Wisdom from the system console.");
        ConsoleTheme.WriteLine("calls                           Show current direct-call targets.");
        ConsoleTheme.WriteLine("status                          Show current cosmology and Yala state.");
        ConsoleTheme.WriteLine("Ctrl+Y                          Enter persistent Yala conversation mode; the mode stays active after each reply.");
        ConsoleTheme.WriteLine("Escape                          Leave Yala conversation mode and return to the normal system prompt.");
        ConsoleTheme.WriteLine("brain                           Show Yala Brain Slice 9 state, plans, investigations, questions, goals, and memory diagnostics.");
        ConsoleTheme.WriteLine("creation / powers               Show currently existing in-world powers.");
        ConsoleTheme.WriteLine("records world                   Show settled in-world history.");
        ConsoleTheme.WriteLine("records oracle                  Show protected Oracle/system truth.");
        ConsoleTheme.WriteLine("events / choices                Show scheduled events and offered choices when they exist.");
        ConsoleTheme.WriteLine("plans / observations / attention Show later-world state when future history creates it.");
        ConsoleTheme.WriteLine("present next                    Present the next living kind if a future Adam naming mandate exists.");
        ConsoleTheme.WriteLine("intervene <vessel> | <message>  Use a later-world vessel when that world exists.");
        ConsoleTheme.WriteLine("save                            Save the current world and Yala cognition state.");
        ConsoleTheme.WriteLine("quit                            Save and exit.");
        ConsoleTheme.WriteLine("You can also ask system questions such as: who made Wisdom, who is Yala, who made Time, or what is Oracle?");
    }

    private sealed record ConsoleInput(string Command, bool EndOfInput)
    {
        public static ConsoleInput CommandText(string command) => new(command, false);
        public static ConsoleInput End() => new("", true);
    }

    private sealed record ConsoleOptions(ulong Seed, string? SavePath, bool Once, bool TerminalChild)
    {
        public static ConsoleOptions Parse(string[] args)
        {
            ulong seed = DefaultSeed; string? savePath = null; bool once = false; bool terminalChild = false;
            for (int index = 0; index < args.Length; index++)
            {
                string arg = args[index];
                if (arg.Equals("--once", StringComparison.OrdinalIgnoreCase)) { once = true; continue; }
                if (arg.Equals("--terminal-child", StringComparison.OrdinalIgnoreCase)) { terminalChild = true; continue; }
                if (arg.Equals("--seed", StringComparison.OrdinalIgnoreCase))
                {
                    if (++index >= args.Length || !ulong.TryParse(args[index], out seed)) throw new ArgumentException("--seed requires a whole number.");
                    continue;
                }
                if (arg.Equals("--save", StringComparison.OrdinalIgnoreCase))
                {
                    if (++index >= args.Length || string.IsNullOrWhiteSpace(args[index])) throw new ArgumentException("--save requires a file path.");
                    savePath = args[index]; continue;
                }
                throw new ArgumentException($"Unknown start option: {arg}");
            }
            return new ConsoleOptions(seed, savePath, once, terminalChild);
        }
    }

    private static class DesktopConsoleBootstrap
    {
        public static bool TryRelaunchInTerminal(string[] originalArgs)
        {
            if (!System.Console.IsInputRedirected && !System.Console.IsOutputRedirected) return false;
            string? executable = Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(executable)) return false;
            string? terminal = FindExecutable("gnome-terminal") ?? FindExecutable("ptyxis") ?? FindExecutable("kgx") ?? FindExecutable("x-terminal-emulator");
            if (terminal is null) return false;

            ProcessStartInfo start = new(terminal) { UseShellExecute = false };
            if (Path.GetFileName(terminal).Equals("gnome-terminal", StringComparison.OrdinalIgnoreCase)) start.ArgumentList.Add("--");
            else if (Path.GetFileName(terminal).Equals("x-terminal-emulator", StringComparison.OrdinalIgnoreCase)) start.ArgumentList.Add("-e");
            else start.ArgumentList.Add("--");
            start.ArgumentList.Add(executable);
            start.ArgumentList.Add("--terminal-child");
            foreach (string arg in originalArgs.Where(arg => !arg.Equals("--terminal-child", StringComparison.OrdinalIgnoreCase))) start.ArgumentList.Add(arg);
            Process.Start(start);
            return true;
        }

        private static string? FindExecutable(string name)
        {
            string? path = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrWhiteSpace(path)) return null;
            foreach (string directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                string candidate = Path.Combine(directory, name);
                if (File.Exists(candidate)) return candidate;
            }
            return null;
        }
    }

    private static Exception Unwrap(Exception error)
    {
        while (error.InnerException is not null && error is System.Reflection.TargetInvocationException) error = error.InnerException;
        return error;
    }
}
