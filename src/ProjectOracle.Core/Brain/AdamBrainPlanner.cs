using ProjectOracle.Domain;
using ProjectOracle.Simulation;

namespace ProjectOracle.Brain;

/// <summary>
/// Later-world Adam planning scaffold retained dormant until autonomous history actually reaches Adam.
/// Yala's v0.0.22 cognition is handled by the real Soar 9.6.5 Brain Slice 5 integration.
/// </summary>
public static class AdamBrainPlanner
{
    public const string SystemName = "Adam HTN Compatibility Brain v0.1";
    public const string Source = "Internal deterministic Adam planner retained dormant until the world actually contains Adam.";

    public static ReasonedPlanState PlanAdamDirectCall(
        long planId,
        long worldMilliseconds,
        ulong worldSeed,
        AdamState adam,
        string message,
        IReadOnlyList<string> options)
    {
        ArgumentNullException.ThrowIfNull(adam);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentNullException.ThrowIfNull(options);

        string selected = SelectOption(worldSeed, planId, message, options);
        return new ReasonedPlanState(
            planId,
            worldMilliseconds,
            adam.Id.Value,
            SystemName,
            "Respond to contact without breaking protected choice.",
            $"A direct contact reached Adam: \"{message.Trim()}\".",
            ["notice the contact", "preserve protected choice", "select a lawful response mode", "record the selected response"],
            options,
            selected,
            "Adam's compatibility planner chooses only a response mode. Full Adam cognition remains future work.",
            Source);
    }

    public static ReasonedPlanState PlanAdamVesselSpeech(
        long planId,
        long worldMilliseconds,
        ulong worldSeed,
        AdamState adam,
        string vessel,
        string message,
        IReadOnlyList<string> options)
    {
        ArgumentNullException.ThrowIfNull(adam);
        ArgumentException.ThrowIfNullOrWhiteSpace(vessel);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentNullException.ThrowIfNull(options);

        string selected = SelectOption(worldSeed, planId, vessel + "|" + message, options);
        return new ReasonedPlanState(
            planId,
            worldMilliseconds,
            adam.Id.Value,
            SystemName,
            "Respond to vessel speech without executing the temptation or command.",
            $"A {vessel.Trim()} spoke to Adam: \"{message.Trim()}\".",
            ["hear the vessel", "separate speech from truth", "consider responses", "select one response mode"],
            options,
            selected,
            "The compatibility planner records a response mode only.",
            Source);
    }

    public static ReasonedPlanState PlanAdamNaming(
        long planId,
        long worldMilliseconds,
        AdamState adam,
        LivingKindState kind,
        string adamName,
        string namingReason)
    {
        ArgumentNullException.ThrowIfNull(adam);
        ArgumentNullException.ThrowIfNull(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(adamName);
        ArgumentException.ThrowIfNullOrWhiteSpace(namingReason);

        string selected = $"name it {adamName.Trim()}";
        return new ReasonedPlanState(
            planId,
            worldMilliseconds,
            adam.Id.Value,
            SystemName,
            "Name the presented living kind and test whether it is a true counterpart.",
            $"{kind.AncientKind} was presented to Adam.",
            ["observe the living kind", "compare its form", "choose a name", "record the reason"],
            ["observe", "name", "wait", "turn away"],
            selected,
            $"{namingReason.Trim()} The kind is not a suitable mate in the compatibility scaffold.",
            Source);
    }

    private static string SelectOption(ulong worldSeed, long planId, string situationKey, IReadOnlyList<string> options)
    {
        if (options.Count == 0)
        {
            throw new ArgumentException("A plan must have at least one option.", nameof(options));
        }

        ulong hash = worldSeed ^ ((ulong)planId * 0x9E37_79B9_7F4A_7C15UL);
        foreach (char character in situationKey)
        {
            hash ^= character;
            hash *= 0x100_0000_01B3UL;
        }

        DeterministicRandom random = new(hash);
        return options[(int)(random.NextUInt64() % (ulong)options.Count)];
    }
}
