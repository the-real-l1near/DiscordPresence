using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DiscordPresence;

public static class ActiveAppDetector
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(
        IntPtr hWnd,
        out uint processId
    );

    public static ForegroundAppInfo? GetForegroundApp()
    {
        var windowHandle = GetForegroundWindow();

        if (windowHandle == IntPtr.Zero)
            return null;

        GetWindowThreadProcessId(
            windowHandle,
            out var processId
        );

        if (processId == 0)
            return null;

        try
        {
            using var process =
                Process.GetProcessById((int)processId);

            return new ForegroundAppInfo(
                process.ProcessName,
                process.MainWindowTitle
            );
        }
        catch
        {
            return null;
        }
    }
}

public sealed record ForegroundAppInfo(
    string ProcessName,
    string WindowTitle
);