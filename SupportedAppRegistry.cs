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

    private static IReadOnlyDictionary<string, AppPresenceProfile>
        _profiles =
            CreateFallbackProfiles();

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

        var cachedProfiles =
            Database.LoadCachedProfiles();

        if (cachedProfiles is not null)
        {
            _profiles =
                cachedProfiles;
        }

        _ = RefreshFromRemoteAsync();
    }

    private static async Task RefreshFromRemoteAsync()
    {
        var latestProfiles =
            await Database
                .FetchLatestProfilesAsync();

        if (latestProfiles is null)
        {
            return;
        }

        _profiles =
            latestProfiles;
    }

    // =========================================================
    // Lookup
    // =========================================================

    public bool TryGetProfile(
        string processName,
        out AppPresenceProfile profile)
    {
        return _profiles.TryGetValue(
            processName,
            out profile!
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

        return _profiles.ContainsKey(
            processName
        );
    }

    // =========================================================
    // Running applications
    // =========================================================

    public bool HasSupportedAppRunning()
    {
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
                    if (!_profiles.ContainsKey(
                        process.ProcessName
                    ))
                    {
                        continue;
                    }

                    if (process.MainWindowHandle !=
                        IntPtr.Zero)
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

    // =========================================================
    // Embedded fallback
    // =========================================================

    private static IReadOnlyDictionary<string, AppPresenceProfile>
        CreateFallbackProfiles()
    {
        return new Dictionary<string, AppPresenceProfile>(
            StringComparer.OrdinalIgnoreCase
        )
        {
            ["Code"] = new(
                "Visual Studio Code",
                "vscode",
                "Visual Studio Code"
            ),

            ["blender"] = new(
                "Blender",
                "blender",
                "Blender"
            ),

            ["UnrealEditor"] = new(
                "Unreal Engine",
                "unreal_v2",
                "Unreal Engine"
            ),

            ["idea64"] = new(
                "IntelliJ IDEA",
                "intellij",
                "IntelliJ IDEA"
            )
        };
    }
}
