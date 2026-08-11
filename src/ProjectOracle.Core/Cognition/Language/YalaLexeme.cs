namespace ProjectOracle.Cognition.Language;

public sealed record YalaLexeme(
    string Word,
    string PartOfSpeech,
    string BasicMeaning,
    IReadOnlyList<string> Related,
    IReadOnlyList<string> Opposites,
    IReadOnlyList<string> ConceptRelations);
