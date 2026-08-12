using ProjectOracle.Domain;

namespace ProjectOracle.Cognition;

public static class YalaDialogueContext
{
    public static YalaDialogueTurnState? Latest(YalaCognitionState cognition) =>
        cognition.Dialogue?.OrderByDescending(turn => turn.Sequence).FirstOrDefault();

    public static YalaDialogueTurnState? LatestMeaningful(YalaCognitionState cognition) =>
        cognition.Dialogue?
            .Where(turn => !string.Equals(turn.Topic, "greeting", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(turn => turn.Sequence)
            .FirstOrDefault();

    public static string? ResolveRecentEntity(YalaCognitionState cognition)
    {
        foreach (YalaDialogueTurnState turn in (cognition.Dialogue ?? []).OrderByDescending(turn => turn.Sequence))
        {
            foreach (string? value in new[] { turn.Object, turn.Subject })
            {
                if (IsEntity(value)) return CanonicalEntity(value!);
            }
        }
        return null;
    }

    public static string? ResolveRecentAction(YalaCognitionState cognition) =>
        LatestMeaningful(cognition)?.Verb;

    public static bool IsEntity(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.Trim().ToLowerInvariant() is
            "yala" or "gaia" or "wisdom" or "sophia" or "monad" or "adam" or "time";

    public static string CanonicalEntity(string value) => value.Trim().ToLowerInvariant() switch
    {
        "sophia" => "Wisdom",
        "wisdom" => "Wisdom",
        "yala" => "Yala",
        "gaia" => "Gaia",
        "monad" => "Monad",
        "adam" => "Adam",
        "time" => "Time",
        _ => value.Trim()
    };
}
