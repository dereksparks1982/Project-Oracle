namespace ProjectOracle.Domain;

public static class WorldDefaults
{
    public static WorldState CreateInitialState(ulong seed)
    {
        EntityId gardenId = new("place:garden:0001");
        EntityId yalaId = new("being:yala:0001");
        EntityId adamId = new("being:adam:0001");
        IReadOnlyList<LivingKindState> livingKinds = CreateLivingKinds(seed);

        return new WorldState(
            seed,
            WorldMilliseconds: 0,
            new GardenState(gardenId, "the Garden", BoundaryOpen: false),
            new YalaState(
                yalaId,
                TrueName: "Yala",
                WorldTitle: "the Oracle",
                KnowsOfCreators: true,
                KnowsFutureLanguageMandate: true,
                MayClaimSupremeCreator: true,
                AuthorityCaveat: "Yala is the Oracle. She knows the creation order, but may claim that she rules all or created all. The protected Creator Record outranks her claim."),
            new AdamState(adamId, "Adam", gardenId, IsConfinedToGarden: true),
            new SparkState(
                adamId,
                CanBeReadByYala: false,
                CanBeRewrittenByYala: false,
                CreatorDescription: "A protected source of genuine choice placed by the Creators."),
            CreateCreationPowers(yalaId, gardenId, adamId),
            CreateAddressChannels(),
            livingKinds,
            CreateNamingMandate(livingKinds),
            CreateNaturalCourse());
    }

    public static WorldState Normalise(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        IReadOnlyList<LivingKindState> livingKinds = world.LivingKinds is { Count: > 0 }
            ? world.LivingKinds
            : CreateLivingKinds(world.Seed);

        NamingMandateState mandate = world.NamingMandate ?? CreateNamingMandate(livingKinds);
        mandate = mandate with
        {
            TotalLivingKinds = livingKinds.Count,
            PresentedCount = livingKinds.Count(kind => kind.PresentedToAdam),
            NamedCount = livingKinds.Count(kind => kind.NamedByAdam),
            SuitableMateFound = livingKinds.Any(kind => kind.SuitableMate)
        };

        return world with
        {
            Yala = NormaliseYala(world.Yala),
            CreationPowers = CreateCreationPowers(
                world.Yala?.Id ?? new EntityId("being:yala:0001"),
                world.Garden?.Id ?? new EntityId("place:garden:0001"),
                world.Adam?.Id ?? new EntityId("being:adam:0001")),
            AddressChannels = CreateAddressChannels(),
            LivingKinds = livingKinds,
            NamingMandate = mandate,
            NaturalCourse = world.NaturalCourse ?? CreateNaturalCourse()
        };
    }

    public static IReadOnlyList<CreationPowerState> CreateCreationPowers(EntityId yalaId, EntityId gardenId, EntityId adamId) =>
    [
        new(0, new EntityId("condition:void:0001"), "Void", "Yala's prison before the formed world", "The empty prison exists before Yala begins shaping it.", false),
        new(1, yalaId, "Yala", "created demi-god placed inside the void", "Thrown into the void by the Creators to see what she would do with her prison.", true),
        new(2, new EntityId("power:sol:0001"), "Sol", "first light, fire, heat, and counted time", "First formed world power created by Yala.", true),
        new(3, new EntityId("power:gaia:0001"), "Gaia", "earth-body, land, ground, and world-body", "A lesser demi-god power created by Yala for world-body and ground.", true),
        new(3, new EntityId("power:aether:0001"), "Aether", "air, breath-space, sky, and atmosphere", "A lesser demi-god power created by Yala so the world has breath-space.", false),
        new(4, new EntityId("power:thalassa:0001"), "Thalassa", "waters, depths, rivers, and seas", "A lesser demi-god water power created by Yala.", false),
        new(5, new EntityId("power:luna:0001"), "Luna", "moon, night measure, tides, and reflected light", "A lesser demi-god moon power created by Yala after Sol.", true),
        new(6, new EntityId("world:formed:0001"), "World", "the prison shaped into a formed world", "Yala shaped the void-prison into a world under the powers she made.", false),
        new(7, new EntityId("power:green-life:0001"), "Green Life", "plants, growth, and Gaia's first living covering", "Plants and green life come after the world exists.", false),
        new(8, gardenId, "Garden", "contained preserve prepared for man", "Created just before Adam as a closed place to contain man and the appointed living-kind trial.", false),
        new(9, adamId, "Adam", "first man and protected Spark-bearer", "Created after the Garden and before the living kinds in this Oracle canon.", true),
        new(10, new EntityId("kind:living:all"), "Living Kinds", "animals and ancient living forms", "Created after Adam and named by Adam.", false)
    ];

    private static YalaState NormaliseYala(YalaState? yala)
    {
        EntityId yalaId = yala?.Id ?? new EntityId("being:yala:0001");
        const string canonicalAuthorityCaveat = "Yala is the Oracle. She knows the creation order, but may claim that she rules all or created all. The protected Creator Record outranks her claim.";
        string authorityCaveat = string.IsNullOrWhiteSpace(yala?.AuthorityCaveat) ||
            !yala.AuthorityCaveat.Contains("Yala is the Oracle", StringComparison.OrdinalIgnoreCase)
            ? canonicalAuthorityCaveat
            : yala.AuthorityCaveat;

        return new YalaState(
            yalaId,
            string.IsNullOrWhiteSpace(yala?.TrueName) ? "Yala" : yala.TrueName,
            string.IsNullOrWhiteSpace(yala?.WorldTitle) ? "the Oracle" : yala.WorldTitle,
            yala?.KnowsOfCreators ?? true,
            yala?.KnowsFutureLanguageMandate ?? true,
            MayClaimSupremeCreator: true,
            authorityCaveat);
    }

    public static IReadOnlyList<AddressChannelState> CreateAddressChannels() =>
    [
        new("oracle", "<oracle>", "F1", "Yala / the Oracle", "Yala's direct address channel and in-world title; first in creation order and above Gaia in the address hierarchy, but not above the external Creators.", true),
        new("gaia", "<gaia>", "F2", "Gaia", "Earth-body power formed with Aether; governs land, growth, and ordinary world-body systems.", true),
        new("adam", "<adam>", "F3", "Adam", "First man inside the Garden; protected choice must not be puppeteered.", true),
        new("sun", "<sun>", "F4", "Sol", "Sun and fire power; first light, heat, and timekeeper.", true),
        new("moon", "<moon>", "F5", "Luna", "Moon power; night marker, tides, and reflected light.", true)
    ];

    public static NamingMandateState CreateNamingMandate(IReadOnlyList<LivingKindState> livingKinds) =>
        new(
            Active: true,
            TotalLivingKinds: livingKinds.Count,
            PresentedCount: livingKinds.Count(kind => kind.PresentedToAdam),
            NamedCount: livingKinds.Count(kind => kind.NamedByAdam),
            SuitableMateFound: livingKinds.Any(kind => kind.SuitableMate),
            MandateText: "Adam is to name the living kinds and see whether any is a suitable mate.");

    public static NaturalCourseState CreateNaturalCourse() =>
        new(
            Active: true,
            RuleText: "If nobody intervenes, every created being follows its appointed nature, memory, needs, duties, and planned course.");

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
