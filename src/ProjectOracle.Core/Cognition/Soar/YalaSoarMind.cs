using ProjectOracle.Domain;

namespace ProjectOracle.Cognition.Soar;

public sealed class YalaSoarMind : IDisposable
{
    public const string BrainName = "Yala Soar Brain Slice 2";
    public const string Architecture = "Soar 9.6.5";

    private readonly SoarKernelHost _host;
    private bool _disposed;

    public YalaSoarMind(SoarMemoryPaths? memoryPaths = null, YalaCognitionState? cognition = null)
    {
        SoarRuntimePaths paths = SoarRuntimePaths.Discover();
        _host = new SoarKernelHost("yala", paths.YalaAgent, memoryPaths);
        _host.SeedCanonicalSemanticMemory();
        foreach (YalaContactMemory contact in cognition?.Contacts ?? [])
        {
            _host.RememberClaimedContact(contact.ClaimedName);
        }
    }

    public long SessionDecisionCount => _host.RunCount;

    public YalaDecision Decide(YalaPerception perception)
    {
        ThrowIfDisposed();
        return _host.Run(perception);
    }

    public void RememberClaimedContact(string claimedName)
    {
        ThrowIfDisposed();
        _host.RememberClaimedContact(claimedName);
    }

    public bool SemanticMemoryContainsClaimedContact(string claimedName)
    {
        ThrowIfDisposed();
        return _host.SemanticMemoryContainsClaimedContact(claimedName);
    }

    public SoarMemoryDiagnostics GetMemoryDiagnostics()
    {
        ThrowIfDisposed();
        return _host.GetMemoryDiagnostics();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _host.Dispose();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(YalaSoarMind));
    }
}
