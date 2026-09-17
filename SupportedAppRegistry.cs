using System.Diagnostics;

namespace DiscordPresence;

internal sealed class SupportedAppRegistry
{
    // =========================================================
    // Database
    // =========================================================

    private static readonly AppDatabaseService
        Database =
            new();

    private static volatile IReadOnlyDictionary<
        string,
        IReadOnlyList<AppDatabaseEntry>
    > _appsByProcess =
        BuildIndex(
            CreateFallbackEntries()
        );

    private static int
        _initializationStarted;

    // =========================================================
    // Constructor
    // =========================================================

    public SupportedAppRegistry()
    {
        EnsureInitialized();
    }

    // =========================================================
    // Initialize
    // =========================================================

    private static void EnsureInitialized()
    {
        if (Interlocked.Exchange(
            ref _initializationStarted,
            1
        ) != 0)
        {
            return;
        }

        var cachedEntries =
            Database.LoadCachedEntries();

        if (cachedEntries is not null)
        {
            _appsByProcess =
                BuildIndex(cachedEntries);
        }

        _ = RefreshFromRemoteAsync();
    }

    private static async Task RefreshFromRemoteAsync()
    {
        var latestEntries =
            await Database
                .FetchLatestEntriesAsync();

        if (latestEntries is null)
        {
            return;
        }

        _appsByProcess =
            BuildIndex(latestEntries);
    }

    // =========================================================
    // Lookup
    // =========================================================

    public bool TryGetProfile(
        string processName,
        out AppPresenceProfile profile)
    {
        EnsureInitialized();

        return TryResolveRunningProcess(
            processName,
            out profile
        );
    }

    public bool IsSupportedProcess(
        string processName)
    {
        return IsKnownProcess(
            processName
        );
    }

    public static bool IsKnownProcess(
        string processName)
    {
        EnsureInitialized();

        return TryResolveRunningProcess(
            processName,
            out _
        );
    }

    // =========================================================
    // Running applications
    // =========================================================

    public bool HasSupportedAppRunning()
    {
        EnsureInitialized();

        var appsByProcess =
            _appsByProcess;

        Process[] processes;

        try
        {
            processes =
                Process.GetProcesses();
        }
        catch
        {
            return false;
        }

        try
        {
            foreach (var process in processes)
            {
                try
                {
                    if (!appsByProcess.ContainsKey(
                        process.ProcessName
                    ))
                    {
                        continue;
                    }

                    if (process.MainWindowHandle ==
                        IntPtr.Zero)
                    {
                        continue;
                    }

                    if (TryResolveProcess(
                        process,
                        appsByProcess,
                        out _
                    ))
                    {
                        return true;
                    }
                }
                catch
                {
                }
            }

            return false;
        }
        finally
        {
            foreach (var process in processes)
            {
                process.Dispose();
            }
        }
    }

    private static bool TryResolveRunningProcess(
        string processName,
        out AppPresenceProfile profile)
    {
        profile =
            null!;

        var appsByProcess =
            _appsByProcess;

        if (!appsByProcess.ContainsKey(
            processName
        ))
        {
            return false;
        }

        Process[] processes;

        try
        {
            processes =
                Process.GetProcessesByName(
                    processName
                );
        }
        catch
        {
            return false;
        }

        string? resolvedId =
            null;

        AppPresenceProfile? resolvedProfile =
            null;

        try
        {
            foreach (var process in processes)
            {
                if (!TryResolveProcess(
                    process,
                    appsByProcess,
                    out var result
                ))
                {
                    continue;
                }

                if (resolvedId is null)
                {
                    resolvedId =
                        result.Id;

                    resolvedProfile =
                        result.Profile;

                    continue;
                }

                if (!string.Equals(
                    resolvedId,
                    result.Id,
                    StringComparison.OrdinalIgnoreCase
                ))
                {
                    return false;
                }
            }
        }
        finally
        {
            foreach (var process in processes)
            {
                process.Dispose();
            }
        }

        if (resolvedProfile is null)
        {
            return false;
        }

        profile =
            resolvedProfile;

        return true;
    }

    private static bool TryResolveProcess(
        Process process,
        IReadOnlyDictionary<
            string,
            IReadOnlyList<AppDatabaseEntry>
        > appsByProcess,
        out AppDatabaseEntry entry)
    {
        entry =
            null!;

        string processName;

        try
        {
            processName =
                process.ProcessName;
        }
        catch
        {
            return false;
        }

        if (!appsByProcess.TryGetValue(
            processName,
            out var candidates
        ))
        {
            return false;
        }

        var metadata =
            ReadProcessMetadata(
                process,
                processName
            );

        AppDatabaseEntry? resolved =
            null;

        foreach (var candidate in candidates)
        {
            if (!MatchesEntry(
                candidate,
                metadata
            ))
            {
                continue;
            }

            if (resolved is null)
            {
                resolved =
                    candidate;

                continue;
            }

            if (!string.Equals(
                resolved.Id,
                candidate.Id,
                StringComparison.OrdinalIgnoreCase
            ))
            {
                return false;
            }
        }

        if (resolved is null)
        {
            return false;
        }

        entry =
            resolved;

        return true;
    }

    private static bool MatchesEntry(
        AppDatabaseEntry entry,
        ProcessMetadata metadata)
    {
        foreach (var rule in
            entry.MatchRules)
        {
            if (!string.Equals(
                rule.ProcessName,
                metadata.ProcessName,
                StringComparison.OrdinalIgnoreCase
            ))
            {
                continue;
            }

            if (
                rule.ProductName is not null &&
                !string.Equals(
                    rule.ProductName,
                    metadata.ProductName,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                continue;
            }

            if (
                rule.OriginalFilename is not null &&
                !string.Equals(
                    rule.OriginalFilename,
                    metadata.OriginalFilename,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static ProcessMetadata ReadProcessMetadata(
        Process process,
        string processName)
    {
        try
        {
            var fileName =
                process.MainModule?.FileName;

            if (string.IsNullOrWhiteSpace(
                fileName
            ))
            {
                return new ProcessMetadata(
                    processName,
                    null,
                    null
                );
            }

            var versionInfo =
                FileVersionInfo.GetVersionInfo(
                    fileName
                );

            return new ProcessMetadata(
                processName,
                NormalizeMetadata(
                    versionInfo.ProductName
                ),
                NormalizeMetadata(
                    versionInfo.OriginalFilename
                )
            );
        }
        catch
        {
            return new ProcessMetadata(
                processName,
                null,
                null
            );
        }
    }

    private static string? NormalizeMetadata(
        string? value)
    {
        var normalized =
            value?.Trim();

        return string.IsNullOrWhiteSpace(
            normalized
        )
            ? null
            : normalized;
    }

    // =========================================================
    // Index
    // =========================================================

    private static IReadOnlyDictionary<
        string,
        IReadOnlyList<AppDatabaseEntry>
    > BuildIndex(
        IReadOnlyList<AppDatabaseEntry> entries)
    {
        var index =
            new Dictionary<
                string,
                List<AppDatabaseEntry>
            >(
                StringComparer.OrdinalIgnoreCase
            );

        foreach (var entry in entries)
        {
            var processNames =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );

            foreach (var rule in
                entry.MatchRules)
            {
                processNames.Add(
                    rule.ProcessName
                );
            }

            foreach (var processName in
                processNames)
            {
                if (!index.TryGetValue(
                    processName,
                    out var candidates
                ))
                {
                    candidates =
                        new List<AppDatabaseEntry>();

                    index[processName] =
                        candidates;
                }

                candidates.Add(entry);
            }
        }

        return index.ToDictionary(
            pair => pair.Key,
            pair =>
                (IReadOnlyList<AppDatabaseEntry>)
                    pair.Value,
            StringComparer.OrdinalIgnoreCase
        );
    }

    // =========================================================
    // Embedded fallback
    // =========================================================

    private static IReadOnlyList<AppDatabaseEntry>
        CreateFallbackEntries()
    {
        return
        [
            new AppDatabaseEntry(
                "visual-studio-code",
                new AppPresenceProfile(
                    "Visual Studio Code",
                    "vscode",
                    "Visual Studio Code"
                ),
                [
                    new AppMatchRule(
                        "Code",
                        null,
                        null
                    )
                ]
            ),

            new AppDatabaseEntry(
                "blender",
                new AppPresenceProfile(
                    "Blender",
                    "blender",
                    "Blender"
                ),
                [
                    new AppMatchRule(
                        "blender",
                        null,
                        null
                    )
                ]
            ),

            new AppDatabaseEntry(
                "unreal-engine",
                new AppPresenceProfile(
                    "Unreal Engine",
                    "unreal_v2",
                    "Unreal Engine"
                ),
                [
                    new AppMatchRule(
                        "UnrealEditor",
                        null,
                        null
                    )
                ]
            ),

            new AppDatabaseEntry(
                "intellij-idea",
                new AppPresenceProfile(
                    "IntelliJ IDEA",
                    "intellij",
                    "IntelliJ IDEA"
                ),
                [
                    new AppMatchRule(
                        "idea64",
                        null,
                        null
                    )
                ]
            )
        ];
    }

    private sealed record ProcessMetadata(
        string ProcessName,
        string? ProductName,
        string? OriginalFilename
    );
}
