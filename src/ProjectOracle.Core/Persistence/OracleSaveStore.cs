using System.Text.Json;

namespace ProjectOracle.Persistence;

public sealed class OracleSaveStore
{
    public const string SaveFormat = "PROJECT_ORACLE_SAVE";
    public const int CurrentSchemaVersion = 8;
    private static readonly HashSet<string> SupportedProjectVersions = new(StringComparer.Ordinal)
    {
        ProjectVersion.Number
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    public bool Exists(string path) => File.Exists(path);

    public OracleSaveSnapshot Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            return ReadAndValidate(path, "The save file did not contain a Project Oracle snapshot.");
        }
        catch (Exception primaryError) when (primaryError is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
        {
            string backupPath = BackupPath(path);
            if (!File.Exists(backupPath))
            {
                throw new InvalidDataException($"The save could not be loaded: {primaryError.Message}", primaryError);
            }

            try
            {
                return ReadAndValidate(backupPath, "The last-good backup did not contain a Project Oracle snapshot.");
            }
            catch (Exception backupError) when (backupError is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
            {
                throw new InvalidDataException(
                    $"The save and its last-good backup could not be loaded. Save: {primaryError.Message} Backup: {backupError.Message}",
                    backupError);
            }
        }
    }

    public void Save(string path, OracleSaveSnapshot snapshot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(snapshot);
        Validate(snapshot);

        string fullPath = Path.GetFullPath(path);
        string? directory = Path.GetDirectoryName(fullPath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new InvalidOperationException("The save path has no usable directory.");
        }

        Directory.CreateDirectory(directory);
        string temporaryPath = fullPath + ".tmp";
        string backupPath = BackupPath(fullPath);
        string json = JsonSerializer.Serialize(snapshot, JsonOptions);

        File.WriteAllText(temporaryPath, json);

        try
        {
            if (File.Exists(fullPath))
            {
                try
                {
                    ReadAndValidate(fullPath, "The existing save did not contain a Project Oracle snapshot.");
                    File.Copy(fullPath, backupPath, overwrite: true);
                }
                catch (JsonException)
                {
                    // Never let a corrupt primary overwrite a healthy last-good backup.
                }
                catch (InvalidDataException)
                {
                    // Never let a corrupt primary overwrite a healthy last-good backup.
                }
            }

            File.Move(temporaryPath, fullPath, overwrite: true);
        }
        catch
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }

            throw;
        }
    }

    public static string DefaultPath()
    {
        string localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localData))
        {
            localData = AppContext.BaseDirectory;
        }

        // v0.0.26 intentionally starts a new experimental save line so Brain Slice 9 can be
        // observed without importing decisions or conversation history from accepted v0.0.24. Older save lines remain untouched.
        return Path.Combine(localData, "ProjectOracle", "save_v8.json");
    }

    public static string BackupPath(string path) => Path.GetFullPath(path) + ".backup.json";

    private static OracleSaveSnapshot ReadAndValidate(string path, string emptyMessage)
    {
        string json = File.ReadAllText(path);
        OracleSaveSnapshot snapshot = JsonSerializer.Deserialize<OracleSaveSnapshot>(json, JsonOptions)
            ?? throw new InvalidDataException(emptyMessage);

        // Validate version/schema before any normalisation. This prevents an old
        // legacy save line from being transformed into the fresh v0.0.26 save_v8 world.
        Validate(snapshot);
        return snapshot with { World = ProjectOracle.Domain.WorldDefaults.Normalise(snapshot.World) };
    }

    private static void Validate(OracleSaveSnapshot snapshot)
    {
        if (snapshot.World is null || snapshot.Records is null || snapshot.Interventions is null)
        {
            throw new InvalidDataException("The save is missing required world history.");
        }

        if (!string.Equals(snapshot.Format, SaveFormat, StringComparison.Ordinal))
        {
            throw new InvalidDataException("The save format is not recognised.");
        }

        if (snapshot.SchemaVersion != CurrentSchemaVersion)
        {
            throw new InvalidDataException(
                $"Save schema {snapshot.SchemaVersion} is not supported by schema {CurrentSchemaVersion}. v0.0.26 uses the fresh save_v8 world line only. Earlier saves are preserved but are not migrated into this experiment.");
        }

        if (!SupportedProjectVersions.Contains(snapshot.ProjectVersion))
        {
            throw new InvalidDataException(
                $"Save version {snapshot.ProjectVersion} is not supported by Project Oracle v{ProjectVersion.Number}. v0.0.26 accepts current save_v8 worlds only. Earlier saves remain preserved on disk.");
        }

        if (snapshot.World.Cosmic is null)
        {
            throw new InvalidDataException("The save_v8 world is missing required cosmic state.");
        }

        if (snapshot.World.Seed != snapshot.Seed)
        {
            throw new InvalidDataException("The save contains conflicting world seeds.");
        }

        if (snapshot.World.WorldMilliseconds != snapshot.WorldMilliseconds)
        {
            throw new InvalidDataException("The save contains conflicting world-clock values.");
        }

        if (snapshot.WorldMilliseconds < 0 || snapshot.LastRealUnixMilliseconds < 0)
        {
            throw new InvalidDataException("The save contains an invalid world clock.");
        }
    }
}
