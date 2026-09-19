using System.IO;

namespace DiscordPresence;

public static class ProjectNameDetector
{
    public static string? Detect(
        ForegroundAppInfo app)
    {
        if (string.IsNullOrWhiteSpace(
            app.WindowTitle))
        {
            return null;
        }

        return app.ProcessName
            .ToLowerInvariant() switch
        {
            // VS Code family
            "code" =>
                ParseVsCodeFamily(
                    app.WindowTitle
                ),

            "vscodium" =>
                ParseVsCodeFamily(
                    app.WindowTitle
                ),

            "cursor" =>
                ParseVsCodeFamily(
                    app.WindowTitle
                ),

            // JetBrains family
            "idea64" =>
                ParseJetBrains(
                    app.WindowTitle
                ),

            "clion64" =>
                ParseJetBrains(
                    app.WindowTitle
                ),

            "rider64" =>
                ParseJetBrains(
                    app.WindowTitle
                ),

            "pycharm64" =>
                ParseJetBrains(
                    app.WindowTitle
                ),

            "webstorm64" =>
                ParseJetBrains(
                    app.WindowTitle
                ),

            "datagrip64" =>
                ParseJetBrains(
                    app.WindowTitle
                ),

            // Android Studio
            "studio64" =>
                ParseJetBrains(
                    app.WindowTitle
                ),

            // Visual Studio
            "devenv" =>
                ParseVisualStudio(
                    app.WindowTitle
                ),

            // Game engines / creative
            "unrealeditor" =>
                ParseUnreal(
                    app.WindowTitle
                ),

            "blender" =>
                ParseBlender(
                    app.WindowTitle
                ),

            _ => null
        };
    }

    // =========================================================
    // VS Code family
    // =========================================================

    private static string?
        ParseVsCodeFamily(
            string title)
    {
        title =
            RemoveKnownSuffixes(
                title,
                [
                    " - Visual Studio Code",
                    " — Visual Studio Code",
                    " – Visual Studio Code",

                    " - VSCodium",
                    " — VSCodium",
                    " – VSCodium",

                    " - Cursor",
                    " — Cursor",
                    " – Cursor"
                ]
            );

        var parts =
            SplitTitle(title);

        if (parts.Length == 0)
        {
            return null;
        }

        /*
         * Ví dụ:
         *
         * MainForm.cs - DiscordPresence
         *
         * → DiscordPresence
         *
         * hoặc:
         *
         * DiscordPresence
         *
         * → DiscordPresence
         */

        for (var i = parts.Length - 1;
             i >= 0;
             i--)
        {
            var value =
                Clean(parts[i]);

            if (value is null)
            {
                continue;
            }

            if (!LooksLikeFile(value))
            {
                return value;
            }
        }

        return Clean(
            parts[^1]
        );
    }

    // =========================================================
    // JetBrains family
    // =========================================================

    private static string?
        ParseJetBrains(
            string title)
    {
        title =
            RemoveKnownSuffixes(
                title,
                [
                    " - IntelliJ IDEA",
                    " — IntelliJ IDEA",
                    " – IntelliJ IDEA",

                    " - CLion",
                    " — CLion",
                    " – CLion",

                    " - JetBrains Rider",
                    " — JetBrains Rider",
                    " – JetBrains Rider",

                    " - Rider",
                    " — Rider",
                    " – Rider",

                    " - PyCharm",
                    " — PyCharm",
                    " – PyCharm",

                    " - WebStorm",
                    " — WebStorm",
                    " – WebStorm",

                    " - DataGrip",
                    " — DataGrip",
                    " – DataGrip",

                    " - Android Studio",
                    " — Android Studio",
                    " – Android Studio"
                ]
            );

        var parts =
            SplitTitle(title);

        if (parts.Length == 0)
        {
            return null;
        }

        /*
         * JetBrains thường có dạng:
         *
         * BasicRotor - RotorBlockEntity.java
         *
         * hoặc:
         *
         * RotorBlockEntity.java - BasicRotor
         *
         * nên lấy segment đầu tiên không giống filename.
         */

        foreach (var part in parts)
        {
            var value =
                Clean(part);

            if (value is null)
            {
                continue;
            }

            if (!LooksLikeFile(value))
            {
                return value;
            }
        }

        return Clean(
            parts[0]
        );
    }

    // =========================================================
    // Visual Studio
    // =========================================================

    private static string?
        ParseVisualStudio(
            string title)
    {
        title =
            RemoveKnownSuffixes(
                title,
                [
                    " - Microsoft Visual Studio",
                    " — Microsoft Visual Studio",
                    " – Microsoft Visual Studio",

                    " - Microsoft Visual Studio Preview",
                    " — Microsoft Visual Studio Preview",
                    " – Microsoft Visual Studio Preview"
                ]
            );

        var parts =
            SplitTitle(title);

        if (parts.Length == 0)
        {
            return null;
        }

        /*
         * Ví dụ:
         *
         * MainForm.cs - DiscordPresence
         *
         * → DiscordPresence
         *
         * hoặc:
         *
         * DiscordPresence
         *
         * → DiscordPresence
         *
         * Scan từ phải sang trái vì solution/project thường
         * nằm gần cuối title hơn filename hiện tại.
         */

        for (var i = parts.Length - 1;
             i >= 0;
             i--)
        {
            var value =
                Clean(parts[i]);

            if (value is null)
            {
                continue;
            }

            if (!LooksLikeFile(value))
            {
                return value;
            }
        }

        return Clean(
            parts[^1]
        );
    }

    // =========================================================
    // Unreal Engine
    // =========================================================

    private static string?
        ParseUnreal(
            string title)
    {
        title =
            RemoveKnownSuffixes(
                title,
                [
                    " - Unreal Editor",
                    " — Unreal Editor",
                    " – Unreal Editor"
                ]
            );

        var parts =
            SplitTitle(title);

        if (parts.Length == 0)
        {
            return null;
        }

        /*
         * Anatomy3D - Unreal Editor
         *
         * → Anatomy3D
         */

        return Clean(
            parts[0]
        );
    }

    // =========================================================
    // Blender
    // =========================================================

    private static string?
        ParseBlender(
            string title)
    {
        /*
        * Ví dụ thực tế:
        *
        * Startup
        * [C:\Users\...\Startup.blend]
        * - Blender 4.5.12 LTS
        *
        * → Startup
        */

        var blenderIndex =
            title.LastIndexOf(
                " - Blender",
                StringComparison.OrdinalIgnoreCase
            );

        if (blenderIndex < 0)
        {
            blenderIndex =
                title.LastIndexOf(
                    " — Blender",
                    StringComparison.OrdinalIgnoreCase
                );
        }

        if (blenderIndex < 0)
        {
            blenderIndex =
                title.LastIndexOf(
                    " – Blender",
                    StringComparison.OrdinalIgnoreCase
                );
        }

        if (blenderIndex >= 0)
        {
            title =
                title[..blenderIndex];
        }

        title =
            RemoveTrailingBracketInfo(
                title
            );

        var value =
            Clean(title);

        if (value is null)
        {
            return null;
        }

        if (value.EndsWith(
            ".blend",
            StringComparison.OrdinalIgnoreCase))
        {
            return Clean(
                Path.GetFileNameWithoutExtension(
                    value
                )
            );
        }

        return value;
    }

    // =========================================================
    // Shared helpers
    // =========================================================

    private static string[]
        SplitTitle(
            string title)
    {
        return title.Split(
            [
                " - ",
                " — ",
                " – "
            ],
            StringSplitOptions
                .RemoveEmptyEntries |
            StringSplitOptions
                .TrimEntries
        );
    }

    private static string
        RemoveKnownSuffixes(
            string value,
            IReadOnlyList<string> suffixes)
    {
        foreach (var suffix in
            suffixes)
        {
            value =
                RemoveSuffix(
                    value,
                    suffix
                );
        }

        return value;
    }

    private static string
        RemoveSuffix(
            string value,
            string suffix)
    {
        return value.EndsWith(
            suffix,
            StringComparison.OrdinalIgnoreCase)
            ? value[..^suffix.Length]
            : value;
    }

    private static string
        RemoveTrailingBracketInfo(
            string value)
    {
        value =
            value.Trim();

        if (!value.EndsWith(
            ']'))
        {
            return value;
        }

        var bracketIndex =
            value.LastIndexOf(
                " [",
                StringComparison.Ordinal
            );

        if (bracketIndex < 0)
        {
            return value;
        }

        return value[..bracketIndex]
            .TrimEnd();
    }

    private static string?
        Clean(
            string value)
    {
        value =
            value.Trim();

        // Một số app thêm * khi file/project chưa save.
        value =
            value.TrimStart('*');

        value =
            value.Trim();

        return string.IsNullOrWhiteSpace(
            value)
            ? null
            : value;
    }

    private static bool
        LooksLikeFile(
            string value)
    {
        var extension =
            Path.GetExtension(
                value
            );

        return !string.IsNullOrWhiteSpace(
            extension
        );
    }
}