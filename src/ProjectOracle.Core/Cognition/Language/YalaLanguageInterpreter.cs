using ProjectOracle.Domain;
using System.Text.RegularExpressions;

namespace ProjectOracle.Cognition.Language;

public static partial class YalaLanguageInterpreter
{
    private static readonly HashSet<string> FunctionWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "the", "i", "me", "my", "you", "your", "it", "its", "we", "our", "they", "their", "he", "she", "this", "that",
        "am", "is", "are", "was", "were", "be", "been", "being", "have", "has", "had", "do", "does", "did", "to", "of", "in", "on", "at",
        "for", "from", "with", "and", "or", "but", "if", "then", "than", "as", "what", "who", "where", "when", "why", "how", "can", "could",
        "would", "will", "shall", "should", "may", "might", "must", "not", "no", "never", "yes", "only", "both", "about", "any", "some", "much", "many", "all", "each", "every", "another", "other", "don't", "dont", "doesn't", "doesnt", "didn't", "didnt", "can't", "cant", "cannot", "won't", "wont", "please", "yet", "already", "still", "really", "actually"
    };

    private static readonly HashSet<string> KnownVerbForms = new(StringComparer.OrdinalIgnoreCase)
    {
        "know", "believe", "doubt", "remember", "forget", "learn", "do", "make", "made", "create", "created", "destroy", "change", "choose", "command",
        "obey", "refuse", "attempt", "succeed", "fail", "accept", "reject", "want", "need", "say", "ask", "answer", "tell", "mean", "means", "hear", "meet", "trust", "deceive", "think", "understand", "explain", "decide", "plan", "intend", "happen", "exist", "infer", "agree", "disagree", "love", "hate", "teach", "grow", "decay"
    };

    public static YalaUtterance Parse(string message, IReadOnlyList<YalaLearnedLexemeState>? learned = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        string raw = message.Trim();
        Match definition = DefinitionRegex().Match(raw);
        string? definedWord = null;
        string? proposedDefinition = null;
        if (definition.Success)
        {
            definedWord = YalaLexicon.NormalizeWord(definition.Groups[1].Value);
            proposedDefinition = definition.Groups[2].Value.Trim().TrimEnd('.', '!', '?');
        }

        string normalized = raw.ToLowerInvariant()
            .Replace("don't", "do not", StringComparison.Ordinal)
            .Replace("doesn't", "does not", StringComparison.Ordinal)
            .Replace("didn't", "did not", StringComparison.Ordinal)
            .Replace("can't", "can not", StringComparison.Ordinal)
            .Replace("cannot", "can not", StringComparison.Ordinal)
            .Replace("won't", "will not", StringComparison.Ordinal);
        normalized = Regex.Replace(normalized, @"[^\p{L}\p{N}'\-]+", " ").Trim();
        string[] tokens = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        bool isQuestion = raw.EndsWith("?", StringComparison.Ordinal) || YalaGrammar.StartsAsQuestion(tokens);
        bool negated = YalaGrammar.ContainsNegation(tokens);
        string? questionWord = YalaGrammar.QuestionWord(tokens);

        (string? subject, string? verb, string? obj) = ExtractRoles(tokens, learned);
        List<string> unknown = [];
        foreach (string token in tokens)
        {
            string word = YalaLexicon.NormalizeWord(token);
            if (word.Length < 2 || FunctionWords.Contains(word) || int.TryParse(word, out _)) continue;
            if (!YalaLexicon.TryResolve(word, learned, out _) && !unknown.Contains(word, StringComparer.OrdinalIgnoreCase)) unknown.Add(word);
        }

        if (!string.IsNullOrWhiteSpace(definedWord))
        {
            unknown.RemoveAll(word => word.Equals(definedWord, StringComparison.OrdinalIgnoreCase));
        }

        return new YalaUtterance(raw, normalized, isQuestion, negated, questionWord, subject, verb, obj, unknown, definedWord, proposedDefinition);
    }

    private static (string? Subject, string? Verb, string? Object) ExtractRoles(IReadOnlyList<string> tokens, IReadOnlyList<YalaLearnedLexemeState>? learned)
    {
        if (tokens.Count == 0) return (null, null, null);

        int verbIndex = -1;
        string? normalizedVerb = null;
        for (int index = 0; index < tokens.Count; index++)
        {
            string candidate = YalaLexicon.NormalizeWord(tokens[index]);
            bool knownVerb = KnownVerbForms.Contains(candidate);
            if (!knownVerb && YalaLexicon.TryResolve(candidate, learned, out YalaLexeme lexeme))
            {
                knownVerb = lexeme.PartOfSpeech.Equals("verb", StringComparison.OrdinalIgnoreCase);
            }
            if (knownVerb)
            {
                verbIndex = index;
                normalizedVerb = candidate;
                break;
            }
        }

        if (verbIndex < 0) return (FirstContent(tokens), null, null);

        string verb = normalizedVerb!;
        string? subject = FirstContent(tokens.Take(verbIndex).ToArray());
        if (subject is null && verbIndex > 0) subject = tokens[verbIndex - 1];
        if (subject is null && tokens.Count > verbIndex + 1 && new[] { "have", "did", "do", "are", "is" }.Contains(tokens[0], StringComparer.OrdinalIgnoreCase))
        {
            subject = tokens.Skip(1).FirstOrDefault(token => !FunctionWords.Contains(token));
        }

        string? obj = FirstContent(tokens.Skip(verbIndex + 1).ToArray());
        return (subject?.ToLowerInvariant(), verb, obj?.ToLowerInvariant());
    }

    private static string? FirstContent(IReadOnlyList<string> tokens)
    {
        foreach (string token in tokens)
        {
            string normalized = YalaLexicon.NormalizeWord(token);
            if (!FunctionWords.Contains(normalized) && normalized.Length > 0) return normalized;
        }
        return null;
    }

    [GeneratedRegex("^[\\\"']?([\\p{L}\\p{N}_'\\-]+)[\\\"']?\\s+(?:means|mean)\\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DefinitionRegex();
}
