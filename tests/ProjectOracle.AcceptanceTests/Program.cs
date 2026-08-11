using ProjectOracle;
using ProjectOracle.Cognition.Soar;
using ProjectOracle.Domain;
using ProjectOracle.Lore;
using ProjectOracle.Persistence;
using ProjectOracle.Simulation;

namespace ProjectOracle.AcceptanceTests;

internal static class Program
{
    private const long StartRealTime = 1_700_000_000_000;
    private static int _passed;
    private static int _failed;

    public static int Main()
    {
        Run("version is 0.0.18", () => Equal("0.0.18", ProjectVersion.Number));
        Run("fresh world starts with Yala in the Void", FreshWorldStartsWithYalaInVoid);
        Run("fresh Void contains no Garden Adam animals Gaia or Time", FreshVoidHasNoLaterCreation);
        Run("fresh world has no in-world Oracle entity or direct-call target", NoInWorldOracle);
        Run("Yala is both male and female and does not know Oracle exists", YalaNatureAndOracleBoundary);
        Run("Monad made Wisdom, Wisdom made Yala, and Monad rejected Yala for being both", CanonGenealogyAndRejection);
        Run("Monad is never labelled Creator or Omega in active lore", MonadNaming);
        Run("Gaia is not pre-created in a fresh world", GaiaNotPrecreated);
        Run("in-world Time is not pre-created", TimeNotPrecreated);
        Run("pre-Time runtime does not advance world milliseconds", PreTimeClockHolds);
        Run("Yala can create Gaia through world-law resolution", YalaCanCreateGaia);
        Run("Gaia creates Time after Yala commands temporal order", GaiaCreatesTime);
        Run("world clock starts advancing only after Gaia creates Time", ClockStartsAfterTime);
        Run("elemental names match current canon", ElementalCanon);
        Run("Oracle serpent reference is a manifestation, not a fixed identity", SerpentManifestationCanon);
        Run("Yala Soar agent source contains no Oracle knowledge", YalaAgentHasNoOracleKnowledge);
        Run("Soar 9.6.5 runtime files are discoverable", SoarRuntimeDiscoverable);
        Run("one Yala Soar session survives multiple decisions", PersistentSoarSession);
        Run("Soar uses an impasse/substate for undecided autonomous cognition", SoarSubstateDeliberation);
        Run("Soar semantic memory stores and retrieves an unseen contact claim", SoarSemanticMemory);
        Run("Soar episodic memory advances across decisions", SoarEpisodicMemory);
        Run("Yala answers hearing contact without asking Who speaks", YalaHearingReply);
        Run("Yala answers location without revealing Oracle", YalaLocationReply);
        Run("Yala knows both male and female aspects", YalaNatureReply);
        Run("Yala knows why Monad rejected Yala", YalaRejectionReply);
        Run("Yala records a claimed speaker identity as a claim", YalaIntroductionMemory);
        Run("Yala can remember a prior claimed speaker", YalaRememberMe);
        Run("native Soar semantic memory helps recognise repeated claimed contact", YalaRepeatedIntroductionUsesSemanticMemory);
        Run("ordinary statement no longer collapses to Who speaks", OrdinaryStatementConversation);
        Run("unknown question produces honest uncertainty", UnknownQuestionUncertainty);
        Run("conflicting speaker claim remains a rejected claim and does not rewrite truth", ConflictingClaimBoundary);
        Run("a direct command does not puppet Yala or directly alter world state", CommandDoesNotPuppetYala);
        Run("direct-call parser rejects Oracle because Oracle is not a target", DirectCallRejectsOracle);
        Run("Oracle and World records are distinct", RecordsAreDistinct);
        Run("World Record does not reveal Oracle at genesis", WorldRecordDoesNotRevealOracle);
        Run("Oracle Record retains protected system truth", OracleRecordHasSystemTruth);
        Run("structured Yala cognition survives save and restore", CognitionSurvivesSaveRestore);
        Run("v0.0.18 continues the v0.0.17 save_v2 world line", V017SaveContinuesAndNormalises);
        Run("v0.0.16 save snapshots remain rejected", V016SaveIsRejected);
        Run("default save path remains save_v2", SavePathRemainsV2);
        Run("fresh direct-call targets include Monad Wisdom Yala only", FreshCallTargets);
        Run("natural simulation law remains future-open", FutureOpenLaw);
        Run("typing buffer is independent of live-status refresh", InputBufferProtection);
        Run("asynchronous LIVE status is forbidden from the console body", LiveStatusTypingGuard);
        Run("interactive input path contains no LIVE surface refresh", NoLiveSurfaceInInteractivePath);
        Run("long command buffer survives protected-input handling", LongInputBuffer);
        Run("root launcher name is Project_Oracle_v0_0_18", NativeExecutableContract);

        Console.WriteLine();
        Console.WriteLine($"Acceptance result: {_passed} passed; {_failed} failed.");
        return _failed == 0 ? 0 : 1;
    }

    private static OracleSimulation Start(ulong seed = 104729, string? savePath = null) =>
        OracleSimulation.Start(seed, StartRealTime, savePath);

    private static YalaPerception Perception(
        bool gaiaCreated = false,
        bool timeCreated = false,
        int uncertainty = 80,
        string? contactMessage = null,
        YalaContactFrame? contact = null) =>
        new(
            "the Void", gaiaCreated, timeCreated, 0, null, null,
            Curiosity: 70, Caution: 55, Authority: 65, Companionship: 45, Comfort: 60, Uncertainty: uncertainty,
            contactMessage, contact);

    private static void FreshWorldStartsWithYalaInVoid()
    {
        using OracleSimulation simulation = Start();
        Equal("the Void", simulation.State.Yala.Location);
        False(simulation.State.Cosmic!.LowerWorldEstablished);
    }

    private static void FreshVoidHasNoLaterCreation()
    {
        using OracleSimulation simulation = Start();
        True(simulation.State.Garden is null);
        True(simulation.State.Adam is null);
        True(simulation.State.AdamSpark is null);
        Equal(0, simulation.State.LivingKinds.Count);
        False(simulation.State.Cosmic!.GaiaCreated);
        False(simulation.State.Cosmic.TimeCreated);
        False(simulation.State.CreationPowers.Any(power =>
            power.Name is "Gaia" or "Terra" or "Aether" or "Sol" or "Thalassa" or "Luna" or "Adam" or "Eden / Garden"));
    }

    private static void NoInWorldOracle()
    {
        using OracleSimulation simulation = Start();
        False(simulation.State.DirectCallTargets.Any(target => target.Key.Equals("oracle", StringComparison.OrdinalIgnoreCase)));
        False(simulation.State.CreationPowers.Any(power => power.Name.Equals("Oracle", StringComparison.OrdinalIgnoreCase)));
    }

    private static void YalaNatureAndOracleBoundary()
    {
        using OracleSimulation simulation = Start();
        Equal("male and female", simulation.State.Yala.Sex);
        False(simulation.State.Yala.KnowsOfOracle);
        True(simulation.State.YalaCognition!.Memory.Contains("I am both male and female."));
    }

    private static void CanonGenealogyAndRejection()
    {
        Contains(OracleLore.WisdomOrigin, "Monad made Sophia / Wisdom");
        Contains(OracleLore.YalaOrigin, "Wisdom made Yala alone");
        Contains(OracleLore.YalaOrigin, "both male and female");
        Contains(OracleLore.YalaVoid, "Monad rejected Yala because Yala is both male and female");
        Contains(OracleLore.YalaVoid, "cast Yala into the Void");
    }

    private static void MonadNaming()
    {
        False(OracleLore.MonadFoundation.Contains("Omega", StringComparison.OrdinalIgnoreCase));
        False(OracleLore.MonadFoundation.Contains("Creator", StringComparison.OrdinalIgnoreCase));
    }

    private static void GaiaNotPrecreated()
    {
        using OracleSimulation simulation = Start();
        False(simulation.State.Cosmic!.GaiaCreated);
    }

    private static void TimeNotPrecreated()
    {
        using OracleSimulation simulation = Start();
        False(simulation.State.Cosmic!.TimeCreated);
    }

    private static void PreTimeClockHolds()
    {
        using OracleSimulation simulation = Start();
        ClockAdvance advance = simulation.SynchroniseClock(StartRealTime + 60_000);
        Equal(0L, advance.ElapsedWorldMilliseconds);
        Equal(0L, simulation.Clock.WorldMilliseconds);
        Equal(StartRealTime + 60_000, simulation.Clock.LastRealUnixMilliseconds);
    }

    private static void YalaCanCreateGaia()
    {
        using OracleSimulation simulation = Start();
        simulation.ApplyYalaDecision(new YalaDecision("create-gaia", "none", "acceptance", "test"), StartRealTime + 1);
        True(simulation.State.Cosmic!.GaiaCreated);
        True(simulation.State.CreationPowers.Any(power => power.Name == "Gaia"));
        False(simulation.State.Cosmic.TimeCreated);
    }

    private static void GaiaCreatesTime()
    {
        using OracleSimulation simulation = Start();
        simulation.ApplyYalaDecision(new YalaDecision("create-gaia", "none", "acceptance", "test"), StartRealTime + 1);
        simulation.ApplyYalaDecision(new YalaDecision("command-gaia-time", "none", "acceptance", "test"), StartRealTime + 2);
        True(simulation.State.Cosmic!.TimeCreated);
        True(simulation.Ledger.WorldRecords.Any(record => record.Message.Contains("Gaia created in-world Time", StringComparison.Ordinal)));
    }

    private static void ClockStartsAfterTime()
    {
        using OracleSimulation simulation = Start();
        simulation.SynchroniseClock(StartRealTime + 1_000);
        simulation.ApplyYalaDecision(new YalaDecision("create-gaia", "none", "acceptance", "test"), StartRealTime + 1_001);
        simulation.ApplyYalaDecision(new YalaDecision("command-gaia-time", "none", "acceptance", "test"), StartRealTime + 1_002);
        ClockAdvance advance = simulation.SynchroniseClock(StartRealTime + 2_002);
        Equal(4_000L, advance.ElapsedWorldMilliseconds);
        Equal(4_000L, simulation.Clock.WorldMilliseconds);
    }

    private static void ElementalCanon()
    {
        Contains(OracleLore.ElementalOrder, "Terra is Earth");
        Contains(OracleLore.ElementalOrder, "Aether is Air and Wind");
        Contains(OracleLore.ElementalOrder, "Sol is Fire and the Sun power");
        Contains(OracleLore.ElementalOrder, "Thalassa is Water");
        Contains(OracleLore.ElementalOrder, "Luna is the Moon and is not an element");
    }

    private static void SerpentManifestationCanon()
    {
        Contains(OracleLore.OracleSerpentManifestation, "manifested in the form of a clever serpent");
        Contains(OracleLore.OracleSerpentManifestation, "Eve knew only the clever serpent");
        False(OracleLore.OracleSerpentManifestation.Contains("Oracle is the serpent", StringComparison.OrdinalIgnoreCase));
    }

    private static void YalaAgentHasNoOracleKnowledge()
    {
        SoarRuntimePaths paths = SoarRuntimePaths.Discover();
        string source = File.ReadAllText(paths.YalaAgent);
        string productionText = string.Join('\n', source.Split('\n').Where(line => !line.TrimStart().StartsWith('#')));
        False(productionText.Contains("oracle", StringComparison.OrdinalIgnoreCase));
    }

    private static void SoarRuntimeDiscoverable()
    {
        SoarRuntimePaths paths = SoarRuntimePaths.Discover();
        True(File.Exists(paths.ManagedBridge));
        True(File.Exists(paths.NativeBridge));
        True(File.Exists(paths.NativeKernel));
        True(File.Exists(paths.YalaAgent));
    }

    private static void PersistentSoarSession()
    {
        using YalaSoarMind mind = new();
        YalaDecision first = mind.Decide(Perception(gaiaCreated: true, timeCreated: true));
        YalaDecision second = mind.Decide(Perception(gaiaCreated: true, timeCreated: true, uncertainty: 40));
        Equal(2L, mind.SessionDecisionCount);
        Equal("Soar 9.6.5", first.Source);
        Equal("Soar 9.6.5", second.Source);
    }

    private static void SoarSubstateDeliberation()
    {
        using YalaSoarMind mind = new();
        YalaDecision decision = mind.Decide(Perception(gaiaCreated: true, timeCreated: true, uncertainty: 80));
        True(decision.UsedSubstateDeliberation);
        True(decision.DecisionCycles >= 2);
        Equal("observe", decision.Action);
    }

    private static void SoarSemanticMemory()
    {
        using YalaSoarMind mind = new();
        mind.RememberClaimedContact("Derek");
        True(mind.SemanticMemoryContainsClaimedContact("Derek"));
        SoarMemoryDiagnostics diagnostics = mind.GetMemoryDiagnostics();
        True(diagnostics.SemanticNodes >= 4);
        True(diagnostics.SemanticEdges >= 1);
    }

    private static void SoarEpisodicMemory()
    {
        using YalaSoarMind mind = new();
        long before = mind.GetMemoryDiagnostics().EpisodicTime;
        mind.Decide(Perception(gaiaCreated: true, timeCreated: true));
        long after = mind.GetMemoryDiagnostics().EpisodicTime;
        True(after > before);
    }

    private static void YalaHearingReply()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("Can you hear me?", StartRealTime + 1);
        Equal("Yes. I hear you.", reply.Reply);
        False(reply.Reply.Contains("Who speaks", StringComparison.OrdinalIgnoreCase));
    }

    private static void YalaLocationReply()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("Where are you?", StartRealTime + 1);
        Equal("I am in the Void.", reply.Reply);
        False(reply.Reply.Contains("Oracle", StringComparison.OrdinalIgnoreCase));
    }

    private static void YalaNatureReply()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("Are you male or female?", StartRealTime + 1);
        Contains(reply.Reply, "both male and female");
    }

    private static void YalaRejectionReply()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("Why did Monad reject you?", StartRealTime + 1);
        Contains(reply.Reply, "both male and female");
        Contains(reply.Reply, "cast me into the Void");
    }

    private static void YalaIntroductionMemory()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("I am Derek.", StartRealTime + 1);
        Contains(reply.Reply, "You call yourself Derek");
        Equal("Derek", simulation.State.YalaCognition!.LastSpeakerClaim);
        True(simulation.State.YalaCognition.Contacts!.Any(contact => contact.ClaimedName == "Derek"));
        True(simulation.State.YalaCognition.Beliefs!.Any(belief => belief.Proposition.Contains("I am Derek", StringComparison.OrdinalIgnoreCase)));
    }

    private static void YalaRememberMe()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("I am Derek.", StartRealTime + 1);
        YalaDirectReply reply = simulation.CallYala("Do you remember me?", StartRealTime + 2);
        Contains(reply.Reply, "called itself Derek");
        False(reply.Reply.Contains("Oracle", StringComparison.OrdinalIgnoreCase));
    }

    private static void YalaRepeatedIntroductionUsesSemanticMemory()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("I am Derek.", StartRealTime + 1);
        YalaDirectReply second = simulation.CallYala("I am Derek.", StartRealTime + 2);
        True(second.Contact.KnownContact);
        Contains(second.Reply, "heard the unseen speaker");
    }

    private static void OrdinaryStatementConversation()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("The Void seems empty.", StartRealTime + 1);
        False(reply.Reply.Contains("Who speaks", StringComparison.OrdinalIgnoreCase));
        True(reply.Decision.UsedSubstateDeliberation);
    }

    private static void UnknownQuestionUncertainty()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("What is beyond everything you know?", StartRealTime + 1);
        Equal("I do not know.", reply.Reply);
    }

    private static void ConflictingClaimBoundary()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("You created yourself.", StartRealTime + 1);
        Contains(reply.Reply, "conflicts with what I know");
        True(simulation.State.YalaCognition!.Beliefs!.Any(belief =>
            belief.Proposition.Contains("created yourself", StringComparison.OrdinalIgnoreCase) && belief.Status == "rejected-as-conflicting"));
        Contains(OracleLore.YalaOrigin, "Wisdom made Yala alone");
    }

    private static void CommandDoesNotPuppetYala()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("Create Gaia.", StartRealTime + 1);
        Contains(reply.Reply, "does not make it my decision");
        False(simulation.State.Cosmic!.GaiaCreated);
    }

    private static void DirectCallRejectsOracle()
    {
        using OracleSimulation simulation = Start();
        bool parsed = ProjectOracle.ConsoleApp.DirectCallParser.TryParse(
            "(Oracle who are you?", simulation.State.DirectCallTargets, out _, out _);
        False(parsed);
    }

    private static void RecordsAreDistinct()
    {
        using OracleSimulation simulation = Start();
        True(simulation.Ledger.WorldRecords.Count > 0);
        True(simulation.Ledger.OracleRecords.Count > 0);
        False(simulation.Ledger.WorldRecords.Intersect(simulation.Ledger.OracleRecords).Any());
    }

    private static void WorldRecordDoesNotRevealOracle()
    {
        using OracleSimulation simulation = Start();
        False(simulation.Ledger.WorldRecords.Any(record => record.Message.Contains("Oracle", StringComparison.OrdinalIgnoreCase)));
    }

    private static void OracleRecordHasSystemTruth()
    {
        using OracleSimulation simulation = Start();
        True(simulation.Ledger.OracleRecords.Any(record => record.Message.Contains("Master Key", StringComparison.OrdinalIgnoreCase)));
        True(simulation.Ledger.OracleRecords.Any(record => record.Message.Contains("clever serpent", StringComparison.OrdinalIgnoreCase)));
    }

    private static void CognitionSurvivesSaveRestore()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"project-oracle-v18-cognition-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string savePath = Path.Combine(directory, "save_v2.json");
        try
        {
            OracleSaveSnapshot snapshot;
            using (OracleSimulation simulation = Start(savePath: savePath))
            {
                simulation.CallYala("I am Derek.", StartRealTime + 1);
                snapshot = simulation.CreateSnapshot(StartRealTime + 2);
            }
            using OracleSimulation restored = OracleSimulation.Restore(snapshot, StartRealTime + 3, savePath);
            Equal("Derek", restored.State.YalaCognition!.LastSpeakerClaim);
            True(restored.State.YalaCognition.Contacts!.Any(contact => contact.ClaimedName == "Derek"));
            True(restored.State.YalaCognition.Episodes!.Count > 0);
            True(restored.State.YalaCognition.Beliefs!.Count > 0);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static void V017SaveContinuesAndNormalises()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"project-oracle-v17-continue-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            string path = Path.Combine(directory, "save_v2.json");
            using OracleSimulation simulation = Start(9);
            YalaCognitionState cognition = simulation.State.YalaCognition! with
            {
                Memory = ["I am Yala.", "I am male.", "Wisdom made me."]
            };
            OracleSaveSnapshot v17 = simulation.CreateSnapshot(StartRealTime) with
            {
                ProjectVersion = "0.0.17",
                World = simulation.State with
                {
                    Yala = simulation.State.Yala with { Sex = "male" },
                    YalaCognition = cognition
                }
            };
            OracleSaveStore store = new();
            store.Save(path, v17);
            OracleSaveSnapshot loaded = store.Load(path);
            Equal("male and female", loaded.World.Yala.Sex);
            True(loaded.World.YalaCognition!.Memory.Contains("I am both male and female."));
            False(loaded.World.YalaCognition.Memory.Contains("I am male."));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static void V016SaveIsRejected()
    {
        using OracleSimulation simulation = Start(9);
        OracleSaveSnapshot oldSnapshot = simulation.CreateSnapshot(StartRealTime) with { ProjectVersion = "0.0.16" };
        string directory = Path.Combine(Path.GetTempPath(), $"project-oracle-v16-reject-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            string path = Path.Combine(directory, "old-save.json");
            OracleSaveStore store = new();
            bool rejected = false;
            try { store.Save(path, oldSnapshot); }
            catch (InvalidDataException) { rejected = true; }
            True(rejected);
            False(File.Exists(path));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static void SavePathRemainsV2()
    {
        Equal("save_v2.json", Path.GetFileName(OracleSaveStore.DefaultPath()));
    }

    private static void FreshCallTargets()
    {
        using OracleSimulation simulation = Start();
        string[] keys = simulation.State.DirectCallTargets.Select(target => target.Key).OrderBy(key => key).ToArray();
        Equal("monad,wisdom,yala", string.Join(',', keys));
    }

    private static void FutureOpenLaw()
    {
        Contains(OracleLore.PrimeSimulationLaw, "Future history is not canon until it occurs");
        Contains(OracleLore.PotentialDemonOrigin, "not settled history until it occurs");
    }

    private static void InputBufferProtection()
    {
        ProjectOracle.ConsoleApp.ConsoleInputLine line = new();
        foreach (char value in "(Yala I am Derek") line.Append(value);
        string before = line.Text;
        False(ProjectOracle.ConsoleApp.LiveConsoleSurface.MayPaintVisibleStatus(new ProjectOracle.ConsoleApp.ConsoleInputLine()));
        Equal(before, line.Text);
        True(line.Backspace());
        line.Append('k');
        Equal("(Yala I am Derek", line.Text);
    }

    private static void LiveStatusTypingGuard()
    {
        ProjectOracle.ConsoleApp.ConsoleInputLine empty = new();
        ProjectOracle.ConsoleApp.ConsoleInputLine active = new();
        active.Append('x');
        False(ProjectOracle.ConsoleApp.LiveConsoleSurface.MayPaintVisibleStatus(empty));
        False(ProjectOracle.ConsoleApp.LiveConsoleSurface.MayPaintVisibleStatus(active));
    }

    private static void NoLiveSurfaceInInteractivePath()
    {
        string sourcePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "ProjectOracle.Console", "Program.cs");
        sourcePath = Path.GetFullPath(sourcePath);
        True(File.Exists(sourcePath));
        string source = File.ReadAllText(sourcePath);
        False(source.Contains("LiveConsoleSurface", StringComparison.Ordinal));
        False(source.Contains("surface.Refresh", StringComparison.Ordinal));
        False(source.Contains("SetCursorPosition", StringComparison.Ordinal));
    }

    private static void LongInputBuffer()
    {
        ProjectOracle.ConsoleApp.ConsoleInputLine line = new();
        string longText = new('x', 4096);
        foreach (char value in longText) line.Append(value);
        Equal(4096, line.Length);
        Equal(longText, line.Text);
        for (int i = 0; i < 96; i++) True(line.Backspace());
        Equal(4000, line.Length);
    }

    private static void NativeExecutableContract()
    {
        string root = SoarRuntimePaths.Discover().RepositoryRoot;
        string expected = Path.Combine(root, "Project_Oracle_v0_0_18");
        if (Environment.GetEnvironmentVariable("PROJECT_ORACLE_REQUIRE_NATIVE_EXECUTABLE") == "1")
        {
            True(File.Exists(expected));
        }
        else
        {
            Equal("Project_Oracle_v0_0_18", Path.GetFileName(expected));
        }
    }

    private static void Run(string name, Action test)
    {
        try
        {
            test();
            _passed++;
            Console.WriteLine($"PASS: {name}");
        }
        catch (Exception error)
        {
            _failed++;
            Console.WriteLine($"FAIL: {name}: {error.Message}");
        }
    }

    private static void True(bool value)
    {
        if (!value) throw new InvalidOperationException("expected true");
    }

    private static void False(bool value)
    {
        if (value) throw new InvalidOperationException("expected false");
    }

    private static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new InvalidOperationException($"expected '{expected}', got '{actual}'");
    }

    private static void Contains(string value, string expected)
    {
        if (!value.Contains(expected, StringComparison.Ordinal))
            throw new InvalidOperationException($"expected text containing '{expected}'");
    }
}
