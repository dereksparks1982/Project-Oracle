namespace ProjectOracle.Audit;

public enum RecordAudience
{
    World,
    Creator
}

public sealed record OracleRecord(
    long Sequence,
    long Tick,
    RecordAudience Audience,
    string Category,
    string Message);
