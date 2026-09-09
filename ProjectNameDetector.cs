using System.IO;

namespace DiscordPresence;

public static class ProjectNameDetector
{
    public static string? Detect(ForegroundAppInfo app)
    {
        if (string.IsNullOrWhiteSpace(app.WindowTitle))
            return null;

        return app.ProcessName.ToLowerInvariant() switch
        {
            "code" => ParseVsCode(app.WindowTitle),
            "idea64" => ParseIntelliJ(app.WindowTitle),
            "unrealeditor" => ParseUnreal(app.WindowTitle),
            "blender" => ParseBlender(app.WindowTitle),

            _ => null
        };
    }

    private static string? ParseVsCode(string title)
    {
        title = RemoveSuffix(title, " - Visual Studio Code");
        title = RemoveSuffix(title, " — Visual Studio Code");

        var parts = SplitTitle(title);

        if (parts.Length == 0)
            return null;

        /*
         * Ví dụ:
         *
         * MainForm.cs - DiscordPresence
         *              ↑ project
         *
         * hoặc:
         *
         * DiscordPresence
         */

        return Clean(parts[^1]);
    }

    private static string? ParseIntelliJ(string title)
    {
        var parts = SplitTitle(title);

        if (parts.Length == 0)
            return null;

        /*
         * JetBrains thường kiểu:
         *
         * BasicRotor - RotorBlockEntity.java
         *
         * nên ưu tiên segment không giống filename.
         */

        foreach (var part in parts)
        {
            var value = Clean(part);

            if (value is null)
                continue;

            if (!LooksLikeFile(value))
                return value;
        }

        return Clean(parts[0]);
    }

    private static string? ParseUnreal(string title)
    {
        title = RemoveSuffix(title, " - Unreal Editor");
        title = RemoveSuffix(title, " — Unreal Editor");

        var parts = SplitTitle(title);

        if (parts.Length == 0)
            return null;

        /*
         * Ví dụ:
         *
         * Anatomy3D - Unreal Editor
         *
         * → Anatomy3D
         */

        return Clean(parts[0]);
    }

    private static string? ParseBlender(string title)
    {
        title = RemoveSuffix(title, " - Blender");
        title = RemoveSuffix(title, " — Blender");

        title = Clean(title) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title))
            return null;

        /*
         * anatomy_final.blend
         * →
         * anatomy_final
         */

        if (title.EndsWith(
            ".blend",
            StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetFileNameWithoutExtension(title);
        }

        return title;
    }

    private static string[] SplitTitle(string title)
    {
        return title.Split(
            [" - ", " — ", " – "],
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries
        );
    }

    private static string RemoveSuffix(
        string value,
        string suffix)
    {
        return value.EndsWith(
            suffix,
            StringComparison.OrdinalIgnoreCase)
            ? value[..^suffix.Length]
            : value;
    }

    private static string? Clean(string value)
    {
        value = value.Trim();

        // Một số app thêm * khi file/project chưa save.
        value = value.TrimStart('*');

        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static bool LooksLikeFile(string value)
    {
        var extension = Path.GetExtension(value);

        return !string.IsNullOrWhiteSpace(extension);
    }
}