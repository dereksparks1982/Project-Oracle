namespace ProjectOracle.Interventions;

public sealed record OracleIntervention(
    long Id,
    long RequestedAtTick,
    string Vessel,
    string Message,
    bool ContaminatesExperiment,
    InterventionStatus Status);

public enum InterventionStatus
{
    Queued,
    OfferedChoice
}
