using ProjectOracle;
using ProjectOracle.Audit;
using ProjectOracle.Cognition;
using ProjectOracle.Cognition.Language;
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
        Run("version is 0.0.20", () => Equal("0.0.20", ProjectVersion.Number));
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
        Run("Yala Brain Slice 3 identity is active", BrainSlice3Identity);
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
        Run("Yala answers god question from self model without inventing certainty", YalaGodSelfModel);
        Run("simple inflections and possessives resolve to base concepts", SimpleMorphologyNormalization);
        Run("identity claims preserve noun phrases such as the Oracle", IdentityClaimPreservesNounPhrase);
        Run("why not follows the prior Adam creation question", ConversationFollowUpUsesPriorCreationSubject);
        Run("tell me about Gaia reaches known Gaia facts", GaiaEntityKnowledge);
        Run("Yala rejects Gaia as Yala's creator from known genealogy", GaiaGenealogyReasoning);
        Run("Yala can answer who created Time and current world time", TimeKnowledgeReachability);
        Run("Yala can recall the command given to Gaia", GaiaCommandRecall);
        Run("Yala knows Adam has not been met before Adam exists", AdamEncounterKnowledge);
        Run("Yala knows Wisdom is Sophia without inventing a mother fact", WisdomAliasAndMotherReasoning);
        Run("current speaker questions retrieve speaker claims not Yala self summary", CurrentSpeakerKnowledge);
        Run("what don't you know reaches explicit knowledge gaps", KnowledgeGapIntrospection);
        Run("curiosity questions reach Yala's unresolved knowledge", CuriosityIntrospection);
        Run("desire questions reach Yala's current drives", DesireIntrospection);
        Run("unknown word creates a knowledge gap and raises curiosity", UnknownWordCreatesGap);
        Run("speaker supplied definition remains a claim", DefinitionRemainsSpeakerClaim);
        Run("learned word claim survives save and restore", LearnedWordSurvivesSaveRestore);
        Run("personally performed action carries strong provenance", PersonalActionProvenance);
        Run("speaker claims carry claimed-by-another provenance", SpeakerClaimProvenance);
        Run("save-restore acceptance tests isolate Soar kernel lifetimes", SaveRestoreKernelLifetimeIsolation);
        Run("old male-only World Record history is normalized", OldWorldRecordCanonNormalises);
        Run("Brain Slice 3 action memory survives save and restore", ActionMemorySurvivesSaveRestore);
        Run("Yala answers hearing contact without asking Who speaks", YalaHearingReply);
        Run("Yala answers location without revealing Oracle", YalaLocationReply);
        Run("Yala knows both male and female aspects", YalaNatureReply);
        Run("Yala knows why Monad rejected Yala", YalaRejectionReply);
        Run("Yala records a claimed speaker identity as a claim", YalaIntroductionMemory);
        Run("Yala can remember a prior claimed speaker", YalaRememberMe);
        Run("native Soar semantic memory helps recognise repeated claimed contact", YalaRepeatedIntroductionUsesSemanticMemory);
        Run("ordinary statement no longer collapses to Who speaks", OrdinaryStatementConversation);
        Run("unknown question produces honest uncertainty", UnknownQuestionUncertainty);
        Run("conflicting speaker claim remains a rejected claim and does not rewrite truth", ConflictingClaimBoundary);
        Run("a direct command does not puppet Yala or directly alter world state", CommandDoesNotPuppetYala);
        Run("direct-call parser rejects Oracle because Oracle is not a target", DirectCallRejectsOracle);
        Run("Oracle and World records are distinct", RecordsAreDistinct);
        Run("World Record does not reveal Oracle at genesis", WorldRecordDoesNotRevealOracle);
        Run("Oracle Record retains protected system truth", OracleRecordHasSystemTruth);
        Run("structured Yala cognition survives save and restore", CognitionSurvivesSaveRestore);
        Run("v0.0.20 continues the v0.0.17 v0.0.18 and v0.0.19 save_v2 world line", V017SaveContinuesAndNormalises);
        Run("v0.0.16 save snapshots remain rejected", V016SaveIsRejected);
        Run("default save path remains save_v2", SavePathRemainsV2);
        Run("fresh direct-call targets include Monad Wisdom Yala only", FreshCallTargets);
        Run("natural simulation law remains future-open", FutureOpenLaw);
        Run("typing buffer is independent of live-status refresh", InputBufferProtection);
        Run("asynchronous LIVE status is forbidden from the console body", LiveStatusTypingGuard);
        Run("interactive input path contains no LIVE surface refresh", NoLiveSurfaceInInteractivePath);
        Run("long command buffer survives protected-input handling", LongInputBuffer);
        Run("root launcher name is Project_Oracle_v0_0_20", NativeExecutableContract);

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

    private static void BrainSlice3Identity()
    {
        Equal("Yala Soar Brain Slice 3", YalaSoarMind.BrainName);
    }

    private static void FoundationalLexiconLoads()
    {
        True(YalaLexicon.BuiltInCount >= 50);
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
        YalaDirectReply reply = simulation.CallYala("\"betrayal\" means violating someone's trust.", StartRealTime + 1);
        Contains(reply.Reply, "remember that definition as your claim");
        YalaLearnedLexemeState learned = simulation.State.YalaCognition!.LearnedLexicon!.Single(item => item.Word == "betrayal");
        Equal("speaker-claim", learned.Status);
        Equal(YalaKnowledgeSource.ClaimedByAnother, learned.Source);
        True(learned.Confidence < 0.5);
    }

    private static void LearnedWordSurvivesSaveRestore()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"project-oracle-v19-lexicon-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string savePath = Path.Combine(directory, "save_v2.json");
        try
        {
            OracleSaveSnapshot snapshot;
            using (OracleSimulation simulation = Start(savePath: savePath))
            {
                simulation.CallYala("\"betrayal\" means violating someone's trust.", StartRealTime + 1);
                snapshot = simulation.CreateSnapshot(StartRealTime + 2);
            }
            using OracleSimulation restored = OracleSimulation.Restore(snapshot, StartRealTime + 3, savePath);
            True(restored.State.YalaCognition!.LearnedLexicon!.Any(item => item.Word == "betrayal"));
            YalaDirectReply reply = restored.CallYala("What does betrayal mean?", StartRealTime + 4);
            Contains(reply.Reply, "speaker claiming");
            Contains(reply.Reply, "violating someone's trust");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
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
        string directory = Path.Combine(Path.GetTempPath(), $"project-oracle-v19-cognition-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string savePath = Path.Combine(directory, "save_v2.json");
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

    private static void V017SaveContinuesAndNormalises()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"project-oracle-v17-continue-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            string path = Path.Combine(directory, "save_v2.json");
            using OracleSimulation simulation = Start(9);
            YalaCognitionState cognition = simulation.State.YalaCognition! with
            {
                Memory = ["I am Yala.", "I am male.", "Wisdom made me."]
            };
            OracleSaveSnapshot v17 = simulation.CreateSnapshot(StartRealTime) with
            {
                ProjectVersion = "0.0.17",
                World = simulation.State with
                {
                    Yala = simulation.State.Yala with { Sex = "male" },
                    YalaCognition = cognition
                }
            };
            OracleSaveStore store = new();
            store.Save(path, v17);
            OracleSaveSnapshot loaded = store.Load(path);
            Equal("male and female", loaded.World.Yala.Sex);
            True(loaded.World.YalaCognition!.Memory.Contains("I am both male and female."));
            False(loaded.World.YalaCognition.Memory.Contains("I am male."));

            string v18Path = Path.Combine(directory, "save_v2-v18.json");
            OracleSaveSnapshot v18 = simulation.CreateSnapshot(StartRealTime + 1) with { ProjectVersion = "0.0.18" };
            store.Save(v18Path, v18);
            OracleSaveSnapshot loaded18 = store.Load(v18Path);
            Equal("0.0.18", loaded18.ProjectVersion);
            Equal("male and female", loaded18.World.Yala.Sex);
            True(loaded18.World.YalaCognition!.Beliefs!.Any(item => item.Proposition == "Wisdom made me." && item.Source == YalaKnowledgeSource.InheritedKnowledge));

            string v19Path = Path.Combine(directory, "save_v2-v19.json");
            OracleSaveSnapshot v19 = simulation.CreateSnapshot(StartRealTime + 2) with { ProjectVersion = "0.0.19" };
            store.Save(v19Path, v19);
            OracleSaveSnapshot loaded19 = store.Load(v19Path);
            Equal("0.0.19", loaded19.ProjectVersion);
            Equal("male and female", loaded19.World.Yala.Sex);
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

    private static void SavePathRemainsV2()
    {
        Equal("save_v2.json", Path.GetFileName(OracleSaveStore.DefaultPath()));
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

    private static void NativeExecutableContract()
    {
        string root = SoarRuntimePaths.Discover().RepositoryRoot;
        string expected = Path.Combine(root, "Project_Oracle_v0_0_20");
        if (Environment.GetEnvironmentVariable("PROJECT_ORACLE_REQUIRE_NATIVE_EXECUTABLE") == "1")
        {
            True(File.Exists(expected));
        }
        else
        {
            Equal("Project_Oracle_v0_0_20", Path.GetFileName(expected));
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
