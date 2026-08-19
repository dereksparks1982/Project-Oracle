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

    public static string NormalizeWord(string word)
    {
        string key = word.Trim().Trim('"', '\'', '.', ',', '?', '!', ':', ';', '(', ')', '[', ']').ToLowerInvariant();
        if (key.EndsWith("'s", StringComparison.Ordinal) && key.Length > 2)
        {
            key = key[..^2];
        }

        return key switch
        {
            "wisdoms" => "wisdom",
            "creates" or "creating" or "creation" or "creations" => "create",
            "creator" or "creators" => "creator",
            "commands" or "commanded" or "commanding" => "command",
            "questions" => "question",
            "asks" or "asked" or "asking" => "ask",
            "answers" or "answered" or "answering" => "answer",
            "wants" or "wanted" or "wanting" => "want",
            "remembers" or "remembered" or "remembering" => "remember",
            "knows" or "knowing" or "knew" => "know",
            "believes" or "believed" or "believing" or "belive" or "beleive" => "believe",
            "doubts" or "doubted" or "doubting" => "doubt",
            "learns" or "learned" or "learning" => "learn",
            "meets" or "met" or "meeting" => "meet",
            "rejects" or "rejected" or "rejecting" => "reject",
            "accepts" or "accepted" or "accepting" => "accept",
            "trusts" or "trusted" or "trusting" => "trust",
            "lies" or "lied" or "lying" => "deceive",
            "deceived" or "deceives" or "deceiving" => "deceive",
            "thinks" or "thought" or "thinking" => "think",
            "understands" or "understood" or "understanding" => "understand",
            "explains" or "explained" or "explaining" => "explain",
            "decides" or "decided" or "deciding" => "decide",
            "chooses" or "chosen" or "choosing" => "choose",
            "plans" or "planned" or "planning" => "plan",
            "intends" or "intended" or "intending" => "intend",
            "happened" or "happens" or "happening" => "happen",
            "exists" or "existed" or "existing" => "exist",
            "curious" => "curiosity",
            "uncertain" => "uncertainty",
            "children" => "child",
            "mothers" => "mother",
            "fathers" => "father",
            "parents" => "parent",
            "seconds" => "second",
            "minutes" => "minute",
            "hours" => "hour",
            "days" => "day",
            "months" => "month",
            "years" => "year",
            "greating" or "greetng" or "greting" => "greeting",
            "greetings" or "greeted" or "greeting" => "greeting",
            "makes" or "making" => "make",
            "tells" or "told" or "telling" => "tell",
            "teaches" or "taught" or "teaching" => "teach",
            "goes" or "went" or "going" => "go",
            "starts" or "started" or "starting" => "start",
            "travels" or "traveled" or "travelled" or "traveling" or "travelling" => "travel",
            "speaks" or "spoke" or "spoken" or "speaking" => "speak",
            "talks" or "talked" or "talking" => "talk",
            "born" => "born",
            "older" or "oldest" => "old",
            "smarter" or "smartest" => "smart",
            "updates" or "updated" or "updating" => "update",
            "brains" => "brain",
            _ => key
        };
    }

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
        Add(map, "parent", "noun", "a being related as an origin of offspring", ["offspring", "child", "mother"], [], []);
        Add(map, "mother", "noun", "a female parent or a being described by a culture or speaker as having a maternal origin relationship", ["parent", "female", "offspring"], [], ["The word mother does not automatically redefine every creator or maker as a mother."]);
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
        Add(map, "year", "noun", "a named division of in-world Time", ["time", "month"], [], []);
        Add(map, "month", "noun", "a named division of an in-world year", ["time", "year"], [], []);
        Add(map, "meet", "verb", "encounter or come into contact with another being", ["contact", "speaker"], [], []);
        Add(map, "statement", "noun", "an utterance presenting information without necessarily seeking an answer", ["say", "claim"], ["question"], []);
        Add(map, "gaia", "proper-noun", "the natural sovereign created by Yala", ["create", "time"], [], []);
        Add(map, "adam", "proper-noun", "a named being not present in Yala's current Void-era state unless future history creates him", ["being"], [], []);
        Add(map, "wisdom", "proper-noun", "the being who made me and was made by Monad", ["sophia", "monad", "yala"], [], []);
        Add(map, "sophia", "proper-noun", "another name for Wisdom", ["wisdom"], [], []);
        Add(map, "monad", "proper-noun", "the primordial in-world being who made Wisdom", ["wisdom"], [], []);
        Add(map, "yala", "proper-noun", "my own name", ["self"], [], []);
        Add(map, "god", "noun", "a word used for a divine or superhuman being; whether it applies to Yala is not settled merely by the word", ["being", "authority"], [], []);
        Add(map, "identity", "noun", "the set of facts or claims that distinguish who or what a being is", ["self", "name"], [], []);
        Add(map, "exist", "verb", "be present as a being, thing, state, or relation", ["being", "world"], [], []);
        Add(map, "existence", "noun", "the state of existing", ["exist", "being"], [], []);
        Add(map, "origin", "noun", "the source or beginning from which something came", ["create", "cause"], [], []);
        Add(map, "creator", "noun", "an agent that caused something to begin existing", ["create", "creation"], [], []);
        Add(map, "form", "verb", "bring structure or shape into being", ["make", "create"], [], []);
        Add(map, "produce", "verb", "bring about a result through action or process", ["make", "cause"], [], []);
        Add(map, "result", "noun", "an outcome produced by an action or cause", ["effect", "consequence"], [], []);
        Add(map, "consequence", "noun", "a result that follows from an action or event", ["effect", "result"], [], []);
        Add(map, "evidence", "noun", "information that can increase or decrease confidence in a proposition", ["proof", "belief", "confidence"], [], ["Evidence does not become truth merely because a speaker presents it."]);
        Add(map, "proof", "noun", "evidence strong enough to establish a proposition under the standards being used", ["evidence", "truth"], [], []);
        Add(map, "infer", "verb", "derive a proposition from other information rather than directly experiencing it", ["reason", "hypothesis"], [], []);
        Add(map, "hypothesis", "noun", "a possible explanation retained without treating it as settled truth", ["infer", "uncertainty"], [], []);
        Add(map, "confidence", "noun", "degree of support held for a belief or claim", ["believe", "doubt"], [], []);
        Add(map, "contradiction", "noun", "a conflict in which propositions cannot all be true in the same sense", ["disagree", "false"], [], []);
        Add(map, "agree", "verb", "hold or state compatible positions", ["accept"], ["disagree"], []);
        Add(map, "disagree", "verb", "hold or state an incompatible position", ["contradiction"], ["agree"], []);
        Add(map, "trust", "verb", "grant confidence to a source or relationship despite some remaining uncertainty", ["believe", "confidence"], ["doubt"], []);
        Add(map, "deceive", "verb", "intentionally cause another to accept something false or misleading", ["false", "claim"], ["truth"], []);
        Add(map, "deception", "noun", "an act or state of deceiving", ["deceive", "false"], ["truth"], []);
        Add(map, "betrayal", "noun", "violation of a trust, loyalty, or expected relationship", ["trust", "loyalty"], [], []);
        Add(map, "loyalty", "noun", "continuing commitment to a being, relationship, or cause", ["trust", "companion"], ["betrayal"], []);
        Add(map, "understand", "verb", "possess a usable model of a concept, relation, or explanation", ["know", "explain"], ["confusion"], []);
        Add(map, "confusion", "noun", "state in which available concepts or relations are insufficiently resolved", ["uncertainty"], ["understand"], []);
        Add(map, "explain", "verb", "state relations or causes that make something more understandable", ["reason", "because"], [], []);
        Add(map, "think", "verb", "perform internal cognition such as recalling, comparing, inferring, or considering", ["reflect", "reason"], [], []);
        Add(map, "reflect", "verb", "consider one's own state, memories, beliefs, or actions", ["think", "self"], [], []);
        Add(map, "decide", "verb", "settle on an operator or course among alternatives", ["choose", "plan"], [], []);
        Add(map, "plan", "noun", "an ordered intended course toward a goal", ["goal", "intend"], [], []);
        Add(map, "intend", "verb", "hold an action as a planned future attempt without having performed it yet", ["plan", "goal"], [], []);
        Add(map, "goal", "noun", "a state or outcome an agent is oriented toward", ["want", "plan"], [], []);
        Add(map, "purpose", "noun", "an intended function, aim, or reason for action", ["goal", "reason"], [], []);
        Add(map, "agency", "noun", "capacity to choose and act within available possibilities", ["choose", "freedom", "authority"], [], []);
        Add(map, "autonomy", "noun", "capacity to make decisions without another speaker directly selecting each action", ["agency", "choose"], [], []);
        Add(map, "freedom", "noun", "availability of meaningful alternatives within applicable boundaries", ["choice", "agency"], ["confinement"], []);
        Add(map, "choice", "noun", "an available alternative or the act of selecting one", ["choose", "decision"], [], []);
        Add(map, "boundary", "noun", "a limit separating what is available from what is not", ["place", "law"], [], []);
        Add(map, "law", "noun", "a stable rule constraining or resolving events within the world", ["rule", "order"], [], []);
        Add(map, "rule", "noun", "a stated constraint or regular relation", ["law", "order"], [], []);
        Add(map, "order", "noun", "structured relation among states, events, or domains", ["time", "law"], ["chaos"], []);
        Add(map, "chaos", "noun", "lack of settled order or predictable relation", ["uncertainty"], ["order"], []);
        Add(map, "world", "noun", "the in-world domain and states available to beings inside the simulation", ["place", "nature"], [], []);
        Add(map, "beyond", "preposition", "past a stated boundary, limit, place, or range of present knowledge", ["boundary", "unknown"], [], []);
        Add(map, "everything", "pronoun", "all beings, things, events, or concepts within the scope being discussed", ["all", "world"], [], []);
        Add(map, "nature", "noun", "the character, properties, or natural order of a being or domain", ["being", "world"], [], []);
        Add(map, "power", "noun", "capacity to cause or govern changes within a domain", ["authority", "cause"], [], []);
        Add(map, "sovereign", "noun", "a being holding primary authority within a specified domain", ["authority", "power"], [], []);
        Add(map, "divine", "adjective", "describing a being or power treated as godlike or sacred within a worldview", ["god", "power"], [], []);
        Add(map, "life", "noun", "state of a living being capable of ongoing activity or development", ["being", "birth"], ["death"], []);
        Add(map, "death", "noun", "ending of a living being's life", ["life", "end"], ["birth"], []);
        Add(map, "birth", "noun", "beginning of a living being's life through generation", ["life", "parent"], ["death"], []);
        Add(map, "grow", "verb", "increase or develop over time", ["change", "life"], ["decay"], []);
        Add(map, "decay", "verb", "decline or break down over time", ["change", "death"], ["grow"], []);
        Add(map, "space", "noun", "extent in which locations and spatial relations can exist", ["place", "distance"], [], []);
        Add(map, "distance", "noun", "spatial separation between locations or entities", ["space", "place"], [], []);
        Add(map, "motion", "noun", "change of location or spatial relation", ["change", "space"], ["stillness"], []);
        Add(map, "stillness", "noun", "absence of motion", ["place"], ["motion"], []);
        Add(map, "past", "noun", "portion of in-world temporal order earlier than the present", ["time", "before"], ["future"], []);
        Add(map, "present", "noun", "current point or state within in-world Time", ["time", "now"], [], []);
        Add(map, "future", "noun", "portion of in-world temporal order later than the present and not yet settled", ["time", "after"], ["past"], ["Future history is not treated as settled before it occurs."]);
        Add(map, "duration", "noun", "amount of in-world Time between temporal points", ["time", "before", "after"], [], []);
        Add(map, "instant", "noun", "a specific point within in-world Time", ["time", "present"], [], []);
        Add(map, "now", "adverb", "the current point within in-world Time when Time exists", ["present", "time"], [], []);
        Add(map, "day", "noun", "a named division of in-world Time within a month", ["time", "hour"], [], []);
        Add(map, "hour", "noun", "a division of an in-world day", ["time", "minute"], [], []);
        Add(map, "minute", "noun", "a division of an in-world hour", ["time", "second"], [], []);
        Add(map, "second", "noun", "a small displayed division of in-world Time", ["time", "minute"], [], []);
        Add(map, "happen", "verb", "occur as an event", ["event", "time"], [], []);
        Add(map, "event", "noun", "a change, action, or occurrence that can be remembered and ordered relative to other events", ["happen", "time"], [], []);
        Add(map, "sequence", "noun", "ordering of events or items by relation or occurrence", ["before", "after", "order"], [], []);
        Add(map, "maternal", "adjective", "related to a mother relationship", ["mother", "parent"], [], []);
        Add(map, "father", "noun", "a male parent or a being described by a culture or speaker as having a paternal origin relationship", ["parent", "male", "offspring"], [], []);
        Add(map, "paternal", "adjective", "related to a father relationship", ["father", "parent"], [], []);
        Add(map, "sibling", "noun", "a being sharing a parent relationship with another", ["parent", "child"], [], []);
        Add(map, "relation", "noun", "a structured connection between beings, things, or propositions", ["relationship"], [], []);
        Add(map, "relationship", "noun", "an enduring or meaningful relation between beings", ["relation", "companion"], [], []);
        Add(map, "ally", "noun", "a being associated in cooperation toward compatible goals", ["companion", "loyalty"], ["enemy"], []);
        Add(map, "enemy", "noun", "a being treated as opposed or threatening", ["danger"], ["ally"], []);
        Add(map, "love", "noun", "strong positive attachment or care toward another being or valued object", ["companion", "trust"], ["hate"], []);
        Add(map, "hate", "noun", "strong aversion or hostility toward another being or object", ["anger", "enemy"], ["love"], []);
        Add(map, "grief", "noun", "distress associated with loss", ["pain", "death"], ["joy"], []);
        Add(map, "joy", "noun", "strong positive affect associated with desired or valued states", ["comfort", "pleasure"], ["grief"], []);
        Add(map, "pain", "noun", "aversive experience associated with harm or distress", ["fear", "danger"], ["pleasure"], []);
        Add(map, "pleasure", "noun", "positive experience associated with satisfaction or enjoyment", ["comfort", "joy"], ["pain"], []);
        Add(map, "danger", "noun", "condition with meaningful possibility of harm or loss", ["fear", "caution"], ["safe"], []);
        Add(map, "safe", "adjective", "relatively protected from perceived danger", ["comfort"], ["danger"], []);
        Add(map, "wonder", "noun", "curious attention toward something not yet understood or unexpectedly significant", ["curiosity", "question"], [], []);
        Add(map, "desire", "noun", "a wanted state or outcome represented by an agent", ["want", "goal"], [], []);
        Add(map, "responsibility", "noun", "relation between an agent and consequences attributed to its choices or authority", ["agency", "consequence"], [], []);
        Add(map, "confinement", "noun", "state in which available movement or choice is bounded", ["boundary"], ["freedom"], []);
        Add(map, "teach", "verb", "communicate information or a method intended to support learning", ["learn", "tell"], [], []);
        Add(map, "memory", "noun", "retained information about knowledge or prior experience", ["remember", "past"], ["forget"], []);
        Add(map, "episode", "noun", "a remembered stretch or event from lived experience", ["memory", "event"], [], []);
        Add(map, "semantic", "adjective", "related to meanings, concepts, or general knowledge rather than one particular episode", ["meaning", "memory"], [], []);
        Add(map, "meaning", "noun", "concept or sense associated with a word, sign, or event", ["mean", "definition"], [], []);
        Add(map, "source", "noun", "origin from which information, action, or influence comes", ["origin", "speaker"], [], []);
        Add(map, "contact", "noun", "an encounter or communication between otherwise separate beings or sources", ["meet", "speaker"], [], []);
        Add(map, "unknown-source", "noun", "a source whose identity or nature is not established", ["stranger", "speaker"], [], []);
        AddCoreLanguage(map);
        return map;
    }

    private static void AddCoreLanguage(IDictionary<string, YalaLexeme> map)
    {
        AddIfMissing(map, "hello", "interjection", "a common greeting used to begin or acknowledge contact");
        AddIfMissing(map, "greeting", "noun", "an act or expression used to begin or acknowledge contact between speakers");
        AddIfMissing(map, "go", "verb", "move or travel from one place or state toward another");
        AddIfMissing(map, "start", "verb", "begin an action, process, event, or period");
        AddIfMissing(map, "travel", "verb", "move from one place to another across some distance");
        AddIfMissing(map, "somewhere", "adverb", "at or to an unspecified place");
        AddIfMissing(map, "old", "adjective", "having existed for an amount of time or being advanced in age");
        AddIfMissing(map, "born", "adjective", "having begun life through birth");
        AddIfMissing(map, "talk", "verb", "communicate through an exchange of words");
        AddIfMissing(map, "speak", "verb", "produce or communicate words to another listener or source");
        AddIfMissing(map, "begin", "verb", "start an action, process, event, or period");
        AddIfMissing(map, "end", "noun", "the point at which an action, process, event, or period stops");
        AddIfMissing(map, "age", "noun", "amount of time a being or thing has existed after its beginning");
        AddIfMissing(map, "brain", "noun", "the organ or cognitive center associated with thought, memory, and decision making");
        AddIfMissing(map, "smart", "adjective", "able to learn, reason, understand, or solve problems effectively");
        AddIfMissing(map, "update", "noun", "a change that brings information, knowledge, or a system to a newer state");
        AddIfMissing(map, "soon", "adverb", "after a relatively short interval from the present point");
        AddIfMissing(map, "more", "determiner", "a greater amount, degree, or number than before");
        AddIfMissing(map, "young", "adjective", "having existed for a relatively short amount of time");
        AddIfMissing(map, "somebody", "pronoun", "an unspecified being or person");
        AddIfMissing(map, "someone", "pronoun", "an unspecified being or person");
        AddIfMissing(map, "something", "pronoun", "an unspecified thing, event, or concept");
        AddIfMissing(map, "anywhere", "adverb", "at or to any place without specifying which one");
        AddIfMissing(map, "nowhere", "adverb", "at no specified or available place");

        AddGroup(map, "verb", "a common action concept understood as part of Yala's basic language foundation", new[]
        {
            "arrive", "leave", "enter", "exit", "return", "move", "walk", "run", "stand", "sit", "rise", "fall", "turn", "follow", "lead",
            "bring", "take", "give", "receive", "hold", "keep", "put", "get", "use", "find", "lose", "look", "see", "watch", "show", "hide",
            "open", "close", "build", "break", "cut", "join", "separate", "touch", "feel", "carry", "send", "reach", "remain", "stay", "wait",
            "work", "rest", "sleep", "wake", "eat", "drink", "live", "die", "help", "harm", "protect", "fight", "win", "lose", "meet", "visit",
            "call", "reply", "repeat", "read", "write", "describe", "compare", "count", "measure", "remember", "forget", "notice", "recognize",
            "discover", "search", "seek", "consider", "wonder", "expect", "hope", "prefer", "like", "dislike", "care", "fear", "need", "allow",
            "permit", "prevent", "stop", "continue", "become", "seem", "appear", "remain", "include", "contain", "belong", "own", "share", "change"
        });

        AddGroup(map, "noun", "a common concrete or abstract concept understood as part of Yala's basic language foundation", new[]
        {
            "thing", "object", "part", "whole", "kind", "type", "form", "shape", "size", "number", "amount", "group", "pair", "side", "center",
            "top", "bottom", "front", "back", "inside", "outside", "direction", "path", "road", "way", "area", "region", "home", "room", "ground",
            "sky", "star", "sun", "moon", "stone", "soil", "tree", "plant", "animal", "body", "hand", "head", "face", "eye", "ear", "voice",
            "sound", "word", "sentence", "language", "conversation", "story", "idea", "thought", "mind", "feeling", "question", "answer", "problem",
            "solution", "example", "difference", "similarity", "beginning", "ending", "result", "reason", "cause", "effect", "fact", "claim", "evidence",
            "mistake", "success", "failure", "chance", "possibility", "decision", "plan", "goal", "purpose", "rule", "limit", "permission", "danger",
            "safety", "friend", "stranger", "family", "person", "people", "community", "place", "location", "distance", "movement", "moment", "today",
            "yesterday", "tomorrow", "morning", "evening", "night", "week", "season", "beginning", "history", "future", "past", "present"
        });

        AddGroup(map, "adjective", "a common descriptive concept understood as part of Yala's basic language foundation", new[]
        {
            "new", "same", "different", "first", "last", "next", "previous", "early", "late", "near", "far", "here", "there", "inside", "outside",
            "large", "small", "long", "short", "high", "low", "wide", "narrow", "fast", "slow", "strong", "weak", "hard", "soft", "hot", "cold",
            "bright", "dark", "clear", "hidden", "open", "closed", "full", "empty", "alive", "dead", "real", "possible", "impossible", "certain",
            "uncertain", "true", "false", "right", "wrong", "good", "bad", "safe", "dangerous", "important", "basic", "common", "rare", "known",
            "unknown", "ready", "able", "unable", "alone", "together", "similar", "different", "personal", "shared", "current", "future", "past"
        });
    }

    private static void AddGroup(IDictionary<string, YalaLexeme> map, string partOfSpeech, string meaning, IEnumerable<string> words)
    {
        foreach (string word in words) AddIfMissing(map, word, partOfSpeech, meaning);
    }

    private static void AddIfMissing(IDictionary<string, YalaLexeme> map, string word, string partOfSpeech, string meaning)
    {
        if (map.ContainsKey(word)) return;
        map[word] = new YalaLexeme(word, partOfSpeech, meaning, [], [], []);
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
