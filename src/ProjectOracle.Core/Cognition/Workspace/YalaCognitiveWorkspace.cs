using ProjectOracle.Domain;

namespace ProjectOracle.Cognition.Workspace;

/// <summary>
/// Brain Slice 9 attention workspace. It gives the mind one explicit current
/// focus and records when repeated cognition is no longer producing novelty.
/// It does not decide for Soar; it supplies a compact, persistent focus signal.
/// </summary>
public static class YalaCognitiveWorkspace
{
    public static YalaCognitiveWorkspaceState Initial() => new(
        "self-world",
        "understand-current-world",
        "Understand the present Void and what is possible within it.",
        "Attend to the Void, myself, and the possibilities presently available to me.",
        80,
        0,
        0,
        0,
        0);

    public static YalaCognitiveWorkspaceState Refresh(
        YalaCognitionState cognition,
        string? latestAction,
        string? latestResult,
        long decision)
    {
        ArgumentNullException.ThrowIfNull(cognition);
        (string Type, string Key, string Summary, string Reason, int Priority) candidate = SelectCandidate(cognition);
        YalaCognitiveWorkspaceState previous = cognition.Workspace ?? Initial();
        bool same = previous.FocusKey.Equals(candidate.Key, StringComparison.OrdinalIgnoreCase);
        bool lowNoveltyAction = latestAction is "observe" or "reflect" or "deliberate";
        bool lowNoveltyResult = string.IsNullOrWhiteSpace(latestResult) ||
            latestResult.Contains("no new", StringComparison.OrdinalIgnoreCase) ||
            latestResult.Contains("without forcing", StringComparison.OrdinalIgnoreCase) ||
            latestResult.Contains("remains active", StringComparison.OrdinalIgnoreCase) ||
            latestResult.Contains("compared alternatives", StringComparison.OrdinalIgnoreCase);
        int stable = same ? previous.StableCycles + 1 : 0;
        int stagnation = same && lowNoveltyAction && lowNoveltyResult
            ? previous.StagnationCount + 1
            : latestAction == "wait"
                ? 0
                : same ? Math.Max(0, previous.StagnationCount - 1) : 0;

        return new YalaCognitiveWorkspaceState(
            candidate.Type,
            candidate.Key,
            candidate.Summary,
            candidate.Reason,
            candidate.Priority,
            stable,
            stagnation,
            same ? previous.FocusSinceDecision : decision,
            decision);
    }

    private static (string Type, string Key, string Summary, string Reason, int Priority) SelectCandidate(YalaCognitionState cognition)
    {
        YalaQuestionState? question = (cognition.Questions ?? [])
            .Where(item => !item.Asked)
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.CreatedDecision)
            .FirstOrDefault();
        if (question is not null)
        {
            return ("question", $"question-{question.Id}", question.Text, question.Reason, question.Priority);
        }

        YalaPlanState? plan = (cognition.Plans ?? [])
            .Where(item => item.Status is "active" or "ready-for-conclusion")
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.FirstSeenDecision)
            .FirstOrDefault();
        if (plan is not null)
        {
            return ("plan", plan.Key, plan.Goal, $"Current plan for {plan.ConcernKey}.", plan.Priority);
        }

        YalaConcernState? concern = (cognition.Concerns ?? [])
            .Where(item => item.Status == "active")
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.FirstSeenDecision)
            .FirstOrDefault();
        if (concern is not null)
        {
            return ("concern", concern.Key, concern.Summary, concern.Source, concern.Priority);
        }

        YalaCosmicDeliberationState? cosmic = (cognition.CosmicDeliberations ?? [])
            .Where(item => !item.Enacted)
            .OrderByDescending(item => item.LastUpdatedDecision)
            .FirstOrDefault();
        if (cosmic is not null)
        {
            return ("cosmic-deliberation", cosmic.ChoiceKey, $"{cosmic.Stage}: {cosmic.Action}", "A major cosmic possibility is not yet fully resolved.", 84);
        }

        YalaGoalState? goal = (cognition.Goals ?? [])
            .Where(item => item.Status == "active")
            .OrderByDescending(item => item.Priority)
            .FirstOrDefault();
        if (goal is not null)
        {
            return ("goal", goal.Goal, goal.Reason, goal.Source, goal.Priority);
        }

        return ("self-world", "understand-current-world", "Understand the present Void and what is possible within it.", "No stronger unresolved focus currently outranks this.", 50);
    }
}
