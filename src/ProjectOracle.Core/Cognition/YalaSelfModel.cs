using ProjectOracle.Domain;

namespace ProjectOracle.Cognition;

public sealed class YalaSelfModel
{
    private readonly WorldState _world;
    private readonly YalaCognitionState _cognition;

    public YalaSelfModel(WorldState world, YalaCognitionState cognition)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _cognition = cognition ?? throw new ArgumentNullException(nameof(cognition));
    }

    public string Identity => _world.Yala.TrueName;
    public string Nature => _world.Yala.Sex;
    public string Origin => "Wisdom made me.";
    public string Rejection => "Monad rejected me because I am both male and female rather than exclusively one or the other, and cast me into the Void.";
    public string Location => _world.Yala.Location;

    public bool HasPersonallyCreated(string name)
    {
        string key = NormalizeObject(name);
        return (_cognition.ActionMemory ?? []).Any(memory =>
            memory.Completed &&
            memory.Action.Equals("create", StringComparison.OrdinalIgnoreCase) &&
            NormalizeObject(memory.Object).Equals(key, StringComparison.OrdinalIgnoreCase));
    }

    public bool KnowsHasNotCreated(string name)
    {
        string key = NormalizeObject(name);
        if (HasPersonallyCreated(key)) return false;
        if (key == "adam" && _world.Adam is null) return true;
        if (key == "gaia" && _world.Cosmic?.GaiaCreated == false) return true;
        return false;
    }

    public IReadOnlyList<YalaKnowledgeProposition> DescribeKnowledge()
    {
        List<YalaKnowledgeProposition> propositions =
        [
            new($"I am {Identity}.", YalaKnowledgeSource.InheritedKnowledge, 1.0, true),
            new("I am both male and female.", YalaKnowledgeSource.InheritedKnowledge, 1.0, true),
            new(Origin, YalaKnowledgeSource.InheritedKnowledge, 1.0, true),
            new("Monad made Wisdom.", YalaKnowledgeSource.InheritedKnowledge, 1.0, true),
            new(Rejection, YalaKnowledgeSource.Remembered, 1.0, true),
            new($"I am in {Location}.", YalaKnowledgeSource.PersonallyExperienced, 1.0, true)
        ];

        foreach (YalaActionMemoryState memory in (_cognition.ActionMemory ?? []).Where(item => item.Completed))
        {
            propositions.Add(new YalaKnowledgeProposition(
                memory.Outcome,
                YalaKnowledgeSource.PersonallyPerformed,
                1.0,
                true));
        }

        if ((_cognition.Contacts?.Count ?? 0) > 0)
        {
            propositions.Add(new YalaKnowledgeProposition(
                "An unseen speaker has contacted me.",
                YalaKnowledgeSource.PersonallyExperienced,
                1.0,
                true));
        }

        return propositions
            .GroupBy(item => item.Proposition, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();
    }

    public string SummarizeKnowledge()
    {
        IReadOnlyList<YalaKnowledgeProposition> facts = DescribeKnowledge();
        return string.Join(" ", facts.Where(item => item.Settled).Select(item => item.Proposition));
    }

    private static string NormalizeObject(string value) => value.Trim().Trim('.', ',', '?', '!', '"', '\'').ToLowerInvariant();
}
