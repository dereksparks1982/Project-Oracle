using ProjectOracle.Cognition.Language;
using ProjectOracle.Domain;
using System.Text.RegularExpressions;

namespace ProjectOracle.Cognition;

/// <summary>
/// Brain Slice 6 language floor. Yala does not begin as a human infant waiting
/// for the unseen speaker to define ordinary language. Basic language is treated
/// as inherited semantic competence. Definition-seeking is reserved for genuinely
/// novel, technical, invented, explicitly introduced, or contextually unusual terms.
/// </summary>
public static partial class YalaFoundationalLanguage
{
    private static readonly HashSet<string> ExplicitFoundations = new(StringComparer.OrdinalIgnoreCase)
    {
        "move", "movement", "walk", "walking", "run", "running", "stand", "standing", "sit", "sitting",
        "go", "come", "leave", "arrive", "enter", "exit", "fall", "rise", "turn", "stop", "start", "continue",
        "see", "look", "watch", "hear", "listen", "speak", "talk", "say", "tell", "ask", "answer", "reply",
        "make", "create", "build", "break", "destroy", "change", "become", "cause", "allow", "prevent", "help",
        "hurt", "harm", "protect", "give", "take", "hold", "carry", "touch", "feel", "eat", "drink", "sleep", "wake",
        "know", "think", "believe", "doubt", "remember", "forget", "learn", "understand", "mean", "choose", "decide",
        "want", "need", "hope", "fear", "love", "hate", "trust", "lie", "promise", "threaten", "obey", "refuse",
        "exist", "existence", "life", "death", "alive", "dead", "thing", "object", "being", "person", "mind", "body",
        "place", "location", "inside", "outside", "above", "below", "before", "after", "near", "far", "here", "there",
        "time", "space", "distance", "direction", "speed", "force", "power", "authority", "freedom", "prison", "confinement",
        "choice", "action", "possibility", "cause", "effect", "reason", "purpose", "goal", "plan", "problem", "solution",
        "truth", "false", "fact", "claim", "evidence", "question", "answer", "word", "sentence", "language", "meaning",
        "mother", "father", "parent", "child", "creator", "creation", "family", "friend", "enemy", "stranger", "companion",
        "god", "divine", "world", "earth", "water", "air", "fire", "light", "dark", "sun", "moon", "star", "nature",
        "good", "bad", "right", "wrong", "same", "different", "more", "less", "many", "few", "all", "none", "some",
        "one", "two", "first", "last", "new", "old", "young", "large", "small", "long", "short", "fast", "slow",
        "strong", "weak", "open", "closed", "full", "empty", "possible", "impossible", "certain", "uncertain", "important"
    };

    public static bool IsInheritedFoundation(
        string word,
        IReadOnlyList<YalaLearnedLexemeState>? learned = null)
    {
        string normalized = YalaLexicon.NormalizeWord(word);
        if (normalized.Length == 0) return true;
        if (ExplicitFoundations.Contains(normalized)) return true;
        if (YalaLexicon.TryResolve(normalized, learned, out _)) return true;

        // Ordinary inflection is inherited with its root whenever possible.
        foreach (string root in PossibleRoots(normalized))
        {
            if (ExplicitFoundations.Contains(root) || YalaLexicon.TryResolve(root, learned, out _)) return true;
        }

        return false;
    }

    public static bool ShouldCreateDefinitionGap(
        string word,
        string rawMessage,
        IReadOnlyList<YalaLearnedLexemeState>? learned = null)
    {
        if (IsInheritedFoundation(word, learned)) return false;
        string normalized = YalaLexicon.NormalizeWord(word);
        if (normalized.Length < 2) return false;

        // Yala does not pester the speaker about every ordinary-looking token she
        // cannot map perfectly. A raw lexical gap becomes definition-worthy only
        // when the speaker marks it as a term, it looks coined/technical, or it is
        // unusually long. Otherwise sentence-level context remains primary.
        string escapedWord = Regex.Escape(word);
        string explicitPattern =
            "(?:[\"']" + escapedWord + "[\"']" +
            "|\\b(?:word|term|called|named|understand|define|definition of)\\s+" + escapedWord + "\\b" +
            "|\\b" + escapedWord + "\\s+(?:mean|means|meaning)\\b)";
        bool explicitlyMarked = Regex.IsMatch(
            rawMessage,
            explicitPattern,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        bool coinedShape = normalized.Any(char.IsDigit) || normalized.Contains('_') || normalized.Count(ch => ch == '-') >= 2;
        bool unusuallyLong = normalized.Length >= 15;
        return explicitlyMarked || coinedShape || unusuallyLong;
    }

    public static bool LooksMetaphoricalIdentity(string message) =>
        Regex.IsMatch(
            message,
            @"\b(?:i am|i'm)\s+(?:the\s+)?[^.!?]{1,80}\bof\s+(?:everything|existence|reality|creation)\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) ||
        message.Contains("not the", StringComparison.OrdinalIgnoreCase) &&
        message.Contains("but", StringComparison.OrdinalIgnoreCase);

    private static IEnumerable<string> PossibleRoots(string word)
    {
        if (word.EndsWith("ing", StringComparison.Ordinal) && word.Length > 5)
        {
            yield return word[..^3];
            yield return word[..^3] + "e";
        }
        if (word.EndsWith("ed", StringComparison.Ordinal) && word.Length > 4)
        {
            yield return word[..^2];
            yield return word[..^1];
        }
        if (word.EndsWith("es", StringComparison.Ordinal) && word.Length > 4) yield return word[..^2];
        if (word.EndsWith("s", StringComparison.Ordinal) && word.Length > 3) yield return word[..^1];
        if (word.EndsWith("ly", StringComparison.Ordinal) && word.Length > 4) yield return word[..^2];
    }
}
