namespace ProjectOracle.Observation;

public sealed record AttentionState(
    string ActorId,
    string ActorName,
    string TargetId,
    string TargetName,
    string Focus,
    long ChangedAtWorldMilliseconds,
    string Source);
