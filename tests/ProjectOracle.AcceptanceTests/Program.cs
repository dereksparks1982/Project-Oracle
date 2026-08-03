using ProjectOracle.Interventions;
using ProjectOracle.Persistence;
using ProjectOracle.Domain;
using ProjectOracle.Simulation;

namespace ProjectOracle.AcceptanceTests;

internal static class Program
{
    private const long StartRealTime = 1_800_000_000_000;
    private static int _passed;
    private static int _failed;

    public static int Main()
    {
        Run("same seed gives same random sequence", SameSeedGivesSameRandomSequence);
        Run("different seeds diverge", DifferentSeedsDiverge);
        Run("one real second becomes four world seconds", RealTimeScaleIsFour);
        Run("six real hours become one Garden day", SixHoursBecomeOneGardenDay);
        Run("one real day becomes four Garden days", OneRealDayBecomesFourGardenDays);
        Run("the world begins at Year 1 Month 1 Day 1 01:01:01", EpochIsExact);
        Run("the epoch begins under night and a new moon", EpochSkyIsExact);
        Run("lunar phases advance deterministically", LunarPhasesAdvanceDeterministically);
        Run("backwards system time never rewinds the world", BackwardsTimeCannotRewind);
        Run("offline catch-up is recorded", OfflineCatchUpIsRecorded);
        Run("save and restore preserve world time", SaveAndRestorePreserveWorldTime);
        Run("version 0.1.1 save upgrades through current world defaults", Version011SaveUpgradesThroughCurrentDefaults);
        Run("restore applies closed-time catch-up once", RestoreAppliesClosedTimeCatchUpOnce);
        Run("corrupt primary recovers last-good backup", CorruptPrimaryRecoversBackup);
        Run("Adam begins confined to the Garden", AdamBeginsConfined);
        Run("Yala knows the future language mandate", YalaKnowsLanguageMandate);
        Run("Spark is protected from Yala", SparkIsProtected);
        Run("Yala true name stays out of world records", TrueNameDoesNotLeak);
        Run("Creator truth stays out of world records", CreatorTruthDoesNotLeak);
        Run("world seed creates deterministic living kinds", WorldSeedCreatesDeterministicLivingKinds);
        Run("address channels follow the appointed hierarchy", AddressChannelsAreAppointed);
        Run("Adam begins with the naming mandate", AdamBeginsWithNamingMandate);
        Run("Natural Course rule is active", NaturalCourseRuleIsActive);
        Run("presenting a living kind lets Adam name it without finding a mate", PresentingLivingKindNamesIt);
        Run("direct address to Adam is recorded without puppeteering him", DirectAddressToAdamDoesNotPuppet);
        Run("vessel message is queued without forcing Adam", InterventionDoesNotForceAdam);
        Run("intervention contamination is recorded", InterventionContaminationIsRecorded);
        Run("records keep stable sequence order", RecordsKeepStableOrder);
        Run("version is 0.1.5", VersionIsCorrect);

        Console.WriteLine();
        Console.WriteLine($"Acceptance result: {_passed} passed; {_failed} failed.");
        return _failed == 0 ? 0 : 1;
    }

    private static OracleSimulation Start(ulong seed = 11) => OracleSimulation.Start(seed, StartRealTime);

    private static void SameSeedGivesSameRandomSequence()
    {
        DeterministicRandom first = new(77);
        DeterministicRandom second = new(77);
        for (int index = 0; index < 100; index++)
        {
            Equal(first.NextUInt64(), second.NextUInt64());
        }
    }

    private static void DifferentSeedsDiverge()
    {
        DeterministicRandom first = new(77);
        DeterministicRandom second = new(78);
        NotEqual(first.NextUInt64(), second.NextUInt64());
    }

    private static void RealTimeScaleIsFour()
    {
        OracleSimulation simulation = Start();
        ClockAdvance advance = simulation.SynchroniseClock(StartRealTime + 1_000);
        Equal(4_000L, advance.ElapsedWorldMilliseconds);
        Equal(4_000L, simulation.Clock.WorldMilliseconds);
    }

    private static void SixHoursBecomeOneGardenDay()
    {
        OracleSimulation simulation = Start();
        simulation.SynchroniseClock(StartRealTime + PersistentWorldClock.RealMillisecondsPerWorldDay);
        Equal(2L, simulation.Clock.DayNumber);
        Equal(1, simulation.Clock.Hour);
    }

    private static void OneRealDayBecomesFourGardenDays()
    {
        OracleSimulation simulation = Start();
        simulation.SynchroniseClock(StartRealTime + 86_400_000);
        Equal(5L, simulation.Clock.DayNumber);
        Equal(345_600_000L, simulation.Clock.WorldMilliseconds);
    }

    private static void EpochIsExact()
    {
        CalendarSnapshot calendar = Start().Clock.Calendar;
        Equal(1L, calendar.Year);
        Equal(1, calendar.Month);
        Equal(1, calendar.Day);
        Equal(1, calendar.Hour);
        Equal(1, calendar.Minute);
        Equal(1, calendar.Second);
    }

    private static void EpochSkyIsExact()
    {
        CalendarSnapshot calendar = Start().Clock.Calendar;
        Equal("night", calendar.SolarPhase);
        Equal("new moon", calendar.LunarPhase);
    }

    private static void LunarPhasesAdvanceDeterministically()
    {
        OracleSimulation simulation = Start();
        long worldAdvance = (OracleCalendar.LunarCycleMilliseconds / 2) - OracleCalendar.EpochTimeOfDayMilliseconds;
        long realAdvance = (worldAdvance + PersistentWorldClock.WorldSecondsPerRealSecond - 1)
            / PersistentWorldClock.WorldSecondsPerRealSecond;
        simulation.SynchroniseClock(StartRealTime + realAdvance);
        Equal("full moon", simulation.Clock.Calendar.LunarPhase);
    }

    private static void BackwardsTimeCannotRewind()
    {
        OracleSimulation simulation = Start();
        simulation.SynchroniseClock(StartRealTime + 10_000);
        long before = simulation.Clock.WorldMilliseconds;
        ClockAdvance advance = simulation.SynchroniseClock(StartRealTime + 5_000);
        True(advance.BackwardClockDetected);
        Equal(before, simulation.Clock.WorldMilliseconds);
    }

    private static void OfflineCatchUpIsRecorded()
    {
        OracleSimulation simulation = Start();
        simulation.SynchroniseClock(StartRealTime + 60_000, offlineCatchUp: true);
        Equal(1, simulation.Clock.CatchUpRuns);
        Equal(60_000L, simulation.Clock.LastOfflineElapsedRealMilliseconds);
        True(simulation.Ledger.CreatorRecords.Any(record =>
            record.Message.Contains("offline catch-up", StringComparison.OrdinalIgnoreCase)));
    }

    private static void SaveAndRestorePreserveWorldTime()
    {
        WithTemporarySave((store, path) =>
        {
            OracleSimulation simulation = Start();
            simulation.SynchroniseClock(StartRealTime + 10_000);
            store.Save(path, simulation.CreateSnapshot(StartRealTime + 10_000));
            OracleSaveSnapshot loaded = store.Load(path);
            Equal(40_000L, loaded.WorldMilliseconds);
            Equal(StartRealTime + 10_000, loaded.LastRealUnixMilliseconds);
        });
    }

    private static void Version011SaveUpgradesThroughCurrentDefaults()
    {
        WithTemporarySave((store, path) =>
        {
            OracleSimulation simulation = Start(104729);
            OracleSaveSnapshot legacySnapshot = simulation.CreateSnapshot(StartRealTime) with
            {
                ProjectVersion = "0.1.1",
                World = simulation.State with
                {
                    AddressChannels = [],
                    LivingKinds = [],
                    NamingMandate = null!,
                    NaturalCourse = null!
                }
            };

            store.Save(path, legacySnapshot);
            OracleSaveSnapshot loaded = store.Load(path);

            Equal("0.1.1", loaded.ProjectVersion);
            True(loaded.World.AddressChannels.Count > 0);
            True(loaded.World.LivingKinds.Count > 0);
            True(loaded.World.NamingMandate.Active);
            True(loaded.World.NaturalCourse.Active);
        });
    }

    private static void RestoreAppliesClosedTimeCatchUpOnce()
    {
        OracleSimulation simulation = Start();
        OracleSaveSnapshot snapshot = simulation.CreateSnapshot(StartRealTime);
        OracleSimulation restored = OracleSimulation.Restore(snapshot, StartRealTime + 21_600_000);
        Equal(86_400_000L, restored.Clock.WorldMilliseconds);
        Equal(1, restored.Clock.CatchUpRuns);
    }

    private static void CorruptPrimaryRecoversBackup()
    {
        WithTemporarySave((store, path) =>
        {
            OracleSimulation simulation = Start();
            store.Save(path, simulation.CreateSnapshot(StartRealTime));
            simulation.SynchroniseClock(StartRealTime + 1_000);
            store.Save(path, simulation.CreateSnapshot(StartRealTime + 1_000));
            File.WriteAllText(path, "not valid JSON");
            OracleSaveSnapshot recovered = store.Load(path);
            Equal(0L, recovered.WorldMilliseconds);
        });
    }

    private static void AdamBeginsConfined()
    {
        OracleSimulation simulation = Start();
        True(simulation.State.Adam.IsConfinedToGarden);
        False(simulation.State.Garden.BoundaryOpen);
    }

    private static void YalaKnowsLanguageMandate()
    {
        OracleSimulation simulation = Start();
        True(simulation.State.Yala.KnowsFutureLanguageMandate);
    }

    private static void SparkIsProtected()
    {
        OracleSimulation simulation = Start();
        False(simulation.State.AdamSpark.CanBeReadByYala);
        False(simulation.State.AdamSpark.CanBeRewrittenByYala);
    }

    private static void TrueNameDoesNotLeak()
    {
        OracleSimulation simulation = Start();
        False(simulation.Ledger.WorldRecords.Any(record =>
            record.Message.Contains("Yala", StringComparison.OrdinalIgnoreCase)));
    }

    private static void CreatorTruthDoesNotLeak()
    {
        OracleSimulation simulation = Start();
        False(simulation.Ledger.WorldRecords.Any(record =>
            record.Message.Contains("Spark", StringComparison.OrdinalIgnoreCase) ||
            record.Message.Contains("Creators", StringComparison.OrdinalIgnoreCase)));
    }

    private static void WorldSeedCreatesDeterministicLivingKinds()
    {
        OracleSimulation first = Start(104729);
        OracleSimulation second = Start(104729);
        OracleSimulation different = Start(104730);

        True(first.State.LivingKinds.Count >= 6);
        Equal(
            first.State.LivingKinds.Select(kind => kind.Id.Value).ToArray(),
            second.State.LivingKinds.Select(kind => kind.Id.Value).ToArray());
        Equal(
            first.State.LivingKinds.Select(kind => kind.AncientKind).ToArray(),
            second.State.LivingKinds.Select(kind => kind.AncientKind).ToArray());
        False(first.State.LivingKinds.Select(kind => kind.AncientKind).SequenceEqual(
            different.State.LivingKinds.Select(kind => kind.AncientKind)));
    }

    private static void AddressChannelsAreAppointed()
    {
        OracleSimulation simulation = Start();
        string[] keys = simulation.State.AddressChannels.Select(channel => $"{channel.FunctionKey}:{channel.Key}").ToArray();
        Equal(new[] { "F1:oracle", "F2:gaia", "F3:adam", "F4:sun", "F5:moon" }, keys);
        True(simulation.State.AddressChannels.First(channel => channel.Key == "oracle").AuthoritySummary.Contains("above Gaia", StringComparison.Ordinal));
    }

    private static void AdamBeginsWithNamingMandate()
    {
        OracleSimulation simulation = Start();
        True(simulation.State.NamingMandate.Active);
        Equal(simulation.State.LivingKinds.Count, simulation.State.NamingMandate.TotalLivingKinds);
        Equal(0, simulation.State.NamingMandate.NamedCount);
        False(simulation.State.NamingMandate.SuitableMateFound);
        True(simulation.Ledger.WorldRecords.Any(record =>
            record.Message.Contains("naming the living kinds", StringComparison.OrdinalIgnoreCase)));
    }

    private static void NaturalCourseRuleIsActive()
    {
        OracleSimulation simulation = Start();
        True(simulation.State.NaturalCourse.Active);
        True(simulation.State.NaturalCourse.RuleText.Contains("appointed nature", StringComparison.OrdinalIgnoreCase));
    }

    private static void PresentingLivingKindNamesIt()
    {
        OracleSimulation simulation = Start();
        LivingKindState? named = simulation.PresentNextLivingKindToAdam("Gaia");
        True(named is not null);
        Equal(1, simulation.State.NamingMandate.PresentedCount);
        Equal(1, simulation.State.NamingMandate.NamedCount);
        False(simulation.State.NamingMandate.SuitableMateFound);
        True(simulation.State.LivingKinds[0].AdamName is not null);
        True(simulation.Ledger.WorldRecords.Any(record =>
            record.Message.Contains("No suitable mate", StringComparison.OrdinalIgnoreCase)));
    }

    private static void DirectAddressToAdamDoesNotPuppet()
    {
        OracleSimulation simulation = Start();
        simulation.AddressChannel("adam", "Name what Gaia presents to you.");
        True(simulation.Ledger.CreatorRecords.Any(record =>
            record.Message.Contains("addressed Adam", StringComparison.OrdinalIgnoreCase)));
        True(simulation.Ledger.WorldRecords.Any(record =>
            record.Message.Contains("Adam heard a direct address", StringComparison.OrdinalIgnoreCase)));
        False(simulation.Ledger.WorldRecords.Any(record =>
            record.Message.Contains("obeyed", StringComparison.OrdinalIgnoreCase) ||
            record.Message.Contains("refused", StringComparison.OrdinalIgnoreCase)));
    }

    private static void InterventionDoesNotForceAdam()
    {
        OracleSimulation simulation = Start();
        CreatorIntervention intervention = simulation.QueueVesselMessage("serpent", "Eat the fruit and know the truth.");
        Equal(InterventionStatus.Queued, intervention.Status);
        False(simulation.Ledger.WorldRecords.Any(record =>
            record.Message.Contains("accepted", StringComparison.OrdinalIgnoreCase) ||
            record.Message.Contains("refused", StringComparison.OrdinalIgnoreCase)));
    }

    private static void InterventionContaminationIsRecorded()
    {
        OracleSimulation simulation = Start();
        CreatorIntervention intervention = simulation.QueueVesselMessage("cat", "Look beyond the boundary.");
        True(intervention.ContaminatesExperiment);
        True(simulation.Ledger.CreatorRecords.Any(record =>
            record.Message.Contains("contaminated", StringComparison.OrdinalIgnoreCase)));
    }

    private static void RecordsKeepStableOrder()
    {
        OracleSimulation simulation = Start();
        simulation.SynchroniseClock(StartRealTime + 2_000);
        long[] sequences = simulation.Ledger.CreatorRecords.Select(record => record.Sequence).ToArray();
        Equal(sequences.Order().ToArray(), sequences);
    }

    private static void VersionIsCorrect() => Equal("0.1.5", ProjectVersion.Number);

    private static void WithTemporarySave(Action<OracleSaveStore, string> test)
    {
        string directory = Path.Combine(Path.GetTempPath(), "ProjectOracleAcceptance", Guid.NewGuid().ToString("N"));
        string path = Path.Combine(directory, "save.json");
        Directory.CreateDirectory(directory);
        try
        {
            test(new OracleSaveStore(), path);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
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
            Console.WriteLine($"FAIL: {name} — {error.Message}");
        }
    }

    private static void True(bool value)
    {
        if (!value)
        {
            throw new InvalidOperationException("Expected true but received false.");
        }
    }

    private static void False(bool value) => True(!value);

    private static void Equal<T>(T expected, T actual)
    {
        if (expected is Array expectedArray && actual is Array actualArray)
        {
            if (!expectedArray.Cast<object>().SequenceEqual(actualArray.Cast<object>()))
            {
                throw new InvalidOperationException("The sequences were different.");
            }

            return;
        }

        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"Expected {expected}; received {actual}.");
        }
    }

    private static void NotEqual<T>(T first, T second)
    {
        if (EqualityComparer<T>.Default.Equals(first, second))
        {
            throw new InvalidOperationException($"Expected different values but both were {first}.");
        }
    }
}
