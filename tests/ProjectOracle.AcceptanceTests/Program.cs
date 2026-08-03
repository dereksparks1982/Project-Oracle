using ProjectOracle.Interventions;
using ProjectOracle.Persistence;
using ProjectOracle.ConsoleApp;
using ProjectOracle.Domain;
using ProjectOracle.Events;
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
        Run("version 0.1.4 save upgrades through current world defaults", Version014SaveUpgradesThroughCurrentDefaults);
        Run("version 0.1.6 save upgrades through current event defaults", Version016SaveUpgradesThroughCurrentEventDefaults);
        Run("restore applies closed-time catch-up once", RestoreAppliesClosedTimeCatchUpOnce);
        Run("corrupt primary recovers last-good backup", CorruptPrimaryRecoversBackup);
        Run("Adam begins confined to the Garden", AdamBeginsConfined);
        Run("Yala knows the future language mandate", YalaKnowsLanguageMandate);
        Run("Yala and the Oracle are the same identity", YalaAndOracleAreSameIdentity);
        Run("Yala may overclaim authority but Creator records outrank her claim", YalaMayOverclaimAuthority);
        Run("Spark is protected from Yala", SparkIsProtected);
        Run("World Record begins with void and Yala before Adam", WorldRecordBeginsWithVoidAndYala);
        Run("World Record is creator-facing, not Adam knowledge", WorldRecordIsCreatorFacing);
        Run("creation order records void, Yala, world, Garden, Adam, and animals", CreationOrderRecordsWorldPowers);
        Run("version 0.1.7 save upgrades through current creation powers", Version017SaveUpgradesThroughCurrentCreationPowers);
        Run("version 0.1.8 save upgrades through corrected creation powers", Version018SaveUpgradesThroughCorrectedCreationPowers);
        Run("Oracle answers creation-order questions", OracleAnswersCreationOrderQuestions);
        Run("Oracle answers as Yala, not as a separate witness", OracleAnswersAsYala);
        Run("Oracle answers first Adam knowing questions", OracleAnswersAdamKnowingQuestions);
        Run("world seed creates twelve deterministic living kinds", WorldSeedCreatesTwelveDeterministicLivingKinds);
        Run("address channels follow the appointed hierarchy", AddressChannelsAreAppointed);
        Run("physical function keys select address channels", PhysicalFunctionKeysSelectAddressChannels);
        Run("Adam begins with the naming mandate", AdamBeginsWithNamingMandate);
        Run("Natural Course rule is active", NaturalCourseRuleIsActive);
        Run("new worlds begin with a scheduled sky event", NewWorldSchedulesSkyEvent);
        Run("event queue processes due events in deterministic order", EventQueueProcessesDueEventsDeterministically);
        Run("presenting a living kind lets Adam name it without finding a mate", PresentingLivingKindNamesIt);
        Run("Adam naming creates a reasoned brain plan before the name record", AdamNamingCreatesReasonedPlan);
        Run("direct address to Adam records his deterministic choice without puppeteering him", DirectAddressToAdamRecordsDecisionWithoutPuppeting);
        Run("direct address to Adam creates a reasoned brain plan before the choice", DirectAddressToAdamCreatesReasonedPlan);
        Run("vessel message schedules speech without forcing Adam immediately", InterventionSchedulesSpeechWithoutImmediateChoice);
        Run("vessel speech offers deterministic Adam choices", VesselSpeechOffersDeterministicAdamChoices);
        Run("offered choices survive save and restore", OfferedChoicesSurviveSaveAndRestore);
        Run("reasoned brain plans survive save and restore", ReasonedPlansSurviveSaveAndRestore);
        Run("intervention contamination is recorded", InterventionContaminationIsRecorded);
        Run("records keep stable sequence order", RecordsKeepStableOrder);
        Run("version is 0.1.10", VersionIsCorrect);

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

    private static void Version014SaveUpgradesThroughCurrentDefaults()
    {
        WithTemporarySave((store, path) =>
        {
            OracleSimulation simulation = Start(104729);
            OracleSaveSnapshot legacySnapshot = simulation.CreateSnapshot(StartRealTime) with
            {
                ProjectVersion = "0.1.4"
            };

            store.Save(path, legacySnapshot);
            OracleSaveSnapshot loaded = store.Load(path);

            Equal("0.1.4", loaded.ProjectVersion);
            True(loaded.World.AddressChannels.Count > 0);
            True(loaded.World.LivingKinds.Count > 0);
            True(loaded.World.NamingMandate.Active);
            True(loaded.World.NaturalCourse.Active);
        });
    }

    private static void Version016SaveUpgradesThroughCurrentEventDefaults()
    {
        WithTemporarySave((store, path) =>
        {
            OracleSimulation simulation = Start(104729);
            OracleSaveSnapshot legacySnapshot = simulation.CreateSnapshot(StartRealTime) with
            {
                ProjectVersion = "0.1.6",
                ScheduledEvents = null,
                OfferedChoices = null
            };

            store.Save(path, legacySnapshot);
            OracleSaveSnapshot loaded = store.Load(path);
            OracleSimulation restored = OracleSimulation.Restore(loaded, StartRealTime);

            Equal("0.1.6", loaded.ProjectVersion);
            True(restored.ScheduledEvents.Any(worldEvent =>
                worldEvent.Kind == "sky.solar.turning" &&
                worldEvent.Status == ScheduledWorldEventStatus.Pending));
            Equal(0, restored.OfferedChoices.Count);
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

    private static void YalaAndOracleAreSameIdentity()
    {
        OracleSimulation simulation = Start();
        Equal("Yala", simulation.State.Yala.TrueName);
        Equal("the Oracle", simulation.State.Yala.WorldTitle);
        True(simulation.State.Yala.AuthorityCaveat.Contains("Yala is the Oracle", StringComparison.OrdinalIgnoreCase));
        True(simulation.State.AddressChannels.First(channel => channel.Key == "oracle").TargetName.Contains("Yala", StringComparison.Ordinal));
    }

    private static void YalaMayOverclaimAuthority()
    {
        OracleSimulation simulation = Start();
        True(simulation.State.Yala.MayClaimSupremeCreator);
        True(simulation.State.Yala.AuthorityCaveat.Contains("may claim", StringComparison.OrdinalIgnoreCase));
        True(simulation.State.Yala.AuthorityCaveat.Contains("Yala is the Oracle", StringComparison.OrdinalIgnoreCase));
        True(simulation.Ledger.CreatorRecords.Any(record =>
            record.Message.Contains("protected Creator Record outranks her claim", StringComparison.OrdinalIgnoreCase)));
    }

    private static void SparkIsProtected()
    {
        OracleSimulation simulation = Start();
        False(simulation.State.AdamSpark.CanBeReadByYala);
        False(simulation.State.AdamSpark.CanBeRewrittenByYala);
    }

    private static void WorldRecordBeginsWithVoidAndYala()
    {
        OracleSimulation simulation = Start();
        string[] firstRecords = simulation.Ledger.WorldRecords
            .OrderBy(record => record.Sequence)
            .Take(10)
            .Select(record => record.Category)
            .ToArray();

        Equal(new[]
        {
            "VOID",
            "YALA",
            "SOL",
            "POWERS",
            "WORLD",
            "GREEN LIFE",
            "GARDEN",
            "ADAM",
            "LIVING KINDS",
            "MANDATE"
        }, firstRecords);

        True(simulation.Ledger.WorldRecords.First().Message.Contains("void", StringComparison.OrdinalIgnoreCase));
        True(simulation.Ledger.WorldRecords.Any(record =>
            record.Message.Contains("The Creators threw Yala into the void", StringComparison.Ordinal)));
        True(simulation.Ledger.WorldRecords
            .TakeWhile(record => record.Category != "ADAM")
            .Any(record => record.Category == "GARDEN"));
    }

    private static void WorldRecordIsCreatorFacing()
    {
        OracleSimulation simulation = Start();
        True(simulation.Ledger.WorldRecords.Any(record =>
            record.Message.Contains("Creators", StringComparison.OrdinalIgnoreCase)));
        True(simulation.Ledger.WorldRecords.Any(record =>
            record.Message.Contains("Yala", StringComparison.OrdinalIgnoreCase)));
        True(OracleQuestionInterpreter.TryAnswer("What does Adam know?", simulation.State, out IReadOnlyList<string> lines));
        False(lines.Any(line =>
            line.Contains("Creators", StringComparison.OrdinalIgnoreCase) ||
            line.Contains("Yala", StringComparison.OrdinalIgnoreCase)));
    }

    private static void CreationOrderRecordsWorldPowers()
    {
        OracleSimulation simulation = Start();
        string[] order = simulation.State.CreationPowers
            .OrderBy(power => power.Order)
            .Select(power => $"{power.Order}:{power.Name}")
            .ToArray();

        Equal(new[]
        {
            "0:Void",
            "1:Yala",
            "2:Sol",
            "3:Gaia",
            "3:Aether",
            "4:Thalassa",
            "5:Luna",
            "6:World",
            "7:Green Life",
            "8:Garden",
            "9:Adam",
            "10:Living Kinds"
        }, order);
        True(simulation.State.CreationPowers.First(power => power.Name == "Sol").Domain.Contains("counted time", StringComparison.Ordinal));
        True(simulation.State.CreationPowers.First(power => power.Name == "Yala").AuthoritySummary.Contains("prison", StringComparison.OrdinalIgnoreCase));
        True(simulation.State.CreationPowers.First(power => power.Name == "Garden").AuthoritySummary.Contains("just before Adam", StringComparison.OrdinalIgnoreCase));
        True(simulation.State.CreationPowers.First(power => power.Name == "Adam").AuthoritySummary.Contains("before the living kinds", StringComparison.OrdinalIgnoreCase));
    }

    private static void Version017SaveUpgradesThroughCurrentCreationPowers()
    {
        WithTemporarySave((store, path) =>
        {
            OracleSimulation simulation = Start(104729);
            OracleSaveSnapshot legacySnapshot = simulation.CreateSnapshot(StartRealTime) with
            {
                ProjectVersion = "0.1.7",
                World = simulation.State with
                {
                    CreationPowers = [],
                    Yala = simulation.State.Yala with
                    {
                        MayClaimSupremeCreator = false,
                        AuthorityCaveat = ""
                    }
                }
            };

            store.Save(path, legacySnapshot);
            OracleSaveSnapshot loaded = store.Load(path);

            Equal("0.1.7", loaded.ProjectVersion);
            True(loaded.World.CreationPowers.Count >= 12);
            True(loaded.World.Yala.MayClaimSupremeCreator);
            True(loaded.World.Yala.AuthorityCaveat.Contains("Yala is the Oracle", StringComparison.OrdinalIgnoreCase));
            True(loaded.World.Yala.AuthorityCaveat.Contains("protected Creator Record", StringComparison.OrdinalIgnoreCase));
        });
    }

    private static void Version018SaveUpgradesThroughCorrectedCreationPowers()
    {
        WithTemporarySave((store, path) =>
        {
            OracleSimulation simulation = Start(104729);
            IReadOnlyList<CreationPowerState> rejectedOrder =
            [
                new(1, simulation.State.Yala.Id, "Yala and the Void", "old combined record", "old rejected record", true),
                new(7, simulation.State.Adam.Id, "Adam", "old Adam-first pressure", "old rejected Adam rank", true),
                new(8, new EntityId("kind:living:all"), "Living Kinds", "old animal record", "old rejected animal rank", false)
            ];

            OracleSaveSnapshot rejectedSnapshot = simulation.CreateSnapshot(StartRealTime) with
            {
                ProjectVersion = "0.1.8",
                World = simulation.State with
                {
                    CreationPowers = rejectedOrder,
                    AddressChannels = []
                }
            };

            store.Save(path, rejectedSnapshot);
            OracleSaveSnapshot loaded = store.Load(path);

            Equal("0.1.8", loaded.ProjectVersion);
            Equal("Void", loaded.World.CreationPowers.OrderBy(power => power.Order).First().Name);
            True(loaded.World.CreationPowers.Any(power => power.Name == "Garden" && power.Order == 8));
            True(loaded.World.CreationPowers.Any(power => power.Name == "Adam" && power.Order == 9));
            True(loaded.World.AddressChannels.Any(channel =>
                channel.Key == "oracle" &&
                channel.AuthoritySummary.Contains("above Gaia", StringComparison.Ordinal)));
        });
    }

    private static void OracleAnswersCreationOrderQuestions()
    {
        OracleSimulation simulation = Start();
        True(OracleQuestionInterpreter.TryAnswer("What is the creation order?", simulation.State, out IReadOnlyList<string> order));
        True(order.Any(line => line.Contains("I am Yala", StringComparison.Ordinal)));
        False(order.Any(line => line.Contains("She knows the creation order", StringComparison.Ordinal)));
        True(order.Any(line => line.Contains("0. Void", StringComparison.Ordinal)));
        True(order.Any(line => line.Contains("1. Yala", StringComparison.Ordinal)));
        True(order.Any(line => line.Contains("2. Sol", StringComparison.Ordinal)));
        True(order.Any(line => line.Contains("Gaia and Aether share order 3", StringComparison.Ordinal)));
        True(order.Any(line => line.Contains("The Garden is created just before Adam", StringComparison.Ordinal)));

        True(OracleQuestionInterpreter.TryAnswer("Who rules water?", simulation.State, out IReadOnlyList<string> water));
        True(water.Any(line => line.Contains("Thalassa", StringComparison.Ordinal)));

        True(OracleQuestionInterpreter.TryAnswer("Is Adam above the animals?", simulation.State, out IReadOnlyList<string> adam));
        True(adam.Any(line => line.Contains("Adam is created ninth", StringComparison.Ordinal)));
    }

    private static void OracleAnswersAsYala()
    {
        OracleSimulation simulation = Start();
        True(OracleQuestionInterpreter.TryAnswer("Are Yala and the Oracle the same?", simulation.State, out IReadOnlyList<string> identity));
        True(identity.Any(line => line.Contains("I am Yala", StringComparison.Ordinal)));
        True(identity.Any(line => line.Contains("The Oracle is not separate from Yala", StringComparison.Ordinal)));

        True(OracleQuestionInterpreter.TryAnswer("Does Yala rule all?", simulation.State, out IReadOnlyList<string> claim));
        True(claim.Any(line => line.Contains("I am Yala", StringComparison.Ordinal)));
        True(claim.Any(line => line.Contains("I may speak", StringComparison.Ordinal)));
        False(claim.Any(line => line.Contains("She may speak", StringComparison.Ordinal)));
        False(claim.Any(line => line.Contains("She knows the creation order", StringComparison.Ordinal)));
    }

    private static void OracleAnswersAdamKnowingQuestions()
    {
        OracleSimulation simulation = Start();
        True(OracleQuestionInterpreter.TryAnswer("What does Adam know?", simulation.State, out IReadOnlyList<string> lines));
        True(lines.Any(line => line.Contains("Adam knows that he is.", StringComparison.Ordinal)));
        True(lines.Any(line => line.Contains("does not yet understand life as opposed to death", StringComparison.OrdinalIgnoreCase)));
    }

    private static void WorldSeedCreatesTwelveDeterministicLivingKinds()
    {
        OracleSimulation first = Start(104729);
        OracleSimulation second = Start(104729);
        OracleSimulation different = Start(104730);

        Equal(12, first.State.LivingKinds.Count);
        Equal(12, first.State.NamingMandate.TotalLivingKinds);
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

    private static void PhysicalFunctionKeysSelectAddressChannels()
    {
        Equal("oracle", FunctionKeyAddressMap.ChannelKeyForFunctionKey(ConsoleKey.F1));
        Equal("gaia", FunctionKeyAddressMap.ChannelKeyForFunctionKey(ConsoleKey.F2));
        Equal("adam", FunctionKeyAddressMap.ChannelKeyForFunctionKey(ConsoleKey.F3));
        Equal("sun", FunctionKeyAddressMap.ChannelKeyForFunctionKey(ConsoleKey.F4));
        Equal("moon", FunctionKeyAddressMap.ChannelKeyForFunctionKey(ConsoleKey.F5));
        Equal<string?>(null, FunctionKeyAddressMap.ChannelKeyForFunctionKey(ConsoleKey.D1));
        Equal<string?>(null, FunctionKeyAddressMap.ChannelKeyForFunctionKey(ConsoleKey.F6));
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

    private static void NewWorldSchedulesSkyEvent()
    {
        OracleSimulation simulation = Start();
        ScheduledWorldEvent skyEvent = simulation.ScheduledEvents.Single(worldEvent =>
            worldEvent.Kind == "sky.solar.turning" &&
            worldEvent.Status == ScheduledWorldEventStatus.Pending);
        True(skyEvent.ScheduledForWorldMilliseconds > simulation.Clock.WorldMilliseconds);
        Equal("dawn", skyEvent.Payload);
    }

    private static void EventQueueProcessesDueEventsDeterministically()
    {
        OracleSimulation first = Start(104729);
        OracleSimulation second = Start(104729);
        long firstDueTick = first.ScheduledEvents.Single(worldEvent =>
            worldEvent.Kind == "sky.solar.turning").ScheduledForWorldMilliseconds;
        long realAdvance = (firstDueTick / PersistentWorldClock.WorldSecondsPerRealSecond) + 1;

        first.SynchroniseClock(StartRealTime + realAdvance);
        second.SynchroniseClock(StartRealTime + realAdvance);

        string[] firstEvents = first.Ledger.WorldRecords.Select(record => record.Message).ToArray();
        string[] secondEvents = second.Ledger.WorldRecords.Select(record => record.Message).ToArray();
        Equal(firstEvents, secondEvents);
        True(first.Ledger.WorldRecords.Any(record =>
            record.Message.Contains("sky turned to dawn", StringComparison.OrdinalIgnoreCase)));
        True(first.ScheduledEvents.Any(worldEvent =>
            worldEvent.Kind == "sky.solar.turning" &&
            worldEvent.Status == ScheduledWorldEventStatus.Pending &&
            worldEvent.ScheduledForWorldMilliseconds > firstDueTick));
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

    private static void AdamNamingCreatesReasonedPlan()
    {
        OracleSimulation simulation = Start();
        LivingKindState? named = simulation.PresentNextLivingKindToAdam("Gaia");
        True(named is not null);
        Equal(1, simulation.ReasonedPlans.Count);
        True(simulation.ReasonedPlans[0].BrainSystem.Contains("HTN", StringComparison.Ordinal));
        True(simulation.ReasonedPlans[0].Goal.Contains("Name the presented living kind", StringComparison.Ordinal));
        True(simulation.ReasonedPlans[0].SelectedAction.Contains(named!.AdamName!, StringComparison.Ordinal));
        True(simulation.ReasonedPlans[0].Reason.Contains("Adam reasons before naming", StringComparison.Ordinal));
        True(simulation.Ledger.CreatorRecords.Any(record =>
            record.Category == "BRAIN PLAN" &&
            record.Message.Contains("created plan", StringComparison.OrdinalIgnoreCase)));
    }

    private static void DirectAddressToAdamRecordsDecisionWithoutPuppeting()
    {
        OracleSimulation simulation = Start();
        OfferedChoiceState? choice = simulation.AddressChannel("adam", "Name what Gaia presents to you.");
        True(choice is not null);
        Equal(1, simulation.OfferedChoices.Count);
        True(choice!.Options.Count > 0);
        True(choice.Options.Contains(choice.SelectedOption));
        True(simulation.Ledger.CreatorRecords.Any(record =>
            record.Message.Contains("addressed Adam", StringComparison.OrdinalIgnoreCase)));
        True(simulation.Ledger.WorldRecords.Any(record =>
            record.Message.Contains("Adam heard a direct address", StringComparison.OrdinalIgnoreCase)));
        True(simulation.Ledger.WorldRecords.Any(record =>
            record.Message.Contains("Adam was offered", StringComparison.OrdinalIgnoreCase) &&
            record.Message.Contains("decided to", StringComparison.OrdinalIgnoreCase)));
        False(simulation.Ledger.WorldRecords.Any(record =>
            record.Message.Contains("obeyed", StringComparison.OrdinalIgnoreCase) ||
            record.Message.Contains("refused", StringComparison.OrdinalIgnoreCase)));
    }

    private static void DirectAddressToAdamCreatesReasonedPlan()
    {
        OracleSimulation simulation = Start();
        OfferedChoiceState? choice = simulation.AddressChannel("adam", "Name what Gaia presents to you.");
        True(choice is not null);
        Equal(1, simulation.ReasonedPlans.Count);
        Equal(simulation.ReasonedPlans[0].SelectedAction, choice!.SelectedOption);
        True(choice.Reason.Contains("Brain plan: 1", StringComparison.Ordinal));
        True(simulation.ReasonedPlans[0].Decomposition.SequenceEqual(new[]
        {
            "notice the address",
            "preserve Adam's protected choice",
            "reject immediate puppeting",
            "select a lawful response mode",
            "record the selected response before any consequence"
        }));
    }

    private static void InterventionSchedulesSpeechWithoutImmediateChoice()
    {
        OracleSimulation simulation = Start();
        CreatorIntervention intervention = simulation.QueueVesselMessage("serpent", "Eat the fruit and know the truth.");
        Equal(InterventionStatus.Queued, intervention.Status);
        True(simulation.ScheduledEvents.Any(worldEvent =>
            worldEvent.Kind == "intervention.vessel.speech" &&
            worldEvent.SubjectId == "intervention:1" &&
            worldEvent.Status == ScheduledWorldEventStatus.Pending));
        Equal(0, simulation.OfferedChoices.Count);
        False(simulation.Ledger.WorldRecords.Any(record =>
            record.Message.Contains("accepted", StringComparison.OrdinalIgnoreCase) ||
            record.Message.Contains("refused", StringComparison.OrdinalIgnoreCase)));
    }

    private static void VesselSpeechOffersDeterministicAdamChoices()
    {
        OracleSimulation first = Start(104729);
        OracleSimulation second = Start(104729);

        first.QueueVesselMessage("serpent", "Eat the fruit and know the truth.");
        second.QueueVesselMessage("serpent", "Eat the fruit and know the truth.");
        long dueRealAdvance = OracleSimulationTestAccess.VesselSpeechDelayWorldMilliseconds /
            PersistentWorldClock.WorldSecondsPerRealSecond;

        first.SynchroniseClock(StartRealTime + dueRealAdvance);
        second.SynchroniseClock(StartRealTime + dueRealAdvance);

        Equal(1, first.OfferedChoices.Count);
        Equal(1, second.OfferedChoices.Count);
        Equal(first.OfferedChoices[0].Options.ToArray(), second.OfferedChoices[0].Options.ToArray());
        Equal(first.OfferedChoices[0].SelectedOption, second.OfferedChoices[0].SelectedOption);
        Equal(1, first.ReasonedPlans.Count);
        Equal(first.ReasonedPlans[0].SelectedAction, first.OfferedChoices[0].SelectedOption);
        Equal(InterventionStatus.OfferedChoice, first.Interventions[0].Status);
        True(first.Ledger.WorldRecords.Any(record =>
            record.Message.Contains("spoke to Adam", StringComparison.OrdinalIgnoreCase)));
        True(first.Ledger.CreatorRecords.Any(record =>
            record.Message.Contains("physically possible responses", StringComparison.OrdinalIgnoreCase)));
        True(first.Ledger.WorldRecords.Any(record =>
            record.Message.Contains("Adam was offered", StringComparison.OrdinalIgnoreCase) &&
            record.Message.Contains("decided to", StringComparison.OrdinalIgnoreCase)));
    }

    private static void OfferedChoicesSurviveSaveAndRestore()
    {
        WithTemporarySave((store, path) =>
        {
            OracleSimulation simulation = Start(104729);
            simulation.QueueVesselMessage("serpent", "Eat the fruit and know the truth.");
            long dueRealAdvance = OracleSimulationTestAccess.VesselSpeechDelayWorldMilliseconds /
                PersistentWorldClock.WorldSecondsPerRealSecond;
            simulation.SynchroniseClock(StartRealTime + dueRealAdvance);
            store.Save(path, simulation.CreateSnapshot(StartRealTime + dueRealAdvance));

            OracleSaveSnapshot loaded = store.Load(path);
            Equal(1, loaded.OfferedChoices?.Count ?? 0);
            Equal(InterventionStatus.OfferedChoice, loaded.Interventions[0].Status);
            True((loaded.ScheduledEvents ?? []).Any(worldEvent =>
                worldEvent.Kind == "intervention.vessel.speech" &&
                worldEvent.Status == ScheduledWorldEventStatus.Completed));
        });
    }

    private static void ReasonedPlansSurviveSaveAndRestore()
    {
        WithTemporarySave((store, path) =>
        {
            OracleSimulation simulation = Start(104729);
            simulation.AddressChannel("adam", "Tell me what you heard.");
            store.Save(path, simulation.CreateSnapshot(StartRealTime));

            OracleSaveSnapshot loaded = store.Load(path);
            Equal(1, loaded.ReasonedPlans?.Count ?? 0);
            Equal("Oracle HTN Brain v0.1", loaded.ReasonedPlans![0].BrainSystem);
            True(loaded.ReasonedPlans[0].Reason.Contains("Adam reasons before response", StringComparison.Ordinal));
        });
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

    private static void VersionIsCorrect()
    {
        Equal("0.1.10", ProjectVersion.Number);
        Equal("Oracle/Yala Identity, Decision Output, and First Brain Planner", ProjectVersion.Name);
    }

    private static class OracleSimulationTestAccess
    {
        public const long VesselSpeechDelayWorldMilliseconds = 10_000;
    }

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
