namespace ProjectOracle.Simulation;

/// <summary>
/// SplitMix64 with an explicitly frozen algorithm for reproducible Oracle runs.
/// Do not replace this with System.Random without a versioned migration.
/// </summary>
public sealed class DeterministicRandom
{
    private ulong _state;

    public DeterministicRandom(ulong seed)
    {
        _state = seed;
    }

    public ulong NextUInt64()
    {
        _state += 0x9E3779B97F4A7C15UL;
        ulong value = _state;
        value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
        value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
        return value ^ (value >> 31);
    }

    public int NextInt(int exclusiveUpperBound)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(exclusiveUpperBound);
        return (int)(NextUInt64() % (uint)exclusiveUpperBound);
    }
}
