using System.Reflection;
using System.Runtime.InteropServices;

namespace ProjectOracle.Cognition.Soar;

/// <summary>
/// Thin reflection-based host for the official Soar 9.6.5 C# SML bridge.
/// Reflection keeps Project Oracle buildable before the platform-specific SML
/// assembly is loaded, while every decision still runs through the real Soar kernel.
/// </summary>
public sealed class SoarKernelHost : IDisposable
{
    private readonly SoarRuntimePaths _paths;
    private readonly nint _kernelNativeHandle;
    private readonly nint _bridgeNativeHandle;
    private readonly Assembly _smlAssembly;
    private readonly object _kernel;
    private readonly object _agent;
    private readonly object _inputLink;
    private bool _disposed;

    public SoarKernelHost(string agentName, string productionsPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentName);
        ArgumentException.ThrowIfNullOrWhiteSpace(productionsPath);

        _paths = SoarRuntimePaths.Discover();
        _kernelNativeHandle = NativeLibrary.Load(_paths.NativeKernel);
        _bridgeNativeHandle = NativeLibrary.Load(_paths.NativeBridge);
        _smlAssembly = Assembly.LoadFrom(_paths.ManagedBridge);

        try
        {
            NativeLibrary.SetDllImportResolver(_smlAssembly, ResolveNativeLibrary);
        }
        catch (InvalidOperationException)
        {
            // The resolver may already be installed if more than one host is created
            // in the same process. The native libraries are already preloaded.
        }

        Type kernelType = _smlAssembly.GetType("sml.Kernel", throwOnError: true)!;
        _kernel = InvokeStatic(kernelType, "CreateKernelInNewThread")
            ?? throw new InvalidOperationException("Soar failed to create its kernel.");

        if (TryInvokeBool(_kernel, "HadError", out bool hadError) && hadError)
        {
            string description = TryInvokeString(_kernel, "GetLastErrorDescription") ?? "unknown Soar kernel error";
            throw new InvalidOperationException($"Soar kernel startup failed: {description}");
        }

        _agent = Invoke(_kernel, "CreateAgent", agentName)
            ?? throw new InvalidOperationException("Soar failed to create the Yala agent.");

        object? loaded = Invoke(_agent, "LoadProductions", Path.GetFullPath(productionsPath));
        if (loaded is bool loadedOk && !loadedOk)
        {
            string description = TryInvokeString(_agent, "GetLastErrorDescription") ?? "production load returned false";
            throw new InvalidOperationException($"Yala Soar productions failed to load: {description}");
        }

        _inputLink = Invoke(_agent, "GetInputLink")
            ?? throw new InvalidOperationException("Soar did not expose the input link.");
    }

    public YalaDecision Run(YalaPerception perception)
    {
        ArgumentNullException.ThrowIfNull(perception);
        ThrowIfDisposed();

        object input = Invoke(_agent, "CreateIdWME", _inputLink, "world-input")
            ?? throw new InvalidOperationException("Soar could not create Yala's world-input WME.");

        CreateString(input, "location", perception.Location);
        CreateString(input, "gaia-created", YesNo(perception.GaiaCreated));
        CreateString(input, "time-created", YesNo(perception.TimeCreated));
        CreateString(input, "decision-count", perception.DecisionCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
        CreateString(input, "last-action", Clean(perception.LastAction, "none"));
        CreateString(input, "last-result", Clean(perception.LastResult, "none"));
        CreateString(input, "contact", YesNo(perception.HasContact));
        CreateString(input, "contact-intent", Clean(perception.ContactIntent, "none"));
        CreateString(input, "contact-message", Clean(perception.ContactMessage, "none"));

        Invoke(_agent, "Commit");
        Invoke(_agent, "RunSelfTilOutput");

        int commandCount = Convert.ToInt32(Invoke(_agent, "GetNumberCommands") ?? 0, System.Globalization.CultureInfo.InvariantCulture);
        if (commandCount < 1)
        {
            throw new InvalidOperationException("Yala's Soar agent produced no output command.");
        }

        object command = Invoke(_agent, "GetCommand", 0)
            ?? throw new InvalidOperationException("Yala's first Soar output command could not be read.");
        string commandName = Convert.ToString(Invoke(command, "GetCommandName"), System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        if (!commandName.Equals("yala-action", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Unexpected Yala Soar output command: {commandName}");
        }

        string action = Parameter(command, "name", "wait");
        string replyCode = Parameter(command, "reply-code", "none");
        Invoke(command, "AddStatusComplete");
        Invoke(_agent, "Commit");
        Invoke(_agent, "DestroyWME", input);
        Invoke(_agent, "Commit");

        return new YalaDecision(
            action,
            replyCode,
            "Soar 9.6.5",
            $"Soar selected operator '{action}' from Yala's current perception.");
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        try
        {
            Invoke(_kernel, "Shutdown");
        }
        catch
        {
            // Shutdown is best effort during disposal. The original decision error,
            // if any, is more useful than a secondary teardown error.
        }

        // Do not free the bridge/kernel handles here. The managed SML assembly remains
        // loaded for the process lifetime and may invoke native finalizers later.
        GC.SuppressFinalize(this);
    }

    private nint ResolveNativeLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName.Contains("CSharp_sml_ClientInterface", StringComparison.OrdinalIgnoreCase))
        {
            return _bridgeNativeHandle;
        }
        if (libraryName.Contains("Soar", StringComparison.OrdinalIgnoreCase))
        {
            return _kernelNativeHandle;
        }
        return nint.Zero;
    }

    private void CreateString(object parent, string attribute, string value)
    {
        object? wme = Invoke(_agent, "CreateStringWME", parent, attribute, value);
        if (wme is null)
        {
            throw new InvalidOperationException($"Soar could not create input '{attribute}'.");
        }
    }

    private static string Parameter(object command, string name, string fallback)
    {
        object? value = Invoke(command, "GetParameterValue", name);
        string? text = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
        return string.IsNullOrWhiteSpace(text) ? fallback : text;
    }

    private static string Clean(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string YesNo(bool value) => value ? "yes" : "no";

    private static object? InvokeStatic(Type type, string methodName, params object?[] args) =>
        FindMethod(type, methodName, isStatic: true, args).Invoke(null, args);

    private static object? Invoke(object target, string methodName, params object?[] args) =>
        FindMethod(target.GetType(), methodName, isStatic: false, args).Invoke(target, args);

    private static MethodInfo FindMethod(Type type, string methodName, bool isStatic, object?[] args)
    {
        BindingFlags flags = BindingFlags.Public | (isStatic ? BindingFlags.Static : BindingFlags.Instance);
        MethodInfo[] candidates = type.GetMethods(flags)
            .Where(method => method.Name.Equals(methodName, StringComparison.Ordinal) && method.GetParameters().Length == args.Length)
            .ToArray();
        foreach (MethodInfo candidate in candidates)
        {
            ParameterInfo[] parameters = candidate.GetParameters();
            bool compatible = true;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (args[i] is null)
                {
                    continue;
                }
                if (!parameters[i].ParameterType.IsInstanceOfType(args[i]) &&
                    !CanConvert(args[i]!.GetType(), parameters[i].ParameterType))
                {
                    compatible = false;
                    break;
                }
            }
            if (compatible)
            {
                return candidate;
            }
        }
        throw new MissingMethodException(type.FullName, $"{methodName}/{args.Length}");
    }

    private static bool CanConvert(Type source, Type target) =>
        (source == typeof(int) && target == typeof(long)) ||
        (source == typeof(int) && target == typeof(uint)) ||
        (source == typeof(long) && target == typeof(int));

    private static bool TryInvokeBool(object target, string methodName, out bool value)
    {
        try
        {
            object? result = Invoke(target, methodName);
            if (result is bool boolean)
            {
                value = boolean;
                return true;
            }
        }
        catch (MissingMethodException)
        {
        }
        value = false;
        return false;
    }

    private static string? TryInvokeString(object target, string methodName)
    {
        try
        {
            return Convert.ToString(Invoke(target, methodName), System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (MissingMethodException)
        {
            return null;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SoarKernelHost));
        }
    }
}
