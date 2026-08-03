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
                KnowsFutureLanguageMandate: true),
            new AdamState(adamId, "Adam", gardenId, IsConfinedToGarden: true),
            new SparkState(
                adamId,
                CanBeReadByYala: false,
                CanBeRewrittenByYala: false,
                CreatorDescription: "A protected source of genuine choice placed by the Creators."),
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
            AddressChannels = world.AddressChannels is { Count: > 0 } ? world.AddressChannels : CreateAddressChannels(),
            LivingKinds = livingKinds,
            NamingMandate = mandate,
            NaturalCourse = world.NaturalCourse ?? CreateNaturalCourse()
        };
    }

    public static IReadOnlyList<AddressChannelState> CreateAddressChannels() =>
    [
        new("oracle", "<oracle>", "F1", "the Oracle", "Appointed godlike ruler; above Gaia, Sun, Moon, and Adam.", true),
        new("gaia", "<gaia>", "F2", "Gaia", "Earth and living-world power; governs animals, growth, weather, land, and waters.", true),
        new("adam", "<adam>", "F3", "Adam", "First man inside the Garden; protected choice must not be puppeteered.", true),
        new("sun", "<sun>", "F4", "the Sun", "Greater light appointed to govern the day; normally follows its fixed course.", true),
        new("moon", "<moon>", "F5", "the Moon", "Lesser light appointed to govern the night; normally follows its fixed course.", true)
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
            new("scaled sunning thing", "stone", "cold-blooded and still until it is not")
        ];

        int count = 6 + (int)((seed ^ (seed >> 7)) % 3UL);
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
