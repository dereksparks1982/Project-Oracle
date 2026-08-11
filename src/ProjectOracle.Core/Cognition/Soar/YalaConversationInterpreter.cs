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
        string? claimedName = ExtractClaimedName(raw);
        bool asksRemember = text.Contains("remember me", StringComparison.Ordinal) ||
            text.Contains("remember who i am", StringComparison.Ordinal) ||
            text.Contains("do you know me", StringComparison.Ordinal);

        string speechAct = DetermineSpeechAct(raw, text, claimedName, asksRemember);
        string topic = DetermineTopic(text, asksRemember);
        bool knownContact = IsKnownContact(cognition, claimedName) ||
            (asksRemember && !string.IsNullOrWhiteSpace(cognition.LastSpeakerClaim));

        bool factKnown = TryKnownFact(topic, out string? knownFactAnswer);
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
            ambiguous);
    }

    private static string DetermineSpeechAct(string raw, string text, string? claimedName, bool asksRemember)
    {
        if (!string.IsNullOrWhiteSpace(claimedName)) return "introduction";
        if (asksRemember) return "question";
        if (text is "hello" or "hi" or "hey" || text.StartsWith("hello ", StringComparison.Ordinal) || text.StartsWith("hi ", StringComparison.Ordinal)) return "greeting";
        if (raw.EndsWith("?", StringComparison.Ordinal) || StartsLikeQuestion(text)) return "question";
        if (StartsLikeCommand(text)) return "command";
        if (LooksLikeClaim(text)) return "claim";
        if (raw.Length < 3) return "ambiguous";
        return "statement";
    }

    private static string DetermineTopic(string text, bool asksRemember)
    {
        if (asksRemember) return "memory";
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

    private static bool TryKnownFact(string topic, out string? answer)
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
            _ => null
        };
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
        new[] { "who ", "what ", "where ", "when ", "why ", "how ", "do ", "did ", "are ", "is ", "can ", "could ", "would ", "will " }
            .Any(prefix => text.StartsWith(prefix, StringComparison.Ordinal));

    private static bool StartsLikeCommand(string text) =>
        new[] { "go ", "make ", "create ", "tell ", "show ", "do ", "stop ", "start ", "listen ", "remember " }
            .Any(prefix => text.StartsWith(prefix, StringComparison.Ordinal)) && !StartsLikeQuestion(text);

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
}
