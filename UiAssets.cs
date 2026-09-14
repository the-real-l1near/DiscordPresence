using SkiaSharp;
using Svg.Skia;

namespace DiscordPresence;

internal static class UiAssets
{
    // =========================================================
    // Paths
    // =========================================================

    private static readonly string
        IconsDirectory =
            Path.Combine(
                AppContext.BaseDirectory,
                "Assets",
                "Icons"
            );

    // =========================================================
    // Section icons
    // =========================================================

    public static Image Folder(
        int size)
    {
        return Get(
            "folder.svg",
            size
        );
    }

    public static Image App(
        int size)
    {
        return Get(
            "app.svg",
            size
        );
    }

    public static Image Window(
        int size)
    {
        return Get(
            "window.svg",
            size
        );
    }

    public static Image Gamepad(
        int size)
    {
        return Get(
            "gamepad.svg",
            size
        );
    }

    public static Image Ban(
        int size)
    {
        return Get(
            "ban.svg",
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
            "settings.svg",
            size
        );
    }

    public static Image Refresh(
        int size)
    {
        return Get(
            "refresh.svg",
            size
        );
    }

    public static Image RefreshHover(
        int size)
    {
        return Get(
            "refresh-hover.svg",
            size
        );
    }

    public static Image Trash(
        int size)
    {
        return Get(
            "trash.svg",
            size
        );
    }

    public static Image TrashHover(
        int size)
    {
        return Get(
            "trash-hover.svg",
            size
        );
    }

    public static Image TrashOutline(
        int size)
    {
        return Get(
            "trash-outline.svg",
            size
        );
    }

    public static Image TrashOutlineHover(
        int size)
    {
        return Get(
            "trash-outline-hover.svg",
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
            "info.svg",
            size
        );
    }

    public static Image Check(
        int size)
    {
        return Get(
            "check.svg",
            size
        );
    }

    public static Image Warning(
        int size)
    {
        return Get(
            "warning.svg",
            size
        );
    }

    // =========================================================
    // Cache lookup
    // =========================================================

    private static Image Get(
        string fileName,
        int size)
    {
        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(size)
            );
        }

        return AppCacheService.Shared.GetImage(
            "svg:" +
            fileName +
            ":" +
            size,

            () =>
                RenderSvg(
                    fileName,
                    size
                )
        );
    }

    // =========================================================
    // SVG rendering
    // =========================================================

    private static Image RenderSvg(
        string fileName,
        int size)
    {
        var path =
            Path.Combine(
                IconsDirectory,
                fileName
            );

        if (!File.Exists(
            path
        ))
        {
            throw new FileNotFoundException(
                $"SVG asset not found: {path}",
                path
            );
        }

        using var svg =
            new SKSvg();

        var picture =
            svg.Load(
                path
            );

        if (picture is null)
        {
            throw new InvalidOperationException(
                $"Could not load SVG asset: {path}"
            );
        }

        var bounds =
            picture.CullRect;

        if (
            bounds.Width <= 0 ||
            bounds.Height <= 0
        )
        {
            throw new InvalidOperationException(
                $"SVG asset has invalid bounds: {path}"
            );
        }

        // -----------------------------------------------------
        // Preserve aspect ratio
        // -----------------------------------------------------

        var scaleX =
            size /
            bounds.Width;

        var scaleY =
            size /
            bounds.Height;

        var scale =
            Math.Min(
                scaleX,
                scaleY
            );

        var renderedWidth =
            bounds.Width *
            scale;

        var renderedHeight =
            bounds.Height *
            scale;

        var offsetX =
            (
                size -
                renderedWidth
            ) /
            2f;

        var offsetY =
            (
                size -
                renderedHeight
            ) /
            2f;

        // -----------------------------------------------------
        // Render surface
        // -----------------------------------------------------

        var imageInfo =
            new SKImageInfo(
                size,
                size,
                SKColorType.Bgra8888,
                SKAlphaType.Premul
            );

        using var surface =
            SKSurface.Create(
                imageInfo
            );

        if (surface is null)
        {
            throw new InvalidOperationException(
                $"Could not create SVG render surface: {path}"
            );
        }

        var canvas =
            surface.Canvas;

        canvas.Clear(
            SKColors.Transparent
        );

        canvas.Save();

        canvas.Translate(
            offsetX,
            offsetY
        );

        canvas.Scale(
            scale,
            scale
        );

        canvas.Translate(
            -bounds.Left,
            -bounds.Top
        );

        canvas.DrawPicture(
            picture
        );

        canvas.Restore();

        canvas.Flush();

        // -----------------------------------------------------
        // SKImage -> System.Drawing.Bitmap
        // -----------------------------------------------------

        using var skImage =
            surface.Snapshot();

        using var data =
            skImage.Encode(
                SKEncodedImageFormat.Png,
                100
            );

        if (data is null)
        {
            throw new InvalidOperationException(
                $"Could not encode SVG asset: {path}"
            );
        }

        using var stream =
            new MemoryStream(
                data.ToArray()
            );

        using var source =
            Image.FromStream(
                stream
            );

        return new Bitmap(
            source
        );
    }

    // =========================================================
    // Cleanup
    // =========================================================

    public static void Dispose()
    {
        AppCacheService.Shared.Dispose();
    }
}