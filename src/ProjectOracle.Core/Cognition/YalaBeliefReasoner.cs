using ProjectOracle.Domain;

namespace ProjectOracle.Cognition;

/// <summary>
/// Small, inspectable belief utilities for Brain Slice 6. Claims remain separate
/// from settled knowledge and can gain confidence without silently becoming truth.
/// </summary>
public static class YalaBeliefReasoner
{
    public static string ConfidenceLabel(double confidence) => Math.Clamp(confidence, 0.0, 1.0) switch
    {
        >= 0.90 => "very strong",
        >= 0.70 => "strong",
        >= 0.45 => "moderate",
        >= 0.20 => "tentative",
        > 0.0 => "very weak",
        _ => "none"
    };

    public static YalaBeliefState? StrongestSpeakerClaim(YalaCognitionState cognition) =>
        (cognition.Beliefs ?? [])
            .Where(item => item.Source.Equals(YalaKnowledgeSource.ClaimedByAnother, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.Confidence)
            .ThenByDescending(item => item.LastConsideredDecision)
            .FirstOrDefault();

    public static bool IsSettled(YalaBeliefState belief) =>
        belief.Status.Equals("known", StringComparison.OrdinalIgnoreCase) && belief.Confidence >= 0.90;

    public static bool IsClaim(YalaBeliefState belief) =>
        belief.Source.Equals(YalaKnowledgeSource.ClaimedByAnother, StringComparison.OrdinalIgnoreCase);
}
