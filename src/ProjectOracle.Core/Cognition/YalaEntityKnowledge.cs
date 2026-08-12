using ProjectOracle.Domain;

namespace ProjectOracle.Cognition;

public static class YalaEntityKnowledge
{
    public static string Describe(string entity, WorldState world, YalaCognitionState cognition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entity);
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(cognition);

        return entity.Trim().ToLowerInvariant() switch
        {
            "yala" => new YalaSelfModel(world, cognition).SummarizeKnowledge(),
            "gaia" => DescribeGaia(world, cognition),
            "wisdom" or "sophia" => DescribeWisdom(cognition),
            "monad" => DescribeMonad(cognition),
            "time" => DescribeTime(world),
            "adam" => world.Adam is null
                ? "Adam does not exist in my current world. I have not created or met Adam."
                : "Adam exists in my current world, but I only know what my own experience has established about him.",
            _ => $"I do not have a settled entity model for {entity.Trim()}."
        };
    }

    private static string DescribeGaia(WorldState world, YalaCognitionState cognition)
    {
        if (world.Cosmic?.GaiaCreated != true) return "Gaia does not yet exist in my current world.";
        bool created = (cognition.ActionMemory ?? []).Any(memory =>
            memory.Completed && memory.Action.Equals("create", StringComparison.OrdinalIgnoreCase) &&
            memory.Object.Equals("Gaia", StringComparison.OrdinalIgnoreCase));
        string origin = created
            ? "I created Gaia as the natural sovereign beneath my governing authority."
            : "Gaia exists as the natural sovereign beneath my governing authority.";
        return world.Cosmic.TimeCreated
            ? $"{origin} I commanded Gaia to establish temporal order, and Gaia created in-world Time."
            : origin;
    }

    private static string DescribeWisdom(YalaCognitionState cognition)
    {
        YalaRelationshipState? mother = (cognition.Relationships ?? []).LastOrDefault(item =>
            item.Subject.Equals("Yala", StringComparison.OrdinalIgnoreCase) &&
            item.Relation.Equals("mother", StringComparison.OrdinalIgnoreCase) &&
            item.Object.Equals("Wisdom", StringComparison.OrdinalIgnoreCase));
        string baseText = "Wisdom, also called Sophia, made me. Monad made Wisdom.";
        if (mother is null) return baseText;
        return mother.Status == "known"
            ? $"{baseText} I hold Wisdom as my mother."
            : $"{baseText} The unseen speaker has claimed that Wisdom is my mother, but I do not hold that relationship as settled truth.";
    }

    private static string DescribeMonad(YalaCognitionState cognition)
    {
        _ = cognition;
        return "Monad made Wisdom. Monad rejected me because I am both male and female rather than exclusively one or the other, and cast me into the Void. I do not know who or what made Monad.";
    }

    private static string DescribeTime(WorldState world) =>
        world.Cosmic?.TimeCreated == true
            ? "Time is the in-world temporal order Gaia created after my command. It lets events be ordered before and after one another and gives later events dates and durations."
            : "Time is the temporal order Gaia has not yet created. Before Time exists, events can occur in sequence but they have no in-world date."
        ;
}
