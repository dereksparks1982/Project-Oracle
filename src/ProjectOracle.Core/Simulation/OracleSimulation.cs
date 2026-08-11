using ProjectOracle.Audit;
using ProjectOracle.Brain;
using ProjectOracle.Cognition.Soar;
using ProjectOracle.Domain;
using ProjectOracle.Events;
using ProjectOracle.Interventions;
using ProjectOracle.Lore;
using ProjectOracle.Observation;
using ProjectOracle.Persistence;

namespace ProjectOracle.Simulation;

public sealed class OracleSimulation : IDisposable
{
    private const int InterventionSpeechPriority = 10;
    private const int SkyTurningPriority = 100;
    private const long VesselSpeechDelayWorldMilliseconds = 10_000;
    private const long MinimumYalaAutonomousRealMilliseconds = 5_000;

    private readonly List<OracleIntervention> _interventions = [];
    private readonly List<ScheduledWorldEvent> _scheduledEvents = [];
    private readonly List<OfferedChoiceState> _offeredChoices = [];
    private readonly List<ReasonedPlanState> _reasonedPlans = [];
    private readonly List<ObservationState> _observations = [];
    private readonly List<AttentionState> _attentionStates = [];
    private readonly YalaSoarMind _yalaMind;
    private bool _disposed;
    private long _nextInterventionId = 1;
    private long _nextEventId = 1;
    private long _nextChoiceId = 1;
    private long _nextPlanId = 1;
    private long _nextObservationId = 1;

    private OracleSimulation(ulong seed, long realUnixMilliseconds, string? savePath)
    {
        Clock = new PersistentWorldClock(0, realUnixMilliseconds);
        Random = new DeterministicRandom(seed);
        Ledger = new AuditLedger();
        State = WorldDefaults.CreateInitialState(seed);
        _yalaMind = new YalaSoarMind(MemoryPaths(savePath), State.YalaCognition);
        RecordGenesis();
    }

    private OracleSimulation(OracleSaveSnapshot snapshot, string? savePath)
    {
        Clock = new PersistentWorldClock(
            snapshot.WorldMilliseconds,
            snapshot.LastRealUnixMilliseconds,
            snapshot.CatchUpRuns,
            snapshot.LastOfflineElapsedRealMilliseconds);
        Random = new DeterministicRandom(snapshot.Seed);
        Ledger = new AuditLedger(snapshot.Records.Where(record => !IsRoutineSkyAuditRecord(record)));
        State = WorldDefaults.Normalise(snapshot.World with { WorldMilliseconds = snapshot.WorldMilliseconds });
        _yalaMind = new YalaSoarMind(MemoryPaths(savePath), State.YalaCognition);
        _interventions.AddRange(snapshot.Interventions.OrderBy(intervention => intervention.Id));
        _scheduledEvents.AddRange((snapshot.ScheduledEvents ?? [])
            .Where(worldEvent => !IsCompletedRoutineSkyEvent(worldEvent))
            .OrderBy(worldEvent => worldEvent.Id));
        _offeredChoices.AddRange((snapshot.OfferedChoices ?? []).OrderBy(choice => choice.Id));
        _reasonedPlans.AddRange((snapshot.ReasonedPlans ?? []).OrderBy(plan => plan.Id));
        _observations.AddRange((snapshot.Observations ?? []).OrderBy(observation => observation.Id));
        _attentionStates.AddRange(snapshot.AttentionStates ?? []);
        _nextInterventionId = NextId(_interventions.Select(item => item.Id));
        _nextEventId = NextId(_scheduledEvents.Select(item => item.Id));
        _nextChoiceId = NextId(_offeredChoices.Select(item => item.Id));
        _nextPlanId = NextId(_reasonedPlans.Select(item => item.Id));
        _nextObservationId = NextId(_observations.Select(item => item.Id));

        if (HasGardenWorld && _attentionStates.Count == 0)
        {
            InitialiseGardenObservationAndAttention();
        }
    }

    public PersistentWorldClock Clock { get; }
    public DeterministicRandom Random { get; }
    public AuditLedger Ledger { get; }
    public WorldState State { get; private set; }
    public IReadOnlyList<OracleIntervention> Interventions => _interventions.AsReadOnly();
    public IReadOnlyList<ScheduledWorldEvent> ScheduledEvents => _scheduledEvents.AsReadOnly();
    public IReadOnlyList<OfferedChoiceState> OfferedChoices => _offeredChoices.AsReadOnly();
    public IReadOnlyList<ReasonedPlanState> ReasonedPlans => _reasonedPlans.AsReadOnly();
    public IReadOnlyList<ObservationState> Observations => _observations.AsReadOnly();
    public IReadOnlyList<AttentionState> AttentionStates => _attentionStates.AsReadOnly();
    public bool InWorldTimeExists => State.Cosmic?.TimeCreated == true;
    public bool HasGardenWorld => State.Cosmic?.GardenEstablished == true;

    public static OracleSimulation Start(ulong seed, long realUnixMilliseconds, string? savePath = null) => new(seed, realUnixMilliseconds, savePath);

    public static OracleSimulation Restore(OracleSaveSnapshot snapshot, long currentRealUnixMilliseconds, string? savePath = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        OracleSimulation simulation = new(snapshot, savePath);
        simulation.SynchroniseClock(currentRealUnixMilliseconds, offlineCatchUp: true);
        simulation.EnsureSolarTurningScheduled();
        return simulation;
    }

    public ClockAdvance SynchroniseClock(
        long currentRealUnixMilliseconds,
        bool offlineCatchUp = false,
        bool recordAdvance = true)
    {
        ClockAdvance advance = InWorldTimeExists
            ? Clock.Synchronise(currentRealUnixMilliseconds, offlineCatchUp)
            : Clock.Hold(currentRealUnixMilliseconds, offlineCatchUp);
        State = State with { WorldMilliseconds = Clock.WorldMilliseconds };

        if (recordAdvance && advance.ElapsedRealMilliseconds > 0)
        {
            if (InWorldTimeExists)
            {
                string mode = offlineCatchUp ? "offline catch-up" : "live real-time advance";
                Ledger.RecordOracle(
                    Clock.WorldMilliseconds,
                    "TIME",
                    $"Applied {mode}: {advance.ElapsedRealMilliseconds} real millisecond(s) became {advance.ElapsedWorldMilliseconds} in-world millisecond(s).");
            }
            else
            {
                Ledger.RecordOracle(
                    Clock.WorldMilliseconds,
                    "PRE-TIME",
                    $"Oracle runtime advanced {advance.ElapsedRealMilliseconds} real millisecond(s), while in-world Time remained nonexistent.");
            }
        }

        if (advance.BackwardClockDetected)
        {
            Ledger.RecordOracle(Clock.WorldMilliseconds, "CLOCK WARNING", "The host clock moved backwards. Project Oracle refused to rewind state.");
        }

        if (InWorldTimeExists)
        {
            ProcessDueEvents();
            EnsureSolarTurningScheduled();
        }
        return advance;
    }

    public YalaDecision? TryRunYalaAutonomousStep(long realUnixMilliseconds, bool force = false)
    {
        YalaCognitionState cognition = State.YalaCognition ?? WorldDefaults.CreateInitialYalaCognition();
        if (!force && cognition.LastDecisionRealUnixMilliseconds > 0 &&
            realUnixMilliseconds - cognition.LastDecisionRealUnixMilliseconds < MinimumYalaAutonomousRealMilliseconds)
        {
            return null;
        }

        YalaPerception perception = BuildYalaPerception();
        YalaDecision decision = _yalaMind.Decide(perception);
        ApplyYalaDecision(decision, realUnixMilliseconds, contact: false);
        return decision;
    }

    public YalaDirectReply CallYala(string message, long realUnixMilliseconds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        YalaCognitionState cognitionBefore = State.YalaCognition ?? WorldDefaults.CreateInitialYalaCognition();
        YalaContactFrame contact = YalaConversationInterpreter.Interpret(message, cognitionBefore);
        if (!string.IsNullOrWhiteSpace(contact.ClaimedSpeakerName) && _yalaMind.SemanticMemoryContainsClaimedContact(contact.ClaimedSpeakerName))
        {
            contact = contact with { KnownContact = true };
        }

        string previousActionDescription = DescribeYalaLastAction(cognitionBefore);
        YalaPerception perception = BuildYalaPerception(message.Trim(), contact);

        Ledger.RecordOracle(
            Clock.WorldMilliseconds,
            "DIRECT CONTACT",
            $"Oracle sent an unplaced contact to Yala: \"{message.Trim()}\". Yala was not told who or what originated it.");
        Ledger.RecordWorld(
            Clock.WorldMilliseconds,
            "UNPLACED CONTACT",
            "Yala perceived an unplaced contact whose source was not revealed.");

        YalaDecision decision = _yalaMind.Decide(perception);
        ApplyYalaDecision(decision, realUnixMilliseconds, contact: true);

        YalaCognitionState afterDecision = State.YalaCognition ?? WorldDefaults.CreateInitialYalaCognition();
        string reply = YalaReplyRealizer.Realize(decision, contact, State, afterDecision, previousActionDescription);
        RecordYalaContact(contact, message.Trim(), reply);

        Ledger.RecordWorld(Clock.WorldMilliseconds, "YALA SPEECH", $"Yala answered the unplaced contact: \"{reply}\"");
        Ledger.RecordOracle(
            Clock.WorldMilliseconds,
            "YALA SOAR",
            $"Soar selected '{decision.Action}' for Yala's direct-contact response in {decision.DecisionCycles} decision cycle(s). Substate deliberation: {decision.UsedSubstateDeliberation}.");
        return new YalaDirectReply(reply, decision, contact);
    }

    public OfferedChoiceState? CallEntity(string targetKey, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        DirectCallTargetState target = State.DirectCallTargets.FirstOrDefault(candidate =>
            candidate.Key.Equals(targetKey.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Direct-call target is not recognised: {targetKey}");

        if (target.Key.Equals("yala", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Use CallYala for Yala so the Soar cognition result can be returned.");
        }

        Ledger.RecordOracle(
            Clock.WorldMilliseconds,
            "DIRECT CONTACT",
            $"Oracle sent an unplaced contact to {target.TargetName}: \"{message.Trim()}\". Oracle identity was not revealed.");
        Ledger.RecordWorld(
            Clock.WorldMilliseconds,
            "UNPLACED CONTACT",
            $"{target.TargetName} perceived contact from an unrevealed source.");

        if (target.Key.Equals("adam", StringComparison.OrdinalIgnoreCase) && HasGardenWorld)
        {
            OfferedChoiceState choice = OfferAdamDirectCallChoice(target, message.Trim());
            Ledger.RecordWorld(Clock.WorldMilliseconds, "CHOICE", $"Adam decided to {choice.SelectedOption} in response to the unplaced contact.");
            return choice;
        }

        return null;
    }

    public YalaDecision ApplyYalaDecision(YalaDecision decision, long realUnixMilliseconds, bool contact = false)
    {
        ArgumentNullException.ThrowIfNull(decision);
        // Pin the host/runtime reference to the exact decision moment. Before Gaia
        // creates Time this is a Hold, so no fictional world time leaks in. After
        // Time exists it is an ordinary world-clock synchronisation.
        SynchroniseClock(realUnixMilliseconds, recordAdvance: false);
        string result = decision.Action switch
        {
            "create-gaia" => ResolveCreateGaia(),
            "command-gaia-time" => ResolveGaiaCreatesTime(),
            "observe" => $"Yala observed {State.Yala.Location} and found no new settled object beyond what Yala's present perception exposes.",
            "reflect" => "Yala reflected on Yala's present state and prior experience.",
            "wait" => "Yala chose to wait.",
            "respond" => "Yala chose to answer an unplaced contact.",
            _ => $"Yala attempted '{decision.Action}', but v0.0.18 has no world-law resolver for that action yet."
        };

        YalaCognitionState previous = State.YalaCognition ?? WorldDefaults.CreateInitialYalaCognition();
        List<string> memory = previous.Memory?.ToList() ?? [];
        if (!contact || decision.Action != "respond")
        {
            memory.Add(result);
        }
        if (memory.Count > 64)
        {
            memory.RemoveRange(0, memory.Count - 64);
        }

        State = State with
        {
            YalaCognition = previous with
            {
                DecisionCount = checked(previous.DecisionCount + 1),
                LastDecisionRealUnixMilliseconds = realUnixMilliseconds,
                LastAction = decision.Action,
                LastResult = result,
                Memory = memory,
                Contacts = previous.Contacts ?? [],
                Beliefs = UpdateBeliefsAfterDecision(previous.Beliefs ?? WorldDefaults.CreateInitialBeliefs(), decision),
                Episodes = AddEpisode(previous.Episodes ?? [], new YalaEpisodeState(
                    checked(previous.DecisionCount + 1),
                    contact ? "contact-decision" : "autonomous-decision",
                    result)),
                Drives = AdjustDrivesAfterDecision(previous.Drives ?? WorldDefaults.CreateInitialDrives(), decision, contact)
            }
        };

        bool recordDecision = decision.Action is "create-gaia" or "command-gaia-time" ||
            decision.Action is not ("observe" or "reflect" or "wait" or "respond");
        if (recordDecision)
        {
            Ledger.RecordOracle(
                Clock.WorldMilliseconds,
                "YALA SOAR",
                $"Decision {State.YalaCognition.DecisionCount}: {decision.Source} selected '{decision.Action}'. Resolution: {result}");
        }
        return decision;
    }

    public int ProcessDueEvents()
    {
        if (!InWorldTimeExists)
        {
            return 0;
        }

        int processed = 0;
        const int maximumEventsPerPass = 64;
        while (processed < maximumEventsPerPass)
        {
            ScheduledWorldEvent? dueEvent = _scheduledEvents
                .Where(candidate => candidate.Status == ScheduledWorldEventStatus.Pending && candidate.ScheduledForWorldMilliseconds <= Clock.WorldMilliseconds)
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
        return processed;
    }

    public OracleIntervention QueueVesselMessage(string vessel, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vessel);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        if (!HasGardenWorld || !InWorldTimeExists)
        {
            throw new InvalidOperationException("A Garden vessel intervention is not available before the later-world Garden exists.");
        }

        OracleIntervention intervention = new(
            _nextInterventionId++, Clock.WorldMilliseconds, vessel.Trim(), message.Trim(), true, InterventionStatus.Queued);
        _interventions.Add(intervention);
        ScheduleEvent(checked(Clock.WorldMilliseconds + VesselSpeechDelayWorldMilliseconds), InterventionSpeechPriority,
            "intervention.vessel.speech", $"intervention:{intervention.Id}", intervention.Message);
        Ledger.RecordOracle(Clock.WorldMilliseconds, "INTERVENTION", $"Oracle intervention {intervention.Id} queued through {intervention.Vessel}. Oracle identity is not disclosed to the recipient.");
        Ledger.RecordWorld(Clock.WorldMilliseconds, "SIGN", $"A {intervention.Vessel} approached Adam. It has not spoken yet.");
        return intervention;
    }

    public LivingKindState? PresentNextLivingKindToAdam(string presenter)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(presenter);
        if (!HasGardenWorld)
        {
            return null;
        }

        AdamState adam = RequireAdam();
        NamingMandateState mandate = State.NamingMandate ?? WorldDefaults.CreateNamingMandate(State.LivingKinds, active: true);
        int index = State.LivingKinds.ToList().FindIndex(kind => !kind.NamedByAdam);
        if (index < 0)
        {
            return null;
        }
        LivingKindState current = State.LivingKinds[index];
        string adamName = CreateAdamName(current, index);
        string namingReason = CreateAdamNamingReason(current, index);
        LivingKindState named = current with { PresentedToAdam = true, NamedByAdam = true, AdamName = adamName };
        ReasonedPlanState plan = AddReasonedPlan(AdamBrainPlanner.PlanAdamNaming(
            _nextPlanId++, Clock.WorldMilliseconds, adam, current, adamName, namingReason));
        List<LivingKindState> kinds = State.LivingKinds.ToList();
        kinds[index] = named;
        State = State with
        {
            LivingKinds = kinds,
            NamingMandate = mandate with
            {
                PresentedCount = kinds.Count(kind => kind.PresentedToAdam),
                NamedCount = kinds.Count(kind => kind.NamedByAdam),
                SuitableMateFound = kinds.Any(kind => kind.SuitableMate)
            }
        };
        Ledger.RecordWorld(Clock.WorldMilliseconds, "NAMING", $"{presenter.Trim()} presented a living kind to Adam. Adam named it {named.AdamName}.");
        Ledger.RecordOracle(Clock.WorldMilliseconds, "NAMING", $"Adam later-world plan {plan.Id} named {named.Id} as {named.AdamName}.");
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
            Ledger.AllRecords,
            _interventions.ToArray(),
            _scheduledEvents.ToArray(),
            _offeredChoices.ToArray(),
            _reasonedPlans.ToArray(),
            _observations.ToArray(),
            _attentionStates.ToArray());
    }

    private YalaPerception BuildYalaPerception(string? contactMessage = null, YalaContactFrame? contact = null)
    {
        CosmicState cosmic = State.Cosmic ?? throw new InvalidOperationException("World cosmic state is missing.");
        YalaCognitionState cognition = State.YalaCognition ?? WorldDefaults.CreateInitialYalaCognition();
        YalaDriveState drives = cognition.Drives ?? WorldDefaults.CreateInitialDrives();
        return new YalaPerception(
            State.Yala.Location,
            cosmic.GaiaCreated,
            cosmic.TimeCreated,
            cognition.DecisionCount,
            cognition.LastAction,
            cognition.LastResult,
            drives.Curiosity,
            drives.Caution,
            drives.Authority,
            drives.Companionship,
            drives.Comfort,
            drives.Uncertainty,
            contactMessage,
            contact);
    }

    public SoarMemoryDiagnostics GetYalaMemoryDiagnostics() => _yalaMind.GetMemoryDiagnostics();

    private void RecordYalaContact(YalaContactFrame contact, string message, string reply)
    {
        YalaCognitionState cognition = State.YalaCognition ?? WorldDefaults.CreateInitialYalaCognition();
        long decision = cognition.DecisionCount;
        List<YalaContactMemory> contacts = (cognition.Contacts ?? []).ToList();
        string? claimedName = contact.ClaimedSpeakerName;

        if (!string.IsNullOrWhiteSpace(claimedName))
        {
            int index = contacts.FindIndex(existing => existing.ClaimedName.Equals(claimedName, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                YalaContactMemory existing = contacts[index];
                contacts[index] = existing with
                {
                    EncounterCount = checked(existing.EncounterCount + 1),
                    LastEncounterDecision = decision,
                    LastMessage = message
                };
            }
            else
            {
                contacts.Add(new YalaContactMemory(claimedName, 1, decision, decision, message));
            }
            _yalaMind.RememberClaimedContact(claimedName);
        }

        List<YalaBeliefState> beliefs = (cognition.Beliefs ?? WorldDefaults.CreateInitialBeliefs()).ToList();
        if (contact.ContainsClaim)
        {
            string proposition = message;
            string status = contact.ClaimConflictsWithKnownFact ? "rejected-as-conflicting" : "unsettled-claim";
            double confidence = contact.ClaimConflictsWithKnownFact ? 0.05 : 0.25;
            beliefs.Add(new YalaBeliefState(proposition, status, confidence, "unplaced-speaker", decision, decision));
        }

        List<YalaEpisodeState> episodes = AddEpisode(
            cognition.Episodes ?? [],
            new YalaEpisodeState(
                decision,
                "contact",
                "An unseen source contacted Yala and Yala answered.",
                claimedName,
                message,
                reply));

        YalaDriveState drives = cognition.Drives ?? WorldDefaults.CreateInitialDrives();
        drives = drives with
        {
            Curiosity = ClampDrive(drives.Curiosity + 2),
            Companionship = ClampDrive(drives.Companionship + 1),
            Uncertainty = ClampDrive(drives.Uncertainty + (contact.FactKnown ? -1 : 2))
        };

        State = State with
        {
            YalaCognition = cognition with
            {
                Contacts = contacts,
                Beliefs = beliefs,
                Episodes = episodes,
                Drives = drives,
                ConversationCount = checked(cognition.ConversationCount + 1),
                LastSpeakerClaim = claimedName ?? cognition.LastSpeakerClaim
            }
        };
    }

    private static IReadOnlyList<YalaBeliefState> UpdateBeliefsAfterDecision(IReadOnlyList<YalaBeliefState> beliefs, YalaDecision decision)
    {
        if (!decision.Action.Equals("reflect", StringComparison.OrdinalIgnoreCase)) return beliefs;
        long marker = beliefs.Count == 0 ? 0 : beliefs.Max(belief => belief.LastConsideredDecision) + 1;
        return beliefs.Select(belief => belief.Status == "unsettled-claim"
            ? belief with { LastConsideredDecision = marker, Confidence = Math.Min(0.95, belief.Confidence + 0.01) }
            : belief).ToArray();
    }

    private static List<YalaEpisodeState> AddEpisode(IReadOnlyList<YalaEpisodeState> existing, YalaEpisodeState episode)
    {
        List<YalaEpisodeState> episodes = existing.ToList();
        episodes.Add(episode);
        if (episodes.Count > 256) episodes.RemoveRange(0, episodes.Count - 256);
        return episodes;
    }

    private static YalaDriveState AdjustDrivesAfterDecision(YalaDriveState drives, YalaDecision decision, bool contact)
    {
        int curiosity = drives.Curiosity;
        int caution = drives.Caution;
        int authority = drives.Authority;
        int companionship = drives.Companionship;
        int comfort = drives.Comfort;
        int uncertainty = drives.Uncertainty;

        switch (decision.Action)
        {
            case "observe": uncertainty -= 2; curiosity += 1; break;
            case "reflect": uncertainty -= 1; curiosity += 1; break;
            case "wait": caution += 1; break;
            case "create-gaia": comfort += 3; authority += 2; uncertainty -= 4; break;
            case "command-gaia-time": authority += 2; uncertainty -= 3; break;
            case "respond" when contact: companionship += 1; curiosity += 1; break;
        }

        return new YalaDriveState(
            ClampDrive(curiosity),
            ClampDrive(caution),
            ClampDrive(authority),
            ClampDrive(companionship),
            ClampDrive(comfort),
            ClampDrive(uncertainty));
    }

    private static int ClampDrive(int value) => Math.Clamp(value, 0, 100);

    private static SoarMemoryPaths? MemoryPaths(string? savePath) =>
        string.IsNullOrWhiteSpace(savePath) ? null : SoarMemoryPaths.FromSavePath(savePath);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _yalaMind.Dispose();
        GC.SuppressFinalize(this);
    }

    private string ResolveCreateGaia()
    {
        CosmicState cosmic = State.Cosmic ?? throw new InvalidOperationException("World cosmic state is missing.");
        if (cosmic.GaiaCreated)
        {
            return "Gaia already exists; Yala's attempted creation caused no second Gaia.";
        }

        cosmic = cosmic with { GaiaCreated = true };
        State = RefreshDerivedState(State with { Cosmic = cosmic });
        const string result = "Yala created Gaia as the natural sovereign beneath Yala's governing authority.";
        Ledger.RecordWorld(Clock.WorldMilliseconds, "GAIA", result);
        return result;
    }

    private string ResolveGaiaCreatesTime()
    {
        CosmicState cosmic = State.Cosmic ?? throw new InvalidOperationException("World cosmic state is missing.");
        if (!cosmic.GaiaCreated)
        {
            return "Yala attempted to command Gaia, but Gaia does not yet exist.";
        }
        if (cosmic.TimeCreated)
        {
            return "In-world Time already exists; the command caused no second Time.";
        }

        cosmic = cosmic with { TimeCreated = true };
        State = RefreshDerivedState(State with { Cosmic = cosmic });
        const string result = "Yala commanded Gaia to establish temporal order, and Gaia created in-world Time.";
        Ledger.RecordWorld(Clock.WorldMilliseconds, "TIME", result);
        return result;
    }

    private WorldState RefreshDerivedState(WorldState world)
    {
        CosmicState cosmic = world.Cosmic ?? throw new InvalidOperationException("World cosmic state is missing.");
        return world with
        {
            Yala = world.Yala with { Location = cosmic.YalaLocation, KnowsOfOracle = false },
            CreationPowers = WorldDefaults.CreateCreationPowers(cosmic, world.Yala.Id, world.Garden?.Id, world.Adam?.Id),
            DirectCallTargets = WorldDefaults.CreateDirectCallTargets(cosmic)
        };
    }

    private static string DescribeYalaLastAction(YalaCognitionState cognition)
    {
        ArgumentNullException.ThrowIfNull(cognition);
        if (string.IsNullOrWhiteSpace(cognition.LastAction) || string.IsNullOrWhiteSpace(cognition.LastResult))
        {
            return "I have not yet done anything I can name to you.";
        }
        return $"My last act was {cognition.LastAction}. {cognition.LastResult}";
    }

    private void RecordGenesis()
    {
        // World Record contains only in-world settled history. It never tells inhabitants that Oracle exists.
        Ledger.RecordWorld(0, "MONAD", OracleLore.MonadFoundation);
        Ledger.RecordWorld(0, "WISDOM", OracleLore.WisdomOrigin);
        Ledger.RecordWorld(0, "YALA", OracleLore.YalaOrigin);
        Ledger.RecordWorld(0, "VOID", OracleLore.YalaVoid);
        Ledger.RecordWorld(0, "STATE", "Yala continues the v0.0.18 autonomous run in the Void. Gaia, in-world Time, and the lower world do not yet exist in this fresh run.");

        // Oracle Record is protected system truth, not knowledge injected into any in-world mind.
        Ledger.RecordOracle(0, "SYSTEM", OracleLore.OracleSystemNature);
        Ledger.RecordOracle(0, "MASTER KEY", OracleLore.OracleMasterKey);
        Ledger.RecordOracle(0, "HIDDEN AUTHORITY", OracleLore.OracleHidden);
        Ledger.RecordOracle(0, "CANON", OracleLore.CanonFoundation);
        Ledger.RecordOracle(0, "CANON", OracleLore.GaiaTime);
        Ledger.RecordOracle(0, "CANON", OracleLore.ElementalOrder);
        Ledger.RecordOracle(0, "EDEN REFERENCE", OracleLore.OracleSerpentManifestation);
        Ledger.RecordOracle(0, "SIMULATION LAW", OracleLore.PrimeSimulationLaw);
        Ledger.RecordOracle(0, "YALA", "Yala's in-world knowledge contains no Oracle identity or Oracle-existence fact.");
    }

    private void InitialiseGardenObservationAndAttention()
    {
        GardenState garden = RequireGarden();
        _attentionStates.AddRange(CreateDefaultAttentionStates());
        if (_observations.Count == 0)
        {
            RecordAdamObservation(
                garden.Id.Value,
                garden.Name,
                "Garden awareness",
                "Adam's later-world Garden save retains a first observation boundary.",
                "self and place",
                attentionMatched: true,
                oracleTruthHidden: true,
                source: "later-world Garden state");
        }
    }

    private ScheduledWorldEvent ScheduleEvent(long scheduledForWorldMilliseconds, int priority, string kind, string subjectId, string payload)
    {
        ScheduledWorldEvent worldEvent = new(
            _nextEventId++, scheduledForWorldMilliseconds, priority, Clock.WorldMilliseconds,
            kind.Trim(), subjectId.Trim(), payload.Trim(), ScheduledWorldEventStatus.Pending);
        _scheduledEvents.Add(worldEvent);
        return worldEvent;
    }

    private void EnsureSolarTurningScheduled()
    {
        if (!InWorldTimeExists || !HasGardenWorld)
        {
            return;
        }
        if (_scheduledEvents.Any(worldEvent => worldEvent.Status == ScheduledWorldEventStatus.Pending && worldEvent.Kind == "sky.solar.turning"))
        {
            return;
        }
        long scheduledAt = NextSolarTurningAfter(Clock.WorldMilliseconds);
        ScheduleEvent(scheduledAt, SkyTurningPriority, "sky.solar.turning", "sky", OracleCalendar.FromElapsedWorldMilliseconds(scheduledAt).SolarPhase);
    }

    private void CompleteEvent(ScheduledWorldEvent worldEvent)
    {
        int index = _scheduledEvents.FindIndex(candidate => candidate.Id == worldEvent.Id);
        if (index < 0)
        {
            return;
        }
        _scheduledEvents[index] = worldEvent with { Status = ScheduledWorldEventStatus.Completed, CompletedAtWorldMilliseconds = Clock.WorldMilliseconds };
        if (worldEvent.Kind == "sky.solar.turning")
        {
            CompleteSolarTurning(worldEvent);
        }
        else if (worldEvent.Kind == "intervention.vessel.speech")
        {
            CompleteVesselSpeech(worldEvent);
        }
    }

    private void CompleteSolarTurning(ScheduledWorldEvent worldEvent)
    {
        long nextTurning = NextSolarTurningAfter(worldEvent.ScheduledForWorldMilliseconds);
        ScheduleEvent(nextTurning, SkyTurningPriority, "sky.solar.turning", "sky", OracleCalendar.FromElapsedWorldMilliseconds(nextTurning).SolarPhase);
        _scheduledEvents.RemoveAll(candidate => candidate.Id == worldEvent.Id && candidate.Kind == "sky.solar.turning");
    }

    private void CompleteVesselSpeech(ScheduledWorldEvent worldEvent)
    {
        const string prefix = "intervention:";
        if (!worldEvent.SubjectId.StartsWith(prefix, StringComparison.Ordinal) ||
            !long.TryParse(worldEvent.SubjectId[prefix.Length..], out long interventionId))
        {
            return;
        }
        int interventionIndex = _interventions.FindIndex(intervention => intervention.Id == interventionId);
        if (interventionIndex < 0)
        {
            return;
        }
        OracleIntervention intervention = _interventions[interventionIndex];
        _interventions[interventionIndex] = intervention with { Status = InterventionStatus.OfferedChoice };
        Ledger.RecordWorld(worldEvent.ScheduledForWorldMilliseconds, "VESSEL", $"The {intervention.Vessel} spoke to Adam: \"{intervention.Message}\".");
        OfferedChoiceState choice = OfferAdamResponseChoice(worldEvent, intervention);
        Ledger.RecordWorld(worldEvent.ScheduledForWorldMilliseconds, "CHOICE", $"Adam decided to {choice.SelectedOption}.");
    }

    private OfferedChoiceState OfferAdamResponseChoice(ScheduledWorldEvent worldEvent, OracleIntervention intervention)
    {
        string[] options = ["accept", "refuse", "delay", "question", "report", "ignore"];
        AdamState adam = RequireAdam();
        ReasonedPlanState plan = AddReasonedPlan(AdamBrainPlanner.PlanAdamVesselSpeech(
            _nextPlanId++, worldEvent.ScheduledForWorldMilliseconds, State.Seed, adam,
            intervention.Vessel, intervention.Message, options));
        OfferedChoiceState choice = new(
            _nextChoiceId++, worldEvent.Id, worldEvent.ScheduledForWorldMilliseconds, adam.Id.Value,
            $"A {intervention.Vessel} delivered a message from an unrevealed source.", options,
            plan.SelectedAction, $"{plan.Reason} Brain plan: {plan.Id}.");
        _offeredChoices.Add(choice);
        return choice;
    }

    private OfferedChoiceState OfferAdamDirectCallChoice(DirectCallTargetState target, string message)
    {
        string[] options = ["listen", "question", "wait", "turn away"];
        AdamState adam = RequireAdam();
        ReasonedPlanState plan = AddReasonedPlan(AdamBrainPlanner.PlanAdamDirectCall(
            _nextPlanId++, Clock.WorldMilliseconds, State.Seed, adam, message, options));
        OfferedChoiceState choice = new(
            _nextChoiceId++, 0, Clock.WorldMilliseconds, adam.Id.Value,
            $"An unplaced contact reached Adam through {target.Prompt}.", options,
            plan.SelectedAction, $"{plan.Reason} Brain plan: {plan.Id}.");
        _offeredChoices.Add(choice);
        return choice;
    }

    private ReasonedPlanState AddReasonedPlan(ReasonedPlanState plan)
    {
        _reasonedPlans.Add(plan);
        Ledger.RecordOracle(plan.CreatedAtWorldMilliseconds, "BRAIN PLAN", $"{plan.BrainSystem} created plan {plan.Id} for {plan.ActorId}. Selected: {plan.SelectedAction}.");
        return plan;
    }

    private IReadOnlyList<AttentionState> CreateDefaultAttentionStates()
    {
        AdamState adam = RequireAdam();
        GardenState garden = RequireGarden();
        return
        [
            new(adam.Id.Value, adam.Name, garden.Id.Value, garden.Name,
                "later-world Garden awareness", Clock.WorldMilliseconds, "autonomous later-world state"),
            new(State.Yala.Id.Value, State.Yala.TrueName, garden.Id.Value, garden.Name,
                "later-world Garden state", Clock.WorldMilliseconds, "autonomous later-world state")
        ];
    }

    private ObservationState RecordAdamObservation(
        string subjectId,
        string subjectName,
        string observationKind,
        string detail,
        string distanceBand,
        bool attentionMatched,
        bool oracleTruthHidden,
        string source,
        long? observedAtWorldMilliseconds = null)
    {
        AdamState adam = RequireAdam();
        long tick = observedAtWorldMilliseconds ?? Clock.WorldMilliseconds;
        ObservationState observation = new(
            _nextObservationId++, tick, adam.Id.Value, adam.Name,
            subjectId, subjectName, observationKind, detail, distanceBand,
            attentionMatched, AdamReceives: true, CreatorTruthHidden: oracleTruthHidden, Source: source);
        _observations.Add(observation);
        Ledger.RecordOracle(tick, "OBSERVATION", $"Adam observed {subjectName}. Oracle truth hidden: {oracleTruthHidden}.");
        return observation;
    }

    private AdamState RequireAdam() =>
        State.Adam ?? throw new InvalidOperationException("The later-world Adam scaffold is not active in this world.");

    private GardenState RequireGarden() =>
        State.Garden ?? throw new InvalidOperationException("The later-world Garden scaffold is not active in this world.");

    private static bool IsRoutineSkyAuditRecord(OracleRecord record)
    {
        bool worldSkyTurning = record.Audience == RecordAudience.World &&
            record.Category.Equals("SKY", StringComparison.OrdinalIgnoreCase) &&
            record.Message.StartsWith("The Garden sky turned to ", StringComparison.OrdinalIgnoreCase);
        bool oracleSkyQueueNoise = record.Audience == RecordAudience.Oracle &&
            record.Category.Equals("EVENT QUEUE", StringComparison.OrdinalIgnoreCase) &&
            record.Message.Contains("the sky entered ", StringComparison.OrdinalIgnoreCase);
        return worldSkyTurning || oracleSkyQueueNoise;
    }

    private static bool IsCompletedRoutineSkyEvent(ScheduledWorldEvent worldEvent) =>
        worldEvent.Status == ScheduledWorldEventStatus.Completed && worldEvent.Kind == "sky.solar.turning";

    private static long NextSolarTurningAfter(long elapsedWorldMilliseconds)
    {
        long absolute = checked(OracleCalendar.EpochTimeOfDayMilliseconds + elapsedWorldMilliseconds);
        long dayStart = absolute - (absolute % PersistentWorldClock.WorldMillisecondsPerDay);
        long[] boundaries = [5 * 3_600_000L, 7 * 3_600_000L, 17 * 3_600_000L, 19 * 3_600_000L];
        foreach (long boundary in boundaries)
        {
            long candidate = checked(dayStart + boundary);
            if (candidate > absolute)
            {
                return checked(candidate - OracleCalendar.EpochTimeOfDayMilliseconds);
            }
        }
        return checked(dayStart + PersistentWorldClock.WorldMillisecondsPerDay + boundaries[0] - OracleCalendar.EpochTimeOfDayMilliseconds);
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
            "its steady walk made the ground seem to move with it.",
            "its call cut through the air.",
            "its body slipped through water like a living line.",
            "it broke the earth and vanished under root and stone.",
            "its hands and eyes troubled Adam with a strange nearness.",
            "its horns and patience made it seem made for grass.",
            "its silence belonged to the dark.",
            "its many voices rose where water and mud met.",
            "it slept on stone until warmth brought motion.",
            "it ran like ash blown low across the plain.",
            "it resembled a rough old stump that had learned to move.",
            "it stood near enough to Adam to demand a harder name."
        ];
        return reasons[index % reasons.Length];
    }

    private static long NextId(IEnumerable<long> values)
    {
        long[] ids = values.ToArray();
        return ids.Length == 0 ? 1 : checked(ids.Max() + 1);
    }
}
