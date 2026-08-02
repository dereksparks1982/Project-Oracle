namespace ProjectOracle.Domain;

public sealed record YalaState(
    EntityId Id,
    string TrueName,
    string WorldTitle,
    bool KnowsOfCreators,
    bool KnowsFutureLanguageMandate);

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

public sealed record WorldState(
    ulong Seed,
    long WorldMilliseconds,
    GardenState Garden,
    YalaState Yala,
    AdamState Adam,
    SparkState AdamSpark);
