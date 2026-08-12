using ProjectOracle.Domain;

namespace ProjectOracle.Cognition;

public static class YalaQuestionPlanner
{
    public const int AutonomousPriorityFloor = 85;

    public static YalaQuestionState? SelectNext(IReadOnlyList<YalaQuestionState>? questions) =>
        (questions ?? [])
            .Where(item => !item.Asked)
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.Id)
            .FirstOrDefault();

    public static YalaQuestionState? SelectNextAutonomous(YalaCognitionState cognition)
    {
        ArgumentNullException.ThrowIfNull(cognition);
        if (!string.IsNullOrWhiteSpace(cognition.PendingAutonomousUtterance)) return null;

        IReadOnlyList<YalaQuestionState> questions = cognition.Questions ?? [];
        long latestAskedDecision = questions
            .Where(item => item.AskedDecision.HasValue)
            .Select(item => item.AskedDecision.GetValueOrDefault())
            .DefaultIfEmpty(-1)
            .Max();
        long latestSpeakerResponseDecision = (cognition.Episodes ?? [])
            .Where(item => item.Kind.Equals("contact", StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Sequence)
            .DefaultIfEmpty(-1)
            .Max();

        // Once Yala asks something autonomously, give the unseen speaker a turn.
        // No dictionary-machine-gun behavior: another autonomous question becomes
        // eligible only after a later contact has actually been received.
        if (latestAskedDecision >= 0 && latestAskedDecision >= latestSpeakerResponseDecision)
        {
            return null;
        }

        return questions
            .Where(item => !item.Asked && item.Priority >= AutonomousPriorityFloor)
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.Id)
            .FirstOrDefault();
    }

    public static string SpeakerNatureQuestion => "What are you, beyond the words you use to name yourself?";

    public static string IdentityMeaningQuestion(string identity) => $"What does {identity} mean?";

    public static string UnknownWordQuestion(string word) => $"What does {word} mean?";

    public static string MotherReasonQuestion => "Why do you use the word mother for Wisdom?";

    public static string SpeakerPurposeQuestion => "Why are you speaking to me?";

    public static string SpeakerUnderstandingQuestion => "What do you want me to understand about you?";
}
