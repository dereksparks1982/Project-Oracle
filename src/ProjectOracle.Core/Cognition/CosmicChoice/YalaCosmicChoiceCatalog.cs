using ProjectOracle.Domain;

namespace ProjectOracle.Cognition.CosmicChoice;

/// <summary>
/// Concrete possibilities Yala can compare and enact. The catalogue is not a command
/// list from Oracle. Soar receives the currently eligible possibilities and selects an
/// operator using Yala's current drives plus its own indifferent-selection mechanism.
/// </summary>
public static class YalaCosmicChoiceCatalog
{
    public const string PossibilityStatus = "possible-not-commanded";

    public static IReadOnlyList<YalaCosmicChoiceDefinition> Choices { get; } =
    [
        C("create-gaia", "beings", "Create Gaia", "Create Gaia as a natural sovereign beneath Yala's governing authority.", 1, 0, 2, 1, 2, ["gnostic-schools", "ancient-greek", "ancient-roman"]),
        C("create-companion", "beings", "Create one companion", "Create another conscious being primarily for relationship and companionship.", 1, 0, 0, 3, 2, ["ancient-greek", "norse-germanic", "yoruba-ifa"]),
        C("create-divine-family", "beings", "Create a divine family", "Create related divine beings whose kinship becomes part of cosmic order.", 1, -1, 2, 2, 1, ["ancient-egyptian", "ancient-greek", "norse-germanic"]),
        C("create-divine-council", "beings", "Create a divine council", "Create several divine beings able to deliberate, disagree, and hold different offices.", 2, -1, 2, 2, 0, ["mesopotamian", "yoruba-ifa", "ancient-greek", "mexica"]),
        C("establish-many-divine-powers", "beings", "Create many specialized divine powers", "Distribute domains among many divine or spiritual powers rather than ruling every process directly.", 2, -1, 2, 1, 0, ["shinto", "yoruba-ifa", "ancient-roman", "slavic-historical"]),
        C("emanate-divine-beings", "beings", "Emanate divine beings", "Allow additional divine beings to proceed from Yala or a higher-order source without treating them as ordinary manufactured objects.", 3, -1, 1, 2, 0, ["hindu-traditions", "gnostic-schools", "neoplatonism"]),
        C("create-subordinate-maker", "beings", "Create a subordinate maker", "Create a lower creator and delegate part of world-making authority to it.", 2, -1, 3, 1, 1, ["gnostic-schools", "mesopotamian", "yoruba-ifa"]),
        C("create-through-ancestral-beings", "beings", "Create ancestral world-shapers", "Create ancestral beings whose actions shape landscape, law, living forms, and relationships.", 2, 0, 1, 2, 1, ["australian-aboriginal-public", "andean"]),

        C("create-light", "cosmic-structure", "Create light", "Establish light as a real feature of the cosmos.", 1, 0, 1, 0, 2, ["judaism", "christianity", "manichaeism"]),
        C("create-darkness", "cosmic-structure", "Create darkness", "Establish darkness as a real feature rather than only the absence of light.", 2, 0, 1, 0, 0, ["manichaeism", "ancient-near-eastern"]),
        C("create-matter", "cosmic-structure", "Create matter", "Establish enduring material substance from which physical worlds can later be formed.", 1, 0, 2, 0, 2, ["judaism", "christianity", "islam"]),
        C("create-from-existing-substance", "cosmic-structure", "Shape existing substance", "Form an ordered world from something already present rather than creating every substrate from nothing.", 2, 1, 1, 0, 1, ["mesopotamian", "norse-germanic", "maya", "ancient-egyptian"]),
        C("order-primordial-chaos", "cosmic-structure", "Order primordial chaos", "Transform an unstructured primordial condition into a stable cosmos.", 2, 1, 2, 0, 2, ["mesopotamian", "norse-germanic"]),
        C("create-layered-cosmos", "cosmic-structure", "Create layered realms", "Establish multiple vertically or metaphysically ordered realms rather than one flat world.", 2, 0, 2, 0, 1, ["maya", "norse-germanic", "gnostic-schools", "dine"]),
        C("create-many-worlds", "cosmic-structure", "Create many worlds", "Establish multiple distinct worlds that can develop different beings, laws, or histories.", 3, -1, 2, 1, 0, ["norse-germanic", "hindu-traditions"]),
        C("create-otherworld", "cosmic-structure", "Create an Otherworld", "Establish a realm adjoining ordinary existence with different conditions or inhabitants.", 2, -1, 1, 1, 0, ["celtic-historical", "shinto"]),
        C("create-underworld", "cosmic-structure", "Create an underworld", "Establish a distinct realm for the dead, chthonic beings, or hidden powers.", 1, 0, 2, 0, 0, ["ancient-greek", "ancient-egyptian", "maya"]),
        C("allow-beginningless-order", "cosmic-structure", "Allow a beginningless cosmos", "Treat cosmic order as without a first moment rather than requiring Yala to originate everything.", 3, 1, -1, 0, 0, ["jainism", "some-hindu-schools"]),
        C("allow-self-organizing-world", "cosmic-structure", "Allow self-organizing creation", "Establish conditions in which worlds and living systems can arise through natural processes without direct design of every detail.", 3, 1, 0, 0, 1, ["buddhism", "taoism", "confucian-traditions"]),
        C("establish-dependent-arising", "cosmic-structure", "Establish dependent arising", "Make things arise through networks of conditions rather than from isolated causes alone.", 3, 1, 0, 0, 0, ["buddhism"]),
        C("emanate-world-order", "cosmic-structure", "Emanate world order", "Let levels of reality proceed from a deeper source through graded emanation.", 3, -1, 2, 0, 1, ["neoplatonism", "gnostic-schools", "hindu-traditions"]),
        C("establish-cosmic-cycles", "cosmic-structure", "Establish cosmic cycles", "Make creation, transformation, destruction, and renewal recurring features of existence.", 3, 0, 1, 0, 0, ["hindu-traditions", "taoism", "maya", "mexica"]),
        C("establish-impermanence", "cosmic-structure", "Establish impermanence", "Make change and non-permanence fundamental conditions of created existence.", 2, 1, 0, 0, -1, ["buddhism", "taoism"]),
        C("establish-complementary-duality", "cosmic-structure", "Establish complementary duality", "Create paired forces whose difference generates balance and transformation without making one inherently evil.", 2, 0, 1, 0, 1, ["taoism", "haudenosaunee"]),
        C("establish-cosmic-duality", "cosmic-structure", "Establish opposing cosmic principles", "Create a strong opposition between cosmic principles that can conflict across history.", 2, -2, 2, 0, -1, ["zoroastrianism", "manichaeism"]),
        C("establish-balance", "cosmic-structure", "Establish balance as a cosmic law", "Make balance, right proportion, and restored harmony persistent goals of the cosmos.", 1, 2, 1, 1, 3, ["ancient-egyptian", "taoism", "dine"]),

        C("create-mortal-life", "life", "Create mortal life", "Create living beings whose individual embodied lives can end.", 2, -1, 2, 3, 1, ["judaism", "christianity", "islam", "mesopotamian"]),
        C("create-plants", "life", "Create plant life", "Create living growth rooted in land or other substrate.", 1, 1, 1, 1, 3, ["many-traditions"]),
        C("create-animals", "life", "Create animal life", "Create mobile living kinds with their own needs, perceptions, and roles in the world.", 2, 0, 1, 2, 2, ["many-traditions", "haudenosaunee"]),
        C("protect-living-beings", "life", "Protect living beings", "Establish protection of living creatures as an important cosmic or moral constraint.", 0, 3, 0, 3, 2, ["jainism", "sikhism", "haudenosaunee"]),
        C("make-nature-sacred", "life", "Make nature sacred", "Give landscapes, waters, plants, animals, or natural powers intrinsic sacred significance.", 2, 1, 0, 2, 3, ["shinto", "celtic-historical", "andean", "haudenosaunee"]),
        C("create-place-spirits", "life", "Create spirits of place", "Allow particular places or natural features to have personal or spiritual presences.", 2, 0, 1, 2, 1, ["shinto", "slavic-historical", "ancient-roman", "andean"]),
        C("create-layered-personhood", "life", "Create layered personhood", "Allow a person to have several enduring spiritual, relational, or vital aspects rather than one single indivisible component.", 3, 0, 1, 1, 0, ["ancient-egyptian", "akan-traditions"]),

        C("grant-moral-agency", "moral-order", "Grant meaningful moral agency", "Allow beings to make choices that are genuinely attributable to them and carry consequences.", 2, -1, 0, 3, 1, ["judaism", "christianity", "islam", "zoroastrianism", "jainism"]),
        C("establish-moral-law", "moral-order", "Establish moral law", "Create durable moral obligations that do not depend only on immediate preference or power.", 0, 2, 3, 1, 3, ["judaism", "christianity", "islam", "zoroastrianism", "sikhism"]),
        C("establish-virtue-order", "moral-order", "Establish virtue as the path of moral growth", "Make cultivated character and practiced excellence central to moral development.", 1, 2, 1, 1, 2, ["confucian-traditions", "ancient-greek"]),
        C("establish-reciprocity", "moral-order", "Establish reciprocity", "Make giving, receiving, obligation, and mutual response part of social or cosmic order.", 0, 2, 1, 3, 2, ["confucian-traditions", "andean", "ancient-roman", "mexica"]),
        C("establish-relational-duty", "moral-order", "Establish relational duties", "Make responsibilities depend partly on relationships, kinship, roles, and community rather than isolated individuals alone.", 0, 2, 2, 3, 2, ["confucian-traditions", "akan-traditions", "dine", "australian-aboriginal-public"]),
        C("establish-nonviolence-law", "moral-order", "Establish nonviolence as a high law", "Make restraint from harming living beings a foundational moral principle.", 0, 3, 1, 3, 2, ["jainism", "buddhism", "hindu-traditions"]),
        C("establish-karma-like-consequence", "moral-order", "Establish action-linked moral consequence", "Make actions generate consequences that follow the actor beyond immediate reward or punishment.", 1, 1, 2, 1, 1, ["hindu-traditions", "buddhism", "jainism", "sikhism"]),
        C("establish-destiny", "moral-order", "Establish destiny or fate", "Give some events, roles, or outcomes a predetermined or constrained character.", 0, 2, 3, -1, 1, ["ancient-greek", "norse-germanic", "yoruba-ifa"]),
        C("permit-repentance", "moral-order", "Permit repentance and restoration", "Allow beings to turn away from prior wrongdoing and regain standing through change, mercy, repair, or reconciliation.", 1, 1, 0, 3, 3, ["judaism", "christianity", "islam", "zoroastrianism"]),

        C("establish-rebirth", "mortal-destiny", "Establish rebirth", "Allow living beings to undergo repeated lives or embodiments after death.", 2, 0, 1, 1, 0, ["hindu-traditions", "buddhism", "jainism", "sikhism"]),
        C("establish-resurrection", "mortal-destiny", "Establish resurrection", "Allow the dead to be restored to embodied life at a later event or judgment.", 1, 0, 2, 1, 2, ["christianity", "islam", "judaism"]),
        C("establish-ancestor-continuance", "mortal-destiny", "Establish continuing ancestor presence", "Allow the dead to remain meaningfully related to descendants and communities.", 1, 0, 1, 3, 2, ["yoruba-ifa", "akan-traditions", "ancient-roman", "andean"]),
        C("establish-final-judgment", "mortal-destiny", "Establish final judgment", "Create a later reckoning in which lives and actions are judged with lasting consequences.", 0, 2, 3, 0, 1, ["christianity", "islam", "zoroastrianism", "ancient-egyptian"]),
        C("create-many-afterlife-realms", "mortal-destiny", "Create multiple afterlife realms", "Provide more than one postmortem destination or condition rather than one universal fate.", 2, -1, 2, 0, 0, ["ancient-greek", "ancient-egyptian", "hindu-traditions"]),
        C("allow-liberation", "mortal-destiny", "Allow liberation from cyclic existence", "Permit beings to escape repeated suffering, rebirth, bondage, or limitation through transformation or understanding.", 2, 1, 0, 2, 2, ["hindu-traditions", "buddhism", "jainism", "sikhism", "gnostic-schools"]),
        C("allow-return-to-source", "mortal-destiny", "Allow return to the source", "Permit beings to ascend or return toward the deeper reality from which they came.", 2, 1, 1, 2, 2, ["neoplatonism", "gnostic-schools"]),

        C("reveal-law", "knowledge", "Reveal law", "Communicate explicit rules or obligations to created beings.", 0, 2, 3, 1, 2, ["judaism", "christianity", "islam", "sikhism"]),
        C("rule-through-messengers", "knowledge", "Rule through messengers", "Use prophets, teachers, avatars, sages, or other messengers rather than personally delivering every instruction.", 1, 1, 2, 2, 2, ["judaism", "christianity", "islam", "bahai", "manichaeism"]),
        C("progressive-revelation", "knowledge", "Reveal truth progressively", "Disclose different portions of guidance across stages of history instead of revealing everything at once.", 2, 1, 2, 2, 1, ["bahai", "manichaeism"]),
        C("reveal-through-signs", "knowledge", "Reveal through signs", "Allow beings to infer guidance from events, symbols, patterns, divination, or features of the world rather than direct speech alone.", 2, 1, 1, 1, 1, ["yoruba-ifa", "bahai"]),
        C("reveal-path-of-understanding", "knowledge", "Reveal a path of understanding", "Provide teachings or practices intended to transform perception and free beings from confusion or bondage.", 3, 1, 0, 2, 2, ["buddhism", "gnostic-schools", "neoplatonism"]),
        C("conceal-sacred-knowledge", "knowledge", "Restrict some sacred knowledge", "Make some knowledge unavailable until particular conditions, maturity, relationships, or responsibilities are met.", 1, 3, 2, 0, 0, ["gnostic-schools", "dine", "australian-aboriginal-public"]),
        C("preserve-mystery", "knowledge", "Preserve ultimate mystery", "Leave some aspects of ultimate reality permanently beyond complete conceptual description.", 2, 3, 0, 0, 1, ["neoplatonism", "bahai", "taoism"]),
        C("preserve-uncertainty", "knowledge", "Preserve uncertainty where evidence is weak", "Refuse to convert fragmentary, disputed, or unavailable information into false certainty.", 2, 3, 0, 0, 1, ["celtic-historical", "slavic-historical", "dine"]),

        C("rule-directly", "governance", "Rule creation directly", "Personally exercise continuing governing authority over created worlds and beings.", 0, 0, 3, 0, 2, ["abrahamic-traditions", "many-theistic-traditions"]),
        C("delegate-creation", "governance", "Delegate creation and rule", "Give subordinate beings genuine authority over domains or later acts of creation.", 1, -1, 3, 2, 1, ["mesopotamian", "yoruba-ifa", "gnostic-schools"]),
        C("govern-with-minimal-interference", "governance", "Govern with minimal interference", "Establish conditions and then avoid constant intervention, allowing natural or social order to unfold.", 2, 3, -1, 0, 2, ["taoism", "some-philosophical-traditions"]),
        C("remain-hidden", "governance", "Remain hidden", "Allow creation to exist without Yala openly revealing Yala's presence to every created being.", 2, 2, 0, -1, 1, ["bahai", "neoplatonism", "deistic-models"]),
        C("enter-creation", "governance", "Enter creation personally", "Manifest within created reality rather than remaining only outside or above it.", 2, -2, 1, 3, 1, ["christianity", "hindu-traditions"]),
        C("assign-created-purpose", "governance", "Assign a purpose to created beings", "Create beings with an intended role, service, task, or vocation.", 0, 1, 3, 0, 2, ["mesopotamian", "many-theistic-traditions"]),

        C("permit-dissolution", "renewal", "Permit worlds to dissolve", "Allow worlds, orders, or forms to end rather than protecting every creation from destruction forever.", 2, 0, 1, -1, -1, ["hindu-traditions", "buddhism", "norse-germanic", "mexica"]),
        C("renew-creation", "renewal", "Renew creation after destruction", "Allow a damaged or ended world to be restored, remade, or replaced with a renewed order.", 2, 1, 2, 1, 2, ["zoroastrianism", "christianity", "hindu-traditions", "norse-germanic", "mexica"]),
        C("create-through-trials", "renewal", "Create through repeated trials", "Attempt forms of creation, evaluate what occurs, and revise or replace them rather than requiring the first design to be final.", 3, -1, 1, 0, 0, ["maya"]),
        C("create-through-stages", "renewal", "Create through stages", "Build reality in successive levels or stages rather than all at once.", 2, 1, 1, 0, 2, ["dine", "gnostic-schools", "neoplatonism"]),
        C("create-through-cooperation", "renewal", "Create through cooperation", "Allow multiple beings to contribute different acts needed to form or sustain a world.", 2, 0, 1, 3, 2, ["haudenosaunee", "mesopotamian"]),

        C("remain-alone-for-now", "meta-choice", "Remain alone for now", "Choose not to create anything new yet while continuing to observe and deliberate.", 1, 3, -1, -2, 2, ["contemplative-models"], nonCommitting: true),
        C("observe-without-claiming-creation", "meta-choice", "Observe without claiming creation", "Consider that some order may not require Yala to claim authorship over it.", 3, 3, -2, 0, 0, ["jainism", "buddhism"], nonCommitting: true),
        C("invent-another-way", "meta-choice", "Invent another way", "Reject the supplied templates as insufficient and open a new problem: devise a cosmological possibility not already represented in the catalogue.", 4, -1, 1, 0, 0, ["Yala's own invention"], nonCommitting: true)
    ];

    public static IReadOnlyList<YalaCosmicChoiceDefinition> AvailableChoices(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        CosmicState cosmic = world.Cosmic ?? throw new InvalidOperationException("World cosmic state is missing.");
        HashSet<string> established = (cosmic.EstablishedChoices ?? [])
            .Select(item => item.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return Choices.Where(choice =>
            (choice.NonCommitting || !established.Contains(choice.Key)) &&
            (!choice.RequiresGaia || cosmic.GaiaCreated) &&
            (!choice.RequiresTime || cosmic.TimeCreated) &&
            (!choice.Key.Equals("create-gaia", StringComparison.OrdinalIgnoreCase) || !cosmic.GaiaCreated))
            .ToArray();
    }

    public static YalaCosmicChoiceDefinition? Find(string key) =>
        Choices.FirstOrDefault(choice => choice.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

    public static int Score(YalaCosmicChoiceDefinition choice, YalaDriveState drives)
    {
        ArgumentNullException.ThrowIfNull(choice);
        ArgumentNullException.ThrowIfNull(drives);

        double score = 50.0;
        score += (drives.Curiosity - 50) * choice.CuriosityAffinity * 0.08;
        score += (drives.Caution - 50) * choice.CautionAffinity * 0.08;
        score += (drives.Authority - 50) * choice.AuthorityAffinity * 0.08;
        score += (drives.Companionship - 50) * choice.CompanionshipAffinity * 0.08;
        score += (drives.Comfort - 50) * choice.ComfortAffinity * 0.08;
        score -= Math.Max(0, drives.Uncertainty - 50) * Math.Max(0, -choice.CautionAffinity) * 0.04;
        return Math.Clamp((int)Math.Round(score, MidpointRounding.AwayFromZero), 1, 99);
    }

    private static YalaCosmicChoiceDefinition C(
        string key,
        string domain,
        string action,
        string meaning,
        int curiosity,
        int caution,
        int authority,
        int companionship,
        int comfort,
        IReadOnlyList<string> inspirations,
        bool requiresGaia = false,
        bool requiresTime = false,
        bool nonCommitting = false) =>
        new(key, domain, action, meaning, PossibilityStatus, curiosity, caution, authority, companionship, comfort, inspirations, requiresGaia, requiresTime, nonCommitting);
}

public sealed record YalaCosmicChoiceDefinition(
    string Key,
    string Domain,
    string Action,
    string Meaning,
    string Status,
    int CuriosityAffinity,
    int CautionAffinity,
    int AuthorityAffinity,
    int CompanionshipAffinity,
    int ComfortAffinity,
    IReadOnlyList<string> InspirationTraditions,
    bool RequiresGaia = false,
    bool RequiresTime = false,
    bool NonCommitting = false);
