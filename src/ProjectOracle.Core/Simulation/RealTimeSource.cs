namespace ProjectOracle.Simulation;

public interface IRealTimeSource
{
    long GetUnixTimeMilliseconds();
}

public sealed class SystemRealTimeSource : IRealTimeSource
{
    public long GetUnixTimeMilliseconds() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
