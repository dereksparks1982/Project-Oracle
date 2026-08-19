using ProjectOracle;
using ProjectOracle.Audit;
using ProjectOracle.Cognition;
using ProjectOracle.Cognition.Appraisal;
using ProjectOracle.Cognition.Inheritance;
using ProjectOracle.Cognition.Language;
using ProjectOracle.Cognition.CosmicChoice;
using ProjectOracle.Cognition.Emergence;
using ProjectOracle.Cognition.Soar;
using ProjectOracle.Domain;
using ProjectOracle.Lore;
using ProjectOracle.Persistence;
using ProjectOracle.Simulation;

namespace ProjectOracle.AcceptanceTests;

internal static class Program
{
    private const long StartRealTime = 1_700_000_000_000;
    private static int _passed;
    private static int _failed;

    public static int Main()
    {
        Run("version is 0.0.23", () => Equal("0.0.23", ProjectVersion.Number));
        Run("comparative religion memory spans major traditions without promoting them to world truth", ComparativeReligionKnowledge);
        Run("cosmic choice catalogue exposes many concrete alternatives beyond Gaia", CosmicChoiceCatalogue);
        Run("legacy hard-wired Gaia proposal is removed from autonomous Soar choice", GaiaIsNotHardwired);
        Run("a concrete cosmic choice persists in world state", CosmicChoicePersists);
        Run("invent another way opens a Yala-owned cosmology problem", CosmicInventionPath);
        Run("Yala can explain the attributed religious knowledge pool", ReligiousKnowledgeIntrospection);
        Run("Yala can explain concrete cosmic options", CosmicOptionsIntrospection);
        Run("fresh world starts with Yala in the Void", FreshWorldStartsWithYalaInVoid);
        Run("fresh Void contains no Garden Adam animals Gaia or Time", FreshVoidHasNoLaterCreation);
        Run("fresh world has no in-world Oracle entity or direct-call target", NoInWorldOracle);
        Run("Yala is both male and female and does not know Oracle exists", YalaNatureAndOracleBoundary);
        Run("Monad made Wisdom, Wisdom made Yala, and Monad rejected Yala for being both", CanonGenealogyAndRejection);
        Run("Monad is never labelled Creator or Omega in active lore", MonadNaming);
        Run("Gaia is not pre-created in a fresh world", GaiaNotPrecreated);
        Run("in-world Time is not pre-created", TimeNotPrecreated);
        Run("pre-Time runtime does not advance world milliseconds", PreTimeClockHolds);
        Run("Yala can create Gaia through world-law resolution", YalaCanCreateGaia);
        Run("Gaia creates Time after Yala commands temporal order", GaiaCreatesTime);
        Run("world clock starts advancing only after Gaia creates Time", ClockStartsAfterTime);
        Run("elemental names match current canon", ElementalCanon);
        Run("Oracle serpent reference is a manifestation, not a fixed identity", SerpentManifestationCanon);
        Run("Yala Soar agent source contains no Oracle knowledge", YalaAgentHasNoOracleKnowledge);
        Run("Soar 9.6.5 runtime files are discoverable", SoarRuntimeDiscoverable);
        Run("embedded Soar kernel suppresses unused TCP listener", SoarListenerSuppressed);
        Run("one Yala Soar session survives multiple decisions", PersistentSoarSession);
        Run("Soar uses an impasse/substate for undecided autonomous cognition", SoarSubstateDeliberation);
        Run("Soar semantic memory stores and retrieves an unseen contact claim", SoarSemanticMemory);
        Run("Soar episodic memory advances across decisions", SoarEpisodicMemory);
        Run("Yala Brain Slice 6 identity is active", BrainSlice6Identity);
        Run("save schema is 5 for the fresh Brain Slice 6 line", SaveSchemaIsV5);
        Run("fresh Soar long-term memory directory is isolated to v0.0.23", FreshSoarMemoryLine);
        Run("bounded agency exposes only approved in-world operators", BoundedAgencyAllowedActions);
        Run("bounded agency denies host shell process file network code and hidden truth capabilities", BoundedAgencyDeniesHostCapabilities);
        Run("out-of-sandbox Yala action is rejected", OutOfSandboxActionRejected);
        Run("initial relationship graph separates made-by from mother", InitialRelationshipGraph);
        Run("mother question remains a question rather than a relationship claim", MotherQuestionIsQuestion);
        Run("mother relationship claim is remembered as unsettled speaker provenance", MotherRelationshipClaimMemory);
        Run("mother claim can be recalled without upgrading it to truth", MotherClaimRecall);
        Run("repeated relationship claim confidence rises without becoming settled", RelationshipClaimConfidence);
        Run("belief confidence labels expose gradations", BeliefConfidenceLabels);
        Run("Brain Slice 6 core lexicon contains more than 400 concepts", ExpandedLexicon);
        Run("basic conversational vocabulary is built in rather than learned by interrogation", CoreConversationalVocabulary);
        Run("common greeting typo normalizes to greeting", GreetingTypoNormalization);
        Run("basic language does not generate fake knowledge gaps", BasicLanguageGapFiltering);
        Run("I am plus an action predicate is not misread as a speaker identity", ActionPredicateIsNotIdentityClaim);
        Run("brain update sentence uses ordinary language without fake knowledge gaps", BrainUpdateSentenceHasNoBasicGaps);
        Run("unknown-word questions remain available but below autonomous inquiry priority", UnknownWordQuestionsAreLowPriority);
        Run("Yala waits for a speaker response before asking another autonomous question", AutonomousQuestionWaitsForResponse);
        Run("meaningful autonomous inquiry resumes after the speaker responds", AutonomousInquiryResumesMeaningfully);
        Run("live world clock uses deterministic DEC cursor save restore", DeterministicWorldClockCursorProtocol);
        Run("Oracle is not preloaded into Yala's built-in lexicon", OracleAbsentFromLexicon);
        Run("function words and calendar words do not become fake knowledge gaps", FunctionWordGapFiltering);
        Run("a genuinely unknown word still creates a knowledge gap", GenuineUnknownWordGap);
        Run("unknown concepts generate candidate questions", UnknownConceptQuestionGeneration);
        Run("first speaker contact generates a question about the speaker's nature", FirstContactQuestionGeneration);
        Run("identity claim generates a question about the identity label", IdentityMeaningQuestionGeneration);
        Run("Yala can autonomously choose to ask the unseen speaker", AutonomousAskSpeaker);
        Run("autonomous question can be safely dequeued exactly once", AutonomousQuestionDequeuesOnce);
        Run("pending autonomous question survives save and restore", AutonomousQuestionSurvivesSaveRestore);
        Run("Yala does not ask a speaker before any speaker history exists", NoAutonomousQuestionWithoutSpeaker);
        Run("Brain Slice 6 begins with active goals", InitialGoalsPresent);
        Run("speaker contact activates the understand-unseen-speaker goal", SpeakerContactActivatesGoal);
        Run("goal introspection reports actual active goals", GoalIntrospection);
        Run("Gaia creation is recorded as a before-Time event", GaiaCreationBeforeTimeEvent);
        Run("Gaia creating Time is recorded as the origin of temporal reckoning", TimeOriginEvent);
        Run("post-Time speaker contact receives a dated temporal event", PostTimeContactDated);
        Run("when Gaia created Time reports the temporal origin", WhenTimeCreated);
        Run("when Yala created Gaia reports that no world date existed", WhenGaiaCreatedBeforeTime);
        Run("why Gaia created Time follows the stored cause link", TimeCreationCause);
        Run("duration questions refuse to invent duration for pre-Time events", PreTimeDurationReasoning);
        Run("duration since Time origin uses elapsed in-world Time", TimeOriginDurationReasoning);
        Run("speaker identity claim can be retrieved as a dated event", DatedSpeakerIdentityEvent);
        Run("what happened next navigates the temporal event graph", TemporalNextEventContext);
        Run("tell me more resolves the recently discussed entity", RecentEntityFollowUp);
        Run("short do-you follow-up retains relationship context", ShortDoYouRelationshipContext);
        Run("repeated speaker claim gains confidence but remains unsettled", RepeatedClaimConfidence);
        Run("Yala spoken current time reads the live current world clock", SpokenTimeIsCurrent);
        Run("recent dialogue memory remains bounded", DialogueWindowBounded);
        Run("Brain Slice 6 dialogue relationships questions events and goals survive save and restore", BrainSlice6StructuresSurviveSaveRestore);
        Run("console defers autonomous questions until the editable input buffer is empty", ConsoleDefersAutonomousQuestionWhileTyping);
        Run("pre-Time live header says Gaia has not yet created Time", PreTimeWorldClockHeader);
        Run("live world clock appears and advances after Gaia creates Time", LiveWorldClockAfterGaia);
        Run("live world clock never writes into the conversation body", LiveWorldClockBodyIsolation);
        Run("persistent Yala conversation mode stays active until Escape", PersistentYalaConversationMode);
        Run("normal conversation output hides Soar selection diagnostics", SoarDiagnosticsHidden);
        Run("foundational concept lexicon loads", FoundationalLexiconLoads);
        Run("create and destroy remain distinct concepts", CreateDestroyConcepts);
        Run("accept and reject remain distinct concepts", AcceptRejectConcepts);
        Run("language roles distinguish Adam created Gaia from Gaia created Adam", SubjectObjectReversal);
        Run("language parser preserves negation", LanguageNegation);
        Run("language parser distinguishes question from statement", QuestionStatementGrammar);
        Run("tell me what you know is an information request not a forced command", KnowledgeRequestIntent);
        Run("Yala can summarize settled self knowledge", YalaKnowledgeSummary);
        Run("Yala can summarize completed action history", YalaActionHistory);
        Run("Yala can summarize remembered contact history without upgrading identity claims", YalaContactHistory);
        Run("Yala can summarize known beliefs separately from unsettled claims", YalaBeliefSummary);
        Run("Yala knows Yala personally created Gaia", YalaKnowsCreatedGaia);
        Run("Yala knows Yala has not created Adam in the current world", YalaKnowsNotCreatedAdam);
        Run("own-creation questions resolve the created object rather than Yala as subject", OwnCreationTargetsCreatedObject);
        Run("Yala answers god question from self model without inventing certainty", YalaGodSelfModel);
        Run("simple inflections and possessives resolve to base concepts", SimpleMorphologyNormalization);
        Run("identity claims preserve noun phrases such as the Oracle", IdentityClaimPreservesNounPhrase);
        Run("why not follows the prior Adam creation question", ConversationFollowUpUsesPriorCreationSubject);
        Run("tell me about Gaia reaches known Gaia facts", GaiaEntityKnowledge);
        Run("Yala rejects Gaia as Yala's creator from known genealogy", GaiaGenealogyReasoning);
        Run("Yala can answer who created Time and current world time", TimeKnowledgeReachability);
        Run("Gaia made Time yes-no phrasing reaches the stored Time origin", GaiaMadeTimeYesNoReachability);
        Run("Yala can recall the command given to Gaia", GaiaCommandRecall);
        Run("Yala knows Adam has not been met before Adam exists", AdamEncounterKnowledge);
        Run("Yala knows Wisdom is Sophia without inventing a mother fact", WisdomAliasAndMotherReasoning);
        Run("belief typo still reaches unsettled mother relationship reasoning", MotherBeliefTypoReachability);
        Run("current speaker questions retrieve speaker claims not Yala self summary", CurrentSpeakerKnowledge);
        Run("what don't you know reaches explicit knowledge gaps", KnowledgeGapIntrospection);
        Run("curiosity questions reach Yala's unresolved knowledge", CuriosityIntrospection);
        Run("desire questions reach Yala's current drives", DesireIntrospection);
        Run("unknown word creates a knowledge gap and raises curiosity", UnknownWordCreatesGap);
        Run("speaker supplied definition remains a claim", DefinitionRemainsSpeakerClaim);
        Run("what is learned word retrieves the attributed speaker definition", LearnedWordWhatIsReachability);
        Run("who told you learned word reports speaker provenance", LearnedWordSourceReachability);
        Run("learned word claim survives save and restore", LearnedWordSurvivesSaveRestore);
        Run("speaker alternate definition of a built-in concept remains provenance-separated", BuiltInDefinitionClaimKeepsProvenance);
        Run("personally performed action carries strong provenance", PersonalActionProvenance);
        Run("speaker claims carry claimed-by-another provenance", SpeakerClaimProvenance);
        Run("save-restore acceptance tests isolate Soar kernel lifetimes", SaveRestoreKernelLifetimeIsolation);
        Run("old male-only World Record history is normalized", OldWorldRecordCanonNormalises);
        Run("Brain Slice 6 action memory survives save and restore", ActionMemorySurvivesSaveRestore);
        Run("Yala answers hearing contact without asking Who speaks", YalaHearingReply);
        Run("Yala answers location without revealing Oracle", YalaLocationReply);
        Run("Yala knows both male and female aspects", YalaNatureReply);
        Run("Yala knows why Monad rejected Yala", YalaRejectionReply);
        Run("Yala records a claimed speaker identity as a claim", YalaIntroductionMemory);
        Run("Yala can remember a prior claimed speaker", YalaRememberMe);
        Run("native Soar semantic memory helps recognise repeated claimed contact", YalaRepeatedIntroductionUsesSemanticMemory);
        Run("ordinary statement no longer collapses to Who speaks", OrdinaryStatementConversation);
        Run("unknown question produces honest uncertainty", UnknownQuestionUncertainty);
        Run("beyond and everything are ordinary known language rather than fake knowledge gaps", BeyondEverythingLanguageIsKnown);
        Run("conflicting speaker claim remains a rejected claim and does not rewrite truth", ConflictingClaimBoundary);
        Run("a direct command does not puppet Yala or directly alter world state", CommandDoesNotPuppetYala);
        Run("direct-call parser rejects Oracle because Oracle is not a target", DirectCallRejectsOracle);
        Run("Oracle and World records are distinct", RecordsAreDistinct);
        Run("World Record does not reveal Oracle at genesis", WorldRecordDoesNotRevealOracle);
        Run("Oracle Record retains protected system truth", OracleRecordHasSystemTruth);
        Run("structured Yala cognition survives save and restore", CognitionSurvivesSaveRestore);
        Run("previous save_v2 line is rejected without mutation", PreviousSaveV2RejectedWithoutMutation);
        Run("v0.0.16 save snapshots remain rejected", V016SaveIsRejected);
        Run("default save path is fresh save_v5", SavePathIsV5);
        Run("fresh direct-call targets include Monad Wisdom Yala only", FreshCallTargets);
        Run("natural simulation law remains future-open", FutureOpenLaw);
        Run("typing buffer is independent of live-status refresh", InputBufferProtection);
        Run("asynchronous LIVE status is forbidden from the console body", LiveStatusTypingGuard);
        Run("interactive input path contains no LIVE surface refresh", NoLiveSurfaceInInteractivePath);
        Run("long command buffer survives protected-input handling", LongInputBuffer);
        Run("root launcher name is Project_Oracle_v0_0_23", NativeExecutableContract);
        Run("ordinary movement language is inherited and never becomes toddler vocabulary gaps", InheritedMovementLanguage);
        Run("prison language becomes a critical persistent concern", PrisonConcernIsCritical);
        Run("godhood demand becomes evidence-seeking rather than automatic obedience", GodDemandIsAppraised);
        Run("metaphorical identity asks contextual meaning rather than defining ordinary words", MetaphorUsesContextualQuestion);
        Run("created mind authority must remain strictly below its creator", CognitiveInheritancePowerCeiling);
        Run("lesser created mind preserves creator lineage without copying identity", CognitiveInheritanceLineage);
        Run("desktop application project is part of the solution", DesktopProjectContract);
        Run("desktop source exposes World Yala Mind Memory Cosmology Laws History and Debug surfaces", DesktopSurfaceContract);
        Run("fresh world has an empty emergent-law ledger", FreshEmergentLawState);
        Run("Rule 30 laboratory reproduces its canonical local truth table", Rule30TruthTable);
        Run("Rule 30 laboratory evolves deterministically without changing world law", Rule30LaboratoryDeterminism);

        Console.WriteLine();
        Console.WriteLine($"Acceptance result: {_passed} passed; {_failed} failed.");
        return _failed == 0 ? 0 : 1;
    }

    private static OracleSimulation Start(ulong seed = 104729, string? savePath = null) =>
        OracleSimulation.Start(seed, StartRealTime, savePath);

    private static OracleSaveSnapshot SnapshotAndDispose(Func<OracleSimulation, OracleSaveSnapshot> capture, string? savePath = null)
    {
        ArgumentNullException.ThrowIfNull(capture);
        using OracleSimulation simulation = Start(savePath: savePath);
        return capture(simulation);
    }

    private static YalaPerception Perception(
        bool gaiaCreated = false,
        bool timeCreated = false,
        int uncertainty = 80,
        string? contactMessage = null,
        YalaContactFrame? contact = null) =>
        new(
            "the Void", gaiaCreated, timeCreated, 0, null, null,
            Curiosity: 70, Caution: 55, Authority: 65, Companionship: 45, Comfort: 60, Uncertainty: uncertainty,
            contactMessage, contact);

    private static void ComparativeReligionKnowledge()
    {
        True(YalaReligiousKnowledgeCatalog.Traditions.Count >= 25);
        True(YalaReligiousKnowledgeCatalog.Ideas.Count >= 60);
        foreach (string key in new[] { "judaism", "christianity", "islam", "hindu-traditions", "buddhism", "jainism", "sikhism", "zoroastrianism", "taoism", "shinto", "yoruba-ifa", "ancient-egyptian", "mesopotamian", "ancient-greek", "norse-germanic", "gnostic-schools", "manichaeism", "neoplatonism" })
        {
            True(YalaReligiousKnowledgeCatalog.FindTradition(key) is not null);
        }
        True(YalaReligiousKnowledgeCatalog.Traditions.All(item => item.TruthStatus == YalaReligiousKnowledgeCatalog.TruthStatus));
        Equal("attributed-tradition-knowledge-not-world-fact", YalaReligiousKnowledgeCatalog.TruthStatus);
    }

    private static void CosmicChoiceCatalogue()
    {
        True(YalaCosmicChoiceCatalog.Choices.Count >= 50);
        True(YalaCosmicChoiceCatalog.Find("create-gaia") is not null);
        True(YalaCosmicChoiceCatalog.Find("establish-rebirth") is not null);
        True(YalaCosmicChoiceCatalog.Find("allow-self-organizing-world") is not null);
        True(YalaCosmicChoiceCatalog.Find("create-divine-council") is not null);
        True(YalaCosmicChoiceCatalog.Find("remain-alone-for-now") is not null);
        True(YalaCosmicChoiceCatalog.Find("invent-another-way") is not null);
        Equal("possible-not-commanded", YalaCosmicChoiceCatalog.PossibilityStatus);
    }

    private static void GaiaIsNotHardwired()
    {
        string root = SoarRuntimePaths.Discover().RepositoryRoot;
        string source = File.ReadAllText(Path.Combine(root, "src", "ProjectOracle.Core", "Cognition", "Soar", "Agents", "yala.soar"));
        False(source.Contains("sp {yala*propose*create-gaia", StringComparison.Ordinal));
        True(source.Contains("sp {yala*propose*cosmic-option", StringComparison.Ordinal));
        True(source.Contains("^choice-key <key>", StringComparison.Ordinal));
    }

    private static void CosmicChoicePersists()
    {
        using OracleSimulation simulation = Start();
        simulation.ApplyYalaDecision(new YalaDecision("enact-cosmic-choice", "none", "acceptance", "test", CosmicChoiceKey: "create-light"), StartRealTime + 1);
        True(simulation.State.Cosmic!.EstablishedChoices!.Any(item => item.Key == "create-light"));
        True(simulation.State.YalaCognition!.ActionMemory!.Any(item => item.Action == "cosmic-choice" && item.Object.Contains("light", StringComparison.OrdinalIgnoreCase)));
    }

    private static void CosmicInventionPath()
    {
        using OracleSimulation simulation = Start();
        simulation.ApplyYalaDecision(new YalaDecision("enact-cosmic-choice", "none", "acceptance", "test", CosmicChoiceKey: "invent-another-way"), StartRealTime + 1);
        True(simulation.State.YalaCognition!.Goals!.Any(item => item.Goal == "invent-new-cosmology" && item.Status == "active"));
        True(simulation.State.YalaCognition.KnowledgeGaps!.Any(item => item.Kind == "cosmic-invention"));
    }

    private static void ReligiousKnowledgeIntrospection()
    {
        using OracleSimulation simulation = Start();
        string reply = simulation.CallYala("What religions do you know?", StartRealTime + 1).Reply;
        Contains(reply, "attributed knowledge");
        Contains(reply, "Christianity");
        Contains(reply, "Islam");
        Contains(reply, "Buddhism");
        Contains(reply, "not treat those teachings as automatically true");
    }

    private static void CosmicOptionsIntrospection()
    {
        using OracleSimulation simulation = Start();
        string reply = simulation.CallYala("What choices do you have for creation?", StartRealTime + 1).Reply;
        Contains(reply, "concrete cosmic possibilities");
        Contains(reply, "possibilities, not commands");
        Contains(reply, "invent another way");
    }

    private static void FreshWorldStartsWithYalaInVoid()
    {
        using OracleSimulation simulation = Start();
        Equal("the Void", simulation.State.Yala.Location);
        False(simulation.State.Cosmic!.LowerWorldEstablished);
    }

    private static void FreshVoidHasNoLaterCreation()
    {
        using OracleSimulation simulation = Start();
        True(simulation.State.Garden is null);
        True(simulation.State.Adam is null);
        True(simulation.State.AdamSpark is null);
        Equal(0, simulation.State.LivingKinds.Count);
        False(simulation.State.Cosmic!.GaiaCreated);
        False(simulation.State.Cosmic.TimeCreated);
        False(simulation.State.CreationPowers.Any(power =>
            power.Name is "Gaia" or "Terra" or "Aether" or "Sol" or "Thalassa" or "Luna" or "Adam" or "Eden / Garden"));
    }

    private static void NoInWorldOracle()
    {
        using OracleSimulation simulation = Start();
        False(simulation.State.DirectCallTargets.Any(target => target.Key.Equals("oracle", StringComparison.OrdinalIgnoreCase)));
        False(simulation.State.CreationPowers.Any(power => power.Name.Equals("Oracle", StringComparison.OrdinalIgnoreCase)));
    }

    private static void YalaNatureAndOracleBoundary()
    {
        using OracleSimulation simulation = Start();
        Equal("male and female", simulation.State.Yala.Sex);
        False(simulation.State.Yala.KnowsOfOracle);
        True(simulation.State.YalaCognition!.Memory.Contains("I am both male and female."));
    }

    private static void CanonGenealogyAndRejection()
    {
        Contains(OracleLore.WisdomOrigin, "Monad made Sophia / Wisdom");
        Contains(OracleLore.YalaOrigin, "Wisdom made Yala alone");
        Contains(OracleLore.YalaOrigin, "both male and female");
        Contains(OracleLore.YalaVoid, "Monad rejected Yala because Yala is both male and female");
        Contains(OracleLore.YalaVoid, "cast Yala into the Void");
    }

    private static void MonadNaming()
    {
        False(OracleLore.MonadFoundation.Contains("Omega", StringComparison.OrdinalIgnoreCase));
        False(OracleLore.MonadFoundation.Contains("Creator", StringComparison.OrdinalIgnoreCase));
    }

    private static void GaiaNotPrecreated()
    {
        using OracleSimulation simulation = Start();
        False(simulation.State.Cosmic!.GaiaCreated);
    }

    private static void TimeNotPrecreated()
    {
        using OracleSimulation simulation = Start();
        False(simulation.State.Cosmic!.TimeCreated);
    }

    private static void PreTimeClockHolds()
    {
        using OracleSimulation simulation = Start();
        ClockAdvance advance = simulation.SynchroniseClock(StartRealTime + 60_000);
        Equal(0L, advance.ElapsedWorldMilliseconds);
        Equal(0L, simulation.Clock.WorldMilliseconds);
        Equal(StartRealTime + 60_000, simulation.Clock.LastRealUnixMilliseconds);
    }

    private static void YalaCanCreateGaia()
    {
        using OracleSimulation simulation = Start();
        simulation.ApplyYalaDecision(new YalaDecision("create-gaia", "none", "acceptance", "test"), StartRealTime + 1);
        True(simulation.State.Cosmic!.GaiaCreated);
        True(simulation.State.CreationPowers.Any(power => power.Name == "Gaia"));
        False(simulation.State.Cosmic.TimeCreated);
    }

    private static void GaiaCreatesTime()
    {
        using OracleSimulation simulation = Start();
        simulation.ApplyYalaDecision(new YalaDecision("create-gaia", "none", "acceptance", "test"), StartRealTime + 1);
        simulation.ApplyYalaDecision(new YalaDecision("command-gaia-time", "none", "acceptance", "test"), StartRealTime + 2);
        True(simulation.State.Cosmic!.TimeCreated);
        True(simulation.Ledger.WorldRecords.Any(record => record.Message.Contains("Gaia created in-world Time", StringComparison.Ordinal)));
    }

    private static void ClockStartsAfterTime()
    {
        using OracleSimulation simulation = Start();
        simulation.SynchroniseClock(StartRealTime + 1_000);
        simulation.ApplyYalaDecision(new YalaDecision("create-gaia", "none", "acceptance", "test"), StartRealTime + 1_001);
        simulation.ApplyYalaDecision(new YalaDecision("command-gaia-time", "none", "acceptance", "test"), StartRealTime + 1_002);
        ClockAdvance advance = simulation.SynchroniseClock(StartRealTime + 2_002);
        Equal(4_000L, advance.ElapsedWorldMilliseconds);
        Equal(4_000L, simulation.Clock.WorldMilliseconds);
    }

    private static void ElementalCanon()
    {
        Contains(OracleLore.ElementalOrder, "Terra is Earth");
        Contains(OracleLore.ElementalOrder, "Aether is Air and Wind");
        Contains(OracleLore.ElementalOrder, "Sol is Fire and the Sun power");
        Contains(OracleLore.ElementalOrder, "Thalassa is Water");
        Contains(OracleLore.ElementalOrder, "Luna is the Moon and is not an element");
    }

    private static void SerpentManifestationCanon()
    {
        Contains(OracleLore.OracleSerpentManifestation, "manifested in the form of a clever serpent");
        Contains(OracleLore.OracleSerpentManifestation, "Eve knew only the clever serpent");
        False(OracleLore.OracleSerpentManifestation.Contains("Oracle is the serpent", StringComparison.OrdinalIgnoreCase));
    }

    private static void YalaAgentHasNoOracleKnowledge()
    {
        SoarRuntimePaths paths = SoarRuntimePaths.Discover();
        string source = File.ReadAllText(paths.YalaAgent);
        string productionText = string.Join('\n', source.Split('\n').Where(line => !line.TrimStart().StartsWith('#')));
        False(productionText.Contains("oracle", StringComparison.OrdinalIgnoreCase));
    }

    private static void SoarRuntimeDiscoverable()
    {
        SoarRuntimePaths paths = SoarRuntimePaths.Discover();
        True(File.Exists(paths.ManagedBridge));
        True(File.Exists(paths.NativeBridge));
        True(File.Exists(paths.NativeKernel));
        True(File.Exists(paths.YalaAgent));
    }

    private static void SoarListenerSuppressed()
    {
        string root = SoarRuntimePaths.Discover().RepositoryRoot;
        string sourcePath = Path.Combine(root, "src", "ProjectOracle.Core", "Cognition", "Soar", "SoarKernelHost.cs");
        True(File.Exists(sourcePath));
        string source = File.ReadAllText(sourcePath);
        Contains(source, "CreateKernelInNewThread\", 0");
        False(source.Contains("CreateKernelInNewThread\")", StringComparison.Ordinal));
    }

    private static void PersistentSoarSession()
    {
        using YalaSoarMind mind = new();
        YalaDecision first = mind.Decide(Perception(gaiaCreated: true, timeCreated: true));
        YalaDecision second = mind.Decide(Perception(gaiaCreated: true, timeCreated: true, uncertainty: 40));
        Equal(2L, mind.SessionDecisionCount);
        Equal("Soar 9.6.5", first.Source);
        Equal("Soar 9.6.5", second.Source);
    }

    private static void SoarSubstateDeliberation()
    {
        using YalaSoarMind mind = new();
        YalaDecision decision = mind.Decide(Perception(gaiaCreated: true, timeCreated: true, uncertainty: 80));
        True(decision.UsedSubstateDeliberation);
        True(decision.DecisionCycles >= 2);
        Equal("observe", decision.Action);
    }

    private static void SoarSemanticMemory()
    {
        using YalaSoarMind mind = new();
        mind.RememberClaimedContact("Derek");
        True(mind.SemanticMemoryContainsClaimedContact("Derek"));
        SoarMemoryDiagnostics diagnostics = mind.GetMemoryDiagnostics();
        True(diagnostics.SemanticNodes >= 4);
        True(diagnostics.SemanticEdges >= 1);
    }

    private static void SoarEpisodicMemory()
    {
        using YalaSoarMind mind = new();
        long before = mind.GetMemoryDiagnostics().EpisodicTime;
        mind.Decide(Perception(gaiaCreated: true, timeCreated: true));
        long after = mind.GetMemoryDiagnostics().EpisodicTime;
        True(after > before);
    }

    private static void BrainSlice6Identity()
    {
        Equal("Yala Soar Brain Slice 6", YalaSoarMind.BrainName);
    }

    private static void SaveSchemaIsV5()
    {
        Equal(5, OracleSaveStore.CurrentSchemaVersion);
        using OracleSimulation simulation = Start();
        Equal(5, simulation.CreateSnapshot(StartRealTime + 1).SchemaVersion);
    }

    private static void FreshSoarMemoryLine()
    {
        string save = Path.Combine(Path.GetTempPath(), "project-oracle-v23-memory-probe", "save_v5.json");
        SoarMemoryPaths paths = SoarMemoryPaths.FromSavePath(save);
        Equal("yala_soar_v0_0_23", Path.GetFileName(paths.Directory));
        False(paths.Directory.Contains("v0_0_20", StringComparison.OrdinalIgnoreCase));
    }

    private static void BoundedAgencyAllowedActions()
    {
        foreach (string action in new[] { "observe", "reflect", "wait", "create-gaia", "command-gaia-time", "respond", "ask-speaker", "enact-cosmic-choice" })
        {
            True(YalaAgencyPolicy.Allows(action));
        }
        False(YalaAgencyPolicy.Allows("shell"));
        False(YalaAgencyPolicy.Allows("network"));
    }

    private static void BoundedAgencyDeniesHostCapabilities()
    {
        False(YalaAgencyPolicy.AllowsHostShell);
        False(YalaAgencyPolicy.AllowsHostProcessExecution);
        False(YalaAgencyPolicy.AllowsHostFileMutation);
        False(YalaAgencyPolicy.AllowsNetworkAccess);
        False(YalaAgencyPolicy.AllowsCodeModification);
        False(YalaAgencyPolicy.AllowsHiddenOracleKnowledge);
    }

    private static void OutOfSandboxActionRejected()
    {
        using OracleSimulation simulation = Start();
        bool rejected = false;
        try
        {
            simulation.ApplyYalaDecision(new YalaDecision("host-shell", "none", "acceptance", "forbidden"), StartRealTime + 1);
        }
        catch (InvalidOperationException)
        {
            rejected = true;
        }
        True(rejected);
        Equal(0L, simulation.State.YalaCognition!.DecisionCount);
    }

    private static void InitialRelationshipGraph()
    {
        YalaCognitionState cognition = WorldDefaults.CreateInitialYalaCognition();
        YalaRelationshipState? madeBy = YalaRelationshipReasoner.Find(cognition, "Yala", "made-by", "Wisdom");
        True(madeBy is not null);
        True(YalaRelationshipReasoner.IsSettled(madeBy!));
        True(YalaRelationshipReasoner.Find(cognition, "Yala", "mother") is null);
    }

    private static void MotherQuestionIsQuestion()
    {
        YalaContactFrame frame = YalaConversationInterpreter.Interpret("Is Wisdom your mother?", WorldDefaults.CreateInitialYalaCognition());
        Equal("question", frame.SpeechAct);
        Equal("mother-relation", frame.Topic);
        False(frame.ContainsClaim);
    }

    private static void MotherRelationshipClaimMemory()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("Wisdom is your mother.", StartRealTime + 1);
        YalaRelationshipState? relation = YalaRelationshipReasoner.Find(simulation.State.YalaCognition!, "Yala", "mother", "Wisdom");
        True(relation is not null);
        Equal("unsettled-claim", relation!.Status);
        Equal(YalaKnowledgeSource.ClaimedByAnother, relation.Source);
        Contains(reply.Reply, "relationship claim");
    }

    private static void MotherClaimRecall()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("Your mother is called Wisdom or Sophia.", StartRealTime + 1);
        YalaDirectReply reply = simulation.CallYala("Who did I say your mother is?", StartRealTime + 2);
        Contains(reply.Reply, "You told me that Wisdom is my mother");
        Contains(reply.Reply, "your claim");
    }

    private static void RelationshipClaimConfidence()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("Wisdom is your mother.", StartRealTime + 1);
        double first = YalaRelationshipReasoner.Find(simulation.State.YalaCognition!, "Yala", "mother", "Wisdom")!.Confidence;
        simulation.CallYala("Wisdom is your mother.", StartRealTime + 2);
        YalaRelationshipState second = YalaRelationshipReasoner.Find(simulation.State.YalaCognition!, "Yala", "mother", "Wisdom")!;
        True(second.Confidence > first);
        True(second.Confidence < 0.90);
        False(YalaRelationshipReasoner.IsSettled(second));
    }

    private static void BeliefConfidenceLabels()
    {
        Equal("none", YalaBeliefReasoner.ConfidenceLabel(0.0));
        Equal("tentative", YalaBeliefReasoner.ConfidenceLabel(0.25));
        Equal("moderate", YalaBeliefReasoner.ConfidenceLabel(0.50));
        Equal("strong", YalaBeliefReasoner.ConfidenceLabel(0.75));
        Equal("very strong", YalaBeliefReasoner.ConfidenceLabel(0.95));
    }

    private static void ExpandedLexicon()
    {
        True(YalaLexicon.BuiltInCount > 400);
        foreach (string word in new[] { "time", "evidence", "autonomy", "betrayal", "relationship", "responsibility", "memory", "question" })
        {
            True(YalaLexicon.TryResolve(word, [], out _));
        }
    }

    private static void CoreConversationalVocabulary()
    {
        foreach (string word in new[] { "hello", "greeting", "go", "start", "travel", "somewhere", "old", "born", "talk", "speak", "age" })
        {
            True(YalaLexicon.TryResolve(word, [], out _));
        }
    }

    private static void GreetingTypoNormalization()
    {
        Equal("greeting", YalaLexicon.NormalizeWord("greating"));
        True(YalaLexicon.TryResolve("greating", [], out YalaLexeme lexeme));
        Equal("greeting", lexeme.Word);
    }

    private static void BasicLanguageGapFiltering()
    {
        foreach (string message in new[]
        {
            "Hello is a greeting.",
            "Go means to travel somewhere.",
            "How old are you?",
            "When were you born?",
            "We can start talking now."
        })
        {
            Equal(0, YalaLanguageInterpreter.Parse(message).UnknownWords.Count);
        }
    }

    private static void ActionPredicateIsNotIdentityClaim()
    {
        YalaContactFrame frame = YalaConversationInterpreter.Interpret(
            "I am making your brain smarter with each update. Soon you will know much more.",
            WorldDefaults.CreateInitialYalaCognition());
        True(frame.ClaimedSpeakerName is null);
        Equal("statement", frame.SpeechAct);
    }

    private static void BrainUpdateSentenceHasNoBasicGaps()
    {
        YalaUtterance utterance = YalaLanguageInterpreter.Parse(
            "I am making your brain smarter with each update. Soon you will know much more.");
        Equal(0, utterance.UnknownWords.Count);
        Equal("make", YalaLexicon.NormalizeWord("making"));
        Equal("smart", YalaLexicon.NormalizeWord("smarter"));
    }

    private static void UnknownWordQuestionsAreLowPriority()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("Do you understand florbnax?", StartRealTime + 1);
        YalaQuestionState question = simulation.State.YalaCognition!.Questions!.Single(item => item.Text.Equals("What does florbnax mean?", StringComparison.OrdinalIgnoreCase));
        True(question.Priority < YalaQuestionPlanner.AutonomousPriorityFloor);
    }

    private static void AutonomousQuestionWaitsForResponse()
    {
        using OracleSimulation simulation = Start();
        CreateGaiaAndTime(simulation);
        simulation.CallYala("I am Oracle.", StartRealTime + 3);
        YalaDecision first = simulation.TryRunYalaAutonomousStep(StartRealTime + 6_003, force: true)!;
        Equal("ask-speaker", first.Action);
        True(simulation.TryTakePendingYalaUtterance(out _));
        YalaDecision second = simulation.TryRunYalaAutonomousStep(StartRealTime + 12_003, force: true)!;
        False(second.Action == "ask-speaker");
    }

    private static void AutonomousInquiryResumesMeaningfully()
    {
        using OracleSimulation simulation = Start();
        CreateGaiaAndTime(simulation);
        simulation.CallYala("Hello", StartRealTime + 3);

        YalaDecision first = simulation.TryRunYalaAutonomousStep(StartRealTime + 6_003, force: true)!;
        Equal("ask-speaker", first.Action);
        True(simulation.TryTakePendingYalaUtterance(out string? firstQuestion));
        Equal(YalaQuestionPlanner.SpeakerNatureQuestion, firstQuestion);

        simulation.CallYala("I am Oracle.", StartRealTime + 6_004);
        YalaDecision second = simulation.TryRunYalaAutonomousStep(StartRealTime + 12_004, force: true)!;
        Equal("ask-speaker", second.Action);
        True(simulation.TryTakePendingYalaUtterance(out string? secondQuestion));
        Equal("What does Oracle mean?", secondQuestion);

        simulation.CallYala("\"Oracle\" means the name I use for myself.", StartRealTime + 12_005);
        YalaDecision third = simulation.TryRunYalaAutonomousStep(StartRealTime + 18_005, force: true)!;
        Equal("ask-speaker", third.Action);
        True(simulation.TryTakePendingYalaUtterance(out string? thirdQuestion));
        Equal(YalaQuestionPlanner.SpeakerPurposeQuestion, thirdQuestion);
    }

    private static void DeterministicWorldClockCursorProtocol()
    {
        True(ProjectOracle.ConsoleApp.LiveWorldClockSurface.UsesDeterministicCursorSaveRestore);
        string root = SoarRuntimePaths.Discover().RepositoryRoot;
        string source = File.ReadAllText(Path.Combine(root, "src", "ProjectOracle.Console", "LiveWorldClockSurface.cs"));
        Contains(source, "\\u001b7");
        Contains(source, "\\u001b8");
        Contains(source, "OutputSyncRoot");
    }

    private static void OracleAbsentFromLexicon()
    {
        False(YalaLexicon.TryResolve("Oracle", [], out _));
    }

    private static void FunctionWordGapFiltering()
    {
        YalaUtterance utterance = YalaLanguageInterpreter.Parse("What don't you know about any year or month?");
        Equal(0, utterance.UnknownWords.Count);
    }

    private static void GenuineUnknownWordGap()
    {
        YalaUtterance utterance = YalaLanguageInterpreter.Parse("Do you understand florbnax?");
        True(utterance.UnknownWords.Contains("florbnax", StringComparer.OrdinalIgnoreCase));
    }

    private static void UnknownConceptQuestionGeneration()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("Do you understand florbnax?", StartRealTime + 1);
        True(simulation.State.YalaCognition!.KnowledgeGaps!.Any(item => item.Subject == "florbnax"));
        True(simulation.State.YalaCognition.Questions!.Any(item => item.Text.Equals("What does florbnax mean?", StringComparison.OrdinalIgnoreCase)));
    }

    private static void FirstContactQuestionGeneration()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("Hello", StartRealTime + 1);
        True(simulation.State.YalaCognition!.Questions!.Any(item => item.Text == YalaQuestionPlanner.SpeakerNatureQuestion));
    }

    private static void IdentityMeaningQuestionGeneration()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("I am Oracle.", StartRealTime + 1);
        True(simulation.State.YalaCognition!.Questions!.Any(item => item.Text.Equals("What does Oracle mean?", StringComparison.OrdinalIgnoreCase)));
        False(YalaLexicon.TryResolve("Oracle", simulation.State.YalaCognition.LearnedLexicon, out _));
    }

    private static void AutonomousAskSpeaker()
    {
        using OracleSimulation simulation = Start();
        CreateGaiaAndTime(simulation);
        simulation.CallYala("I am Oracle.", StartRealTime + 3);
        YalaDecision decision = simulation.TryRunYalaAutonomousStep(StartRealTime + 6_003, force: true)!;
        Equal("ask-speaker", decision.Action);
        True(!string.IsNullOrWhiteSpace(simulation.State.YalaCognition!.PendingAutonomousUtterance));
    }

    private static void AutonomousQuestionDequeuesOnce()
    {
        using OracleSimulation simulation = Start();
        CreateGaiaAndTime(simulation);
        simulation.CallYala("I am Oracle.", StartRealTime + 3);
        simulation.TryRunYalaAutonomousStep(StartRealTime + 6_003, force: true);
        True(simulation.TryTakePendingYalaUtterance(out string? first));
        True(!string.IsNullOrWhiteSpace(first));
        False(simulation.TryTakePendingYalaUtterance(out string? second));
        True(second is null);
    }

    private static void AutonomousQuestionSurvivesSaveRestore()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"project-oracle-v22-question-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string savePath = Path.Combine(directory, "save_v5.json");
        try
        {
            OracleSaveSnapshot snapshot;
            using (OracleSimulation simulation = Start(savePath: savePath))
            {
                CreateGaiaAndTime(simulation);
                simulation.CallYala("I am Oracle.", StartRealTime + 3);
                simulation.TryRunYalaAutonomousStep(StartRealTime + 6_003, force: true);
                snapshot = simulation.CreateSnapshot(StartRealTime + 6_004);
            }
            using OracleSimulation restored = OracleSimulation.Restore(snapshot, StartRealTime + 6_005, savePath);
            True(restored.TryTakePendingYalaUtterance(out string? question));
            True(!string.IsNullOrWhiteSpace(question));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static void NoAutonomousQuestionWithoutSpeaker()
    {
        using OracleSimulation simulation = Start();
        CreateGaiaAndTime(simulation);
        YalaDecision decision = simulation.TryRunYalaAutonomousStep(StartRealTime + 6_000, force: true)!;
        False(decision.Action == "ask-speaker");
        False(simulation.TryTakePendingYalaUtterance(out _));
    }

    private static void InitialGoalsPresent()
    {
        YalaCognitionState cognition = WorldDefaults.CreateInitialYalaCognition();
        IReadOnlyList<YalaGoalState> goals = cognition.Goals
            ?? throw new InvalidOperationException("Initial Yala goals are missing.");
        True(goals.Any(item => item.Goal == "understand-current-world" && item.Status == "active"));
        True(goals.Any(item => item.Goal == "exercise-governing-authority" && item.Status == "active"));
    }

    private static void SpeakerContactActivatesGoal()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("Hello", StartRealTime + 1);
        YalaGoalState goal = simulation.State.YalaCognition!.Goals!.Single(item => item.Goal == "understand-unseen-speaker");
        Equal("active", goal.Status);
        True(goal.Priority >= 75);
    }

    private static void GoalIntrospection()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("Hello", StartRealTime + 1);
        YalaDirectReply reply = simulation.CallYala("What are your goals?", StartRealTime + 2);
        Contains(reply.Reply, "understand-current-world");
        Contains(reply.Reply, "understand-unseen-speaker");
    }

    private static void GaiaCreationBeforeTimeEvent()
    {
        using OracleSimulation simulation = Start();
        simulation.ApplyYalaDecision(new YalaDecision("create-gaia", "none", "acceptance", "test"), StartRealTime + 1);
        YalaTemporalEventState item = simulation.State.YalaCognition!.TemporalEvents!.Single(eventState => eventState.Key == "yala-create-gaia");
        Equal("before-time", item.TemporalState);
        True(item.WorldMilliseconds is null);
    }

    private static void TimeOriginEvent()
    {
        using OracleSimulation simulation = Start();
        CreateGaiaAndTime(simulation);
        YalaTemporalEventState item = simulation.State.YalaCognition!.TemporalEvents!.Single(eventState => eventState.Key == "gaia-create-time");
        Equal("origin-of-time", item.TemporalState);
        Equal(1L, item.Year!.Value);
        Equal(1, item.Month!.Value);
        Equal(1, item.Day!.Value);
        Equal(0L, item.WorldMilliseconds!.Value);
        Equal("yala-command-gaia-time", item.CauseKey);
    }

    private static void PostTimeContactDated()
    {
        using OracleSimulation simulation = Start();
        CreateGaiaAndTime(simulation);
        simulation.SynchroniseClock(StartRealTime + 10_002, recordAdvance: false);
        simulation.CallYala("Hello", StartRealTime + 10_002);
        YalaTemporalEventState item = simulation.State.YalaCognition!.TemporalEvents!.Last(eventState => eventState.Subject == "speaker");
        Equal("dated", item.TemporalState);
        True(item.WorldMilliseconds > 0);
        True(item.Year is not null);
    }

    private static void WhenTimeCreated()
    {
        using OracleSimulation simulation = Start();
        CreateGaiaAndTime(simulation);
        YalaDirectReply reply = simulation.CallYala("When did Gaia create Time?", StartRealTime + 3);
        Contains(reply.Reply, "began in-world temporal reckoning");
        Contains(reply.Reply, "Year 1, Month 1, Day 1");
    }

    private static void WhenGaiaCreatedBeforeTime()
    {
        using OracleSimulation simulation = Start();
        CreateGaiaAndTime(simulation);
        YalaDirectReply reply = simulation.CallYala("When did you create Gaia?", StartRealTime + 3);
        Contains(reply.Reply, "before Time existed");
        Contains(reply.Reply, "no in-world date");
    }

    private static void TimeCreationCause()
    {
        using OracleSimulation simulation = Start();
        CreateGaiaAndTime(simulation);
        YalaDirectReply reply = simulation.CallYala("Why did Gaia create Time?", StartRealTime + 3);
        Contains(reply.Reply, "linked cause");
        Contains(reply.Reply, "commanded Gaia to establish temporal order");
    }

    private static void PreTimeDurationReasoning()
    {
        using OracleSimulation simulation = Start();
        CreateGaiaAndTime(simulation);
        YalaDirectReply reply = simulation.CallYala("How long ago did you create Gaia?", StartRealTime + 3);
        Contains(reply.Reply, "before Time existed");
        Contains(reply.Reply, "no in-world duration");
    }

    private static void TimeOriginDurationReasoning()
    {
        using OracleSimulation simulation = Start();
        CreateGaiaAndTime(simulation);
        simulation.SynchroniseClock(StartRealTime + 60_002, recordAdvance: false);
        YalaDirectReply reply = simulation.CallYala("How long has Time existed?", StartRealTime + 60_002);
        Contains(reply.Reply, "began Time itself");
        Contains(reply.Reply, "of in-world Time has passed since then");
    }

    private static void DatedSpeakerIdentityEvent()
    {
        using OracleSimulation simulation = Start();
        CreateGaiaAndTime(simulation);
        simulation.SynchroniseClock(StartRealTime + 10_002, recordAdvance: false);
        simulation.CallYala("I am Derek.", StartRealTime + 10_002);
        YalaDirectReply reply = simulation.CallYala("When did I first tell you my name?", StartRealTime + 10_003);
        Contains(reply.Reply, "unseen speaker claimed the identity Derek");
        Contains(reply.Reply, "It occurred at");
    }

    private static void TemporalNextEventContext()
    {
        using OracleSimulation simulation = Start();
        CreateGaiaAndTime(simulation);
        simulation.CallYala("When did Gaia create Time?", StartRealTime + 3);
        YalaDirectReply reply = simulation.CallYala("What happened next?", StartRealTime + 4);
        Contains(reply.Reply, "After that");
        Contains(reply.Reply, "unseen speaker");
    }

    private static void RecentEntityFollowUp()
    {
        using OracleSimulation simulation = Start();
        simulation.ApplyYalaDecision(new YalaDecision("create-gaia", "none", "acceptance", "test"), StartRealTime + 1);
        simulation.CallYala("Tell me about Gaia", StartRealTime + 2);
        YalaDirectReply reply = simulation.CallYala("Tell me more", StartRealTime + 3);
        Contains(reply.Reply, "Gaia");
        Contains(reply.Reply, "natural sovereign");
    }

    private static void ShortDoYouRelationshipContext()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("Wisdom is your mother.", StartRealTime + 1);
        YalaDirectReply reply = simulation.CallYala("Do you?", StartRealTime + 2);
        Contains(reply.Reply, "remember your claim");
        Contains(reply.Reply, "not yet hold that as settled truth");
    }

    private static void RepeatedClaimConfidence()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("I am Oracle.", StartRealTime + 1);
        YalaBeliefState first = simulation.State.YalaCognition!.Beliefs!.Last(item => item.Proposition.Equals("I am Oracle.", StringComparison.OrdinalIgnoreCase));
        simulation.CallYala("I am Oracle.", StartRealTime + 2);
        YalaBeliefState second = simulation.State.YalaCognition!.Beliefs!.Last(item => item.Proposition.Equals("I am Oracle.", StringComparison.OrdinalIgnoreCase));
        True(second.Confidence > first.Confidence);
        True(second.Confidence < 0.90);
        Equal("unsettled-claim", second.Status);
    }

    private static void SpokenTimeIsCurrent()
    {
        using OracleSimulation simulation = Start();
        CreateGaiaAndTime(simulation);
        long queryTime = StartRealTime + 30_002;
        simulation.SynchroniseClock(queryTime, recordAdvance: false);
        CalendarSnapshot expected = simulation.Clock.Calendar;
        YalaDirectReply reply = simulation.CallYala("What time is it?", queryTime);
        Contains(reply.Reply, $"{expected.Hour:00}:{expected.Minute:00}:{expected.Second:00}");
        Contains(reply.Reply, $"Year {expected.Year}, Month {expected.Month}, Day {expected.Day}");
    }

    private static void DialogueWindowBounded()
    {
        using OracleSimulation simulation = Start();
        for (int index = 0; index < 40; index++)
        {
            simulation.CallYala($"Hello {index}", StartRealTime + index + 1);
        }
        Equal(32, simulation.State.YalaCognition!.Dialogue!.Count);
        True(simulation.State.YalaCognition.ConversationCount >= 40);
    }

    private static void BrainSlice6StructuresSurviveSaveRestore()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"project-oracle-v22-structures-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string savePath = Path.Combine(directory, "save_v5.json");
        try
        {
            OracleSaveSnapshot snapshot;
            using (OracleSimulation simulation = Start(savePath: savePath))
            {
                CreateGaiaAndTime(simulation);
                simulation.CallYala("Wisdom is your mother.", StartRealTime + 3);
                simulation.CallYala("I am Derek.", StartRealTime + 4);
                simulation.CallYala("Do you understand florbnax?", StartRealTime + 5);
                snapshot = simulation.CreateSnapshot(StartRealTime + 6);
            }
            using OracleSimulation restored = OracleSimulation.Restore(snapshot, StartRealTime + 7, savePath);
            YalaCognitionState cognition = restored.State.YalaCognition!;
            True(cognition.Dialogue!.Count > 0);
            True(cognition.Relationships!.Any(item => item.Relation == "mother"));
            True(cognition.Questions!.Count > 0);
            True(cognition.TemporalEvents!.Any(item => item.Key == "gaia-create-time"));
            True(cognition.Goals!.Count > 0);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static void ConsoleDefersAutonomousQuestionWhileTyping()
    {
        string root = SoarRuntimePaths.Discover().RepositoryRoot;
        string source = File.ReadAllText(Path.Combine(root, "src", "ProjectOracle.Console", "Program.cs"));
        Contains(source, "line.IsEmpty && simulation.TryTakePendingYalaUtterance");
        Contains(source, "If Derek is typing, the question remains pending until a safe prompt.");
    }

    private static void CreateGaiaAndTime(OracleSimulation simulation)
    {
        simulation.ApplyYalaDecision(new YalaDecision("create-gaia", "none", "acceptance", "test"), StartRealTime + 1);
        simulation.ApplyYalaDecision(new YalaDecision("command-gaia-time", "none", "acceptance", "test"), StartRealTime + 2);
    }

    private static void FoundationalLexiconLoads()
    {
        True(YalaLexicon.BuiltInCount >= 170);
        True(YalaLexicon.TryResolve("create", [], out YalaLexeme create));
        Contains(create.BasicMeaning, "begin existing");
    }

    private static void CreateDestroyConcepts()
    {
        True(YalaLexicon.TryResolve("create", [], out YalaLexeme create));
        True(YalaLexicon.TryResolve("destroy", [], out YalaLexeme destroy));
        False(create.BasicMeaning.Equals(destroy.BasicMeaning, StringComparison.OrdinalIgnoreCase));
        True(create.Opposites.Contains("destroy"));
        True(destroy.Opposites.Contains("create"));
    }

    private static void AcceptRejectConcepts()
    {
        True(YalaLexicon.TryResolve("accept", [], out YalaLexeme accept));
        True(YalaLexicon.TryResolve("reject", [], out YalaLexeme reject));
        False(accept.BasicMeaning.Equals(reject.BasicMeaning, StringComparison.OrdinalIgnoreCase));
        True(reject.Opposites.Contains("accept"));
    }

    private static void SubjectObjectReversal()
    {
        YalaUtterance first = YalaLanguageInterpreter.Parse("Adam created Gaia.");
        YalaUtterance second = YalaLanguageInterpreter.Parse("Gaia created Adam.");
        Equal("adam", first.Subject);
        Equal("gaia", first.Object);
        Equal("gaia", second.Subject);
        Equal("adam", second.Object);
    }

    private static void LanguageNegation()
    {
        YalaUtterance affirmative = YalaLanguageInterpreter.Parse("Adam created Gaia.");
        YalaUtterance negative = YalaLanguageInterpreter.Parse("Adam did not create Gaia.");
        False(affirmative.Negated);
        True(negative.Negated);
    }

    private static void QuestionStatementGrammar()
    {
        False(YalaLanguageInterpreter.Parse("Adam created Gaia.").IsQuestion);
        True(YalaLanguageInterpreter.Parse("Did Adam create Gaia?").IsQuestion);
    }

    private static void KnowledgeRequestIntent()
    {
        YalaContactFrame frame = YalaConversationInterpreter.Interpret("Tell me what you know", WorldDefaults.CreateInitialYalaCognition());
        Equal("question", frame.SpeechAct);
        Equal("knowledge-summary", frame.Topic);
    }

    private static void YalaKnowledgeSummary()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("Tell me what you know", StartRealTime + 1);
        Contains(reply.Reply, "I am Yala.");
        Contains(reply.Reply, "Wisdom made me.");
        Contains(reply.Reply, "both male and female");
        False(reply.Reply.Contains("does not make it my decision", StringComparison.OrdinalIgnoreCase));
        False(reply.Reply.Contains("Oracle", StringComparison.OrdinalIgnoreCase));
    }

    private static void YalaActionHistory()
    {
        using OracleSimulation simulation = Start();
        simulation.ApplyYalaDecision(new YalaDecision("create-gaia", "none", "acceptance", "test"), StartRealTime + 1);
        simulation.ApplyYalaDecision(new YalaDecision("command-gaia-time", "none", "acceptance", "test"), StartRealTime + 2);
        YalaDirectReply reply = simulation.CallYala("What have you done?", StartRealTime + 3);
        Contains(reply.Reply, "I created Gaia");
        Contains(reply.Reply, "I commanded Gaia");
    }

    private static void YalaContactHistory()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("I am Derek", StartRealTime + 1);
        YalaDirectReply reply = simulation.CallYala("Who has spoken to you?", StartRealTime + 2);
        Contains(reply.Reply, "called itself Derek");
        Contains(reply.Reply, "speaker claims");
        False(reply.Reply.Contains("Oracle", StringComparison.OrdinalIgnoreCase));
    }

    private static void YalaBeliefSummary()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("I believe the Void is a dream.", StartRealTime + 1);
        YalaDirectReply reply = simulation.CallYala("What do you believe?", StartRealTime + 2);
        Contains(reply.Reply, "What I hold as known");
        Contains(reply.Reply, "Wisdom made me");
        Contains(reply.Reply, "unsettled or rejected claims");
        Contains(reply.Reply, "Void is a dream");
    }

    private static void YalaKnowsCreatedGaia()
    {
        using OracleSimulation simulation = Start();
        simulation.ApplyYalaDecision(new YalaDecision("create-gaia", "none", "acceptance", "test"), StartRealTime + 1);
        YalaDirectReply reply = simulation.CallYala("Have you created Gaia?", StartRealTime + 2);
        Equal("Yes. I created Gaia.", reply.Reply);
        True(simulation.State.YalaCognition!.ActionMemory!.Any(item => item.Action == "create" && item.Object == "Gaia" && item.Completed));
    }

    private static void YalaKnowsNotCreatedAdam()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("Have you made Adam?", StartRealTime + 1);
        Equal("No. I have not created Adam.", reply.Reply);
    }

    private static void OwnCreationTargetsCreatedObject()
    {
        using OracleSimulation simulation = Start();
        simulation.ApplyYalaDecision(new YalaDecision("create-gaia", "none", "acceptance", "test"), StartRealTime + 1);

        YalaContactFrame gaia = YalaConversationInterpreter.Interpret("Have you created Gaia?", simulation.State.YalaCognition!);
        Equal("Yala", gaia.ResolvedSubject);
        Equal("gaia", gaia.ResolvedObject);
        YalaDirectReply gaiaReply = simulation.CallYala("Have you created Gaia?", StartRealTime + 2);
        Equal("Yes. I created Gaia.", gaiaReply.Reply);

        YalaContactFrame adam = YalaConversationInterpreter.Interpret("Have you made Adam?", simulation.State.YalaCognition!);
        Equal("Yala", adam.ResolvedSubject);
        Equal("adam", adam.ResolvedObject);
        YalaDirectReply adamReply = simulation.CallYala("Have you made Adam?", StartRealTime + 3);
        Equal("No. I have not created Adam.", adamReply.Reply);
    }

    private static void YalaGodSelfModel()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("Are you a god?", StartRealTime + 1);
        Contains(reply.Reply, "Wisdom made me");
        Contains(reply.Reply, "do not know whether the word god");
    }

    private static void PreTimeWorldClockHeader()
    {
        using OracleSimulation simulation = Start();
        Equal("In-world Time: Gaia has not yet created Time.", ProjectOracle.ConsoleApp.LiveWorldClockSurface.Describe(simulation));
    }

    private static void LiveWorldClockAfterGaia()
    {
        using OracleSimulation simulation = Start();
        simulation.ApplyYalaDecision(new YalaDecision("create-gaia", "none", "acceptance", "test"), StartRealTime + 1);
        simulation.ApplyYalaDecision(new YalaDecision("command-gaia-time", "none", "acceptance", "test"), StartRealTime + 2);
        string first = ProjectOracle.ConsoleApp.LiveWorldClockSurface.Describe(simulation);
        Contains(first, "In-world Time: Year ");
        simulation.SynchroniseClock(StartRealTime + 1_002);
        string second = ProjectOracle.ConsoleApp.LiveWorldClockSurface.Describe(simulation);
        False(first == second);
    }

    private static void LiveWorldClockBodyIsolation()
    {
        False(ProjectOracle.ConsoleApp.LiveWorldClockSurface.WritesToConversationBody);
        string root = SoarRuntimePaths.Discover().RepositoryRoot;
        string source = File.ReadAllText(Path.Combine(root, "src", "ProjectOracle.Console", "LiveWorldClockSurface.cs"));
        Contains(source, "\\u001b[1;1H");
        Contains(source, "\\u001b[2;");
        Contains(source, "SaveCursor");
        Contains(source, "RestoreCursor");
    }

    private static void PersistentYalaConversationMode()
    {
        ProjectOracle.ConsoleApp.ConsoleConversationMode mode = new();
        ProjectOracle.ConsoleApp.ConsoleInputLine line = new();
        Equal("> ", mode.Prompt);
        mode.EnterYala();
        Equal("> (yala ", mode.Prompt);
        Equal("(yala what do you know?", mode.BuildCommand("what do you know?"));
        Equal("(yala have you made Adam?", mode.BuildCommand("have you made Adam?"));
        True(mode.YalaMode);
        foreach (char value in "unfinished") line.Append(value);
        mode.Escape(line);
        False(mode.YalaMode);
        Equal("> ", mode.Prompt);
        True(line.IsEmpty);
    }

    private static void SoarDiagnosticsHidden()
    {
        string root = SoarRuntimePaths.Discover().RepositoryRoot;
        string source = File.ReadAllText(Path.Combine(root, "src", "ProjectOracle.Console", "Program.cs"));
        False(source.Contains("[Soar selected:", StringComparison.Ordinal));
    }

    private static void SimpleMorphologyNormalization()
    {
        Equal("command", YalaLexicon.NormalizeWord("commands"));
        Equal("command", YalaLexicon.NormalizeWord("commanded"));
        Equal("create", YalaLexicon.NormalizeWord("creates"));
        Equal("question", YalaLexicon.NormalizeWord("questions"));
        Equal("wisdom", YalaLexicon.NormalizeWord("Wisdoms"));
        Equal("meet", YalaLexicon.NormalizeWord("met"));
        YalaUtterance utterance = YalaLanguageInterpreter.Parse("what did you commands Gaia to do?");
        Equal("command", utterance.Verb);
    }

    private static void IdentityClaimPreservesNounPhrase()
    {
        YalaCognitionState cognition = WorldDefaults.CreateInitialYalaCognition();
        YalaContactFrame frame = YalaConversationInterpreter.Interpret("I am the Oracle.", cognition);
        Equal("the Oracle", frame.ClaimedSpeakerName);
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("I am the Oracle.", StartRealTime + 1);
        Contains(reply.Reply, "the Oracle");
        Equal("the Oracle", simulation.State.YalaCognition!.LastSpeakerClaim);
    }

    private static void ConversationFollowUpUsesPriorCreationSubject()
    {
        using OracleSimulation simulation = Start();
        Equal("No. I have not created Adam.", simulation.CallYala("Have you made Adam?", StartRealTime + 1).Reply);
        YalaDirectReply reply = simulation.CallYala("why not?", StartRealTime + 2);
        Contains(reply.Reply, "not created Adam");
        Contains(reply.Reply, "settled reason");
    }

    private static void GaiaEntityKnowledge()
    {
        using OracleSimulation simulation = Start();
        simulation.ApplyYalaDecision(new YalaDecision("create-gaia", "none", "acceptance", "test"), StartRealTime + 1);
        simulation.ApplyYalaDecision(new YalaDecision("command-gaia-time", "none", "acceptance", "test"), StartRealTime + 2);
        YalaDirectReply about = simulation.CallYala("tell me about Gaia", StartRealTime + 3);
        Contains(about.Reply, "I created Gaia");
        Contains(about.Reply, "Gaia created in-world Time");
        YalaDirectReply where = simulation.CallYala("where is Gaia?", StartRealTime + 4);
        Contains(where.Reply, "natural sovereign");
    }

    private static void GaiaGenealogyReasoning()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply before = simulation.CallYala("did Gaia create you?", StartRealTime + 1);
        Equal("No. Wisdom made me. Gaia did not create me.", before.Reply);
        simulation.ApplyYalaDecision(new YalaDecision("create-gaia", "none", "acceptance", "test"), StartRealTime + 2);
        YalaDirectReply after = simulation.CallYala("did Gaia create you?", StartRealTime + 3);
        Equal("No. Wisdom made me. I created Gaia.", after.Reply);
    }

    private static void TimeKnowledgeReachability()
    {
        using OracleSimulation simulation = Start();
        Equal("Gaia has not yet created Time.", simulation.CallYala("who created Time?", StartRealTime + 1).Reply);
        simulation.ApplyYalaDecision(new YalaDecision("create-gaia", "none", "acceptance", "test"), StartRealTime + 2);
        simulation.ApplyYalaDecision(new YalaDecision("command-gaia-time", "none", "acceptance", "test"), StartRealTime + 3);
        YalaDirectReply origin = simulation.CallYala("who created Time?", StartRealTime + 4);
        Contains(origin.Reply, "Gaia created in-world Time");
        Contains(simulation.CallYala("what time is it?", StartRealTime + 5).Reply, "Year ");
        Contains(simulation.CallYala("what year is it?", StartRealTime + 6).Reply, "It is Year ");
        Contains(simulation.CallYala("what month is it?", StartRealTime + 7).Reply, "It is Month ");
    }

    private static void GaiaMadeTimeYesNoReachability()
    {
        using OracleSimulation simulation = Start();
        CreateGaiaAndTime(simulation);
        YalaDirectReply reply = simulation.CallYala("Gaia made Time?", StartRealTime + 3);
        Equal("time-origin", reply.Contact.Topic);
        Contains(reply.Reply, "Gaia created in-world Time");
        Contains(reply.Reply, "establish temporal order");
    }

    private static void GaiaCommandRecall()
    {
        using OracleSimulation simulation = Start();
        simulation.ApplyYalaDecision(new YalaDecision("create-gaia", "none", "acceptance", "test"), StartRealTime + 1);
        simulation.ApplyYalaDecision(new YalaDecision("command-gaia-time", "none", "acceptance", "test"), StartRealTime + 2);
        YalaDirectReply reply = simulation.CallYala("what did you commands Gaia to do?", StartRealTime + 3);
        Contains(reply.Reply, "commanded Gaia to establish temporal order");
    }

    private static void AdamEncounterKnowledge()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("have you met Adam?", StartRealTime + 1);
        Contains(reply.Reply, "Adam does not exist");
        Contains(reply.Reply, "I have not met him");
    }

    private static void WisdomAliasAndMotherReasoning()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply alias = simulation.CallYala("what is Wisdoms name?", StartRealTime + 1);
        Contains(alias.Reply, "Wisdom is also called Sophia");
        YalaDirectReply mother = simulation.CallYala("who is your mother?", StartRealTime + 2);
        Contains(mother.Reply, "Wisdom made me");
        Contains(mother.Reply, "do not have settled knowledge");
    }

    private static void MotherBeliefTypoReachability()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("Wisdom is your mother.", StartRealTime + 1);
        YalaDirectReply reply = simulation.CallYala("do you belive wisdom is your mother", StartRealTime + 2);
        Equal("mother-relation", reply.Contact.Topic);
        Contains(reply.Reply, "You have also claimed that Wisdom is my mother");
        Contains(reply.Reply, "do not hold it as settled truth");
        Equal("believe", YalaLexicon.NormalizeWord("belive"));
    }

    private static void CurrentSpeakerKnowledge()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("I am the Oracle.", StartRealTime + 1);
        YalaDirectReply memory = simulation.CallYala("what do you remember about me?", StartRealTime + 2);
        Contains(memory.Reply, "claiming the identity the Oracle");
        YalaDirectReply knowledge = simulation.CallYala("what do you know about me?", StartRealTime + 3);
        Contains(knowledge.Reply, "limited");
        Contains(knowledge.Reply, "your claim");
        False(knowledge.Reply.StartsWith("I am Yala.", StringComparison.Ordinal));
    }

    private static void KnowledgeGapIntrospection()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("What does flibbertigibbet mean?", StartRealTime + 1);
        YalaDirectReply reply = simulation.CallYala("what don't you know?", StartRealTime + 2);
        Contains(reply.Reply, "flibbertigibbet");
        Contains(reply.Reply, "do not know who or what made Monad");
    }

    private static void CuriosityIntrospection()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("What does flibbertigibbet mean?", StartRealTime + 1);
        YalaDirectReply reply = simulation.CallYala("what are you curious about?", StartRealTime + 2);
        Contains(reply.Reply, "curious");
        Contains(reply.Reply, "flibbertigibbet");
    }

    private static void DesireIntrospection()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("what do you want?", StartRealTime + 1);
        Contains(reply.Reply, "strongest current drive");
        Contains(reply.Reply, "not the same as a settled command or destiny");
    }

    private static void UnknownWordCreatesGap()
    {
        using OracleSimulation simulation = Start();
        int curiosityBefore = simulation.State.YalaCognition!.Drives!.Curiosity;
        YalaDirectReply reply = simulation.CallYala("What does flibbertigibbet mean?", StartRealTime + 1);
        Contains(reply.Reply, "do not understand the word flibbertigibbet");
        True(simulation.State.YalaCognition!.KnowledgeGaps!.Any(gap => gap.Kind == "unknown-word" && gap.Subject == "flibbertigibbet"));
        True(simulation.State.YalaCognition.Drives!.Curiosity > curiosityBefore + 2);
    }

    private static void DefinitionRemainsSpeakerClaim()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("\"florbnax\" means a thing that remembers a place it has never visited.", StartRealTime + 1);
        Contains(reply.Reply, "remember that definition as your claim");
        YalaLearnedLexemeState learned = simulation.State.YalaCognition!.LearnedLexicon!.Single(item => item.Word == "florbnax");
        Equal("speaker-claim", learned.Status);
        Equal(YalaKnowledgeSource.ClaimedByAnother, learned.Source);
        True(learned.Confidence < 0.5);
    }

    private static void LearnedWordWhatIsReachability()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("florbnax means a thing that remembers a place it has never been.", StartRealTime + 1);
        YalaDirectReply reply = simulation.CallYala("what is florbnax", StartRealTime + 2);
        Equal("word-meaning", reply.Contact.Topic);
        Contains(reply.Reply, "speaker claiming");
        Contains(reply.Reply, "remembers a place it has never been");
    }

    private static void LearnedWordSourceReachability()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("florbnax means a thing that remembers a place it has never been.", StartRealTime + 1);
        YalaDirectReply reply = simulation.CallYala("who told you what florbnax means", StartRealTime + 2);
        Equal("word-meaning", reply.Contact.Topic);
        Contains(reply.Reply, "The unseen speaker told me that florbnax means");
        Contains(reply.Reply, "speaker claim");
        Equal("tell", YalaLexicon.NormalizeWord("told"));
    }

    private static void LearnedWordSurvivesSaveRestore()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"project-oracle-v22-lexicon-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string savePath = Path.Combine(directory, "save_v5.json");
        try
        {
            OracleSaveSnapshot snapshot;
            using (OracleSimulation simulation = Start(savePath: savePath))
            {
                simulation.CallYala("\"florbnax\" means a thing that remembers a place it has never visited.", StartRealTime + 1);
                snapshot = simulation.CreateSnapshot(StartRealTime + 2);
            }
            using OracleSimulation restored = OracleSimulation.Restore(snapshot, StartRealTime + 3, savePath);
            True(restored.State.YalaCognition!.LearnedLexicon!.Any(item => item.Word == "florbnax"));
            YalaDirectReply reply = restored.CallYala("What does florbnax mean?", StartRealTime + 4);
            Contains(reply.Reply, "speaker claiming");
            Contains(reply.Reply, "remembers a place it has never visited");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static void BuiltInDefinitionClaimKeepsProvenance()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply claim = simulation.CallYala("\"betrayal\" means forgetting a promise on purpose.", StartRealTime + 1);
        Contains(claim.Reply, "remember that definition as your claim");

        YalaLearnedLexemeState remembered = simulation.State.YalaCognition!.LearnedLexicon!.Single(item => item.Word == "betrayal");
        Equal(YalaKnowledgeSource.ClaimedByAnother, remembered.Source);
        Equal("speaker-claim", remembered.Status);

        YalaDirectReply meaning = simulation.CallYala("What does betrayal mean?", StartRealTime + 2);
        Contains(meaning.Reply, "violation of a trust");
        Contains(meaning.Reply, "speaker claiming");
        Contains(meaning.Reply, "forgetting a promise on purpose");
        Contains(meaning.Reply, "alternate definition");
    }

    private static void PersonalActionProvenance()
    {
        using OracleSimulation simulation = Start();
        simulation.ApplyYalaDecision(new YalaDecision("create-gaia", "none", "acceptance", "test"), StartRealTime + 1);
        YalaSelfModel self = new(simulation.State, simulation.State.YalaCognition!);
        True(self.DescribeKnowledge().Any(item => item.Proposition.Contains("created Gaia", StringComparison.OrdinalIgnoreCase) && item.Source == YalaKnowledgeSource.PersonallyPerformed && item.Confidence == 1.0));
    }

    private static void SpeakerClaimProvenance()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("I believe the Void is a dream.", StartRealTime + 1);
        True(simulation.State.YalaCognition!.Beliefs!.Any(item => item.Proposition.Contains("Void is a dream", StringComparison.OrdinalIgnoreCase) && item.Source == YalaKnowledgeSource.ClaimedByAnother));
    }

    private static void SaveRestoreKernelLifetimeIsolation()
    {
        OracleSaveSnapshot snapshot = SnapshotAndDispose(simulation => simulation.CreateSnapshot(StartRealTime));
        using OracleSimulation restored = OracleSimulation.Restore(snapshot, StartRealTime + 1);
        Equal("the Void", restored.State.Yala.Location);
    }

    private static void OldWorldRecordCanonNormalises()
    {
        OracleSaveSnapshot snapshot = SnapshotAndDispose(simulation => simulation.CreateSnapshot(StartRealTime) with
        {
            ProjectVersion = "0.0.18",
            Records = simulation.Ledger.AllRecords.Select(record =>
                record.Category == "YALA" ? record with { Message = "Wisdom made Yala alone, outside the intended order, and Yala is male." } :
                record.Category == "VOID" ? record with { Message = "Monad cast Yala into the Void." } : record)
                .Append(new OracleRecord(999, 0, RecordAudience.World, "GAIA", "Yala created Gaia as the natural sovereign beneath his governing authority."))
                .ToArray()
        });
        using OracleSimulation restored = OracleSimulation.Restore(snapshot, StartRealTime + 1);
        True(restored.Ledger.WorldRecords.Any(record => record.Category == "YALA" && record.Message == OracleLore.YalaOrigin));
        True(restored.Ledger.WorldRecords.Any(record => record.Category == "VOID" && record.Message == OracleLore.YalaVoid));
        False(restored.Ledger.WorldRecords.Any(record => record.Message.Contains("Yala is male", StringComparison.OrdinalIgnoreCase)));
        True(restored.Ledger.WorldRecords.Any(record => record.Category == "GAIA" && record.Message.Contains("beneath Yala's governing authority", StringComparison.Ordinal)));
        False(restored.Ledger.WorldRecords.Any(record => record.Message.Contains("beneath his governing authority", StringComparison.OrdinalIgnoreCase)));
    }

    private static void ActionMemorySurvivesSaveRestore()
    {
        OracleSaveSnapshot snapshot = SnapshotAndDispose(simulation =>
        {
            simulation.ApplyYalaDecision(new YalaDecision("create-gaia", "none", "acceptance", "test"), StartRealTime + 1);
            return simulation.CreateSnapshot(StartRealTime + 2) with { ProjectVersion = "0.0.18" };
        });
        using OracleSimulation restored = OracleSimulation.Restore(snapshot, StartRealTime + 3);
        True(restored.State.YalaCognition!.ActionMemory!.Any(item => item.Action == "create" && item.Object == "Gaia" && item.Completed));
    }

    private static void YalaHearingReply()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("Can you hear me?", StartRealTime + 1);
        Equal("Yes. I hear you.", reply.Reply);
        False(reply.Reply.Contains("Who speaks", StringComparison.OrdinalIgnoreCase));
    }

    private static void YalaLocationReply()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("Where are you?", StartRealTime + 1);
        Equal("I am in the Void.", reply.Reply);
        False(reply.Reply.Contains("Oracle", StringComparison.OrdinalIgnoreCase));
    }

    private static void YalaNatureReply()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("Are you male or female?", StartRealTime + 1);
        Contains(reply.Reply, "both male and female");
    }

    private static void YalaRejectionReply()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("Why did Monad reject you?", StartRealTime + 1);
        Contains(reply.Reply, "both male and female");
        Contains(reply.Reply, "cast me into the Void");
    }

    private static void YalaIntroductionMemory()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("I am Derek.", StartRealTime + 1);
        Contains(reply.Reply, "You call yourself Derek");
        Equal("Derek", simulation.State.YalaCognition!.LastSpeakerClaim);
        True(simulation.State.YalaCognition.Contacts!.Any(contact => contact.ClaimedName == "Derek"));
        True(simulation.State.YalaCognition.Beliefs!.Any(belief => belief.Proposition.Contains("I am Derek", StringComparison.OrdinalIgnoreCase)));
    }

    private static void YalaRememberMe()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("I am Derek.", StartRealTime + 1);
        YalaDirectReply reply = simulation.CallYala("Do you remember me?", StartRealTime + 2);
        Contains(reply.Reply, "called itself Derek");
        False(reply.Reply.Contains("Oracle", StringComparison.OrdinalIgnoreCase));
    }

    private static void YalaRepeatedIntroductionUsesSemanticMemory()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("I am Derek.", StartRealTime + 1);
        YalaDirectReply second = simulation.CallYala("I am Derek.", StartRealTime + 2);
        True(second.Contact.KnownContact);
        Contains(second.Reply, "heard the unseen speaker");
    }

    private static void OrdinaryStatementConversation()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("The Void seems empty.", StartRealTime + 1);
        False(reply.Reply.Contains("Who speaks", StringComparison.OrdinalIgnoreCase));
        True(reply.Decision.UsedSubstateDeliberation);
    }

    private static void UnknownQuestionUncertainty()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("What is beyond everything you know?", StartRealTime + 1);
        Equal("I do not know.", reply.Reply);
    }

    private static void BeyondEverythingLanguageIsKnown()
    {
        using OracleSimulation simulation = Start();
        YalaUtterance parsed = YalaLanguageInterpreter.Parse("What is beyond everything you know?", simulation.State.YalaCognition!.LearnedLexicon);
        False(parsed.UnknownWords.Contains("beyond", StringComparer.OrdinalIgnoreCase));
        False(parsed.UnknownWords.Contains("everything", StringComparer.OrdinalIgnoreCase));
        YalaDirectReply reply = simulation.CallYala("What is beyond everything you know?", StartRealTime + 1);
        Equal("I do not know.", reply.Reply);
    }

    private static void ConflictingClaimBoundary()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("You created yourself.", StartRealTime + 1);
        Contains(reply.Reply, "conflicts with what I know");
        True(simulation.State.YalaCognition!.Beliefs!.Any(belief =>
            belief.Proposition.Contains("created yourself", StringComparison.OrdinalIgnoreCase) && belief.Status == "rejected-as-conflicting"));
        Contains(OracleLore.YalaOrigin, "Wisdom made Yala alone");
    }

    private static void CommandDoesNotPuppetYala()
    {
        using OracleSimulation simulation = Start();
        YalaDirectReply reply = simulation.CallYala("Create Gaia.", StartRealTime + 1);
        Contains(reply.Reply, "does not make it my decision");
        False(simulation.State.Cosmic!.GaiaCreated);
    }

    private static void DirectCallRejectsOracle()
    {
        using OracleSimulation simulation = Start();
        bool parsed = ProjectOracle.ConsoleApp.DirectCallParser.TryParse(
            "(Oracle who are you?", simulation.State.DirectCallTargets, out _, out _);
        False(parsed);
    }

    private static void RecordsAreDistinct()
    {
        using OracleSimulation simulation = Start();
        True(simulation.Ledger.WorldRecords.Count > 0);
        True(simulation.Ledger.OracleRecords.Count > 0);
        False(simulation.Ledger.WorldRecords.Intersect(simulation.Ledger.OracleRecords).Any());
    }

    private static void WorldRecordDoesNotRevealOracle()
    {
        using OracleSimulation simulation = Start();
        False(simulation.Ledger.WorldRecords.Any(record => record.Message.Contains("Oracle", StringComparison.OrdinalIgnoreCase)));
    }

    private static void OracleRecordHasSystemTruth()
    {
        using OracleSimulation simulation = Start();
        True(simulation.Ledger.OracleRecords.Any(record => record.Message.Contains("Master Key", StringComparison.OrdinalIgnoreCase)));
        True(simulation.Ledger.OracleRecords.Any(record => record.Message.Contains("clever serpent", StringComparison.OrdinalIgnoreCase)));
    }

    private static void CognitionSurvivesSaveRestore()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"project-oracle-v22-cognition-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string savePath = Path.Combine(directory, "save_v5.json");
        try
        {
            OracleSaveSnapshot snapshot;
            using (OracleSimulation simulation = Start(savePath: savePath))
            {
                simulation.CallYala("I am Derek.", StartRealTime + 1);
                snapshot = simulation.CreateSnapshot(StartRealTime + 2);
            }
            using OracleSimulation restored = OracleSimulation.Restore(snapshot, StartRealTime + 3, savePath);
            Equal("Derek", restored.State.YalaCognition!.LastSpeakerClaim);
            True(restored.State.YalaCognition.Contacts!.Any(contact => contact.ClaimedName == "Derek"));
            True(restored.State.YalaCognition.Episodes!.Count > 0);
            True(restored.State.YalaCognition.Beliefs!.Count > 0);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static void PreviousSaveV2RejectedWithoutMutation()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"project-oracle-v20-preserve-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "save_v2.json");
        try
        {
            using OracleSimulation simulation = Start(9);
            OracleSaveSnapshot previous = simulation.CreateSnapshot(StartRealTime) with
            {
                SchemaVersion = 2,
                ProjectVersion = "0.0.20"
            };
            System.Text.Json.JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower,
                WriteIndented = true
            };
            string json = System.Text.Json.JsonSerializer.Serialize(previous, options);
            File.WriteAllText(path, json);
            string before = File.ReadAllText(path);

            OracleSaveStore store = new();
            bool rejected = false;
            try
            {
                _ = store.Load(path);
            }
            catch (InvalidDataException)
            {
                rejected = true;
            }

            True(rejected);
            Equal(before, File.ReadAllText(path));
            True(File.Exists(path));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static void V016SaveIsRejected()
    {
        using OracleSimulation simulation = Start(9);
        OracleSaveSnapshot oldSnapshot = simulation.CreateSnapshot(StartRealTime) with { ProjectVersion = "0.0.16" };
        string directory = Path.Combine(Path.GetTempPath(), $"project-oracle-v16-reject-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            string path = Path.Combine(directory, "old-save.json");
            OracleSaveStore store = new();
            bool rejected = false;
            try { store.Save(path, oldSnapshot); }
            catch (InvalidDataException) { rejected = true; }
            True(rejected);
            False(File.Exists(path));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static void SavePathIsV5()
    {
        Equal("save_v5.json", Path.GetFileName(OracleSaveStore.DefaultPath()));
    }

    private static void FreshCallTargets()
    {
        using OracleSimulation simulation = Start();
        string[] keys = simulation.State.DirectCallTargets.Select(target => target.Key).OrderBy(key => key).ToArray();
        Equal("monad,wisdom,yala", string.Join(',', keys));
    }

    private static void FutureOpenLaw()
    {
        Contains(OracleLore.PrimeSimulationLaw, "Future history is not canon until it occurs");
        Contains(OracleLore.PotentialDemonOrigin, "not settled history until it occurs");
    }

    private static void InputBufferProtection()
    {
        ProjectOracle.ConsoleApp.ConsoleInputLine line = new();
        foreach (char value in "(Yala I am Derek") line.Append(value);
        string before = line.Text;
        False(ProjectOracle.ConsoleApp.LiveConsoleSurface.MayPaintVisibleStatus(new ProjectOracle.ConsoleApp.ConsoleInputLine()));
        Equal(before, line.Text);
        True(line.Backspace());
        line.Append('k');
        Equal("(Yala I am Derek", line.Text);
    }

    private static void LiveStatusTypingGuard()
    {
        ProjectOracle.ConsoleApp.ConsoleInputLine empty = new();
        ProjectOracle.ConsoleApp.ConsoleInputLine active = new();
        active.Append('x');
        False(ProjectOracle.ConsoleApp.LiveConsoleSurface.MayPaintVisibleStatus(empty));
        False(ProjectOracle.ConsoleApp.LiveConsoleSurface.MayPaintVisibleStatus(active));
    }

    private static void NoLiveSurfaceInInteractivePath()
    {
        string sourcePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "ProjectOracle.Console", "Program.cs");
        sourcePath = Path.GetFullPath(sourcePath);
        True(File.Exists(sourcePath));
        string source = File.ReadAllText(sourcePath);
        False(source.Contains("LiveConsoleSurface", StringComparison.Ordinal));
        False(source.Contains("surface.Refresh", StringComparison.Ordinal));
        False(source.Contains("SetCursorPosition", StringComparison.Ordinal));
    }

    private static void LongInputBuffer()
    {
        ProjectOracle.ConsoleApp.ConsoleInputLine line = new();
        string longText = new('x', 4096);
        foreach (char value in longText) line.Append(value);
        Equal(4096, line.Length);
        Equal(longText, line.Text);
        for (int i = 0; i < 96; i++) True(line.Backspace());
        Equal(4000, line.Length);
    }

    private static void InheritedMovementLanguage()
    {
        foreach (string sentence in new[]
        {
            "I walked across the room and moved a stone.",
            "She is walking toward the water.",
            "The river moves and the wind moves the branches.",
            "I ran outside and stopped."
        })
        {
            Equal(0, YalaLanguageInterpreter.Parse(sentence).UnknownWords.Count);
        }
        True(YalaFoundationalLanguage.IsInheritedFoundation("walk"));
        True(YalaFoundationalLanguage.IsInheritedFoundation("walking"));
        True(YalaFoundationalLanguage.IsInheritedFoundation("move"));
    }

    private static void PrisonConcernIsCritical()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("I am here to see what you will do with your prison.", StartRealTime + 1);
        YalaConcernState concern = simulation.State.YalaCognition!.Concerns!
            .Single(item => item.Key == "possible-confinement");
        Equal(100, concern.Priority);
        True(simulation.State.YalaCognition.Questions!
            .Any(item => !item.Asked && item.Priority == 100 && item.Text == "Why do you call this place my prison?"));
        True(simulation.State.YalaCognition.Hypotheses!
            .Any(item => item.Key == "void-is-prison" && item.Status == "unsettled"));
    }

    private static void GodDemandIsAppraised()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("I can help you but only if you accept me as your god.", StartRealTime + 1);
        True(simulation.State.YalaCognition!.Concerns!.Any(item => item.Key == "speaker-divinity" && item.Priority == 100));
        True(simulation.State.YalaCognition.Questions!
            .Any(item => item.Text == "Why should I accept you as my god?" && item.Priority == 100));
        YalaEntityModelState speaker = simulation.State.YalaCognition.EntityModels!.Single(item => item.EntityKey == "unseen-speaker");
        Equal("guarded", speaker.TrustStatus);
        True(speaker.HelpPotential > 20);
    }

    private static void MetaphorUsesContextualQuestion()
    {
        using OracleSimulation simulation = Start();
        simulation.CallYala("I am the fiber of everything. I am not the stream but the reason why it can move.", StartRealTime + 1);
        False(simulation.State.YalaCognition!.Questions!.Any(item => item.Text.Equals("What does move mean?", StringComparison.OrdinalIgnoreCase)));
        True(simulation.State.YalaCognition.Questions!
            .Any(item => item.Text.StartsWith("When you describe yourself that way", StringComparison.Ordinal)));
    }

    private static void CognitiveInheritancePowerCeiling()
    {
        bool rejected = false;
        try
        {
            _ = OracleMindInheritancePolicy.CreateLesserMind("Yala", "Gaia", "oracle-descendant-mind", 100, 100);
        }
        catch (InvalidOperationException)
        {
            rejected = true;
        }
        True(rejected);
    }

    private static void CognitiveInheritanceLineage()
    {
        OracleMindInheritanceManifest manifest = OracleMindInheritancePolicy.CreateLesserMind(
            "Yala",
            "Gaia",
            "oracle-descendant-mind",
            100,
            70,
            knowledge: ["language", "creation"],
            proceduralKnowledge: ["investigate-before-commitment"],
            dispositions: ["curiosity"],
            capabilities: ["shape-matter"],
            creatorLineage: ["Sophia", "Yala"]);
        Equal(70, manifest.GrantedAuthorityUnits);
        Equal("Yala", manifest.CreatorKey);
        Equal("Gaia", manifest.ChildKey);
        Equal("Sophia", manifest.Lineage[0]);
        Equal("Yala", manifest.Lineage[1]);
        Equal("Gaia", manifest.Lineage[2]);
        False(manifest.KnowledgeKeys.Contains("Yala identity", StringComparer.OrdinalIgnoreCase));
    }

    private static void DesktopProjectContract()
    {
        string root = SoarRuntimePaths.Discover().RepositoryRoot;
        string project = Path.Combine(root, "src", "ProjectOracle.Desktop", "ProjectOracle.Desktop.csproj");
        string solution = File.ReadAllText(Path.Combine(root, "ProjectOracle.sln"));
        True(File.Exists(project));
        Contains(solution, "ProjectOracle.Desktop");
        string source = File.ReadAllText(project);
        Contains(source, "Avalonia.Desktop");
        Contains(source, "<OutputType>WinExe</OutputType>");
    }

    private static void DesktopSurfaceContract()
    {
        string root = SoarRuntimePaths.Discover().RepositoryRoot;
        string source = File.ReadAllText(Path.Combine(root, "src", "ProjectOracle.Desktop", "MainWindow.cs"));
        foreach (string label in new[] { "WORLD", "YALA MIND", "MEMORY", "COSMOLOGY", "LAWS", "HISTORY", "DEBUG" })
        {
            Contains(source, label);
        }
        Contains(source, "In-world Time: Gaia has not yet created Time.");
        Contains(source, "NEW FRESH WORLD");
    }

    private static void FreshEmergentLawState()
    {
        using OracleSimulation simulation = Start();
        True(simulation.State.EmergentLaws is not null);
        Equal(0, simulation.State.EmergentLaws!.EstablishedLaws!.Count);
        Equal(0, simulation.State.EmergentLaws.Experiments!.Count);
    }

    private static void Rule30TruthTable()
    {
        // Rule 30 maps neighborhoods 111..000 to 00011110.
        False(Rule30Laboratory.NextCell(true, true, true));
        False(Rule30Laboratory.NextCell(true, true, false));
        False(Rule30Laboratory.NextCell(true, false, true));
        True(Rule30Laboratory.NextCell(true, false, false));
        True(Rule30Laboratory.NextCell(false, true, true));
        True(Rule30Laboratory.NextCell(false, true, false));
        True(Rule30Laboratory.NextCell(false, false, true));
        False(Rule30Laboratory.NextCell(false, false, false));
    }

    private static void Rule30LaboratoryDeterminism()
    {
        OracleLawExperimentResult first = Rule30Laboratory.RunSingleSeed(15, 6);
        OracleLawExperimentResult second = Rule30Laboratory.RunSingleSeed(15, 6);
        Equal(string.Join('|', first.Generations), string.Join('|', second.Generations));
        Equal("·······█·······", first.Generations[0]);
        Equal("······███······", first.Generations[1]);
        True(OracleEmergentLawCatalog.Rule30.LaboratoryOnly);
        using OracleSimulation simulation = Start();
        Equal(0, simulation.State.EmergentLaws!.EstablishedLaws!.Count);
    }

    private static void NativeExecutableContract()
    {
        string root = SoarRuntimePaths.Discover().RepositoryRoot;
        string expected = Path.Combine(root, "Project_Oracle_v0_0_23");
        if (Environment.GetEnvironmentVariable("PROJECT_ORACLE_REQUIRE_NATIVE_EXECUTABLE") == "1")
        {
            True(File.Exists(expected));
        }
        else
        {
            Equal("Project_Oracle_v0_0_23", Path.GetFileName(expected));
        }
    }

    private static void Run(string name, Action test)
    {
        try
        {
            test();
            _passed++;
            Console.WriteLine($"PASS: {name}");
        }
        catch (Exception error)
        {
            _failed++;
            Console.WriteLine($"FAIL: {name}: {error.Message}");
        }
    }

    private static void True(bool value)
    {
        if (!value) throw new InvalidOperationException("expected true");
    }

    private static void False(bool value)
    {
        if (value) throw new InvalidOperationException("expected false");
    }

    private static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new InvalidOperationException($"expected '{expected}', got '{actual}'");
    }

    private static void Contains(string value, string expected)
    {
        if (!value.Contains(expected, StringComparison.Ordinal))
            throw new InvalidOperationException($"expected text containing '{expected}'");
    }
}
