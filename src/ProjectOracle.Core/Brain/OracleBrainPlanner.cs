using ProjectOracle.Domain;
using ProjectOracle.Simulation;

namespace ProjectOracle.Brain;

public static class OracleBrainPlanner
{
    public const string SystemName = "Oracle HTN Brain v0.1";
    public const string Source = "Internal deterministic HTN-style planner inspired by Fluid HTN concepts; no external planner code is vendored in v0.1.10.";

    public static ReasonedPlanState PlanAdamDirectAddress(
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
            "Respond to direct address without breaking protected choice.",
            $"A direct address reached Adam: \"{message.Trim()}\".",
            [
                "notice the address",
                "preserve Adam's protected choice",
                "reject immediate puppeting",
                "select a lawful response mode",
                "record the selected response before any consequence"
            ],
            options,
            selected,
            "Adam reasons before response: the address is real contact, but no memory, belief, obedience, or consequence engine exists yet, so the safest lawful plan is to choose a response mode only.",
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
            [
                "hear the vessel",
                "separate speech from truth",
                "consider physically possible responses",
                "select one response mode",
                "record the choice for later memory and belief systems"
            ],
            options,
            selected,
            "Adam reasons before response: a speaking vessel creates contact, but the current brain only selects a response mode and leaves full belief, obedience, and consequence execution for later builds.",
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
            [
                "observe the living kind",
                "compare its form to Adam",
                "choose a name from the strongest impression",
                "record why the name was chosen",
                "record whether a suitable mate was found"
            ],
            ["observe", "name", "wait", "turn away"],
            selected,
            $"Adam reasons before naming: {namingReason.Trim()} The kind is not a suitable mate in the current Garden trial.",
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
