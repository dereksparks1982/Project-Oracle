namespace ProjectOracle.Cognition.Inheritance;

/// <summary>
/// Reusable cognitive inheritance contract for future created minds. The mind
/// architecture may be shared while identity, autobiography, knowledge, learned
/// procedures, dispositions, capabilities, and world authority remain explicit.
/// </summary>
public sealed record OracleMindInheritanceManifest(
    string CreatorKey,
    string ChildKey,
    string ArchitectureKey,
    IReadOnlyList<string> KnowledgeKeys,
    IReadOnlyList<string> ProceduralKnowledgeKeys,
    IReadOnlyList<string> DispositionKeys,
    IReadOnlyList<string> CapabilityKeys,
    int GrantedAuthorityUnits,
    IReadOnlyList<string> Lineage);

public static class OracleMindInheritancePolicy
{
    public static OracleMindInheritanceManifest CreateLesserMind(
        string creatorKey,
        string childKey,
        string architectureKey,
        int creatorAuthorityUnits,
        int requestedChildAuthorityUnits,
        IEnumerable<string>? knowledge = null,
        IEnumerable<string>? proceduralKnowledge = null,
        IEnumerable<string>? dispositions = null,
        IEnumerable<string>? capabilities = null,
        IEnumerable<string>? creatorLineage = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(creatorKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(childKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(architectureKey);
        if (creatorAuthorityUnits <= 0) throw new ArgumentOutOfRangeException(nameof(creatorAuthorityUnits));
        if (requestedChildAuthorityUnits < 0) throw new ArgumentOutOfRangeException(nameof(requestedChildAuthorityUnits));
        if (requestedChildAuthorityUnits >= creatorAuthorityUnits)
        {
            throw new InvalidOperationException(
                $"Creation power ceiling: {creatorKey} cannot create {childKey} with authority equal to or greater than the creator.");
        }

        List<string> lineage = (creatorLineage ?? [])
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .ToList();
        if (lineage.Count == 0 || !lineage[^1].Equals(creatorKey, StringComparison.OrdinalIgnoreCase)) lineage.Add(creatorKey.Trim());
        lineage.Add(childKey.Trim());

        return new OracleMindInheritanceManifest(
            creatorKey.Trim(),
            childKey.Trim(),
            architectureKey.Trim(),
            Distinct(knowledge),
            Distinct(proceduralKnowledge),
            Distinct(dispositions),
            Distinct(capabilities),
            requestedChildAuthorityUnits,
            lineage);
    }

    public static void DemandBelowCreator(int creatorAuthorityUnits, OracleMindInheritanceManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        if (creatorAuthorityUnits <= 0 || manifest.GrantedAuthorityUnits >= creatorAuthorityUnits)
        {
            throw new InvalidOperationException("A created mind must remain strictly below its creator's world-authority ceiling.");
        }
    }

    private static IReadOnlyList<string> Distinct(IEnumerable<string>? items) =>
        (items ?? [])
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item, StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
