using ProjectOracle.Lore;

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
                WorldTitle: "the Demiurge",
                KnowsOfCreators: true,
                KnowsFutureLanguageMandate: false,
                MayClaimSupremeCreator: true,
                AuthorityCaveat: "Yala is a powerful created demiurge, but she is not the Highest Source and she is not the Oracle. Oracle lies beyond Yala's control. Protected Creator records outrank Yala's claims."),
            new AdamState(adamId, "Adam", gardenId, IsConfinedToGarden: true),
            new SparkState(
                adamId,
                CanBeReadByYala: false,
                CanBeRewrittenByYala: false,
                CreatorDescription: "A protected higher spark carried into the humanoid line; Yala cannot read or rewrite it."),
            CreateCreationPowers(yalaId, gardenId, adamId),
            CreateAddressChannels(),
            livingKinds,
            CreateNamingMandate(livingKinds),
            CreateNaturalCourse(),
            CreateOracle());
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
            Garden = world.Garden is null
                ? new GardenState(new EntityId("place:garden:0001"), "the Garden", BoundaryOpen: false)
                : world.Garden with { Name = "the Garden" },
            Yala = NormaliseYala(world.Yala),
            Oracle = world.Oracle ?? CreateOracle(),
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

    public static OracleState CreateOracle() =>
        new(
            new EntityId("anomaly:oracle:master-key"),
            "Oracle",
            IsGod: false,
            IsCreator: false,
            BeyondYalaControl: true,
            Nature: OracleLore.OracleNature,
            FirstManifestation: OracleLore.OracleSerpent,
            AlignmentRule: OracleLore.OracleAlignment);

    public static IReadOnlyList<CreationPowerState> CreateCreationPowers(EntityId yalaId, EntityId gardenId, EntityId adamId) =>
    [
        new(0, new EntityId("source:monad:0001"), "Highest Source / Monad", "uncreated highest source above the lower genealogy", "The highest source is not Yala and is not subject to Yala's authority.", false),
        new(1, new EntityId("aeon:sophia:0001"), "Sophia / Wisdom", "higher aeonic Wisdom and source of Yala", "Sophia created Yala. Her later fall into Deception and union with Yala belong to the humanoid-origin canon.", false),
        new(2, yalaId, "Yala", "created demiurge; creator of Gaia; co-creator of humans and humanoids with Sophia", "Yala may claim supremacy, but Oracle is separate and beyond Yala's control.", false),
        new(3, new EntityId("power:gaia:0001"), "Gaia", "natural-world sovereign and creator of the elemental powers", "Gaia answers beneath Yala in lineage, while the elemental beings answer to Gaia.", true),
        new(4, new EntityId("power:elements:all"), "Elemental Powers", "separate elemental entities governing weather and natural forces", "Created by Gaia. They control weather and natural forces and answer to Gaia. Their final four-or-five-member roster remains open.", false),
        new(5, new EntityId("world:formed:0001"), "World", "formed natural world shaped through the lower creation", "The exact mechanical division between Gaia and her elements remains a future implementation decision.", false),
        new(6, new EntityId("life:plants:all"), "Plants", "ordinary plant life", "Plants are brought forth through Gaia's elemental powers. There is no Green Life entity or category.", false),
        new(7, gardenId, "Eden / Garden", "beautiful prison and containment environment", "Eden is a prison. Oracle enters it as the serpent and opens forbidden access to knowledge.", false),
        new(8, new EntityId("kind:humanoid:all"), "Humanoid Peoples", "humans and other humanoid beings", "Sophia and Yala bring forth humans and the other humanoid peoples together.", false),
        new(9, adamId, "Adam", "first currently modelled human inside Eden", "Adam is confined inside Eden at world start and retains protected choice.", true),
        new(10, new EntityId("kind:living:all"), "Ordinary Animals", "ordinary animals and ancient living forms", "Yala did not create ordinary animals. Their exact origin within the Gaia/elemental natural branch remains unresolved.", false)
    ];

    private static YalaState NormaliseYala(YalaState? yala)
    {
        EntityId yalaId = yala?.Id ?? new EntityId("being:yala:0001");
        const string canonicalAuthorityCaveat = "Yala is a powerful created demiurge, but she is not the Highest Source and she is not the Oracle. Oracle lies beyond Yala's control. Protected Creator records outrank Yala's claims.";

        return new YalaState(
            yalaId,
            string.IsNullOrWhiteSpace(yala?.TrueName) ? "Yala" : yala.TrueName,
            "the Demiurge",
            KnowsOfCreators: true,
            KnowsFutureLanguageMandate: false,
            MayClaimSupremeCreator: true,
            canonicalAuthorityCaveat);
    }

    public static IReadOnlyList<AddressChannelState> CreateAddressChannels() =>
    [
        new("oracle", "<oracle>", "F1", "Oracle", "The living Master Key. Oracle is neither a god nor a creator and cannot be controlled or removed by Yala.", true),
        new("gaia", "<gaia>", "F2", "Gaia", "Natural-world sovereign; creator and ruler of the elemental powers.", true),
        new("adam", "<adam>", "F3", "Adam", "A human inside Eden; protected choice must not be puppeteered.", true),
        new("sun", "<sun>", "F4", "Sol / Sun", "Existing celestial direct-address channel retained while the final elemental roster is still open.", true),
        new("moon", "<moon>", "F5", "Luna / Moon", "Existing celestial direct-address channel retained while the final elemental roster is still open.", true)
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
