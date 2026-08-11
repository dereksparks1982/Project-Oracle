namespace ProjectOracle.Cognition.Soar;

public sealed record YalaDecision(
    string Action,
    string ReplyCode,
    string Source,
    string Detail,
    int DecisionCycles = 0,
    bool UsedSubstateDeliberation = false);

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
    YalaContactFrame? Contact = null)
{
    public bool HasContact => !string.IsNullOrWhiteSpace(ContactMessage);
    public YalaContactFrame ContactFrame => Contact ?? YalaContactFrame.None;
}

public sealed record YalaDirectReply(
    string Reply,
    YalaDecision Decision,
    YalaContactFrame Contact);
