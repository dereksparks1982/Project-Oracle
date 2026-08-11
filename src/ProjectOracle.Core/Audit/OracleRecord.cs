namespace ProjectOracle.Audit;

public enum RecordAudience
{
    World,
    Oracle
}

public sealed record OracleRecord(
    long Sequence,
    long Tick,
    RecordAudience Audience,
    string Category,
    string Message);
