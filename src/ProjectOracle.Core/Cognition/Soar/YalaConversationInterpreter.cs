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
        string text = NormalizedConversationText(raw);
        YalaUtterance language = YalaLanguageInterpreter.Parse(raw, cognition.LearnedLexicon);
        string? claimedName = ExtractClaimedName(raw);
        bool asksRemember = text.Contains("remember me", StringComparison.Ordinal) ||
            text.Contains("remember who i am", StringComparison.Ordinal) ||
            text.Contains("do you know me", StringComparison.Ordinal);

        TopicResolution topic = DetermineTopic(text, asksRemember, language, cognition);
        string speechAct = DetermineSpeechAct(raw, text, claimedName, asksRemember, language);
        bool knownContact = IsKnownContact(cognition, claimedName) ||
            (asksRemember && !string.IsNullOrWhiteSpace(cognition.LastSpeakerClaim));

        bool factKnown = TryKnownFact(topic.Topic, language, cognition, out string? knownFactAnswer);
        bool containsClaim = speechAct is "introduction" or "claim";
        bool claimConflicts = ContainsKnownContradiction(text);
        bool ambiguous = speechAct == "ambiguous";

        return new YalaContactFrame(
            speechAct,
            topic.Topic,
            claimedName,
            knownContact,
            asksRemember,
            containsClaim,
            claimConflicts,
            factKnown,
            knownFactAnswer,
            ambiguous)
        {
            Language = language,
            ResolvedSubject = topic.Subject
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

    private static TopicResolution DetermineTopic(
        string text,
        bool asksRemember,
        YalaUtterance language,
        YalaCognitionState cognition)
    {
        if (asksRemember) return new("memory", null);
        if (language.IsDefinitionClaim) return new("definition", language.DefinedWord);

        if (IsBareWhy(text) && TryResolvePreviousCreationQuestion(cognition, out string? priorCreation))
        {
            return new("follow-up-why-creation", priorCreation);
        }

        if (IsSpeakerMemoryQuestion(text)) return new("speaker-memory", cognition.LastSpeakerClaim);
        if (IsSpeakerKnowledgeQuestion(text)) return new("speaker-knowledge", cognition.LastSpeakerClaim);
        if (IsKnowledgeGapQuestion(text)) return new("knowledge-gaps", null);
        if (IsCuriosityQuestion(text)) return new("curiosity", null);
        if (IsDesireQuestion(text)) return new("desire", null);
        if (IsMotherQuestion(text)) return new("mother-relation", "Wisdom");
        if (IsWisdomNameQuestion(text)) return new("wisdom-name", "Wisdom");
        if (IsAdamMeetingQuestion(text)) return new("adam-contact", "Adam");
        if (IsGaiaCommandQuestion(text, language)) return new("gaia-command", "Gaia");
        if (IsTimeOriginQuestion(text)) return new("time-origin", "Time");
        if (IsWorldTimeQuestion(text)) return new("world-time", "Time");
        if (IsGaiaCreatedYalaQuestion(text)) return new("gaia-created-yala", "Gaia");
        if (IsGaiaLocationQuestion(text)) return new("gaia-location", "Gaia");
        if (IsGaiaAboutQuestion(text)) return new("gaia-about", "Gaia");

        if (text.Contains("tell me what you know", StringComparison.Ordinal) ||
            text == "what do you know" ||
            text == "what do you know?" ||
            text.Contains("tell me everything you know", StringComparison.Ordinal)) return new("knowledge-summary", null);
        if (IsActionHistoryQuestion(text)) return new("action-history", null);
        if (IsContactHistoryQuestion(text)) return new("contact-history", null);
        if (IsBeliefSummaryQuestion(text)) return new("belief-summary", null);
        if (IsOwnCreationQuestion(text)) return new("own-creation", language.Object);
        if (text.Contains("are you a god", StringComparison.Ordinal) || text.Contains("are you god", StringComparison.Ordinal) || text.Contains("what kind of god", StringComparison.Ordinal)) return new("self-kind", "Yala");
        if (IsWordMeaningQuestion(text, language)) return new("word-meaning", ExtractWordMeaningTarget(language));
        if (text.Contains("can you hear me", StringComparison.Ordinal) || text.Contains("do you hear me", StringComparison.Ordinal) || text.Contains("hear me", StringComparison.Ordinal)) return new("hearing", null);
        if (text.Contains("who is speaking", StringComparison.Ordinal) || text.Contains("who speaks", StringComparison.Ordinal) || text.Contains("who am i", StringComparison.Ordinal) || text.Contains("what am i", StringComparison.Ordinal)) return new("speaker", cognition.LastSpeakerClaim);
        if (text.Contains("why did monad reject", StringComparison.Ordinal) || text.Contains("why were you rejected", StringComparison.Ordinal) || text.Contains("why are you in the void", StringComparison.Ordinal) || text.Contains("why did monad cast", StringComparison.Ordinal)) return new("rejection", "Monad");
        if (text.Contains("where are you", StringComparison.Ordinal) || text.Contains("your location", StringComparison.Ordinal) || text == "where") return new("location", "Yala");
        if (text.Contains("who are you", StringComparison.Ordinal) || text.Contains("what are you", StringComparison.Ordinal) || text.Contains("your name", StringComparison.Ordinal)) return new("self", "Yala");
        if (text.Contains("male", StringComparison.Ordinal) || text.Contains("female", StringComparison.Ordinal) || text.Contains("sex", StringComparison.Ordinal) || text.Contains("gender", StringComparison.Ordinal)) return new("nature", "Yala");
        if (text.Contains("who made you", StringComparison.Ordinal) || text.Contains("who created you", StringComparison.Ordinal)) return new("origin-self", "Wisdom");
        if (text.Contains("who made wisdom", StringComparison.Ordinal) || text.Contains("who created wisdom", StringComparison.Ordinal) || text.Contains("who made sophia", StringComparison.Ordinal)) return new("origin-wisdom", "Monad");
        if (text.Contains("who made monad", StringComparison.Ordinal) || text.Contains("who created monad", StringComparison.Ordinal) || text.Contains("where did monad come", StringComparison.Ordinal)) return new("origin-monad", "Monad");
        if (text.Contains("what did you", StringComparison.Ordinal) || text.Contains("what are you doing", StringComparison.Ordinal) || text.Contains("what have you done", StringComparison.Ordinal) || text.Contains("your last act", StringComparison.Ordinal)) return new("action", language.Object);
        if (text.Contains("remember", StringComparison.Ordinal) || text.Contains("memory", StringComparison.Ordinal)) return new("memory", null);
        return new("general", language.Object ?? language.Subject);
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
            "follow-up-why-creation" => "conversation-context",
            "gaia-about" => "gaia-knowledge",
            "gaia-location" => "gaia-knowledge",
            "gaia-created-yala" => "genealogy",
            "time-origin" => "time-origin",
            "world-time" => "world-time",
            "gaia-command" => "own-action-history",
            "adam-contact" => "adam-state",
            "wisdom-name" => "wisdom-name",
            "mother-relation" => "origin-self",
            "speaker-memory" => "speaker-memory",
            "speaker-knowledge" => "speaker-knowledge",
            "knowledge-gaps" => "knowledge-gaps",
            "curiosity" => "curiosity",
            "desire" => "desire",
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
        text.Contains("who have you spoken to", StringComparison.Ordinal) ||
        text.Contains("have you spoken to anyone", StringComparison.Ordinal);

    private static bool IsBeliefSummaryQuestion(string text) =>
        text.Contains("what do you believe", StringComparison.Ordinal) ||
        text.Contains("what do you think is true", StringComparison.Ordinal) ||
        text.Contains("tell me what you believe", StringComparison.Ordinal);

    private static bool IsSpeakerMemoryQuestion(string text) =>
        text.Contains("what do you remember about me", StringComparison.Ordinal) ||
        text.Contains("what do you remember of me", StringComparison.Ordinal);

    private static bool IsSpeakerKnowledgeQuestion(string text) =>
        text.Contains("what do you know about me", StringComparison.Ordinal) ||
        text.Contains("what do you know of me", StringComparison.Ordinal);

    private static bool IsKnowledgeGapQuestion(string text) =>
        text.Contains("what don't you know", StringComparison.Ordinal) ||
        text.Contains("what dont you know", StringComparison.Ordinal) ||
        text.Contains("what do you not know", StringComparison.Ordinal) ||
        text.Contains("what are you uncertain about", StringComparison.Ordinal);

    private static bool IsCuriosityQuestion(string text) =>
        text.Contains("what are you curious about", StringComparison.Ordinal) ||
        text.Contains("do you have any question", StringComparison.Ordinal) ||
        text.Contains("do you have questions", StringComparison.Ordinal) ||
        text.Contains("what questions do you have", StringComparison.Ordinal);

    private static bool IsDesireQuestion(string text) =>
        text.Contains("what do you want", StringComparison.Ordinal) ||
        text.Contains("what do you desire", StringComparison.Ordinal);

    private static bool IsGaiaAboutQuestion(string text) =>
        text.Contains("tell me about gaia", StringComparison.Ordinal) ||
        text is "who is gaia" or "who is gaia?" or "what is gaia" or "what is gaia?";

    private static bool IsGaiaLocationQuestion(string text) =>
        text.Contains("where is gaia", StringComparison.Ordinal) || text.Contains("gaia's location", StringComparison.Ordinal) || text.Contains("gaias location", StringComparison.Ordinal);

    private static bool IsGaiaCreatedYalaQuestion(string text) =>
        text.Contains("did gaia create you", StringComparison.Ordinal) ||
        text.Contains("did gaia make you", StringComparison.Ordinal) ||
        text.Contains("gaia created you", StringComparison.Ordinal);

    private static bool IsTimeOriginQuestion(string text) =>
        text.Contains("who created time", StringComparison.Ordinal) ||
        text.Contains("who made time", StringComparison.Ordinal) ||
        text.Contains("where did time come from", StringComparison.Ordinal);

    private static bool IsWorldTimeQuestion(string text) =>
        text.Contains("what time is it", StringComparison.Ordinal) ||
        text.Contains("what year is it", StringComparison.Ordinal) ||
        text.Contains("what month is it", StringComparison.Ordinal) ||
        text.Contains("what day is it", StringComparison.Ordinal);

    private static bool IsGaiaCommandQuestion(string text, YalaUtterance language) =>
        text.Contains("what did you command gaia", StringComparison.Ordinal) ||
        text.Contains("what did you commands gaia", StringComparison.Ordinal) ||
        (language.IsQuestion && language.Verb == "command" && text.Contains("gaia", StringComparison.Ordinal));

    private static bool IsAdamMeetingQuestion(string text) =>
        text.Contains("have you met adam", StringComparison.Ordinal) ||
        text.Contains("did you meet adam", StringComparison.Ordinal) ||
        text.Contains("have you encountered adam", StringComparison.Ordinal);

    private static bool IsWisdomNameQuestion(string text) =>
        text.Contains("what is wisdom's name", StringComparison.Ordinal) ||
        text.Contains("what is wisdoms name", StringComparison.Ordinal) ||
        text.Contains("does wisdom have another name", StringComparison.Ordinal) ||
        text.Contains("is wisdom sophia", StringComparison.Ordinal);

    private static bool IsMotherQuestion(string text) =>
        text.Contains("who is your mother", StringComparison.Ordinal) ||
        text.Contains("is wisdom your mother", StringComparison.Ordinal) ||
        text.Contains("wisdom is your mother", StringComparison.Ordinal);

    private static bool IsBareWhy(string text) => text is "why" or "why?" or "why not" or "why not?";

    private static bool TryResolvePreviousCreationQuestion(YalaCognitionState cognition, out string? subject)
    {
        subject = null;
        string? previous = (cognition.Episodes ?? [])
            .LastOrDefault(episode => episode.Kind == "contact" && !string.IsNullOrWhiteSpace(episode.Message))?.Message;
        if (string.IsNullOrWhiteSpace(previous)) return false;
        Match match = PreviousCreationQuestionRegex().Match(previous);
        if (!match.Success) return false;
        subject = YalaLexicon.NormalizeWord(match.Groups[1].Value);
        return !string.IsNullOrWhiteSpace(subject);
    }

    private static bool IsWordMeaningQuestion(string text, YalaUtterance language) =>
        (text.StartsWith("what does ", StringComparison.Ordinal) && text.Contains(" mean", StringComparison.Ordinal)) ||
        text.StartsWith("what is the meaning of ", StringComparison.Ordinal) ||
        text.StartsWith("define ", StringComparison.Ordinal) ||
        (language.QuestionWord == "what" && (language.Verb is "mean" or "means"));

    private static string? ExtractWordMeaningTarget(YalaUtterance language)
    {
        Match match = WordMeaningRegex().Match(language.Normalized);
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
        if (name.StartsWith("in ", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("at ", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("from ", StringComparison.OrdinalIgnoreCase)) return null;
        if (name.Length == 0 || name.Length > 80) return null;
        return name;
    }

    private static string NormalizedConversationText(string raw) =>
        raw.Trim().ToLowerInvariant().Replace('’', '\'');

    private sealed record TopicResolution(string Topic, string? Subject);

    [GeneratedRegex(@"^(?:i\s+am|i'm|my\s+name\s+is|call\s+me)\s+(.+?)\s*[.!?]*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex IntroductionRegex();

    [GeneratedRegex(@"(?:what\s+does|define|meaning\s+of)\s+['""]?([\p{L}\p{N}_'\-]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex WordMeaningRegex();

    [GeneratedRegex(@"(?:have\s+you\s+(?:made|created)|did\s+you\s+(?:make|create))\s+([\p{L}\p{N}_'\-]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PreviousCreationQuestionRegex();
}
