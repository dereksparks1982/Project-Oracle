using System.Text.Json.Serialization;

namespace ProjectOracle.Domain;

public sealed record YalaState(
    EntityId Id,
    string TrueName,
    string WorldTitle,
    bool KnowsOfOracle = false,
    bool MayClaimSupremeCreator = true,
    string AuthorityCaveat = "",
    string Location = "the Void",
    string Sex = "male and female");

public sealed record CosmicState(
    bool GaiaCreated,
    bool TimeCreated,
    bool LowerWorldEstablished,
    bool GardenEstablished,
    string YalaLocation,
    IReadOnlyList<YalaEstablishedCosmicChoiceState>? EstablishedChoices = null);

public sealed record YalaEstablishedCosmicChoiceState(
    string Key,
    string Domain,
    string Action,
    string Meaning,
    long Decision,
    string Source = "Yala autonomous choice");


public sealed record OracleEstablishedLawState(
    string Key,
    string Domain,
    string Name,
    string EstablishedBy,
    int RequiredAuthorityUnits,
    long Decision,
    string Description);

public sealed record OracleLawExperimentState(
    long Sequence,
    string LawKey,
    string RunBy,
    string InitialState,
    string ResultSummary,
    bool ChangedWorld);

public sealed record OracleEmergentLawState(
    IReadOnlyList<OracleEstablishedLawState>? EstablishedLaws = null,
    IReadOnlyList<OracleLawExperimentState>? Experiments = null);

public sealed record YalaContactMemory(
    string ClaimedName,
    int EncounterCount,
    long FirstEncounterDecision,
    long LastEncounterDecision,
    string? LastMessage);

public sealed record YalaBeliefState(
    string Proposition,
    string Status,
    double Confidence,
    string Source,
    long FirstSeenDecision,
    long LastConsideredDecision);

public sealed record YalaEpisodeState(
    long Sequence,
    string Kind,
    string Summary,
    string? SpeakerClaim = null,
    string? Message = null,
    string? Response = null);

public sealed record YalaDriveState(
    int Curiosity,
    int Caution,
    int Authority,
    int Companionship,
    int Comfort,
    int Uncertainty);

public sealed record YalaActionMemoryState(
    string Action,
    string Object,
    string Outcome,
    bool Completed,
    long Decision);

public sealed record YalaKnowledgeGapState(
    string Kind,
    string Subject,
    string Detail,
    long FirstSeenDecision,
    long LastSeenDecision);

public sealed record YalaLearnedLexemeState(
    string Word,
    string PartOfSpeech,
    string ProposedMeaning,
    string Status,
    string Source,
    double Confidence,
    long FirstSeenDecision,
    long LastSeenDecision);


public sealed record YalaDialogueTurnState(
    long Sequence,
    string Speaker,
    string Message,
    string Topic,
    string? Subject,
    string? Verb,
    string? Object,
    string? Response,
    string TemporalState,
    long? WorldMilliseconds);

public sealed record YalaRelationshipState(
    string Subject,
    string Relation,
    string Object,
    string Status,
    string Source,
    double Confidence,
    long FirstSeenDecision,
    long LastConsideredDecision);

public sealed record YalaQuestionState(
    long Id,
    string Text,
    string Subject,
    string Reason,
    int Priority,
    bool Asked,
    long CreatedDecision,
    long? AskedDecision = null);

public sealed record YalaTemporalEventState(
    long Sequence,
    string Key,
    string Subject,
    string Action,
    string Object,
    string Summary,
    string TemporalState,
    long? WorldMilliseconds,
    long? Year = null,
    int? Month = null,
    int? Day = null,
    int? Hour = null,
    int? Minute = null,
    int? Second = null,
    string? CauseKey = null,
    string Source = "remembered");

public sealed record YalaGoalState(
    string Goal,
    string Reason,
    string Status,
    int Priority,
    string Source,
    long FirstSeenDecision,
    long LastConsideredDecision);


public sealed record YalaConcernState(
    string Key,
    string Subject,
    string Summary,
    string Status,
    int Priority,
    string Source,
    long FirstSeenDecision,
    long LastConsideredDecision);

public sealed record YalaAppraisalState(
    long Sequence,
    string Trigger,
    string Primary,
    string Secondary,
    string Summary,
    int Salience,
    int Threat,
    int Opportunity,
    int Uncertainty,
    string Source);

public sealed record YalaHypothesisState(
    string Key,
    string Proposition,
    string Status,
    double Confidence,
    string Reason,
    long FirstSeenDecision,
    long LastConsideredDecision);

public sealed record YalaEntityModelState(
    string EntityKey,
    string IdentityStatus,
    string LocationStatus,
    string IntentStatus,
    string CapabilityStatus,
    string TrustStatus,
    int ThreatPotential,
    int HelpPotential,
    long LastUpdatedDecision);

public sealed record YalaReflectionState(
    long Sequence,
    string ConcernKey,
    string Summary,
    string Result,
    long Decision);


public sealed record YalaPlanStepState(
    int Order,
    string Action,
    string Rationale,
    string Status);

public sealed record YalaPlanState(
    string Key,
    string Goal,
    string ConcernKey,
    string Status,
    int Priority,
    IReadOnlyList<YalaPlanStepState> Steps,
    int CurrentStepOrder,
    string LastObservation,
    string RevisionReason,
    long FirstSeenDecision,
    long LastUpdatedDecision);

public sealed record YalaInvestigationState(
    string Key,
    string Question,
    string ConcernKey,
    string Status,
    int Priority,
    IReadOnlyList<string> EvidenceFor,
    IReadOnlyList<string> EvidenceAgainst,
    string NextTest,
    string CurrentConclusion,
    double Confidence,
    long FirstSeenDecision,
    long LastUpdatedDecision);

public sealed record YalaCounterfactualState(
    long Sequence,
    string Subject,
    string Option,
    string PossibleBenefit,
    string PossibleRisk,
    int Uncertainty,
    string Source,
    long Decision);

public sealed record YalaDecisionSnapshotState(
    string ActiveConcern,
    int ActiveConcernPriority,
    string ActivePlan,
    string ActivePlanStep,
    string ActiveInvestigation,
    string SpeakerTrust,
    string SpeakerIntent,
    string Appraisal,
    IReadOnlyList<string> ActiveGoals,
    IReadOnlyList<string> TopHypotheses);

public sealed record YalaDecisionTraceState(
    long Sequence,
    string Trigger,
    string? SpeakerMessage,
    string SelectedAction,
    string Rationale,
    string? PlanKey,
    long WorldMilliseconds,
    string TemporalState,
    YalaDecisionSnapshotState Before,
    YalaDecisionSnapshotState After);

public sealed record YalaCognitionState(
    long DecisionCount,
    long LastDecisionRealUnixMilliseconds,
    string? LastAction,
    string? LastResult,
    IReadOnlyList<string> Memory,
    IReadOnlyList<YalaContactMemory>? Contacts = null,
    IReadOnlyList<YalaBeliefState>? Beliefs = null,
    IReadOnlyList<YalaEpisodeState>? Episodes = null,
    YalaDriveState? Drives = null,
    long ConversationCount = 0,
    string? LastSpeakerClaim = null,
    IReadOnlyList<YalaActionMemoryState>? ActionMemory = null,
    IReadOnlyList<YalaKnowledgeGapState>? KnowledgeGaps = null,
    IReadOnlyList<YalaLearnedLexemeState>? LearnedLexicon = null,
    IReadOnlyList<YalaDialogueTurnState>? Dialogue = null,
    IReadOnlyList<YalaRelationshipState>? Relationships = null,
    IReadOnlyList<YalaQuestionState>? Questions = null,
    IReadOnlyList<YalaTemporalEventState>? TemporalEvents = null,
    IReadOnlyList<YalaGoalState>? Goals = null,
    IReadOnlyList<YalaConcernState>? Concerns = null,
    IReadOnlyList<YalaAppraisalState>? Appraisals = null,
    IReadOnlyList<YalaHypothesisState>? Hypotheses = null,
    IReadOnlyList<YalaEntityModelState>? EntityModels = null,
    IReadOnlyList<YalaReflectionState>? Reflections = null,
    IReadOnlyList<YalaPlanState>? Plans = null,
    IReadOnlyList<YalaInvestigationState>? Investigations = null,
    IReadOnlyList<YalaCounterfactualState>? Counterfactuals = null,
    IReadOnlyList<YalaDecisionTraceState>? DecisionTrace = null,
    string? PendingAutonomousUtterance = null);

public sealed record AdamState(
    EntityId Id,
    string Name,
    EntityId LocationId,
    bool IsConfinedToGarden);

public sealed record SparkState(
    EntityId BearerId,
    bool CanBeReadByYala,
    bool CanBeRewrittenByYala,
    [property: JsonPropertyName("oracle_description")] string OracleDescription);

public sealed record GardenState(
    EntityId Id,
    string Name,
    bool BoundaryOpen);

public sealed record DirectCallTargetState(
    string Key,
    string Prompt,
    string TargetName,
    string AuthoritySummary,
    bool ReceivesDirectCall);

public sealed record LivingKindState(
    EntityId Id,
    string AncientKind,
    string Domain,
    string Form,
    bool PresentedToAdam,
    bool NamedByAdam,
    string? AdamName,
    bool SuitableMate);

public sealed record CreationPowerState(
    int Order,
    EntityId Id,
    string Name,
    string Domain,
    string AuthoritySummary,
    bool ReceivesDirectCall,
    bool Exists = true);

public sealed record NamingMandateState(
    bool Active,
    int TotalLivingKinds,
    int PresentedCount,
    int NamedCount,
    bool SuitableMateFound,
    string MandateText);

public sealed record NaturalCourseState(
    bool Active,
    string RuleText);

public sealed record WorldState(
    ulong Seed,
    long WorldMilliseconds,
    GardenState? Garden,
    YalaState Yala,
    AdamState? Adam,
    SparkState? AdamSpark,
    IReadOnlyList<CreationPowerState> CreationPowers,
    [property: JsonPropertyName("DirectCallTargets")]
    IReadOnlyList<DirectCallTargetState> DirectCallTargets,
    IReadOnlyList<LivingKindState> LivingKinds,
    NamingMandateState? NamingMandate,
    NaturalCourseState NaturalCourse,
    CosmicState? Cosmic = null,
    YalaCognitionState? YalaCognition = null,
    OracleEmergentLawState? EmergentLaws = null);
