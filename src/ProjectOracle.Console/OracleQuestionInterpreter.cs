using ProjectOracle.Domain;
using ProjectOracle.Lore;
using ProjectOracle.Observation;

namespace ProjectOracle.ConsoleApp;

/// <summary>
/// Oracle-console interpretation is system-level help for Derek. It is not an
/// in-world mind and its answers are never inserted into Yala's knowledge.
/// </summary>
public static class OracleQuestionInterpreter
{
    public static bool TryAnswer(string question, WorldState world, out IReadOnlyList<string> lines) =>
        TryAnswer(question, world, [], out lines);

    public static bool TryAnswer(
        string question,
        WorldState world,
        IReadOnlyList<ObservationState> observations,
        out IReadOnlyList<string> lines)
    {
        string text = Normalise(question);

        if (ContainsAny(text, "creation order", "who made who", "genealogy"))
        {
            lines = ["Oracle system truth:", OracleLore.MonadFoundation, OracleLore.WisdomOrigin, OracleLore.YalaOrigin, OracleLore.YalaVoid];
            return true;
        }

        if (ContainsAny(text, "who is monad", "what is monad", "who made wisdom", "who created wisdom"))
        {
            lines = ["Oracle system truth:", OracleLore.MonadFoundation, OracleLore.WisdomOrigin];
            return true;
        }

        if (ContainsAny(text, "who is wisdom", "what is wisdom", "who is sophia"))
        {
            lines = ["Oracle system truth:", OracleLore.WisdomOrigin, OracleLore.YalaOrigin, OracleLore.WisdomFuture];
            return true;
        }

        if (ContainsAny(text, "who is yala", "what is yala", "who made yala", "who created yala"))
        {
            lines = ["Oracle system truth:", OracleLore.YalaOrigin, OracleLore.YalaVoid, OracleLore.YalaGovernance, world.Yala.AuthorityCaveat];
            return true;
        }

        if (ContainsAny(text, "who is oracle", "what is oracle", "master key"))
        {
            lines = ["Oracle system truth:", OracleLore.OracleSystemNature, OracleLore.OracleMasterKey, OracleLore.OracleHidden];
            return true;
        }

        if (ContainsAny(text, "serpent", "snake", "eden"))
        {
            lines = ["Oracle system truth:", OracleLore.OracleSerpentManifestation, OracleLore.Eden];
            return true;
        }

        if (ContainsAny(text, "who made time", "who created time", "what is time"))
        {
            lines = ["Oracle system truth:", OracleLore.GaiaTime];
            return true;
        }

        if (ContainsAny(text, "gaia", "elements", "aether", "terra", "thalassa", "sol", "luna", "wind"))
        {
            lines = ["Oracle system truth:", OracleLore.GaiaOrigin, OracleLore.GaiaTime, OracleLore.ElementalOrder, OracleLore.WeatherAuthority];
            return true;
        }

        if (ContainsAny(text, "demons", "demon"))
        {
            lines = ["Oracle system truth:", OracleLore.PotentialDemonOrigin];
            return true;
        }

        if (ContainsAny(text, "simulation law", "future history", "canon law"))
        {
            lines = ["Oracle system truth:", OracleLore.PrimeSimulationLaw];
            return true;
        }

        lines = [];
        return false;
    }

    private static string Normalise(string value) =>
        string.Join(' ', value.Trim().ToLowerInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static bool ContainsAny(string text, params string[] fragments) =>
        fragments.Any(fragment => text.Contains(fragment, StringComparison.Ordinal));
}
