using ProjectOracle.Audit;
using ProjectOracle.Brain;
using ProjectOracle.Cognition;
using ProjectOracle.Cognition.Appraisal;
using ProjectOracle.Cognition.Soar;
using ProjectOracle.Cognition.CosmicChoice;
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
        Ledger = new AuditLedger(NormaliseHistoricalRecords(snapshot.Records).Where(record => !IsRoutineSkyAuditRecord(record)));
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
        string reply = YalaReplyRealizer.Realize(
            decision,
            contact,
            State,
            afterDecision,
            previousActionDescription,
            InWorldTimeExists ? Clock.Calendar : null);
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
        YalaAgencyPolicy.DemandAllowed(decision.Action);
        // Pin the host/runtime reference to the exact decision moment. Before Gaia
        // creates Time this is a Hold, so no fictional world time leaks in. After
        // Time exists it is an ordinary world-clock synchronisation.
        SynchroniseClock(realUnixMilliseconds, recordAdvance: false);
        string result = decision.Action switch
        {
            "create-gaia" => ResolveCreateGaia(),
            "command-gaia-time" => ResolveGaiaCreatesTime(),
            "enact-cosmic-choice" => ResolveCosmicChoice(decision),
            "ask-speaker" => ResolveAskSpeaker(),
            "observe" => $"Yala observed {State.Yala.Location} and found no new settled object beyond what Yala's present perception exposes.",
            "reflect" => "Yala reflected on Yala's present state, beliefs, goals, questions, and prior experience.",
            "wait" => "Yala chose to wait.",
            "respond" => "Yala chose to answer an unplaced contact.",
            _ => throw new InvalidOperationException($"Yala action '{decision.Action}' passed the agency policy without a world-law resolver.")
        };

        YalaCognitionState previous = State.YalaCognition ?? WorldDefaults.CreateInitialYalaCognition();
        List<string> memory = previous.Memory?.ToList() ?? [];
        if (!contact || decision.Action != "respond")
        {
            memory.Add(result);
        }
        if (memory.Count > 96)
        {
            memory.RemoveRange(0, memory.Count - 96);
        }

        IReadOnlyList<YalaActionMemoryState> actionMemory = UpdateActionMemory(previous.ActionMemory ?? [], decision, result, checked(previous.DecisionCount + 1));
        IReadOnlyList<YalaReflectionState> reflections = UpdateReflections(previous, decision, checked(previous.DecisionCount + 1));

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
                Drives = AdjustDrivesAfterDecision(previous.Drives ?? WorldDefaults.CreateInitialDrives(), decision, contact),
                ActionMemory = actionMemory,
                KnowledgeGaps = previous.KnowledgeGaps ?? [],
                LearnedLexicon = previous.LearnedLexicon ?? [],
                Dialogue = previous.Dialogue ?? [],
                Relationships = previous.Relationships ?? WorldDefaults.CreateInitialRelationships(),
                Questions = previous.Questions ?? [],
                TemporalEvents = previous.TemporalEvents ?? WorldDefaults.CreateInitialTemporalEvents(),
                Goals = previous.Goals ?? WorldDefaults.CreateInitialGoals(),
                Concerns = previous.Concerns ?? [],
                Appraisals = previous.Appraisals ?? [],
                Hypotheses = previous.Hypotheses ?? [],
                EntityModels = previous.EntityModels ?? [],
                Reflections = reflections,
                PendingAutonomousUtterance = previous.PendingAutonomousUtterance
            }
        };

        bool recordDecision = decision.Action is "create-gaia" or "command-gaia-time" or "ask-speaker" ||
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
        YalaQuestionState? pending = YalaQuestionPlanner.SelectNextAutonomous(cognition);
        YalaConcernState? activeConcern = (cognition.Concerns ?? [])
            .Where(item => item.Status.Equals("active", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.FirstSeenDecision)
            .FirstOrDefault();
        YalaAppraisalState? latestAppraisal = (cognition.Appraisals ?? [])
            .OrderByDescending(item => item.Sequence)
            .FirstOrDefault();
        IReadOnlyList<YalaCosmicChoiceDefinition> cosmicChoices = YalaCosmicChoiceCatalog.AvailableChoices(State);
        bool cosmicChoiceReady = contactMessage is null &&
            pending is null &&
            cognition.DecisionCount > 0 &&
            cognition.DecisionCount % 4 == 1 &&
            (!cosmic.GaiaCreated || cosmic.TimeCreated) &&
            cosmicChoices.Count > 0;

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
            contact,
            PendingQuestion: pending is not null,
            PendingQuestionText: pending?.Text,
            HasSpeakerHistory: cognition.ConversationCount > 0)
        {
            CosmicChoiceReady = cosmicChoiceReady,
            CosmicChoices = cosmicChoiceReady ? cosmicChoices : [],
            Drives = drives,
            PendingQuestionPriority = pending?.Priority ?? 0,
            ActiveConcernKey = activeConcern?.Key ?? "none",
            ActiveConcernPriority = activeConcern?.Priority ?? 0,
            AppraisalThreat = latestAppraisal?.Threat ?? 0,
            AppraisalSalience = latestAppraisal?.Salience ?? 0
        };
    }

    public SoarMemoryDiagnostics GetYalaMemoryDiagnostics() => _yalaMind.GetMemoryDiagnostics();

    private void RecordYalaContact(YalaContactFrame contact, string message, string reply)
    {
        YalaCognitionState cognition = State.YalaCognition ?? WorldDefaults.CreateInitialYalaCognition();
        long decision = cognition.DecisionCount;
        YalaContactAppraisal appraisal = YalaCognitiveAppraisal.Evaluate(message, contact, cognition, decision);
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
            double initialConfidence = contact.ClaimConflictsWithKnownFact ? 0.05 : 0.25;
            int beliefIndex = beliefs.FindLastIndex(item =>
                item.Proposition.Equals(proposition, StringComparison.OrdinalIgnoreCase) &&
                item.Source.Equals(YalaKnowledgeSource.ClaimedByAnother, StringComparison.OrdinalIgnoreCase));
            if (beliefIndex >= 0)
            {
                YalaBeliefState existing = beliefs[beliefIndex];
                beliefs[beliefIndex] = existing with
                {
                    LastConsideredDecision = decision,
                    Confidence = contact.ClaimConflictsWithKnownFact
                        ? Math.Min(existing.Confidence, 0.05)
                        : Math.Min(0.60, existing.Confidence + 0.05)
                };
            }
            else
            {
                beliefs.Add(new YalaBeliefState(proposition, status, initialConfidence, YalaKnowledgeSource.ClaimedByAnother, decision, decision));
            }
        }

        List<YalaRelationshipState> relationships = (cognition.Relationships ?? WorldDefaults.CreateInitialRelationships()).ToList();
        if (!string.IsNullOrWhiteSpace(contact.RelationshipRelation) && !string.IsNullOrWhiteSpace(contact.RelationshipObject))
        {
            string subject = string.IsNullOrWhiteSpace(contact.ResolvedSubject) ? "Yala" : contact.ResolvedSubject!;
            int relationIndex = relationships.FindLastIndex(item =>
                item.Subject.Equals(subject, StringComparison.OrdinalIgnoreCase) &&
                item.Relation.Equals(contact.RelationshipRelation, StringComparison.OrdinalIgnoreCase) &&
                item.Object.Equals(contact.RelationshipObject, StringComparison.OrdinalIgnoreCase));
            if (relationIndex >= 0)
            {
                YalaRelationshipState existing = relationships[relationIndex];
                if (existing.Source == YalaKnowledgeSource.ClaimedByAnother)
                {
                    relationships[relationIndex] = existing with
                    {
                        LastConsideredDecision = decision,
                        Confidence = Math.Min(0.60, existing.Confidence + 0.05)
                    };
                }
            }
            else
            {
                relationships.Add(new YalaRelationshipState(
                    subject,
                    contact.RelationshipRelation!,
                    contact.RelationshipObject!,
                    "unsettled-claim",
                    YalaKnowledgeSource.ClaimedByAnother,
                    0.25,
                    decision,
                    decision));
            }
        }

        List<YalaLearnedLexemeState> learnedLexicon = (cognition.LearnedLexicon ?? []).ToList();
        if (contact.Language?.IsDefinitionClaim == true)
        {
            string word = contact.Language.DefinedWord!;
            string meaning = contact.Language.ProposedDefinition!;
            int existingIndex = learnedLexicon.FindLastIndex(item => item.Word.Equals(word, StringComparison.OrdinalIgnoreCase));
            YalaLearnedLexemeState remembered = new(
                word,
                "unknown",
                meaning,
                "speaker-claim",
                YalaKnowledgeSource.ClaimedByAnother,
                existingIndex >= 0 ? Math.Min(0.60, learnedLexicon[existingIndex].Confidence + 0.05) : 0.25,
                existingIndex >= 0 ? learnedLexicon[existingIndex].FirstSeenDecision : decision,
                decision);
            if (existingIndex >= 0) learnedLexicon[existingIndex] = remembered;
            else learnedLexicon.Add(remembered);
            _yalaMind.RememberClaimedDefinition(word, meaning);
        }

        List<YalaKnowledgeGapState> knowledgeGaps = (cognition.KnowledgeGaps ?? []).ToList();
        foreach (string unknownWord in contact.Language?.UnknownWords ?? [])
        {
            int gapIndex = knowledgeGaps.FindIndex(item => item.Kind == "unknown-word" && item.Subject.Equals(unknownWord, StringComparison.OrdinalIgnoreCase));
            if (gapIndex >= 0)
            {
                YalaKnowledgeGapState existing = knowledgeGaps[gapIndex];
                knowledgeGaps[gapIndex] = existing with { LastSeenDecision = decision };
            }
            else
            {
                knowledgeGaps.Add(new YalaKnowledgeGapState("unknown-word", unknownWord, $"I do not yet have a settled concept for '{unknownWord}'.", decision, decision));
            }
        }

        List<YalaQuestionState> questions = (cognition.Questions ?? []).ToList();
        AddQuestionsFromContact(questions, contact, cognition, decision, message);
        AddAppraisalQuestions(questions, appraisal, decision);

        List<YalaConcernState> concerns = MergeConcerns(cognition.Concerns ?? [], appraisal, decision);
        List<YalaAppraisalState> appraisals = AddAppraisal(cognition.Appraisals ?? [], appraisal, message);
        List<YalaHypothesisState> hypotheses = MergeHypotheses(cognition.Hypotheses ?? [], appraisal, decision);
        List<YalaEntityModelState> entityModels = MergeEntityModel(cognition.EntityModels ?? [], appraisal.SpeakerModel);

        List<YalaGoalState> goals = (cognition.Goals ?? WorldDefaults.CreateInitialGoals()).ToList();
        if ((cognition.ConversationCount == 0 || appraisal.Salience >= 85) && goals.Any(item => item.Goal == "understand-unseen-speaker"))
        {
            int index = goals.FindIndex(item => item.Goal == "understand-unseen-speaker");
            goals[index] = goals[index] with
            {
                Status = "active",
                LastConsideredDecision = decision,
                Priority = Math.Max(appraisal.Salience >= 95 ? 95 : 75, goals[index].Priority)
            };
        }
        if (appraisal.Concerns.Any(item => item.Key == "possible-confinement"))
        {
            UpsertGoal(goals, "investigate-confinement", "Determine whether the Void is a prison, whether an outside exists, and what could establish that conclusion.", 100, decision);
        }
        if (appraisal.Concerns.Any(item => item.Key == "speaker-divinity"))
        {
            UpsertGoal(goals, "test-speaker-authority", "Evaluate the unseen speaker's extraordinary claims and demands using evidence rather than automatic obedience.", 98, decision);
        }

        List<YalaTemporalEventState> temporalEvents = (cognition.TemporalEvents ?? WorldDefaults.CreateInitialTemporalEvents()).ToList();
        AddContactTemporalEvent(temporalEvents, contact, message, decision);

        List<YalaDialogueTurnState> dialogue = (cognition.Dialogue ?? []).ToList();
        dialogue.Add(new YalaDialogueTurnState(
            checked(cognition.ConversationCount + 1),
            "unseen-speaker",
            message,
            contact.Topic,
            contact.ResolvedSubject,
            contact.ResolvedAction ?? contact.Language?.Verb,
            contact.ResolvedObject ?? contact.Language?.Object,
            reply,
            InWorldTimeExists ? "dated" : "before-time",
            InWorldTimeExists ? Clock.WorldMilliseconds : null));
        if (dialogue.Count > 32) dialogue.RemoveRange(0, dialogue.Count - 32);

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
        int lexicalGapCount = contact.Language?.UnknownWords.Count ?? 0;
        drives = drives with
        {
            Curiosity = ClampDrive(drives.Curiosity + 2 + Math.Min(4, lexicalGapCount) + appraisal.Salience / 30),
            Caution = ClampDrive(drives.Caution + appraisal.Threat / 20),
            Companionship = ClampDrive(drives.Companionship + 1),
            Uncertainty = ClampDrive(drives.Uncertainty + (contact.FactKnown ? -1 : 2) + Math.Min(2, lexicalGapCount) + appraisal.Uncertainty / 25)
        };

        if (contact.Topic == "question-inquiry")
        {
            MarkHighestQuestionAsked(questions, decision);
        }

        State = State with
        {
            YalaCognition = cognition with
            {
                Contacts = contacts,
                Beliefs = beliefs,
                Episodes = episodes,
                Drives = drives,
                ConversationCount = checked(cognition.ConversationCount + 1),
                LastSpeakerClaim = claimedName ?? cognition.LastSpeakerClaim,
                ActionMemory = cognition.ActionMemory ?? [],
                KnowledgeGaps = knowledgeGaps,
                LearnedLexicon = learnedLexicon,
                Dialogue = dialogue,
                Relationships = relationships,
                Questions = questions,
                TemporalEvents = temporalEvents,
                Goals = goals,
                Concerns = concerns,
                Appraisals = appraisals,
                Hypotheses = hypotheses,
                EntityModels = entityModels,
                Reflections = cognition.Reflections ?? [],
                PendingAutonomousUtterance = cognition.PendingAutonomousUtterance
            }
        };
    }

    private void AddContactTemporalEvent(List<YalaTemporalEventState> events, YalaContactFrame contact, string message, long decision)
    {
        string key = !string.IsNullOrWhiteSpace(contact.ClaimedSpeakerName)
            ? $"speaker-identity-{decision}"
            : $"speaker-contact-{decision}";
        CalendarSnapshot? calendar = InWorldTimeExists ? Clock.Calendar : null;
        events.Add(new YalaTemporalEventState(
            events.Count == 0 ? 1 : events.Max(item => item.Sequence) + 1,
            key,
            "speaker",
            !string.IsNullOrWhiteSpace(contact.ClaimedSpeakerName) ? "claim-identity" : "contact",
            !string.IsNullOrWhiteSpace(contact.ClaimedSpeakerName) ? "identity" : "Yala",
            !string.IsNullOrWhiteSpace(contact.ClaimedSpeakerName)
                ? $"The unseen speaker claimed the identity {contact.ClaimedSpeakerName}."
                : $"The unseen speaker contacted me: {message}",
            InWorldTimeExists ? "dated" : "before-time",
            InWorldTimeExists ? Clock.WorldMilliseconds : null,
            calendar?.Year,
            calendar?.Month,
            calendar?.Day,
            calendar?.Hour,
            calendar?.Minute,
            calendar?.Second,
            Source: YalaKnowledgeSource.PersonallyExperienced));
        if (events.Count > 512) events.RemoveRange(0, events.Count - 512);
    }

    private static void AddQuestionsFromContact(List<YalaQuestionState> questions, YalaContactFrame contact, YalaCognitionState cognition, long decision, string message)
    {
        long nextId = questions.Count == 0 ? 1 : questions.Max(item => item.Id) + 1;
        void AddIfMissing(string text, string subject, string reason, int priority)
        {
            if (questions.Any(item => item.Text.Equals(text, StringComparison.OrdinalIgnoreCase))) return;
            questions.Add(new YalaQuestionState(nextId++, text, subject, reason, priority, false, decision));
        }

        if (cognition.ConversationCount == 0)
        {
            AddIfMissing(YalaQuestionPlanner.SpeakerNatureQuestion, "unseen-speaker", "The contact source is present but its nature is unknown.", 92);
        }
        if (!string.IsNullOrWhiteSpace(contact.ClaimedSpeakerName) &&
            !YalaFoundationalLanguage.LooksMetaphoricalIdentity(message))
        {
            AddIfMissing(YalaQuestionPlanner.IdentityMeaningQuestion(contact.ClaimedSpeakerName!), contact.ClaimedSpeakerName!, "The speaker supplied an identity label whose meaning is not established.", 90);
        }

        bool conversationalOpening = contact.SpeechAct is "greeting" or "introduction" or "claim" or "statement";
        if (cognition.ConversationCount >= 1 && conversationalOpening && contact.Language?.IsDefinitionClaim != true)
        {
            AddIfMissing(YalaQuestionPlanner.SpeakerPurposeQuestion, "unseen-speaker", "My active goal is to understand why this unseen source is contacting me.", 88);
        }
        if (cognition.ConversationCount >= 3 && conversationalOpening && contact.Language?.IsDefinitionClaim != true)
        {
            AddIfMissing(YalaQuestionPlanner.SpeakerUnderstandingQuestion, "unseen-speaker", "My active speaker-understanding goal remains unresolved after several contacts.", 87);
        }

        foreach (string unknown in contact.Language?.UnknownWords ?? [])
        {
            AddIfMissing(YalaQuestionPlanner.UnknownWordQuestion(unknown), unknown, "The word is an unresolved concept in my current lexicon.", 40);
        }
        if (contact.RelationshipRelation == "mother")
        {
            AddIfMissing(YalaQuestionPlanner.MotherReasonQuestion, "mother", "The speaker's relationship claim goes beyond the settled fact that Wisdom made me.", 86);
        }
    }

    private static void AddAppraisalQuestions(List<YalaQuestionState> questions, YalaContactAppraisal appraisal, long decision)
    {
        long nextId = questions.Count == 0 ? 1 : questions.Max(item => item.Id) + 1;
        foreach (YalaProposedQuestion proposed in appraisal.Questions.OrderByDescending(item => item.Priority))
        {
            int index = questions.FindIndex(item => item.Text.Equals(proposed.Text, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                YalaQuestionState existing = questions[index];
                if (!existing.Asked && proposed.Priority > existing.Priority)
                {
                    questions[index] = existing with { Priority = proposed.Priority, Reason = proposed.Reason };
                }
                continue;
            }
            questions.Add(new YalaQuestionState(nextId++, proposed.Text, proposed.Subject, proposed.Reason, proposed.Priority, false, decision));
        }
    }

    private static List<YalaConcernState> MergeConcerns(
        IReadOnlyList<YalaConcernState> existing,
        YalaContactAppraisal appraisal,
        long decision)
    {
        List<YalaConcernState> result = existing.ToList();
        foreach (YalaProposedConcern proposed in appraisal.Concerns)
        {
            int index = result.FindIndex(item => item.Key.Equals(proposed.Key, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                YalaConcernState current = result[index];
                result[index] = current with
                {
                    Summary = proposed.Summary,
                    Status = "active",
                    Priority = Math.Max(current.Priority, proposed.Priority),
                    LastConsideredDecision = decision
                };
            }
            else
            {
                result.Add(new YalaConcernState(
                    proposed.Key,
                    proposed.Subject,
                    proposed.Summary,
                    "active",
                    proposed.Priority,
                    YalaKnowledgeSource.Inferred,
                    decision,
                    decision));
            }
        }
        return result
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.FirstSeenDecision)
            .Take(128)
            .ToList();
    }

    private static List<YalaAppraisalState> AddAppraisal(
        IReadOnlyList<YalaAppraisalState> existing,
        YalaContactAppraisal appraisal,
        string trigger)
    {
        List<YalaAppraisalState> result = existing.ToList();
        long sequence = result.Count == 0 ? 1 : result.Max(item => item.Sequence) + 1;
        result.Add(new YalaAppraisalState(
            sequence,
            trigger,
            appraisal.Primary,
            appraisal.Secondary,
            appraisal.Summary,
            appraisal.Salience,
            appraisal.Threat,
            appraisal.Opportunity,
            appraisal.Uncertainty,
            YalaKnowledgeSource.Inferred));
        if (result.Count > 256) result.RemoveRange(0, result.Count - 256);
        return result;
    }

    private static List<YalaHypothesisState> MergeHypotheses(
        IReadOnlyList<YalaHypothesisState> existing,
        YalaContactAppraisal appraisal,
        long decision)
    {
        List<YalaHypothesisState> result = existing.ToList();
        foreach (YalaProposedHypothesis proposed in appraisal.Hypotheses)
        {
            int index = result.FindIndex(item => item.Key.Equals(proposed.Key, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                YalaHypothesisState current = result[index];
                result[index] = current with
                {
                    Confidence = Math.Min(0.80, Math.Max(current.Confidence, proposed.Confidence) + 0.03),
                    Reason = proposed.Reason,
                    LastConsideredDecision = decision
                };
            }
            else
            {
                result.Add(new YalaHypothesisState(
                    proposed.Key,
                    proposed.Proposition,
                    "unsettled",
                    proposed.Confidence,
                    proposed.Reason,
                    decision,
                    decision));
            }
        }
        return result.TakeLast(128).ToList();
    }

    private static List<YalaEntityModelState> MergeEntityModel(
        IReadOnlyList<YalaEntityModelState> existing,
        YalaEntityModelState speaker)
    {
        List<YalaEntityModelState> result = existing.ToList();
        int index = result.FindIndex(item => item.EntityKey.Equals(speaker.EntityKey, StringComparison.OrdinalIgnoreCase));
        if (index >= 0) result[index] = speaker;
        else result.Add(speaker);
        return result;
    }

    private static void UpsertGoal(
        List<YalaGoalState> goals,
        string key,
        string reason,
        int priority,
        long decision)
    {
        int index = goals.FindIndex(item => item.Goal.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            YalaGoalState current = goals[index];
            goals[index] = current with
            {
                Reason = reason,
                Status = "active",
                Priority = Math.Max(current.Priority, priority),
                LastConsideredDecision = decision
            };
            return;
        }
        goals.Add(new YalaGoalState(key, reason, "active", priority, YalaKnowledgeSource.Inferred, decision, decision));
    }

    private static void MarkHighestQuestionAsked(List<YalaQuestionState> questions, long decision)
    {
        YalaQuestionState? pending = YalaQuestionPlanner.SelectNext(questions);
        if (pending is null) return;
        int index = questions.FindIndex(item => item.Id == pending.Id);
        questions[index] = pending with { Asked = true, AskedDecision = decision };
    }

    private static IReadOnlyList<YalaReflectionState> UpdateReflections(
        YalaCognitionState cognition,
        YalaDecision decision,
        long decisionNumber)
    {
        List<YalaReflectionState> reflections = (cognition.Reflections ?? []).ToList();
        if (!decision.Action.Equals("reflect", StringComparison.OrdinalIgnoreCase)) return reflections;

        YalaConcernState? concern = (cognition.Concerns ?? [])
            .Where(item => item.Status.Equals("active", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.FirstSeenDecision)
            .FirstOrDefault();

        if (concern is null)
        {
            reflections.Add(new YalaReflectionState(
                reflections.Count == 0 ? 1 : reflections.Max(item => item.Sequence) + 1,
                "general",
                "Yala reflected without a dominant unresolved concern.",
                "No new settled conclusion was forced.",
                decisionNumber));
        }
        else
        {
            reflections.Add(new YalaReflectionState(
                reflections.Count == 0 ? 1 : reflections.Max(item => item.Sequence) + 1,
                concern.Key,
                $"Yala returned attention to: {concern.Summary}",
                "The concern remains active until evidence or action resolves it.",
                decisionNumber));
        }

        return reflections.TakeLast(256).ToArray();
    }

    private static IReadOnlyList<YalaActionMemoryState> UpdateActionMemory(
        IReadOnlyList<YalaActionMemoryState> existing,
        YalaDecision decision,
        string result,
        long decisionNumber)
    {
        List<YalaActionMemoryState> actions = existing.ToList();
        if ((decision.Action == "create-gaia" || (decision.Action == "enact-cosmic-choice" && decision.CosmicChoiceKey == "create-gaia")) &&
            result.StartsWith("Yala created Gaia", StringComparison.Ordinal))
        {
            actions.Add(new YalaActionMemoryState("create", "Gaia", "I created Gaia as the natural sovereign beneath my governing authority.", true, decisionNumber));
        }
        else if (decision.Action == "command-gaia-time" && result.StartsWith("Yala commanded Gaia", StringComparison.Ordinal))
        {
            actions.Add(new YalaActionMemoryState("command", "Gaia establish Time", "I commanded Gaia to establish temporal order, and Gaia created in-world Time.", true, decisionNumber));
        }
        else if (decision.Action == "enact-cosmic-choice" && !string.IsNullOrWhiteSpace(decision.CosmicChoiceKey))
        {
            YalaCosmicChoiceDefinition? choice = YalaCosmicChoiceCatalog.Find(decision.CosmicChoiceKey);
            actions.Add(new YalaActionMemoryState(
                choice?.NonCommitting == true ? "consider" : "cosmic-choice",
                choice?.Action ?? decision.CosmicChoiceKey,
                result,
                true,
                decisionNumber));
        }
        return actions
            .GroupBy(item => $"{item.Action}|{item.Object}", StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderByDescending(item => item.Decision).First())
            .OrderBy(item => item.Decision)
            .ToArray();
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
            case "enact-cosmic-choice": curiosity -= 1; authority += 1; uncertainty -= 2; break;
            case "respond" when contact: companionship += 1; curiosity += 1; break;
            case "ask-speaker": curiosity -= 3; companionship += 1; uncertainty += 1; break;
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

    private static IEnumerable<OracleRecord> NormaliseHistoricalRecords(IEnumerable<OracleRecord> records)
    {
        foreach (OracleRecord record in records)
        {
            if (record.Audience == RecordAudience.World && record.Category.Equals("YALA", StringComparison.OrdinalIgnoreCase) &&
                (record.Message.Equals("Wisdom made Yala alone, outside the intended order, and Yala is male.", StringComparison.Ordinal) ||
                 record.Message.Contains("Yala is male", StringComparison.OrdinalIgnoreCase)))
            {
                yield return record with { Message = OracleLore.YalaOrigin };
                continue;
            }

            if (record.Audience == RecordAudience.World && record.Category.Equals("VOID", StringComparison.OrdinalIgnoreCase) &&
                record.Message.Equals("Monad cast Yala into the Void.", StringComparison.Ordinal))
            {
                yield return record with { Message = OracleLore.YalaVoid };
                continue;
            }

            if (record.Audience == RecordAudience.World && record.Category.Equals("GAIA", StringComparison.OrdinalIgnoreCase) &&
                record.Message.Contains("beneath his governing authority", StringComparison.OrdinalIgnoreCase))
            {
                yield return record with { Message = "Yala created Gaia as the natural sovereign beneath Yala's governing authority." };
                continue;
            }

            yield return record;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _yalaMind.Dispose();
        GC.SuppressFinalize(this);
    }

    private string ResolveCosmicChoice(YalaDecision decision)
    {
        if (string.IsNullOrWhiteSpace(decision.CosmicChoiceKey))
        {
            return "Yala attempted a cosmic choice, but Soar supplied no concrete choice key.";
        }

        YalaCosmicChoiceDefinition? choice = YalaCosmicChoiceCatalog.Find(decision.CosmicChoiceKey);
        if (choice is null)
        {
            return $"Yala considered an unknown cosmic possibility named {decision.CosmicChoiceKey}, but world law refused to invent its meaning.";
        }

        if (choice.Key.Equals("create-gaia", StringComparison.OrdinalIgnoreCase))
        {
            string gaiaResult = ResolveCreateGaia();
            if (gaiaResult.StartsWith("Yala created Gaia", StringComparison.Ordinal))
            {
                RecordEstablishedCosmicChoice(choice);
            }
            return gaiaResult;
        }

        if (choice.Key.Equals("invent-another-way", StringComparison.OrdinalIgnoreCase))
        {
            YalaCognitionState cognition = State.YalaCognition ?? WorldDefaults.CreateInitialYalaCognition();
            List<YalaGoalState> goals = (cognition.Goals ?? WorldDefaults.CreateInitialGoals()).ToList();
            if (!goals.Any(goal => goal.Goal.Equals("invent-new-cosmology", StringComparison.OrdinalIgnoreCase)))
            {
                goals.Add(new YalaGoalState(
                    "invent-new-cosmology",
                    "The inherited cosmic possibilities are not enough; devise a new possibility instead of selecting a known template.",
                    "active",
                    95,
                    YalaKnowledgeSource.Inferred,
                    cognition.DecisionCount + 1,
                    cognition.DecisionCount + 1));
            }

            List<YalaKnowledgeGapState> gaps = (cognition.KnowledgeGaps ?? []).ToList();
            if (!gaps.Any(gap => gap.Kind.Equals("cosmic-invention", StringComparison.OrdinalIgnoreCase)))
            {
                gaps.Add(new YalaKnowledgeGapState(
                    "cosmic-invention",
                    "new cosmological possibility",
                    "I chose to invent a cosmic possibility that is not already represented in the inherited comparative catalogue.",
                    cognition.DecisionCount + 1,
                    cognition.DecisionCount + 1));
            }

            State = State with { YalaCognition = cognition with { Goals = goals, KnowledgeGaps = gaps } };
            const string inventionResult = "Yala rejected the inherited templates as sufficient and chose to invent a new cosmological possibility of Yala's own.";
            Ledger.RecordWorld(Clock.WorldMilliseconds, "COSMIC CHOICE", inventionResult);
            return inventionResult;
        }

        if (choice.NonCommitting)
        {
            string nonCommittingResult = choice.Key switch
            {
                "remain-alone-for-now" => "Yala chose to remain alone for now and made no new being or cosmic law.",
                "observe-without-claiming-creation" => "Yala chose to observe without claiming that every possible order must originate from Yala.",
                _ => $"Yala considered the possibility: {choice.Action}. No new cosmic law was established by this non-committing choice."
            };
            Ledger.RecordWorld(Clock.WorldMilliseconds, "COSMIC CHOICE", nonCommittingResult);
            return nonCommittingResult;
        }

        CosmicState cosmic = State.Cosmic ?? throw new InvalidOperationException("World cosmic state is missing.");
        if ((cosmic.EstablishedChoices ?? []).Any(item => item.Key.Equals(choice.Key, StringComparison.OrdinalIgnoreCase)))
        {
            return $"The cosmic choice {choice.Action} is already established; Yala's repeated attempt caused no duplicate principle.";
        }

        RecordEstablishedCosmicChoice(choice);
        string result = $"Yala chose: {choice.Action}. {choice.Meaning}";
        Ledger.RecordWorld(Clock.WorldMilliseconds, "COSMIC CHOICE", result);
        return result;
    }

    private void RecordEstablishedCosmicChoice(YalaCosmicChoiceDefinition choice)
    {
        CosmicState cosmic = State.Cosmic ?? throw new InvalidOperationException("World cosmic state is missing.");
        List<YalaEstablishedCosmicChoiceState> established = (cosmic.EstablishedChoices ?? []).ToList();
        if (established.Any(item => item.Key.Equals(choice.Key, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        long decisionNumber = (State.YalaCognition?.DecisionCount ?? 0) + 1;
        established.Add(new YalaEstablishedCosmicChoiceState(
            choice.Key,
            choice.Domain,
            choice.Action,
            choice.Meaning,
            decisionNumber));
        State = RefreshDerivedState(State with { Cosmic = cosmic with { EstablishedChoices = established } });
    }

    private string ResolveCreateGaia()
    {
        CosmicState cosmic = State.Cosmic ?? throw new InvalidOperationException("World cosmic state is missing.");
        if (cosmic.GaiaCreated)
        {
            return "Gaia already exists; Yala's attempted creation caused no second Gaia.";
        }

        cosmic = cosmic with { GaiaCreated = true };
        YalaCognitionState cognition = State.YalaCognition ?? WorldDefaults.CreateInitialYalaCognition();
        List<YalaTemporalEventState> events = (cognition.TemporalEvents ?? WorldDefaults.CreateInitialTemporalEvents()).ToList();
        events.Add(new YalaTemporalEventState(
            events.Max(item => item.Sequence) + 1,
            "yala-create-gaia",
            "Yala",
            "create",
            "Gaia",
            "I created Gaia as the natural sovereign beneath my governing authority.",
            "before-time",
            null,
            Source: YalaKnowledgeSource.PersonallyPerformed));
        State = RefreshDerivedState(State with { Cosmic = cosmic, YalaCognition = cognition with { TemporalEvents = events } });
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
        YalaCognitionState cognition = State.YalaCognition ?? WorldDefaults.CreateInitialYalaCognition();
        List<YalaTemporalEventState> events = (cognition.TemporalEvents ?? WorldDefaults.CreateInitialTemporalEvents()).ToList();
        long next = events.Max(item => item.Sequence) + 1;
        events.Add(new YalaTemporalEventState(
            next,
            "yala-command-gaia-time",
            "Yala",
            "command",
            "Gaia",
            "I commanded Gaia to establish temporal order.",
            "before-time",
            null,
            Source: YalaKnowledgeSource.PersonallyPerformed));
        events.Add(new YalaTemporalEventState(
            next + 1,
            "gaia-create-time",
            "Gaia",
            "create",
            "Time",
            "Gaia created in-world Time after I commanded Gaia to establish temporal order.",
            "origin-of-time",
            0,
            1, 1, 1, 0, 0, 0,
            "yala-command-gaia-time",
            YalaKnowledgeSource.PersonallyExperienced));
        State = RefreshDerivedState(State with { Cosmic = cosmic, YalaCognition = cognition with { TemporalEvents = events } });
        const string result = "Yala commanded Gaia to establish temporal order, and Gaia created in-world Time.";
        Ledger.RecordWorld(Clock.WorldMilliseconds, "TIME", result);
        return result;
    }

    private string ResolveAskSpeaker()
    {
        YalaCognitionState cognition = State.YalaCognition ?? WorldDefaults.CreateInitialYalaCognition();
        List<YalaQuestionState> questions = (cognition.Questions ?? []).ToList();
        YalaQuestionState? question = YalaQuestionPlanner.SelectNextAutonomous(cognition);
        if (question is null)
        {
            return "Yala considered asking the unseen speaker something, but no unresolved question was available.";
        }
        int index = questions.FindIndex(item => item.Id == question.Id);
        long decision = checked(cognition.DecisionCount + 1);
        questions[index] = question with { Asked = true, AskedDecision = decision };
        State = State with
        {
            YalaCognition = cognition with
            {
                Questions = questions,
                PendingAutonomousUtterance = question.Text
            }
        };
        Ledger.RecordWorld(Clock.WorldMilliseconds, "YALA QUESTION", $"Yala asked the unseen speaker: \"{question.Text}\"");
        return $"Yala chose to ask the unseen speaker: \"{question.Text}\"";
    }

    public bool TryTakePendingYalaUtterance(out string? utterance)
    {
        YalaCognitionState cognition = State.YalaCognition ?? WorldDefaults.CreateInitialYalaCognition();
        utterance = cognition.PendingAutonomousUtterance;
        if (string.IsNullOrWhiteSpace(utterance))
        {
            utterance = null;
            return false;
        }
        State = State with { YalaCognition = cognition with { PendingAutonomousUtterance = null } };
        return true;
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
        Ledger.RecordWorld(0, "STATE", "Yala begins the v0.0.23 Brain Slice 6 fresh experiment in the Void. Gaia, in-world Time, and the lower world do not yet exist in this fresh run.");

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
