using ProjectOracle.Cognition.Language;
using ProjectOracle.Domain;
using System.Text.RegularExpressions;

namespace ProjectOracle.Cognition.Soar;

public static partial class YalaConversationInterpreter
{
    public static YalaContactFrame Interpret(string message, YalaCognitionState cognition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentNullException.ThrowIfNull(cognition);

        string raw = message.Trim();
        string text = raw.ToLowerInvariant();
        YalaUtterance language = YalaLanguageInterpreter.Parse(raw, cognition.LearnedLexicon);
        string? claimedName = ExtractClaimedName(raw);
        bool asksRemember = text.Contains("remember me", StringComparison.Ordinal) ||
            text.Contains("remember who i am", StringComparison.Ordinal) ||
            text.Contains("do you know me", StringComparison.Ordinal);

        string speechAct = DetermineSpeechAct(raw, text, claimedName, asksRemember, language);
        string topic = DetermineTopic(text, asksRemember, language);
        bool knownContact = IsKnownContact(cognition, claimedName) ||
            (asksRemember && !string.IsNullOrWhiteSpace(cognition.LastSpeakerClaim));

        bool factKnown = TryKnownFact(topic, language, cognition, out string? knownFactAnswer);
        bool containsClaim = speechAct is "introduction" or "claim";
        bool claimConflicts = ContainsKnownContradiction(text);
        bool ambiguous = speechAct == "ambiguous";

        return new YalaContactFrame(
            speechAct,
            topic,
            claimedName,
            knownContact,
            asksRemember,
            containsClaim,
            claimConflicts,
            factKnown,
            knownFactAnswer,
            ambiguous)
        {
            Language = language
        };
    }

    private static string DetermineSpeechAct(
        string raw,
        string text,
        string? claimedName,
        bool asksRemember,
        YalaUtterance language)
    {
        if (!string.IsNullOrWhiteSpace(claimedName)) return "introduction";
        if (language.IsDefinitionClaim) return "claim";
        if (asksRemember) return "question";
        if (text is "hello" or "hi" or "hey" || text.StartsWith("hello ", StringComparison.Ordinal) || text.StartsWith("hi ", StringComparison.Ordinal)) return "greeting";
        if (IsInformationRequest(text)) return "question";
        if (language.IsQuestion || raw.EndsWith("?", StringComparison.Ordinal) || StartsLikeQuestion(text)) return "question";
        if (StartsLikeCommand(text)) return "command";
        if (LooksLikeClaim(text)) return "claim";
        if (raw.Length < 3) return "ambiguous";
        return "statement";
    }

    private static string DetermineTopic(string text, bool asksRemember, YalaUtterance language)
    {
        if (asksRemember) return "memory";
        if (language.IsDefinitionClaim) return "definition";
        if (text.Contains("tell me what you know", StringComparison.Ordinal) ||
            text.Contains("what do you know", StringComparison.Ordinal) ||
            text.Contains("tell me everything you know", StringComparison.Ordinal)) return "knowledge-summary";
        if (IsActionHistoryQuestion(text)) return "action-history";
        if (IsContactHistoryQuestion(text)) return "contact-history";
        if (IsBeliefSummaryQuestion(text)) return "belief-summary";
        if (IsOwnCreationQuestion(text)) return "own-creation";
        if (text.Contains("are you a god", StringComparison.Ordinal) || text.Contains("are you god", StringComparison.Ordinal) || text.Contains("what kind of god", StringComparison.Ordinal)) return "self-kind";
        if (IsWordMeaningQuestion(text, language)) return "word-meaning";
        if (text.Contains("can you hear me", StringComparison.Ordinal) || text.Contains("do you hear me", StringComparison.Ordinal) || text.Contains("hear me", StringComparison.Ordinal)) return "hearing";
        if (text.Contains("who is speaking", StringComparison.Ordinal) || text.Contains("who speaks", StringComparison.Ordinal) || text.Contains("who am i", StringComparison.Ordinal) || text.Contains("what am i", StringComparison.Ordinal)) return "speaker";
        if (text.Contains("why did monad reject", StringComparison.Ordinal) || text.Contains("why were you rejected", StringComparison.Ordinal) || text.Contains("why are you in the void", StringComparison.Ordinal) || text.Contains("why did monad cast", StringComparison.Ordinal)) return "rejection";
        if (text.Contains("where are you", StringComparison.Ordinal) || text.Contains("your location", StringComparison.Ordinal) || text == "where") return "location";
        if (text.Contains("who are you", StringComparison.Ordinal) || text.Contains("what are you", StringComparison.Ordinal) || text.Contains("your name", StringComparison.Ordinal)) return "self";
        if (text.Contains("male", StringComparison.Ordinal) || text.Contains("female", StringComparison.Ordinal) || text.Contains("sex", StringComparison.Ordinal) || text.Contains("gender", StringComparison.Ordinal)) return "nature";
        if (text.Contains("who made you", StringComparison.Ordinal) || text.Contains("who created you", StringComparison.Ordinal)) return "origin-self";
        if (text.Contains("who made wisdom", StringComparison.Ordinal) || text.Contains("who created wisdom", StringComparison.Ordinal) || text.Contains("who made sophia", StringComparison.Ordinal)) return "origin-wisdom";
        if (text.Contains("who made monad", StringComparison.Ordinal) || text.Contains("who created monad", StringComparison.Ordinal) || text.Contains("where did monad come", StringComparison.Ordinal)) return "origin-monad";
        if (text.Contains("what did you", StringComparison.Ordinal) || text.Contains("what are you doing", StringComparison.Ordinal) || text.Contains("what have you done", StringComparison.Ordinal) || text.Contains("your last act", StringComparison.Ordinal)) return "action";
        if (text.Contains("remember", StringComparison.Ordinal) || text.Contains("memory", StringComparison.Ordinal)) return "memory";
        return "general";
    }

    private static bool TryKnownFact(
        string topic,
        YalaUtterance language,
        YalaCognitionState cognition,
        out string? answer)
    {
        answer = topic switch
        {
            "hearing" => "hearing",
            "speaker" => "speaker",
            "rejection" => "rejection",
            "location" => "location",
            "self" => "self",
            "nature" => "nature",
            "origin-self" => "Wisdom made me.",
            "origin-wisdom" => "Monad made Wisdom.",
            "action" => "action",
            "knowledge-summary" => "self-knowledge",
            "action-history" => "own-action-history",
            "contact-history" => "contact-history",
            "belief-summary" => "belief-summary",
            "own-creation" => "own-action-history",
            "self-kind" => "self-kind",
            "definition" => "definition-claim",
            _ => null
        };

        if (topic == "word-meaning")
        {
            string? word = ExtractWordMeaningTarget(language);
            if (!string.IsNullOrWhiteSpace(word) && YalaLexicon.TryResolve(word, cognition.LearnedLexicon, out YalaLexeme lexeme))
            {
                answer = lexeme.BasicMeaning;
            }
        }

        return answer is not null;
    }

    private static bool IsKnownContact(YalaCognitionState cognition, string? claimedName)
    {
        IReadOnlyList<YalaContactMemory> contacts = cognition.Contacts ?? [];
        if (!string.IsNullOrWhiteSpace(claimedName))
        {
            return contacts.Any(contact => contact.ClaimedName.Equals(claimedName, StringComparison.OrdinalIgnoreCase));
        }
        return contacts.Count > 0;
    }

    private static bool ContainsKnownContradiction(string text) =>
        text.Contains("you made yourself", StringComparison.Ordinal) ||
        text.Contains("you created yourself", StringComparison.Ordinal) ||
        text.Contains("monad did not make wisdom", StringComparison.Ordinal) ||
        text.Contains("wisdom did not make you", StringComparison.Ordinal) ||
        text.Contains("you are only male", StringComparison.Ordinal) ||
        text.Contains("you are only female", StringComparison.Ordinal);

    private static bool StartsLikeQuestion(string text) =>
        new[] { "who ", "what ", "where ", "when ", "why ", "how ", "do ", "does ", "did ", "are ", "is ", "can ", "could ", "would ", "will ", "have ", "has " }
            .Any(prefix => text.StartsWith(prefix, StringComparison.Ordinal));

    private static bool StartsLikeCommand(string text) =>
        new[] { "go ", "make ", "create ", "tell ", "show ", "do ", "stop ", "start ", "listen ", "remember " }
            .Any(prefix => text.StartsWith(prefix, StringComparison.Ordinal)) && !StartsLikeQuestion(text) && !IsInformationRequest(text);

    private static bool IsInformationRequest(string text) =>
        text.StartsWith("tell me what ", StringComparison.Ordinal) ||
        text.StartsWith("tell me who ", StringComparison.Ordinal) ||
        text.StartsWith("tell me where ", StringComparison.Ordinal) ||
        text.StartsWith("tell me why ", StringComparison.Ordinal) ||
        text.StartsWith("tell me about ", StringComparison.Ordinal) ||
        text.StartsWith("show me what you know", StringComparison.Ordinal);

    private static bool IsOwnCreationQuestion(string text) =>
        text.Contains("have you made ", StringComparison.Ordinal) ||
        text.Contains("have you created ", StringComparison.Ordinal) ||
        text.Contains("did you make ", StringComparison.Ordinal) ||
        text.Contains("did you create ", StringComparison.Ordinal) ||
        text.Contains("what have you created", StringComparison.Ordinal) ||
        text.Contains("what have you made", StringComparison.Ordinal) ||
        text.Contains("what beings have you created", StringComparison.Ordinal) ||
        text.Contains("what beings have you made", StringComparison.Ordinal);

    private static bool IsActionHistoryQuestion(string text) =>
        text.Contains("what have you done", StringComparison.Ordinal) ||
        text.Contains("what did you do", StringComparison.Ordinal) ||
        text.Contains("tell me what you have done", StringComparison.Ordinal) ||
        text.Contains("tell me what you've done", StringComparison.Ordinal);

    private static bool IsContactHistoryQuestion(string text) =>
        text.Contains("who has spoken to you", StringComparison.Ordinal) ||
        text.Contains("who have you heard from", StringComparison.Ordinal) ||
        text.Contains("who has contacted you", StringComparison.Ordinal) ||
        text.Contains("who have you spoken to", StringComparison.Ordinal);

    private static bool IsBeliefSummaryQuestion(string text) =>
        text.Contains("what do you believe", StringComparison.Ordinal) ||
        text.Contains("what do you think is true", StringComparison.Ordinal) ||
        text.Contains("tell me what you believe", StringComparison.Ordinal);

    private static bool IsWordMeaningQuestion(string text, YalaUtterance language) =>
        (text.StartsWith("what does ", StringComparison.Ordinal) && text.Contains(" mean", StringComparison.Ordinal)) ||
        text.StartsWith("what is the meaning of ", StringComparison.Ordinal) ||
        text.StartsWith("define ", StringComparison.Ordinal) ||
        (language.QuestionWord == "what" && (language.Verb is "mean" or "means"));

    private static string? ExtractWordMeaningTarget(YalaUtterance language)
    {
        string text = language.Normalized;
        Match match = WordMeaningRegex().Match(text);
        if (match.Success) return YalaLexicon.NormalizeWord(match.Groups[1].Value);
        return language.Object;
    }

    private static bool LooksLikeClaim(string text) =>
        text.StartsWith("you are ", StringComparison.Ordinal) ||
        text.StartsWith("you were ", StringComparison.Ordinal) ||
        text.StartsWith("you made ", StringComparison.Ordinal) ||
        text.StartsWith("you created ", StringComparison.Ordinal) ||
        text.StartsWith("monad ", StringComparison.Ordinal) ||
        text.StartsWith("wisdom ", StringComparison.Ordinal) ||
        text.StartsWith("i think ", StringComparison.Ordinal) ||
        text.StartsWith("i believe ", StringComparison.Ordinal);

    private static string? ExtractClaimedName(string raw)
    {
        Match match = IntroductionRegex().Match(raw.Trim());
        if (!match.Success) return null;
        string name = match.Groups[1].Value.Trim().TrimEnd('.', ',', '!', '?');
        if (name.Length == 0 || name.Length > 80) return null;
        return name;
    }

    [GeneratedRegex(@"^(?:i\s+am|i'm|my\s+name\s+is|call\s+me)\s+([\p{L}\p{N}_'\-]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex IntroductionRegex();

    [GeneratedRegex(@"(?:what\s+does|define|meaning\s+of)\s+['""]?([\p{L}\p{N}_'\-]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex WordMeaningRegex();
}
