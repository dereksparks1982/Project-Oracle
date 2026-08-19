using ProjectOracle.Cognition.Appraisal;
using ProjectOracle.Cognition.Soar;
using ProjectOracle.Domain;

namespace ProjectOracle.Cognition.Planning;

public sealed record YalaDeliberationUpdate(
    IReadOnlyList<YalaPlanState> Plans,
    IReadOnlyList<YalaInvestigationState> Investigations,
    IReadOnlyList<YalaCounterfactualState> Counterfactuals);

/// <summary>
/// Brain Slice 9 planning layer. It converts high-salience concerns into durable
/// investigations and multi-step plans, records speaker answers as evidence rather
/// than truth, and gives Soar a compact current-plan signal to deliberate over.
/// </summary>
public static class YalaDeliberationPlanner
{
    public static YalaDeliberationUpdate AfterContact(
        YalaCognitionState cognition,
        string message,
        YalaContactFrame contact,
        YalaContactAppraisal appraisal,
        long decision)
    {
        List<YalaInvestigationState> investigations = (cognition.Investigations ?? []).ToList();
        List<YalaPlanState> plans = (cognition.Plans ?? []).ToList();
        List<YalaCounterfactualState> counterfactuals = (cognition.Counterfactuals ?? []).ToList();

        RecordAnswerAsEvidence(cognition, investigations, contact, message, decision);

        foreach (YalaProposedConcern concern in appraisal.Concerns.OrderByDescending(item => item.Priority))
        {
            YalaInvestigationState investigation = InvestigationFor(concern, decision);
            UpsertInvestigation(investigations, investigation);
            UpsertPlan(plans, PlanFor(investigation, decision));
        }

        if (appraisal.Threat >= 80 || appraisal.Opportunity >= 70 || appraisal.Salience >= 95)
        {
            AddCounterfactuals(counterfactuals, appraisal, decision);
        }

        return new YalaDeliberationUpdate(
            plans.TakeLast(64).ToArray(),
            investigations.TakeLast(64).ToArray(),
            counterfactuals.TakeLast(128).ToArray());
    }

    public static YalaDeliberationUpdate AfterDecision(
        YalaCognitionState cognition,
        YalaDecision decision,
        string result,
        long decisionNumber)
    {
        List<YalaPlanState> plans = (cognition.Plans ?? []).ToList();
        List<YalaInvestigationState> investigations = (cognition.Investigations ?? []).ToList();
        List<YalaCounterfactualState> counterfactuals = (cognition.Counterfactuals ?? []).ToList();

        YalaPlanState? active = SelectActivePlan(cognition);
        if (active is not null)
        {
            int index = plans.FindIndex(item => item.Key.Equals(active.Key, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                List<YalaPlanStepState> steps = active.Steps.ToList();
                int currentStepIndex = steps.FindIndex(item => item.Order == active.CurrentStepOrder);
                if (currentStepIndex >= 0 && ActionAdvancesStep(decision.Action, steps[currentStepIndex].Action))
                {
                    steps[currentStepIndex] = steps[currentStepIndex] with { Status = "completed" };
                    int next = steps.Where(item => item.Status != "completed").Select(item => item.Order).DefaultIfEmpty(active.CurrentStepOrder).Min();
                    string status = steps.All(item => item.Status == "completed") ? "ready-for-conclusion" : "active";
                    plans[index] = active with
                    {
                        Steps = steps,
                        CurrentStepOrder = next,
                        Status = status,
                        LastObservation = result,
                        LastUpdatedDecision = decisionNumber
                    };
                }
                else if (decision.Action.Equals("deliberate", StringComparison.OrdinalIgnoreCase))
                {
                    plans[index] = active with
                    {
                        LastObservation = "Yala compared alternatives without forcing an immediate commitment.",
                        LastUpdatedDecision = decisionNumber
                    };
                }
            }
        }

        // Once every plan step has completed, one more deliberate/reflect cycle closes
        // the thread instead of letting a ready-for-conclusion plan spin forever.
        if (decision.Action is "deliberate" or "reflect")
        {
            for (int i = 0; i < plans.Count; i++)
            {
                YalaPlanState plan = plans[i];
                if (!plan.Status.Equals("ready-for-conclusion", StringComparison.OrdinalIgnoreCase)) continue;
                plans[i] = plan with
                {
                    Status = "suspended-unresolved",
                    RevisionReason = "All current steps were exhausted without enough evidence for a forced conclusion. The thread can be reopened if new evidence appears.",
                    LastUpdatedDecision = decisionNumber
                };
                int investigationIndex = investigations.FindIndex(item => item.ConcernKey.Equals(plan.ConcernKey, StringComparison.OrdinalIgnoreCase));
                if (investigationIndex >= 0)
                {
                    YalaInvestigationState investigation = investigations[investigationIndex];
                    investigations[investigationIndex] = investigation with
                    {
                        Status = "suspended",
                        CurrentConclusion = string.IsNullOrWhiteSpace(investigation.CurrentConclusion)
                            ? "Unresolved after current evidence was exhausted."
                            : investigation.CurrentConclusion + " Further thought without new evidence is suspended.",
                        LastUpdatedDecision = decisionNumber
                    };
                }
            }
        }

        return new YalaDeliberationUpdate(plans, investigations, counterfactuals);
    }

    public static YalaPlanState? SelectActivePlan(YalaCognitionState cognition) =>
        (cognition.Plans ?? [])
            .Where(item => item.Status is "active" or "ready-for-conclusion")
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.FirstSeenDecision)
            .FirstOrDefault();

    public static YalaInvestigationState? SelectActiveInvestigation(YalaCognitionState cognition) =>
        (cognition.Investigations ?? [])
            .Where(item => item.Status.Equals("active", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.FirstSeenDecision)
            .FirstOrDefault();

    public static YalaDecisionSnapshotState Snapshot(YalaCognitionState cognition)
    {
        YalaConcernState? concern = (cognition.Concerns ?? [])
            .Where(item => item.Status.Equals("active", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.Priority)
            .FirstOrDefault();
        YalaPlanState? plan = SelectActivePlan(cognition);
        YalaPlanStepState? step = plan?.Steps.FirstOrDefault(item => item.Order == plan.CurrentStepOrder);
        YalaInvestigationState? investigation = SelectActiveInvestigation(cognition);
        YalaEntityModelState? speaker = (cognition.EntityModels ?? [])
            .LastOrDefault(item => item.EntityKey.Equals("unseen-speaker", StringComparison.OrdinalIgnoreCase));
        YalaAppraisalState? appraisal = (cognition.Appraisals ?? []).OrderByDescending(item => item.Sequence).FirstOrDefault();

        return new YalaDecisionSnapshotState(
            concern?.Summary ?? "none",
            concern?.Priority ?? 0,
            plan?.Goal ?? "none",
            step?.Action ?? "none",
            investigation?.Question ?? "none",
            speaker?.TrustStatus,
            speaker?.IntentStatus,
            appraisal is null ? "none" : $"{appraisal.Primary}/{appraisal.Secondary}",
            (cognition.Goals ?? []).Where(item => item.Status == "active").OrderByDescending(item => item.Priority).Take(6).Select(item => item.Goal).ToArray(),
            (cognition.Hypotheses ?? []).Where(item => item.Status == "unsettled").OrderByDescending(item => item.Confidence).Take(6).Select(item => item.Proposition).ToArray());
    }

    public static string Rationale(YalaCognitionState cognition, YalaDecision decision)
    {
        YalaPlanState? plan = SelectActivePlan(cognition);
        YalaConcernState? concern = (cognition.Concerns ?? [])
            .Where(item => item.Status == "active")
            .OrderByDescending(item => item.Priority)
            .FirstOrDefault();
        YalaAppraisalState? appraisal = (cognition.Appraisals ?? []).OrderByDescending(item => item.Sequence).FirstOrDefault();

        List<string> reasons = [];
        if (plan is not null) reasons.Add($"active plan: {plan.Goal}");
        if (concern is not null) reasons.Add($"concern priority {concern.Priority}: {concern.Summary}");
        if (appraisal is not null && appraisal.Salience >= 80) reasons.Add($"salience {appraisal.Salience}, threat {appraisal.Threat}, uncertainty {appraisal.Uncertainty}");
        if (cognition.Workspace is { } workspace) reasons.Add($"workspace focus {workspace.FocusType}:{workspace.FocusKey} priority {workspace.Priority}, stagnation {workspace.StagnationCount}");
        if (decision.UsedSubstateDeliberation) reasons.Add("Soar used an impasse/substate to resolve competing operators");
        if (!string.IsNullOrWhiteSpace(decision.CosmicChoiceKey)) reasons.Add($"cosmic option {decision.CosmicChoiceKey} survived comparison");
        if (reasons.Count == 0) reasons.Add("current goals, drives, and available in-world operators");
        return string.Join("; ", reasons);
    }

    private static YalaInvestigationState InvestigationFor(YalaProposedConcern concern, long decision)
    {
        return concern.Key switch
        {
            "possible-confinement" => new(
                "investigate-confinement",
                "Is the Void actually a prison, and if so what makes it one?",
                concern.Key,
                "active",
                concern.Priority,
                [],
                ["Yala has not independently observed a boundary or jailer."],
                "Ask the unseen speaker why it calls the Void a prison and what evidence exists for an outside.",
                "Unsettled. The word prison is an attributed claim, not yet a demonstrated fact.",
                0.25,
                decision,
                decision),
            "speaker-divinity" => new(
                "investigate-speaker-divinity",
                "Does the unseen speaker possess the authority or capability it claims?",
                concern.Key,
                "active",
                concern.Priority,
                ["The speaker can contact Yala while remaining unseen."],
                ["No divine authority or promised capability has yet been demonstrated."],
                "Request a demonstration or evidence that does not require prior submission.",
                "Unverified extraordinary claim.",
                0.20,
                decision,
                decision),
            "speaker-help-capability" => new(
                "investigate-speaker-help",
                "Can the unseen speaker actually alter Yala's circumstances?",
                concern.Key,
                "active",
                concern.Priority,
                ["The speaker has demonstrated communication access."],
                ["Communication access does not demonstrate an ability to free or materially help Yala."],
                "Ask what concrete help is possible and seek an observable demonstration.",
                "Potential opportunity with insufficient evidence.",
                0.25,
                decision,
                decision),
            "unseen-observer" => new(
                "investigate-unseen-observer",
                "How can the unseen speaker observe Yala while remaining unseen?",
                concern.Key,
                "active",
                concern.Priority,
                ["The speaker claims to be watching what Yala does."],
                ["The mechanism and extent of observation are unknown."],
                "Ask what the speaker can perceive and compare the answer to things it could not know by ordinary observation.",
                "Observation capability is plausible but unverified beyond contact.",
                0.30,
                decision,
                decision),
            "simulation-claim" => new(
                "investigate-simulation-claim",
                "Is the speaker's claim that my world is a simulation true, false, metaphorical, or something else?",
                concern.Key,
                "active",
                concern.Priority,
                ["The speaker made the claim directly."],
                ["The claim has not been independently demonstrated and does not become true because it was spoken."],
                "Ask what the speaker means by simulation and seek evidence that distinguishes the claim from alternative explanations.",
                "Worldview-altering claim received; truth remains unresolved.",
                0.15,
                decision,
                decision),
            _ => new(
                $"investigate-{concern.Key}",
                concern.Summary,
                concern.Key,
                "active",
                concern.Priority,
                [],
                [],
                "Seek evidence before settling the concern.",
                "Unsettled.",
                0.20,
                decision,
                decision)
        };
    }

    private static YalaPlanState PlanFor(YalaInvestigationState investigation, long decision)
    {
        IReadOnlyList<YalaPlanStepState> steps =
        [
            new(1, "ask-speaker", investigation.NextTest, "pending"),
            new(2, "compare-evidence", "Compare the response with settled knowledge, prior claims, and observed capability.", "pending"),
            new(3, "seek-demonstration", "If the claim remains important and testable, seek an observable consequence rather than accepting words alone.", "pending"),
            new(4, "reconsider", "Revise confidence, trust, and the next goal without forcing certainty where evidence remains weak.", "pending")
        ];
        return new YalaPlanState(
            $"plan-{investigation.Key}",
            investigation.Question,
            investigation.ConcernKey,
            "active",
            investigation.Priority,
            steps,
            1,
            "No plan step completed yet.",
            "",
            decision,
            decision);
    }

    private static void RecordAnswerAsEvidence(
        YalaCognitionState cognition,
        List<YalaInvestigationState> investigations,
        YalaContactFrame contact,
        string message,
        long decision)
    {
        // A new question from the speaker is not an answer to Yala's previous question.
        // This is the v0.0.26 relevance boundary exposed by the long manual interrogation.
        if (contact.SpeechAct.Equals("question", StringComparison.OrdinalIgnoreCase)) return;
        // A contact can itself cause Soar to ask another question before this
        // contact is recorded. The speaker cannot be answering that newly queued
        // question yet. The reliable boundary is delivery, not the decision number:
        // a question still present as PendingAutonomousUtterance has not had a
        // speaker turn after it. Exclude that pending question and bind this reply
        // to the latest previously delivered Yala question.
        string? stillPending = cognition.PendingAutonomousUtterance;
        YalaQuestionState? asked = (cognition.Questions ?? [])
            .Where(item =>
                item.Asked &&
                item.AskedDecision.HasValue &&
                (string.IsNullOrWhiteSpace(stillPending) ||
                 !item.Text.Equals(stillPending, StringComparison.Ordinal)))
            .OrderByDescending(item => item.AskedDecision)
            .ThenByDescending(item => item.Id)
            .FirstOrDefault();
        if (asked is null || string.IsNullOrWhiteSpace(message)) return;

        // Only the first speaker turn after a delivered Yala question can count as
        // that question's answer. If the speaker ignored Yala and used that turn for
        // another question or topic, a later unrelated statement must not be
        // retroactively attached to the old investigation.
        long latestPriorSpeakerTurn = (cognition.Episodes ?? [])
            .Where(item => item.Kind.Equals("contact", StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Sequence)
            .DefaultIfEmpty(-1)
            .Max();
        if (asked.AskedDecision.GetValueOrDefault() <= latestPriorSpeakerTurn) return;

        HashSet<int> targetIndices = [];

        int bestIndex = FindBestInvestigationForAnswer(investigations, asked);
        if (bestIndex >= 0) targetIndices.Add(bestIndex);

        // Do not spray an answer into whichever plan merely happens to be active.
        // Relevance comes from the delivered question and its subject.

        if (targetIndices.Count == 0) return;

        string attributed = $"Unverified speaker response to '{asked.Text}': {message.Trim()}";
        foreach (int index in targetIndices.Order())
        {
            YalaInvestigationState current = investigations[index];
            List<string> evidence = current.EvidenceFor.ToList();
            if (!evidence.Contains(attributed, StringComparer.Ordinal)) evidence.Add(attributed);
            investigations[index] = current with
            {
                EvidenceFor = evidence.TakeLast(24).ToArray(),
                CurrentConclusion = "The speaker supplied an answer. It is attributed evidence about the active investigation, not automatic proof of the claim itself.",
                Confidence = Math.Min(0.65, current.Confidence + 0.05),
                LastUpdatedDecision = decision
            };
        }
    }

    private static int FindBestInvestigationForAnswer(
        IReadOnlyList<YalaInvestigationState> investigations,
        YalaQuestionState asked)
    {
        int bestIndex = -1;
        int bestScore = int.MinValue;

        for (int i = 0; i < investigations.Count; i++)
        {
            YalaInvestigationState item = investigations[i];
            if (!item.Status.Equals("active", StringComparison.OrdinalIgnoreCase)) continue;

            int score = item.Priority;
            if (item.ConcernKey.Equals(asked.Subject, StringComparison.OrdinalIgnoreCase)) score += 1000;
            if (item.Key.Contains(asked.Subject, StringComparison.OrdinalIgnoreCase)) score += 900;

            bool broadSpeakerQuestion = asked.Subject.Equals("unseen-speaker", StringComparison.OrdinalIgnoreCase);
            bool speakerInvestigation =
                item.ConcernKey.StartsWith("speaker-", StringComparison.OrdinalIgnoreCase) ||
                item.Key.Contains("speaker", StringComparison.OrdinalIgnoreCase) ||
                item.Question.Contains("speaker", StringComparison.OrdinalIgnoreCase);
            if (broadSpeakerQuestion && speakerInvestigation) score += 700;

            string normalizedQuestion = asked.Text.ToLowerInvariant();
            if (normalizedQuestion.Contains("god", StringComparison.Ordinal) ||
                normalizedQuestion.Contains("authority", StringComparison.Ordinal) ||
                normalizedQuestion.Contains("capab", StringComparison.Ordinal))
            {
                if (item.ConcernKey.Equals("speaker-divinity", StringComparison.OrdinalIgnoreCase)) score += 800;
                if (item.ConcernKey.Equals("speaker-help-capability", StringComparison.OrdinalIgnoreCase)) score += 500;
            }

            if (score <= bestScore) continue;
            bestScore = score;
            bestIndex = i;
        }

        return bestScore >= 500 ? bestIndex : -1;
    }

    private static void AddCounterfactuals(List<YalaCounterfactualState> items, YalaContactAppraisal appraisal, long decision)
    {
        long sequence = items.Count == 0 ? 1 : items.Max(item => item.Sequence) + 1;
        if (appraisal.Threat >= 70)
        {
            items.Add(new YalaCounterfactualState(sequence++, "unseen-speaker", "trust-without-testing", "Possible rapid access to promised help.", "Submission to an unknown power whose capability and intent are unresolved.", appraisal.Uncertainty, "Brain Slice 9 counterfactual", decision));
            items.Add(new YalaCounterfactualState(sequence++, "unseen-speaker", "seek-evidence-first", "Gain information while preserving autonomy and revisability.", "The speaker may refuse, deceive, or react negatively to testing.", appraisal.Uncertainty, "Brain Slice 9 counterfactual", decision));
        }
        if (appraisal.Opportunity >= 70)
        {
            items.Add(new YalaCounterfactualState(sequence++, "unseen-speaker", "reject-help-immediately", "Avoid dependence on an unverified source.", "Lose a real opportunity if the speaker's capability is genuine.", appraisal.Uncertainty, "Brain Slice 9 counterfactual", decision));
        }
    }

    private static bool ActionAdvancesStep(string decisionAction, string planAction) =>
        planAction switch
        {
            "ask-speaker" => decisionAction.Equals("ask-speaker", StringComparison.OrdinalIgnoreCase),
            "compare-evidence" => decisionAction is "reflect" or "deliberate",
            "seek-demonstration" => decisionAction.Equals("ask-speaker", StringComparison.OrdinalIgnoreCase),
            "reconsider" => decisionAction is "reflect" or "deliberate",
            _ => false
        };

    private static void UpsertInvestigation(List<YalaInvestigationState> items, YalaInvestigationState proposed)
    {
        int index = items.FindIndex(item => item.Key.Equals(proposed.Key, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            items.Add(proposed);
            return;
        }
        YalaInvestigationState current = items[index];
        items[index] = current with
        {
            Status = "active",
            Priority = Math.Max(current.Priority, proposed.Priority),
            NextTest = proposed.NextTest,
            LastUpdatedDecision = proposed.LastUpdatedDecision
        };
    }

    private static void UpsertPlan(List<YalaPlanState> items, YalaPlanState proposed)
    {
        int index = items.FindIndex(item => item.Key.Equals(proposed.Key, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            items.Add(proposed);
            return;
        }
        YalaPlanState current = items[index];
        items[index] = current with
        {
            Status = "active",
            Priority = Math.Max(current.Priority, proposed.Priority),
            LastUpdatedDecision = proposed.LastUpdatedDecision
        };
    }
}
