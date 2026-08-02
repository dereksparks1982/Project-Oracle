namespace ProjectOracle.Simulation;

public static class OracleCalendar
{
    public const int MonthsPerYear = 12;
    public const int DaysPerYear = 365;
    public const long EpochTimeOfDayMilliseconds = 3_661_000;
    public const long LunarCycleMilliseconds = 2_551_442_803;

    private static readonly int[] MonthLengths = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
    private static readonly string[] LunarPhases =
    [
        "new moon",
        "waxing crescent",
        "first quarter",
        "waxing gibbous",
        "full moon",
        "waning gibbous",
        "last quarter",
        "waning crescent"
    ];

    public static CalendarSnapshot FromElapsedWorldMilliseconds(long elapsedWorldMilliseconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(elapsedWorldMilliseconds);

        long absoluteWorldMilliseconds = checked(EpochTimeOfDayMilliseconds + elapsedWorldMilliseconds);
        long elapsedWholeDays = absoluteWorldMilliseconds / PersistentWorldClock.WorldMillisecondsPerDay;
        long millisecondsInDay = absoluteWorldMilliseconds % PersistentWorldClock.WorldMillisecondsPerDay;

        long year = checked((elapsedWholeDays / DaysPerYear) + 1);
        int dayOfYear = (int)(elapsedWholeDays % DaysPerYear);
        int month = 1;
        while (dayOfYear >= MonthLengths[month - 1])
        {
            dayOfYear -= MonthLengths[month - 1];
            month++;
        }

        int day = dayOfYear + 1;
        int hour = (int)(millisecondsInDay / 3_600_000);
        int minute = (int)((millisecondsInDay % 3_600_000) / 60_000);
        int second = (int)((millisecondsInDay % 60_000) / 1_000);

        int lunarPhaseIndex = (int)((absoluteWorldMilliseconds % LunarCycleMilliseconds) * 8 / LunarCycleMilliseconds);
        string solarPhase = SolarPhase(hour, minute);
        int solarCycleDegrees = (int)(millisecondsInDay * 360 / PersistentWorldClock.WorldMillisecondsPerDay);

        return new CalendarSnapshot(
            year,
            month,
            day,
            hour,
            minute,
            second,
            solarPhase,
            solarCycleDegrees,
            LunarPhases[lunarPhaseIndex],
            lunarPhaseIndex,
            absoluteWorldMilliseconds % LunarCycleMilliseconds);
    }

    private static string SolarPhase(int hour, int minute)
    {
        int minuteOfDay = (hour * 60) + minute;
        return minuteOfDay switch
        {
            >= 300 and < 420 => "dawn",
            >= 420 and < 1_020 => "day",
            >= 1_020 and < 1_140 => "dusk",
            _ => "night"
        };
    }
}

public sealed record CalendarSnapshot(
    long Year,
    int Month,
    int Day,
    int Hour,
    int Minute,
    int Second,
    string SolarPhase,
    int SolarCycleDegrees,
    string LunarPhase,
    int LunarPhaseIndex,
    long LunarAgeMilliseconds)
{
    public string DescribeDateAndTime() =>
        $"Year {Year}, Month {Month}, Day {Day}, {Hour:00}:{Minute:00}:{Second:00}";

    public string DescribeSky() =>
        $"Sun: {SolarPhase}; Moon: {LunarPhase}";
}
