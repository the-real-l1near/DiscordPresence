using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DiscordPresence;

internal sealed class GameDetector
{
    // =========================================================
    // Win32
    // =========================================================

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(
        IntPtr hWnd,
        out RECT lpRect
    );

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(
        IntPtr hwnd,
        uint dwFlags
    );

    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(
        IntPtr hMonitor,
        ref MONITORINFO lpmi
    );

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(
        IntPtr hWnd,
        out uint lpdwProcessId
    );

    [DllImport(
        "user32.dll",
        CharSet = CharSet.Unicode
    )]
    private static extern int GetWindowText(
        IntPtr hWnd,
        char[] lpString,
        int nMaxCount
    );

    private const uint MonitorDefaultToNearest =
        0x00000002;

    // =========================================================
    // Services
    // =========================================================

    private readonly DiscordDetectableAppService
        _detectableApps;

    private readonly GameOverrideStore
        _overrides;

    // =========================================================
    // Built-in exclusions
    // =========================================================

    /*
     * Đây chỉ là những process mình đủ chắc rằng
     * không nên bao giờ auto-detect thành game.
     *
     * Supported applications are loaded from the public
     * application database and checked separately below.
     *
     * Unknown fullscreen app KHÔNG nằm đây sẽ thành
     * Suspected và user tự quyết định.
     */
    private static readonly HashSet<string>
        BuiltInExclusions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                // Discord
                "Discord",
                "DiscordCanary",
                "DiscordPTB",

                // Windows
                "explorer",

                // Common browsers
                "chrome",
                "msedge",
                "firefox",

                // This application
                "DiscordPresence"
            };

    // =========================================================
    // Constructor
    // =========================================================

    public GameDetector(
        DiscordDetectableAppService detectableApps,
        GameOverrideStore overrides)
    {
        _detectableApps =
            detectableApps;

        _overrides =
            overrides;
    }

    // =========================================================
    // Detection
    // =========================================================

    public GameDetectionResult DetectForeground()
    {
        var window =
            GetForegroundWindow();

        if (window == IntPtr.Zero)
        {
            return new GameDetectionResult(
                GameDetectionKind.NotGame
            );
        }

        GetWindowThreadProcessId(
            window,
            out var processId
        );

        if (processId == 0)
        {
            return new GameDetectionResult(
                GameDetectionKind.NotGame
            );
        }

        Process? process =
            null;

        try
        {
            process =
                Process.GetProcessById(
                    (int)processId
                );

            var processName =
                process.ProcessName;

            AppCacheService.Shared.ObserveProcess((int)processId, processName);

            var windowTitle =
                GetWindowTitle(
                    window
                );

            // -------------------------------------------------
            // User explicitly said NO
            // -------------------------------------------------

            if (_overrides.IsExcluded(
                processName
            ))
            {
                return new GameDetectionResult(
                    GameDetectionKind.NotGame,
                    processName,
                    windowTitle
                );
            }

            // -------------------------------------------------
            // User explicitly said YES
            // -------------------------------------------------

            if (_overrides.IsIncluded(
                processName
            ))
            {
                return new GameDetectionResult(
                    GameDetectionKind.Game,
                    processName,
                    windowTitle
                );
            }

            // -------------------------------------------------
            // Known non-games
            // -------------------------------------------------

            if (BuiltInExclusions.Contains(
                processName
            ))
            {
                return new GameDetectionResult(
                    GameDetectionKind.NotGame,
                    processName,
                    windowTitle
                );
            }

            // -------------------------------------------------
            // Supported application database
            // -------------------------------------------------

            if (SupportedAppRegistry.IsKnownProcess(
                processName
            ))
            {
                return new GameDetectionResult(
                    GameDetectionKind.NotGame,
                    processName,
                    windowTitle
                );
            }

            // -------------------------------------------------
            // Discord detectable database
            // -------------------------------------------------

            if (_detectableApps.IsDetectableProcess(
                processName
            ))
            {
                return new GameDetectionResult(
                    GameDetectionKind.Game,
                    processName,
                    windowTitle
                );
            }

            // -------------------------------------------------
            // Fullscreen / borderless heuristic
            // -------------------------------------------------

            if (IsFullscreenWindow(
                window
            ))
            {
                /*
                 * Quan trọng:
                 *
                 * fullscreen != game.
                 *
                 * Chỉ yêu cầu user xác nhận.
                 */
                return new GameDetectionResult(
                    GameDetectionKind.Suspected,
                    processName,
                    windowTitle
                );
            }

            // -------------------------------------------------
            // Not a game
            // -------------------------------------------------

            return new GameDetectionResult(
                GameDetectionKind.NotGame,
                processName,
                windowTitle
            );
        }
        catch
        {
            return new GameDetectionResult(
                GameDetectionKind.NotGame
            );
        }
        finally
        {
            process?.Dispose();
        }
    }

    // =========================================================
    // Window title
    // =========================================================

    private static string? GetWindowTitle(
        IntPtr window)
    {
        var buffer =
            new char[512];

        var length =
            GetWindowText(
                window,
                buffer,
                buffer.Length
            );

        if (length <= 0)
        {
            return null;
        }

        var title =
            new string(
                buffer,
                0,
                length
            )
            .Trim();

        return string.IsNullOrWhiteSpace(
            title
        )
            ? null
            : title;
    }

    // =========================================================
    // Fullscreen / borderless
    // =========================================================

    private static bool IsFullscreenWindow(
        IntPtr window)
    {
        if (!GetWindowRect(
            window,
            out var windowRect
        ))
        {
            return false;
        }

        var monitor =
            MonitorFromWindow(
                window,
                MonitorDefaultToNearest
            );

        if (monitor == IntPtr.Zero)
        {
            return false;
        }

        var monitorInfo =
            new MONITORINFO
            {
                cbSize =
                    Marshal.SizeOf<MONITORINFO>()
            };

        if (!GetMonitorInfo(
            monitor,
            ref monitorInfo
        ))
        {
            return false;
        }

        var monitorRect =
            monitorInfo.rcMonitor;

        const int tolerance =
            4;

        return
            Math.Abs(
                windowRect.Left -
                monitorRect.Left
            ) <= tolerance &&

            Math.Abs(
                windowRect.Top -
                monitorRect.Top
            ) <= tolerance &&

            Math.Abs(
                windowRect.Right -
                monitorRect.Right
            ) <= tolerance &&

            Math.Abs(
                windowRect.Bottom -
                monitorRect.Bottom
            ) <= tolerance;
    }

    // =========================================================
    // Win32 structs
    // =========================================================

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;

        public RECT rcMonitor;

        public RECT rcWork;

        public uint dwFlags;
    }
}
