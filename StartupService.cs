using Microsoft.Win32;

namespace DiscordPresence;

public static class StartupService
{
    private const string StartupKeyPath =
        @"Software\Microsoft\Windows\CurrentVersion\Run";

    private const string StartupValueName =
        "DiscordPresence";

    public static bool IsEnabled()
    {
        try
        {
            using var key =
                Registry.CurrentUser.OpenSubKey(
                    StartupKeyPath
                );

            var value =
                key?.GetValue(
                    StartupValueName
                ) as string;

            return !string.IsNullOrWhiteSpace(value);
        }
        catch
        {
            return false;
        }
    }

    public static void SetEnabled(bool enabled)
    {
        using var key =
            Registry.CurrentUser.CreateSubKey(
                StartupKeyPath
            );

        if (enabled)
        {
            var executablePath =
                Application.ExecutablePath;

            key.SetValue(
                StartupValueName,
                $"\"{executablePath}\"",
                RegistryValueKind.String
            );
        }
        else
        {
            key.DeleteValue(
                StartupValueName,
                throwOnMissingValue: false
            );
        }
    }
}