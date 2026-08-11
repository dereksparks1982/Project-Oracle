namespace ProjectOracle.Cognition;

public sealed record YalaKnowledgeProposition(
    string Proposition,
    string Source,
    double Confidence,
    bool Settled);
