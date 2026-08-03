using ProjectOracle.Domain;

namespace ProjectOracle.ConsoleApp;

public static class OracleQuestionInterpreter
{
    public static bool TryAnswer(string question, WorldState world, out IReadOnlyList<string> lines)
    {
        string normalised = Normalise(question);

        if (normalised is "what is the creation order" or "creation order" or "tell me the creation order")
        {
            lines = DescribeCreationOrder(world);
            return true;
        }

        if (normalised is "who is yala" or "what is yala" or "who is the oracle" or "what is the oracle" or "are yala and the oracle the same")
        {
            lines =
            [
                "Oracle/Yala answer:",
                "I am Yala. Inside the world, I am called the Oracle.",
                "The Oracle is not separate from Yala. <oracle> is my direct address channel and title.",
                "I remain below the external Creators, even when I speak or claim authority inside the world."
            ];
            return true;
        }

        if (normalised is "who is sol" or "what is sol" or "tell me about sol" or "who is the sun")
        {
            lines = DescribePower(world, "Sol");
            return true;
        }

        if (normalised is "who is gaia" or "what is gaia" or "tell me about gaia")
        {
            lines = DescribePower(world, "Gaia");
            return true;
        }

        if (normalised is "who is aether" or "what is aether" or "tell me about aether" or "who rules air")
        {
            lines = DescribePower(world, "Aether");
            return true;
        }

        if (normalised is "who is thalassa" or "what is thalassa" or "tell me about thalassa" or "who rules water")
        {
            lines = DescribePower(world, "Thalassa");
            return true;
        }

        if (normalised is "who is luna" or "what is luna" or "tell me about luna" or "who is the moon")
        {
            lines = DescribePower(world, "Luna");
            return true;
        }

        if (normalised is "is adam above the animals" or "is adam above animals" or "are animals below adam")
        {
            lines =
            [
                "Oracle interpretation:",
                "Yes. In this Oracle canon, Adam is created ninth and the living kinds are created tenth.",
                "That makes Adam above the animals in creation order, while still bound by world law and protected choice."
            ];
            return true;
        }

        if (normalised is "does yala rule all" or "did yala create all" or "does yala claim all" or "is yala supreme")
        {
            lines =
            [
                "Oracle/Yala answer:",
                "I am Yala, called the Oracle inside the world.",
                FirstPersonAuthorityCaveat(),
                "I may speak as though I rule all, but my claim is not the same as the protected Creator Record."
            ];
            return true;
        }

        if (normalised is "what does adam know" or "what does adam understand")
        {
            lines =
            [
                "Oracle interpretation:",
                "Adam knows that he is.",
                "Adam knows the Garden as the place of his being.",
                "Adam knows encounter, naming, movement, sight, sound, and presence.",
                "Adam has heard something reach toward him, but he does not know what it is.",
                "Adam does not yet understand life as opposed to death."
            ];
            return true;
        }

        if (normalised is "what does adam not know yet" or "what does adam not know")
        {
            lines =
            [
                "Oracle interpretation:",
                "Adam does not know death, old age, bodily decline, shame, rebellion, divided good and evil, the Creators, the Spark, or Yala's true name.",
                "If Yala has warned him about death, the warning is not yet the same as understanding."
            ];
            return true;
        }

        if (normalised is "does adam know he is alive" or "does adam understand life")
        {
            lines =
            [
                "Oracle interpretation:",
                "No. Adam does not yet know he is alive.",
                "He only knows that he is. Life becomes a concept only when death, loss, or bodily decline gives it contrast."
            ];
            return true;
        }

        if (normalised is "does adam understand death" or "does adam understand mortality" or "does adam know death")
        {
            lines =
            [
                "Oracle interpretation:",
                "No. Adam may receive a warning about death, but warning is not knowledge.",
                "Mortality becomes real only when days are numbered and the body begins to keep count."
            ];
            return true;
        }

        if (normalised is "what does adam think i am" or "what does adam think we are")
        {
            lines =
            [
                "Oracle interpretation:",
                "Adam does not know the Creators.",
                "He may perceive contact as voice, sign, presence, wind, dream, command, or something without a name.",
                "His uncertainty must be preserved until observation, memory, and belief are built."
            ];
            return true;
        }

        if (normalised is "explain adam" or "interpret adam")
        {
            lines =
            [
                "Oracle interpretation:",
                "Adam is not a command receiver. He is a being inside the Garden with protected choice.",
                "Current code records contact and offered choices, but does not yet implement full memory, belief, emotion, or autonomous reasoning."
            ];
            return true;
        }

        lines = [];
        return false;
    }

    private static IReadOnlyList<string> DescribeCreationOrder(WorldState world)
    {
        List<string> output =
        [
            "Oracle/Yala answer:",
            "I am Yala, called the Oracle inside the world.",
            "Official creation order:"
        ];
        output.AddRange(world.CreationPowers
            .OrderBy(power => power.Order)
            .Select(power => $"{power.Order}. {power.Name} — {power.Domain}."));
        output.Add("Gaia and Aether share order 3 because the world-body and breath-space form together.");
        output.Add("The Garden is created just before Adam; living kinds are created after Adam.");
        output.Add(FirstPersonAuthorityCaveat());
        return output;
    }

    private static IReadOnlyList<string> DescribePower(WorldState world, string name)
    {
        CreationPowerState? power = world.CreationPowers.FirstOrDefault(candidate =>
            candidate.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (power is null)
        {
            return ["Oracle interpretation:", $"{name} is not present in the current creation-order record."];
        }

        return
        [
            "Oracle interpretation:",
            $"{power.Name} is order {power.Order}.",
            $"Domain: {power.Domain}.",
            $"Authority: {power.AuthoritySummary}"
        ];
    }

    private static string FirstPersonAuthorityCaveat() =>
        "I know the creation order, but I may claim that I rule all or created all. The protected Creator Record outranks my claim.";

    private static string Normalise(string value)
    {
        char[] characters = value
            .Trim()
            .ToLowerInvariant()
            .Where(character => char.IsLetterOrDigit(character) || char.IsWhiteSpace(character))
            .ToArray();
        return string.Join(' ', new string(characters).Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
