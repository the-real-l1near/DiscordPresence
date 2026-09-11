using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DiscordPresence;

internal static class GameDetector
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

    // =========================================================
    // Constants
    // =========================================================

    private const uint MonitorDefaultToNearest =
        0x00000002;

    // =========================================================
    // Excluded applications
    // =========================================================

    /*
     * Fullscreen app không đồng nghĩa với game.
     *
     * Loại các app phổ biến có thể fullscreen
     * nhưng không nên override presence.
     */
    private static readonly HashSet<string>
        ExcludedProcesses =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Discord",
                "DiscordCanary",
                "DiscordPTB",

                "explorer",

                "chrome",
                "msedge",
                "firefox",

                "vlc",
                "mpv",

                "Code",
                "idea64",
                "blender",
                "UnrealEditor",

                "DiscordPresence"
            };

    // =========================================================
    // Detection
    // =========================================================

    public static bool IsForegroundGame()
    {
        var window =
            GetForegroundWindow();

        if (window == IntPtr.Zero)
        {
            return false;
        }

        GetWindowThreadProcessId(
            window,
            out var processId
        );

        if (processId == 0)
        {
            return false;
        }

        Process? process =
            null;

        try
        {
            process =
                Process.GetProcessById(
                    (int)processId
                );

            if (ExcludedProcesses.Contains(
                process.ProcessName
            ))
            {
                return false;
            }

            /*
             * Hiện tại dùng fullscreen /
             * borderless fullscreen làm tín hiệu game.
             */
            return IsFullscreenWindow(
                window
            );
        }
        catch
        {
            return false;
        }
        finally
        {
            process?.Dispose();
        }
    }

    // =========================================================
    // Fullscreen detection
    // =========================================================

    private static bool IsFullscreenWindow(
        IntPtr window)
    {
        if (!GetWindowRect(
            window,
            out var windowRect))
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
            ref monitorInfo))
        {
            return false;
        }

        var monitorRect =
            monitorInfo.rcMonitor;

        /*
         * Cho sai số vài pixel vì borderless
         * window đôi khi lệch 1–2 px.
         */
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
    // Native structs
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