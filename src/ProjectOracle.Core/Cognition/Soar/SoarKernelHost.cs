using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ProjectOracle.Cognition.CosmicChoice;
using ProjectOracle.Domain;

namespace ProjectOracle.Cognition.Soar;

/// <summary>
/// Persistent reflection-based host for the official Soar 9.6.5 C# SML bridge.
/// One host is kept alive for Yala's whole Project Oracle session so working memory,
/// semantic memory, episodic memory, impasses, and substates belong to one continuing mind.
/// </summary>
public sealed class SoarKernelHost : IDisposable
{
    private readonly SoarRuntimePaths _paths;
    private readonly nint _kernelNativeHandle;
    private readonly nint _bridgeNativeHandle;
    private readonly Assembly _smlAssembly;
    private readonly object _kernel;
    private readonly object _agent;
    private readonly object _inputLink;
    private long _runCount;
    private bool _disposed;

    public SoarKernelHost(string agentName, string productionsPath, SoarMemoryPaths? memoryPaths = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentName);
        ArgumentException.ThrowIfNullOrWhiteSpace(productionsPath);

        _paths = SoarRuntimePaths.Discover();
        _kernelNativeHandle = NativeLibrary.Load(_paths.NativeKernel);
        _bridgeNativeHandle = NativeLibrary.Load(_paths.NativeBridge);
        _smlAssembly = Assembly.LoadFrom(_paths.ManagedBridge);

        try
        {
            NativeLibrary.SetDllImportResolver(_smlAssembly, ResolveNativeLibrary);
        }
        catch (InvalidOperationException)
        {
            // Resolver can already be installed when multiple test hosts are created.
        }

        Type kernelType = _smlAssembly.GetType("sml.Kernel", throwOnError: true)!;
        // Project Oracle embeds Soar in-process and does not expose a remote SML service.
        // Soar defines port 0 as kSuppressListener, which prevents each embedded kernel
        // from binding the default TCP listener (12121). This is especially important
        // during the acceptance suite, which creates and disposes many kernels quickly.
        _kernel = InvokeStatic(kernelType, "CreateKernelInNewThread", 0)
            ?? throw new InvalidOperationException("Soar failed to create its kernel.");

        if (TryInvokeBool(_kernel, "HadError", out bool hadError) && hadError)
        {
            string description = TryInvokeString(_kernel, "GetLastErrorDescription") ?? "unknown Soar kernel error";
            throw new InvalidOperationException($"Soar kernel startup failed: {description}");
        }

        _agent = Invoke(_kernel, "CreateAgent", agentName)
            ?? throw new InvalidOperationException("Soar failed to create the Yala agent.");

        ConfigureLongTermMemory(memoryPaths);

        object? loaded = Invoke(_agent, "LoadProductions", Path.GetFullPath(productionsPath));
        if (loaded is bool loadedOk && !loadedOk)
        {
            string description = TryInvokeString(_agent, "GetLastErrorDescription") ?? "production load returned false";
            throw new InvalidOperationException($"Yala Soar productions failed to load: {description}");
        }

        _inputLink = Invoke(_agent, "GetInputLink")
            ?? throw new InvalidOperationException("Soar did not expose the input link.");
    }

    public long RunCount => _runCount;

    public YalaDecision Run(YalaPerception perception)
    {
        ArgumentNullException.ThrowIfNull(perception);
        ThrowIfDisposed();

        long beforeCycles = GetDecisionCycleCounter();
        object input = Invoke(_agent, "CreateIdWME", _inputLink, "world-input")
            ?? throw new InvalidOperationException("Soar could not create Yala's world-input WME.");

        CreateString(input, "location", perception.Location);
        CreateString(input, "gaia-created", YesNo(perception.GaiaCreated));
        CreateString(input, "time-created", YesNo(perception.TimeCreated));
        CreateInt(input, "decision-count", perception.DecisionCount);
        CreateString(input, "last-action", Clean(perception.LastAction, "none"));
        CreateString(input, "last-result", Clean(perception.LastResult, "none"));
        CreateInt(input, "drive-curiosity", perception.Curiosity);
        CreateInt(input, "drive-caution", perception.Caution);
        CreateInt(input, "drive-authority", perception.Authority);
        CreateInt(input, "drive-companionship", perception.Companionship);
        CreateInt(input, "drive-comfort", perception.Comfort);
        CreateInt(input, "uncertainty", perception.Uncertainty);
        CreateString(input, "contact", YesNo(perception.HasContact));
        CreateString(input, "pending-question", YesNo(perception.PendingQuestion));
        CreateString(input, "pending-question-text", Clean(perception.PendingQuestionText, "none"));
        CreateInt(input, "pending-question-priority", perception.PendingQuestionPriority);
        CreateString(input, "active-concern", Clean(perception.ActiveConcernKey, "none"));
        CreateInt(input, "active-concern-priority", perception.ActiveConcernPriority);
        CreateInt(input, "appraisal-threat", perception.AppraisalThreat);
        CreateInt(input, "appraisal-salience", perception.AppraisalSalience);
        CreateString(input, "active-plan", Clean(perception.ActivePlanKey, "none"));
        CreateInt(input, "active-plan-priority", perception.ActivePlanPriority);
        CreateString(input, "active-plan-next-action", Clean(perception.ActivePlanNextAction, "none"));
        CreateString(input, "active-investigation", Clean(perception.ActiveInvestigationKey, "none"));
        CreateInt(input, "active-investigation-priority", perception.ActiveInvestigationPriority);
        CreateString(input, "workspace-focus-type", Clean(perception.WorkspaceFocusType, "self-world"));
        CreateString(input, "workspace-focus-key", Clean(perception.WorkspaceFocusKey, "understand-current-world"));
        CreateInt(input, "workspace-focus-priority", perception.WorkspaceFocusPriority);
        CreateInt(input, "workspace-stagnation", perception.WorkspaceStagnationCount);
        CreateString(input, "speaker-history", YesNo(perception.HasSpeakerHistory));
        CreateString(input, "cosmic-choice-ready", YesNo(perception.CosmicChoiceReady));
        CreateInt(input, "cosmic-choice-count", perception.CosmicChoices.Count);
        CreateInt(input, "religious-tradition-count", YalaReligiousKnowledgeCatalog.Traditions.Count);
        CreateInt(input, "religious-idea-count", YalaReligiousKnowledgeCatalog.Ideas.Count);

        YalaDriveState scoringDrives = perception.Drives ?? new YalaDriveState(
            perception.Curiosity,
            perception.Caution,
            perception.Authority,
            perception.Companionship,
            perception.Comfort,
            perception.Uncertainty);
        foreach (YalaCosmicChoiceDefinition choice in perception.CosmicChoices)
        {
            object choiceInput = Invoke(_agent, "CreateIdWME", input, "cosmic-option")
                ?? throw new InvalidOperationException("Soar could not create a cosmic-option input WME.");
            CreateString(choiceInput, "key", choice.Key);
            CreateString(choiceInput, "domain", choice.Domain);
            CreateString(choiceInput, "action", choice.Action);
            CreateString(choiceInput, "meaning", choice.Meaning);
            CreateString(choiceInput, "status", choice.Status);
            CreateInt(choiceInput, "score", YalaCosmicChoiceCatalog.Score(choice, scoringDrives));
            CreateInt(choiceInput, "affinity-curiosity", choice.CuriosityAffinity);
            CreateInt(choiceInput, "affinity-caution", choice.CautionAffinity);
            CreateInt(choiceInput, "affinity-authority", choice.AuthorityAffinity);
            CreateInt(choiceInput, "affinity-companionship", choice.CompanionshipAffinity);
            CreateInt(choiceInput, "affinity-comfort", choice.ComfortAffinity);
        }

        if (perception.HasContact)
        {
            YalaContactFrame contact = perception.ContactFrame;
            CreateString(input, "contact-message", Clean(perception.ContactMessage, "none"));
            CreateString(input, "speech-act", contact.SpeechAct);
            CreateString(input, "topic", contact.Topic);
            CreateString(input, "claimed-speaker", Clean(contact.ClaimedSpeakerName, "none"));
            CreateString(input, "known-contact", YesNo(contact.KnownContact));
            CreateString(input, "asks-remember", YesNo(contact.AsksRememberMe));
            CreateString(input, "contains-claim", YesNo(contact.ContainsClaim));
            CreateString(input, "claim-conflicts", YesNo(contact.ClaimConflictsWithKnownFact));
            CreateString(input, "fact-known", YesNo(contact.FactKnown));
            CreateString(input, "ambiguous", YesNo(contact.Ambiguous));
            if (contact.Language is not null)
            {
                CreateString(input, "language-subject", Clean(contact.Language.Subject, "none"));
                CreateString(input, "language-verb", Clean(contact.Language.Verb, "none"));
                CreateString(input, "language-object", Clean(contact.Language.Object, "none"));
                CreateString(input, "language-negated", YesNo(contact.Language.Negated));
                CreateInt(input, "unknown-word-count", contact.Language.UnknownWords.Count);
                CreateString(input, "defined-word", Clean(contact.Language.DefinedWord, "none"));
            }
        }

        Invoke(_agent, "Commit");
        Invoke(_agent, "RunSelfTilOutput");

        int commandCount = Convert.ToInt32(Invoke(_agent, "GetNumberCommands") ?? 0, CultureInfo.InvariantCulture);
        if (commandCount < 1)
        {
            throw new InvalidOperationException("Yala's Soar agent produced no output command.");
        }

        object command = Invoke(_agent, "GetCommand", commandCount - 1)
            ?? throw new InvalidOperationException("Yala's Soar output command could not be read.");
        string commandName = Convert.ToString(Invoke(command, "GetCommandName"), CultureInfo.InvariantCulture) ?? string.Empty;
        if (!commandName.Equals("yala-action", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Unexpected Yala Soar output command: {commandName}");
        }

        string action = Parameter(command, "name", "wait");
        string replyCode = Parameter(command, "reply-code", "none");
        string deliberation = Parameter(command, "deliberation", "direct");
        string cosmicChoiceKey = Parameter(command, "choice-key", "none");
        Invoke(command, "AddStatusComplete");
        Invoke(_agent, "Commit");
        Invoke(_agent, "DestroyWME", input);
        Invoke(_agent, "Commit");
        // Let Soar retract the completed output production before the next perception.
        Invoke(_agent, "RunSelf", 1);
        Invoke(_agent, "ClearOutputLinkChanges");

        long afterCycles = GetDecisionCycleCounter();
        _runCount = checked(_runCount + 1);
        int cycles = checked((int)Math.Clamp(afterCycles - beforeCycles, 0, int.MaxValue));
        bool usedSubstate = deliberation.Equals("substate", StringComparison.OrdinalIgnoreCase) || cycles > 1;

        return new YalaDecision(
            action,
            replyCode,
            "Soar 9.6.5",
            usedSubstate
                ? $"Soar selected operator '{action}' after an impasse/substate deliberation."
                : $"Soar selected operator '{action}' from Yala's current perception.",
            cycles,
            usedSubstate,
            cosmicChoiceKey.Equals("none", StringComparison.OrdinalIgnoreCase) ? null : cosmicChoiceKey);
    }

    public void SeedCanonicalSemanticMemory()
    {
        ThrowIfDisposed();
        Execute("smem --add {(@yala ^name Yala ^nature male ^nature female ^made-by Wisdom ^rejected-by Monad ^rejection-reason both-male-and-female)}");
        Execute("smem --add {(@wisdom ^name Wisdom ^made-by Monad ^made Yala)}");
        Execute("smem --add {(@monad ^name Monad ^made Wisdom)}");
        Execute("smem --add {(@concept-create ^word create ^meaning |cause something to begin existing| ^opposite destroy)}");
        Execute("smem --add {(@concept-reject ^word reject ^meaning |refuse to accept| ^opposite accept)}");
        Execute("smem --add {(@concept-claim ^word claim ^meaning |assertion whose truth is not guaranteed|)}");
        Execute("smem --add {(@concept-evidence ^word evidence ^meaning |information that can change confidence in a proposition|)}");
        Execute("smem --add {(@concept-relationship ^word relationship ^meaning |structured connection between beings|)}");
        Execute("smem --add {(@concept-question ^word question ^meaning |utterance seeking information|)}");
        Execute("smem --add {(@concept-agency ^word agency ^meaning |capacity to choose and act within available possibilities|)}");
        Execute("smem --add {(@concept-autonomy ^word autonomy ^meaning |capacity to choose without another speaker selecting each action|)}");
        Execute("smem --add {(@concept-comparative-religion ^word |comparative religion| ^meaning |attributed knowledge of multiple religious and cosmological traditions without treating any tradition as automatic world truth|)}");
        Execute("smem --add {(@concept-cosmic-choice ^word |cosmic choice| ^meaning |a concrete possible way Yala may shape existence; a possibility is not a command or destiny|)}");
        Execute("smem --add {(@concept-salience ^word salience ^meaning |how strongly an event should command attention because it matters to current concerns goals danger opportunity or uncertainty|)}");
        Execute("smem --add {(@concept-appraisal ^word appraisal ^meaning |evaluation of what an event means for Yala rather than a simple mood meter|)}");
        Execute("smem --add {(@concept-inherited-language ^word |inherited foundational language| ^meaning |ordinary language and basic concepts Yala begins knowing without needing infant-style definitions|)}");
        Execute("smem --add {(@concept-plan ^word plan ^meaning |an ordered revisable sequence of intended actions toward a goal|)}");
        Execute("smem --add {(@concept-investigation ^word investigation ^meaning |a persistent question pursued through evidence tests and revisable conclusions|)}");
        Execute("smem --add {(@concept-counterfactual ^word counterfactual ^meaning |a considered possibility about what might happen if another action were taken|)}");
        Execute("smem --add {(@concept-metacognition ^word metacognition ^meaning |reasoning about uncertainty evidence and the quality of one's own reasoning|)}");

        foreach (ReligiousTraditionKnowledge tradition in YalaReligiousKnowledgeCatalog.Traditions)
        {
            string traditionLti = StableLti("tradition", tradition.Key);
            Execute($"smem --add {{({traditionLti} ^type comparative-religion ^key |{EscapeSymbol(tradition.Key)}| ^name |{EscapeSymbol(tradition.Name)}| ^family |{EscapeSymbol(tradition.Family)}| ^source-basis |{EscapeSymbol(tradition.SourceBasis)}| ^truth-status |{EscapeSymbol(tradition.TruthStatus)}|)}}");
            for (int ideaIndex = 0; ideaIndex < tradition.Ideas.Count; ideaIndex++)
            {
                ReligiousIdea idea = tradition.Ideas[ideaIndex];
                string ideaLti = StableLti($"idea_{ideaIndex}", tradition.Key);
                Execute($"smem --add {{({ideaLti} ^type religious-cosmological-idea ^tradition |{EscapeSymbol(tradition.Key)}| ^topic |{EscapeSymbol(idea.Topic)}| ^summary |{EscapeSymbol(idea.Summary)}| ^truth-status |{EscapeSymbol(YalaReligiousKnowledgeCatalog.TruthStatus)}|)}}");
            }
        }

        foreach (YalaCosmicChoiceDefinition choice in YalaCosmicChoiceCatalog.Choices)
        {
            string choiceLti = StableLti("choice", choice.Key);
            Execute($"smem --add {{({choiceLti} ^type cosmic-possibility ^key |{EscapeSymbol(choice.Key)}| ^domain |{EscapeSymbol(choice.Domain)}| ^action |{EscapeSymbol(choice.Action)}| ^meaning |{EscapeSymbol(choice.Meaning)}| ^status |{EscapeSymbol(choice.Status)}|)}}");
        }
    }

    public void SeedTemporalSemanticMemory()
    {
        ThrowIfDisposed();
        Execute("smem --add {(@concept-time ^word time ^meaning |the temporal order Gaia brought into existence in response to Yala commanding Gaia to establish order|)}");
    }

    public void RememberClaimedContact(string claimedName)
    {
        if (string.IsNullOrWhiteSpace(claimedName)) return;
        ThrowIfDisposed();
        string safe = EscapeSymbol(claimedName.Trim());
        Execute($"smem --add {{(<contact> ^type unseen-contact ^claimed-name |{safe}|)}}");
    }

    public bool SemanticMemoryContainsClaimedContact(string claimedName)
    {
        if (string.IsNullOrWhiteSpace(claimedName)) return false;
        string safe = EscapeSymbol(claimedName.Trim());
        string result = Execute($"smem --query {{(<cue> ^type unseen-contact ^claimed-name |{safe}|)}} 1");
        return !result.Contains("No LTI", StringComparison.OrdinalIgnoreCase) &&
            result.Contains(claimedName.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public void RememberClaimedDefinition(string word, string meaning)
    {
        if (string.IsNullOrWhiteSpace(word) || string.IsNullOrWhiteSpace(meaning)) return;
        ThrowIfDisposed();
        string safeWord = EscapeSymbol(word.Trim());
        string safeMeaning = EscapeSymbol(meaning.Trim());
        Execute($"smem --add {{(<definition> ^type speaker-definition-claim ^word |{safeWord}| ^meaning |{safeMeaning}|)}}");
    }

    public SoarMemoryDiagnostics GetMemoryDiagnostics()
    {
        ThrowIfDisposed();
        string smem = Execute("smem --stats");
        string epmem = Execute("epmem --stats");
        return new SoarMemoryDiagnostics(
            ParseStat(smem, "Nodes"),
            ParseStat(smem, "Edges"),
            ParseStat(epmem, "Time"),
            smem,
            epmem);
    }

    public string Execute(string command)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        ThrowIfDisposed();
        return Convert.ToString(Invoke(_agent, "ExecuteCommandLine", command, false, false), CultureInfo.InvariantCulture) ?? string.Empty;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { Invoke(_kernel, "Shutdown"); }
        catch { }
        GC.SuppressFinalize(this);
    }

    private void ConfigureLongTermMemory(SoarMemoryPaths? memoryPaths)
    {
        if (memoryPaths is not null)
        {
            Directory.CreateDirectory(memoryPaths.Directory);
            ExecuteBeforeReady("smem --set database file");
            ExecuteBeforeReady($"smem --set path \"{EscapeTclString(memoryPaths.SemanticDatabase)}\"");
            ExecuteBeforeReady("smem --set append on");
            ExecuteBeforeReady("smem --set learning on");
            ExecuteBeforeReady("smem --init");
            ExecuteBeforeReady("epmem --set database file");
            ExecuteBeforeReady($"epmem --set path \"{EscapeTclString(memoryPaths.EpisodicDatabase)}\"");
            ExecuteBeforeReady("epmem --set append on");
        }
        else
        {
            ExecuteBeforeReady("smem --set database memory");
            ExecuteBeforeReady("smem --set learning on");
            ExecuteBeforeReady("epmem --set database memory");
        }

        ExecuteBeforeReady("epmem --set learning on");
        ExecuteBeforeReady("epmem --set trigger dc");
        ExecuteBeforeReady("epmem --init");
    }

    private string ExecuteBeforeReady(string command) =>
        Convert.ToString(Invoke(_agent, "ExecuteCommandLine", command, false, false), CultureInfo.InvariantCulture) ?? string.Empty;

    private long GetDecisionCycleCounter() =>
        Convert.ToInt64(Invoke(_agent, "GetDecisionCycleCounter") ?? 0L, CultureInfo.InvariantCulture);

    private nint ResolveNativeLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName.Contains("CSharp_sml_ClientInterface", StringComparison.OrdinalIgnoreCase)) return _bridgeNativeHandle;
        if (libraryName.Contains("Soar", StringComparison.OrdinalIgnoreCase)) return _kernelNativeHandle;
        return nint.Zero;
    }

    private void CreateString(object parent, string attribute, string value)
    {
        object? wme = Invoke(_agent, "CreateStringWME", parent, attribute, value);
        if (wme is null) throw new InvalidOperationException($"Soar could not create input '{attribute}'.");
    }

    private void CreateInt(object parent, string attribute, long value)
    {
        object? wme = Invoke(_agent, "CreateIntWME", parent, attribute, value);
        if (wme is null) throw new InvalidOperationException($"Soar could not create numeric input '{attribute}'.");
    }

    private static string Parameter(object command, string name, string fallback)
    {
        object? value = Invoke(command, "GetParameterValue", name);
        string? text = Convert.ToString(value, CultureInfo.InvariantCulture);
        return string.IsNullOrWhiteSpace(text) ? fallback : text;
    }

    private static long ParseStat(string text, string label)
    {
        Match match = Regex.Match(text, $@"(?im)^\s*{Regex.Escape(label)}:\s*(\d+)");
        return match.Success && long.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long value)
            ? value
            : -1;
    }

    private static string StableLti(string prefix, string value)
    {
        string safePrefix = Regex.Replace(prefix, "[^A-Za-z0-9_]", "_");
        string safeValue = Regex.Replace(value, "[^A-Za-z0-9_]", "_");
        return $"@{safePrefix}_{safeValue}";
    }

    private static string EscapeSymbol(string value) => value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("|", "\\|", StringComparison.Ordinal);
    private static string EscapeTclString(string value) => value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
    private static string Clean(string? value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    private static string YesNo(bool value) => value ? "yes" : "no";

    private static object? InvokeStatic(Type type, string methodName, params object?[] args) =>
        FindMethod(type, methodName, isStatic: true, args).Invoke(null, args);

    private static object? Invoke(object target, string methodName, params object?[] args) =>
        FindMethod(target.GetType(), methodName, isStatic: false, args).Invoke(target, args);

    private static MethodInfo FindMethod(Type type, string methodName, bool isStatic, object?[] args)
    {
        BindingFlags flags = BindingFlags.Public | (isStatic ? BindingFlags.Static : BindingFlags.Instance);
        MethodInfo[] candidates = type.GetMethods(flags)
            .Where(method => method.Name.Equals(methodName, StringComparison.Ordinal) && method.GetParameters().Length == args.Length)
            .ToArray();
        foreach (MethodInfo candidate in candidates)
        {
            ParameterInfo[] parameters = candidate.GetParameters();
            bool compatible = true;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (args[i] is null) continue;
                if (!parameters[i].ParameterType.IsInstanceOfType(args[i]) && !CanConvert(args[i]!.GetType(), parameters[i].ParameterType))
                {
                    compatible = false;
                    break;
                }
            }
            if (compatible) return candidate;
        }
        throw new MissingMethodException(type.FullName, $"{methodName}/{args.Length}");
    }

    private static bool CanConvert(Type source, Type target) =>
        (source == typeof(int) && target == typeof(long)) ||
        (source == typeof(int) && target == typeof(uint)) ||
        (source == typeof(long) && target == typeof(int)) ||
        (source == typeof(long) && target == typeof(long));

    private static bool TryInvokeBool(object target, string methodName, out bool value)
    {
        try
        {
            object? result = Invoke(target, methodName);
            if (result is bool boolean) { value = boolean; return true; }
        }
        catch (MissingMethodException) { }
        value = false;
        return false;
    }

    private static string? TryInvokeString(object target, string methodName)
    {
        try { return Convert.ToString(Invoke(target, methodName), CultureInfo.InvariantCulture); }
        catch (MissingMethodException) { return null; }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(SoarKernelHost));
    }
}

public sealed record SoarMemoryPaths(string Directory, string SemanticDatabase, string EpisodicDatabase)
{
    public static SoarMemoryPaths FromSavePath(string savePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(savePath);
        string full = Path.GetFullPath(savePath);
        string parent = Path.GetDirectoryName(full) ?? throw new InvalidOperationException("Save path has no parent directory.");
        // v0.0.26 starts a fresh Brain Slice 9 integrated mind alongside save_v8. Earlier Soar databases remain untouched.
        string directory = Path.Combine(parent, "yala_soar_v0_0_26");
        return new SoarMemoryPaths(
            directory,
            Path.Combine(directory, "semantic.sqlite"),
            Path.Combine(directory, "episodic.sqlite"));
    }
}

public sealed record SoarMemoryDiagnostics(
    long SemanticNodes,
    long SemanticEdges,
    long EpisodicTime,
    string SemanticStats,
    string EpisodicStats);
