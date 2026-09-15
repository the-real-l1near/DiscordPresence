using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text.Json;

namespace DiscordPresence;

internal static class UiAssets
{
    // =========================================================
    // Embedded resources
    // =========================================================

    private const string
        EmbeddedIconsResourceName =
            "DiscordPresence.EmbeddedIcons.json";

    private static readonly Lazy<
        Dictionary<string, SortedDictionary<int, string>>>
        EmbeddedIcons =
            new(
                LoadEmbeddedIcons
            );

    // =========================================================
    // Section icons
    // =========================================================

    public static Image Folder(
        int size)
    {
        return Get(
            "folder",
            size
        );
    }

    public static Image App(
        int size)
    {
        return Get(
            "app",
            size
        );
    }

    public static Image Window(
        int size)
    {
        return Get(
            "window",
            size
        );
    }

    public static Image Gamepad(
        int size)
    {
        return Get(
            "gamepad",
            size
        );
    }

    public static Image Ban(
        int size)
    {
        return Get(
            "ban",
            size
        );
    }

    // =========================================================
    // Action icons
    // =========================================================

    public static Image Settings(
        int size)
    {
        return Get(
            "settings",
            size
        );
    }

    public static Image Refresh(
        int size)
    {
        return Get(
            "refresh",
            size
        );
    }

    public static Image RefreshHover(
        int size)
    {
        return Get(
            "refresh-hover",
            size
        );
    }

    public static Image Trash(
        int size)
    {
        return Get(
            "trash",
            size
        );
    }

    public static Image TrashHover(
        int size)
    {
        return Get(
            "trash-hover",
            size
        );
    }

    public static Image TrashOutline(
        int size)
    {
        return Get(
            "trash-outline",
            size
        );
    }

    public static Image TrashOutlineHover(
        int size)
    {
        return Get(
            "trash-outline-hover",
            size
        );
    }

    // =========================================================
    // Status icons
    // =========================================================

    public static Image Info(
        int size)
    {
        return Get(
            "info",
            size
        );
    }

    public static Image Check(
        int size)
    {
        return Get(
            "check",
            size
        );
    }

    public static Image Warning(
        int size)
    {
        return Get(
            "warning",
            size
        );
    }

    // =========================================================
    // Cache lookup
    // =========================================================

    private static Image Get(
        string iconName,
        int size)
    {
        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(size)
            );
        }

        return AppCacheService.Shared.GetImage(
            "embedded-png:" +
            iconName +
            ":" +
            size,

            () =>
                LoadEmbeddedPng(
                    iconName,
                    size
                )
        );
    }

    // =========================================================
    // PNG loading
    // =========================================================

    private static Image LoadEmbeddedPng(
        string iconName,
        int requestedSize)
    {
        var icons =
            EmbeddedIcons.Value;

        if (
            !icons.TryGetValue(
                iconName,
                out var variants
            ) ||
            variants.Count ==
            0
        )
        {
            throw new InvalidOperationException(
                $"Embedded icon '{iconName}' was not generated. " +
                "Run tools\\GenerateEmbeddedIcons.ps1 and rebuild."
            );
        }

        var sourceSize =
            SelectSourceSize(
                variants.Keys,
                requestedSize
            );

        var base64 =
            variants[sourceSize];

        var bytes =
            Convert.FromBase64String(
                base64
            );

        using var stream =
            new MemoryStream(
                bytes,
                writable: false
            );

        using var source =
            Image.FromStream(
                stream
            );

        if (
            source.Width ==
            requestedSize &&
            source.Height ==
            requestedSize
        )
        {
            return new Bitmap(
                source
            );
        }

        return ResizeImage(
            source,
            requestedSize
        );
    }

    private static int SelectSourceSize(
        IEnumerable<int> sizes,
        int requestedSize)
    {
        var largest =
            0;

        foreach (
            var size
            in sizes
        )
        {
            largest =
                size;

            if (
                size >=
                requestedSize
            )
            {
                return size;
            }
        }

        if (largest > 0)
        {
            return largest;
        }

        throw new InvalidOperationException(
            "Embedded icon has no generated PNG sizes."
        );
    }

    private static Image ResizeImage(
        Image source,
        int size)
    {
        var result =
            new Bitmap(
                size,
                size,
                PixelFormat.Format32bppPArgb
            );

        using var graphics =
            Graphics.FromImage(
                result
            );

        graphics.CompositingMode =
            CompositingMode.SourceCopy;

        graphics.CompositingQuality =
            CompositingQuality.HighQuality;

        graphics.InterpolationMode =
            InterpolationMode.HighQualityBicubic;

        graphics.SmoothingMode =
            SmoothingMode.HighQuality;

        graphics.PixelOffsetMode =
            PixelOffsetMode.HighQuality;

        graphics.DrawImage(
            source,
            new Rectangle(
                0,
                0,
                size,
                size
            ),
            0,
            0,
            source.Width,
            source.Height,
            GraphicsUnit.Pixel
        );

        return result;
    }

    // =========================================================
    // Resource manifest
    // =========================================================

    private static Dictionary<
        string,
        SortedDictionary<int, string>>
        LoadEmbeddedIcons()
    {
        var assembly =
            typeof(UiAssets)
                .Assembly;

        using var stream =
            assembly
                .GetManifestResourceStream(
                    EmbeddedIconsResourceName
                );

        if (stream is null)
        {
            throw new InvalidOperationException(
                $"Embedded icon resource '{EmbeddedIconsResourceName}' was not found. " +
                "Run tools\\GenerateEmbeddedIcons.ps1 and rebuild."
            );
        }

        using var document =
            JsonDocument.Parse(
                stream
            );

        var result =
            new Dictionary<
                string,
                SortedDictionary<int, string>>(
                    StringComparer.OrdinalIgnoreCase
                );

        foreach (
            var iconProperty
            in document.RootElement.EnumerateObject()
        )
        {
            if (
                iconProperty.Value.ValueKind !=
                JsonValueKind.Object
            )
            {
                continue;
            }

            var variants =
                new SortedDictionary<
                    int,
                    string>();

            foreach (
                var sizeProperty
                in iconProperty.Value.EnumerateObject()
            )
            {
                if (
                    !int.TryParse(
                        sizeProperty.Name,
                        out var size
                    ) ||
                    size <= 0 ||
                    sizeProperty.Value.ValueKind !=
                    JsonValueKind.String
                )
                {
                    continue;
                }

                var base64 =
                    sizeProperty.Value.GetString();

                if (
                    string.IsNullOrWhiteSpace(
                        base64
                    )
                )
                {
                    continue;
                }

                variants[size] =
                    base64;
            }

            if (
                variants.Count >
                0
            )
            {
                result[
                    iconProperty.Name
                ] =
                    variants;
            }
        }

        return result;
    }

    // =========================================================
    // Cleanup
    // =========================================================

    public static void Dispose()
    {
        AppCacheService.Shared.Dispose();
    }
}
