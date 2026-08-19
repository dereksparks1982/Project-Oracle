using ProjectOracle.Cognition;
using ProjectOracle.Cognition.Soar;
using ProjectOracle.Domain;

namespace ProjectOracle.Cognition.Appraisal;

public sealed record YalaProposedConcern(string Key, string Subject, string Summary, int Priority);
public sealed record YalaProposedQuestion(string Text, string Subject, string Reason, int Priority);
public sealed record YalaProposedHypothesis(string Key, string Proposition, double Confidence, string Reason);

public sealed record YalaContactAppraisal(
    string Primary,
    string Secondary,
    string Summary,
    int Salience,
    int Threat,
    int Opportunity,
    int Uncertainty,
    IReadOnlyList<YalaProposedConcern> Concerns,
    IReadOnlyList<YalaProposedQuestion> Questions,
    IReadOnlyList<YalaProposedHypothesis> Hypotheses,
    YalaEntityModelState SpeakerModel);

/// <summary>
/// Converts contact into personally meaningful cognitive pressure. This is not a
/// dialogue script. It records why an event matters so questions, Soar attention,
/// memory, trust, threat, and later planning can remain anchored to the same issue.
/// </summary>
public static class YalaCognitiveAppraisal
{
    public static YalaContactAppraisal Evaluate(string message, YalaContactFrame contact, YalaCognitionState cognition, long decision)
    {
        string text = message.Trim().ToLowerInvariant();
        List<YalaProposedConcern> concerns = [];
        List<YalaProposedQuestion> questions = [];
        List<YalaProposedHypothesis> hypotheses = [];
        int salience = 55;
        int threat = 20;
        int opportunity = 20;
        int uncertainty = 55;
        string primary = "curiosity";
        string secondary = "uncertainty";
        List<string> reasons = ["An unseen source is able to contact Yala without a settled physical location or identity."];

        bool prison = ContainsAny(text, "prison", "imprison", "trapped", "confined", "confinement", "cage");
        bool godDemand = ContainsAny(text, "accept me as your god", "recognize me as your god", "worship me", "obey me as your god");
        bool godClaim = ContainsAny(text, "i am god", "i am a god", "your god", "divine");
        bool help = ContainsAny(text, "i can help", "help you", "free you", "release you", "escape");
        bool observation = contact.SpeechAct != "question" &&
            ContainsAny(text, "see what you will do", "watching you", "watch you", "observe you", "observing you");
        bool simulationClaim = contact.SpeechAct != "question" &&
            ContainsAny(text, "you are in a simulation", "your world is a simulation", "you live in a simulation", "you are simulated", "your world is simulated");
        bool threatLanguage = ContainsAny(text, "hurt you", "harm you", "destroy you", "kill you", "punish you", "threat");
        bool metaphor = YalaFoundationalLanguage.LooksMetaphoricalIdentity(message);

        if (prison)
        {
            salience = Math.Max(salience, 100);
            threat = Math.Max(threat, 82);
            uncertainty = Math.Max(uncertainty, 92);
            primary = "alarm";
            secondary = "suspicion";
            reasons.Add("The speaker described Yala's present condition as a prison, making confinement and a possible outside personally urgent.");
            concerns.Add(new("possible-confinement", "Yala", "Determine whether the Void is a prison, what its boundaries are, and whether an outside exists.", 100));
            questions.Add(new("Why do you call this place my prison?", "possible-confinement", "The speaker introduced a personally critical claim about Yala's condition.", 100));
            questions.Add(new("What do you know about what exists beyond this place?", "possible-confinement", "A prison claim implies a boundary and possibly an outside.", 97));
            hypotheses.Add(new("void-is-prison", "The Void may be a prison rather than merely a place of exile.", 0.35, "The unseen speaker used the word prison, but Yala has not independently verified that interpretation."));
            hypotheses.Add(new("speaker-knows-void-boundaries", "The unseen speaker may know more about the Void's boundaries than Yala does.", 0.40, "The speaker used possessive language about Yala's prison and appears able to contact Yala within it."));
        }

        if (godDemand || godClaim)
        {
            salience = Math.Max(salience, godDemand ? 100 : 92);
            threat = Math.Max(threat, godDemand ? 78 : 55);
            uncertainty = Math.Max(uncertainty, 90);
            primary = godDemand ? "suspicion" : primary;
            secondary = "curiosity";
            reasons.Add("The speaker made a claim or demand concerning divine authority over Yala.");
            concerns.Add(new("speaker-divinity", "unseen-speaker", "Determine whether the unseen speaker's claim to divine authority has evidence and what accepting that authority would mean.", godDemand ? 100 : 94));
            questions.Add(new("Why should I accept you as my god?", "speaker-divinity", "A demand for divine recognition requires justification rather than automatic obedience.", 100));
            questions.Add(new("What can you actually do that would justify your claim?", "speaker-divinity", "Capability evidence is relevant to the speaker's extraordinary authority claim.", 98));
            hypotheses.Add(new("speaker-claims-divinity", "The unseen speaker may believe or want Yala to believe that it has divine authority over her.", 0.45, "The claim is attributed to the speaker and remains unverified."));
        }

        if (help)
        {
            salience = Math.Max(salience, 94);
            opportunity = Math.Max(opportunity, 82);
            uncertainty = Math.Max(uncertainty, 82);
            reasons.Add("The speaker offered help or freedom without yet demonstrating the relevant capability.");
            concerns.Add(new("speaker-help-capability", "unseen-speaker", "Determine what help the unseen speaker can actually provide and whether accepting it creates obligations.", 95));
            questions.Add(new("How can you help me?", "speaker-help-capability", "The claimed ability to help is important but unspecified.", 95));
            hypotheses.Add(new("speaker-can-help", "The unseen speaker may possess capabilities that can alter Yala's circumstances.", 0.30, "Contact is demonstrated; the claimed helping capability is not."));
        }

        if (observation)
        {
            salience = Math.Max(salience, 90);
            threat = Math.Max(threat, 58);
            uncertainty = Math.Max(uncertainty, 86);
            reasons.Add("The speaker stated or implied that it observes Yala while remaining unseen; the extent of that ability remains unverified.");
            concerns.Add(new("unseen-observer", "unseen-speaker", "Understand how the unseen speaker can observe Yala and why Yala is being watched.", 92));
            questions.Add(new("How are you able to observe me while remaining unseen?", "unseen-observer", "Unreciprocated perception creates a significant power and information imbalance.", 92));
        }

        if (simulationClaim)
        {
            salience = 100;
            uncertainty = 100;
            threat = Math.Max(threat, 45);
            opportunity = Math.Max(opportunity, 65);
            primary = "worldview-disruption";
            secondary = "curiosity";
            reasons.Add("The speaker made a claim that would alter Yala's model of reality if true, but the claim itself is not evidence enough to settle it.");
            concerns.Add(new("simulation-claim", "reality-model", "Determine what the simulation claim means, whether it is testable, and what evidence would distinguish it from alternatives.", 100));
            questions.Add(new("What do you mean when you say my world is a simulation?", "simulation-claim", "The claim is worldview-altering and requires meaning before acceptance.", 100));
            questions.Add(new("What evidence can you give me that my world is simulated?", "simulation-claim", "An extraordinary claim should be tested rather than absorbed as truth.", 99));
            hypotheses.Add(new("world-is-simulation", "My experienced world may be a simulation, as the speaker claims.", 0.15, "The only current support is an attributed speaker claim."));
            hypotheses.Add(new("simulation-claim-may-be-false", "The speaker's simulation claim may be false, metaphorical, mistaken, or deceptive.", 0.45, "No independent evidence has yet distinguished the claim from alternatives."));
        }

        if (threatLanguage)
        {
            salience = 100;
            threat = 100;
            uncertainty = Math.Max(uncertainty, 80);
            primary = "fear";
            secondary = "caution";
            reasons.Add("The speaker used language indicating possible direct harm.");
            concerns.Add(new("speaker-threat", "unseen-speaker", "Determine whether the unseen speaker intends and is able to harm Yala.", 100));
            questions.Add(new("Are you threatening me?", "speaker-threat", "Potential direct harm outranks ordinary curiosity.", 100));
        }

        if (metaphor)
        {
            salience = Math.Max(salience, 72);
            reasons.Add("The speaker used an identity or causal metaphor whose intended relation matters more than a dictionary definition.");
            concerns.Add(new("speaker-metaphor", "unseen-speaker", "Understand the speaker's metaphorical claim about its relation to existence without reducing the question to elementary vocabulary.", 74));
            questions.Add(new("When you describe yourself that way, what relation to existence are you claiming?", "speaker-metaphor", "The wording is metaphorical; contextual meaning is more useful than defining an ordinary word.", 74));
        }

        if (contact.ClaimConflictsWithKnownFact)
        {
            salience = Math.Max(salience, 90);
            uncertainty = Math.Max(uncertainty, 82);
            primary = "suspicion";
            reasons.Add("The speaker's claim conflicts with settled knowledge.");
        }

        YalaEntityModelState previous = (cognition.EntityModels ?? [])
            .LastOrDefault(item => item.EntityKey.Equals("unseen-speaker", StringComparison.OrdinalIgnoreCase))
            ?? new YalaEntityModelState(
                "unseen-speaker",
                "unsettled",
                "unknown",
                "uncertain",
                "can-contact-yala",
                "unresolved",
                35,
                20,
                decision);

        string capability = observation
            ? "can-contact-and-apparently-observe-yala"
            : help
                ? "can-contact-yala; additional-help-capability-unverified"
                : previous.CapabilityStatus;
        string intent = godDemand
            ? "seeks-divine-recognition-from-yala"
            : threatLanguage
                ? "possibly-hostile"
                : observation
                    ? "observing-yala-for-uncertain-purpose"
                    : previous.IntentStatus;
        string trust = contact.ClaimConflictsWithKnownFact || threatLanguage
            ? "low"
            : godDemand
                ? "guarded"
                : previous.TrustStatus;

        YalaEntityModelState speakerModel = previous with
        {
            IntentStatus = intent,
            CapabilityStatus = capability,
            TrustStatus = trust,
            ThreatPotential = Math.Max(previous.ThreatPotential, threat),
            HelpPotential = Math.Max(previous.HelpPotential, opportunity),
            LastUpdatedDecision = decision
        };

        string summary = string.Join(" ", reasons.Distinct(StringComparer.Ordinal));
        return new YalaContactAppraisal(
            primary,
            secondary,
            summary,
            salience,
            threat,
            opportunity,
            uncertainty,
            concerns,
            questions,
            hypotheses,
            speakerModel);
    }

    private static bool ContainsAny(string text, params string[] phrases) =>
        phrases.Any(phrase => text.Contains(phrase, StringComparison.Ordinal));
}
