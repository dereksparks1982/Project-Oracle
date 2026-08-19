using ProjectOracle.Domain;

namespace ProjectOracle.Cognition;

public static class YalaTemporalReasoner
{
    public static string DescribeWhen(
        YalaCognitionState cognition,
        string? subject,
        string? action,
        string? obj)
    {
        IReadOnlyList<YalaTemporalEventState> events = cognition.TemporalEvents ?? [];
        YalaTemporalEventState? match = FindEvent(events, subject, action, obj);
        if (match is null)
        {
            if (string.Equals(obj, "Adam", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(subject, "Adam", StringComparison.OrdinalIgnoreCase))
            {
                return "Adam has not been created in my current world, so there is no creation date for Adam.";
            }
            return "I remember no event matching that question closely enough to give it a time.";
        }
        return DescribeEventTime(match);
    }

    public static string DescribeEventTime(YalaTemporalEventState item) => item.TemporalState switch
    {
        "atemporal" => $"{item.Summary} I remember it from existence outside temporal order, so it has no in-world date.",
        "origin-of-time" => $"{item.Summary} That event began in-world temporal reckoning at Year 1, Month 1, Day 1, 00:00:00.",
        "dated" when item.Year is not null => $"{item.Summary} It occurred at {item.Hour:00}:{item.Minute:00}:{item.Second:00}, Year {item.Year}, Month {item.Month}, Day {item.Day}.",
        _ => $"{item.Summary} I remember the event, but not a usable in-world date."
    };


    public static string DescribeCause(YalaCognitionState cognition, string? subject, string? action, string? obj)
    {
        IReadOnlyList<YalaTemporalEventState> events = cognition.TemporalEvents ?? [];
        YalaTemporalEventState? item = FindEvent(events, subject, action, obj);
        if (item is null) return "I cannot identify the event closely enough to give its cause.";
        if (string.IsNullOrWhiteSpace(item.CauseKey)) return $"I remember {item.Summary.ToLowerInvariant()} but I do not have a settled cause linked to that event.";
        YalaTemporalEventState? cause = events.FirstOrDefault(candidate => candidate.Key.Equals(item.CauseKey, StringComparison.OrdinalIgnoreCase));
        return cause is null
            ? $"I remember that {item.Summary.ToLowerInvariant()} had a cause, but I cannot retrieve the cause event."
            : $"{item.Summary} The linked cause I remember is: {cause.Summary}";
    }

    public static string DescribeHowLongAgo(
        YalaCognitionState cognition,
        string? subject,
        string? action,
        string? obj,
        long currentWorldMilliseconds)
    {
        IReadOnlyList<YalaTemporalEventState> events = cognition.TemporalEvents ?? [];
        YalaTemporalEventState? item = FindEvent(events, subject, action, obj);
        if (item is null) return "I cannot identify the event closely enough to measure how long ago it occurred.";
        if (item.TemporalState == "atemporal") return $"{item.Summary} I remember it from existence outside temporal order, so no in-world duration can measure it.";
        if (item.TemporalState == "origin-of-time") return $"{item.Summary} That event began Time itself. {FormatDuration(Math.Max(0, currentWorldMilliseconds))} of in-world Time has passed since then.";
        if (item.WorldMilliseconds is null) return $"I remember {item.Summary.ToLowerInvariant()}, but I do not have a usable in-world timestamp for it.";
        long elapsed = Math.Max(0, currentWorldMilliseconds - item.WorldMilliseconds.Value);
        return $"{item.Summary} That was {FormatDuration(elapsed)} ago in in-world Time.";
    }

    public static string DescribeAdjacent(YalaCognitionState cognition, string direction, string? subject, string? action, string? obj)
    {
        IReadOnlyList<YalaTemporalEventState> events = (cognition.TemporalEvents ?? []).OrderBy(item => item.Sequence).ToArray();
        YalaTemporalEventState? anchor = FindEvent(events, subject, action, obj);
        if (anchor is null) return "I cannot identify the event you mean well enough to compare what came before or after it.";
        if (anchor.TemporalState == "atemporal")
        {
            return $"I remember {anchor.Summary.ToLowerInvariant()}, but that memory is outside temporal order. I cannot truthfully place another event before or after it. I can follow a remembered cause instead when one exists.";
        }
        int index = events.ToList().FindIndex(item => item.Sequence == anchor.Sequence);
        int adjacent = direction.Equals("before", StringComparison.OrdinalIgnoreCase) ? index - 1 : index + 1;
        if (adjacent < 0 || adjacent >= events.Count) return $"I remember no event {direction} {anchor.Summary.ToLowerInvariant()}.";
        YalaTemporalEventState other = events[adjacent];
        if (other.TemporalState == "atemporal")
        {
            return $"The nearby memory is {other.Summary.ToLowerInvariant()}, but it is outside temporal order, so I cannot truthfully call it {direction} this event.";
        }
        return $"{Capitalize(direction)} that, {other.Summary}";
    }

    private static YalaTemporalEventState? FindEvent(
        IReadOnlyList<YalaTemporalEventState> events,
        string? subject,
        string? action,
        string? obj)
    {
        string? s = Normalize(subject);
        string? a = NormalizeAction(action);
        string? o = Normalize(obj);
        IEnumerable<YalaTemporalEventState> query = events.OrderByDescending(item => item.Sequence);
        if (!string.IsNullOrWhiteSpace(s)) query = query.Where(item => Normalize(item.Subject) == s);
        if (!string.IsNullOrWhiteSpace(a)) query = query.Where(item => NormalizeAction(item.Action) == a);
        if (!string.IsNullOrWhiteSpace(o)) query = query.Where(item => Normalize(item.Object) == o || item.Summary.Contains(obj!, StringComparison.OrdinalIgnoreCase));
        return query.FirstOrDefault();
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value)
        ? null
        : YalaDialogueContext.CanonicalEntity(value.Trim()).ToLowerInvariant();

    private static string? NormalizeAction(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        string key = ProjectOracle.Cognition.Language.YalaLexicon.NormalizeWord(value);
        return key switch
        {
            "make" => "create",
            "made" => "create",
            _ => key
        };
    }


    private static string FormatDuration(long milliseconds)
    {
        TimeSpan span = TimeSpan.FromMilliseconds(milliseconds);
        if (span.TotalDays >= 1) return $"{(long)span.TotalDays} day(s), {span.Hours} hour(s), {span.Minutes} minute(s)";
        if (span.TotalHours >= 1) return $"{(long)span.TotalHours} hour(s), {span.Minutes} minute(s), {span.Seconds} second(s)";
        if (span.TotalMinutes >= 1) return $"{(long)span.TotalMinutes} minute(s), {span.Seconds} second(s)";
        return $"{Math.Max(0, (long)span.TotalSeconds)} second(s)";
    }

    private static string Capitalize(string value) => string.IsNullOrWhiteSpace(value)
        ? value
        : char.ToUpperInvariant(value[0]) + value[1..];
}
