using System.Text;
using System.Text.Json;
using ProjectOracle;
using ProjectOracle.Audit;
using ProjectOracle.Domain;
using ProjectOracle.Simulation;

namespace ProjectOracle.Export;

public static class OracleSessionExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    public static string ExportJson(OracleSimulation simulation, string savePath, string? outputDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(simulation);
        YalaCognitionState cognition = simulation.State.YalaCognition ?? WorldDefaults.CreateInitialYalaCognition();
        DateTimeOffset exportedAt = DateTimeOffset.Now;
        string directory = string.IsNullOrWhiteSpace(outputDirectory) ? ExportDirectory() : Path.GetFullPath(outputDirectory);
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, $"Project_Oracle_v{ProjectVersion.Number}_Session_{exportedAt:yyyyMMdd_HHmmss_fff}.json");

        object envelope = new
        {
            export_format = "PROJECT_ORACLE_COGNITIVE_FLIGHT_RECORDER",
            export_schema = 1,
            project_version = ProjectVersion.Number,
            project_name = ProjectVersion.Name,
            exported_at_local = exportedAt,
            save_path = savePath,
            save_schema = ProjectOracle.Persistence.OracleSaveStore.CurrentSchemaVersion,
            world_seed = simulation.State.Seed,
            world_milliseconds = simulation.Clock.WorldMilliseconds,
            in_world_time_exists = simulation.InWorldTimeExists,
            in_world_time = simulation.InWorldTimeExists
                ? simulation.Clock.Calendar.DescribeDateAndTime()
                : "Gaia has not yet created Time.",
            conversation = BuildConversationTimeline(simulation),
            recent_dialogue_memory = cognition.Dialogue ?? [],
            cognitive_flight_recorder = cognition.DecisionTrace ?? [],
            current_yala_cognition = cognition,
            current_world = simulation.State,
            world_record = simulation.Ledger.WorldRecords,
            protected_oracle_record = simulation.Ledger.OracleRecords,
            offered_choices = simulation.OfferedChoices,
            reasoned_plans = simulation.ReasonedPlans,
            observations = simulation.Observations,
            attention_states = simulation.AttentionStates,
            soar_memory_diagnostics = SafeMemoryDiagnostics(simulation)
        };

        File.WriteAllText(path, JsonSerializer.Serialize(envelope, JsonOptions));
        return path;
    }

    public static string ExportConversationText(OracleSimulation simulation, string? outputDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(simulation);
        DateTimeOffset exportedAt = DateTimeOffset.Now;
        string directory = string.IsNullOrWhiteSpace(outputDirectory) ? ExportDirectory() : Path.GetFullPath(outputDirectory);
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, $"Project_Oracle_v{ProjectVersion.Number}_Conversation_{exportedAt:yyyyMMdd_HHmmss_fff}.txt");

        StringBuilder builder = new();
        builder.AppendLine($"Project Oracle v{ProjectVersion.Number}");
        builder.AppendLine($"World Seed: {simulation.State.Seed}");
        builder.AppendLine($"In-world Time: {(simulation.InWorldTimeExists ? simulation.Clock.Calendar.DescribeDateAndTime() : "Gaia has not yet created Time.")}");
        builder.AppendLine($"Exported: {exportedAt:O}");
        builder.AppendLine();

        foreach (ConversationEvent item in BuildConversationTimeline(simulation))
        {
            builder.AppendLine($"{item.Speaker}: {item.Text}");
            builder.AppendLine();
        }

        File.WriteAllText(path, builder.ToString());
        return path;
    }


    private static IReadOnlyList<ConversationEvent> BuildConversationTimeline(OracleSimulation simulation)
    {
        List<ConversationEvent> timeline = [];
        foreach (OracleRecord record in simulation.Ledger.AllRecords.OrderBy(item => item.Sequence))
        {
            string speaker;
            if (record.Audience == RecordAudience.Oracle && record.Category == "DIRECT CONTACT")
            {
                speaker = "You";
            }
            else if (record.Audience == RecordAudience.World && record.Category is "YALA SPEECH" or "YALA QUESTION")
            {
                speaker = "Yala";
            }
            else
            {
                continue;
            }

            string text = ExtractQuotedPayload(record.Message);
            timeline.Add(new ConversationEvent(
                record.Sequence,
                record.Tick,
                speaker,
                text,
                record.Category,
                record.Audience.ToString()));
        }
        return timeline;
    }

    private static string ExtractQuotedPayload(string message)
    {
        int first = message.IndexOf('\"');
        int last = message.LastIndexOf('\"');
        return first >= 0 && last > first
            ? message[(first + 1)..last]
            : message;
    }

    private sealed record ConversationEvent(
        long Sequence,
        long WorldMilliseconds,
        string Speaker,
        string Text,
        string SourceCategory,
        string SourceAudience);

    private static object SafeMemoryDiagnostics(OracleSimulation simulation)
    {
        try
        {
            return simulation.GetYalaMemoryDiagnostics();
        }
        catch (Exception error)
        {
            return new { unavailable = true, reason = error.GetBaseException().Message };
        }
    }

    private static string ExportDirectory()
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrWhiteSpace(home)) home = AppContext.BaseDirectory;
        string downloads = Path.Combine(home, "Downloads");
        string parent = Directory.Exists(downloads) ? downloads : Path.Combine(home, "Documents");
        return Path.Combine(parent, "Project Oracle Exports");
    }
}
