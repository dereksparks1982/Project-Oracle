using ProjectOracle.Audit;
using ProjectOracle.Domain;
using ProjectOracle.Interventions;
using ProjectOracle.Persistence;

namespace ProjectOracle.Simulation;

public sealed class OracleSimulation
{
    private readonly List<CreatorIntervention> _interventions = [];
    private long _nextInterventionId = 1;

    private OracleSimulation(ulong seed, long realUnixMilliseconds)
    {
        Clock = new PersistentWorldClock(0, realUnixMilliseconds);
        Random = new DeterministicRandom(seed);
        Ledger = new AuditLedger();
        State = CreateInitialState(seed);
        RecordGenesis();
    }

    private OracleSimulation(OracleSaveSnapshot snapshot)
    {
        Clock = new PersistentWorldClock(
            snapshot.WorldMilliseconds,
            snapshot.LastRealUnixMilliseconds,
            snapshot.CatchUpRuns,
            snapshot.LastOfflineElapsedRealMilliseconds);
        Random = new DeterministicRandom(snapshot.Seed);
        Ledger = new AuditLedger(snapshot.Records);
        State = snapshot.World with { WorldMilliseconds = snapshot.WorldMilliseconds };
        _interventions.AddRange(snapshot.Interventions.OrderBy(intervention => intervention.Id));
        _nextInterventionId = _interventions.Count == 0 ? 1 : checked(_interventions[^1].Id + 1);
    }

    public PersistentWorldClock Clock { get; }

    public DeterministicRandom Random { get; }

    public AuditLedger Ledger { get; }

    public WorldState State { get; private set; }

    public IReadOnlyList<CreatorIntervention> Interventions => _interventions.AsReadOnly();

    public static OracleSimulation Start(ulong seed, long realUnixMilliseconds) => new(seed, realUnixMilliseconds);

    public static OracleSimulation Restore(OracleSaveSnapshot snapshot, long currentRealUnixMilliseconds)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        OracleSimulation simulation = new(snapshot);
        simulation.SynchroniseClock(currentRealUnixMilliseconds, offlineCatchUp: true);
        return simulation;
    }

    public ClockAdvance SynchroniseClock(long currentRealUnixMilliseconds, bool offlineCatchUp = false)
    {
        ClockAdvance advance = Clock.Synchronise(currentRealUnixMilliseconds, offlineCatchUp);
        State = State with { WorldMilliseconds = Clock.WorldMilliseconds };

        if (advance.ElapsedRealMilliseconds > 0)
        {
            string mode = offlineCatchUp ? "offline catch-up" : "live real-time advance";
            Ledger.RecordCreator(
                Clock.WorldMilliseconds,
                "TIME",
                $"Applied {mode}: {advance.ElapsedRealMilliseconds} real millisecond(s) became {advance.ElapsedWorldMilliseconds} world millisecond(s).");
        }

        if (advance.BackwardClockDetected)
        {
            Ledger.RecordCreator(
                Clock.WorldMilliseconds,
                "CLOCK WARNING",
                "The system clock moved backwards. Project Oracle refused to rewind the world.");
        }

        return advance;
    }

    public CreatorIntervention QueueVesselMessage(string vessel, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vessel);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        CreatorIntervention intervention = new(
            _nextInterventionId++,
            Clock.WorldMilliseconds,
            vessel.Trim(),
            message.Trim(),
            ContaminatesExperiment: true,
            InterventionStatus.Queued);

        _interventions.Add(intervention);
        Ledger.RecordCreator(
            Clock.WorldMilliseconds,
            "INTERVENTION",
            $"Creator intervention {intervention.Id} queued through {intervention.Vessel}. The experiment is contaminated from this point.");
        Ledger.RecordWorld(
            Clock.WorldMilliseconds,
            "SIGN",
            $"A {intervention.Vessel} approached Adam. It has not spoken yet.");

        return intervention;
    }

    public void AddressChannel(string channelKey, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channelKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        AddressChannelState channel = State.AddressChannels.FirstOrDefault(candidate =>
            candidate.Key.Equals(channelKey.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Address channel is not recognised: {channelKey}");

        Ledger.RecordCreator(
            Clock.WorldMilliseconds,
            "DIRECT ADDRESS",
            $"The Creators addressed {channel.TargetName} at {channel.Prompt}: \"{message.Trim()}\". The address contaminates the experiment.");

        if (channel.Key.Equals("adam", StringComparison.OrdinalIgnoreCase))
        {
            Ledger.RecordWorld(
                Clock.WorldMilliseconds,
                "VOICE",
                "Adam heard a direct address from beyond his ordinary world. His response has not been decided.");
        }
    }

    public LivingKindState? PresentNextLivingKindToAdam(string presenter)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(presenter);

        int index = State.LivingKinds.ToList().FindIndex(kind => !kind.NamedByAdam);
        if (index < 0)
        {
            return null;
        }

        LivingKindState current = State.LivingKinds[index];
        LivingKindState named = current with
        {
            PresentedToAdam = true,
            NamedByAdam = true,
            AdamName = CreateAdamName(current, index)
        };

        List<LivingKindState> kinds = State.LivingKinds.ToList();
        kinds[index] = named;
        State = State with
        {
            LivingKinds = kinds,
            NamingMandate = State.NamingMandate with
            {
                PresentedCount = kinds.Count(kind => kind.PresentedToAdam),
                NamedCount = kinds.Count(kind => kind.NamedByAdam),
                SuitableMateFound = kinds.Any(kind => kind.SuitableMate)
            }
        };

        Ledger.RecordWorld(
            Clock.WorldMilliseconds,
            "NAMING",
            $"{presenter.Trim()} presented a living kind to Adam. Adam named it {named.AdamName} and found no suitable mate.");
        Ledger.RecordCreator(
            Clock.WorldMilliseconds,
            "NAMING",
            $"Adam named {named.Id} ({named.AncientKind}) as {named.AdamName}. Suitable mate: {named.SuitableMate}.");

        return named;
    }

    public OracleSaveSnapshot CreateSnapshot(long savedAtUnixMilliseconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(savedAtUnixMilliseconds);
        return new OracleSaveSnapshot(
            OracleSaveStore.SaveFormat,
            OracleSaveStore.CurrentSchemaVersion,
            ProjectVersion.Number,
            savedAtUnixMilliseconds,
            State.Seed,
            Clock.WorldMilliseconds,
            Clock.LastRealUnixMilliseconds,
            Clock.CatchUpRuns,
            Clock.LastOfflineElapsedRealMilliseconds,
            WorldDefaults.Normalise(State),
            Ledger.WorldRecords.Concat(Ledger.CreatorRecords).OrderBy(record => record.Sequence).ToArray(),
            _interventions.ToArray());
    }

    private static WorldState CreateInitialState(ulong seed) => WorldDefaults.CreateInitialState(seed);

    private void RecordGenesis()
    {
        Ledger.RecordWorld(0, "GENESIS", "The Garden was formed and filled with ancient living kinds.");
        Ledger.RecordWorld(0, "GENESIS", "Adam awoke in the Garden. The Oracle watched in silence.");
        Ledger.RecordWorld(0, "MANDATE", "Adam was given the task of naming the living kinds and finding whether any was a suitable mate.");
        Ledger.RecordWorld(0, "BOUNDARY", "The Garden boundary was closed.");

        Ledger.RecordCreator(0, "GENESIS", $"World Seed: {State.Seed}.");
        Ledger.RecordCreator(0, "AUTHORITY", "The Creators made Yala. Yala formed the Garden, Gaia, the celestial governors, the living kinds, Adam's body, and Adam's ordinary mind.");
        Ledger.RecordCreator(0, "AUTHORITY", "Direct address channels are appointed: F1 Oracle, F2 Gaia, F3 Adam, F4 Sun, F5 Moon.");
        Ledger.RecordCreator(0, "NATURAL COURSE", State.NaturalCourse.RuleText);
        Ledger.RecordCreator(0, "SPARK", State.AdamSpark.CreatorDescription);
        Ledger.RecordCreator(0, "MANDATE", "Yala knows the Creators will one day give her a new language to learn and teach. The language has not been supplied.");
    }

    private static string CreateAdamName(LivingKindState kind, int index)
    {
        string[] firstWords = ["ground", "wing", "water", "root", "hand", "horn", "night", "reed", "stone"];
        string[] secondWords = ["walker", "caller", "glider", "crawler", "climber", "grazer", "hunter", "singer", "sleeper"];
        int kindOffset = kind.Id.Value.Sum(character => (int)character);
        return $"{firstWords[index % firstWords.Length]}-{secondWords[kindOffset % secondWords.Length]}";
    }
}
