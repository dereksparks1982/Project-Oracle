namespace ProjectOracle.Events;

public sealed record ScheduledWorldEvent(
    long Id,
    long ScheduledForWorldMilliseconds,
    int Priority,
    long CreatedAtWorldMilliseconds,
    string Kind,
    string SubjectId,
    string Payload,
    ScheduledWorldEventStatus Status,
    long? CompletedAtWorldMilliseconds = null);

public enum ScheduledWorldEventStatus
{
    Pending,
    Completed,
    Cancelled
}
