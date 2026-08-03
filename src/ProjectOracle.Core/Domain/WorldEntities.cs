namespace ProjectOracle.Domain;

public sealed record YalaState(
    EntityId Id,
    string TrueName,
    string WorldTitle,
    bool KnowsOfCreators,
    bool KnowsFutureLanguageMandate,
    bool MayClaimSupremeCreator,
    string AuthorityCaveat);

public sealed record AdamState(
    EntityId Id,
    string Name,
    EntityId LocationId,
    bool IsConfinedToGarden);

public sealed record SparkState(
    EntityId BearerId,
    bool CanBeReadByYala,
    bool CanBeRewrittenByYala,
    string CreatorDescription);

public sealed record GardenState(
    EntityId Id,
    string Name,
    bool BoundaryOpen);

public sealed record AddressChannelState(
    string Key,
    string Prompt,
    string FunctionKey,
    string TargetName,
    string AuthoritySummary,
    bool ReceivesDirectAddress);

public sealed record LivingKindState(
    EntityId Id,
    string AncientKind,
    string Domain,
    string Form,
    bool PresentedToAdam,
    bool NamedByAdam,
    string? AdamName,
    bool SuitableMate);

public sealed record CreationPowerState(
    int Order,
    EntityId Id,
    string Name,
    string Domain,
    string AuthoritySummary,
    bool ReceivesDirectAddress);

public sealed record NamingMandateState(
    bool Active,
    int TotalLivingKinds,
    int PresentedCount,
    int NamedCount,
    bool SuitableMateFound,
    string MandateText);

public sealed record NaturalCourseState(
    bool Active,
    string RuleText);

public sealed record WorldState(
    ulong Seed,
    long WorldMilliseconds,
    GardenState Garden,
    YalaState Yala,
    AdamState Adam,
    SparkState AdamSpark,
    IReadOnlyList<CreationPowerState> CreationPowers,
    IReadOnlyList<AddressChannelState> AddressChannels,
    IReadOnlyList<LivingKindState> LivingKinds,
    NamingMandateState NamingMandate,
    NaturalCourseState NaturalCourse);
