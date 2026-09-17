using System.Text.Json;

namespace DiscordPresence;

internal sealed class AppDatabaseService
{
    // =========================================================
    // Remote database
    // =========================================================

    private const string DatabaseUrl =
        "https://raw.githubusercontent.com/the-real-l1near/DiscordPresence-AppDatabase/main/database.json";

    private const int SupportedSchemaVersion =
        1;

    private static readonly HttpClient HttpClient =
        new()
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

    // =========================================================
    // Cache
    // =========================================================

    private static readonly string CachePath =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData
            ),
            "DiscordPresence",
            "Cache",
            "app-database.json"
        );

    // =========================================================
    // Load cache
    // =========================================================

    public IReadOnlyDictionary<string, AppPresenceProfile>?
        LoadCachedProfiles()
    {
        try
        {
            if (!File.Exists(CachePath))
            {
                return null;
            }

            var json =
                File.ReadAllText(CachePath);

            return ParseProfiles(json);
        }
        catch
        {
            return null;
        }
    }

    // =========================================================
    // Refresh remote database
    // =========================================================

    public async Task<IReadOnlyDictionary<string, AppPresenceProfile>?>
        FetchLatestProfilesAsync()
    {
        try
        {
            using var response =
                await HttpClient.GetAsync(
                    DatabaseUrl
                );

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json =
                await response.Content
                    .ReadAsStringAsync();

            var profiles =
                ParseProfiles(json);

            if (profiles is null)
            {
                return null;
            }

            await SaveCacheAsync(json);

            return profiles;
        }
        catch
        {
            return null;
        }
    }

    // =========================================================
    // Parse
    // =========================================================

    private static IReadOnlyDictionary<string, AppPresenceProfile>?
        ParseProfiles(
            string json)
    {
        try
        {
            using var document =
                JsonDocument.Parse(json);

            var root =
                document.RootElement;

            if (
                !root.TryGetProperty(
                    "schemaVersion",
                    out var schemaVersionElement
                ) ||
                schemaVersionElement.GetInt32() !=
                    SupportedSchemaVersion ||
                !root.TryGetProperty(
                    "apps",
                    out var appsElement
                ) ||
                appsElement.ValueKind !=
                    JsonValueKind.Array
            )
            {
                return null;
            }

            var candidates =
                new Dictionary<
                    string,
                    (string AppId, AppPresenceProfile Profile)
                >(
                    StringComparer.OrdinalIgnoreCase
                );

            var ambiguousProcesses =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );

            foreach (var appElement in
                appsElement.EnumerateArray())
            {
                if (!TryCreateProfile(
                    appElement,
                    out var appId,
                    out var profile,
                    out var processNames
                ))
                {
                    continue;
                }

                foreach (var processName in
                    processNames)
                {
                    if (ambiguousProcesses.Contains(
                        processName
                    ))
                    {
                        continue;
                    }

                    if (candidates.TryGetValue(
                        processName,
                        out var existing
                    ))
                    {
                        if (string.Equals(
                            existing.AppId,
                            appId,
                            StringComparison.OrdinalIgnoreCase
                        ))
                        {
                            continue;
                        }

                        candidates.Remove(
                            processName
                        );

                        ambiguousProcesses.Add(
                            processName
                        );

                        continue;
                    }

                    candidates[processName] =
                        (appId, profile);
                }
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            return candidates.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.Profile,
                StringComparer.OrdinalIgnoreCase
            );
        }
        catch
        {
            return null;
        }
    }

    private static bool TryCreateProfile(
        JsonElement appElement,
        out string appId,
        out AppPresenceProfile profile,
        out HashSet<string> processNames)
    {
        appId =
            string.Empty;

        profile =
            null!;

        processNames =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase
            );

        if (
            !TryGetRequiredString(
                appElement,
                "id",
                out appId
            ) ||
            !TryGetRequiredString(
                appElement,
                "name",
                out var displayName
            ) ||
            !appElement.TryGetProperty(
                "match",
                out var matchElement
            ) ||
            matchElement.ValueKind !=
                JsonValueKind.Array ||
            !appElement.TryGetProperty(
                "presence",
                out var presenceElement
            ) ||
            !TryGetRequiredString(
                presenceElement,
                "largeImage",
                out var largeImage
            )
        )
        {
            return false;
        }

        if (
            !Uri.TryCreate(
                largeImage,
                UriKind.Absolute,
                out var imageUri
            ) ||
            imageUri.Scheme !=
                Uri.UriSchemeHttps
        )
        {
            return false;
        }

        foreach (var ruleElement in
            matchElement.EnumerateArray())
        {
            if (TryGetRequiredString(
                ruleElement,
                "processName",
                out var processName
            ))
            {
                processNames.Add(
                    processName
                );
            }
        }

        if (processNames.Count == 0)
        {
            return false;
        }

        profile =
            new AppPresenceProfile(
                displayName,
                largeImage,
                displayName
            );

        return true;
    }

    private static bool TryGetRequiredString(
        JsonElement element,
        string propertyName,
        out string value)
    {
        value =
            string.Empty;

        if (
            !element.TryGetProperty(
                propertyName,
                out var propertyElement
            ) ||
            propertyElement.ValueKind !=
                JsonValueKind.String
        )
        {
            return false;
        }

        value =
            propertyElement.GetString()?.Trim() ??
            string.Empty;

        return !string.IsNullOrWhiteSpace(
            value
        );
    }

    // =========================================================
    // Cache write
    // =========================================================

    private static async Task SaveCacheAsync(
        string json)
    {
        try
        {
            var directory =
                Path.GetDirectoryName(
                    CachePath
                );

            if (string.IsNullOrWhiteSpace(
                directory
            ))
            {
                return;
            }

            Directory.CreateDirectory(
                directory
            );

            var tempPath =
                CachePath + ".tmp";

            await File.WriteAllTextAsync(
                tempPath,
                json
            );

            File.Move(
                tempPath,
                CachePath,
                true
            );
        }
        catch
        {
        }
    }
}
