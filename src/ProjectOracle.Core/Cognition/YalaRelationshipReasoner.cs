using ProjectOracle.Domain;

namespace ProjectOracle.Cognition;

public static class YalaRelationshipReasoner
{
    public static YalaRelationshipState? Find(
        YalaCognitionState cognition,
        string subject,
        string relation,
        string? obj = null)
    {
        IEnumerable<YalaRelationshipState> query = (cognition.Relationships ?? [])
            .Where(item => item.Subject.Equals(subject, StringComparison.OrdinalIgnoreCase))
            .Where(item => item.Relation.Equals(relation, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(obj))
        {
            query = query.Where(item => item.Object.Equals(obj, StringComparison.OrdinalIgnoreCase));
        }
        return query.OrderByDescending(item => item.LastConsideredDecision).FirstOrDefault();
    }

    public static bool IsSettled(YalaRelationshipState relation) =>
        relation.Status.Equals("known", StringComparison.OrdinalIgnoreCase) && relation.Confidence >= 0.90;

    public static string Describe(YalaRelationshipState relation)
    {
        string confidence = YalaBeliefReasoner.ConfidenceLabel(relation.Confidence);
        return IsSettled(relation)
            ? $"I hold the relationship {relation.Subject} {relation.Relation} {relation.Object} as known."
            : $"I remember the relationship claim {relation.Subject} {relation.Relation} {relation.Object}. Its present confidence is {confidence}, and I do not hold it as settled truth.";
    }
}
