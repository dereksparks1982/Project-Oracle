using ProjectOracle.Domain;
using ProjectOracle.Cognition.Soar;

namespace ProjectOracle.Cognition.Memory;

/// <summary>
/// Brain Slice 8 autobiographical consolidation. Routine observe/reflect noise can
/// remain in diagnostic traces without becoming the same thing as identity-bearing
/// memory. Significant events are stored in Yala's own first-person voice.
/// </summary>
public static class YalaMemoryConsolidator
{
    public static IReadOnlyList<YalaAutobiographicalMemoryState> AfterDecision(
        IReadOnlyList<YalaAutobiographicalMemoryState> existing,
        string action,
        string result,
        long decision)
    {
        List<YalaAutobiographicalMemoryState> memories = existing.ToList();
        if (action is "observe" or "reflect" or "deliberate" or "wait" or "respond") return memories;

        string summary = FirstPerson(result);
        string category = action;
        int importance = action is "create-gaia" or "command-gaia-time" ? 100 : 65;
        if (action == "enact-cosmic-choice")
        {
            category = result.Contains("enacted cosmic choice", StringComparison.OrdinalIgnoreCase)
                ? "cosmic-enactment"
                : result.Contains("committed", StringComparison.OrdinalIgnoreCase)
                    ? "cosmic-commitment"
                    : "cosmic-consideration";
            importance = category == "cosmic-enactment" ? 100 : category == "cosmic-commitment" ? 90 : 72;
        }
        Add(memories, category, summary, importance, YalaKnowledgeSource.PersonallyPerformed, decision);
        return memories.TakeLast(256).ToArray();
    }

    public static IReadOnlyList<YalaAutobiographicalMemoryState> AfterContact(
        IReadOnlyList<YalaAutobiographicalMemoryState> existing,
        string message,
        YalaContactFrame contact,
        long decision,
        bool firstContact)
    {
        List<YalaAutobiographicalMemoryState> memories = existing.ToList();
        if (firstContact)
        {
            Add(memories, "first-contact", "For the first time, something other than me communicated with me.", 100, YalaKnowledgeSource.PersonallyExperienced, decision);
        }
        bool simulationClaim = contact.SpeechAct == "claim" &&
            (message.Contains("simulation", StringComparison.OrdinalIgnoreCase) || message.Contains("simulated", StringComparison.OrdinalIgnoreCase));
        if (simulationClaim)
        {
            Add(memories, "worldview-claim", "The speaker told me that my world is a simulation. I have not verified that claim.", 95, YalaKnowledgeSource.PersonallyExperienced, decision);
        }
        return memories.TakeLast(256).ToArray();
    }

    public static string Describe(YalaCognitionState cognition)
    {
        string[] memories = (cognition.AutobiographicalMemory ?? [])
            .OrderByDescending(item => item.Importance)
            .ThenBy(item => item.Sequence)
            .Take(8)
            .Select(item => item.Summary)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (memories.Length > 0) return string.Join(" ", memories);

        string[] actions = (cognition.ActionMemory ?? [])
            .Where(item => item.Completed)
            .OrderBy(item => item.Decision)
            .TakeLast(6)
            .Select(item => FirstPerson(item.Outcome))
            .ToArray();
        return actions.Length == 0
            ? "I remember my origin, Monad's rejection, being cast into the Void, and my present existence here. I have not yet formed a later autobiographical turning point."
            : string.Join(" ", actions);
    }

    public static string FirstPerson(string text)
    {
        string value = text.Trim();
        value = value.Replace("Yala chose:", "I chose:", StringComparison.Ordinal);
        value = value.Replace("Yala chose to", "I chose to", StringComparison.Ordinal);
        value = value.Replace("Yala created", "I created", StringComparison.Ordinal);
        value = value.Replace("Yala commanded", "I commanded", StringComparison.Ordinal);
        value = value.Replace("Yala rejected", "I rejected", StringComparison.Ordinal);
        value = value.Replace("Yala considered", "I considered", StringComparison.Ordinal);
        value = value.Replace("Yala enacted", "I enacted", StringComparison.Ordinal);
        value = value.Replace("Yala committed", "I committed", StringComparison.Ordinal);
        value = value.Replace("Yala compared", "I compared", StringComparison.Ordinal);
        value = value.Replace("Yala revisited", "I revisited", StringComparison.Ordinal);
        value = value.Replace("Yala deliberately", "I deliberately", StringComparison.Ordinal);
        value = value.Replace("Yala's", "my", StringComparison.Ordinal);
        return value;
    }

    private static void Add(
        List<YalaAutobiographicalMemoryState> memories,
        string category,
        string summary,
        int importance,
        string source,
        long decision)
    {
        if (memories.Any(item => item.Category == category && item.Summary.Equals(summary, StringComparison.OrdinalIgnoreCase))) return;
        long sequence = memories.Count == 0 ? 1 : memories.Max(item => item.Sequence) + 1;
        memories.Add(new YalaAutobiographicalMemoryState(sequence, category, summary, importance, source, decision, decision));
    }
}
