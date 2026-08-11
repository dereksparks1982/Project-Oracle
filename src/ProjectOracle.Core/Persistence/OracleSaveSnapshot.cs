using ProjectOracle.Audit;
using ProjectOracle.Brain;
using ProjectOracle.Domain;
using ProjectOracle.Events;
using ProjectOracle.Interventions;
using ProjectOracle.Observation;

namespace ProjectOracle.Persistence;

public sealed record OracleSaveSnapshot(
    string Format,
    int SchemaVersion,
    string ProjectVersion,
    long SavedAtUnixMilliseconds,
    ulong Seed,
    long WorldMilliseconds,
    long LastRealUnixMilliseconds,
    int CatchUpRuns,
    long LastOfflineElapsedRealMilliseconds,
    WorldState World,
    IReadOnlyList<OracleRecord> Records,
    IReadOnlyList<CreatorIntervention> Interventions,
    IReadOnlyList<ScheduledWorldEvent>? ScheduledEvents = null,
    IReadOnlyList<OfferedChoiceState>? OfferedChoices = null,
    IReadOnlyList<ReasonedPlanState>? ReasonedPlans = null,
    IReadOnlyList<ObservationState>? Observations = null,
    IReadOnlyList<AttentionState>? AttentionStates = null);
