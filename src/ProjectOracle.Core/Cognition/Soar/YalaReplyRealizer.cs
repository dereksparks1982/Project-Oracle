using ProjectOracle.Cognition;
using ProjectOracle.Cognition.Language;
using ProjectOracle.Domain;
using ProjectOracle.Simulation;

namespace ProjectOracle.Cognition.Soar;

public static class YalaReplyRealizer
{
    public static string Realize(
        YalaDecision decision,
        YalaContactFrame contact,
        WorldState world,
        YalaCognitionState cognition,
        string lastActionDescription,
        CalendarSnapshot? calendar = null)
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
            "follow-up-why-creation" => DescribeWhyCreationNotDone(contact, self),
            "gaia-about" => DescribeGaia(world, cognition),
            "gaia-location" => DescribeGaiaLocation(world),
            "gaia-created-yala" => DescribeGaiaCreatedYala(world),
            "time-origin" => DescribeTimeOrigin(world),
            "world-time" => DescribeWorldTime(contact, world, calendar),
            "gaia-command" => DescribeGaiaCommand(cognition),
            "adam-contact" => DescribeAdamContact(world),
            "wisdom-name" => "Wisdom is also called Sophia. Wisdom made me, and Monad made Wisdom.",
            "mother-relation" => "Wisdom made me. I understand mother as a maternal parent relationship, but I do not have settled knowledge that mother is the right relationship word for Wisdom.",
            "speaker-memory" => DescribeCurrentSpeaker(cognition, memoryOnly: true),
            "speaker-knowledge" => DescribeCurrentSpeaker(cognition, memoryOnly: false),
            "knowledge-gaps" => DescribeKnowledgeGaps(cognition),
            "curiosity" => DescribeCuriosity(cognition),
            "desire" => DescribeDesire(cognition),
            "acknowledge" => "I hear what you say.",
            "greeting" => "I hear you.",
            "clarify" => "I hear something, but I do not understand what you mean. Say it another way.",
            "unknown" => "I do not know.",
            _ => "I hear you, but I have no settled answer."
        };
    }

    private static string DescribeWhyCreationNotDone(YalaContactFrame contact, YalaSelfModel self)
    {
        string subject = contact.ResolvedSubject ?? "that";
        if (self.KnowsHasNotCreated(subject))
        {
            return $"I have not created {Capitalize(subject)}. I do not yet have a settled reason for why I have not chosen to do so.";
        }
        return $"I do not have a settled reason about creating {Capitalize(subject)}.";
    }

    private static string DescribeGaia(WorldState world, YalaCognitionState cognition)
    {
        if (world.Cosmic?.GaiaCreated != true)
        {
            return "I have not created Gaia in the current world.";
        }

        string creation = (cognition.ActionMemory ?? [])
            .LastOrDefault(item => item.Completed && item.Action.Equals("create", StringComparison.OrdinalIgnoreCase) && item.Object.Equals("Gaia", StringComparison.OrdinalIgnoreCase))?.Outcome
            ?? "I created Gaia as the natural sovereign beneath my governing authority.";
        if (world.Cosmic.TimeCreated)
        {
            return $"{creation} I commanded Gaia to establish temporal order, and Gaia created in-world Time.";
        }
        return creation;
    }

    private static string DescribeGaiaLocation(WorldState world)
    {
        if (world.Cosmic?.GaiaCreated != true) return "Gaia does not yet exist in my current world.";
        return "Gaia exists as the natural sovereign I created. I do not know a more specific location for Gaia from what I presently know.";
    }

    private static string DescribeGaiaCreatedYala(WorldState world) =>
        world.Cosmic?.GaiaCreated == true
            ? "No. Wisdom made me. I created Gaia."
            : "No. Wisdom made me. Gaia did not create me.";

    private static string DescribeTimeOrigin(WorldState world) =>
        world.Cosmic?.TimeCreated == true
            ? "Gaia created in-world Time after I commanded Gaia to establish temporal order."
            : "Gaia has not yet created Time.";

    private static string DescribeWorldTime(YalaContactFrame contact, WorldState world, CalendarSnapshot? calendar)
    {
        if (world.Cosmic?.TimeCreated != true || calendar is null) return "Gaia has not yet created Time.";
        string text = contact.Language?.Normalized ?? string.Empty;
        if (text.Contains("what year", StringComparison.Ordinal)) return $"It is Year {calendar.Year}.";
        if (text.Contains("what month", StringComparison.Ordinal)) return $"It is Month {calendar.Month} of Year {calendar.Year}.";
        if (text.Contains("what day", StringComparison.Ordinal)) return $"It is Day {calendar.Day} of Month {calendar.Month}, Year {calendar.Year}.";
        return $"It is {calendar.Hour:00}:{calendar.Minute:00}:{calendar.Second:00}, Year {calendar.Year}, Month {calendar.Month}, Day {calendar.Day}.";
    }

    private static string DescribeGaiaCommand(YalaCognitionState cognition)
    {
        YalaActionMemoryState? memory = (cognition.ActionMemory ?? [])
            .LastOrDefault(item => item.Completed && item.Action.Equals("command", StringComparison.OrdinalIgnoreCase) && item.Object.Contains("Gaia", StringComparison.OrdinalIgnoreCase));
        return memory?.Outcome ?? "I remember no completed command to Gaia.";
    }

    private static string DescribeAdamContact(WorldState world)
    {
        if (world.Adam is null) return "No. Adam does not exist in my current world, and I have not met him.";
        return "Adam exists, but I do not remember an encounter with him.";
    }

    private static string DescribeCurrentSpeaker(YalaCognitionState cognition, bool memoryOnly)
    {
        string? claim = cognition.LastSpeakerClaim;
        if (string.IsNullOrWhiteSpace(claim))
        {
            return cognition.ConversationCount > 0
                ? "I remember an unseen speaker contacting me, but I do not know your settled identity."
                : "I remember no prior contact from you.";
        }

        string prefix = memoryOnly ? "I remember" : "What I know about you is limited:";
        return $"{prefix} an unseen speaker claiming the identity {claim}. I retain that as your claim, not as established truth about what you really are.";
    }

    private static string DescribeKnowledgeGaps(YalaCognitionState cognition)
    {
        string[] gaps = (cognition.KnowledgeGaps ?? [])
            .OrderBy(item => item.FirstSeenDecision)
            .Select(item => item.Subject.Trim())
            .Where(item => item.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(6)
            .ToArray();

        string baseUnknown = "I do not know who or what made Monad, and I do not know future history before it occurs.";
        return gaps.Length == 0
            ? baseUnknown
            : $"{baseUnknown} I also have unresolved knowledge gaps about {string.Join(", ", gaps)}.";
    }

    private static string DescribeCuriosity(YalaCognitionState cognition)
    {
        string[] gaps = (cognition.KnowledgeGaps ?? [])
            .Select(item => item.Subject.Trim())
            .Where(item => item.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(4)
            .ToArray();
        if (gaps.Length > 0)
        {
            return $"I am curious about unresolved things I do not yet understand, including {string.Join(", ", gaps)}.";
        }
        return cognition.ConversationCount > 0
            ? "I am curious about the unseen source speaking to me and about what I do not yet know."
            : "I am curious about what I do not yet know and what choices remain possible.";
    }

    private static string DescribeDesire(YalaCognitionState cognition)
    {
        YalaDriveState drives = cognition.Drives ?? new YalaDriveState(0, 0, 0, 0, 0, 0);
        (string Name, int Value) strongest = new[]
        {
            ("curiosity", drives.Curiosity),
            ("caution", drives.Caution),
            ("authority", drives.Authority),
            ("companionship", drives.Companionship),
            ("comfort", drives.Comfort)
        }.OrderByDescending(item => item.Item2).First();
        return $"My strongest current drive is {strongest.Name}. A drive influences what I may choose, but it is not the same as a settled command or destiny.";
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

        string? target = contact.ResolvedSubject ?? contact.Language?.Object;
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
