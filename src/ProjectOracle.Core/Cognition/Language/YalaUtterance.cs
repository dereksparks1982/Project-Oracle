namespace ProjectOracle.Cognition.Language;

public sealed record YalaUtterance(
    string Raw,
    string Normalized,
    bool IsQuestion,
    bool Negated,
    string? QuestionWord,
    string? Subject,
    string? Verb,
    string? Object,
    IReadOnlyList<string> UnknownWords,
    string? DefinedWord = null,
    string? ProposedDefinition = null)
{
    public bool IsDefinitionClaim => !string.IsNullOrWhiteSpace(DefinedWord) && !string.IsNullOrWhiteSpace(ProposedDefinition);
}
