using ProjectOracle.Cognition;
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
            YalaLocation: "the Void");

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
            YalaCognition: CreateInitialYalaCognition());
    }

    public static WorldState Normalise(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        // v0.0.20 continues the v0.0.17/v0.0.18 save line. Missing cosmic state never
        // resurrects the old Garden-era save; it normalises to the new Void start.
        CosmicState cosmic = world.Cosmic ?? new CosmicState(
            GaiaCreated: false,
            TimeCreated: false,
            LowerWorldEstablished: false,
            GardenEstablished: false,
            YalaLocation: "the Void");

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
            NaturalCourse = CreateNaturalCourse()
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
        if (cosmic.TimeCreated && !actions.Any(item => item.Completed && item.Action == "command" && item.Object.Equals("Gaia establish Time", StringComparison.OrdinalIgnoreCase)))
        {
            actions.Add(new YalaActionMemoryState("command", "Gaia establish Time", "I commanded Gaia to establish temporal order, and Gaia created in-world Time.", true, 0));
        }

        return cognition with
        {
            Memory = memory,
            Contacts = cognition.Contacts ?? [],
            Beliefs = beliefs,
            Episodes = cognition.Episodes ?? [],
            Drives = cognition.Drives ?? CreateInitialDrives(),
            ActionMemory = actions,
            KnowledgeGaps = cognition.KnowledgeGaps ?? [],
            LearnedLexicon = cognition.LearnedLexicon ?? []
        };
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
            powers.Add(new(5, new EntityId("power:terra:0001"), "Terra", "Earth", "Terra is Earth.", true));
            powers.Add(new(6, new EntityId("power:aether:0001"), "Aether", "Air and Wind", "Aether is Air and Wind and governs that domain.", true));
            powers.Add(new(7, new EntityId("power:sol:0001"), "Sol", "Fire and Sun power", "Sol is Fire and the Sun power.", true));
            powers.Add(new(8, new EntityId("power:thalassa:0001"), "Thalassa", "Water", "Thalassa is Water.", true));
            powers.Add(new(9, new EntityId("power:luna:0001"), "Luna", "Moon", "Luna is the Moon and is not an element.", true));
        }

        if (cosmic.GardenEstablished)
        {
            EntityId resolvedGardenId = gardenId ?? new EntityId("place:garden:0001");
            EntityId resolvedAdamId = adamId ?? new EntityId("being:adam:0001");
            powers.Add(new(10, resolvedGardenId, "Eden / Garden", "later-world prison domain if autonomous history reaches it", OracleLore.Eden, false));
            powers.Add(new(11, resolvedAdamId, "Adam", "later-world human if autonomous history reaches his formation", "Adam is not pre-created in v0.0.20; this entry exists only after the world state says he exists.", true));
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

        if (cosmic.LowerWorldEstablished)
        {
            targets.Add(new("terra", "(Terra", "Terra", "Terra is Earth.", true));
            targets.Add(new("aether", "(Aether", "Aether", "Aether is Air and Wind.", true));
            targets.Add(new("sol", "(Sol", "Sol", "Sol is Fire and the Sun power.", true));
            targets.Add(new("thalassa", "(Thalassa", "Thalassa", "Thalassa is Water.", true));
            targets.Add(new("luna", "(Luna", "Luna", "Luna is the Moon and is not an element.", true));
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
            LearnedLexicon: []);

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
