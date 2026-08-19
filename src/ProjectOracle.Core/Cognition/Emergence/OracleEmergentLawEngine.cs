using ProjectOracle.Domain;

namespace ProjectOracle.Cognition.Emergence;

/// <summary>
/// Foundation for worlds whose large-scale behavior emerges from compact local laws.
/// A law available to the engine is only a possibility until an authorised in-world
/// creator establishes it. Rule 30 is included strictly as a laboratory demonstration
/// of local deterministic emergence, never as an automatic law of Project Oracle.
/// </summary>
public sealed record OracleLawDefinition(
    string Key,
    string Domain,
    string Name,
    string Description,
    string RuleKind,
    int RequiredAuthorityUnits,
    bool Local,
    bool Deterministic,
    bool LaboratoryOnly);

public sealed record OracleLawExperimentResult(
    string LawKey,
    string InitialState,
    IReadOnlyList<string> Generations,
    string Interpretation);

public static class OracleEmergentLawCatalog
{
    public static IReadOnlyList<OracleLawDefinition> LaboratoryDemonstrations { get; } =
    [
        new(
            "rule-30-demo",
            "emergence",
            "Rule 30 cellular automaton",
            "A compact one-dimensional local rule used to demonstrate how repeated simple interactions can generate complex structure.",
            "elementary-cellular-automaton",
            RequiredAuthorityUnits: 0,
            Local: true,
            Deterministic: true,
            LaboratoryOnly: true)
    ];

    public static OracleLawDefinition Rule30 => LaboratoryDemonstrations[0];
}

public static class OracleLawAuthorityPolicy
{
    public static void DemandMayEstablishLaw(
        string actorKey,
        int actorAuthorityUnits,
        OracleLawDefinition law)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actorKey);
        ArgumentNullException.ThrowIfNull(law);
        if (actorAuthorityUnits < 0) throw new ArgumentOutOfRangeException(nameof(actorAuthorityUnits));
        if (law.LaboratoryOnly)
        {
            throw new InvalidOperationException($"{law.Name} is a laboratory demonstration and cannot be established as a world law.");
        }
        if (law.RequiredAuthorityUnits > actorAuthorityUnits)
        {
            throw new InvalidOperationException(
                $"{actorKey} does not possess enough world authority to establish {law.Name}.");
        }
    }

    public static bool IsWithinAuthority(int actorAuthorityUnits, int requiredAuthorityUnits) =>
        actorAuthorityUnits >= 0 && requiredAuthorityUnits >= 0 && requiredAuthorityUnits <= actorAuthorityUnits;
}

public static class Rule30Laboratory
{
    public static OracleLawExperimentResult RunSingleSeed(int width, int generations)
    {
        if (width < 3) throw new ArgumentOutOfRangeException(nameof(width));
        if (generations < 1) throw new ArgumentOutOfRangeException(nameof(generations));

        bool[] state = new bool[width];
        state[width / 2] = true;
        List<string> rows = [Render(state)];
        for (int i = 1; i < generations; i++)
        {
            state = Step(state);
            rows.Add(Render(state));
        }

        return new OracleLawExperimentResult(
            OracleEmergentLawCatalog.Rule30.Key,
            rows[0],
            rows,
            "Demonstration only: simple local deterministic rules can produce unexpectedly complicated global patterns.");
    }

    public static bool[] Step(IReadOnlyList<bool> current)
    {
        ArgumentNullException.ThrowIfNull(current);
        if (current.Count < 3) throw new ArgumentException("Rule 30 requires at least three cells.", nameof(current));

        bool[] next = new bool[current.Count];
        for (int i = 0; i < current.Count; i++)
        {
            bool left = i > 0 && current[i - 1];
            bool center = current[i];
            bool right = i + 1 < current.Count && current[i + 1];
            next[i] = NextCell(left, center, right);
        }
        return next;
    }

    public static bool NextCell(bool left, bool center, bool right)
    {
        int neighborhood = (left ? 4 : 0) | (center ? 2 : 0) | (right ? 1 : 0);
        const int rule = 30;
        return ((rule >> neighborhood) & 1) == 1;
    }

    public static string Render(IEnumerable<bool> cells) =>
        string.Concat(cells.Select(cell => cell ? '█' : '·'));
}
