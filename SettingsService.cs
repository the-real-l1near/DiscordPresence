using System.Text.Json;

namespace DiscordPresence;

public static class SettingsService
{
    private static readonly string SettingsDirectory =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData
            ),
            "DiscordPresence"
        );

    private static readonly string SettingsFile =
        Path.Combine(
            SettingsDirectory,
            "settings.json"
        );

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsFile))
            {
                return new AppSettings();
            }

            var json =
                File.ReadAllText(SettingsFile);

            return JsonSerializer.Deserialize<AppSettings>(
                json
            ) ?? new AppSettings();
        }
        catch
        {
            // Nếu config lỗi/corrupt thì dùng default.
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(
                SettingsDirectory
            );

            var json =
                JsonSerializer.Serialize(
                    settings,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                );

            File.WriteAllText(
                SettingsFile,
                json
            );
        }
        catch
        {
            // Settings không phải critical.
            // App vẫn tiếp tục chạy nếu save fail.
        }
    }
}