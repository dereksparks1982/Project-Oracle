using ProjectOracle.Domain;

namespace ProjectOracle.Cognition.Soar;

public sealed class YalaSoarMind : IDisposable
{
    public const string BrainName = "Yala Soar Brain Slice 7";
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
        foreach (YalaLearnedLexemeState lexeme in cognition?.LearnedLexicon ?? [])
        {
            _host.RememberClaimedDefinition(lexeme.Word, lexeme.ProposedMeaning);
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

    public void RememberClaimedDefinition(string word, string meaning)
    {
        ThrowIfDisposed();
        _host.RememberClaimedDefinition(word, meaning);
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
