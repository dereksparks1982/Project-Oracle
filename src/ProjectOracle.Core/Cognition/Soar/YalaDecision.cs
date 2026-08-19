namespace ProjectOracle.Cognition.Soar;

public sealed record YalaDecision(
    string Action,
    string ReplyCode,
    string Source,
    string Detail,
    int DecisionCycles = 0,
    bool UsedSubstateDeliberation = false,
    string? CosmicChoiceKey = null);

public sealed record YalaContactFrame(
    string SpeechAct,
    string Topic,
    string? ClaimedSpeakerName,
    bool KnownContact,
    bool AsksRememberMe,
    bool ContainsClaim,
    bool ClaimConflictsWithKnownFact,
    bool FactKnown,
    string? KnownFactAnswer,
    bool Ambiguous)
{
    public ProjectOracle.Cognition.Language.YalaUtterance? Language { get; init; }
    public string? ResolvedSubject { get; init; }
    public string? ResolvedAction { get; init; }
    public string? ResolvedObject { get; init; }
    public string? RelationshipRelation { get; init; }
    public string? RelationshipObject { get; init; }
    public string? PriorTopic { get; init; }

    public static YalaContactFrame None { get; } = new(
        "none", "none", null, false, false, false, false, false, null, false);
}

public sealed record YalaPerception(
    string Location,
    bool GaiaCreated,
    bool TimeCreated,
    long DecisionCount,
    string? LastAction,
    string? LastResult,
    int Curiosity,
    int Caution,
    int Authority,
    int Companionship,
    int Comfort,
    int Uncertainty,
    string? ContactMessage = null,
    YalaContactFrame? Contact = null,
    bool PendingQuestion = false,
    string? PendingQuestionText = null,
    bool HasSpeakerHistory = false)
{
    public bool HasContact => !string.IsNullOrWhiteSpace(ContactMessage);
    public YalaContactFrame ContactFrame => Contact ?? YalaContactFrame.None;
    public bool CosmicChoiceReady { get; init; }
    public IReadOnlyList<ProjectOracle.Cognition.CosmicChoice.YalaCosmicChoiceDefinition> CosmicChoices { get; init; } = [];
    public ProjectOracle.Domain.YalaDriveState? Drives { get; init; }
    public int PendingQuestionPriority { get; init; }
    public string ActiveConcernKey { get; init; } = "none";
    public int ActiveConcernPriority { get; init; }
    public int AppraisalThreat { get; init; }
    public int AppraisalSalience { get; init; }
}

public sealed record YalaDirectReply(
    string Reply,
    YalaDecision Decision,
    YalaContactFrame Contact);
