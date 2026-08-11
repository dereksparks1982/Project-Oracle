using ProjectOracle.Domain;

namespace ProjectOracle.ConsoleApp;

public sealed record DirectCall(DirectCallTargetState Target, string Message);

public static class DirectCallParser
{
    public static bool TryParse(
        string input,
        IReadOnlyList<DirectCallTargetState> targets,
        out DirectCall? call,
        out string? error)
    {
        call = null;
        error = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Direct call is empty.";
            return false;
        }

        string trimmed = input.TrimStart();
        if (!trimmed.StartsWith('('))
        {
            error = "A direct call must begin with '(' immediately before the being's name.";
            return false;
        }

        string body = trimmed[1..];
        if (body.Length == 0)
        {
            error = "Name the being after '('.";
            return false;
        }

        if (char.IsWhiteSpace(body[0]))
        {
            error = "The being's name must immediately follow '('.";
            return false;
        }

        DirectCallTargetState? target = targets
            .Where(candidate => candidate.ReceivesDirectCall)
            .SelectMany(candidate => CandidateNames(candidate)
                .Select(name => (Target: candidate, Name: name)))
            .Where(candidate => StartsWithName(body, candidate.Name))
            .OrderByDescending(candidate => candidate.Name.Length)
            .Select(candidate => candidate.Target)
            .FirstOrDefault();

        if (target is null)
        {
            error = "No direct-call target matches that name.";
            return false;
        }

        string matchedName = CandidateNames(target)
            .Where(name => StartsWithName(body, name))
            .OrderByDescending(name => name.Length)
            .First();

        string message = body[matchedName.Length..].Trim();
        if (message.Length == 0)
        {
            error = $"Give {target.TargetName} a message after the name.";
            return false;
        }

        call = new DirectCall(target, message);
        return true;
    }

    private static IEnumerable<string> CandidateNames(DirectCallTargetState target)
    {
        yield return target.TargetName;
        if (!target.Key.Equals(target.TargetName, StringComparison.OrdinalIgnoreCase))
        {
            yield return target.Key;
        }
    }

    private static bool StartsWithName(string body, string name)
    {
        if (!body.StartsWith(name, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return body.Length == name.Length || char.IsWhiteSpace(body[name.Length]);
    }
}
