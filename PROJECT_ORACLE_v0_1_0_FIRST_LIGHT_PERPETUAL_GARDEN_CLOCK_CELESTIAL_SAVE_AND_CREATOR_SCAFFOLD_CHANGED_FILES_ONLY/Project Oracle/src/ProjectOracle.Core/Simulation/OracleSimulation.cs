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
            State,
            Ledger.WorldRecords.Concat(Ledger.CreatorRecords).OrderBy(record => record.Sequence).ToArray(),
            _interventions.ToArray());
    }

    private static WorldState CreateInitialState(ulong seed)
    {
        EntityId gardenId = new("place:garden:0001");
        EntityId yalaId = new("being:yala:0001");
        EntityId adamId = new("being:adam:0001");

        return new WorldState(
            seed,
            WorldMilliseconds: 0,
            new GardenState(gardenId, "the Garden", BoundaryOpen: false),
            new YalaState(
                yalaId,
                TrueName: "Yala",
                WorldTitle: "the Oracle",
                KnowsOfCreators: true,
                KnowsFutureLanguageMandate: true),
            new AdamState(adamId, "Adam", gardenId, IsConfinedToGarden: true),
            new SparkState(
                adamId,
                CanBeReadByYala: false,
                CanBeRewrittenByYala: false,
                CreatorDescription: "A protected source of genuine choice placed by the Creators."));
    }

    private void RecordGenesis()
    {
        Ledger.RecordWorld(0, "GENESIS", "Adam awoke in the Garden. The Oracle watched in silence.");
        Ledger.RecordWorld(0, "BOUNDARY", "The Garden boundary was closed.");

        Ledger.RecordCreator(0, "GENESIS", $"Run created with seed {State.Seed}.");
        Ledger.RecordCreator(0, "AUTHORITY", "The Creators made Yala. Yala formed the world and Adam's body and ordinary mind.");
        Ledger.RecordCreator(0, "SPARK", State.AdamSpark.CreatorDescription);
        Ledger.RecordCreator(0, "MANDATE", "Yala knows the Creators will one day give her a new language to learn and teach. The language has not been supplied.");
    }
}
