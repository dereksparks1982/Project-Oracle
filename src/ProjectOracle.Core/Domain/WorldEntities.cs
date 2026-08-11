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
    string YalaLocation);

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
    IReadOnlyList<YalaLearnedLexemeState>? LearnedLexicon = null);

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
    YalaCognitionState? YalaCognition = null);
