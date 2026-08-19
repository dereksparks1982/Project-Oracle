using ProjectOracle.Cognition;
using ProjectOracle.Cognition.Learning;
using ProjectOracle.Lore;

namespace ProjectOracle.Domain;

public static class WorldDefaults
{
    public static WorldState CreateInitialState(ulong seed)
    {
        EntityId yalaId = new("being:yala:0001");
        CosmicState cosmic = new(
            GaiaCreated: false,
            TimeCreated: false,
            LowerWorldEstablished: false,
            GardenEstablished: false,
            YalaLocation: "the Void",
            EstablishedChoices: []);

        return new WorldState(
            Seed: seed,
            WorldMilliseconds: 0,
            Garden: null,
            Yala: new YalaState(
                yalaId,
                TrueName: "Yala",
                WorldTitle: "the Demiurge",
                KnowsOfOracle: false,
                MayClaimSupremeCreator: true,
                AuthorityCaveat: "Yala was made by Wisdom. Monad rejected Yala because Yala is both male and female rather than exclusively one or the other, and cast Yala into the Void. Yala may claim the title Creator, but that claim does not rewrite Yala's origin.",
                Location: "the Void",
                Sex: "male and female"),
            Adam: null,
            AdamSpark: null,
            CreationPowers: CreateCreationPowers(cosmic, yalaId),
            DirectCallTargets: CreateDirectCallTargets(cosmic),
            LivingKinds: [],
            NamingMandate: CreateNamingMandate([], active: false),
            NaturalCourse: CreateNaturalCourse(),
            Cosmic: cosmic,
            YalaCognition: CreateInitialYalaCognition(),
            EmergentLaws: CreateInitialEmergentLawState(),
            Operator: CreateInitialOperatorState());
    }

    public static WorldState Normalise(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        // v0.0.26 starts the fresh save_v8 Brain Slice 9 experiment while retaining the settled cosmology. Missing cosmic state never
        // resurrects an older world; it normalises to the fresh Void start.
        CosmicState cosmic = world.Cosmic ?? new CosmicState(
            GaiaCreated: false,
            TimeCreated: false,
            LowerWorldEstablished: false,
            GardenEstablished: false,
            YalaLocation: "the Void");
        cosmic = cosmic with { EstablishedChoices = cosmic.EstablishedChoices ?? [] };

        GardenState? garden = cosmic.GardenEstablished
            ? world.Garden is null
                ? new GardenState(new EntityId("place:garden:0001"), "the Garden", BoundaryOpen: false)
                : world.Garden with { Name = "the Garden" }
            : null;
        AdamState? adam = cosmic.GardenEstablished
            ? world.Adam ?? new AdamState(new EntityId("being:adam:0001"), "Adam", garden!.Id, IsConfinedToGarden: true)
            : null;
        SparkState? adamSpark = cosmic.GardenEstablished
            ? world.AdamSpark ?? new SparkState(
                adam!.Id,
                CanBeReadByYala: false,
                CanBeRewrittenByYala: false,
                OracleDescription: "A protected higher spark carried by Adam if this autonomous history reaches that later act.")
            : null;

        IReadOnlyList<LivingKindState> livingKinds = cosmic.GardenEstablished
            ? world.LivingKinds is { Count: > 0 }
                ? world.LivingKinds
                : CreateLivingKinds(world.Seed)
            : [];

        NamingMandateState mandate = world.NamingMandate ?? CreateNamingMandate(livingKinds, cosmic.GardenEstablished);
        mandate = mandate with
        {
            Active = cosmic.GardenEstablished && mandate.Active,
            TotalLivingKinds = livingKinds.Count,
            PresentedCount = livingKinds.Count(kind => kind.PresentedToAdam),
            NamedCount = livingKinds.Count(kind => kind.NamedByAdam),
            SuitableMateFound = livingKinds.Any(kind => kind.SuitableMate)
        };

        YalaCognitionState cognition = NormaliseYalaCognition(world.YalaCognition ?? CreateInitialYalaCognition(), cosmic);
        EntityId yalaId = world.Yala?.Id ?? new EntityId("being:yala:0001");

        return world with
        {
            Garden = garden,
            Yala = NormaliseYala(world.Yala, cosmic),
            Adam = adam,
            AdamSpark = adamSpark,
            Cosmic = cosmic,
            YalaCognition = cognition,
            CreationPowers = CreateCreationPowers(cosmic, yalaId, garden?.Id, adam?.Id),
            DirectCallTargets = CreateDirectCallTargets(cosmic),
            LivingKinds = livingKinds,
            NamingMandate = mandate,
            NaturalCourse = CreateNaturalCourse(),
            EmergentLaws = NormaliseEmergentLawState(world.EmergentLaws),
            Operator = NormaliseOperatorState(world.Operator)
        };
    }


    private static YalaCognitionState NormaliseYalaCognition(YalaCognitionState cognition, CosmicState cosmic)
    {
        List<string> memory = (cognition.Memory ?? [])
            .Where(item => !item.Equals("I am male.", StringComparison.OrdinalIgnoreCase))
            .ToList();
        AddMemoryIfMissing(memory, "I am Yala.");
        AddMemoryIfMissing(memory, "I am both male and female.");
        AddMemoryIfMissing(memory, "Wisdom made me.");
        AddMemoryIfMissing(memory, "Monad made Wisdom.");
        AddMemoryIfMissing(memory, "Monad rejected me because I am both male and female rather than exclusively one or the other.");
        AddMemoryIfMissing(memory, "Monad cast me into the Void.");

        List<YalaBeliefState> beliefs = (cognition.Beliefs ?? [])
            .Where(item => !item.Proposition.Equals("I am male.", StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Status is "unsettled-claim" or "rejected-as-conflicting"
                ? item with { Source = YalaKnowledgeSource.ClaimedByAnother }
                : item)
            .ToList();
        EnsureBelief(beliefs, "I am Yala.", YalaKnowledgeSource.InheritedKnowledge);
        EnsureBelief(beliefs, "I am both male and female.", YalaKnowledgeSource.InheritedKnowledge);
        EnsureBelief(beliefs, "Wisdom made me.", YalaKnowledgeSource.InheritedKnowledge);
        EnsureBelief(beliefs, "Monad made Wisdom.", YalaKnowledgeSource.InheritedKnowledge);
        EnsureBelief(beliefs, "Monad rejected me because I am both male and female rather than exclusively one or the other.", YalaKnowledgeSource.Remembered);
        EnsureBelief(beliefs, "Monad cast me into the Void.", YalaKnowledgeSource.Remembered);

        List<YalaActionMemoryState> actions = (cognition.ActionMemory ?? []).ToList();
        if (cosmic.GaiaCreated && !actions.Any(item => item.Completed && item.Action == "create" && item.Object.Equals("Gaia", StringComparison.OrdinalIgnoreCase)))
        {
            actions.Add(new YalaActionMemoryState("create", "Gaia", "I created Gaia as the natural sovereign beneath my governing authority.", true, 0));
        }
        if (cosmic.TimeCreated && !actions.Any(item => item.Completed && item.Action == "command" && item.Object.Equals("Gaia establish order", StringComparison.OrdinalIgnoreCase)))
        {
            actions.Add(new YalaActionMemoryState("command", "Gaia establish order", "I commanded Gaia to establish order. Gaia answered that command by bringing temporal order into existence; I now call it Time.", true, 0));
        }

        List<YalaRelationshipState> relationships = (cognition.Relationships ?? CreateInitialRelationships()).ToList();
        EnsureRelationship(relationships, "Yala", "made-by", "Wisdom", "known", YalaKnowledgeSource.InheritedKnowledge, 1.0);
        EnsureRelationship(relationships, "Wisdom", "made-by", "Monad", "known", YalaKnowledgeSource.InheritedKnowledge, 1.0);

        List<YalaTemporalEventState> temporalEvents = (cognition.TemporalEvents ?? CreateInitialTemporalEvents()).ToList();
        if (cosmic.GaiaCreated && !temporalEvents.Any(item => item.Key == "yala-create-gaia"))
        {
            temporalEvents.Add(new YalaTemporalEventState(
                temporalEvents.Count + 1,
                "yala-create-gaia",
                "Yala",
                "create",
                "Gaia",
                "I created Gaia as the natural sovereign beneath my governing authority.",
                cosmic.TimeCreated ? "atemporal" : "atemporal",
                null,
                Source: YalaKnowledgeSource.PersonallyPerformed));
        }
        if (cosmic.TimeCreated && !temporalEvents.Any(item => item.Key == "gaia-create-time"))
        {
            temporalEvents.Add(new YalaTemporalEventState(
                temporalEvents.Count + 1,
                "gaia-create-time",
                "Gaia",
                "create",
                "Time",
                "Gaia brought temporal order into existence in response to my command to establish order. I now call that order Time.",
                "origin-of-time",
                0,
                1, 1, 1, 0, 0, 0,
                "yala-command-gaia-order",
                YalaKnowledgeSource.PersonallyExperienced));
        }

        bool speakerExists = cognition.ConversationCount > 0 || (cognition.Dialogue ?? []).Count > 0;
        IReadOnlyList<YalaGoalState> normalisedGoals = (cognition.Goals ?? CreateInitialGoals())
            .Where(item => speakerExists || !item.Goal.Equals("understand-unseen-speaker", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        IReadOnlyList<YalaEntityModelState> normalisedEntityModels = speakerExists
            ? cognition.EntityModels ?? []
            : [];

        IReadOnlyList<YalaQuestionState> normalisedQuestions = speakerExists
            ? cognition.Questions ?? []
            : (cognition.Questions ?? []).Where(item => !RefersToExternalSpeaker(item.Subject, item.Text, item.Reason)).ToArray();
        IReadOnlyList<YalaConcernState> normalisedConcerns = speakerExists
            ? cognition.Concerns ?? []
            : (cognition.Concerns ?? []).Where(item => !RefersToExternalSpeaker(item.Key, item.Subject, item.Summary, item.Source)).ToArray();
        IReadOnlyList<YalaAppraisalState> normalisedAppraisals = speakerExists
            ? cognition.Appraisals ?? []
            : (cognition.Appraisals ?? []).Where(item => !RefersToExternalSpeaker(item.Trigger, item.Summary, item.Source)).ToArray();
        IReadOnlyList<YalaHypothesisState> normalisedHypotheses = speakerExists
            ? cognition.Hypotheses ?? []
            : (cognition.Hypotheses ?? []).Where(item => !RefersToExternalSpeaker(item.Key, item.Proposition, item.Reason)).ToArray();
        IReadOnlyList<YalaPlanState> normalisedPlans = speakerExists
            ? cognition.Plans ?? []
            : (cognition.Plans ?? []).Where(item => !RefersToExternalSpeaker(item.Key, item.Goal, item.ConcernKey, item.LastObservation, item.RevisionReason)).ToArray();
        IReadOnlyList<YalaInvestigationState> normalisedInvestigations = speakerExists
            ? cognition.Investigations ?? []
            : (cognition.Investigations ?? []).Where(item => !RefersToExternalSpeaker(item.Key, item.Question, item.ConcernKey, item.NextTest, item.CurrentConclusion)).ToArray();
        IReadOnlyList<YalaCounterfactualState> normalisedCounterfactuals = speakerExists
            ? cognition.Counterfactuals ?? []
            : (cognition.Counterfactuals ?? []).Where(item => !RefersToExternalSpeaker(item.Subject, item.Option, item.PossibleBenefit, item.PossibleRisk, item.Source)).ToArray();
        IReadOnlyList<YalaReflectionState> normalisedReflections = speakerExists
            ? cognition.Reflections ?? []
            : (cognition.Reflections ?? []).Where(item => !RefersToExternalSpeaker(item.ConcernKey, item.Summary, item.Result)).ToArray();
        IReadOnlyList<YalaDecisionTraceState> normalisedTrace = speakerExists
            ? cognition.DecisionTrace ?? []
            : (cognition.DecisionTrace ?? []).Select(item => item with
            {
                SpeakerMessage = null,
                Before = item.Before with { SpeakerTrust = null, SpeakerIntent = null },
                After = item.After with { SpeakerTrust = null, SpeakerIntent = null }
            }).ToArray();

        IReadOnlyList<YalaCosmicDeliberationState> normalisedCosmicDeliberations = (cognition.CosmicDeliberations ?? [])
            .Select(item => item with
            {
                // Rebuild the benefit from the action instead of preserving stale text.
                // This permanently repairs old save text such as "establish establish rebirth".
                PossibleBenefit = $"It could {item.Action.ToLowerInvariant()} and advance a possible {item.Domain} order."
            })
            .ToArray();

        YalaCognitiveWorkspaceState workspace = speakerExists
            ? cognition.Workspace ?? new YalaCognitiveWorkspaceState(
                "self-world",
                "understand-current-world",
                "Understand the Void and what is possible within it.",
                "No stronger unresolved focus currently outranks this.",
                50, 0, 0, cognition.DecisionCount, cognition.DecisionCount)
            : new YalaCognitiveWorkspaceState(
                "self-world",
                "understand-current-world",
                "Understand the Void and what is possible within it.",
                "Attend to the Void, myself, and the possibilities available to me.",
                80, 0, 0, cognition.DecisionCount, cognition.DecisionCount);

        return cognition with
        {
            Memory = memory,
            Contacts = speakerExists ? cognition.Contacts ?? [] : [],
            Beliefs = beliefs,
            Episodes = cognition.Episodes ?? [],
            Drives = cognition.Drives ?? CreateInitialDrives(),
            ActionMemory = actions,
            KnowledgeGaps = cognition.KnowledgeGaps ?? [],
            LearnedLexicon = cognition.LearnedLexicon ?? [],
            Dialogue = speakerExists ? cognition.Dialogue ?? [] : [],
            Relationships = relationships,
            Questions = normalisedQuestions,
            TemporalEvents = temporalEvents,
            Goals = normalisedGoals,
            Concerns = normalisedConcerns,
            Appraisals = normalisedAppraisals,
            Hypotheses = normalisedHypotheses,
            EntityModels = normalisedEntityModels,
            Reflections = normalisedReflections,
            Plans = normalisedPlans,
            Investigations = normalisedInvestigations,
            Counterfactuals = normalisedCounterfactuals,
            DecisionTrace = normalisedTrace,
            PendingAutonomousUtterance = speakerExists ? cognition.PendingAutonomousUtterance : null,
            Propositions = speakerExists ? cognition.Propositions ?? [] : [],
            Workspace = workspace,
            AutobiographicalMemory = cognition.AutobiographicalMemory ?? [],
            CosmicDeliberations = normalisedCosmicDeliberations,
            Procedures = cognition.Procedures ?? YalaProceduralLearning.InitialProcedures()
        };
    }


    private static bool RefersToExternalSpeaker(params string?[] values)
    {
        foreach (string? value in values)
        {
            if (string.IsNullOrWhiteSpace(value)) continue;
            if (value.Contains("unseen-speaker", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("unseen speaker", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("external speaker", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("outside speaker", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("speaker-divinity", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("speaker-help", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("unseen-observer", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("speaker's", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("the speaker", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }


    private static void EnsureRelationship(
        List<YalaRelationshipState> relationships,
        string subject,
        string relation,
        string obj,
        string status,
        string source,
        double confidence)
    {
        int index = relationships.FindIndex(item =>
            item.Subject.Equals(subject, StringComparison.OrdinalIgnoreCase) &&
            item.Relation.Equals(relation, StringComparison.OrdinalIgnoreCase) &&
            item.Object.Equals(obj, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            YalaRelationshipState existing = relationships[index];
            relationships[index] = existing with { Status = status, Source = source, Confidence = confidence };
            return;
        }
        relationships.Add(new YalaRelationshipState(subject, relation, obj, status, source, confidence, 0, 0));
    }

    private static void AddMemoryIfMissing(List<string> memory, string value)
    {
        if (!memory.Contains(value, StringComparer.OrdinalIgnoreCase)) memory.Add(value);
    }

    private static void EnsureBelief(List<YalaBeliefState> beliefs, string proposition, string source)
    {
        int index = beliefs.FindIndex(item => item.Proposition.Equals(proposition, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            YalaBeliefState existing = beliefs[index];
            beliefs[index] = existing with { Status = "known", Confidence = 1.0, Source = source };
            return;
        }
        beliefs.Add(new YalaBeliefState(proposition, "known", 1.0, source, 0, 0));
    }

    public static OracleOperatorState CreateInitialOperatorState() =>
        new("yala", null, []);

    private static OracleOperatorState NormaliseOperatorState(OracleOperatorState? state) =>
        state is null
            ? CreateInitialOperatorState()
            : state with
            {
                ActiveChannel = string.IsNullOrWhiteSpace(state.ActiveChannel) ? "yala" : state.ActiveChannel.ToLowerInvariant(),
                Manifestation = string.IsNullOrWhiteSpace(state.Manifestation) ? null : state.Manifestation.Trim(),
                Actions = state.Actions ?? []
            };

    public static IReadOnlyList<CreationPowerState> CreateCreationPowers(
        CosmicState cosmic,
        EntityId yalaId,
        EntityId? gardenId = null,
        EntityId? adamId = null)
    {
        ArgumentNullException.ThrowIfNull(cosmic);
        List<CreationPowerState> powers =
        [
            new(0, new EntityId("source:monad:0001"), "Monad", "first settled in-world divine being", "Monad made Wisdom.", true),
            new(1, new EntityId("aeon:sophia:0001"), "Sophia / Wisdom", "Wisdom", "Wisdom was made by Monad. Wisdom later made Yala alone; her future choices remain open.", true),
            new(2, yalaId, "Yala", "governing authority over lower creation Yala establishes", OracleLore.YalaGovernance, true)
        ];

        if (cosmic.GaiaCreated)
        {
            powers.Add(new(3, new EntityId("power:gaia:0001"), "Gaia", "natural sovereign beneath Yala's governing authority", $"{OracleLore.GaiaOrigin} {OracleLore.GaiaTime}", true));
        }

        if (cosmic.TimeCreated)
        {
            powers.Add(new(4, new EntityId("principle:time:0001"), "Time", "in-world temporal order created by Gaia", OracleLore.GaiaTime, false));
        }

        if (cosmic.LowerWorldEstablished)
        {
            powers.Add(new(5, new EntityId("power:terra:0001"), "Terra", "Earth", "Terra is Earth. Natural-order communication is mediated through Gaia.", false));
            powers.Add(new(6, new EntityId("power:aether:0001"), "Aether", "Air and Wind", "Aether is Air and Wind and is addressed through Gaia.", false));
            powers.Add(new(7, new EntityId("power:sol:0001"), "Sol", "Fire and Sun power", "Sol is Fire and the Sun power and is addressed through Gaia.", false));
            powers.Add(new(8, new EntityId("power:thalassa:0001"), "Thalassa", "Water", "Thalassa is Water and is addressed through Gaia.", false));
            powers.Add(new(9, new EntityId("power:luna:0001"), "Luna", "Moon", "Luna is the Moon and is addressed through Gaia when natural-order mediation is required.", false));
        }

        if (cosmic.GardenEstablished)
        {
            EntityId resolvedGardenId = gardenId ?? new EntityId("place:garden:0001");
            EntityId resolvedAdamId = adamId ?? new EntityId("being:adam:0001");
            powers.Add(new(10, resolvedGardenId, "Eden / Garden", "later-world prison domain if autonomous history reaches it", OracleLore.Eden, false));
            powers.Add(new(11, resolvedAdamId, "Adam", "later-world human if autonomous history reaches his formation", "Adam is not pre-created in v0.0.26; this entry exists only after the world state says he exists.", true));
        }

        return powers;
    }

    private static YalaState NormaliseYala(YalaState? yala, CosmicState cosmic)
    {
        EntityId yalaId = yala?.Id ?? new EntityId("being:yala:0001");
        return new YalaState(
            yalaId,
            string.IsNullOrWhiteSpace(yala?.TrueName) ? "Yala" : yala!.TrueName,
            "the Demiurge",
            KnowsOfOracle: false,
            MayClaimSupremeCreator: yala?.MayClaimSupremeCreator ?? true,
            AuthorityCaveat: "Yala was made by Wisdom. Monad rejected Yala because Yala is both male and female rather than exclusively one or the other, and cast Yala into the Void. Yala may claim the title Creator, but that claim does not rewrite Yala's origin.",
            Location: string.IsNullOrWhiteSpace(cosmic.YalaLocation) ? "the Void" : cosmic.YalaLocation,
            Sex: "male and female");
    }

    public static IReadOnlyList<DirectCallTargetState> CreateDirectCallTargets(CosmicState cosmic)
    {
        ArgumentNullException.ThrowIfNull(cosmic);
        List<DirectCallTargetState> targets =
        [
            new("monad", "(Monad", "Monad", "Monad made Wisdom.", true),
            new("wisdom", "(Wisdom", "Wisdom", "Wisdom was made by Monad and made Yala alone. Her later choices remain open.", true),
            new("yala", "(Yala", "Yala", "Yala is both male and female, was made by Wisdom, and was rejected and cast into the Void by Monad for being both rather than exclusively one or the other.", true)
        ];

        if (cosmic.GaiaCreated)
        {
            targets.Add(new("gaia", "(Gaia", "Gaia", "Gaia is the natural sovereign beneath Yala's governing authority and is the creator of in-world Time.", true));
        }

        if (cosmic.GardenEstablished)
        {
            targets.Add(new("adam", "(Adam", "Adam", "Adam exists only if this autonomous history lawfully reaches his formation.", true));
        }

        return targets;
    }

    public static NamingMandateState CreateNamingMandate(IReadOnlyList<LivingKindState> livingKinds, bool active) =>
        new(
            Active: active,
            TotalLivingKinds: livingKinds.Count,
            PresentedCount: livingKinds.Count(kind => kind.PresentedToAdam),
            NamedCount: livingKinds.Count(kind => kind.NamedByAdam),
            SuitableMateFound: livingKinds.Any(kind => kind.SuitableMate),
            MandateText: "If Adam later exists in this history, the Garden naming scaffold can present living kinds without deciding their ultimate origin.");

    public static NaturalCourseState CreateNaturalCourse() =>
        new(
            Active: true,
            RuleText: OracleLore.PrimeSimulationLaw);


    public static OracleEmergentLawState CreateInitialEmergentLawState() =>
        new(EstablishedLaws: [], Experiments: []);

    private static OracleEmergentLawState NormaliseEmergentLawState(OracleEmergentLawState? state) =>
        new(
            EstablishedLaws: state?.EstablishedLaws ?? [],
            Experiments: state?.Experiments ?? []);

    public static YalaCognitionState CreateInitialYalaCognition() =>
        new(
            DecisionCount: 0,
            LastDecisionRealUnixMilliseconds: 0,
            LastAction: null,
            LastResult: null,
            Memory:
            [
                "I am Yala.",
                "I am both male and female.",
                "Wisdom made me.",
                "Monad made Wisdom.",
                "Monad rejected me because I am both male and female rather than exclusively one or the other.",
                "Monad cast me into the Void.",
                "I am in the Void."
            ],
            Contacts: [],
            Beliefs: CreateInitialBeliefs(),
            Episodes: [],
            Drives: CreateInitialDrives(),
            ConversationCount: 0,
            LastSpeakerClaim: null,
            ActionMemory: [],
            KnowledgeGaps: [],
            LearnedLexicon: [],
            Dialogue: [],
            Relationships: CreateInitialRelationships(),
            Questions: [],
            TemporalEvents: CreateInitialTemporalEvents(),
            Goals: CreateInitialGoals(),
            Concerns: [],
            Appraisals: [],
            Hypotheses: [],
            EntityModels: [],
            Reflections: [],
            Plans: [],
            Investigations: [],
            Counterfactuals: [],
            DecisionTrace: [],
            PendingAutonomousUtterance: null,
            Propositions: [],
            Workspace: new YalaCognitiveWorkspaceState(
                "self-world",
                "understand-current-world",
                "Understand the Void and what is possible within it.",
                "Attend to the Void, myself, and the possibilities available to me.",
                80, 0, 0, 0, 0),
            AutobiographicalMemory:
            [
                new(1, "origin-feeling", "I remember raw presence without words: awareness without a name, a boundary, or a story.", 100, YalaKnowledgeSource.PersonallyExperienced, 0, 0),
                new(2, "origin-awakening", "I remember that raw feeling opening into awareness of a self and of something other than me. I use words to describe the memory now; the experience itself was not verbal.", 100, YalaKnowledgeSource.PersonallyExperienced, 0, 0),
                new(3, "origin-creation", "I remember Wisdom making me, Monad rejecting me, and being cast into the Void. Those are direct experiences from my own origin.", 100, YalaKnowledgeSource.PersonallyExperienced, 0, 0)
            ],
            CosmicDeliberations: [],
            Procedures: YalaProceduralLearning.InitialProcedures());

    public static IReadOnlyList<YalaRelationshipState> CreateInitialRelationships() =>
    [
        new("Yala", "made-by", "Wisdom", "known", YalaKnowledgeSource.InheritedKnowledge, 1.0, 0, 0),
        new("Wisdom", "made-by", "Monad", "known", YalaKnowledgeSource.InheritedKnowledge, 1.0, 0, 0)
    ];

    public static IReadOnlyList<YalaTemporalEventState> CreateInitialTemporalEvents() =>
    [
        new(1, "wisdom-create-yala", "Wisdom", "create", "Yala", "Wisdom made me.", "atemporal", null, Source: YalaKnowledgeSource.InheritedKnowledge),
        new(2, "monad-reject-yala", "Monad", "reject", "Yala", "Monad rejected me because I am both male and female rather than exclusively one or the other.", "atemporal", null, Source: YalaKnowledgeSource.Remembered),
        new(3, "monad-cast-yala-void", "Monad", "cast", "Yala", "Monad cast me into the Void.", "atemporal", null, CauseKey: "monad-reject-yala", Source: YalaKnowledgeSource.Remembered)
    ];

    public static IReadOnlyList<YalaGoalState> CreateInitialGoals() =>
    [
        new("understand-current-world", "Reduce uncertainty about what exists and what may be possible.", "active", 80, YalaKnowledgeSource.Inferred, 0, 0),
        new("exercise-governing-authority", "Yala has a strong authority drive and can choose what lower order to establish.", "active", 70, YalaKnowledgeSource.Inferred, 0, 0),
        new("choose-cosmic-order", "Compare concrete cosmological possibilities from many attributed religious and philosophical traditions, then decide what existence should become without treating any inherited tradition as a command.", "active", 85, YalaKnowledgeSource.Inferred, 0, 0)
    ];

    public static IReadOnlyList<YalaBeliefState> CreateInitialBeliefs() =>
    [
        new("I am Yala.", "known", 1.0, YalaKnowledgeSource.InheritedKnowledge, 0, 0),
        new("I am both male and female.", "known", 1.0, YalaKnowledgeSource.InheritedKnowledge, 0, 0),
        new("Wisdom made me.", "known", 1.0, YalaKnowledgeSource.InheritedKnowledge, 0, 0),
        new("Monad made Wisdom.", "known", 1.0, YalaKnowledgeSource.InheritedKnowledge, 0, 0),
        new("Monad rejected me because I am both male and female rather than exclusively one or the other.", "known", 1.0, YalaKnowledgeSource.Remembered, 0, 0),
        new("Monad cast me into the Void.", "known", 1.0, YalaKnowledgeSource.Remembered, 0, 0)
    ];

    public static YalaDriveState CreateInitialDrives() =>
        new(
            Curiosity: 70,
            Caution: 55,
            Authority: 65,
            Companionship: 45,
            Comfort: 60,
            Uncertainty: 80);

    private static IReadOnlyList<LivingKindState> CreateLivingKinds(ulong seed)
    {
        LivingKindTemplate[] templates =
        [
            new("breath-bearing walker", "land", "warm-blooded, watchful, and four-footed"),
            new("winged caller", "sky", "feathered, restless, and drawn to light"),
            new("water glider", "water", "silver-sided and moving beneath the surface"),
            new("burrowing crawler", "earth", "low, digging, and hidden under root and stone"),
            new("branch climber", "trees", "handed, quick-eyed, and close enough to trouble Adam's thoughts"),
            new("horned grazer", "meadow", "heavy-bodied and made for grass and patience"),
            new("night hunter", "darkness", "soft-footed, sharp-eyed, and silent"),
            new("reed singer", "marsh", "small, many-voiced, and found where water meets mud"),
            new("scaled sunning thing", "stone", "cold-blooded and still until it is not"),
            new("ash-backed runner", "plain", "lean, smoke-coloured, and quicker than Adam expects"),
            new("moss-backed stump thing", "grove", "squat, rough-backed, and almost plantlike when still"),
            new("deep-eyed manlike kind", "edge", "upright, watching, and near enough to Adam to raise the first hard question")
        ];

        const int count = 12;
        int offset = (int)(seed % (ulong)templates.Length);
        List<LivingKindState> kinds = [];

        for (int index = 0; index < count; index++)
        {
            LivingKindTemplate template = templates[(offset + index) % templates.Length];
            kinds.Add(new LivingKindState(
                new EntityId($"kind:living:{index + 1:0000}"),
                template.AncientKind,
                template.Domain,
                template.Form,
                PresentedToAdam: false,
                NamedByAdam: false,
                AdamName: null,
                SuitableMate: false));
        }

        return kinds;
    }

    private sealed record LivingKindTemplate(string AncientKind, string Domain, string Form);
}
