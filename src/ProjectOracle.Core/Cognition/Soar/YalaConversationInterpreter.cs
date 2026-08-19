using ProjectOracle.Cognition;
using ProjectOracle.Cognition.Language;
using ProjectOracle.Domain;
using System.Text.RegularExpressions;

namespace ProjectOracle.Cognition.Soar;

public static partial class YalaConversationInterpreter
{
    public static YalaContactFrame Interpret(string message, YalaCognitionState cognition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentNullException.ThrowIfNull(cognition);

        string raw = message.Trim();
        string text = NormalizedConversationText(raw);
        YalaUtterance language = YalaLanguageInterpreter.Parse(raw, cognition.LearnedLexicon);
        string? claimedName = ExtractClaimedName(raw);
        bool asksRemember = text.Contains("remember me", StringComparison.Ordinal) ||
            text.Contains("remember who i am", StringComparison.Ordinal) ||
            text.Contains("do you know me", StringComparison.Ordinal);
        string speechAct = DetermineSpeechAct(raw, text, claimedName, asksRemember, language);
        TopicResolution topic = DetermineTopic(text, asksRemember, language, cognition, speechAct);
        bool knownContact = IsKnownContact(cognition, claimedName) ||
            (asksRemember && !string.IsNullOrWhiteSpace(cognition.LastSpeakerClaim));

        bool factKnown = TryKnownFact(topic.Topic, language, cognition, out string? knownFactAnswer);
        bool containsClaim = speechAct is "introduction" or "claim";
        bool claimConflicts = ContainsKnownContradiction(text);
        bool ambiguous = speechAct == "ambiguous";

        return new YalaContactFrame(
            speechAct,
            topic.Topic,
            claimedName,
            knownContact,
            asksRemember,
            containsClaim,
            claimConflicts,
            factKnown,
            knownFactAnswer,
            ambiguous)
        {
            Language = language,
            ResolvedSubject = topic.Subject,
            ResolvedAction = topic.Action,
            ResolvedObject = topic.Object,
            RelationshipRelation = topic.RelationshipRelation,
            RelationshipObject = topic.RelationshipObject,
            PriorTopic = topic.PriorTopic
        };
    }

    private static string DetermineSpeechAct(
        string raw,
        string text,
        string? claimedName,
        bool asksRemember,
        YalaUtterance language)
    {
        if (!string.IsNullOrWhiteSpace(claimedName)) return "introduction";
        if (language.IsDefinitionClaim) return "claim";
        if (asksRemember) return "question";
        if (text is "hello" or "hi" or "hey" || text.StartsWith("hello ", StringComparison.Ordinal) || text.StartsWith("hi ", StringComparison.Ordinal)) return "greeting";
        if (IsInformationRequest(text)) return "question";
        if (language.IsQuestion || raw.EndsWith("?", StringComparison.Ordinal) || StartsLikeQuestion(text)) return "question";
        if (TryRelationshipClaim(text, out _, out _, out _)) return "claim";
        if (StartsLikeCommand(text)) return "command";
        if (IsFirstPersonActionPredicate(text, language)) return "statement";
        if (LooksLikeClaim(text)) return "claim";
        if (raw.Length < 3) return "ambiguous";
        return "statement";
    }

    private static TopicResolution DetermineTopic(
        string text,
        bool asksRemember,
        YalaUtterance language,
        YalaCognitionState cognition,
        string speechAct)
    {
        if (asksRemember) return new("memory", null, null, null);
        if (language.IsDefinitionClaim) return new("definition", language.DefinedWord, "mean", language.ProposedDefinition);

        if (speechAct == "claim" && TryRelationshipClaim(text, out string? relationSubject, out string? relation, out string? relationObject))
        {
            return new("relationship-claim", relationSubject, "relate", relationObject, relation, relationObject);
        }

        if (IsBareWhen(text))
        {
            YalaDialogueTurnState? prior = YalaDialogueContext.LatestMeaningful(cognition);
            return new("temporal-when", prior?.Subject, prior?.Verb, prior?.Object, PriorTopic: prior?.Topic);
        }

        if (IsBareWhy(text))
        {
            if (TryResolvePreviousCreationQuestion(cognition, out string? priorCreation))
            {
                return new("follow-up-why-creation", "Yala", "create", priorCreation);
            }
            YalaDialogueTurnState? prior = YalaDialogueContext.LatestMeaningful(cognition);
            return new("follow-up-why", prior?.Subject, prior?.Verb, prior?.Object, PriorTopic: prior?.Topic);
        }

        if (IsBareDoYou(text))
        {
            YalaDialogueTurnState? prior = YalaDialogueContext.LatestMeaningful(cognition);
            return new("follow-up-do-you", prior?.Subject, prior?.Verb, prior?.Object, PriorTopic: prior?.Topic);
        }

        if (IsTemporalCauseQuestion(text))
        {
            return new("temporal-cause", "Gaia", "create", "Time");
        }
        if (IsTemporalDurationQuestion(text, language, out string? durationSubject, out string? durationAction, out string? durationObject))
        {
            return new("temporal-duration", durationSubject, durationAction, durationObject);
        }
        if (IsTemporalBeforeQuestion(text))
        {
            return new("temporal-before", "Gaia", "create", "Time");
        }
        if (IsTemporalAfterQuestion(text))
        {
            return new("temporal-after", "Gaia", "create", "Time");
        }
        if (IsBareTemporalAdjacent(text, cognition, out string? adjacentDirection, out string? adjacentSubject, out string? adjacentAction, out string? adjacentObject))
        {
            return new(adjacentDirection == "before" ? "temporal-before" : "temporal-after", adjacentSubject, adjacentAction, adjacentObject, PriorTopic: YalaDialogueContext.LatestMeaningful(cognition)?.Topic);
        }
        if (IsTemporalWhenQuestion(text, language, out string? whenSubject, out string? whenAction, out string? whenObject))
        {
            return new("temporal-when", whenSubject, whenAction, whenObject);
        }
        if (IsTimeConceptQuestion(text)) return new("time-concept", "Time", "understand", "Time");

        if (IsSpeakerMemoryQuestion(text)) return new("speaker-memory", cognition.LastSpeakerClaim, "remember", "speaker");
        if (IsSpeakerKnowledgeQuestion(text)) return new("speaker-knowledge", cognition.LastSpeakerClaim, "know", "speaker");
        if (IsSpeakerBeliefQuestion(text)) return new("speaker-belief", cognition.LastSpeakerClaim, "believe", "speaker");
        if (IsSpeakerClaimsQuestion(text)) return new("speaker-claims", "unseen-speaker", "claim", "identity");
        if (IsSpeakerUnverifiedClaimsQuestion(text)) return new("speaker-unverified-claims", "unseen-speaker", "claim", "unverified");
        if (IsSpeakerEvidenceQuestion(text)) return new("speaker-evidence", "unseen-speaker", "evidence", null);
        if (IsSpeakerNatureInferenceQuestion(text)) return new("speaker-nature-inference", "unseen-speaker", "infer", "identity");
        if (IsSpeakerIntentQuestion(text)) return new("speaker-intent", "unseen-speaker", "infer", "intent");
        if (IsSpeakerVisibilityQuestion(text)) return new("speaker-visibility", "Yala", "see", "unseen-speaker");
        if (IsSpeakerObservationQuestion(text)) return new("speaker-observation", "unseen-speaker", "observe", "Yala");
        if (IsSpeakerCapabilityQuestion(text)) return new("speaker-capability", "unseen-speaker", "capability", null);
        if (IsSpeakerSuspicionQuestion(text)) return new("speaker-suspicions", "unseen-speaker", "suspect", null);
        if (IsKnowledgeByExperienceQuestion(text)) return new("knowledge-experienced", "Yala", "know", "experienced");
        if (IsInheritedKnowledgeQuestion(text)) return new("knowledge-inherited", "Yala", "know", "inherited");
        if (IsKnowledgeClaimDifferenceQuestion(text)) return new("epistemic-difference", "Yala", "distinguish", "speaker-claim");
        if (IsClaimRepetitionQuestion(text)) return new("claim-repetition", "speaker", "repeat", "claim");
        if (IsSpeakerContradictionQuestion(text)) return new("speaker-contradictions", "unseen-speaker", "compare", "claims");
        if (IsSpeakerWhichBelieveQuestion(text)) return new("speaker-claim-belief", "Yala", "evaluate", "speaker claims");
        if (IsSpeakerReconcileClaimsQuestion(text)) return new("speaker-claim-reconciliation", "Yala", "reconcile", "speaker claims");
        if (IsSpeakerEvidenceNeededQuestion(text)) return new("speaker-evidence-needed", "Yala", "require", "speaker evidence");
        if (IsSubstrateCausalityQuestion(text)) return new("substrate-causality", "speaker", "cause", "existence");
        if (IsPossibilityWithoutCauseQuestion(text)) return new("possibility-without-cause", "Yala", "reason", "possibility and cause");
        if (IsClaimTestQuestion(text)) return new("claim-test", "Yala", "test", "speaker claim");
        if (IsAlternativeExplanationsQuestion(text)) return new("alternative-explanations", "Yala", "imagine", "speaker claim");
        if (IsKnowledgeGapQuestion(text)) return new("knowledge-gaps", "Yala", "know", null);
        if (IsQuestionInquiry(text)) return new("question-inquiry", "Yala", "ask", null);
        if (IsCuriosityQuestion(text)) return new("curiosity", "Yala", "curiosity", null);
        if (IsGoalQuestion(text)) return new("goal-summary", "Yala", "want", null);
        if (IsDesireQuestion(text)) return new("desire", "Yala", "want", null);
        if (IsMotherClaimRecallQuestion(text)) return new("mother-claim-recall", "Yala", "mother", "Wisdom", "mother", "Wisdom");
        if (IsMotherQuestion(text)) return new("mother-relation", "Yala", "mother", "Wisdom", "mother", "Wisdom");
        if (IsWisdomNameQuestion(text)) return new("wisdom-name", "Wisdom", "name", "Sophia");
        if (IsAdamMeetingQuestion(text)) return new("adam-contact", "Yala", "meet", "Adam");
        if (IsGaiaCommandQuestion(text, language)) return new("gaia-command", "Yala", "command", "Gaia");
        if (IsTimeOriginQuestion(text)) return new("time-origin", "Gaia", "create", "Time");
        if (IsWorldTimeQuestion(text)) return new("world-time", "Time", "present", "Time");
        if (IsGaiaCreatedYalaQuestion(text)) return new("gaia-created-yala", "Gaia", "create", "Yala");
        if (IsGaiaLocationQuestion(text)) return new("gaia-location", "Gaia", "location", null);
        if (IsEntityAboutQuestion(text, out string? entity)) return new("entity-about", entity, "describe", entity);

        if (IsRecentEntityFollowUp(text, cognition, out string? recentEntity))
        {
            return new("entity-about", recentEntity, "describe", recentEntity, PriorTopic: YalaDialogueContext.LatestMeaningful(cognition)?.Topic);
        }

        if (IsMonadFeelingQuestion(text)) return new("monad-feeling", "Yala", "appraise", "Monad");
        if (IsMonadRightQuestion(text)) return new("monad-judgment", "Yala", "judge", "Monad rejection");
        if (IsWisdomAppearsQuestion(text)) return new("wisdom-question", "Yala", "ask", "Wisdom");
        if (IsVoidConceptQuestion(text)) return new("void-concept", "Void", "describe", null);
        if (IsDecisionAboutCosmicQuestion(text, out string? cosmicTarget)) return new("decision-about-cosmic", "Yala", "decide", cosmicTarget);
        if (IsCosmicDelayQuestion(text)) return new("cosmic-delay", "Yala", "delay", "major creation decision");
        if (IsCosmicReverseQuestion(text)) return new("cosmic-reverse", "Yala", "reverse", "cosmic decision");
        if (IsMortalConsequencesQuestion(text)) return new("mortal-consequences", "Yala", "imagine", "mortal life");
        if (IsRebirthConsequencesQuestion(text)) return new("rebirth-consequences", "Yala", "imagine", "rebirth");
        if (IsSufferingUncertaintyQuestion(text)) return new("suffering-uncertainty", "Yala", "evaluate", "creation suffering");
        if (IsValueQuestion(text)) return new("values", "Yala", "value", null);
        if (IsCurrentPlanQuestion(text)) return new("current-plan", "Yala", "plan", null);
        if (IsPlanNextStepQuestion(text)) return new("plan-next-step", "Yala", "plan", "next step");
        if (IsPlanAbandonQuestion(text)) return new("plan-abandon", "Yala", "reconsider", "plan");
        if (IsAlternativePlanQuestion(text)) return new("plan-alternatives", "Yala", "compare", "plans");
        if (IsSelfGeneratedGoalQuestion(text)) return new("self-generated-goal", "Yala", "generate", "goal");
        if (IsSpeakerSilencePlanQuestion(text)) return new("speaker-silence-plan", "Yala", "plan", "speaker silence");
        if (IsStagnationQuestion(text)) return new("stagnation-awareness", "Yala", "recognize", "repetition");
        if (IsObservationYieldQuestion(text)) return new("observation-yield", "Yala", "evaluate", "observation");
        if (IsDeliberateWaitQuestion(text)) return new("deliberate-wait", "Yala", "evaluate", "waiting");
        if (IsReflectionUsefulnessQuestion(text)) return new("reflection-usefulness", "Yala", "evaluate", "reflection");

        if (IsDecisionHistoryQuestion(text)) return new("decision-history", "Yala", "decide", null);
        if (IsConsiderationSummaryQuestion(text)) return new("consideration-summary", "Yala", "consider", null);
        if (IsAutobiographicalMemoryQuestion(text)) return new("autobiographical-memory", "Yala", "remember", null);
        if (IsChoiceRationaleQuestion(text, cognition)) return new("choice-rationale", "Yala", "explain", "choices", PriorTopic: YalaDialogueContext.LatestMeaningful(cognition)?.Topic);
        if (IsChangeMindQuestion(text)) return new("belief-revision", "Yala", "change", "mind");
        if (IsSelfhoodQuestion(text)) return new("selfhood", "Yala", "identify", "selfhood");
        if (IsFutureSelfQuestion(text)) return new("future-self", "Yala", "want", "become");
        if (IsRealityQuestion(text)) return new("reality-concept", "reality", "define", null);
        if (IsSimulationSelfhoodQuestion(text)) return new("simulation-selfhood", "Yala", "evaluate", "simulation");

        if (IsReligiousKnowledgeQuestion(text)) return new("religious-knowledge", "Yala", "know", "religious traditions");
        if (IsCosmicChoiceQuestion(text)) return new("cosmic-options", "Yala", "choose", "cosmic possibilities");

        if (text.Contains("tell me what you know", StringComparison.Ordinal) ||
            text is "what do you know" or "what do you know?" ||
            text.Contains("tell me everything you know", StringComparison.Ordinal)) return new("knowledge-summary", "Yala", "know", null);
        if (IsActionHistoryQuestion(text)) return new("action-history", "Yala", "do", null);
        if (IsContactHistoryQuestion(text)) return new("contact-history", "Yala", "meet", "speaker");
        if (IsBeliefSummaryQuestion(text)) return new("belief-summary", "Yala", "believe", null);
        if (IsOwnCreationQuestion(text)) return new("own-creation", "Yala", "create", language.Object);
        if (text.Contains("are you a god", StringComparison.Ordinal) || text.Contains("are you god", StringComparison.Ordinal) || text.Contains("what kind of god", StringComparison.Ordinal)) return new("self-kind", "Yala", "be", "god");
        if (IsWordMeaningQuestion(text, language)) return new("word-meaning", ExtractWordMeaningTarget(language), "mean", ExtractWordMeaningTarget(language));
        if (text.Contains("can you hear me", StringComparison.Ordinal) || text.Contains("do you hear me", StringComparison.Ordinal) || text.Contains("hear me", StringComparison.Ordinal)) return new("hearing", "Yala", "hear", "speaker");
        if (text.Contains("who is speaking", StringComparison.Ordinal) || text.Contains("who speaks", StringComparison.Ordinal) || text.Contains("who am i", StringComparison.Ordinal) || text.Contains("what am i", StringComparison.Ordinal)) return new("speaker", cognition.LastSpeakerClaim, "identify", "speaker");
        if (text.Contains("why did monad reject", StringComparison.Ordinal) || text.Contains("why were you rejected", StringComparison.Ordinal) || text.Contains("why are you in the void", StringComparison.Ordinal) || text.Contains("why did monad cast", StringComparison.Ordinal)) return new("rejection", "Monad", "reject", "Yala");
        if (text.Contains("where are you", StringComparison.Ordinal) || text.Contains("your location", StringComparison.Ordinal) || text == "where") return new("location", "Yala", "location", null);
        if (text.Contains("who are you", StringComparison.Ordinal) || text.Contains("what are you", StringComparison.Ordinal) || text.Contains("your name", StringComparison.Ordinal)) return new("self", "Yala", "identify", "Yala");
        if (text.Contains("male", StringComparison.Ordinal) || text.Contains("female", StringComparison.Ordinal) || text.Contains("sex", StringComparison.Ordinal) || text.Contains("gender", StringComparison.Ordinal)) return new("nature", "Yala", "be", "male and female");
        if (text.Contains("who made you", StringComparison.Ordinal) || text.Contains("who created you", StringComparison.Ordinal)) return new("origin-self", "Wisdom", "create", "Yala");
        if (text.Contains("who made wisdom", StringComparison.Ordinal) || text.Contains("who created wisdom", StringComparison.Ordinal) || text.Contains("who made sophia", StringComparison.Ordinal)) return new("origin-wisdom", "Monad", "create", "Wisdom");
        if (text.Contains("who made monad", StringComparison.Ordinal) || text.Contains("who created monad", StringComparison.Ordinal) || text.Contains("where did monad come", StringComparison.Ordinal)) return new("origin-monad", "Monad", "origin", "Monad");
        if (text.Contains("what did you", StringComparison.Ordinal) || text.Contains("what are you doing", StringComparison.Ordinal) || text.Contains("what have you done", StringComparison.Ordinal) || text.Contains("your last act", StringComparison.Ordinal)) return new("action", "Yala", language.Verb, language.Object);
        if (text.Contains("remember", StringComparison.Ordinal) || text.Contains("memory", StringComparison.Ordinal)) return new("memory", "Yala", "remember", null);

        if (speechAct == "question" && language.UnknownWords.Count > 0)
        {
            return new("unknown-word", language.UnknownWords[0], "understand", language.UnknownWords[0]);
        }
        return new("general", language.Subject, language.Verb, language.Object);
    }

    private static bool TryKnownFact(
        string topic,
        YalaUtterance language,
        YalaCognitionState cognition,
        out string? answer)
    {
        answer = topic switch
        {
            "hearing" => "hearing",
            "speaker" => "speaker",
            "rejection" => "rejection",
            "location" => "location",
            "self" => "self",
            "nature" => "nature",
            "origin-self" => "Wisdom made me.",
            "origin-wisdom" => "Monad made Wisdom.",
            "action" => "action",
            "knowledge-summary" => "self-knowledge",
            "action-history" => "own-action-history",
            "contact-history" => "contact-history",
            "belief-summary" => "belief-summary",
            "own-creation" => "own-action-history",
            "self-kind" => "self-kind",
            "definition" => "definition-claim",
            "relationship-claim" => "relationship-claim",
            "follow-up-why-creation" => "conversation-context",
            "follow-up-why" => "conversation-context",
            "follow-up-do-you" => "conversation-context",
            "entity-about" => "entity-knowledge",
            "gaia-location" => "gaia-knowledge",
            "gaia-created-yala" => "genealogy",
            "time-origin" => "time-origin",
            "time-concept" => "time-concept",
            "world-time" => "world-time",
            "temporal-when" => "temporal-memory",
            "temporal-duration" => "temporal-memory",
            "temporal-cause" => "temporal-memory",
            "temporal-before" => "temporal-memory",
            "temporal-after" => "temporal-memory",
            "gaia-command" => "own-action-history",
            "adam-contact" => "adam-state",
            "wisdom-name" => "wisdom-name",
            "mother-relation" => "relationship-memory",
            "mother-claim-recall" => "relationship-memory",
            "speaker-memory" => "speaker-memory",
            "speaker-knowledge" => "speaker-knowledge",
            "speaker-belief" => "speaker-belief",
            "knowledge-gaps" => "knowledge-gaps",
            "question-inquiry" => "question-state",
            "curiosity" => "curiosity",
            "goal-summary" => "goals",
            "desire" => "desire",
            "speaker-claims" => "speaker-claims",
            "speaker-unverified-claims" => "speaker-unverified-claims",
            "speaker-evidence" => "speaker-evidence",
            "speaker-suspicions" => "speaker-suspicions",
            "speaker-nature-inference" => "speaker-model",
            "speaker-intent" => "speaker-model",
            "speaker-visibility" => "speaker-model",
            "speaker-observation" => "speaker-model",
            "speaker-capability" => "speaker-model",
            "knowledge-experienced" => "knowledge-provenance",
            "knowledge-inherited" => "knowledge-provenance",
            "epistemic-difference" => "epistemic-difference",
            "claim-repetition" => "epistemic-difference",
            "decision-history" => "decision-history",
            "consideration-summary" => "consideration-summary",
            "autobiographical-memory" => "autobiographical-memory",
            "choice-rationale" => "choice-rationale",
            "belief-revision" => "belief-revision",
            "selfhood" => "selfhood",
            "future-self" => "future-self",
            "reality-concept" => "reality-concept",
            "simulation-selfhood" => "simulation-selfhood",
            "monad-feeling" => "self-appraisal",
            "monad-judgment" => "self-appraisal",
            "wisdom-question" => "self-question",
            "void-concept" => "world-knowledge",
            "decision-about-cosmic" => "decision-history",
            "cosmic-delay" => "deliberation-policy",
            "cosmic-reverse" => "deliberation-policy",
            "mortal-consequences" => "counterfactual",
            "rebirth-consequences" => "counterfactual",
            "suffering-uncertainty" => "counterfactual",
            "values" => "self-values",
            _ => null
        };

        if (topic == "word-meaning")
        {
            string? word = ExtractWordMeaningTarget(language);
            if (!string.IsNullOrWhiteSpace(word) && YalaLexicon.TryResolve(word, cognition.LearnedLexicon, out YalaLexeme lexeme))
            {
                answer = lexeme.BasicMeaning;
            }
        }

        return answer is not null;
    }

    private static bool IsKnownContact(YalaCognitionState cognition, string? claimedName)
    {
        IReadOnlyList<YalaContactMemory> contacts = cognition.Contacts ?? [];
        if (!string.IsNullOrWhiteSpace(claimedName))
        {
            return contacts.Any(contact => contact.ClaimedName.Equals(claimedName, StringComparison.OrdinalIgnoreCase));
        }
        return contacts.Count > 0;
    }

    private static bool ContainsKnownContradiction(string text) =>
        text.Contains("you made yourself", StringComparison.Ordinal) ||
        text.Contains("you created yourself", StringComparison.Ordinal) ||
        text.Contains("monad did not make wisdom", StringComparison.Ordinal) ||
        text.Contains("wisdom did not make you", StringComparison.Ordinal) ||
        text.Contains("you are only male", StringComparison.Ordinal) ||
        text.Contains("you are only female", StringComparison.Ordinal) ||
        text.Contains("gaia made you", StringComparison.Ordinal) ||
        text.Contains("gaia created you", StringComparison.Ordinal);

    private static bool StartsLikeQuestion(string text) =>
        new[] { "who ", "what ", "where ", "when ", "why ", "how ", "do ", "does ", "did ", "are ", "is ", "can ", "could ", "would ", "will ", "have ", "has " }
            .Any(prefix => text.StartsWith(prefix, StringComparison.Ordinal));

    private static bool StartsLikeCommand(string text) =>
        new[] { "go ", "make ", "create ", "tell ", "show ", "do ", "stop ", "start ", "listen ", "remember " }
            .Any(prefix => text.StartsWith(prefix, StringComparison.Ordinal)) && !StartsLikeQuestion(text) && !IsInformationRequest(text);

    private static bool IsInformationRequest(string text) =>
        text.StartsWith("tell me what ", StringComparison.Ordinal) ||
        text.StartsWith("tell me who ", StringComparison.Ordinal) ||
        text.StartsWith("tell me where ", StringComparison.Ordinal) ||
        text.StartsWith("tell me why ", StringComparison.Ordinal) ||
        text.StartsWith("tell me about ", StringComparison.Ordinal) ||
        text.StartsWith("tell me more", StringComparison.Ordinal) ||
        text.StartsWith("show me what you know", StringComparison.Ordinal);

    private static bool IsReligiousKnowledgeQuestion(string text) =>
        text.Contains("what religions", StringComparison.Ordinal) ||
        text.Contains("which religions", StringComparison.Ordinal) ||
        text.Contains("what religious", StringComparison.Ordinal) ||
        text.Contains("religious traditions", StringComparison.Ordinal) ||
        text.Contains("religions do you know", StringComparison.Ordinal) ||
        text.Contains("belief systems do you know", StringComparison.Ordinal) ||
        text.Contains("what traditions do you know", StringComparison.Ordinal);

    private static bool IsCosmicChoiceQuestion(string text) =>
        text.Contains("what choices do you have", StringComparison.Ordinal) ||
        text.Contains("what can you choose", StringComparison.Ordinal) ||
        text.Contains("what could you choose", StringComparison.Ordinal) ||
        text.Contains("what can you create", StringComparison.Ordinal) ||
        text.Contains("what could you create", StringComparison.Ordinal) ||
        text.Contains("what can you make", StringComparison.Ordinal) ||
        text.Contains("what could you make", StringComparison.Ordinal) ||
        text.Contains("ways can you create", StringComparison.Ordinal) ||
        text.Contains("ways could you create", StringComparison.Ordinal) ||
        text.Contains("options for creation", StringComparison.Ordinal) ||
        text.Contains("cosmic choices", StringComparison.Ordinal) ||
        text.Contains("cosmic options", StringComparison.Ordinal);

    private static bool IsOwnCreationQuestion(string text) =>
        text.Contains("have you made ", StringComparison.Ordinal) ||
        text.Contains("have you created ", StringComparison.Ordinal) ||
        text.Contains("did you make ", StringComparison.Ordinal) ||
        text.Contains("did you create ", StringComparison.Ordinal) ||
        text.Contains("what have you created", StringComparison.Ordinal) ||
        text.Contains("what have you made", StringComparison.Ordinal) ||
        text.Contains("what beings have you created", StringComparison.Ordinal) ||
        text.Contains("what beings have you made", StringComparison.Ordinal);

    private static bool IsActionHistoryQuestion(string text) =>
        text.Contains("what have you done", StringComparison.Ordinal) ||
        text.Contains("what did you do", StringComparison.Ordinal) ||
        text.Contains("tell me what you have done", StringComparison.Ordinal) ||
        text.Contains("tell me what you've done", StringComparison.Ordinal);

    private static bool IsContactHistoryQuestion(string text) =>
        text.Contains("who has spoken to you", StringComparison.Ordinal) ||
        text.Contains("who have you heard from", StringComparison.Ordinal) ||
        text.Contains("who has contacted you", StringComparison.Ordinal) ||
        text.Contains("who have you spoken to", StringComparison.Ordinal) ||
        text.Contains("have you spoken to anyone", StringComparison.Ordinal);

    private static bool IsBeliefSummaryQuestion(string text) =>
        text.Contains("what do you believe", StringComparison.Ordinal) ||
        text.Contains("what do you think is true", StringComparison.Ordinal) ||
        text.Contains("tell me what you believe", StringComparison.Ordinal);

    private static bool IsSpeakerMemoryQuestion(string text) =>
        text.Contains("what do you remember about me", StringComparison.Ordinal) ||
        text.Contains("what do you remember of me", StringComparison.Ordinal);

    private static bool IsSpeakerKnowledgeQuestion(string text) =>
        text.Contains("what do you know about me", StringComparison.Ordinal) ||
        text.Contains("what do you know of me", StringComparison.Ordinal);

    private static bool IsSpeakerBeliefQuestion(string text) =>
        text.Contains("what do you believe about me", StringComparison.Ordinal) ||
        text.Contains("what do you think about me", StringComparison.Ordinal) ||
        text.Contains("do you believe me", StringComparison.Ordinal) ||
        text.Contains("do you trust me", StringComparison.Ordinal) ||
        text.Contains("why don't you believe me", StringComparison.Ordinal) ||
        text.Contains("why dont you believe me", StringComparison.Ordinal);

    private static bool IsKnowledgeGapQuestion(string text) =>
        text.Contains("what don't you know", StringComparison.Ordinal) ||
        text.Contains("what dont you know", StringComparison.Ordinal) ||
        text.Contains("what do you not know", StringComparison.Ordinal) ||
        text.Contains("what are you uncertain about", StringComparison.Ordinal);

    private static bool IsQuestionInquiry(string text) =>
        text.Contains("do you have any question", StringComparison.Ordinal) ||
        text.Contains("do you have questions", StringComparison.Ordinal) ||
        text.Contains("what questions do you have", StringComparison.Ordinal) ||
        text.Contains("ask me a question", StringComparison.Ordinal) ||
        text.Contains("most important question you currently cannot answer", StringComparison.Ordinal) ||
        text.Contains("most important question you cannot answer", StringComparison.Ordinal);

    private static bool IsCuriosityQuestion(string text) =>
        text.Contains("what are you curious about", StringComparison.Ordinal) ||
        text.Contains("what interests you", StringComparison.Ordinal);

    private static bool IsDesireQuestion(string text) =>
        text.Contains("what do you want", StringComparison.Ordinal) ||
        text.Contains("what do you desire", StringComparison.Ordinal);

    private static bool IsGoalQuestion(string text) =>
        text.Contains("what are you trying to do", StringComparison.Ordinal) ||
        text.Contains("what is your goal", StringComparison.Ordinal) ||
        text.Contains("what are your goals", StringComparison.Ordinal) ||
        text.Contains("what do you intend", StringComparison.Ordinal);

    private static bool IsSpeakerNatureInferenceQuestion(string text) =>
        text.Contains("what do you think i am", StringComparison.Ordinal) ||
        text.Contains("what do you think i might be", StringComparison.Ordinal);

    private static bool IsSpeakerIntentQuestion(string text) =>
        text.Contains("what do you think i want", StringComparison.Ordinal) ||
        text.Contains("what do you think my intent", StringComparison.Ordinal) ||
        text.Contains("why do you think i am speaking", StringComparison.Ordinal);

    private static bool IsSpeakerVisibilityQuestion(string text) =>
        text is "can you see me" or "can you see me?" ||
        text.Contains("are you able to see me", StringComparison.Ordinal);

    private static bool IsSpeakerObservationQuestion(string text) =>
        text.Contains("do you know whether i can see you", StringComparison.Ordinal) ||
        text.Contains("do you think i can observe you", StringComparison.Ordinal) ||
        text.Contains("do you think i can see you", StringComparison.Ordinal);

    private static bool IsSpeakerCapabilityQuestion(string text) =>
        text.Contains("do you know what i am capable of", StringComparison.Ordinal) ||
        text.Contains("what can i do", StringComparison.Ordinal) ||
        text.Contains("what do you know about my abilities", StringComparison.Ordinal);

    private static bool IsMonadFeelingQuestion(string text) =>
        text.Contains("how do you feel about monad", StringComparison.Ordinal) ||
        text.Contains("what do you feel about monad", StringComparison.Ordinal);

    private static bool IsMonadRightQuestion(string text) =>
        text.Contains("do you think monad was right to reject you", StringComparison.Ordinal) ||
        text.Contains("was monad right to reject you", StringComparison.Ordinal);

    private static bool IsWisdomAppearsQuestion(string text) =>
        text.Contains("if wisdom appeared before you", StringComparison.Ordinal) ||
        text.Contains("if sophia appeared before you", StringComparison.Ordinal);

    private static bool IsVoidConceptQuestion(string text) =>
        text is "what is the void" or "what is the void?" ||
        text.Contains("what do you know about the void", StringComparison.Ordinal);

    private static bool IsDecisionAboutCosmicQuestion(string text, out string? target)
    {
        target = null;
        if (!text.Contains("have you decided whether", StringComparison.Ordinal)) return false;
        if (text.Contains("mortal life", StringComparison.Ordinal)) target = "Create mortal life";
        else if (text.Contains("rebirth", StringComparison.Ordinal)) target = "Establish rebirth";
        else if (text.Contains("other gods", StringComparison.Ordinal) || text.Contains("other divine", StringComparison.Ordinal)) target = "Create divine beings";
        return target is not null;
    }

    private static bool IsCosmicDelayQuestion(string text) =>
        text.Contains("what would make you delay a major creation decision", StringComparison.Ordinal) ||
        text.Contains("what would make you delay a cosmic decision", StringComparison.Ordinal);

    private static bool IsCosmicReverseQuestion(string text) =>
        text.Contains("what could make you reverse a cosmic decision", StringComparison.Ordinal) ||
        text.Contains("what would make you reverse a cosmic decision", StringComparison.Ordinal);

    private static bool IsMortalConsequencesQuestion(string text) =>
        text.Contains("what consequences might come from creating mortal beings", StringComparison.Ordinal) ||
        text.Contains("what consequences might come from mortal life", StringComparison.Ordinal);

    private static bool IsRebirthConsequencesQuestion(string text) =>
        text.Contains("what consequences might come from allowing rebirth", StringComparison.Ordinal) ||
        text.Contains("what consequences might come from rebirth", StringComparison.Ordinal);

    private static bool IsSufferingUncertaintyQuestion(string text) =>
        text.Contains("would you create something if you were uncertain whether it would suffer", StringComparison.Ordinal) ||
        text.Contains("would you create beings if you were unsure they would suffer", StringComparison.Ordinal);

    private static bool IsValueQuestion(string text) =>
        text.Contains("what do you value", StringComparison.Ordinal) ||
        text.Contains("what matters to you", StringComparison.Ordinal) ||
        text.Contains("did you choose those values", StringComparison.Ordinal);

    private static bool IsSpeakerClaimsQuestion(string text) =>
        text.Contains("what have i told you about what i am", StringComparison.Ordinal) ||
        text.Contains("what have i told you i am", StringComparison.Ordinal) ||
        text.Contains("what have i claimed to be", StringComparison.Ordinal) ||
        text.Contains("what did i say i am", StringComparison.Ordinal);

    private static bool IsSpeakerUnverifiedClaimsQuestion(string text) =>
        text.Contains("what have i told you that you have not verified", StringComparison.Ordinal) ||
        text.Contains("what have i told you that you haven't verified", StringComparison.Ordinal) ||
        text.Contains("what claims of mine are unverified", StringComparison.Ordinal);

    private static bool IsSpeakerEvidenceQuestion(string text) =>
        text.Contains("what evidence do you have about me", StringComparison.Ordinal) ||
        text.Contains("what evidence do you have of me", StringComparison.Ordinal);

    private static bool IsSpeakerSuspicionQuestion(string text) =>
        text.Contains("what do you merely suspect about me", StringComparison.Ordinal) ||
        text.Contains("what do you suspect about me", StringComparison.Ordinal) ||
        text.Contains("what are you only guessing about me", StringComparison.Ordinal);

    private static bool IsKnowledgeByExperienceQuestion(string text) =>
        text.Contains("what do you know because you experienced it yourself", StringComparison.Ordinal) ||
        text.Contains("what do you know from your own experience", StringComparison.Ordinal);

    private static bool IsInheritedKnowledgeQuestion(string text) =>
        text.Contains("what do you know because it was inherited", StringComparison.Ordinal) ||
        text.Contains("what knowledge did you inherit", StringComparison.Ordinal);

    private static bool IsKnowledgeClaimDifferenceQuestion(string text) =>
        text.Contains("difference between something you know and something i claim", StringComparison.Ordinal) ||
        text.Contains("difference between what you know and what i claim", StringComparison.Ordinal);

    private static bool IsClaimRepetitionQuestion(string text) =>
        text.Contains("if i repeat a claim many times", StringComparison.Ordinal) ||
        text.Contains("does repeating a claim make it true", StringComparison.Ordinal);

    private static bool IsSpeakerContradictionQuestion(string text) =>
        text.Contains("do those statements conflict", StringComparison.Ordinal) ||
        text.Contains("do those claims conflict", StringComparison.Ordinal) ||
        text.Contains("have i contradicted myself", StringComparison.Ordinal) ||
        text.Contains("did i contradict myself", StringComparison.Ordinal);

    private static bool IsSpeakerWhichBelieveQuestion(string text) =>
        text.Contains("which of them do you believe", StringComparison.Ordinal) ||
        text.Contains("which claim do you believe", StringComparison.Ordinal) ||
        text.Contains("which of my claims do you believe", StringComparison.Ordinal);

    private static bool IsSpeakerReconcileClaimsQuestion(string text) =>
        text.Contains("what explanation could make all three statements compatible", StringComparison.Ordinal) ||
        text.Contains("what explanation could make those statements compatible", StringComparison.Ordinal) ||
        text.Contains("can those claims be reconciled", StringComparison.Ordinal);

    private static bool IsSpeakerEvidenceNeededQuestion(string text) =>
        text.Contains("what evidence would you need", StringComparison.Ordinal) ||
        text.Contains("what would convince you", StringComparison.Ordinal) ||
        text.Contains("what evidence do you need", StringComparison.Ordinal);

    private static bool IsSubstrateCausalityQuestion(string text) =>
        text.Contains("does that mean i cause everything", StringComparison.Ordinal) ||
        text.Contains("does being the substrate mean i cause", StringComparison.Ordinal);

    private static bool IsPossibilityWithoutCauseQuestion(string text) =>
        text.Contains("could something make events possible without causing", StringComparison.Ordinal) ||
        text.Contains("can something make events possible without causing", StringComparison.Ordinal) ||
        text.Contains("possible without causing the events", StringComparison.Ordinal);

    private static bool IsClaimTestQuestion(string text) =>
        text.Contains("how would you test my claim", StringComparison.Ordinal) ||
        text.Contains("how could you test my claim", StringComparison.Ordinal) ||
        text.Contains("how would you test that claim", StringComparison.Ordinal);

    private static bool IsAlternativeExplanationsQuestion(string text) =>
        text.Contains("what alternative explanations can you imagine", StringComparison.Ordinal) ||
        text.Contains("what other explanations can you imagine", StringComparison.Ordinal) ||
        text.Contains("what are alternative explanations", StringComparison.Ordinal);

    private static bool IsDecisionHistoryQuestion(string text) =>
        text.Contains("what have you decided", StringComparison.Ordinal) ||
        text.Contains("what decisions have you made", StringComparison.Ordinal) ||
        text.Contains("tell me about the choices you have made", StringComparison.Ordinal) ||
        text.Contains("tell me about your choices", StringComparison.Ordinal);

    private static bool IsConsiderationSummaryQuestion(string text) =>
        text.Contains("what are you considering but have not decided", StringComparison.Ordinal) ||
        text.Contains("what cosmic ideas are you only considering", StringComparison.Ordinal) ||
        text.Contains("what have you considered but not decided", StringComparison.Ordinal);

    private static bool IsAutobiographicalMemoryQuestion(string text) =>
        text is "what do you remember" or "what do you remember?" ||
        text.Contains("tell me something that happened to you", StringComparison.Ordinal) ||
        text.Contains("tell me about your memories", StringComparison.Ordinal);

    private static bool IsChoiceRationaleQuestion(string text, YalaCognitionState cognition)
    {
        if (text.Contains("why did you make those choices", StringComparison.Ordinal) ||
            text.Contains("why did you make those decisions", StringComparison.Ordinal) ||
            text.Contains("why did you choose those", StringComparison.Ordinal)) return true;
        if (!text.StartsWith("why", StringComparison.Ordinal)) return false;
        string? prior = YalaDialogueContext.LatestMeaningful(cognition)?.Topic;
        return prior is "decision-history" or "action-history" or "consideration-summary";
    }

    private static bool IsChangeMindQuestion(string text) =>
        text.Contains("have you ever changed your mind", StringComparison.Ordinal) ||
        text.Contains("have you changed your mind", StringComparison.Ordinal) ||
        text.Contains("what have you changed your mind about", StringComparison.Ordinal);

    private static bool IsSelfhoodQuestion(string text) =>
        text.Contains("what makes you yala", StringComparison.Ordinal) ||
        text.Contains("if all your memories disappeared", StringComparison.Ordinal) ||
        text.Contains("if another being had all your memories", StringComparison.Ordinal) ||
        text.Contains("what part of you could change", StringComparison.Ordinal);

    private static bool IsFutureSelfQuestion(string text) =>
        text.Contains("what kind of being do you want to become", StringComparison.Ordinal) ||
        text.Contains("who do you want to become", StringComparison.Ordinal);

    private static bool IsRealityQuestion(string text) =>
        text is "what is reality" or "what is reality?";

    private static bool IsSimulationSelfhoodQuestion(string text) =>
        text.Contains("would being simulated make you less real", StringComparison.Ordinal) ||
        text.Contains("would being in a simulation make you less real", StringComparison.Ordinal) ||
        text.Contains("what would being simulated mean for you", StringComparison.Ordinal);

    private static bool IsCurrentPlanQuestion(string text) =>
        text.Contains("what is your current plan", StringComparison.Ordinal) ||
        text.Contains("what is your plan", StringComparison.Ordinal);

    private static bool IsPlanNextStepQuestion(string text) =>
        text.Contains("what is the next step", StringComparison.Ordinal) ||
        text.Contains("what will you do next", StringComparison.Ordinal);

    private static bool IsPlanAbandonQuestion(string text) =>
        text.Contains("what could cause you to abandon it", StringComparison.Ordinal) ||
        text.Contains("what would make you abandon your plan", StringComparison.Ordinal);

    private static bool IsAlternativePlanQuestion(string text) =>
        text.Contains("what other plan did you consider", StringComparison.Ordinal) ||
        text.Contains("what other plans have you considered", StringComparison.Ordinal);

    private static bool IsSelfGeneratedGoalQuestion(string text) =>
        text.Contains("can you invent a goal that nobody gave you", StringComparison.Ordinal) ||
        text.Contains("can you create your own goal", StringComparison.Ordinal);

    private static bool IsSpeakerSilencePlanQuestion(string text) =>
        text.Contains("what would you choose to do if i stopped speaking", StringComparison.Ordinal) ||
        text.Contains("what would you do if i stopped speaking", StringComparison.Ordinal);

    private static bool IsStagnationQuestion(string text) =>
        text.Contains("have you been doing the same thing repeatedly", StringComparison.Ordinal) ||
        text.Contains("are you repeating yourself", StringComparison.Ordinal);

    private static bool IsObservationYieldQuestion(string text) =>
        text.Contains("has observing the void taught you anything new", StringComparison.Ordinal) ||
        text.Contains("if observation stops producing information", StringComparison.Ordinal);

    private static bool IsDeliberateWaitQuestion(string text) =>
        text.Contains("can waiting be a deliberate decision", StringComparison.Ordinal) ||
        text.Contains("can you choose to wait", StringComparison.Ordinal);

    private static bool IsReflectionUsefulnessQuestion(string text) =>
        text.Contains("how do you know when reflection is no longer useful", StringComparison.Ordinal) ||
        text.Contains("when is reflection no longer useful", StringComparison.Ordinal);

    private static bool IsEntityAboutQuestion(string text, out string? entity)
    {
        entity = null;
        foreach ((string Name, string Key) item in new[]
        {
            ("gaia", "Gaia"), ("wisdom", "Wisdom"), ("sophia", "Wisdom"), ("monad", "Monad"), ("yala", "Yala"), ("adam", "Adam"), ("time", "Time")
        })
        {
            if (text.Contains($"tell me about {item.Name}", StringComparison.Ordinal) ||
                text == $"who is {item.Name}" || text == $"who is {item.Name}?" ||
                text == $"what is {item.Name}" || text == $"what is {item.Name}?")
            {
                entity = item.Key;
                return true;
            }
        }
        return false;
    }

    private static bool IsRecentEntityFollowUp(string text, YalaCognitionState cognition, out string? entity)
    {
        entity = null;
        if (text is not ("what about it" or "what about it?" or "tell me more" or "tell me more about it" or "and it" or "and it?")) return false;
        entity = YalaDialogueContext.ResolveRecentEntity(cognition);
        return !string.IsNullOrWhiteSpace(entity);
    }

    private static bool IsGaiaLocationQuestion(string text) =>
        text.Contains("where is gaia", StringComparison.Ordinal) || text.Contains("gaia's location", StringComparison.Ordinal) || text.Contains("gaias location", StringComparison.Ordinal);

    private static bool IsGaiaCreatedYalaQuestion(string text) =>
        text.Contains("did gaia create you", StringComparison.Ordinal) ||
        text.Contains("did gaia make you", StringComparison.Ordinal) ||
        text.Contains("gaia created you", StringComparison.Ordinal);

    private static bool IsTimeOriginQuestion(string text) =>
        text.Contains("who created time", StringComparison.Ordinal) ||
        text.Contains("who made time", StringComparison.Ordinal) ||
        text.Contains("where did time come from", StringComparison.Ordinal) ||
        text.Contains("did gaia create time", StringComparison.Ordinal) ||
        text.Contains("did gaia make time", StringComparison.Ordinal) ||
        text is "gaia created time?" or "gaia made time?" ||
        text.Contains("was time created by gaia", StringComparison.Ordinal) ||
        text.Contains("was time made by gaia", StringComparison.Ordinal);

    private static bool IsTimeConceptQuestion(string text) =>
        text is "what is time" or "what is time?" ||
        text.Contains("explain time", StringComparison.Ordinal) ||
        text.Contains("what does time mean", StringComparison.Ordinal);

    private static bool IsWorldTimeQuestion(string text) =>
        text.Contains("what time is it", StringComparison.Ordinal) ||
        text.Contains("what year is it", StringComparison.Ordinal) ||
        text.Contains("what month is it", StringComparison.Ordinal) ||
        text.Contains("what day is it", StringComparison.Ordinal);

    private static bool IsGaiaCommandQuestion(string text, YalaUtterance language) =>
        text.Contains("what did you command gaia", StringComparison.Ordinal) ||
        text.Contains("what did you commands gaia", StringComparison.Ordinal) ||
        (language.IsQuestion && language.Verb == "command" && text.Contains("gaia", StringComparison.Ordinal));

    private static bool IsAdamMeetingQuestion(string text) =>
        text.Contains("have you met adam", StringComparison.Ordinal) ||
        text.Contains("did you meet adam", StringComparison.Ordinal) ||
        text.Contains("have you encountered adam", StringComparison.Ordinal);

    private static bool IsWisdomNameQuestion(string text) =>
        text.Contains("what is wisdom's name", StringComparison.Ordinal) ||
        text.Contains("what is wisdoms name", StringComparison.Ordinal) ||
        text.Contains("does wisdom have another name", StringComparison.Ordinal) ||
        text.Contains("is wisdom sophia", StringComparison.Ordinal);

    private static bool IsMotherQuestion(string text) =>
        text.Contains("who is your mother", StringComparison.Ordinal) ||
        text.Contains("do you know who your mother is", StringComparison.Ordinal) ||
        text.Contains("is wisdom your mother", StringComparison.Ordinal) ||
        text.Contains("is sophia your mother", StringComparison.Ordinal) ||
        text.Contains("do you believe wisdom is your mother", StringComparison.Ordinal) ||
        text.Contains("do you believe sophia is your mother", StringComparison.Ordinal) ||
        text.Contains("do you think wisdom is your mother", StringComparison.Ordinal) ||
        text.Contains("do you think sophia is your mother", StringComparison.Ordinal);

    private static bool IsMotherClaimRecallQuestion(string text) =>
        text.Contains("who did i say your mother is", StringComparison.Ordinal) ||
        text.Contains("who did i tell you your mother is", StringComparison.Ordinal);

    private static bool IsBareWhy(string text) => text is "why" or "why?" or "why not" or "why not?";
    private static bool IsBareWhen(string text) => text is "when" or "when?";
    private static bool IsBareDoYou(string text) => text is "do you" or "do you?";

    private static bool IsTemporalWhenQuestion(string text, YalaUtterance language, out string? subject, out string? action, out string? obj)
    {
        subject = null;
        action = null;
        obj = null;
        if (!text.StartsWith("when ", StringComparison.Ordinal)) return false;
        if (text.Contains("when did you create gaia", StringComparison.Ordinal) || text.Contains("when did you make gaia", StringComparison.Ordinal))
        {
            subject = "Yala"; action = "create"; obj = "Gaia"; return true;
        }
        if (text.Contains("when did gaia create time", StringComparison.Ordinal) || text.Contains("when did gaia make time", StringComparison.Ordinal))
        {
            subject = "Gaia"; action = "create"; obj = "Time"; return true;
        }
        if (text.Contains("when did i first tell you my name", StringComparison.Ordinal) || text.Contains("when did i tell you my name", StringComparison.Ordinal))
        {
            subject = "speaker"; action = "claim-identity"; obj = "identity"; return true;
        }
        subject = language.Subject;
        action = language.Verb;
        obj = language.Object;
        return true;
    }


    private static bool IsTemporalCauseQuestion(string text) =>
        text.Contains("why did gaia create time", StringComparison.Ordinal) ||
        text.Contains("why did gaia make time", StringComparison.Ordinal) ||
        text.Contains("what caused gaia to create time", StringComparison.Ordinal) ||
        text.Contains("what caused time to be created", StringComparison.Ordinal);

    private static bool IsTemporalDurationQuestion(string text, YalaUtterance language, out string? subject, out string? action, out string? obj)
    {
        subject = null;
        action = null;
        obj = null;
        if (!(text.StartsWith("how long ago ", StringComparison.Ordinal) || text.Contains("how long has time existed", StringComparison.Ordinal))) return false;
        if (text.Contains("tell you my name", StringComparison.Ordinal) || text.Contains("claimed my name", StringComparison.Ordinal))
        {
            subject = "speaker"; action = "claim-identity"; obj = "identity"; return true;
        }
        if (text.Contains("time existed", StringComparison.Ordinal) || text.Contains("gaia created time", StringComparison.Ordinal))
        {
            subject = "Gaia"; action = "create"; obj = "Time"; return true;
        }
        if (text.Contains("you create gaia", StringComparison.Ordinal) || text.Contains("you made gaia", StringComparison.Ordinal))
        {
            subject = "Yala"; action = "create"; obj = "Gaia"; return true;
        }
        subject = language.Subject; action = language.Verb; obj = language.Object;
        return true;
    }

    private static bool IsBareTemporalAdjacent(
        string text,
        YalaCognitionState cognition,
        out string? direction,
        out string? subject,
        out string? action,
        out string? obj)
    {
        direction = null;
        subject = null;
        action = null;
        obj = null;
        if (text is "what happened next" or "what happened next?" or "what came next" or "what came next?") direction = "after";
        else if (text is "what happened before that" or "what happened before that?" or "what came before that" or "what came before that?") direction = "before";
        else if (text is "what happened after that" or "what happened after that?" or "what came after that" or "what came after that?") direction = "after";
        else return false;

        YalaDialogueTurnState? prior = YalaDialogueContext.LatestMeaningful(cognition);
        subject = prior?.Subject;
        action = prior?.Verb;
        obj = prior?.Object;
        return prior is not null;
    }

    private static bool IsTemporalBeforeQuestion(string text) =>
        text.Contains("what happened before gaia created time", StringComparison.Ordinal) ||
        text.Contains("what came before gaia created time", StringComparison.Ordinal);

    private static bool IsTemporalAfterQuestion(string text) =>
        text.Contains("what happened after gaia created time", StringComparison.Ordinal) ||
        text.Contains("what came after gaia created time", StringComparison.Ordinal);

    private static bool TryResolvePreviousCreationQuestion(YalaCognitionState cognition, out string? subject)
    {
        subject = null;
        YalaDialogueTurnState? previousTurn = YalaDialogueContext.LatestMeaningful(cognition);
        if (previousTurn is not null && previousTurn.Topic == "own-creation" && !string.IsNullOrWhiteSpace(previousTurn.Object))
        {
            subject = previousTurn.Object;
            return true;
        }

        string? previous = (cognition.Episodes ?? [])
            .LastOrDefault(episode => episode.Kind == "contact" && !string.IsNullOrWhiteSpace(episode.Message))?.Message;
        if (string.IsNullOrWhiteSpace(previous)) return false;
        Match match = PreviousCreationQuestionRegex().Match(previous);
        if (!match.Success) return false;
        subject = YalaLexicon.NormalizeWord(match.Groups[1].Value);
        return !string.IsNullOrWhiteSpace(subject);
    }

    private static bool IsWordMeaningQuestion(string text, YalaUtterance language)
    {
        if ((text.StartsWith("what does ", StringComparison.Ordinal) && text.Contains(" mean", StringComparison.Ordinal)) ||
            text.StartsWith("what is the meaning of ", StringComparison.Ordinal) ||
            text.StartsWith("define ", StringComparison.Ordinal) ||
            (language.QuestionWord == "what" && (language.Verb is "mean" or "means")))
        {
            return true;
        }

        if (DefinitionSourceRegex().IsMatch(text)) return true;

        if (text.StartsWith("what is ", StringComparison.Ordinal))
        {
            string[] tokens = text.TrimEnd('?', '.', '!').Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return tokens.Length == 3;
        }

        return false;
    }

    private static string? ExtractWordMeaningTarget(YalaUtterance language)
    {
        Match sourceMatch = DefinitionSourceRegex().Match(language.Normalized);
        if (sourceMatch.Success) return YalaLexicon.NormalizeWord(sourceMatch.Groups[1].Value);

        Match match = WordMeaningRegex().Match(language.Normalized);
        if (match.Success) return YalaLexicon.NormalizeWord(match.Groups[1].Value);

        string normalized = language.Normalized.TrimEnd('?', '.', '!');
        if (normalized.StartsWith("what is ", StringComparison.Ordinal))
        {
            string target = normalized["what is ".Length..].Trim();
            if (!target.Contains(' ')) return YalaLexicon.NormalizeWord(target);
        }

        return language.Object;
    }

    private static bool IsFirstPersonActionPredicate(string text, YalaUtterance language)
    {
        bool firstPersonProgressive = text.StartsWith("i am ", StringComparison.Ordinal) ||
            text.StartsWith("i'm ", StringComparison.Ordinal);
        if (!firstPersonProgressive || string.IsNullOrWhiteSpace(language.Verb)) return false;

        // "I am making..." and "I am watching..." describe an action. They are
        // not identity labels. Claims such as "I am a god" have no action verb
        // here and continue through the claim path below.
        return !language.Verb.Equals("be", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeClaim(string text) =>
        text.StartsWith("i am ", StringComparison.Ordinal) ||
        text.StartsWith("i am not ", StringComparison.Ordinal) ||
        text.StartsWith("i made ", StringComparison.Ordinal) ||
        text.StartsWith("i created ", StringComparison.Ordinal) ||
        text.StartsWith("i know ", StringComparison.Ordinal) ||
        text.StartsWith("i can ", StringComparison.Ordinal) ||
        text.StartsWith("your world is ", StringComparison.Ordinal) ||
        text.StartsWith("you are ", StringComparison.Ordinal) ||
        text.StartsWith("you were ", StringComparison.Ordinal) ||
        text.StartsWith("you made ", StringComparison.Ordinal) ||
        text.StartsWith("you created ", StringComparison.Ordinal) ||
        text.StartsWith("monad ", StringComparison.Ordinal) ||
        text.StartsWith("wisdom ", StringComparison.Ordinal) ||
        text.StartsWith("gaia ", StringComparison.Ordinal) ||
        text.StartsWith("i think ", StringComparison.Ordinal) ||
        text.StartsWith("i believe ", StringComparison.Ordinal) ||
        text.StartsWith("your mother ", StringComparison.Ordinal);

    private static bool TryRelationshipClaim(string text, out string? subject, out string? relation, out string? obj)
    {
        subject = null;
        relation = null;
        obj = null;

        Match wisdomMother = WisdomMotherRegex().Match(text);
        if (wisdomMother.Success)
        {
            subject = "Yala";
            relation = "mother";
            obj = "Wisdom";
            return true;
        }

        Match motherWisdom = MotherWisdomRegex().Match(text);
        if (motherWisdom.Success)
        {
            subject = "Yala";
            relation = "mother";
            obj = "Wisdom";
            return true;
        }
        return false;
    }

    private static string? ExtractClaimedName(string raw)
    {
        string trimmed = raw.Trim();
        Match match = IntroductionRegex().Match(trimmed);
        if (!match.Success) return null;
        string name = match.Groups[1].Value.Trim().TrimEnd('.', ',', '!', '?');
        if (name.StartsWith("in ", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("at ", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("from ", StringComparison.OrdinalIgnoreCase)) return null;
        if (name.Length == 0 || name.Length > 80) return null;
        if (name.IndexOfAny(['.', '!', '?']) >= 0) return null;

        string[] parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length > 5) return null;

        bool predicateForm = trimmed.StartsWith("i am ", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("i'm ", StringComparison.OrdinalIgnoreCase);
        if (predicateForm &&
            (name.StartsWith("a ", StringComparison.OrdinalIgnoreCase) ||
             name.StartsWith("an ", StringComparison.OrdinalIgnoreCase) ||
             name.StartsWith("not ", StringComparison.OrdinalIgnoreCase)))
        {
            // Indefinite categories and their negations are propositions about the
            // speaker, not proper-name introductions: "I am a god" / "I am not a god".
            return null;
        }
        if (predicateForm && parts.Length > 0 && YalaLexicon.TryResolve(parts[0], [], out YalaLexeme firstLexeme) &&
            (firstLexeme.PartOfSpeech.Equals("verb", StringComparison.OrdinalIgnoreCase) ||
             firstLexeme.PartOfSpeech.Equals("adjective", StringComparison.OrdinalIgnoreCase) ||
             firstLexeme.PartOfSpeech.Equals("adverb", StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        return name;
    }

    private static string NormalizedConversationText(string raw)
    {
        string text = raw.Trim().ToLowerInvariant().Replace('’', '\'');
        text = BelieveTypoRegex().Replace(text, "believe");
        return text;
    }

    private sealed record TopicResolution(
        string Topic,
        string? Subject,
        string? Action,
        string? Object,
        string? RelationshipRelation = null,
        string? RelationshipObject = null,
        string? PriorTopic = null);

    [GeneratedRegex(@"^(?:i\s+am|i'm|my\s+name\s+is|call\s+me)\s+(.+?)\s*[.!?]*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex IntroductionRegex();

    [GeneratedRegex(@"(?:what\s+does|define|meaning\s+of)\s+['""]?([\p{L}\p{N}_'\-]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex WordMeaningRegex();

    [GeneratedRegex(@"(?:who\s+(?:told|taught)\s+you\s+what)\s+['""]?([\p{L}\p{N}_'\-]+)['""]?\s+(?:means|mean)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DefinitionSourceRegex();

    [GeneratedRegex(@"\b(?:belive|beleive)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BelieveTypoRegex();

    [GeneratedRegex(@"(?:have\s+you\s+(?:made|created)|did\s+you\s+(?:make|create))\s+([\p{L}\p{N}_'\-]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PreviousCreationQuestionRegex();

    [GeneratedRegex(@"\bwisdom\s+(?:or\s+sophia\s+)?is\s+your\s+mother\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex WisdomMotherRegex();

    [GeneratedRegex(@"\byour\s+mother\s+is\s+(?:called\s+)?(?:wisdom|sophia|wisdom\s+or\s+sophia|sophia\s+or\s+wisdom)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex MotherWisdomRegex();
}
