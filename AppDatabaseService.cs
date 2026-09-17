using System.Net.NetworkInformation;
using System.Text.Json;

namespace DiscordPresence;

internal sealed class AppDatabaseService
{
    // =========================================================
    // Remote database
    // =========================================================

    private const string DatabaseUrl =
        "https://raw.githubusercontent.com/the-real-l1near/DiscordPresence-AppDatabase/main/database.json";

    private const string EmbeddedDatabaseResourceName =
        "DiscordPresence.AppDatabaseSnapshot.json";

    private const int SupportedSchemaVersion =
        1;

    private static readonly HttpClient HttpClient =
        new()
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(5)
    ];

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
    // Load embedded snapshot
    // =========================================================

    public IReadOnlyList<AppDatabaseEntry>?
        LoadEmbeddedEntries()
    {
        try
        {
            using var stream =
                typeof(AppDatabaseService)
                    .Assembly
                    .GetManifestResourceStream(
                        EmbeddedDatabaseResourceName
                    );

            if (stream is null)
            {
                return null;
            }

            using var reader =
                new StreamReader(stream);

            var json =
                reader.ReadToEnd();

            return ParseEntries(json);
        }
        catch
        {
            return null;
        }
    }

    // =========================================================
    // Load cache
    // =========================================================

    public IReadOnlyList<AppDatabaseEntry>?
        LoadCachedEntries()
    {
        try
        {
            if (!File.Exists(CachePath))
            {
                return null;
            }

            var json =
                File.ReadAllText(CachePath);

            return ParseEntries(json);
        }
        catch
        {
            return null;
        }
    }

    // =========================================================
    // Refresh remote database
    // =========================================================

    public async Task<IReadOnlyList<AppDatabaseEntry>?>
        FetchLatestEntriesAsync()
    {
        var networkAvailableSignal =
            CreateNetworkAvailableSignal();

        void HandleNetworkAvailabilityChanged(
            object? sender,
            NetworkAvailabilityEventArgs args)
        {
            if (!args.IsAvailable)
            {
                return;
            }

            networkAvailableSignal.TrySetResult(
                true
            );
        }

        NetworkChange.NetworkAvailabilityChanged +=
            HandleNetworkAvailabilityChanged;

        try
        {
            var retryIndex =
                0;

            while (true)
            {
                var entries =
                    await TryFetchLatestEntriesAsync();

                if (entries is not null)
                {
                    return entries;
                }

                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    await networkAvailableSignal.Task;

                    networkAvailableSignal =
                        CreateNetworkAvailableSignal();

                    retryIndex =
                        0;

                    continue;
                }

                var delay =
                    RetryDelays[
                        Math.Min(
                            retryIndex,
                            RetryDelays.Length - 1
                        )
                    ];

                if (retryIndex <
                    RetryDelays.Length - 1)
                {
                    retryIndex++;
                }

                var networkTask =
                    networkAvailableSignal.Task;

                var delayTask =
                    Task.Delay(delay);

                await Task.WhenAny(
                    networkTask,
                    delayTask
                );

                if (networkTask.IsCompleted)
                {
                    networkAvailableSignal =
                        CreateNetworkAvailableSignal();

                    retryIndex =
                        0;
                }
            }
        }
        finally
        {
            NetworkChange.NetworkAvailabilityChanged -=
                HandleNetworkAvailabilityChanged;
        }
    }

    private static async Task<IReadOnlyList<AppDatabaseEntry>?>
        TryFetchLatestEntriesAsync()
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

            var entries =
                ParseEntries(json);

            if (entries is null)
            {
                return null;
            }

            await SaveCacheAsync(json);

            return entries;
        }
        catch
        {
            return null;
        }
    }

    private static TaskCompletionSource<bool>
        CreateNetworkAvailableSignal()
    {
        return new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
    }

    // =========================================================
    // Parse
    // =========================================================

    private static IReadOnlyList<AppDatabaseEntry>?
        ParseEntries(
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
                schemaVersionElement.ValueKind !=
                    JsonValueKind.Number ||
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

            var entries =
                new List<AppDatabaseEntry>();

            var appIds =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );

            foreach (var appElement in
                appsElement.EnumerateArray())
            {
                if (!TryCreateEntry(
                    appElement,
                    out var entry
                ))
                {
                    return null;
                }

                if (!appIds.Add(
                    entry.Id
                ))
                {
                    return null;
                }

                entries.Add(entry);
            }

            return entries.Count > 0
                ? entries
                : null;
        }
        catch
        {
            return null;
        }
    }

    private static bool TryCreateEntry(
        JsonElement appElement,
        out AppDatabaseEntry entry)
    {
        entry =
            null!;

        if (
            appElement.ValueKind !=
                JsonValueKind.Object ||
            !TryGetRequiredString(
                appElement,
                "id",
                out var appId
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
            presenceElement.ValueKind !=
                JsonValueKind.Object ||
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

        var rules =
            new List<AppMatchRule>();

        foreach (var ruleElement in
            matchElement.EnumerateArray())
        {
            if (
                ruleElement.ValueKind !=
                    JsonValueKind.Object ||
                !TryGetRequiredString(
                    ruleElement,
                    "processName",
                    out var processName
                ) ||
                !TryGetOptionalString(
                    ruleElement,
                    "productName",
                    out var productName
                ) ||
                !TryGetOptionalString(
                    ruleElement,
                    "originalFilename",
                    out var originalFilename
                )
            )
            {
                return false;
            }

            rules.Add(
                new AppMatchRule(
                    processName,
                    productName,
                    originalFilename
                )
            );
        }

        if (rules.Count == 0)
        {
            return false;
        }

        entry =
            new AppDatabaseEntry(
                appId,
                new AppPresenceProfile(
                    displayName,
                    largeImage,
                    displayName
                ),
                rules
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

    private static bool TryGetOptionalString(
        JsonElement element,
        string propertyName,
        out string? value)
    {
        value =
            null;

        if (!element.TryGetProperty(
            propertyName,
            out var propertyElement
        ))
        {
            return true;
        }

        if (propertyElement.ValueKind ==
            JsonValueKind.Null)
        {
            return true;
        }

        if (propertyElement.ValueKind !=
            JsonValueKind.String)
        {
            return false;
        }

        var parsed =
            propertyElement.GetString()?.Trim();

        if (string.IsNullOrWhiteSpace(
            parsed
        ))
        {
            return false;
        }

        value =
            parsed;

        return true;
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
