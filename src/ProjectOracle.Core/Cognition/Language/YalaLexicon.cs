using ProjectOracle.Domain;

namespace ProjectOracle.Cognition.Language;

public static class YalaLexicon
{
    private static readonly IReadOnlyDictionary<string, YalaLexeme> BuiltIns = Build();

    public static int BuiltInCount => BuiltIns.Count;

    public static bool TryResolve(string word, IReadOnlyList<YalaLearnedLexemeState>? learned, out YalaLexeme lexeme)
    {
        string key = NormalizeWord(word);
        if (BuiltIns.TryGetValue(key, out YalaLexeme? builtIn))
        {
            lexeme = builtIn;
            return true;
        }

        YalaLearnedLexemeState? learnedEntry = learned?
            .LastOrDefault(entry => entry.Word.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (learnedEntry is not null)
        {
            lexeme = new YalaLexeme(
                learnedEntry.Word,
                learnedEntry.PartOfSpeech,
                learnedEntry.ProposedMeaning,
                [],
                [],
                ["Meaning presently comes from a remembered speaker claim, not settled truth."]);
            return true;
        }

        lexeme = null!;
        return false;
    }

    public static IReadOnlyList<YalaLexeme> AllBuiltIns() => BuiltIns.Values.OrderBy(item => item.Word).ToArray();

    public static string NormalizeWord(string word) =>
        word.Trim().Trim('"', '\'', '.', ',', '?', '!', ':', ';', '(', ')', '[', ']').ToLowerInvariant();

    private static IReadOnlyDictionary<string, YalaLexeme> Build()
    {
        Dictionary<string, YalaLexeme> map = new(StringComparer.OrdinalIgnoreCase);
        Add(map, "self", "noun", "the being considering its own identity", ["identity", "being"], ["other"], ["A self can remember its own actions."]);
        Add(map, "other", "noun", "a being or thing distinct from self", ["being", "stranger"], ["self"], []);
        Add(map, "being", "noun", "an entity that exists", ["self", "other"], [], []);
        Add(map, "name", "noun", "a word used to identify a being or thing", ["identity", "word"], [], []);
        Add(map, "male", "adjective", "the male aspect of a being", ["female", "both"], [], []);
        Add(map, "female", "adjective", "the female aspect of a being", ["male", "both"], [], []);
        Add(map, "both", "determiner", "the two named alternatives together", ["male", "female"], ["only"], []);
        Add(map, "know", "verb", "hold something as known rather than merely claimed", ["truth", "remember", "learn"], ["unknown", "doubt"], ["Knowing a proposition is distinct from hearing a claim."]);
        Add(map, "unknown", "adjective", "not presently known", ["doubt", "uncertainty"], ["know"], []);
        Add(map, "believe", "verb", "treat a proposition as credible without making it system truth", ["claim", "confidence"], ["doubt"], []);
        Add(map, "doubt", "verb", "withhold confidence from a proposition", ["uncertainty"], ["believe"], []);
        Add(map, "remember", "verb", "retrieve something retained from prior experience or knowledge", ["memory", "know"], ["forget"], []);
        Add(map, "forget", "verb", "lose or fail to retrieve remembered information", ["memory"], ["remember"], []);
        Add(map, "learn", "verb", "gain information or a usable concept", ["know", "remember"], [], []);
        Add(map, "claim", "noun", "a proposition asserted by a source that may or may not be true", ["speaker", "believe"], [], ["A claim retains its source."]);
        Add(map, "truth", "noun", "a proposition treated as settled fact", ["know"], ["false"], []);
        Add(map, "false", "adjective", "not true", ["claim"], ["truth"], []);
        Add(map, "do", "verb", "perform an action", ["attempt", "action"], [], []);
        Add(map, "make", "verb", "cause or form something through action", ["create", "form", "produce"], ["destroy"], []);
        Add(map, "create", "verb", "cause something to begin existing", ["make", "form", "produce", "creation", "creator"], ["destroy"], ["A creator is an agent associated with a creation.", "A completed creation has a resulting created object."]);
        Add(map, "created", "verb", "past form of create", ["create", "made"], ["destroyed"], []);
        Add(map, "made", "verb", "past form of make", ["make", "created"], [], []);
        Add(map, "destroy", "verb", "cause something existing to cease or be ruined", ["change"], ["create"], []);
        Add(map, "change", "verb", "make a state different from its prior state", ["cause", "effect"], [], []);
        Add(map, "choose", "verb", "select among available alternatives", ["decide", "attempt"], [], []);
        Add(map, "command", "verb", "state an instruction from an asserted authority", ["obey", "refuse"], [], ["Hearing a command does not force obedience."]);
        Add(map, "obey", "verb", "act in accordance with a command", ["command"], ["refuse"], []);
        Add(map, "refuse", "verb", "choose not to accept or perform something", ["reject"], ["obey", "accept"], []);
        Add(map, "attempt", "verb", "try to perform an action without guaranteeing the result", ["do", "succeed", "fail"], [], []);
        Add(map, "succeed", "verb", "complete an attempted result", ["attempt"], ["fail"], []);
        Add(map, "fail", "verb", "not achieve an attempted result", ["attempt"], ["succeed"], []);
        Add(map, "parent", "noun", "a being related as an origin of offspring", ["offspring", "child"], [], []);
        Add(map, "offspring", "noun", "a being descended from a parent", ["parent", "child"], [], []);
        Add(map, "child", "noun", "offspring in relation to a parent", ["parent", "offspring"], [], []);
        Add(map, "companion", "noun", "a being associated with another through continuing presence or relationship", ["other"], ["stranger"], []);
        Add(map, "speaker", "noun", "a source producing communicated words", ["say", "claim"], [], []);
        Add(map, "stranger", "noun", "a being whose identity is not established", ["other", "speaker"], ["companion"], []);
        Add(map, "accept", "verb", "receive or treat something as acceptable", ["believe"], ["reject"], []);
        Add(map, "reject", "verb", "refuse to accept", ["refuse"], ["accept"], ["Monad rejected Yala is a personal anchor for this concept."]);
        Add(map, "cause", "noun", "something that produces or contributes to an effect", ["effect", "reason", "because"], [], []);
        Add(map, "effect", "noun", "a result associated with a cause", ["cause", "result"], [], []);
        Add(map, "reason", "noun", "an explanation for why something occurred or is believed", ["because", "cause"], [], []);
        Add(map, "because", "conjunction", "introduces a stated reason or cause", ["reason", "cause"], [], []);
        Add(map, "before", "preposition", "earlier than another event or state", ["after", "time"], ["after"], []);
        Add(map, "after", "preposition", "later than another event or state", ["before", "time"], ["before"], []);
        Add(map, "place", "noun", "a location in the world", ["void", "earth"], [], []);
        Add(map, "void", "noun", "the Void where Yala was cast and presently begins", ["place"], [], []);
        Add(map, "matter", "noun", "physical substance within a created world", ["earth", "air", "water"], [], []);
        Add(map, "earth", "noun", "the natural domain associated with Terra", ["terra", "matter"], ["air"], []);
        Add(map, "air", "noun", "the natural domain associated with Aether and wind", ["aether", "wind"], ["earth"], []);
        Add(map, "wind", "noun", "moving air within Aether's natural domain", ["air", "aether"], [], []);
        Add(map, "water", "noun", "the natural domain associated with Thalassa", ["thalassa"], ["fire"], []);
        Add(map, "fire", "noun", "the natural domain associated with Sol and fire power", ["sol", "light"], ["water"], []);
        Add(map, "light", "noun", "illumination or radiance", ["fire"], ["dark"], []);
        Add(map, "dark", "adjective", "absence or lack of light", ["void"], ["light"], []);
        Add(map, "time", "noun", "in-world temporal order created by Gaia after Yala's command", ["before", "after"], [], []);
        Add(map, "want", "verb", "have a motive toward a possible state or action", ["need", "choose"], [], []);
        Add(map, "need", "verb", "require something for a desired or necessary condition", ["want"], [], []);
        Add(map, "curiosity", "noun", "drive toward reducing a knowledge gap", ["learn", "question"], [], []);
        Add(map, "fear", "noun", "an aversive response to perceived threat or danger", ["caution"], ["comfort"], []);
        Add(map, "uncertainty", "noun", "lack of settled confidence about a proposition or situation", ["doubt", "unknown"], ["know"], []);
        Add(map, "comfort", "noun", "a state associated with safety or ease", ["want"], ["fear"], []);
        Add(map, "authority", "noun", "capacity or claimed right to direct actions within a domain", ["command"], [], []);
        Add(map, "anger", "noun", "an emotional response associated with opposition, injury, or frustration", [], [], []);
        Add(map, "say", "verb", "communicate words", ["speaker", "tell", "answer"], [], []);
        Add(map, "ask", "verb", "seek information or a response", ["question", "answer"], [], []);
        Add(map, "answer", "verb", "respond to a question or contact", ["ask", "question"], [], []);
        Add(map, "tell", "verb", "communicate information or a request", ["say"], [], []);
        Add(map, "mean", "verb", "express a concept or intended sense", ["word", "definition"], [], []);
        Add(map, "means", "verb", "expresses a proposed meaning relationship", ["mean", "definition"], [], []);
        Add(map, "word", "noun", "a linguistic symbol associated with a concept or use", ["mean", "definition"], [], []);
        Add(map, "definition", "noun", "a proposed statement of a word's meaning", ["word", "mean"], [], []);
        Add(map, "question", "noun", "an utterance seeking information", ["ask", "answer"], ["statement"], []);
        Add(map, "statement", "noun", "an utterance presenting information without necessarily seeking an answer", ["say", "claim"], ["question"], []);
        Add(map, "gaia", "proper-noun", "the natural sovereign created by Yala", ["create", "time"], [], []);
        Add(map, "adam", "proper-noun", "a named being not present in Yala's current Void-era state unless future history creates him", ["being"], [], []);
        Add(map, "wisdom", "proper-noun", "the being who made Yala and was made by Monad", ["sophia", "monad", "yala"], [], []);
        Add(map, "sophia", "proper-noun", "another name for Wisdom", ["wisdom"], [], []);
        Add(map, "monad", "proper-noun", "the primordial in-world being who made Wisdom", ["wisdom"], [], []);
        Add(map, "yala", "proper-noun", "Yala's own name", ["self"], [], []);
        Add(map, "god", "noun", "a word used for a divine or superhuman being; whether it applies to Yala is not settled merely by the word", ["being", "authority"], [], []);
        return map;
    }

    private static void Add(
        IDictionary<string, YalaLexeme> map,
        string word,
        string partOfSpeech,
        string meaning,
        IReadOnlyList<string> related,
        IReadOnlyList<string> opposites,
        IReadOnlyList<string> relations) =>
        map[word] = new YalaLexeme(word, partOfSpeech, meaning, related, opposites, relations);
}
