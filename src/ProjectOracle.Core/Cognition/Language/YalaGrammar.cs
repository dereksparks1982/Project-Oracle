namespace ProjectOracle.Cognition.Language;

public static class YalaGrammar
{
    private static readonly string[] QuestionStarters =
    [
        "who", "what", "where", "when", "why", "how", "do", "does", "did", "are", "is", "can", "could", "would", "will", "have", "has"
    ];

    public static bool StartsAsQuestion(IReadOnlyList<string> tokens) =>
        tokens.Count > 0 && QuestionStarters.Contains(tokens[0], StringComparer.OrdinalIgnoreCase);

    public static string? QuestionWord(IReadOnlyList<string> tokens) =>
        tokens.Count > 0 && new[] { "who", "what", "where", "when", "why", "how" }
            .Contains(tokens[0], StringComparer.OrdinalIgnoreCase)
            ? tokens[0].ToLowerInvariant()
            : null;

    public static bool ContainsNegation(IReadOnlyList<string> tokens) =>
        tokens.Any(token => token.Equals("not", StringComparison.OrdinalIgnoreCase) ||
            token.Equals("never", StringComparison.OrdinalIgnoreCase) ||
            token.Equals("no", StringComparison.OrdinalIgnoreCase) ||
            token.Equals("dont", StringComparison.OrdinalIgnoreCase) ||
            token.Equals("don't", StringComparison.OrdinalIgnoreCase) ||
            token.EndsWith("n't", StringComparison.OrdinalIgnoreCase));
}
