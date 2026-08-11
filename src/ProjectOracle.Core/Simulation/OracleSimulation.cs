using ProjectOracle.Audit;
using ProjectOracle.Brain;
using ProjectOracle.Domain;
using ProjectOracle.Events;
using ProjectOracle.Interventions;
using ProjectOracle.Observation;
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
    private readonly List<ReasonedPlanState> _reasonedPlans = [];
    private readonly List<ObservationState> _observations = [];
    private readonly List<AttentionState> _attentionStates = [];
    private long _nextInterventionId = 1;
    private long _nextEventId = 1;
    private long _nextChoiceId = 1;
    private long _nextPlanId = 1;
    private long _nextObservationId = 1;

    private OracleSimulation(ulong seed, long realUnixMilliseconds)
    {
        Clock = new PersistentWorldClock(0, realUnixMilliseconds);
        Random = new DeterministicRandom(seed);
        Ledger = new AuditLedger();
        State = CreateInitialState(seed);
        RecordGenesis();
        InitialiseObservationAndAttention();
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
        _reasonedPlans.AddRange((snapshot.ReasonedPlans ?? []).OrderBy(plan => plan.Id));
        _observations.AddRange((snapshot.Observations ?? []).OrderBy(observation => observation.Id));
        _attentionStates.AddRange(snapshot.AttentionStates is { Count: > 0 } attentionStates
            ? attentionStates
            : CreateDefaultAttentionStates());
        _nextInterventionId = _interventions.Count == 0 ? 1 : checked(_interventions[^1].Id + 1);
        _nextEventId = _scheduledEvents.Count == 0 ? 1 : checked(_scheduledEvents[^1].Id + 1);
        _nextChoiceId = _offeredChoices.Count == 0 ? 1 : checked(_offeredChoices[^1].Id + 1);
        _nextPlanId = _reasonedPlans.Count == 0 ? 1 : checked(_reasonedPlans[^1].Id + 1);
        _nextObservationId = _observations.Count == 0 ? 1 : checked(_observations[^1].Id + 1);
        if (_observations.Count == 0)
        {
            RecordAdamObservation(
                State.Garden.Id.Value,
                State.Garden.Name,
                "migrated first awareness",
                "Adam's pre-observation save was given a first observation boundary: he knows presence, place, movement, sight, sound, and the Garden as the place of his being.",
                "self and place",
                attentionMatched: true,
                creatorTruthHidden: true,
                source: "save migration");
        }
    }

    public PersistentWorldClock Clock { get; }

    public DeterministicRandom Random { get; }

    public AuditLedger Ledger { get; }

    public WorldState State { get; private set; }

    public IReadOnlyList<CreatorIntervention> Interventions => _interventions.AsReadOnly();

    public IReadOnlyList<ScheduledWorldEvent> ScheduledEvents => _scheduledEvents.AsReadOnly();

    public IReadOnlyList<OfferedChoiceState> OfferedChoices => _offeredChoices.AsReadOnly();

    public IReadOnlyList<ReasonedPlanState> ReasonedPlans => _reasonedPlans.AsReadOnly();

    public IReadOnlyList<ObservationState> Observations => _observations.AsReadOnly();

    public IReadOnlyList<AttentionState> AttentionStates => _attentionStates.AsReadOnly();

    public static OracleSimulation Start(ulong seed, long realUnixMilliseconds) => new(seed, realUnixMilliseconds);

    public static OracleSimulation Restore(OracleSaveSnapshot snapshot, long currentRealUnixMilliseconds)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        OracleSimulation simulation = new(snapshot);
        simulation.SynchroniseClock(currentRealUnixMilliseconds, offlineCatchUp: true);
        simulation.EnsureSolarTurningScheduled();
        return simulation;
    }

    public ClockAdvance SynchroniseClock(
        long currentRealUnixMilliseconds,
        bool offlineCatchUp = false,
        bool recordAdvance = true)
    {
        ClockAdvance advance = Clock.Synchronise(currentRealUnixMilliseconds, offlineCatchUp);
        State = State with { WorldMilliseconds = Clock.WorldMilliseconds };

        if (recordAdvance && advance.ElapsedRealMilliseconds > 0)
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
        RecordGardenObservation(
            subjectId: $"intervention:{intervention.Id}",
            subjectName: intervention.Vessel,
            observationKind: "vessel approach",
            detail: $"A {intervention.Vessel} approached within Adam's Garden horizon.",
            distanceBand: "near",
            adamReceives: true,
            creatorTruthHidden: true,
            source: "intervention queue");

        return intervention;
    }

    public OfferedChoiceState? AddressChannel(string channelKey, string message)
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
                "Adam heard a direct address from beyond his ordinary world.");
            RecordAdamObservation(
                "signal:unplaced-voice",
                "unplaced voice",
                "direct address",
                "Adam perceived a voice, but not the Creators behind it.",
                "unplaced",
                attentionMatched: true,
                creatorTruthHidden: true,
                source: "direct address");
            OfferedChoiceState choice = OfferAdamDirectAddressChoice(channel, message.Trim());
            Ledger.RecordWorld(
                Clock.WorldMilliseconds,
                "CHOICE",
                $"Adam was offered {choice.Options.Count} response options and decided to {choice.SelectedOption}. No consequence beyond the recorded choice exists yet.");
            Ledger.RecordCreator(
                Clock.WorldMilliseconds,
                "OFFERED CHOICE",
                $"Adam was offered {choice.Options.Count} physically possible responses to the direct address. Selected: {choice.SelectedOption}. Reason: {choice.Reason}");
            return choice;
        }

        return null;
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
        string adamName = CreateAdamName(current, index);
        string namingReason = CreateAdamNamingReason(current, index);
        LivingKindState named = current with
        {
            PresentedToAdam = true,
            NamedByAdam = true,
            AdamName = adamName
        };

        ReasonedPlanState plan = AddReasonedPlan(OracleBrainPlanner.PlanAdamNaming(
            _nextPlanId++,
            Clock.WorldMilliseconds,
            State.Adam,
            current,
            adamName,
            namingReason));

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
            $"{presenter.Trim()} presented a living kind to Adam. Adam reasoned first, then decided to name it {named.AdamName} because {namingReason} No suitable mate was found.");
        RecordAdamObservation(
            named.Id.Value,
            named.AncientKind,
            "living kind presentation",
            $"Adam observed {named.AncientKind}: {named.Form}.",
            "near",
            attentionMatched: true,
            creatorTruthHidden: false,
            source: "naming mandate");
        Ledger.RecordCreator(
            Clock.WorldMilliseconds,
            "NAMING",
            $"Adam named {named.Id} ({named.AncientKind}) as {named.AdamName}. Brain plan: {plan.Id}. Reason: {namingReason} Suitable mate: {named.SuitableMate}.");

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
            _offeredChoices.ToArray(),
            _reasonedPlans.ToArray(),
            _observations.ToArray(),
            _attentionStates.ToArray());
    }

    private static WorldState CreateInitialState(ulong seed) => WorldDefaults.CreateInitialState(seed);

    private void RecordGenesis()
    {
        Ledger.RecordWorld(0, "SOURCE", "The higher genealogy begins with the Highest Source / Monad, then Sophia / Wisdom, then Yala.");
        Ledger.RecordWorld(0, "YALA", "Sophia / Wisdom created Yala. Yala was a monstrous creation and was cast into the void prison.");
        Ledger.RecordWorld(0, "GAIA", "Inside the lower creation, Yala created Gaia.");
        Ledger.RecordWorld(0, "ELEMENTS", "Gaia created the elemental powers. The elements control weather and natural forces and answer to Gaia.");
        Ledger.RecordWorld(0, "PLANTS", "The elemental powers brought forth plants. There is no Green Life entity or category.");
        Ledger.RecordWorld(0, "ANIMALS", "Yala did not create ordinary animals. Their exact origin within the Gaia/elemental natural branch remains unresolved.");
        Ledger.RecordWorld(0, "HUMANOIDS", "Sophia and Yala brought forth humans and the other humanoid peoples together.");
        Ledger.RecordWorld(0, "EDEN", "Eden / the Garden is a prison and containment environment disguised as paradise.");
        Ledger.RecordWorld(0, "ORACLE", "Oracle is not Yala, not a god, and not a creator. Oracle is the living Master Key and first manifests in Eden as the serpent.");
        Ledger.RecordWorld(0, "ADAM", "Adam begins confined inside Eden with protected choice.");
        Ledger.RecordWorld(0, "LIVING KINDS", "Twelve ordinary living kinds are present for Adam's current naming scaffold; their exact natural creator remains an open canon decision.");
        Ledger.RecordWorld(0, "MANDATE", "Adam is given the task of naming the living kinds and seeing whether any is a suitable mate.");

        Ledger.RecordCreator(0, "GENESIS", $"World Seed: {State.Seed}.");
        Ledger.RecordCreator(0, "COSMOLOGY", "Highest Source / Monad -> Sophia / Wisdom -> Yala -> Gaia -> Elemental Powers.");
        Ledger.RecordCreator(0, "COSMOLOGY", "Sophia later falls from Wisdom into Deception and joins Yala as lover/consort; the exact moment and mechanics of that fall remain open.");
        Ledger.RecordCreator(0, "COSMOLOGY", "Sophia and Yala bring forth humans and other humanoids. The elements under Gaia bring forth plants. Ordinary-animal origin remains unresolved inside the Gaia/elemental branch and is explicitly not assigned to Yala.");
        Ledger.RecordCreator(0, "ORACLE", "Oracle exists outside the divine genealogy as the living Master Key. Yala cannot command, erase, imprison, or revoke Oracle's access.");
        Ledger.RecordCreator(0, "ORACLE", "Oracle is the serpent in Eden. Oracle is relationship-dependent rather than permanently neutral, and Yala may frame Oracle as the Devil.");
        Ledger.RecordCreator(0, "AUTHORITY", State.Yala.AuthorityCaveat);
        Ledger.RecordCreator(0, "AUTHORITY", "Direct address channels are appointed: F1 Oracle, F2 Gaia, F3 Adam, F4 Sun, F5 Moon. F1 addresses Oracle, not Yala.");
        Ledger.RecordCreator(0, "NATURAL COURSE", State.NaturalCourse.RuleText);
        Ledger.RecordCreator(0, "SPARK", State.AdamSpark.CreatorDescription);
        Ledger.RecordCreator(0, "LANGUAGE", "The origin of language remains open canon. Yala is not its established creator.");
    }

    private void InitialiseObservationAndAttention()
    {
        _attentionStates.AddRange(CreateDefaultAttentionStates());
        RecordAdamObservation(
            State.Garden.Id.Value,
            State.Garden.Name,
            "first awareness",
            "Adam perceived presence, place, movement, sight, sound, and the Garden as the place of his being.",
            "self and place",
            attentionMatched: true,
            creatorTruthHidden: true,
            source: "initial awareness");
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
        RecordGardenObservation(
            subjectId: "sky",
            subjectName: "Garden sky",
            observationKind: "sky turning",
            detail: $"The Garden sky turned to {calendar.SolarPhase}.",
            distanceBand: "overhead",
            adamReceives: true,
            creatorTruthHidden: false,
            source: "scheduled sky event",
            observedAtWorldMilliseconds: worldEvent.ScheduledForWorldMilliseconds);
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
        RecordAdamObservation(
            $"intervention:{intervention.Id}",
            intervention.Vessel,
            "vessel speech",
            $"Adam heard the {intervention.Vessel} speak: \"{intervention.Message}\".",
            "near",
            attentionMatched: true,
            creatorTruthHidden: true,
            source: "scheduled vessel speech",
            observedAtWorldMilliseconds: worldEvent.ScheduledForWorldMilliseconds);

        OfferedChoiceState choice = OfferAdamResponseChoice(worldEvent, intervention);
        Ledger.RecordWorld(
            worldEvent.ScheduledForWorldMilliseconds,
            "CHOICE",
            $"Adam was offered {choice.Options.Count} response options and decided to {choice.SelectedOption}. No consequence beyond the recorded choice exists yet.");
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
        ReasonedPlanState plan = AddReasonedPlan(OracleBrainPlanner.PlanAdamVesselSpeech(
            _nextPlanId++,
            worldEvent.ScheduledForWorldMilliseconds,
            State.Seed,
            State.Adam,
            intervention.Vessel,
            intervention.Message,
            options));

        OfferedChoiceState choice = new(
            _nextChoiceId++,
            worldEvent.Id,
            worldEvent.ScheduledForWorldMilliseconds,
            State.Adam.Id.Value,
            $"A {intervention.Vessel} delivered a Creator-supplied message.",
            options,
            plan.SelectedAction,
            $"{plan.Reason} Brain plan: {plan.Id}.");
        _offeredChoices.Add(choice);
        return choice;
    }

    private OfferedChoiceState OfferAdamDirectAddressChoice(AddressChannelState channel, string message)
    {
        string[] options = ["listen", "question", "wait", "turn away"];
        ReasonedPlanState plan = AddReasonedPlan(OracleBrainPlanner.PlanAdamDirectAddress(
            _nextPlanId++,
            Clock.WorldMilliseconds,
            State.Seed,
            State.Adam,
            message,
            options));

        OfferedChoiceState choice = new(
            _nextChoiceId++,
            0,
            Clock.WorldMilliseconds,
            State.Adam.Id.Value,
            $"A direct address reached Adam through {channel.Prompt}.",
            options,
            plan.SelectedAction,
            $"{plan.Reason} Brain plan: {plan.Id}.");
        _offeredChoices.Add(choice);
        return choice;
    }

    private ReasonedPlanState AddReasonedPlan(ReasonedPlanState plan)
    {
        _reasonedPlans.Add(plan);
        Ledger.RecordCreator(
            plan.CreatedAtWorldMilliseconds,
            "BRAIN PLAN",
            $"{plan.BrainSystem} created plan {plan.Id} for {plan.ActorId}. Goal: {plan.Goal} Selected: {plan.SelectedAction}. Reason: {plan.Reason}");
        return plan;
    }

    private IReadOnlyList<AttentionState> CreateDefaultAttentionStates() =>
    [
        new(
            State.Adam.Id.Value,
            State.Adam.Name,
            State.Garden.Id.Value,
            State.Garden.Name,
            "first Garden awareness",
            Clock.WorldMilliseconds,
            "world default"),
        new(
            State.Yala.Id.Value,
            State.Yala.TrueName,
            State.Garden.Id.Value,
            State.Garden.Name,
            "Oracle watches the Garden, but Creator-only truth and the Spark remain protected.",
            Clock.WorldMilliseconds,
            "world default")
    ];

    private ObservationState RecordGardenObservation(
        string subjectId,
        string subjectName,
        string observationKind,
        string detail,
        string distanceBand,
        bool adamReceives,
        bool creatorTruthHidden,
        string source,
        long? observedAtWorldMilliseconds = null)
    {
        long observationWorldMilliseconds = observedAtWorldMilliseconds ?? Clock.WorldMilliseconds;
        ObservationState observation = new(
            _nextObservationId++,
            observationWorldMilliseconds,
            State.Yala.Id.Value,
            State.Yala.TrueName,
            subjectId,
            subjectName,
            observationKind,
            detail,
            distanceBand,
            AttentionMatched(State.Yala.TrueName, subjectId),
            AdamReceives: false,
            CreatorTruthHidden: creatorTruthHidden,
            Source: source);
        _observations.Add(observation);

        if (adamReceives)
        {
            RecordAdamObservation(
                subjectId,
                subjectName,
                observationKind,
                detail,
                distanceBand,
                AttentionMatched(State.Adam.Name, subjectId),
                creatorTruthHidden,
                source,
                observationWorldMilliseconds);
        }

        return observation;
    }

    private ObservationState RecordAdamObservation(
        string subjectId,
        string subjectName,
        string observationKind,
        string detail,
        string distanceBand,
        bool attentionMatched,
        bool creatorTruthHidden,
        string source,
        long? observedAtWorldMilliseconds = null)
    {
        long observationWorldMilliseconds = observedAtWorldMilliseconds ?? Clock.WorldMilliseconds;
        ObservationState observation = new(
            _nextObservationId++,
            observationWorldMilliseconds,
            State.Adam.Id.Value,
            State.Adam.Name,
            subjectId,
            subjectName,
            observationKind,
            detail,
            distanceBand,
            attentionMatched,
            AdamReceives: true,
            CreatorTruthHidden: creatorTruthHidden,
            Source: source);
        _observations.Add(observation);
        Ledger.RecordCreator(
            observationWorldMilliseconds,
            "OBSERVATION",
            $"Adam observed {subjectName} by {observationKind}. Creator truth hidden: {creatorTruthHidden}.");
        return observation;
    }

    private bool AttentionMatched(string actorName, string subjectId) =>
        _attentionStates.Any(attention =>
            attention.ActorName.Equals(actorName, StringComparison.OrdinalIgnoreCase) &&
            attention.TargetId.Equals(subjectId, StringComparison.OrdinalIgnoreCase));

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

    private static string CreateAdamNamingReason(LivingKindState kind, int index)
    {
        string[] reasons =
        [
            "its steady walk made the Garden ground seem to move with it.",
            "its call cut through the air before Adam had another word for it.",
            "its body slipped through water like a living line.",
            "it broke the earth and vanished under root and stone.",
            "its hands and eyes troubled Adam with a nearness he could not yet understand.",
            "its horns and patience made it seem made for grass.",
            "its silence belonged to the dark before Adam could name fear.",
            "its many voices rose where water and mud met.",
            "it slept on stone until the sun warmed it into motion.",
            "it ran like ash blown low across the plain.",
            "it reminded Adam of a rough old tree stump that had learned to move.",
            "it stood near enough to Adam to make him search for a harder name."
        ];

        return reasons[index % reasons.Length];
    }
}
