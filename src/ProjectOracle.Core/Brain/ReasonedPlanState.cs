namespace ProjectOracle.Brain;

public sealed record ReasonedPlanState(
    long Id,
    long CreatedAtWorldMilliseconds,
    string ActorId,
    string BrainSystem,
    string Goal,
    string Situation,
    IReadOnlyList<string> Decomposition,
    IReadOnlyList<string> Options,
    string SelectedAction,
    string Reason,
    string Source);
