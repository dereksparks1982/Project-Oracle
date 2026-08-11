namespace ProjectOracle.Cognition.Soar;

public sealed record SoarRuntimePaths(
    string RepositoryRoot,
    string ManagedBridge,
    string NativeBridge,
    string NativeKernel,
    string YalaAgent)
{
    public static SoarRuntimePaths Discover()
    {
        foreach (string start in CandidateStarts())
        {
            DirectoryInfo? directory = new(Path.GetFullPath(start));
            while (directory is not null)
            {
                string vendor = Path.Combine(directory.FullName, "vendor", "soar", "9.6.5", "linux-x86-64");
                string agent = Path.Combine(directory.FullName, "src", "ProjectOracle.Core", "Cognition", "Soar", "Agents", "yala.soar");
                string managed = Path.Combine(vendor, "sml_csharp.dll");
                string bridge = Path.Combine(vendor, "libCSharp_sml_ClientInterface.so");
                string kernel = Path.Combine(vendor, "libSoar.so");
                if (File.Exists(managed) && File.Exists(bridge) && File.Exists(kernel) && File.Exists(agent))
                {
                    return new SoarRuntimePaths(directory.FullName, managed, bridge, kernel, agent);
                }
                directory = directory.Parent;
            }
        }

        throw new FileNotFoundException(
            "Soar 9.6.5 runtime or Yala agent was not found. Expected vendor/soar/9.6.5/linux-x86-64 and src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar under the Project Oracle repository root.");
    }

    private static IEnumerable<string> CandidateStarts()
    {
        yield return Environment.CurrentDirectory;
        yield return AppContext.BaseDirectory;
        string? processPath = Environment.ProcessPath;
        if (!string.IsNullOrWhiteSpace(processPath))
        {
            string? processDirectory = Path.GetDirectoryName(processPath);
            if (!string.IsNullOrWhiteSpace(processDirectory))
            {
                yield return processDirectory;
            }
        }
    }
}
