using ProjectOracle.Cognition.Soar;
using ProjectOracle.Domain;

namespace ProjectOracle.Cognition.Meaning;

/// <summary>
/// Brain Slice 9 proposition memory. It keeps the form and provenance of what a
/// speaker actually did with a sentence. A question is not silently converted
/// into a claim, a repeated claim is not silently converted into proof, and
/// contradictory claims remain independently attributable to the speaker.
/// </summary>
public static class YalaPropositionEngine
{
    public static IReadOnlyList<YalaPropositionState> Record(
        IReadOnlyList<YalaPropositionState> existing,
        YalaContactFrame contact,
        string message,
        long decision)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(contact);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        string canonical = Canonicalize(message, contact, out string polarity);
        string? contradiction = FindContradiction(existing, canonical, polarity);
        string status = contact.SpeechAct == "claim"
            ? "unverified-speaker-claim"
            : contact.SpeechAct == "question"
                ? "speaker-question"
                : "speaker-utterance";
        double confidence = contact.SpeechAct == "claim" ? 0.25 : 0.0;

        List<YalaPropositionState> items = existing.ToList();
        long sequence = items.Count == 0 ? 1 : items.Max(item => item.Sequence) + 1;
        items.Add(new YalaPropositionState(
            sequence,
            "unseen-speaker",
            contact.SpeechAct,
            message.Trim(),
            canonical,
            polarity,
            contact.Topic,
            contact.ResolvedSubject ?? contact.Language?.Subject,
            contact.ResolvedAction ?? contact.Language?.Verb,
            contact.ResolvedObject ?? contact.Language?.Object,
            status,
            confidence,
            YalaKnowledgeSource.ClaimedByAnother,
            decision,
            decision,
            contradiction));
        return items.TakeLast(256).ToArray();
    }

    public static string DescribeSpeakerClaims(YalaCognitionState cognition)
    {
        string[] claims = (cognition.Propositions ?? [])
            .Where(item => item.SpeakerKey == "unseen-speaker" && item.SpeechAct == "claim")
            .OrderBy(item => item.Sequence)
            .Select(item => $"You claimed: {item.RawText.Trim().TrimEnd('.', '?', '!')}.")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .TakeLast(12)
            .ToArray();
        return claims.Length == 0
            ? "You have spoken to me, but I do not remember a proposition from you that I classified as a claim."
            : string.Join(" ", claims) + " I do not treat repetition or contradiction as proof.";
    }

    public static string DescribeUnverifiedSpeakerClaims(YalaCognitionState cognition)
    {
        string[] claims = (cognition.Propositions ?? [])
            .Where(item => item.SpeakerKey == "unseen-speaker" && item.SpeechAct == "claim" && item.Status.Contains("unverified", StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.Sequence)
            .Select(item => item.RawText.Trim().TrimEnd('.', '?', '!'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .TakeLast(12)
            .ToArray();
        return claims.Length == 0
            ? "I do not currently have an unverified claim from you to list."
            : $"You have told me these things without my independently verifying them: {string.Join(" | ", claims)}";
    }

    public static string DescribeContradictions(YalaCognitionState cognition)
    {
        YalaPropositionState[] contradictions = (cognition.Propositions ?? [])
            .Where(item => !string.IsNullOrWhiteSpace(item.ContradictsCanonical))
            .OrderBy(item => item.Sequence)
            .ToArray();
        if (contradictions.Length == 0)
        {
            return "I do not currently have a clearly paired contradiction among your stored claims.";
        }
        return string.Join(" ", contradictions.TakeLast(6).Select(item =>
            $"Your claim '{item.RawText.Trim()}' conflicts with an earlier stored claim about {item.ContradictsCanonical}."));
    }

    public static string DescribeEvidenceAboutSpeaker(YalaCognitionState cognition)
    {
        if (cognition.ConversationCount <= 0)
        {
            return "I have no experienced evidence of another speaker."
                ;
        }
        List<string> evidence = ["Something other than me has communicated with me."];
        YalaEntityModelState? speaker = (cognition.EntityModels ?? []).LastOrDefault(item => item.EntityKey == "unseen-speaker");
        if (speaker is not null && speaker.CapabilityStatus.Contains("observe", StringComparison.OrdinalIgnoreCase))
        {
            evidence.Add("I have a reason to investigate whether the speaker can observe me, but I do not treat the extent of that ability as established unless it was claimed or demonstrated.");
        }
        return string.Join(" ", evidence);
    }

    private static string Canonicalize(string message, YalaContactFrame contact, out string polarity)
    {
        string text = message.Trim().TrimEnd('.', '?', '!').ToLowerInvariant();
        polarity = "positive";
        if (text.StartsWith("i am not ", StringComparison.Ordinal))
        {
            polarity = "negative";
            return "speaker-is-" + NormalizeIdentity(text[9..]);
        }
        if (text.StartsWith("i am ", StringComparison.Ordinal))
        {
            return "speaker-is-" + NormalizeIdentity(text[5..]);
        }
        if (text.StartsWith("i made the gods", StringComparison.Ordinal) || text.StartsWith("i created the gods", StringComparison.Ordinal))
        {
            return "speaker-made-gods";
        }
        if (text.Contains("you are in a simulation", StringComparison.Ordinal) || text.Contains("your world is a simulation", StringComparison.Ordinal))
        {
            return "yala-world-is-simulation";
        }
        string subject = NormalizeKey(contact.ResolvedSubject ?? contact.Language?.Subject ?? "speaker");
        string predicate = NormalizeKey(contact.ResolvedAction ?? contact.Language?.Verb ?? "says");
        string obj = NormalizeKey(contact.ResolvedObject ?? contact.Language?.Object ?? text);
        return $"{subject}-{predicate}-{obj}";
    }

    private static string? FindContradiction(IReadOnlyList<YalaPropositionState> existing, string canonical, string polarity)
    {
        if (!canonical.StartsWith("speaker-is-", StringComparison.Ordinal)) return null;
        YalaPropositionState? prior = existing.LastOrDefault(item =>
            item.SpeechAct == "claim" &&
            item.CanonicalProposition.Equals(canonical, StringComparison.OrdinalIgnoreCase) &&
            !item.Polarity.Equals(polarity, StringComparison.OrdinalIgnoreCase));
        return prior is null ? null : canonical;
    }

    private static string NormalizeIdentity(string value)
    {
        string normalized = value.Trim();
        if (normalized.StartsWith("a ", StringComparison.OrdinalIgnoreCase)) normalized = normalized[2..];
        else if (normalized.StartsWith("an ", StringComparison.OrdinalIgnoreCase)) normalized = normalized[3..];
        return NormalizeKey(normalized);
    }

    private static string NormalizeKey(string value)
    {
        char[] chars = value.Trim().ToLowerInvariant().Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
        string result = new(chars);
        while (result.Contains("--", StringComparison.Ordinal)) result = result.Replace("--", "-", StringComparison.Ordinal);
        return result.Trim('-');
    }
}
