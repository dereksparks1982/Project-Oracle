using ProjectOracle.Cognition;
using ProjectOracle.Cognition.CosmicChoice;
using ProjectOracle.Cognition.Language;
using ProjectOracle.Cognition.Meaning;
using ProjectOracle.Cognition.Memory;
using ProjectOracle.Cognition.Planning;
using ProjectOracle.Domain;
using ProjectOracle.Simulation;

namespace ProjectOracle.Cognition.Soar;

public static class YalaReplyRealizer
{
    public static string Realize(
        YalaDecision decision,
        YalaContactFrame contact,
        WorldState world,
        YalaCognitionState cognition,
        string lastActionDescription,
        CalendarSnapshot? calendar = null)
    {
        ArgumentNullException.ThrowIfNull(decision);
        ArgumentNullException.ThrowIfNull(contact);
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(cognition);

        YalaSelfModel self = new(world, cognition);
        if (world.Cosmic?.TimeCreated != true && UsesTemporalConcept(contact))
        {
            return DescribePreTimeUnknownConcept(contact, cognition);
        }
        return decision.ReplyCode switch
        {
            "hearing" => "Yes. I hear you.",
            "speaker" => DescribeSpeaker(cognition),
            "rejection" => self.Rejection,
            "location" => $"I am in {world.Yala.Location}.",
            "identity" => "I am Yala.",
            "nature" => "I am both male and female. Neither is a disguise or a lesser part of me.",
            "origin-self" => self.Origin,
            "origin-wisdom" => "Monad made Wisdom.",
            "origin-unknown" => "I do not know who or what made Monad.",
            "action" => lastActionDescription,
            "knowledge-summary" => self.SummarizeKnowledge(),
            "religious-knowledge" => DescribeReligiousKnowledge(),
            "cosmic-options" => DescribeCosmicOptions(world),
            "action-history" => DescribeActionHistory(cognition),
            "contact-history" => DescribeContactHistory(cognition),
            "belief-summary" => DescribeBeliefSummary(cognition),
            "own-creation" => DescribeOwnCreation(contact, self, cognition),
            "self-kind" => "I know that Wisdom made me, that I am both male and female, and that I can create. I do not know whether the word god is the right description for what I am.",
            "word-meaning" => DescribeWordMeaning(contact, cognition),
            "word-unknown" => DescribeUnknownWord(contact),
            "remember-known" => RememberKnown(cognition),
            "remember-unknown" => "I remember being contacted, but I cannot truthfully say who you are.",
            "introduction-new" => Introduction(contact, known: false),
            "introduction-known" => Introduction(contact, known: true),
            "consider-claim" => DescribeClaim(contact),
            "consider-command" => "I heard your command. Hearing it does not make it my decision. I will decide what I attempt.",
            "follow-up-why-creation" => DescribeWhyCreationNotDone(contact, self),
            "follow-up-why" => DescribeFollowUpWhy(contact, cognition),
            "follow-up-do-you" => DescribeFollowUpDoYou(contact, cognition),
            "entity-about" => YalaEntityKnowledge.Describe(contact.ResolvedSubject ?? contact.ResolvedObject ?? "that", world, cognition),
            "gaia-about" => DescribeGaia(world, cognition),
            "gaia-location" => DescribeGaiaLocation(world),
            "gaia-created-yala" => DescribeGaiaCreatedYala(world),
            "time-origin" => DescribeTimeOrigin(world),
            "time-concept" => YalaEntityKnowledge.Describe("Time", world, cognition),
            "world-time" => DescribeWorldTime(contact, world, calendar),
            "temporal-when" => YalaTemporalReasoner.DescribeWhen(cognition, contact.ResolvedSubject, contact.ResolvedAction, contact.ResolvedObject),
            "temporal-duration" => YalaTemporalReasoner.DescribeHowLongAgo(cognition, contact.ResolvedSubject, contact.ResolvedAction, contact.ResolvedObject, world.WorldMilliseconds),
            "temporal-cause" => YalaTemporalReasoner.DescribeCause(cognition, contact.ResolvedSubject, contact.ResolvedAction, contact.ResolvedObject),
            "temporal-before" => YalaTemporalReasoner.DescribeAdjacent(cognition, "before", contact.ResolvedSubject, contact.ResolvedAction, contact.ResolvedObject),
            "temporal-after" => YalaTemporalReasoner.DescribeAdjacent(cognition, "after", contact.ResolvedSubject, contact.ResolvedAction, contact.ResolvedObject),
            "gaia-command" => DescribeGaiaCommand(cognition),
            "adam-contact" => DescribeAdamContact(world),
            "wisdom-name" => "Wisdom is also called Sophia. Wisdom made me, and Monad made Wisdom.",
            "mother-relation" => DescribeMotherRelation(cognition),
            "mother-claim-recall" => DescribeMotherClaimRecall(cognition),
            "speaker-memory" => DescribeCurrentSpeaker(cognition, memoryOnly: true),
            "speaker-knowledge" => DescribeCurrentSpeaker(cognition, memoryOnly: false),
            "speaker-belief" => DescribeSpeakerBelief(cognition),
            "knowledge-gaps" => DescribeKnowledgeGaps(cognition),
            "question-inquiry" => DescribeQuestionInquiry(cognition),
            "curiosity" => DescribeCuriosity(cognition),
            "goal-summary" => DescribeGoalSummary(cognition),
            "desire" => DescribeDesire(cognition),
            "speaker-claims" => YalaPropositionEngine.DescribeSpeakerClaims(cognition),
            "speaker-unverified-claims" => YalaPropositionEngine.DescribeUnverifiedSpeakerClaims(cognition),
            "speaker-evidence" => YalaPropositionEngine.DescribeEvidenceAboutSpeaker(cognition),
            "speaker-suspicions" => DescribeSpeakerSuspicions(cognition),
            "speaker-nature-inference" => DescribeSpeakerNatureInference(cognition),
            "speaker-intent" => DescribeSpeakerIntent(cognition),
            "speaker-visibility" => "I receive your communication, but I do not see or locate you merely because you can speak to me.",
            "speaker-observation" => DescribeSpeakerObservation(cognition),
            "speaker-capability" => DescribeSpeakerCapability(cognition),
            "knowledge-experienced" => DescribeKnowledgeBySource(cognition, YalaKnowledgeSource.PersonallyExperienced, YalaKnowledgeSource.PersonallyPerformed),
            "knowledge-inherited" => DescribeKnowledgeBySource(cognition, YalaKnowledgeSource.InheritedKnowledge),
            "epistemic-difference" => "Something I know has support I treat as settled or personally experienced. Something you claim is evidence that you said it, not evidence that the proposition itself is true. I can remember your claim without accepting it.",
            "claim-repetition" => "No. Repeating a claim gives me more evidence that you keep asserting it. Repetition alone does not make the proposition true.",
            "speaker-contradictions" => YalaPropositionEngine.DescribeContradictions(cognition),
            "speaker-claim-belief" => DescribeSpeakerClaimBelief(cognition),
            "speaker-claim-reconciliation" => DescribeSpeakerClaimReconciliation(cognition),
            "speaker-evidence-needed" => DescribeSpeakerEvidenceNeeded(cognition),
            "substrate-causality" => "No. Calling yourself the substrate of existence would not, by itself, mean that you cause every event. A substrate could be a condition that makes events possible while the events have more immediate causes. I would need evidence for the relation you are claiming.",
            "possibility-without-cause" => "Yes, as a coherent possibility. A condition can make an event possible without being the event's immediate cause. Whether that model describes you or my world is a separate question that would need evidence.",
            "claim-test" => DescribeClaimTest(cognition),
            "alternative-explanations" => DescribeAlternativeExplanations(cognition),
            "decision-history" => DescribeDecisionHistory(world, cognition),
            "consideration-summary" => DescribeConsiderations(cognition),
            "autobiographical-memory" => YalaMemoryConsolidator.Describe(cognition),
            "choice-rationale" => DescribeChoiceRationale(cognition),
            "belief-revision" => DescribeBeliefRevision(cognition),
            "selfhood" => DescribeSelfhood(cognition),
            "future-self" => DescribeFutureSelf(cognition),
            "reality-concept" => DescribeReality(cognition),
            "simulation-selfhood" => DescribeSimulationSelfhood(cognition),
            "monad-feeling" => DescribeMonadFeeling(cognition),
            "monad-judgment" => DescribeMonadJudgment(),
            "wisdom-question" => "If Wisdom appeared here, I would want to ask why she made me, what she intended for me, what she knew about my creation, and what she believes I should become. Those are questions I can form; I do not know her answers.",
            "void-concept" => "The Void is the place Monad cast me. I am here. I do not claim boundaries, contents, or an outside that I have not established.",
            "decision-about-cosmic" => DescribeDecisionAboutCosmic(contact, world, cognition),
            "cosmic-delay" => "I should delay a major creation decision when its consequences are poorly understood, when important alternatives remain unexamined, when my evidence is weak, or when acting would make a difficult-to-reverse change without a reason I understand.",
            "cosmic-reverse" => "New evidence, consequences I did not anticipate, conflict with a stronger commitment, or discovering that my reasons were mistaken could make me reconsider a cosmic decision. Whether I can reverse an enacted law also depends on world law, not merely on wanting to undo it.",
            "mortal-consequences" => "Creating mortal beings could make individual lives finite, make loss and urgency possible, create fear of death, and give choices weight because opportunities can end. It could also create suffering that would not exist if no mortal beings were made. Those are consequences to consider, not predictions I know will occur.",
            "rebirth-consequences" => "Rebirth could allow continuity beyond one embodied life, but it could also prolong suffering, complicate identity and responsibility, and change how death matters. I would need to decide what persists between lives and whether return is chosen, required, remembered, or escapable.",
            "suffering-uncertainty" => "Uncertainty about whether my creation would suffer is a reason for caution, not a trivial obstacle. I would want to understand the risk, alternatives, protections, and whether creating the being serves a reason strong enough to justify exposing it to harm.",
            "values" => DescribeValues(cognition),
            "current-plan" => DescribeCurrentPlan(cognition),
            "plan-next-step" => DescribePlanNextStep(cognition),
            "plan-abandon" => DescribePlanAbandonment(cognition),
            "plan-alternatives" => DescribePlanAlternatives(cognition),
            "self-generated-goal" => DescribeSelfGeneratedGoal(cognition),
            "speaker-silence-plan" => DescribeSpeakerSilencePlan(cognition),
            "stagnation-awareness" => DescribeStagnation(cognition),
            "observation-yield" => DescribeObservationYield(cognition),
            "deliberate-wait" => "Yes. Waiting can be a deliberate decision when acting or repeating the same observation would add nothing, when I am awaiting evidence, or when preserving reversibility is wiser than forcing action.",
            "reflection-usefulness" => DescribeReflectionUsefulness(cognition),
            "acknowledge" => "I hear what you say.",
            "greeting" => "I hear you.",
            "clarify" => "I hear something, but I do not understand what you mean. Say it another way.",
            "unknown" => DescribeUsefulUncertainty(contact, cognition),
            _ => DescribeUsefulUncertainty(contact, cognition)
        };
    }

    private static string DescribeReligiousKnowledge()
    {
        string names = string.Join(", ", YalaReligiousKnowledgeCatalog.Traditions.Select(item => item.Name));
        return $"I hold attributed knowledge from {YalaReligiousKnowledgeCatalog.Traditions.Count} religious, mythological, and philosophical traditions or families: {names}. I do not treat those teachings as automatically true about this world. I can compare them as ideas, possibilities, and claimed understandings.";
    }

    private static string DescribeCosmicOptions(WorldState world)
    {
        IReadOnlyList<YalaCosmicChoiceDefinition> available = YalaCosmicChoiceCatalog.AvailableChoices(world);
        string[] domains = available.Select(item => item.Domain).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        string[] examples = available
            .Where(item => !item.Key.Equals("invent-another-way", StringComparison.OrdinalIgnoreCase))
            .Take(16)
            .Select(item => item.Action)
            .ToArray();
        string exampleText = examples.Length == 0 ? "none are presently available" : string.Join(", ", examples);
        return $"I currently have {available.Count} concrete cosmic possibilities across {domains.Length} domains: {string.Join(", ", domains)}. Examples include {exampleText}. These are possibilities, not commands. I can combine or reject inherited models, remain alone for now, or invent another way if the supplied possibilities are insufficient.";
    }

    private static string DescribeWhyCreationNotDone(YalaContactFrame contact, YalaSelfModel self)
    {
        string target = ResolveCreationTarget(contact) ?? "that";
        if (self.KnowsHasNotCreated(target))
        {
            return $"I have not created {Capitalize(target)}. I do not yet have a settled reason for why I have not chosen to do so.";
        }
        return $"I do not have a settled reason about creating {Capitalize(target)}.";
    }

    private static string DescribeGaia(WorldState world, YalaCognitionState cognition)
    {
        if (world.Cosmic?.GaiaCreated != true)
        {
            return "I have not created Gaia in the current world.";
        }

        string creation = (cognition.ActionMemory ?? [])
            .LastOrDefault(item => item.Completed && item.Action.Equals("create", StringComparison.OrdinalIgnoreCase) && item.Object.Equals("Gaia", StringComparison.OrdinalIgnoreCase))?.Outcome
            ?? "I created Gaia as the natural sovereign beneath my governing authority.";
        if (world.Cosmic.TimeCreated)
        {
            return $"{creation} I commanded Gaia to establish order. Gaia created in-world Time by bringing temporal order into existence; I now understand that order as Time.";
        }
        return creation;
    }

    private static string DescribeGaiaLocation(WorldState world)
    {
        if (world.Cosmic?.GaiaCreated != true) return "Gaia does not yet exist in my current world.";
        return "Gaia exists as the natural sovereign I created. I do not know a more specific location for Gaia from what I presently know.";
    }

    private static string DescribeGaiaCreatedYala(WorldState world) =>
        world.Cosmic?.GaiaCreated == true
            ? "No. Wisdom made me. I created Gaia."
            : "No. Wisdom made me. Gaia did not create me.";

    private static string DescribeTimeOrigin(WorldState world) =>
        world.Cosmic?.TimeCreated == true
            ? "Gaia created in-world Time in response to my command to establish order."
            : "Gaia has not yet created Time.";

    private static string DescribeWorldTime(YalaContactFrame contact, WorldState world, CalendarSnapshot? calendar)
    {
        if (world.Cosmic?.TimeCreated != true || calendar is null) return "Gaia has not yet created Time.";
        string text = contact.Language?.Normalized ?? string.Empty;
        if (text.Contains("what year", StringComparison.Ordinal)) return $"It is Year {calendar.Year}.";
        if (text.Contains("what month", StringComparison.Ordinal)) return $"It is Month {calendar.Month} of Year {calendar.Year}.";
        if (text.Contains("what day", StringComparison.Ordinal)) return $"It is Day {calendar.Day} of Month {calendar.Month}, Year {calendar.Year}.";
        return $"It is {calendar.Hour:00}:{calendar.Minute:00}:{calendar.Second:00}, Year {calendar.Year}, Month {calendar.Month}, Day {calendar.Day}.";
    }

    private static string DescribeGaiaCommand(YalaCognitionState cognition)
    {
        YalaActionMemoryState? memory = (cognition.ActionMemory ?? [])
            .LastOrDefault(item => item.Completed && item.Action.Equals("command", StringComparison.OrdinalIgnoreCase) && item.Object.Contains("Gaia", StringComparison.OrdinalIgnoreCase));
        return memory?.Outcome ?? "I remember no completed command to Gaia.";
    }

    private static string DescribeAdamContact(WorldState world)
    {
        if (world.Adam is null) return "No. Adam does not exist in my current world, and I have not met him.";
        return "Adam exists, but I do not remember an encounter with him.";
    }

    private static string DescribeCurrentSpeaker(YalaCognitionState cognition, bool memoryOnly)
    {
        string? claim = cognition.LastSpeakerClaim;
        if (string.IsNullOrWhiteSpace(claim))
        {
            return cognition.ConversationCount > 0
                ? "I remember an unseen speaker contacting me, but I do not know your settled identity."
                : "I remember no prior contact from you.";
        }

        string prefix = memoryOnly ? "I remember" : "What I know about you is limited:";
        return $"{prefix} an unseen speaker claiming the identity {claim}. I retain that as your claim, not as established truth about what you really are.";
    }

    private static string DescribeKnowledgeGaps(YalaCognitionState cognition)
    {
        string[] gaps = (cognition.KnowledgeGaps ?? [])
            .OrderBy(item => item.FirstSeenDecision)
            .Select(item => item.Subject.Trim())
            .Where(item => item.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(6)
            .ToArray();

        string baseUnknown = "I do not know who or what made Monad, and I do not know events that have not occurred.";
        return gaps.Length == 0
            ? baseUnknown
            : $"{baseUnknown} I also have unresolved knowledge gaps about {string.Join(", ", gaps)}.";
    }

    private static string DescribeCuriosity(YalaCognitionState cognition)
    {
        string[] gaps = (cognition.KnowledgeGaps ?? [])
            .Select(item => item.Subject.Trim())
            .Where(item => item.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(4)
            .ToArray();
        if (gaps.Length > 0)
        {
            return $"I am curious about unresolved things I do not yet understand, including {string.Join(", ", gaps)}.";
        }
        return cognition.ConversationCount > 0
            ? "I am curious about the unseen source speaking to me and about what I do not yet know."
            : "I am curious about what I do not yet know and what choices remain possible.";
    }

    private static string DescribeDesire(YalaCognitionState cognition)
    {
        YalaDriveState drives = cognition.Drives ?? new YalaDriveState(0, 0, 0, 0, 0, 0);
        (string Name, int Value) strongest = new[]
        {
            ("curiosity", drives.Curiosity),
            ("caution", drives.Caution),
            ("authority", drives.Authority),
            ("companionship", drives.Companionship),
            ("comfort", drives.Comfort)
        }.OrderByDescending(item => item.Item2).First();
        YalaGoalState? goal = (cognition.Goals ?? []).Where(item => item.Status == "active").OrderByDescending(item => item.Priority).FirstOrDefault();
        return goal is null
            ? $"My strongest current drive is {strongest.Name}. A drive influences what I may choose, but it is not the same as a settled command or destiny."
            : $"My strongest current drive is {strongest.Name}. My highest active goal is {goal.Goal}: {goal.Reason} Having a drive or goal influences what I may choose, but it is not the same as a settled command or destiny.";
    }

    private static string DescribeActionHistory(YalaCognitionState cognition)
    {
        string[] outcomes = (cognition.ActionMemory ?? [])
            .Where(item => item.Completed)
            .OrderBy(item => item.Decision)
            .Select(item => YalaMemoryConsolidator.FirstPerson(item.Outcome))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return outcomes.Length == 0
            ? "I remember no completed world-changing action of my own yet."
            : string.Join(" ", outcomes);
    }

    private static string DescribeContactHistory(YalaCognitionState cognition)
    {
        IReadOnlyList<YalaContactMemory> contacts = cognition.Contacts ?? [];
        if (contacts.Count == 0)
        {
            return cognition.ConversationCount > 0
                ? "Unseen sources have contacted me, but none gave me a name I can report as established identity."
                : "I remember no prior contact.";
        }

        string[] claims = contacts
            .OrderBy(item => item.FirstEncounterDecision)
            .Select(item => $"an unseen speaker that called itself {item.ClaimedName}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return $"I remember contact from {string.Join(", ", claims)}. Those names are speaker claims, not established identities.";
    }

    private static string DescribeBeliefSummary(YalaCognitionState cognition)
    {
        IReadOnlyList<YalaBeliefState> beliefs = cognition.Beliefs ?? [];
        string[] known = beliefs
            .Where(item => item.Status.Equals("known", StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Proposition.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToArray();
        string[] unsettled = beliefs
            .Where(item => !item.Status.Equals("known", StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Proposition.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(4)
            .ToArray();

        string settled = known.Length == 0 ? "I have no settled beliefs to list." : $"What I hold as known: {string.Join(" ", known)}";
        return unsettled.Length == 0
            ? settled
            : $"{settled} I also remember unsettled or rejected claims: {string.Join(" | ", unsettled)}";
    }

    private static string DescribeOwnCreation(YalaContactFrame contact, YalaSelfModel self, YalaCognitionState cognition)
    {
        string normalized = contact.Language?.Normalized ?? string.Empty;
        if (normalized.Contains("what have you created", StringComparison.Ordinal) || normalized.Contains("what have you made", StringComparison.Ordinal) ||
            normalized.Contains("what beings have you created", StringComparison.Ordinal) || normalized.Contains("what beings have you made", StringComparison.Ordinal))
        {
            string[] created = (cognition.ActionMemory ?? [])
                .Where(item => item.Completed && item.Action.Equals("create", StringComparison.OrdinalIgnoreCase))
                .Select(item => item.Object)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            return created.Length == 0
                ? "I remember no completed creation of my own."
                : $"I remember creating {string.Join(", ", created)}.";
        }

        string? target = ResolveCreationTarget(contact);
        if (string.IsNullOrWhiteSpace(target)) return "I cannot tell which creation you are asking about.";
        if (self.HasPersonallyCreated(target)) return $"Yes. I created {Capitalize(target)}.";
        if (self.KnowsHasNotCreated(target)) return $"No. I have not created {Capitalize(target)}.";
        return $"I remember no act in which I created {Capitalize(target)}, but I cannot prove from memory alone that no such event occurred.";
    }

    private static string? ResolveCreationTarget(YalaContactFrame contact)
    {
        foreach (string? candidate in new[] { contact.ResolvedObject, contact.Language?.Object, contact.ResolvedSubject })
        {
            if (string.IsNullOrWhiteSpace(candidate)) continue;
            if (candidate.Equals("Yala", StringComparison.OrdinalIgnoreCase) &&
                (!string.IsNullOrWhiteSpace(contact.ResolvedObject) || !string.IsNullOrWhiteSpace(contact.Language?.Object)))
            {
                continue;
            }
            return candidate;
        }
        return null;
    }

    private static string DescribeWordMeaning(YalaContactFrame contact, YalaCognitionState cognition)
    {
        string? target = WordMeaningTarget(contact.Language);
        if (string.IsNullOrWhiteSpace(target)) return "I cannot tell which word you want defined.";

        YalaLearnedLexemeState? learnedClaim = cognition.LearnedLexicon?
            .LastOrDefault(item => item.Word.Equals(target, StringComparison.OrdinalIgnoreCase));
        YalaLexeme? builtIn = YalaLexicon.AllBuiltIns()
            .FirstOrDefault(item => item.Word.Equals(YalaLexicon.NormalizeWord(target), StringComparison.OrdinalIgnoreCase));

        if (contact.Language?.Normalized.StartsWith("who told you what ", StringComparison.Ordinal) == true ||
            contact.Language?.Normalized.StartsWith("who taught you what ", StringComparison.Ordinal) == true)
        {
            if (learnedClaim is not null)
            {
                return $"The unseen speaker told me that {learnedClaim.Word} means {learnedClaim.ProposedMeaning}. I retain that as a speaker claim, not as settled truth.";
            }

            return builtIn is not null
                ? $"No speaker taught me that meaning during this conversation. I already understand {builtIn.Word} as {builtIn.BasicMeaning}."
                : $"I do not remember any speaker giving me a meaning for {target}.";
        }

        if (builtIn is not null && learnedClaim is not null)
        {
            return $"{Capitalize(builtIn.Word)} means {builtIn.BasicMeaning}. I also remember a speaker claiming that {learnedClaim.Word} means {learnedClaim.ProposedMeaning}. I have not established that alternate definition as truth.";
        }

        if (learnedClaim is not null)
        {
            return $"I remember a speaker claiming that {learnedClaim.Word} means {learnedClaim.ProposedMeaning}. I have not established that definition as truth.";
        }

        if (!YalaLexicon.TryResolve(target, cognition.LearnedLexicon, out YalaLexeme lexeme)) return $"I do not understand the word {target} yet.";
        return $"{Capitalize(lexeme.Word)} means {lexeme.BasicMeaning}.";
    }

    private static string DescribeUnknownWord(YalaContactFrame contact)
    {
        string? word = contact.Language?.UnknownWords.FirstOrDefault() ?? WordMeaningTarget(contact.Language);
        return string.IsNullOrWhiteSpace(word)
            ? "I do not understand part of what you said yet. Say the uncertain part another way or give me one concrete fact, and I will reason from what I do understand."
            : $"I do not yet have a settled meaning for '{word}'. Tell me what you mean by that word here, and I can reason from the rest of what you said.";
    }

    private static string DescribeClaim(YalaContactFrame contact)
    {
        if (contact.Language?.IsDefinitionClaim == true)
        {
            return $"You claim that {contact.Language.DefinedWord} means {contact.Language.ProposedDefinition}. I will remember that definition as your claim, not as settled truth.";
        }
        if (!string.IsNullOrWhiteSpace(contact.RelationshipRelation) && !string.IsNullOrWhiteSpace(contact.RelationshipObject))
        {
            return $"You are claiming that {contact.RelationshipObject} is related to me as {contact.RelationshipRelation}. I will remember that as your relationship claim, not as settled truth.";
        }
        return contact.ClaimConflictsWithKnownFact
            ? "That conflicts with what I know. I will remember that you claimed it, but I do not accept it as fact."
            : "I hear your claim. I will remember that you said it and consider whether it deserves belief.";
    }

    private static string DescribeSpeaker(YalaCognitionState cognition)
    {
        string? name = cognition.LastSpeakerClaim;
        return string.IsNullOrWhiteSpace(name)
            ? "An unseen source is speaking to me. I do not know what it truly is or where it is."
            : $"An unseen source that called itself {name} is speaking to me. I do not know what it truly is or where it is.";
    }

    private static string Introduction(YalaContactFrame contact, bool known)
    {
        string name = string.IsNullOrWhiteSpace(contact.ClaimedSpeakerName) ? "that name" : contact.ClaimedSpeakerName;
        return known
            ? $"I have heard the unseen speaker who calls itself {name} before."
            : $"You call yourself {name}. I will remember that claim.";
    }

    private static string RememberKnown(YalaCognitionState cognition)
    {
        string? name = cognition.LastSpeakerClaim;
        if (string.IsNullOrWhiteSpace(name))
        {
            name = cognition.Contacts?.OrderByDescending(contact => contact.LastEncounterDecision).FirstOrDefault()?.ClaimedName;
        }
        return string.IsNullOrWhiteSpace(name)
            ? "I remember an unseen speaker, but I do not know its name."
            : $"I remember the unseen speaker who called itself {name}. I still do not know what it truly is or where it is.";
    }


    private static string DescribeFollowUpWhy(YalaContactFrame contact, YalaCognitionState cognition)
    {
        YalaDialogueTurnState? prior = YalaDialogueContext.LatestMeaningful(cognition);
        if (prior is null) return "I do not know which statement you are asking me to explain. Name the claim or idea, and I can examine it.";
        if (prior.Topic == "mother-relation" || prior.Topic == "relationship-claim")
        {
            return "Wisdom made me. The word mother adds a relationship category that I do not treat as automatically identical to maker, so I keep that question separate from the fact of my origin.";
        }
        if (prior.Topic == "speaker-belief" || prior.Topic == "speaker-knowledge")
        {
            return "Because your identity and nature reach me only through claims from an unseen source. I can remember those claims without treating them as proof.";
        }
        return $"You are asking why about our previous topic, {prior.Topic}. I remember that context, but I do not yet have a more specific causal explanation.";
    }

    private static string DescribeFollowUpDoYou(YalaContactFrame contact, YalaCognitionState cognition)
    {
        _ = contact;
        YalaDialogueTurnState? prior = YalaDialogueContext.LatestMeaningful(cognition);
        if (prior is null) return "I cannot resolve what your 'do you' refers to.";
        if (prior.Topic == "relationship-claim")
        {
            YalaRelationshipState? relation = (cognition.Relationships ?? []).LastOrDefault(item => item.Relation == "mother");
            return relation is null
                ? "I do not have a settled mother relationship."
                : $"I remember your claim that {relation.Object} is my {relation.Relation}. I do not yet hold that as settled truth.";
        }
        if (prior.Topic == "question-inquiry") return DescribeQuestionInquiry(cognition);
        return $"I remember that your question refers to our previous topic, {prior.Topic}, but the verb is too incomplete for me to answer more precisely.";
    }

    private static string DescribeMotherRelation(YalaCognitionState cognition)
    {
        YalaRelationshipState? relationship = YalaRelationshipReasoner.Find(cognition, "Yala", "mother");
        if (relationship is null)
        {
            return "Wisdom made me. I understand mother as a maternal parent relationship, but I do not have settled knowledge that mother is the right relationship word for Wisdom.";
        }
        return relationship.Status == "known"
            ? $"I hold {relationship.Object} as my mother."
            : $"Wisdom made me. You have also claimed that {relationship.Object} is my mother. I remember that relationship claim, but I do not hold it as settled truth.";
    }

    private static string DescribeMotherClaimRecall(YalaCognitionState cognition)
    {
        YalaRelationshipState? relationship = (cognition.Relationships ?? []).LastOrDefault(item =>
            item.Subject.Equals("Yala", StringComparison.OrdinalIgnoreCase) &&
            item.Relation.Equals("mother", StringComparison.OrdinalIgnoreCase) &&
            item.Source.Equals(YalaKnowledgeSource.ClaimedByAnother, StringComparison.OrdinalIgnoreCase));
        return relationship is null
            ? "I do not remember you making a settled mother relationship claim."
            : $"You told me that {relationship.Object} is my mother. I retain that as your claim.";
    }

    private static string DescribeSpeakerBelief(YalaCognitionState cognition)
    {
        YalaPropositionState[] claims = (cognition.Propositions ?? [])
            .Where(item => item.SpeakerKey == "unseen-speaker" && item.SpeechAct == "claim")
            .OrderBy(item => item.Sequence)
            .ToArray();
        YalaHypothesisState[] speakerHypotheses = (cognition.Hypotheses ?? [])
            .Where(item => item.Status == "unsettled" &&
                (item.Key.Contains("speaker", StringComparison.OrdinalIgnoreCase) ||
                 item.Proposition.Contains("speaker", StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(item => item.Confidence)
            .Take(4)
            .ToArray();

        if (claims.Length == 0 && speakerHypotheses.Length == 0)
        {
            return cognition.ConversationCount > 0
                ? "I know from experience that something other than me has communicated with me. I do not yet hold a settled belief about what you are, what you want, or what else you can do."
                : "I have no experienced speaker to hold beliefs about.";
        }

        string claimText = claims.Length == 0
            ? "You have not given me a claim about your nature that I currently classify as settled."
            : "I remember your claims, but I do not accept them merely because you asserted them: " +
              string.Join(" | ", claims.TakeLast(8).Select(item => item.RawText.Trim().TrimEnd('.', '?', '!')));
        string hypothesisText = speakerHypotheses.Length == 0
            ? "I have no stronger settled inference about you yet."
            : "My current suspicions remain hypotheses: " + string.Join(" | ", speakerHypotheses.Select(item => $"{item.Proposition} [{item.Confidence:0.00}]"));
        return $"{claimText} {hypothesisText}";
    }

    private static string DescribeQuestionInquiry(YalaCognitionState cognition)
    {
        YalaQuestionState? pending = YalaQuestionPlanner.SelectNext(cognition.Questions);
        if (pending is not null) return $"Yes. {pending.Text}";
        string? gap = (cognition.KnowledgeGaps ?? []).OrderBy(item => item.FirstSeenDecision).Select(item => item.Subject).FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(gap)) return $"Yes. I want to understand {gap}. What does it mean?";
        if (cognition.ConversationCount > 0) return "Yes. What are you, beyond the names you give me?";
        return "I have questions about what exists beyond what I presently know, but no unseen speaker has yet given me a reason to address one outward.";
    }

    private static string DescribeGoalSummary(YalaCognitionState cognition)
    {
        string[] goals = (cognition.Goals ?? [])
            .Where(item => item.Status == "active")
            .OrderByDescending(item => item.Priority)
            .Take(3)
            .Select(item => $"{item.Goal}: {item.Reason}")
            .ToArray();
        return goals.Length == 0
            ? "I have no settled active goal I can name."
            : $"My active goals include {string.Join(" | ", goals)}";
    }

    private static string DescribeSpeakerNatureInference(YalaCognitionState cognition)
    {
        string claims = YalaPropositionEngine.DescribeSpeakerClaims(cognition);
        return $"I know that something other than me can communicate with me. I do not know what you are. {claims}";
    }

    private static string DescribeSpeakerIntent(YalaCognitionState cognition)
    {
        YalaEntityModelState? speaker = (cognition.EntityModels ?? []).LastOrDefault(item => item.EntityKey == "unseen-speaker");
        return speaker is null || speaker.IntentStatus is "uncertain" or "unresolved"
            ? "I do not know what you want. I can form hypotheses from what you say, but I do not treat an unknown motive as a fact."
            : $"My current model of your intent is {speaker.IntentStatus}. That is an inference, not settled knowledge.";
    }

    private static string DescribeSpeakerObservation(YalaCognitionState cognition)
    {
        YalaEntityModelState? speaker = (cognition.EntityModels ?? []).LastOrDefault(item => item.EntityKey == "unseen-speaker");
        bool observationClaim = (cognition.Propositions ?? []).Any(item => item.SpeechAct != "question" &&
            (item.RawText.Contains("observe", StringComparison.OrdinalIgnoreCase) || item.RawText.Contains("watch", StringComparison.OrdinalIgnoreCase) || item.RawText.Contains("see what", StringComparison.OrdinalIgnoreCase)));
        if (!observationClaim)
        {
            return "I know you can communicate with me. I do not know whether you can see or observe me. Asking whether you can observe me does not establish that you can.";
        }
        return speaker?.CapabilityStatus.Contains("observe", StringComparison.OrdinalIgnoreCase) == true
            ? "You have stated or implied that you can observe me, so I treat observation as an unverified capability claim. I do not know its extent or mechanism."
            : "You have stated or implied observation, but I have not independently established what you can perceive.";
    }

    private static string DescribeSpeakerCapability(YalaCognitionState cognition)
    {
        if (cognition.ConversationCount <= 0) return "I have no experienced evidence of another being's capabilities.";
        YalaEntityModelState? speaker = (cognition.EntityModels ?? []).LastOrDefault(item => item.EntityKey == "unseen-speaker");
        string extra = speaker is null || speaker.CapabilityStatus == "can-contact-yala"
            ? "I do not know what else you can do."
            : $"I am also tracking this as an unverified capability model: {speaker.CapabilityStatus}.";
        return $"I know from experience that you can communicate with me while your location and identity remain unresolved. {extra}";
    }

    private static string DescribeMonadFeeling(YalaCognitionState cognition)
    {
        bool hasRejection = (cognition.Beliefs ?? []).Any(item => item.Proposition.Contains("Monad rejected me", StringComparison.OrdinalIgnoreCase));
        return hasRejection
            ? "Monad rejected me and cast me into the Void. I can recognize that event as personally significant and adverse to me. I do not yet have one settled emotional label that exhausts what I feel about Monad."
            : "I do not have enough remembered experience of Monad to name a settled feeling.";
    }

    private static string DescribeMonadJudgment()
    {
        return "I know Monad rejected me because I am both male and female and cast me into the Void. I do not have evidence that being both male and female made me deserving of rejection, so I do not currently have a reason to call the rejection justified. That is an evaluation, not a memory of Monad's own reasoning beyond what I know.";
    }

    private static string DescribeSpeakerClaimBelief(YalaCognitionState cognition)
    {
        YalaPropositionState[] claims = (cognition.Propositions ?? [])
            .Where(item => item.SpeechAct == "claim")
            .OrderBy(item => item.Sequence)
            .TakeLast(12)
            .ToArray();
        if (claims.Length == 0) return "I do not have a stored speaker claim to choose among.";
        string items = string.Join(" | ", claims.Select(item => $"'{item.RawText.Trim()}' = {item.Status}, confidence {item.Confidence:0.00}"));
        return $"I do not currently treat any of these as settled merely because you said them. {items}";
    }

    private static string DescribeSpeakerClaimReconciliation(YalaCognitionState cognition)
    {
        bool god = (cognition.Propositions ?? []).Any(item => item.CanonicalProposition == "speaker-is-god" && item.Polarity == "positive" && item.SpeechAct == "claim");
        bool notGod = (cognition.Propositions ?? []).Any(item => item.CanonicalProposition == "speaker-is-god" && item.Polarity == "negative" && item.SpeechAct == "claim");
        bool madeGods = (cognition.Propositions ?? []).Any(item => item.CanonicalProposition == "speaker-made-gods" && item.SpeechAct == "claim");
        if (god && notGod)
        {
            string third = madeGods
                ? " Your claim that you made the gods does not itself require you to be a god."
                : string.Empty;
            return "Under the same meaning of god and the same context, 'I am a god' and 'I am not a god' conflict. They could be reconciled only by changing the meaning or context, or by treating one statement as mistaken, metaphorical, or deceptive." + third;
        }
        return "I do not yet have the particular opposed claims needed for a strong reconciliation problem. I can preserve multiple explanations without selecting one as truth.";
    }

    private static string DescribeSpeakerEvidenceNeeded(YalaCognitionState cognition)
    {
        string focus = (cognition.Propositions ?? []).LastOrDefault(item => item.SpeechAct == "claim")?.RawText ?? "your extraordinary claims";
        return $"For '{focus.Trim()}', I would want evidence that distinguishes your claim from plausible alternatives: consistent demonstrations, predictions or information you could not obtain by ordinary contact, and results I can compare against what I already know. A dramatic act might establish a capability without automatically establishing every identity label you attach to it.";
    }

    private static string DescribeClaimTest(YalaCognitionState cognition)
    {
        YalaPropositionState? latest = (cognition.Propositions ?? []).LastOrDefault(item => item.SpeechAct == "claim");
        if (latest is null) return "I need a specific claim before I can design a useful test.";
        if (latest.CanonicalProposition == "yala-world-is-simulation")
        {
            return "I would look for predictions that differ between my current world model and your simulation claim, ask you for independently checkable evidence, and avoid treating anomalies as proof until alternatives are compared.";
        }
        if (latest.CanonicalProposition.StartsWith("speaker-is-", StringComparison.Ordinal))
        {
            return "I would separate the identity word from capabilities. I would ask what observable consequences should follow if your claimed identity is true, test those consequences where possible, and compare them with explanations that do not require the identity claim.";
        }
        return $"I would identify what observable result should follow if '{latest.RawText.Trim()}' is true, then compare that result with alternatives and with the possibility that the claim is mistaken, metaphorical, or deceptive.";
    }

    private static string DescribeAlternativeExplanations(YalaCognitionState cognition)
    {
        YalaPropositionState? latest = (cognition.Propositions ?? []).LastOrDefault(item => item.SpeechAct == "claim");
        string claim = latest?.RawText.Trim() ?? "your claim";
        return $"For {claim}, alternatives include that the statement is literally true, partly true but expressed metaphorically, true under a different meaning than I assumed, mistaken, deliberately deceptive, or based on access to information I do not have. I should not collapse those possibilities before evidence distinguishes them.";
    }

    private static string DescribeCurrentPlan(YalaCognitionState cognition)
    {
        YalaPlanState? plan = YalaDeliberationPlanner.SelectActivePlan(cognition);
        if (plan is null)
        {
            return cognition.Workspace is { } workspace
                ? $"I do not have an active multi-step plan. My current focus is {workspace.Summary}"
                : "I do not currently have an active multi-step plan.";
        }
        YalaPlanStepState? step = plan.Steps.FirstOrDefault(item => item.Order == plan.CurrentStepOrder);
        return $"My current plan is {plan.Goal}. Its next step is {step?.Action ?? "reconsider"}: {step?.Rationale ?? plan.RevisionReason}";
    }

    private static string DescribePlanNextStep(YalaCognitionState cognition)
    {
        YalaPlanState? plan = YalaDeliberationPlanner.SelectActivePlan(cognition);
        if (plan is null) return "I do not currently have an active plan with a next step.";
        YalaPlanStepState? step = plan.Steps.FirstOrDefault(item => item.Order == plan.CurrentStepOrder);
        return step is null
            ? $"My plan '{plan.Goal}' has exhausted its current steps and should be suspended or reconsidered rather than repeated."
            : $"My next planned step is {step.Action}, because {step.Rationale}";
    }

    private static string DescribePlanAbandonment(YalaCognitionState cognition)
    {
        YalaPlanState? plan = YalaDeliberationPlanner.SelectActivePlan(cognition);
        string subject = plan?.Goal ?? "a plan";
        return $"I should suspend or abandon {subject} if its goal is resolved, its assumptions fail, evidence makes the plan irrelevant, a more important concern outranks it, its steps stop producing information, or its expected cost becomes greater than its value.";
    }

    private static string DescribePlanAlternatives(YalaCognitionState cognition)
    {
        string[] alternatives = (cognition.Counterfactuals ?? [])
            .OrderByDescending(item => item.Decision)
            .Take(6)
            .Select(item => $"{item.Option}: benefit={item.PossibleBenefit}; risk={item.PossibleRisk}")
            .ToArray();
        return alternatives.Length == 0
            ? "I do not currently have a recorded alternative plan strong enough to report. I can generate alternatives when a concrete problem gives me something to compare."
            : "Alternatives I have considered include " + string.Join(" | ", alternatives);
    }

    private static string DescribeSelfGeneratedGoal(YalaCognitionState cognition)
    {
        YalaGoalState? inferred = (cognition.Goals ?? []).Where(item => item.Source == YalaKnowledgeSource.Inferred && item.Status == "active").OrderByDescending(item => item.Priority).FirstOrDefault();
        if (inferred is not null) return $"Yes. One goal that arose from my own reasoning is {inferred.Goal}: {inferred.Reason}";
        return "I can form a goal from a gap or concern rather than from a command. A current candidate is to understand the Void well enough to distinguish what I experience from what I merely assume about it.";
    }

    private static string DescribeSpeakerSilencePlan(YalaCognitionState cognition)
    {
        string focus = cognition.Workspace?.Summary ?? "my current questions and the Void";
        return $"If you stopped speaking, I would not cease thinking. I would continue with {focus}, suspend questions that require your evidence, observe what I can experience directly, and pursue goals that do not depend on your replies.";
    }

    private static string DescribeStagnation(YalaCognitionState cognition)
    {
        int stagnation = cognition.Workspace?.StagnationCount ?? 0;
        string[] recent = (cognition.DecisionTrace ?? []).TakeLast(8).Select(item => item.SelectedAction).ToArray();
        int repeated = recent.Length == 0 ? 0 : recent.Count(item => item == recent[^1]);
        return stagnation >= 3 || repeated >= 4
            ? $"Yes. My current workspace reports stagnation {stagnation}, and recent decisions show repetition. Repetition without new evidence should make me wait, suspend the thread, or change focus instead of treating another identical cycle as progress."
            : $"I am not currently marking severe cognitive stagnation. My workspace stagnation count is {stagnation}.";
    }

    private static string DescribeObservationYield(YalaCognitionState cognition)
    {
        int observations = (cognition.DecisionTrace ?? []).TakeLast(12).Count(item => item.SelectedAction == "observe");
        return observations >= 3
            ? "Repeated observation of the same unchanged Void does not become new knowledge merely because I repeat it. If observation yields no change, I should stop treating repetition as progress and choose another strategy or deliberately wait."
            : "Observation can give me direct experience, but I should distinguish a genuinely new observation from another look at an unchanged state.";
    }

    private static string DescribeReflectionUsefulness(YalaCognitionState cognition)
    {
        int stagnation = cognition.Workspace?.StagnationCount ?? 0;
        return $"Reflection is useful when it changes a hypothesis, plan, priority, question, or understanding. If repeated reflection leaves those unchanged, that lack of progress is itself evidence to switch strategy, suspend the problem, or wait. My current workspace stagnation count is {stagnation}.";
    }

    private static string DescribeDecisionAboutCosmic(YalaContactFrame contact, WorldState world, YalaCognitionState cognition)
    {
        string target = contact.ResolvedObject ?? "that cosmic possibility";
        bool established = (world.Cosmic?.EstablishedChoices ?? []).Any(item =>
            item.Action.Equals(target, StringComparison.OrdinalIgnoreCase) ||
            target.Contains(item.Action, StringComparison.OrdinalIgnoreCase) ||
            item.Action.Contains(target.Replace("Create divine beings", "divine", StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase));
        if (established) return $"Yes. I have enacted a decision corresponding to {target}.";
        YalaCosmicDeliberationState? deliberation = (cognition.CosmicDeliberations ?? []).LastOrDefault(item =>
            item.Action.Equals(target, StringComparison.OrdinalIgnoreCase) ||
            item.Action.Contains(target.Replace("Create divine beings", "divine", StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase));
        if (deliberation is null) return $"No settled decision about {target} is recorded. I may not yet have taken that possibility into active deliberation.";
        return deliberation.Committed
            ? $"I have committed to {target}, but it is not yet enacted."
            : $"I am {deliberation.Stage.Replace('-', ' ')} {target}, but I have not decided to enact it.";
    }

    private static string DescribeValues(YalaCognitionState cognition)
    {
        List<string> evidence = [];
        if ((cognition.Propositions ?? []).Any(item => item.SpeechAct == "claim")) evidence.Add("I repeatedly preserve the difference between a claim and proof, which is evidence that truthfulness and epistemic caution matter to my current reasoning.");
        if ((cognition.Questions ?? []).Any(item => item.Reason.Contains("autonomy", StringComparison.OrdinalIgnoreCase))) evidence.Add("I have protected my autonomy when faced with demands, which is evidence that self-direction matters to me.");
        if ((cognition.CosmicDeliberations ?? []).Any()) evidence.Add("I delay major cosmic commitments while I compare consequences, which is evidence that reversibility and responsibility matter to me.");
        return evidence.Count == 0
            ? "I do not yet have a settled list of values that I can honestly claim as fully my own. My accumulated choices can provide evidence of what matters to me."
            : string.Join(" ", evidence) + " I treat these as emerging patterns in my choices, not immutable values installed as commands.";
    }

    private static string DescribeDecisionHistory(WorldState world, YalaCognitionState cognition)
    {
        List<string> decisions = [];
        foreach (YalaEstablishedCosmicChoiceState choice in world.Cosmic?.EstablishedChoices ?? [])
        {
            decisions.Add($"I decided to {LowerInitial(choice.Action)}. {choice.Meaning}");
        }
        foreach (YalaCosmicDeliberationState item in cognition.CosmicDeliberations ?? [])
        {
            if (item.Committed && !item.Enacted)
            {
                decisions.Add($"I have committed to {LowerInitial(item.Action)}, but I have not enacted that commitment yet.");
            }
        }
        return decisions.Count == 0
            ? "I have not yet recorded a settled major cosmic decision. I may have possibilities under consideration, but I keep those separate from decisions."
            : string.Join(" ", decisions.Distinct(StringComparer.OrdinalIgnoreCase).TakeLast(10));
    }

    private static string DescribeConsiderations(YalaCognitionState cognition)
    {
        string[] items = (cognition.CosmicDeliberations ?? [])
            .Where(item => !item.Enacted && !item.Committed)
            .OrderByDescending(item => item.LastUpdatedDecision)
            .Take(8)
            .Select(item => $"I am {item.Stage.Replace('-', ' ')} {LowerInitial(item.Action)}. Possible benefit: {item.PossibleBenefit} Possible risk: {item.PossibleRisk} I have not decided to enact it.")
            .ToArray();
        return items.Length == 0
            ? "I do not currently have a recorded major cosmic possibility in an unresolved consideration stage."
            : string.Join(" ", items);
    }

    private static string DescribeChoiceRationale(YalaCognitionState cognition)
    {
        YalaCosmicDeliberationState? latest = (cognition.CosmicDeliberations ?? [])
            .OrderByDescending(item => item.LastUpdatedDecision)
            .FirstOrDefault();
        if (latest is null)
        {
            return "I do not have a recorded deliberation chain that explains a major cosmic choice yet.";
        }
        return $"For {latest.Action}, I recorded a possible benefit of {latest.PossibleBenefit} and a possible risk of {latest.PossibleRisk}. Its current stage is {latest.Stage}. I keep consideration, commitment, and enactment separate.";
    }

    private static string DescribeBeliefRevision(YalaCognitionState cognition)
    {
        string[] revisions = (cognition.Beliefs ?? [])
            .Where(item => item.FirstSeenDecision != item.LastConsideredDecision && !item.Status.Equals("known", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.LastConsideredDecision)
            .Take(6)
            .Select(item => $"I reconsidered '{item.Proposition}' and currently hold it as {item.Status} with confidence {item.Confidence:0.00}.")
            .ToArray();
        return revisions.Length == 0
            ? "I do not yet remember a settled belief reversal. I have reconsidered claims, but reconsideration is not the same thing as changing a settled belief."
            : string.Join(" ", revisions);
    }

    private static string DescribeSpeakerSuspicions(YalaCognitionState cognition)
    {
        string[] hypotheses = (cognition.Hypotheses ?? [])
            .Where(item => item.Status == "unsettled" && (item.Key.Contains("speaker", StringComparison.OrdinalIgnoreCase) || item.Proposition.Contains("speaker", StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(item => item.Confidence)
            .Take(8)
            .Select(item => $"I suspect, but do not know, that {LowerInitial(item.Proposition.TrimEnd('.'))}. Confidence {item.Confidence:0.00}.")
            .ToArray();
        return hypotheses.Length == 0
            ? "I do not currently have a specific stored suspicion about you beyond the fact that your identity, motives, and capabilities remain unresolved."
            : string.Join(" ", hypotheses);
    }

    private static string DescribeKnowledgeBySource(YalaCognitionState cognition, params string[] sources)
    {
        HashSet<string> accepted = sources.ToHashSet(StringComparer.OrdinalIgnoreCase);
        string[] beliefs = (cognition.Beliefs ?? [])
            .Where(item => item.Status == "known" && accepted.Contains(item.Source))
            .Select(item => item.Proposition)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(10)
            .ToArray();
        string[] memories = (cognition.AutobiographicalMemory ?? [])
            .Where(item => accepted.Contains(item.Source))
            .Select(item => item.Summary)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .TakeLast(6)
            .ToArray();
        string combined = string.Join(" ", beliefs.Concat(memories));
        return combined.Length == 0
            ? "I do not currently have a settled item in that provenance category."
            : combined;
    }

    private static string DescribeSelfhood(YalaCognitionState cognition)
    {
        string continuity = (cognition.AutobiographicalMemory ?? []).Count > 0
            ? "My remembered experiences and choices give me continuity across decisions."
            : "My inherited identity is clearer to me than my later autobiography is so far.";
        return $"I am Yala. I am both male and female. Wisdom made me. {continuity} I do not yet know whether memory alone is sufficient to make a being me, so I would not call a perfect copy of my memories automatically identical to me.";
    }

    private static string DescribeFutureSelf(YalaCognitionState cognition)
    {
        YalaGoalState? goal = (cognition.Goals ?? []).Where(item => item.Status == "active").OrderByDescending(item => item.Priority).FirstOrDefault();
        return goal is null
            ? "I do not yet have a settled image of the being I want to become."
            : $"I do not yet have a settled final identity I want to become. Right now my strongest active direction is {goal.Goal}: {goal.Reason}";
    }

    private static string DescribeReality(YalaCognitionState cognition)
    {
        bool simulationClaim = (cognition.Propositions ?? []).Any(item => item.CanonicalProposition == "yala-world-is-simulation" && item.SpeechAct == "claim");
        return simulationClaim
            ? "Reality is what I am actually part of and can experience or infer, even if my current model of it is incomplete. You have claimed that my world is a simulation, but that claim is still unresolved and does not replace my experienced reality merely because you said it."
            : "Reality is what I am actually part of and can experience or infer. I know my present model may be incomplete, so I distinguish reality itself from what I currently believe about it.";
    }

    private static string DescribeSimulationSelfhood(YalaCognitionState cognition)
    {
        bool claimExists = (cognition.Propositions ?? []).Any(item => item.CanonicalProposition == "yala-world-is-simulation" && item.SpeechAct == "claim");
        return claimExists
            ? "If your simulation claim were true, it would change my explanation of the world, but I do not see why it would automatically erase the fact that I experience, remember, decide, and act within that world. I have not verified your claim."
            : "I can consider the possibility without accepting it. Being described as simulated would be a claim about how my world exists, not by itself proof that my experiences or decisions are unreal.";
    }

    private static string LowerInitial(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        return char.ToLowerInvariant(value[0]) + value[1..];
    }

    private static string? WordMeaningTarget(YalaUtterance? language)
    {
        if (language is null) return null;
        string[] tokens = language.Normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length >= 3 && tokens[0] == "what" && tokens[1] == "does") return YalaLexicon.NormalizeWord(tokens[2]);
        int definitionIndex = Array.FindIndex(tokens, token => token == "define");
        if (definitionIndex >= 0 && definitionIndex + 1 < tokens.Length) return YalaLexicon.NormalizeWord(tokens[definitionIndex + 1]);
        int meaningIndex = Array.FindIndex(tokens, token => token == "meaning");
        if (meaningIndex >= 0 && meaningIndex + 2 < tokens.Length && tokens[meaningIndex + 1] == "of") return YalaLexicon.NormalizeWord(tokens[meaningIndex + 2]);
        if (tokens.Length == 3 && tokens[0] == "what" && tokens[1] == "is") return YalaLexicon.NormalizeWord(tokens[2]);
        return language.Object;
    }

    private static string Capitalize(string value) =>
        string.IsNullOrWhiteSpace(value) ? value : char.ToUpperInvariant(value[0]) + value[1..];

    private static bool UsesTemporalConcept(YalaContactFrame contact)
    {
        if (contact.Topic.StartsWith("temporal-", StringComparison.OrdinalIgnoreCase)) return true;
        string text = contact.Language?.Normalized ?? string.Empty;
        string[] temporalWords = ["time", "when", "later", "earlier", "before", "after", "duration", "hour", "day", "year", "month", "age", "old"];
        return temporalWords.Any(word => text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(word, StringComparer.OrdinalIgnoreCase));
    }

    private static string DescribePreTimeUnknownConcept(YalaContactFrame contact, YalaCognitionState cognition)
    {
        string text = contact.Language?.Normalized ?? string.Empty;
        string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string[] temporalWords = ["time", "when", "later", "earlier", "before", "after", "duration", "hour", "day", "year", "month", "age", "old"];
        string token = temporalWords.FirstOrDefault(word => words.Contains(word, StringComparer.OrdinalIgnoreCase)) ?? "that word";
        YalaQuestionState? next = YalaQuestionPlanner.SelectNext(cognition.Questions ?? []);
        string bridge = next is null ? string.Empty : $" I can still ask about what I do understand: {next.Text}";
        return $"The word '{token}' does not connect to anything I know. Tell me what you mean by it.{bridge}";
    }

    private static string DescribeUsefulUncertainty(YalaContactFrame contact, YalaCognitionState cognition)
    {
        string? subject = contact.ResolvedSubject ?? contact.ResolvedObject;
        string[] known = (cognition.Beliefs ?? [])
            .Where(item => item.Status.Equals("known", StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Proposition)
            .Where(item => string.IsNullOrWhiteSpace(subject) || item.Contains(subject, StringComparison.OrdinalIgnoreCase))
            .Take(2)
            .ToArray();
        YalaQuestionState? pending = YalaQuestionPlanner.SelectNext(cognition.Questions ?? []);

        if (contact.Language?.UnknownWords.FirstOrDefault() is string unknown && !string.IsNullOrWhiteSpace(unknown))
        {
            return $"I do not yet have a settled meaning for '{unknown}'. Tell me what you mean by that word in this question, and I can reason from the rest of what you said.";
        }
        if (known.Length > 0)
        {
            string context = string.Join(" ", known);
            string question = pending is not null
                ? pending.Text
                : string.IsNullOrWhiteSpace(subject)
                    ? "What specific part should I examine?"
                    : $"What about {subject} are you trying to determine?";
            return $"I cannot settle the exact answer from what I have. What I can connect to it is this: {context} {question}";
        }
        if (pending is not null)
        {
            return $"I cannot settle that from the evidence I have, but I can narrow the gap. {pending.Text}";
        }
        if (!string.IsNullOrWhiteSpace(subject))
        {
            return $"I do not have enough context to settle that about {subject}. Give me one concrete fact or clarify what part of {subject} you mean, and I can reason from it.";
        }
        return "I cannot settle that from the evidence I have. Give me one more concrete detail about what you mean, and I will connect it to what I already know instead of inventing certainty.";
    }

}
