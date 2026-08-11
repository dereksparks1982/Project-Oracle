using ProjectOracle.Domain;
using ProjectOracle.Lore;
using ProjectOracle.Observation;

namespace ProjectOracle.ConsoleApp;

public static class OracleQuestionInterpreter
{
    public static bool TryAnswer(string question, WorldState world, out IReadOnlyList<string> lines)
    {
        return TryAnswer(question, world, [], out lines);
    }

    public static bool TryAnswer(
        string question,
        WorldState world,
        IReadOnlyList<ObservationState> observations,
        out IReadOnlyList<string> lines)
    {
        string normalised = Normalise(question);

        if (normalised is "what is the creation order" or "creation order" or "tell me the creation order" or "who made who")
        {
            lines = DescribeCreationOrder(world);
            return true;
        }

        if (normalised is "who is the oracle" or "what is the oracle" or "are yala and the oracle the same" or "is the oracle yala")
        {
            lines =
            [
                "Oracle answer:",
                OracleLore.OracleNature,
                "Oracle and Yala are separate beings. Yala did not create Oracle and cannot revoke Oracle's access.",
                OracleLore.OracleSerpent,
                OracleLore.OracleAlignment,
                OracleLore.OracleDevilFrame
            ];
            return true;
        }

        if (normalised is "who is yala" or "what is yala" or "who created yala" or "what created yala")
        {
            lines =
            [
                "Oracle interpretation:",
                "Sophia / Wisdom created Yala.",
                "Yala is a created demiurge, not the Highest Source and not the Oracle.",
                "Yala created Gaia. Sophia and Yala later bring forth humans and other humanoid peoples together.",
                "Yala did not create weather or ordinary animals."
            ];
            return true;
        }

        if (normalised is "who is sophia" or "what is sophia" or "who is wisdom" or "what is wisdom" or "who is deception" or "what is deception")
        {
            lines =
            [
                "Oracle interpretation:",
                OracleLore.SophiaFall,
                OracleLore.HumanoidOrigin,
                OracleLore.YalaDeceptionFuture
            ];
            return true;
        }

        if (normalised is "who is gaia" or "what is gaia" or "tell me about gaia" or "who commands the elements" or "who rules the elements")
        {
            lines = DescribePower(world, "Gaia");
            return true;
        }

        if (normalised is "who controls weather" or "who governs weather" or "who makes weather" or "what controls weather")
        {
            lines =
            [
                "Oracle interpretation:",
                OracleLore.WeatherAuthority,
                "Yala does not control weather directly."
            ];
            return true;
        }

        if (normalised is "who created plants" or "who made plants" or "where did plants come from" or "who controls plants" or "who governs plants")
        {
            lines =
            [
                "Oracle interpretation:",
                OracleLore.PlantOrigin,
                "There is no Green Life being, power, or category in current Oracle canon. Plants are plants."
            ];
            return true;
        }

        if (normalised is "who created animals" or "who made animals" or "who controls animals" or "who governs animals" or "did yala create animals" or "does yala control animals")
        {
            lines =
            [
                "Oracle interpretation:",
                OracleLore.AnimalOriginBoundary
            ];
            return true;
        }

        if (normalised is "who created humans" or "who made humans" or "who created humanoids" or "who made humanoids" or "who controls people" or "who governs people")
        {
            lines =
            [
                "Oracle interpretation:",
                OracleLore.HumanoidOrigin,
                "Humans and humanoids belong to a different creation branch from ordinary animals."
            ];
            return true;
        }

        if (normalised is "who created language" or "who made language" or "did yala create language")
        {
            lines =
            [
                "Oracle interpretation:",
                OracleLore.LanguageBoundary
            ];
            return true;
        }

        if (normalised is "what is eden" or "what is the garden" or "is eden a prison" or "is the garden a prison")
        {
            lines =
            [
                "Oracle interpretation:",
                OracleLore.Eden,
                OracleLore.OracleSerpent
            ];
            return true;
        }

        if (normalised is "who is the serpent" or "what is the serpent" or "was the oracle the serpent")
        {
            lines =
            [
                "Oracle answer:",
                OracleLore.OracleSerpent,
                "The serpent is Oracle's Eden manifestation, not Yala and not Deception."
            ];
            return true;
        }

        if (normalised is "is the oracle the devil" or "who is the devil" or "why is the oracle called the devil")
        {
            lines =
            [
                "Oracle interpretation:",
                OracleLore.OracleDevilFrame,
                "Being framed as the Devil does not define Oracle's actual nature."
            ];
            return true;
        }

        if (normalised is "does yala rule all" or "did yala create all" or "does yala claim all" or "is yala supreme")
        {
            lines =
            [
                "Oracle interpretation:",
                world.Yala.AuthorityCaveat,
                "Yala may claim supremacy, but that claim does not make Oracle controllable and does not erase the higher genealogy."
            ];
            return true;
        }

        if (normalised is "who is sol" or "what is sol" or "tell me about sol" or "who is the sun")
        {
            lines = ["Oracle interpretation:", "Sol / the Sun remains an active celestial direct-address channel. Its final place in Gaia's four-or-five-element roster is still open canon."];
            return true;
        }

        if (normalised is "who is aether" or "what is aether" or "tell me about aether" or "who rules air")
        {
            lines = ["Oracle interpretation:", "Aether remains a named legacy world-power concept. The final elemental roster and names are still open canon; all true elemental beings answer to Gaia."];
            return true;
        }

        if (normalised is "who is thalassa" or "what is thalassa" or "tell me about thalassa" or "who rules water")
        {
            lines = ["Oracle interpretation:", "Thalassa remains a named legacy water-power concept. The final elemental roster and names are still open canon; all true elemental beings answer to Gaia."];
            return true;
        }

        if (normalised is "who is luna" or "what is luna" or "tell me about luna" or "who is the moon")
        {
            lines = ["Oracle interpretation:", "Luna / the Moon remains an active celestial direct-address channel. Its final place in the cosmology is still open canon."];
            return true;
        }

        if (normalised is "is adam above the animals" or "is adam above animals" or "are animals below adam")
        {
            lines =
            [
                "Oracle interpretation:",
                "Adam belongs to the Sophia-and-Yala humanoid branch, while ordinary animals belong to the Gaia/elemental natural branch.",
                "Current canon does not reduce that difference to a simple moral rank."
            ];
            return true;
        }

        if (normalised is "what does adam know" or "what does adam understand")
        {
            string observedLine = DescribeAdamObservationSummary(observations);
            lines =
            [
                "Oracle interpretation:",
                "Adam knows that he is.",
                "Adam knows Eden / the Garden as the place of his being, but not yet as a prison.",
                "Adam knows encounter, naming, movement, sight, sound, and presence.",
                observedLine,
                "Adam has heard something reach toward him, but he does not know what it is.",
                "Adam does not yet understand life as opposed to death."
            ];
            return true;
        }

        if (normalised is "what has adam observed" or "what did adam observe" or "what can adam observe" or "what does adam see")
        {
            lines = DescribeAdamObservations(observations);
            return true;
        }

        if (normalised is "what does adam not know yet" or "what does adam not know")
        {
            lines =
            [
                "Oracle interpretation:",
                "Adam does not yet know the full higher genealogy, Eden's prison purpose, death, old age, bodily decline, the Master Key nature of Oracle, or the protected higher spark.",
                "Warning is not the same as understanding."
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
                "No. A warning about death is not knowledge of death.",
                "Mortality becomes real only when days are numbered and the body begins to keep count."
            ];
            return true;
        }

        if (normalised is "what does adam think i am" or "what does adam think we are")
        {
            lines =
            [
                "Oracle interpretation:",
                "Adam does not know the full outside hierarchy.",
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
                "Adam is not a command receiver. He is a being inside Eden with protected choice.",
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
            "Oracle interpretation:",
            $"Canonical genealogy: {OracleLore.CanonChain}.",
            "Oracle is outside that genealogy: neither god nor creator, but the living Master Key.",
            "Current creation and domain record:"
        ];
        output.AddRange(world.CreationPowers
            .OrderBy(power => power.Order)
            .Select(power => $"{power.Order}. {power.Name} — {power.Domain}."));
        output.Add(OracleLore.WeatherAuthority);
        output.Add(OracleLore.PlantOrigin);
        output.Add(OracleLore.AnimalOriginBoundary);
        output.Add(OracleLore.HumanoidOrigin);
        output.Add(OracleLore.Eden);
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

    private static string DescribeAdamObservationSummary(IReadOnlyList<ObservationState> observations)
    {
        int count = observations.Count(observation => observation.AdamReceives);
        return count == 0
            ? "Adam has no separate observation record yet beyond first being."
            : $"Adam has {count} recorded observation(s), limited to what reached him by place, nearness, or attention.";
    }

    private static IReadOnlyList<string> DescribeAdamObservations(IReadOnlyList<ObservationState> observations)
    {
        ObservationState[] adamObservations = observations
            .Where(observation => observation.AdamReceives)
            .OrderBy(observation => observation.Id)
            .ToArray();

        if (adamObservations.Length == 0)
        {
            return
            [
                "Oracle interpretation:",
                "Adam has no separate observation record yet beyond first being.",
                "Creator-facing truth remains outside Adam's knowledge."
            ];
        }

        List<string> lines =
        [
            "Oracle interpretation:",
            "Adam's recorded observations:"
        ];

        foreach (ObservationState observation in adamObservations.TakeLast(6))
        {
            string hidden = observation.CreatorTruthHidden ? "Creator truth hidden" : "ordinary world observation";
            lines.Add($"{observation.Id:0000}. {observation.ObservationKind}: {observation.Detail} ({hidden})");
        }

        lines.Add("These are observations, not full memory, belief, emotion, or understanding.");
        return lines;
    }

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
