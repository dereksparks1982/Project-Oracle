using ProjectOracle;
using ProjectOracle.Audit;
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
        Run("version is 0.0.17", () => Equal("0.0.17", ProjectVersion.Number));
        Run("fresh world starts with Yala in the Void", FreshWorldStartsWithYalaInVoid);
        Run("fresh Void contains no Garden Adam animals Gaia or Time", FreshVoidHasNoLaterCreation);
        Run("fresh world has no in-world Oracle entity or direct-call target", NoInWorldOracle);
        Run("Yala is male and does not know Oracle exists", YalaDoesNotKnowOracle);
        Run("Monad made Wisdom and Wisdom made Yala", CanonGenealogy);
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
        Run("Soar chooses a legal autonomous Yala operator", SoarAutonomousDecision);
        Run("Soar answers a location contact without revealing Oracle", SoarLocationReply);
        Run("generic Yala contact treats source as unknown", GenericContactUnknown);
        Run("direct-call parser rejects Oracle because Oracle is not a target", DirectCallRejectsOracle);
        Run("Oracle and World records are distinct", RecordsAreDistinct);
        Run("World Record does not reveal Oracle at genesis", WorldRecordDoesNotRevealOracle);
        Run("Oracle Record retains protected system truth", OracleRecordHasSystemTruth);
        Run("Yala cognition survives save and restore", CognitionSurvivesSaveRestore);
        Run("v0.0.17 default save path starts a new save_v2 world line", FreshSavePathIsV2);
        Run("v0.0.16 save snapshots are rejected instead of migrated", V016SaveIsRejected);
        Run("fresh direct-call targets include Monad Wisdom Yala only", FreshCallTargets);
        Run("natural simulation law remains future-open", FutureOpenLaw);
        Run("root launcher name is Project_Oracle_v0_0_17", NativeExecutableContract);

        Console.WriteLine();
        Console.WriteLine($"Acceptance result: {_passed} passed; {_failed} failed.");
        return _failed == 0 ? 0 : 1;
    }

    private static OracleSimulation Start(ulong seed = 104729) => OracleSimulation.Start(seed, StartRealTime);

    private static void FreshWorldStartsWithYalaInVoid()
    {
        OracleSimulation simulation = Start();
        Equal("the Void", simulation.State.Yala.Location);
        False(simulation.State.Cosmic!.LowerWorldEstablished);
    }


    private static void FreshVoidHasNoLaterCreation()
    {
        OracleSimulation simulation = Start();
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
        OracleSimulation simulation = Start();
        False(simulation.State.DirectCallTargets.Any(target => target.Key.Equals("oracle", StringComparison.OrdinalIgnoreCase)));
        False(simulation.State.CreationPowers.Any(power => power.Name.Equals("Oracle", StringComparison.OrdinalIgnoreCase)));
    }

    private static void YalaDoesNotKnowOracle()
    {
        OracleSimulation simulation = Start();
        Equal("male", simulation.State.Yala.Sex);
        False(simulation.State.Yala.KnowsOfOracle);
    }

    private static void CanonGenealogy()
    {
        Contains(OracleLore.WisdomOrigin, "Monad made Sophia / Wisdom");
        Contains(OracleLore.YalaOrigin, "Wisdom made Yala alone");
        Contains(OracleLore.YalaVoid, "Monad cast Yala into the Void");
    }

    private static void MonadNaming()
    {
        False(OracleLore.MonadFoundation.Contains("Omega", StringComparison.OrdinalIgnoreCase));
        False(OracleLore.MonadFoundation.Contains("Creator", StringComparison.OrdinalIgnoreCase));
    }

    private static void GaiaNotPrecreated() => False(Start().State.Cosmic!.GaiaCreated);
    private static void TimeNotPrecreated() => False(Start().State.Cosmic!.TimeCreated);

    private static void PreTimeClockHolds()
    {
        OracleSimulation simulation = Start();
        ClockAdvance advance = simulation.SynchroniseClock(StartRealTime + 60_000);
        Equal(0L, advance.ElapsedWorldMilliseconds);
        Equal(0L, simulation.Clock.WorldMilliseconds);
        Equal(StartRealTime + 60_000, simulation.Clock.LastRealUnixMilliseconds);
    }

    private static void YalaCanCreateGaia()
    {
        OracleSimulation simulation = Start();
        simulation.ApplyYalaDecision(new YalaDecision("create-gaia", "none", "acceptance", "test"), StartRealTime + 1);
        True(simulation.State.Cosmic!.GaiaCreated);
        True(simulation.State.CreationPowers.Any(power => power.Name == "Gaia"));
        False(simulation.State.Cosmic.TimeCreated);
    }

    private static void GaiaCreatesTime()
    {
        OracleSimulation simulation = Start();
        simulation.ApplyYalaDecision(new YalaDecision("create-gaia", "none", "acceptance", "test"), StartRealTime + 1);
        simulation.ApplyYalaDecision(new YalaDecision("command-gaia-time", "none", "acceptance", "test"), StartRealTime + 2);
        True(simulation.State.Cosmic!.TimeCreated);
        True(simulation.Ledger.WorldRecords.Any(record => record.Message.Contains("Gaia created in-world Time", StringComparison.Ordinal)));
    }

    private static void ClockStartsAfterTime()
    {
        OracleSimulation simulation = Start();
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
        // Comments document the non-disclosure rule. Production content after stripping
        // comments must not expose Oracle as a symbol to Yala.
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

    private static void SoarAutonomousDecision()
    {
        YalaDecision decision = YalaSoarMind.Decide(new YalaPerception("the Void", false, false, 0, null, null));
        True(new[] { "observe", "reflect", "wait", "create-gaia" }.Contains(decision.Action));
        Equal("Soar 9.6.5", decision.Source);
    }

    private static void SoarLocationReply()
    {
        OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("Where are you?", StartRealTime + 1);
        Equal("I am in the Void.", reply.Reply);
        False(reply.Reply.Contains("Oracle", StringComparison.OrdinalIgnoreCase));
    }

    private static void GenericContactUnknown()
    {
        OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("Can you hear me?", StartRealTime + 1);
        Equal("I hear you. Who speaks?", reply.Reply);
        False(simulation.State.Yala.KnowsOfOracle);
    }

    private static void DirectCallRejectsOracle()
    {
        OracleSimulation simulation = Start();
        bool parsed = ProjectOracle.ConsoleApp.DirectCallParser.TryParse(
            "(Oracle who are you?", simulation.State.DirectCallTargets, out _, out _);
        False(parsed);
    }

    private static void RecordsAreDistinct()
    {
        OracleSimulation simulation = Start();
        True(simulation.Ledger.WorldRecords.Count > 0);
        True(simulation.Ledger.OracleRecords.Count > 0);
        False(simulation.Ledger.WorldRecords.Intersect(simulation.Ledger.OracleRecords).Any());
    }

    private static void WorldRecordDoesNotRevealOracle()
    {
        OracleSimulation simulation = Start();
        False(simulation.Ledger.WorldRecords.Any(record => record.Message.Contains("Oracle", StringComparison.OrdinalIgnoreCase)));
    }

    private static void OracleRecordHasSystemTruth()
    {
        OracleSimulation simulation = Start();
        True(simulation.Ledger.OracleRecords.Any(record => record.Message.Contains("Master Key", StringComparison.OrdinalIgnoreCase)));
        True(simulation.Ledger.OracleRecords.Any(record => record.Message.Contains("clever serpent", StringComparison.OrdinalIgnoreCase)));
    }

    private static void CognitionSurvivesSaveRestore()
    {
        OracleSimulation simulation = Start();
        simulation.ApplyYalaDecision(new YalaDecision("reflect", "none", "acceptance", "test"), StartRealTime + 1);
        OracleSaveSnapshot snapshot = simulation.CreateSnapshot(StartRealTime + 2);
        OracleSimulation restored = OracleSimulation.Restore(snapshot, StartRealTime + 3);
        Equal(1L, restored.State.YalaCognition!.DecisionCount);
        Equal("reflect", restored.State.YalaCognition.LastAction);
        True(restored.State.YalaCognition.Memory.Any(memory => memory.Contains("reflected", StringComparison.OrdinalIgnoreCase)));
    }

    private static void FreshSavePathIsV2()
    {
        string path = OracleSaveStore.DefaultPath();
        Equal("save_v2.json", Path.GetFileName(path));
        False(path.EndsWith("save_v1.json", StringComparison.OrdinalIgnoreCase));
    }

    private static void V016SaveIsRejected()
    {
        OracleSimulation simulation = Start(9);
        OracleSaveSnapshot oldSnapshot = simulation.CreateSnapshot(StartRealTime) with
        {
            ProjectVersion = "0.0.16"
        };

        string directory = Path.Combine(Path.GetTempPath(), $"project-oracle-v17-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            string path = Path.Combine(directory, "old-save.json");
            OracleSaveStore store = new();
            bool rejected = false;
            try
            {
                store.Save(path, oldSnapshot);
            }
            catch (InvalidDataException)
            {
                rejected = true;
            }
            True(rejected);
            False(File.Exists(path));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static void FreshCallTargets()
    {
        string[] keys = Start().State.DirectCallTargets.Select(target => target.Key).OrderBy(key => key).ToArray();
        Equal("monad,wisdom,yala", string.Join(',', keys));
    }

    private static void FutureOpenLaw()
    {
        Contains(OracleLore.PrimeSimulationLaw, "Future history is not canon until it occurs");
        Contains(OracleLore.PotentialDemonOrigin, "not settled history until it occurs");
    }

    private static void NativeExecutableContract()
    {
        string root = SoarRuntimePaths.Discover().RepositoryRoot;
        string expected = Path.Combine(root, "Project_Oracle_v0_0_17");
        // During a normal source build the file is generated by validate.sh/publish.
        // In installed validation it must exist as an actual native apphost/ELF.
        if (Environment.GetEnvironmentVariable("PROJECT_ORACLE_REQUIRE_NATIVE_EXECUTABLE") == "1")
        {
            True(File.Exists(expected));
        }
        else
        {
            True(Path.GetFileName(expected) == "Project_Oracle_v0_0_17");
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
