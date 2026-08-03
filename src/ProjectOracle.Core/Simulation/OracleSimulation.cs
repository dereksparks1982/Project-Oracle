using ProjectOracle.Audit;
using ProjectOracle.Domain;
using ProjectOracle.Events;
using ProjectOracle.Interventions;
using ProjectOracle.Persistence;

namespace ProjectOracle.Simulation;

public sealed class OracleSimulation
{
    private const int InterventionSpeechPriority = 10;
    private const int SkyTurningPriority = 100;
    private const long VesselSpeechDelayWorldMilliseconds = 10_000;

    private readonly List<CreatorIntervention> _interventions = [];
    private readonly List<ScheduledWorldEvent> _scheduledEvents = [];
    private readonly List<OfferedChoiceState> _offeredChoices = [];
    private long _nextInterventionId = 1;
    private long _nextEventId = 1;
    private long _nextChoiceId = 1;

    private OracleSimulation(ulong seed, long realUnixMilliseconds)
    {
        Clock = new PersistentWorldClock(0, realUnixMilliseconds);
        Random = new DeterministicRandom(seed);
        Ledger = new AuditLedger();
        State = CreateInitialState(seed);
        RecordGenesis();
        EnsureSolarTurningScheduled();
    }

    private OracleSimulation(OracleSaveSnapshot snapshot)
    {
        Clock = new PersistentWorldClock(
            snapshot.WorldMilliseconds,
            snapshot.LastRealUnixMilliseconds,
            snapshot.CatchUpRuns,
            snapshot.LastOfflineElapsedRealMilliseconds);
        Random = new DeterministicRandom(snapshot.Seed);
        Ledger = new AuditLedger(snapshot.Records);
        State = snapshot.World with { WorldMilliseconds = snapshot.WorldMilliseconds };
        _interventions.AddRange(snapshot.Interventions.OrderBy(intervention => intervention.Id));
        _scheduledEvents.AddRange((snapshot.ScheduledEvents ?? []).OrderBy(worldEvent => worldEvent.Id));
        _offeredChoices.AddRange((snapshot.OfferedChoices ?? []).OrderBy(choice => choice.Id));
        _nextInterventionId = _interventions.Count == 0 ? 1 : checked(_interventions[^1].Id + 1);
        _nextEventId = _scheduledEvents.Count == 0 ? 1 : checked(_scheduledEvents[^1].Id + 1);
        _nextChoiceId = _offeredChoices.Count == 0 ? 1 : checked(_offeredChoices[^1].Id + 1);
    }

    public PersistentWorldClock Clock { get; }

    public DeterministicRandom Random { get; }

    public AuditLedger Ledger { get; }

    public WorldState State { get; private set; }

    public IReadOnlyList<CreatorIntervention> Interventions => _interventions.AsReadOnly();

    public IReadOnlyList<ScheduledWorldEvent> ScheduledEvents => _scheduledEvents.AsReadOnly();

    public IReadOnlyList<OfferedChoiceState> OfferedChoices => _offeredChoices.AsReadOnly();

    public static OracleSimulation Start(ulong seed, long realUnixMilliseconds) => new(seed, realUnixMilliseconds);

    public static OracleSimulation Restore(OracleSaveSnapshot snapshot, long currentRealUnixMilliseconds)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        OracleSimulation simulation = new(snapshot);
        simulation.SynchroniseClock(currentRealUnixMilliseconds, offlineCatchUp: true);
        simulation.EnsureSolarTurningScheduled();
        return simulation;
    }

    public ClockAdvance SynchroniseClock(long currentRealUnixMilliseconds, bool offlineCatchUp = false)
    {
        ClockAdvance advance = Clock.Synchronise(currentRealUnixMilliseconds, offlineCatchUp);
        State = State with { WorldMilliseconds = Clock.WorldMilliseconds };

        if (advance.ElapsedRealMilliseconds > 0)
        {
            string mode = offlineCatchUp ? "offline catch-up" : "live real-time advance";
            Ledger.RecordCreator(
                Clock.WorldMilliseconds,
                "TIME",
                $"Applied {mode}: {advance.ElapsedRealMilliseconds} real millisecond(s) became {advance.ElapsedWorldMilliseconds} world millisecond(s).");
        }

        if (advance.BackwardClockDetected)
        {
            Ledger.RecordCreator(
                Clock.WorldMilliseconds,
                "CLOCK WARNING",
                "The system clock moved backwards. Project Oracle refused to rewind the world.");
        }

        ProcessDueEvents();
        EnsureSolarTurningScheduled();
        return advance;
    }

    public int ProcessDueEvents()
    {
        int processed = 0;
        const int maximumEventsPerPass = 64;

        while (processed < maximumEventsPerPass)
        {
            ScheduledWorldEvent? dueEvent = _scheduledEvents
                .Where(candidate =>
                    candidate.Status == ScheduledWorldEventStatus.Pending &&
                    candidate.ScheduledForWorldMilliseconds <= Clock.WorldMilliseconds)
                .OrderBy(candidate => candidate.ScheduledForWorldMilliseconds)
                .ThenBy(candidate => candidate.Priority)
                .ThenBy(candidate => candidate.Id)
                .FirstOrDefault();

            if (dueEvent is null)
            {
                break;
            }

            CompleteEvent(dueEvent);
            processed++;
        }

        if (processed == maximumEventsPerPass && _scheduledEvents.Any(candidate =>
            candidate.Status == ScheduledWorldEventStatus.Pending &&
            candidate.ScheduledForWorldMilliseconds <= Clock.WorldMilliseconds))
        {
            Ledger.RecordCreator(
                Clock.WorldMilliseconds,
                "EVENT QUEUE",
                "The event scheduler reached its bounded processing limit. Remaining due events will continue on the next pass.");
        }

        return processed;
    }

    public CreatorIntervention QueueVesselMessage(string vessel, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vessel);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        CreatorIntervention intervention = new(
            _nextInterventionId++,
            Clock.WorldMilliseconds,
            vessel.Trim(),
            message.Trim(),
            ContaminatesExperiment: true,
            InterventionStatus.Queued);

        _interventions.Add(intervention);
        ScheduleEvent(
            checked(Clock.WorldMilliseconds + VesselSpeechDelayWorldMilliseconds),
            InterventionSpeechPriority,
            "intervention.vessel.speech",
            $"intervention:{intervention.Id}",
            intervention.Message);
        Ledger.RecordCreator(
            Clock.WorldMilliseconds,
            "INTERVENTION",
            $"Creator intervention {intervention.Id} queued through {intervention.Vessel}. A deterministic speech event was scheduled. The experiment is contaminated from this point.");
        Ledger.RecordWorld(
            Clock.WorldMilliseconds,
            "SIGN",
            $"A {intervention.Vessel} approached Adam. It has not spoken yet.");

        return intervention;
    }

    public void AddressChannel(string channelKey, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channelKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        AddressChannelState channel = State.AddressChannels.FirstOrDefault(candidate =>
            candidate.Key.Equals(channelKey.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Address channel is not recognised: {channelKey}");

        Ledger.RecordCreator(
            Clock.WorldMilliseconds,
            "DIRECT ADDRESS",
            $"The Creators addressed {channel.TargetName} at {channel.Prompt}: \"{message.Trim()}\". The address contaminates the experiment.");

        if (channel.Key.Equals("adam", StringComparison.OrdinalIgnoreCase))
        {
            Ledger.RecordWorld(
                Clock.WorldMilliseconds,
                "VOICE",
                "Adam heard a direct address from beyond his ordinary world. His response has not been decided.");
        }
    }

    public LivingKindState? PresentNextLivingKindToAdam(string presenter)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(presenter);

        int index = State.LivingKinds.ToList().FindIndex(kind => !kind.NamedByAdam);
        if (index < 0)
        {
            return null;
        }

        LivingKindState current = State.LivingKinds[index];
        LivingKindState named = current with
        {
            PresentedToAdam = true,
            NamedByAdam = true,
            AdamName = CreateAdamName(current, index)
        };

        List<LivingKindState> kinds = State.LivingKinds.ToList();
        kinds[index] = named;
        State = State with
        {
            LivingKinds = kinds,
            NamingMandate = State.NamingMandate with
            {
                PresentedCount = kinds.Count(kind => kind.PresentedToAdam),
                NamedCount = kinds.Count(kind => kind.NamedByAdam),
                SuitableMateFound = kinds.Any(kind => kind.SuitableMate)
            }
        };

        Ledger.RecordWorld(
            Clock.WorldMilliseconds,
            "NAMING",
            $"{presenter.Trim()} presented a living kind to Adam. Adam named it {named.AdamName} and found no suitable mate.");
        Ledger.RecordCreator(
            Clock.WorldMilliseconds,
            "NAMING",
            $"Adam named {named.Id} ({named.AncientKind}) as {named.AdamName}. Suitable mate: {named.SuitableMate}.");

        return named;
    }

    public OracleSaveSnapshot CreateSnapshot(long savedAtUnixMilliseconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(savedAtUnixMilliseconds);
        return new OracleSaveSnapshot(
            OracleSaveStore.SaveFormat,
            OracleSaveStore.CurrentSchemaVersion,
            ProjectVersion.Number,
            savedAtUnixMilliseconds,
            State.Seed,
            Clock.WorldMilliseconds,
            Clock.LastRealUnixMilliseconds,
            Clock.CatchUpRuns,
            Clock.LastOfflineElapsedRealMilliseconds,
            WorldDefaults.Normalise(State),
            Ledger.WorldRecords.Concat(Ledger.CreatorRecords).OrderBy(record => record.Sequence).ToArray(),
            _interventions.ToArray(),
            _scheduledEvents.ToArray(),
            _offeredChoices.ToArray());
    }

    private static WorldState CreateInitialState(ulong seed) => WorldDefaults.CreateInitialState(seed);

    private void RecordGenesis()
    {
        Ledger.RecordWorld(0, "GENESIS", "The Garden was formed and filled with ancient living kinds.");
        Ledger.RecordWorld(0, "GENESIS", "Adam awoke in the Garden. The Oracle watched in silence.");
        Ledger.RecordWorld(0, "MANDATE", "Adam was given the task of naming the living kinds and finding whether any was a suitable mate.");
        Ledger.RecordWorld(0, "BOUNDARY", "The Garden boundary was closed.");

        Ledger.RecordCreator(0, "GENESIS", $"World Seed: {State.Seed}.");
        Ledger.RecordCreator(0, "AUTHORITY", "The Creators made Yala. Yala formed the Garden, Gaia, the celestial governors, the living kinds, Adam's body, and Adam's ordinary mind.");
        Ledger.RecordCreator(0, "AUTHORITY", "Direct address channels are appointed: F1 Oracle, F2 Gaia, F3 Adam, F4 Sun, F5 Moon.");
        Ledger.RecordCreator(0, "NATURAL COURSE", State.NaturalCourse.RuleText);
        Ledger.RecordCreator(0, "SPARK", State.AdamSpark.CreatorDescription);
        Ledger.RecordCreator(0, "MANDATE", "Yala knows the Creators will one day give her a new language to learn and teach. The language has not been supplied.");
    }

    private ScheduledWorldEvent ScheduleEvent(
        long scheduledForWorldMilliseconds,
        int priority,
        string kind,
        string subjectId,
        string payload)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(scheduledForWorldMilliseconds);
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectId);

        ScheduledWorldEvent worldEvent = new(
            _nextEventId++,
            scheduledForWorldMilliseconds,
            priority,
            Clock.WorldMilliseconds,
            kind.Trim(),
            subjectId.Trim(),
            payload.Trim(),
            ScheduledWorldEventStatus.Pending);
        _scheduledEvents.Add(worldEvent);
        return worldEvent;
    }

    private void EnsureSolarTurningScheduled()
    {
        if (_scheduledEvents.Any(worldEvent =>
            worldEvent.Status == ScheduledWorldEventStatus.Pending &&
            worldEvent.Kind.Equals("sky.solar.turning", StringComparison.Ordinal)))
        {
            return;
        }

        long scheduledAt = NextSolarTurningAfter(Clock.WorldMilliseconds);
        CalendarSnapshot calendar = OracleCalendar.FromElapsedWorldMilliseconds(scheduledAt);
        ScheduleEvent(
            scheduledAt,
            SkyTurningPriority,
            "sky.solar.turning",
            "sky",
            calendar.SolarPhase);
    }

    private void CompleteEvent(ScheduledWorldEvent worldEvent)
    {
        int index = _scheduledEvents.FindIndex(candidate => candidate.Id == worldEvent.Id);
        if (index < 0)
        {
            return;
        }

        _scheduledEvents[index] = worldEvent with
        {
            Status = ScheduledWorldEventStatus.Completed,
            CompletedAtWorldMilliseconds = Clock.WorldMilliseconds
        };

        if (worldEvent.Kind.Equals("sky.solar.turning", StringComparison.Ordinal))
        {
            CompleteSolarTurning(worldEvent);
            return;
        }

        if (worldEvent.Kind.Equals("intervention.vessel.speech", StringComparison.Ordinal))
        {
            CompleteVesselSpeech(worldEvent);
        }
    }

    private void CompleteSolarTurning(ScheduledWorldEvent worldEvent)
    {
        CalendarSnapshot calendar = OracleCalendar.FromElapsedWorldMilliseconds(worldEvent.ScheduledForWorldMilliseconds);
        Ledger.RecordWorld(
            worldEvent.ScheduledForWorldMilliseconds,
            "SKY",
            $"The Garden sky turned to {calendar.SolarPhase}.");
        Ledger.RecordCreator(
            Clock.WorldMilliseconds,
            "EVENT QUEUE",
            $"Event {worldEvent.Id} completed: the sky entered {calendar.SolarPhase}.");
        long nextTurning = NextSolarTurningAfter(worldEvent.ScheduledForWorldMilliseconds);
        ScheduleEvent(
            nextTurning,
            SkyTurningPriority,
            "sky.solar.turning",
            "sky",
            OracleCalendar.FromElapsedWorldMilliseconds(nextTurning).SolarPhase);
    }

    private void CompleteVesselSpeech(ScheduledWorldEvent worldEvent)
    {
        const string prefix = "intervention:";
        if (!worldEvent.SubjectId.StartsWith(prefix, StringComparison.Ordinal))
        {
            return;
        }

        if (!long.TryParse(worldEvent.SubjectId[prefix.Length..], out long interventionId))
        {
            return;
        }

        int interventionIndex = _interventions.FindIndex(intervention => intervention.Id == interventionId);
        if (interventionIndex < 0)
        {
            return;
        }

        CreatorIntervention intervention = _interventions[interventionIndex];
        _interventions[interventionIndex] = intervention with { Status = InterventionStatus.OfferedChoice };

        Ledger.RecordWorld(
            worldEvent.ScheduledForWorldMilliseconds,
            "VESSEL",
            $"The {intervention.Vessel} spoke to Adam: \"{intervention.Message}\".");

        OfferedChoiceState choice = OfferAdamResponseChoice(worldEvent, intervention);
        Ledger.RecordWorld(
            worldEvent.ScheduledForWorldMilliseconds,
            "CHOICE",
            $"Adam selected the {choice.SelectedOption} response option. No consequence beyond the recorded choice exists yet.");
        Ledger.RecordCreator(
            Clock.WorldMilliseconds,
            "OFFERED CHOICE",
            $"Adam was offered {choice.Options.Count} physically possible responses to intervention {intervention.Id}. Selected: {choice.SelectedOption}. Reason: {choice.Reason}");
    }

    private OfferedChoiceState OfferAdamResponseChoice(
        ScheduledWorldEvent worldEvent,
        CreatorIntervention intervention)
    {
        string[] options = ["accept", "refuse", "delay", "question", "report", "ignore"];
        ulong derivedSeed = State.Seed ^ ((ulong)intervention.Id * 0x9E37_79B9_7F4A_7C15UL);
        DeterministicRandom choiceRandom = new(derivedSeed);
        string selected = options[(int)(choiceRandom.NextUInt64() % (ulong)options.Length)];
        string reason = "Deterministic scaffold choice from world seed, intervention id, and Adam's current confined Garden state; memory and belief are not implemented yet.";

        OfferedChoiceState choice = new(
            _nextChoiceId++,
            worldEvent.Id,
            worldEvent.ScheduledForWorldMilliseconds,
            State.Adam.Id.Value,
            $"A {intervention.Vessel} delivered a Creator-supplied message.",
            options,
            selected,
            reason);
        _offeredChoices.Add(choice);
        return choice;
    }

    private static long NextSolarTurningAfter(long elapsedWorldMilliseconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(elapsedWorldMilliseconds);

        long absoluteWorldMilliseconds = checked(OracleCalendar.EpochTimeOfDayMilliseconds + elapsedWorldMilliseconds);
        long currentDayStart = absoluteWorldMilliseconds -
            (absoluteWorldMilliseconds % PersistentWorldClock.WorldMillisecondsPerDay);
        long[] boundaries =
        [
            5 * 3_600_000L,
            7 * 3_600_000L,
            17 * 3_600_000L,
            19 * 3_600_000L
        ];

        foreach (long boundary in boundaries)
        {
            long candidate = checked(currentDayStart + boundary);
            if (candidate > absoluteWorldMilliseconds)
            {
                return checked(candidate - OracleCalendar.EpochTimeOfDayMilliseconds);
            }
        }

        return checked(currentDayStart + PersistentWorldClock.WorldMillisecondsPerDay + boundaries[0] - OracleCalendar.EpochTimeOfDayMilliseconds);
    }

    private static string CreateAdamName(LivingKindState kind, int index)
    {
        string[] firstWords = ["ground", "wing", "water", "root", "hand", "horn", "night", "reed", "stone"];
        string[] secondWords = ["walker", "caller", "glider", "crawler", "climber", "grazer", "hunter", "singer", "sleeper"];
        int kindOffset = kind.Id.Value.Sum(character => (int)character);
        return $"{firstWords[index % firstWords.Length]}-{secondWords[kindOffset % secondWords.Length]}";
    }
}
