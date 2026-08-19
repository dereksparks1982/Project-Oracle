using ProjectOracle.Cognition.Soar;
using ProjectOracle.Persistence;
using ProjectOracle.Simulation;
using ProjectOracle.Export;

namespace ProjectOracle.Desktop;

internal sealed class OracleDesktopSession : IDisposable
{
    private const ulong DefaultSeed = 104729UL;
    private readonly OracleSaveStore _store = new();
    private readonly IRealTimeSource _realTime = new SystemRealTimeSource();
    private readonly string _savePath;
    private OracleSimulation _simulation;

    public OracleDesktopSession()
    {
        _savePath = OracleSaveStore.DefaultPath();
        long now = _realTime.GetUnixTimeMilliseconds();
        _simulation = _store.Exists(_savePath)
            ? OracleSimulation.Restore(_store.Load(_savePath), now, _savePath)
            : OracleSimulation.Start(DefaultSeed, now, _savePath);
        Save();
    }

    public OracleSimulation Simulation => _simulation;
    public string SavePath => _savePath;

    public YalaDirectReply Speak(string message)
    {
        long now = _realTime.GetUnixTimeMilliseconds();
        _simulation.SynchroniseClock(now, recordAdvance: false);
        YalaDirectReply reply = _simulation.CallYala(message, now);
        Save();
        return reply;
    }

    public YalaDecision? AutonomousStep(bool force = false)
    {
        long now = _realTime.GetUnixTimeMilliseconds();
        _simulation.SynchroniseClock(now, recordAdvance: false);
        YalaDecision? decision = _simulation.TryRunYalaAutonomousStep(now, force);
        if (decision is not null) Save();
        return decision;
    }

    public void Tick()
    {
        _simulation.SynchroniseClock(_realTime.GetUnixTimeMilliseconds(), recordAdvance: false);
    }

    public void StartFreshWorld()
    {
        Save();
        ArchiveCurrentSave();
        _simulation.Dispose();
        long now = _realTime.GetUnixTimeMilliseconds();
        _simulation = OracleSimulation.Start(DefaultSeed, now, _savePath);
        Save();
    }

    public void Save() =>
        _store.Save(_savePath, _simulation.CreateSnapshot(_realTime.GetUnixTimeMilliseconds()));

    public string ExportSessionJson()
    {
        Save();
        return OracleSessionExporter.ExportJson(_simulation, _savePath);
    }

    public string ExportConversationText()
    {
        Save();
        return OracleSessionExporter.ExportConversationText(_simulation);
    }

    private void ArchiveCurrentSave()
    {
        if (!File.Exists(_savePath)) return;
        string directory = Path.GetDirectoryName(_savePath) ?? AppContext.BaseDirectory;
        string archive = Path.Combine(directory, "archives");
        Directory.CreateDirectory(archive);
        string stamp = DateTimeOffset.Now.ToString("yyyyMMdd_HHmmss", System.Globalization.CultureInfo.InvariantCulture);
        File.Copy(_savePath, Path.Combine(archive, $"save_v6_before_fresh_{stamp}.json"), overwrite: false);
    }

    public void Dispose()
    {
        Save();
        _simulation.Dispose();
    }
}
