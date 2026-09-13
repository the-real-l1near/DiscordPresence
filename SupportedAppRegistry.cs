using System.Diagnostics;

namespace DiscordPresence;

internal sealed class SupportedAppRegistry
{
    // =========================================================
    // Profiles
    // =========================================================

    private static readonly Dictionary<string, AppPresenceProfile>
        Profiles = new(StringComparer.OrdinalIgnoreCase)
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

    // =========================================================
    // Lookup
    // =========================================================

    public bool TryGetProfile(
        string processName,
        out AppPresenceProfile profile)
    {
        return Profiles.TryGetValue(
            processName,
            out profile!
        );
    }

    public bool IsSupportedProcess(
        string processName)
    {
        return Profiles.ContainsKey(
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
                    if (!Profiles.ContainsKey(
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
}