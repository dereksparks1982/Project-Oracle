namespace ProjectOracle.Cognition.Soar;

public sealed record YalaDecision(
    string Action,
    string ReplyCode,
    string Source,
    string Detail);

public sealed record YalaPerception(
    string Location,
    bool GaiaCreated,
    bool TimeCreated,
    long DecisionCount,
    string? LastAction,
    string? LastResult,
    string? ContactMessage = null,
    string ContactIntent = "none")
{
    public bool HasContact => !string.IsNullOrWhiteSpace(ContactMessage);
}

public sealed record YalaDirectReply(
    string Reply,
    YalaDecision Decision);
