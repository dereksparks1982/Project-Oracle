namespace ProjectOracle.Audit;

public sealed class AuditLedger
{
    private readonly List<OracleRecord> _records = [];
    private long _nextSequence = 1;

    public AuditLedger()
    {
    }

    public AuditLedger(IEnumerable<OracleRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);
        _records.AddRange(records.OrderBy(record => record.Sequence));
        _nextSequence = _records.Count == 0 ? 1 : checked(_records[^1].Sequence + 1);
    }

    public IReadOnlyList<OracleRecord> WorldRecords =>
        _records.Where(record => record.Audience == RecordAudience.World).ToArray();

    public IReadOnlyList<OracleRecord> CreatorRecords =>
        _records.Where(record => record.Audience == RecordAudience.Creator).ToArray();

    public void RecordWorld(long tick, string category, string message) =>
        Add(tick, RecordAudience.World, category, message);

    public void RecordCreator(long tick, string category, string message) =>
        Add(tick, RecordAudience.Creator, category, message);

    private void Add(long tick, RecordAudience audience, string category, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _records.Add(new OracleRecord(_nextSequence++, tick, audience, category, message));
    }
}
