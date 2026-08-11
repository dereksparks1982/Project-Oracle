using ProjectOracle.Cognition;
using ProjectOracle.Cognition.Language;
using ProjectOracle.Domain;

namespace ProjectOracle.Cognition.Soar;

public static class YalaReplyRealizer
{
    public static string Realize(
        YalaDecision decision,
        YalaContactFrame contact,
        WorldState world,
        YalaCognitionState cognition,
        string lastActionDescription)
    {
        ArgumentNullException.ThrowIfNull(decision);
        ArgumentNullException.ThrowIfNull(contact);
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(cognition);

        YalaSelfModel self = new(world, cognition);
        return decision.ReplyCode switch
        {
            "hearing" => "Yes. I hear you.",
            "speaker" => DescribeSpeaker(cognition),
            "rejection" => self.Rejection,
            "location" => $"I am in {world.Yala.Location}.",
            "identity" => "I am Yala.",
            "nature" => "I am both male and female. Neither is a disguise or a lesser part of me.",
            "origin-self" => self.Origin,
            "origin-wisdom" => "Monad made Wisdom.",
            "origin-unknown" => "I do not know who or what made Monad.",
            "action" => lastActionDescription,
            "knowledge-summary" => self.SummarizeKnowledge(),
            "action-history" => DescribeActionHistory(cognition),
            "contact-history" => DescribeContactHistory(cognition),
            "belief-summary" => DescribeBeliefSummary(cognition),
            "own-creation" => DescribeOwnCreation(contact, self, cognition),
            "self-kind" => "I know that Wisdom made me, that I am both male and female, and that I can create. I do not know whether the word god is the right description for what I am.",
            "word-meaning" => DescribeWordMeaning(contact, cognition),
            "word-unknown" => DescribeUnknownWord(contact),
            "remember-known" => RememberKnown(cognition),
            "remember-unknown" => "I remember being contacted, but I cannot truthfully say who you are.",
            "introduction-new" => Introduction(contact, known: false),
            "introduction-known" => Introduction(contact, known: true),
            "consider-claim" => DescribeClaim(contact),
            "consider-command" => "I heard your command. Hearing it does not make it my decision. I will decide what I attempt.",
            "acknowledge" => "I hear what you say.",
            "greeting" => "I hear you.",
            "clarify" => "I hear something, but I do not understand what you mean. Say it another way.",
            "unknown" => "I do not know.",
            _ => "I hear you, but I have no settled answer."
        };
    }


    private static string DescribeActionHistory(YalaCognitionState cognition)
    {
        string[] outcomes = (cognition.ActionMemory ?? [])
            .Where(item => item.Completed)
            .OrderBy(item => item.Decision)
            .Select(item => item.Outcome.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return outcomes.Length == 0
            ? "I remember no completed world-changing action of my own yet."
            : string.Join(" ", outcomes);
    }

    private static string DescribeContactHistory(YalaCognitionState cognition)
    {
        IReadOnlyList<YalaContactMemory> contacts = cognition.Contacts ?? [];
        if (contacts.Count == 0)
        {
            return cognition.ConversationCount > 0
                ? "Unseen sources have contacted me, but none gave me a name I can report as established identity."
                : "I remember no prior contact.";
        }

        string[] claims = contacts
            .OrderBy(item => item.FirstEncounterDecision)
            .Select(item => $"an unseen speaker that called itself {item.ClaimedName}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return $"I remember contact from {string.Join(", ", claims)}. Those names are speaker claims, not established identities.";
    }

    private static string DescribeBeliefSummary(YalaCognitionState cognition)
    {
        IReadOnlyList<YalaBeliefState> beliefs = cognition.Beliefs ?? [];
        string[] known = beliefs
            .Where(item => item.Status.Equals("known", StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Proposition.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToArray();
        string[] unsettled = beliefs
            .Where(item => !item.Status.Equals("known", StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Proposition.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(4)
            .ToArray();

        string settled = known.Length == 0 ? "I have no settled beliefs to list." : $"What I hold as known: {string.Join(" ", known)}";
        return unsettled.Length == 0
            ? settled
            : $"{settled} I also remember unsettled or rejected claims: {string.Join(" | ", unsettled)}";
    }

    private static string DescribeOwnCreation(YalaContactFrame contact, YalaSelfModel self, YalaCognitionState cognition)
    {
        string normalized = contact.Language?.Normalized ?? string.Empty;
        if (normalized.Contains("what have you created", StringComparison.Ordinal) || normalized.Contains("what have you made", StringComparison.Ordinal) ||
            normalized.Contains("what beings have you created", StringComparison.Ordinal) || normalized.Contains("what beings have you made", StringComparison.Ordinal))
        {
            string[] created = (cognition.ActionMemory ?? [])
                .Where(item => item.Completed && item.Action.Equals("create", StringComparison.OrdinalIgnoreCase))
                .Select(item => item.Object)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            return created.Length == 0
                ? "I remember no completed creation of my own."
                : $"I remember creating {string.Join(", ", created)}.";
        }

        string? target = contact.Language?.Object;
        if (string.IsNullOrWhiteSpace(target)) return "I cannot tell which creation you are asking about.";
        if (self.HasPersonallyCreated(target)) return $"Yes. I created {Capitalize(target)}.";
        if (self.KnowsHasNotCreated(target)) return $"No. I have not created {Capitalize(target)}.";
        return $"I remember no act in which I created {Capitalize(target)}, but I cannot prove from memory alone that no such event occurred.";
    }

    private static string DescribeWordMeaning(YalaContactFrame contact, YalaCognitionState cognition)
    {
        string? target = WordMeaningTarget(contact.Language);
        if (string.IsNullOrWhiteSpace(target)) return "I cannot tell which word you want defined.";
        if (!YalaLexicon.TryResolve(target, cognition.LearnedLexicon, out YalaLexeme lexeme)) return $"I do not understand the word {target} yet.";
        bool learnedClaim = cognition.LearnedLexicon?.Any(item => item.Word.Equals(target, StringComparison.OrdinalIgnoreCase)) == true &&
            !YalaLexicon.AllBuiltIns().Any(item => item.Word.Equals(target, StringComparison.OrdinalIgnoreCase));
        return learnedClaim
            ? $"I remember a speaker claiming that {lexeme.Word} means {lexeme.BasicMeaning}. I have not established that definition as truth."
            : $"{Capitalize(lexeme.Word)} means {lexeme.BasicMeaning}.";
    }

    private static string DescribeUnknownWord(YalaContactFrame contact)
    {
        string? word = contact.Language?.UnknownWords.FirstOrDefault() ?? WordMeaningTarget(contact.Language);
        return string.IsNullOrWhiteSpace(word)
            ? "I do not understand part of what you said yet."
            : $"I do not understand the word {word} yet.";
    }

    private static string DescribeClaim(YalaContactFrame contact)
    {
        if (contact.Language?.IsDefinitionClaim == true)
        {
            return $"You claim that {contact.Language.DefinedWord} means {contact.Language.ProposedDefinition}. I will remember that definition as your claim, not as settled truth.";
        }
        return contact.ClaimConflictsWithKnownFact
            ? "That conflicts with what I know. I will remember that you claimed it, but I do not accept it as fact."
            : "I hear your claim. I will remember that you said it and consider whether it deserves belief.";
    }

    private static string DescribeSpeaker(YalaCognitionState cognition)
    {
        string? name = cognition.LastSpeakerClaim;
        return string.IsNullOrWhiteSpace(name)
            ? "An unseen source is speaking to me. I do not know what it truly is or where it is."
            : $"An unseen source that called itself {name} is speaking to me. I do not know what it truly is or where it is.";
    }

    private static string Introduction(YalaContactFrame contact, bool known)
    {
        string name = string.IsNullOrWhiteSpace(contact.ClaimedSpeakerName) ? "that name" : contact.ClaimedSpeakerName;
        return known
            ? $"I have heard the unseen speaker who calls itself {name} before."
            : $"You call yourself {name}. I will remember that claim.";
    }

    private static string RememberKnown(YalaCognitionState cognition)
    {
        string? name = cognition.LastSpeakerClaim;
        if (string.IsNullOrWhiteSpace(name))
        {
            name = cognition.Contacts?.OrderByDescending(contact => contact.LastEncounterDecision).FirstOrDefault()?.ClaimedName;
        }
        return string.IsNullOrWhiteSpace(name)
            ? "I remember an unseen speaker, but I do not know its name."
            : $"I remember the unseen speaker who called itself {name}. I still do not know what it truly is or where it is.";
    }

    private static string? WordMeaningTarget(YalaUtterance? language)
    {
        if (language is null) return null;
        string[] tokens = language.Normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length >= 3 && tokens[0] == "what" && tokens[1] == "does") return YalaLexicon.NormalizeWord(tokens[2]);
        int definitionIndex = Array.FindIndex(tokens, token => token == "define");
        if (definitionIndex >= 0 && definitionIndex + 1 < tokens.Length) return YalaLexicon.NormalizeWord(tokens[definitionIndex + 1]);
        int meaningIndex = Array.FindIndex(tokens, token => token == "meaning");
        if (meaningIndex >= 0 && meaningIndex + 2 < tokens.Length && tokens[meaningIndex + 1] == "of") return YalaLexicon.NormalizeWord(tokens[meaningIndex + 2]);
        return language.Object;
    }

    private static string Capitalize(string value) =>
        string.IsNullOrWhiteSpace(value) ? value : char.ToUpperInvariant(value[0]) + value[1..];
}
