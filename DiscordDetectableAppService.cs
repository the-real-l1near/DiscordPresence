using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscordPresence;

internal sealed class DiscordDetectableAppService
{
    // =========================================================
    // Discord API
    // =========================================================

    private const string DetectableAppsUrl =
        "https://discord.com/api/v10/applications/detectable";

    private static readonly TimeSpan CacheLifetime =
        TimeSpan.FromHours(24);

    private static readonly HttpClient HttpClient =
        CreateHttpClient();

    // =========================================================
    // Cache
    // =========================================================

    private readonly string _cacheDirectory;

    private readonly string _cacheFilePath;

    // =========================================================
    // Lookup
    // =========================================================

    private readonly object _sync =
        new();

    private Dictionary<string, DiscordDetectableApp>
        _windowsExecutables =
            new(StringComparer.OrdinalIgnoreCase);

    // =========================================================
    // Refresh state
    // =========================================================

    private int _refreshInProgress;

    // =========================================================
    // Constructor
    // =========================================================

    public DiscordDetectableAppService()
    {
        _cacheDirectory =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData
                ),
                "DiscordPresence",
                "Cache"
            );

        _cacheFilePath =
            Path.Combine(
                _cacheDirectory,
                "discord-detectable-apps.json"
            );

        LoadCache();

        StartBackgroundRefresh();
    }

    // =========================================================
    // Public lookup
    // =========================================================

    public bool TryGetByProcessName(
        string processName,
        out DiscordDetectableApp app)
    {
        var normalizedName =
            NormalizeProcessName(
                processName
            );

        lock (_sync)
        {
            return _windowsExecutables.TryGetValue(
                normalizedName,
                out app!
            );
        }
    }

    public bool IsDetectableProcess(
        string processName)
    {
        return TryGetByProcessName(
            processName,
            out _
        );
    }

    // =========================================================
    // Background refresh
    // =========================================================

    public void StartBackgroundRefresh()
    {
        if (!ShouldRefreshCache())
        {
            return;
        }

        if (Interlocked.CompareExchange(
            ref _refreshInProgress,
            1,
            0
        ) != 0)
        {
            return;
        }

        _ = Task.Run(
            async () =>
            {
                try
                {
                    await RefreshAsync();
                }
                catch
                {
                    /*
                     * Detectable-app data is optional.
                     *
                     * Network/API failure must never break
                     * the presence application.
                     *
                     * Existing cache remains active.
                     */
                }
                finally
                {
                    Interlocked.Exchange(
                        ref _refreshInProgress,
                        0
                    );
                }
            }
        );
    }

    // =========================================================
    // Refresh
    // =========================================================

    private async Task RefreshAsync()
    {
        using var response =
            await HttpClient.GetAsync(
                DetectableAppsUrl
            );

        response.EnsureSuccessStatusCode();

        var json =
            await response.Content
                .ReadAsStringAsync();

        var lookup =
            ParseLookup(
                json
            );

        /*
         * Không overwrite cache tốt bằng response
         * rỗng / schema Discord thay đổi.
         */
        if (lookup.Count == 0)
        {
            return;
        }

        lock (_sync)
        {
            _windowsExecutables =
                lookup;
        }

        SaveCache(
            json
        );
    }

    // =========================================================
    // Cache loading
    // =========================================================

    private void LoadCache()
    {
        if (!File.Exists(
            _cacheFilePath
        ))
        {
            return;
        }

        try
        {
            var json =
                File.ReadAllText(
                    _cacheFilePath
                );

            var lookup =
                ParseLookup(
                    json
                );

            if (lookup.Count == 0)
            {
                return;
            }

            lock (_sync)
            {
                _windowsExecutables =
                    lookup;
            }
        }
        catch
        {
            /*
             * Corrupt cache:
             * ignore và refresh lại từ Discord.
             */
        }
    }

    // =========================================================
    // Cache saving
    // =========================================================

    private void SaveCache(
        string json)
    {
        try
        {
            Directory.CreateDirectory(
                _cacheDirectory
            );

            var temporaryPath =
                _cacheFilePath +
                ".tmp";

            File.WriteAllText(
                temporaryPath,
                json
            );

            File.Move(
                temporaryPath,
                _cacheFilePath,
                overwrite: true
            );
        }
        catch
        {
            /*
             * Không ghi được cache cũng không phải
             * fatal error.
             *
             * Lookup vừa tải vẫn dùng được trong RAM.
             */
        }
    }

    // =========================================================
    // Cache expiration
    // =========================================================

    private bool ShouldRefreshCache()
    {
        if (!File.Exists(
            _cacheFilePath
        ))
        {
            return true;
        }

        try
        {
            var lastWriteTime =
                File.GetLastWriteTimeUtc(
                    _cacheFilePath
                );

            return
                DateTime.UtcNow -
                lastWriteTime >=
                CacheLifetime;
        }
        catch
        {
            return true;
        }
    }

    // =========================================================
    // JSON parsing
    // =========================================================

    private static Dictionary<string, DiscordDetectableApp>
        ParseLookup(
            string json)
    {
        var lookup =
            new Dictionary<string, DiscordDetectableApp>(
                StringComparer.OrdinalIgnoreCase
            );

        DiscordDetectableApplicationDto[]? applications;

        try
        {
            applications =
                JsonSerializer.Deserialize<
                    DiscordDetectableApplicationDto[]
                >(
                    json
                );
        }
        catch
        {
            return lookup;
        }

        if (applications is null)
        {
            return lookup;
        }

        foreach (var application in applications)
        {
            if (string.IsNullOrWhiteSpace(
                application.Id
            ))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(
                application.Name
            ))
            {
                continue;
            }

            if (application.Executables is null)
            {
                continue;
            }

            var app =
                new DiscordDetectableApp(
                    application.Id,
                    application.Name
                );

            foreach (var executable in
                application.Executables)
            {
                if (!string.Equals(
                    executable.Os,
                    "win32",
                    StringComparison.OrdinalIgnoreCase
                ))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(
                    executable.Name
                ))
                {
                    continue;
                }

                var processName =
                    NormalizeProcessName(
                        executable.Name
                    );

                if (string.IsNullOrWhiteSpace(
                    processName
                ))
                {
                    continue;
                }

                /*
                 * Có thể có collision giữa nhiều app.
                 *
                 * GameDetector v2 hiện chỉ cần biết
                 * executable có nằm trong database hay
                 * không, nên giữ entry đầu tiên.
                 */
                lookup.TryAdd(
                    processName,
                    app
                );
            }
        }

        return lookup;
    }

    // =========================================================
    // Process-name normalization
    // =========================================================

    private static string NormalizeProcessName(
        string value)
    {
        if (string.IsNullOrWhiteSpace(
            value
        ))
        {
            return string.Empty;
        }

        /*
         * Discord có thể trả:
         *
         * game.exe
         * game/game.exe
         * folder\game.exe
         *
         * Process.ProcessName lại trả "game".
         */
        var normalizedPath =
            value
                .Trim()
                .Replace(
                    '/',
                    '\\'
                );

        var fileName =
            normalizedPath[
                (
                    normalizedPath.LastIndexOf(
                        '\\'
                    ) + 1
                )..
            ];

        if (fileName.EndsWith(
            ".exe",
            StringComparison.OrdinalIgnoreCase
        ))
        {
            fileName =
                fileName[..^4];
        }

        return fileName.Trim();
    }

    // =========================================================
    // HttpClient
    // =========================================================

    private static HttpClient CreateHttpClient()
    {
        var client =
            new HttpClient
            {
                Timeout =
                    TimeSpan.FromSeconds(15)
            };

        client.DefaultRequestHeaders
            .UserAgent
            .ParseAdd(
                "DiscordPresence/1.0"
            );

        return client;
    }

    // =========================================================
    // Discord DTOs
    // =========================================================

    private sealed class DiscordDetectableApplicationDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("executables")]
        public DiscordExecutableDto[]? Executables { get; set; }
    }

    private sealed class DiscordExecutableDto
    {
        [JsonPropertyName("os")]
        public string? Os { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}