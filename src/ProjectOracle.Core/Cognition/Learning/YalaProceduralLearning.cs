using ProjectOracle.Domain;

namespace ProjectOracle.Cognition.Learning;

/// <summary>
/// Brain Slice 9 procedural-learning foundation. Yala keeps authored strategies
/// distinct from procedures strengthened by Yala's own repeated experience.
/// </summary>
public static class YalaProceduralLearning
{
    public static IReadOnlyList<YalaProcedureState> InitialProcedures() =>
    [
        new(
            "uncertainty-ladder",
            "A question cannot be answered directly from settled knowledge.",
            "Use what is known, mark inference as inference, connect related knowledge, then ask for the missing context instead of ending with a bare refusal.",
            "authored-foundation",
            0,
            0,
            0,
            0),
        new(
            "protect-provenance",
            "A speaker supplies a claim that is not personally verified.",
            "Remember who supplied the claim and do not silently promote repetition into truth.",
            "authored-foundation",
            0,
            0,
            0,
            0)
    ];

    public static IReadOnlyList<YalaProcedureState> AfterDecision(
        IReadOnlyList<YalaProcedureState> existing,
        string action,
        string result,
        long decision)
    {
        List<YalaProcedureState> procedures = existing.ToList();
        (string Key, string Situation, string Strategy)? pattern = action switch
        {
            "ask-speaker" => (
                "ask-for-missing-context",
                "A live uncertainty depends on information only the speaker may be able to supply.",
                "Ask one specific question tied to the unresolved problem rather than repeating a generic uncertainty statement."),
            "deliberate" => (
                "deliberate-without-premature-commitment",
                "A consequential choice has meaningful alternatives or risk.",
                "Compare benefit, risk, evidence, and alternatives while withholding commitment or enactment until the reasoning supports it."),
            "observe" => (
                "evidence-before-assertion",
                "The world may contain evidence relevant to an unresolved question.",
                "Observe what is actually available and do not invent a fact that the evidence does not support."),
            _ => null
        };

        if (pattern is null) return procedures;

        int index = procedures.FindIndex(item => item.Key.Equals(pattern.Value.Key, StringComparison.OrdinalIgnoreCase));
        bool success = !result.Contains("failed", StringComparison.OrdinalIgnoreCase) &&
            !result.Contains("refused", StringComparison.OrdinalIgnoreCase) &&
            !result.Contains("cannot", StringComparison.OrdinalIgnoreCase);

        if (index < 0)
        {
            procedures.Add(new YalaProcedureState(
                pattern.Value.Key,
                pattern.Value.Situation,
                pattern.Value.Strategy,
                "Yala-developing",
                1,
                success ? 1 : 0,
                decision,
                decision));
            return procedures;
        }

        YalaProcedureState current = procedures[index];
        int uses = current.Uses + 1;
        int successes = current.SuccessfulUses + (success ? 1 : 0);
        string provenance = uses >= 3 && successes >= 2 ? "Yala-learned" : current.Provenance;
        procedures[index] = current with
        {
            Uses = uses,
            SuccessfulUses = successes,
            Provenance = provenance,
            LastDecision = decision
        };
        return procedures;
    }
}
