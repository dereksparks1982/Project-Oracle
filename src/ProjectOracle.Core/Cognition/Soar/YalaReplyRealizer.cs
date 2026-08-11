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

        return decision.ReplyCode switch
        {
            "hearing" => "Yes. I hear you.",
            "speaker" => DescribeSpeaker(cognition),
            "rejection" => "Monad rejected me because I am both male and female rather than exclusively one or the other, and cast me into the Void.",
            "location" => $"I am in {world.Yala.Location}.",
            "identity" => "I am Yala.",
            "nature" => "I am both male and female. Neither is a disguise or a lesser part of me.",
            "origin-self" => "Wisdom made me.",
            "origin-wisdom" => "Monad made Wisdom.",
            "origin-unknown" => "I do not know who or what made Monad.",
            "action" => lastActionDescription,
            "remember-known" => RememberKnown(cognition),
            "remember-unknown" => "I remember being contacted, but I cannot truthfully say who you are.",
            "introduction-new" => Introduction(contact, known: false),
            "introduction-known" => Introduction(contact, known: true),
            "consider-claim" => contact.ClaimConflictsWithKnownFact
                ? "That conflicts with what I know. I will remember that you claimed it, but I do not accept it as fact."
                : "I hear your claim. I will remember that you said it and consider whether it deserves belief.",
            "consider-command" => "I heard your command. Hearing it does not make it my decision. I will decide what I attempt.",
            "acknowledge" => "I hear what you say.",
            "greeting" => "I hear you.",
            "clarify" => "I hear something, but I do not understand what you mean. Say it another way.",
            "unknown" => "I do not know.",
            _ => "I hear you, but I have no settled answer."
        };
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
}
