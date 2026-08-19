using ProjectOracle.Cognition.Soar;
using ProjectOracle.Persistence;
using ProjectOracle.Simulation;
using ProjectOracle.Export;
using ProjectOracle.Domain;

namespace ProjectOracle.Desktop;

internal sealed record OperatorDispatchResult(
    string Channel,
    string Result,
    YalaDirectReply? YalaReply,
    OracleActionState? Action);

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
    public string ActiveChannel => _simulation.OperatorState.ActiveChannel;

    public OracleActionState SelectChannel(string channel)
    {
        OracleActionState action = _simulation.SelectOperatorChannel(channel);
        Save();
        return action;
    }

    public OperatorDispatchResult Dispatch(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        long now = _realTime.GetUnixTimeMilliseconds();
        _simulation.SynchroniseClock(now, recordAdvance: false);
        string channel = ActiveChannel;
        if (channel == "oracle")
        {
            OracleActionState action = _simulation.ActAsOracle(message);
            Save();
            return new OperatorDispatchResult(channel, action.Result, null, action);
        }
        if (channel == "yala")
        {
            YalaDirectReply reply = _simulation.CallYala(message, now);
            Save();
            return new OperatorDispatchResult(channel, reply.Reply, reply, _simulation.OperatorState.Actions?.LastOrDefault());
        }

        string target = channel == "sophia" ? "wisdom" : channel;
        if (target == "gaia" && _simulation.State.Cosmic?.GaiaCreated != true)
        {
            OracleActionState action = _simulation.ActAsOracle($"Attempted contact with Gaia before Gaia existed: {message}");
            Save();
            return new OperatorDispatchResult(channel, "Gaia does not yet exist. The contact was not delivered.", null, action);
        }
        _simulation.CallEntity(target, message);
        Save();
        OracleActionState? latest = _simulation.OperatorState.Actions?.LastOrDefault();
        return new OperatorDispatchResult(channel, latest?.Result ?? "Contact recorded.", null, latest);
    }

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
        File.Copy(_savePath, Path.Combine(archive, $"save_v8_before_fresh_{stamp}.json"), overwrite: false);
    }

    public void Dispose()
    {
        Save();
        _simulation.Dispose();
    }
}
