namespace ProjectOracle.Observation;

public sealed record ObservationState(
    long Id,
    long WorldMilliseconds,
    string ObserverId,
    string ObserverName,
    string SubjectId,
    string SubjectName,
    string ObservationKind,
    string Detail,
    string DistanceBand,
    bool AttentionMatched,
    bool AdamReceives,
    bool CreatorTruthHidden,
    string Source);
