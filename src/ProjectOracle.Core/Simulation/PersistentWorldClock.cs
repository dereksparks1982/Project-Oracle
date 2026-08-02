namespace ProjectOracle.Simulation;

public sealed class PersistentWorldClock
{
    public const int WorldSecondsPerRealSecond = 4;
    public const int WorldDaysPerRealDay = 4;
    public const long RealMillisecondsPerWorldDay = 21_600_000;
    public const long WorldMillisecondsPerDay = 86_400_000;

    public PersistentWorldClock(
        long worldMilliseconds,
        long lastRealUnixMilliseconds,
        int catchUpRuns = 0,
        long lastOfflineElapsedRealMilliseconds = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(worldMilliseconds);
        ArgumentOutOfRangeException.ThrowIfNegative(lastRealUnixMilliseconds);
        ArgumentOutOfRangeException.ThrowIfNegative(catchUpRuns);
        ArgumentOutOfRangeException.ThrowIfNegative(lastOfflineElapsedRealMilliseconds);

        WorldMilliseconds = worldMilliseconds;
        LastRealUnixMilliseconds = lastRealUnixMilliseconds;
        CatchUpRuns = catchUpRuns;
        LastOfflineElapsedRealMilliseconds = lastOfflineElapsedRealMilliseconds;
    }

    public long WorldMilliseconds { get; private set; }

    public long LastRealUnixMilliseconds { get; private set; }

    public int CatchUpRuns { get; private set; }

    public long LastOfflineElapsedRealMilliseconds { get; private set; }

    public long DayNumber => (WorldMilliseconds / WorldMillisecondsPerDay) + 1;

    public int Hour => Calendar.Hour;

    public int Minute => Calendar.Minute;

    public int Second => Calendar.Second;

    public string Phase => Calendar.SolarPhase;

    public CalendarSnapshot Calendar => OracleCalendar.FromElapsedWorldMilliseconds(WorldMilliseconds);

    public ClockAdvance Synchronise(long currentRealUnixMilliseconds, bool offlineCatchUp)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(currentRealUnixMilliseconds);

        long rawElapsed = checked(currentRealUnixMilliseconds - LastRealUnixMilliseconds);
        bool backwardClockDetected = rawElapsed < 0;
        long elapsedRealMilliseconds = Math.Max(rawElapsed, 0);
        long elapsedWorldMilliseconds = checked(elapsedRealMilliseconds * WorldSecondsPerRealSecond);

        WorldMilliseconds = checked(WorldMilliseconds + elapsedWorldMilliseconds);
        LastRealUnixMilliseconds = Math.Max(currentRealUnixMilliseconds, LastRealUnixMilliseconds);

        if (offlineCatchUp)
        {
            CatchUpRuns = checked(CatchUpRuns + 1);
            LastOfflineElapsedRealMilliseconds = elapsedRealMilliseconds;
        }

        return new ClockAdvance(
            elapsedRealMilliseconds,
            elapsedWorldMilliseconds,
            offlineCatchUp,
            backwardClockDetected);
    }

    public string Describe() => Calendar.DescribeDateAndTime();
}

public sealed record ClockAdvance(
    long ElapsedRealMilliseconds,
    long ElapsedWorldMilliseconds,
    bool WasOfflineCatchUp,
    bool BackwardClockDetected);
