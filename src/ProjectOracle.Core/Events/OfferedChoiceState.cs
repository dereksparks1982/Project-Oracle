namespace ProjectOracle.Events;

public sealed record OfferedChoiceState(
    long Id,
    long SourceEventId,
    long OfferedAtWorldMilliseconds,
    string ActorId,
    string Situation,
    IReadOnlyList<string> Options,
    string SelectedOption,
    string Reason);
