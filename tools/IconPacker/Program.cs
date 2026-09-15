using System.Text.Json;
using SkiaSharp;
using Svg.Skia;

internal static class Program
{
    // =========================================================
    // Generated sizes
    // =========================================================

    private static readonly int[]
        GeneratedSizes =
        [
            16,
            20,
            24,
            30,
            36,
            40,
            48,
            56,
            64,
            72,
            80,
            96,
            112,
            128
        ];

    // =========================================================
    // Entry point
    // =========================================================

    private static int Main(
        string[] args)
    {
        try
        {
            var repositoryRoot =
                ResolveRepositoryRoot(
                    args
                );

            var iconsDirectory =
                Path.Combine(
                    repositoryRoot,
                    "Assets",
                    "Icons"
                );

            var outputDirectory =
                Path.Combine(
                    repositoryRoot,
                    "Assets",
                    "Generated"
                );

            var outputPath =
                Path.Combine(
                    outputDirectory,
                    "embedded-icons.json"
                );

            if (!Directory.Exists(
                iconsDirectory
            ))
            {
                throw new DirectoryNotFoundException(
                    $"SVG icon directory not found: {iconsDirectory}"
                );
            }

            var svgFiles =
                Directory
                    .EnumerateFiles(
                        iconsDirectory,
                        "*.svg",
                        SearchOption.TopDirectoryOnly
                    )
                    .OrderBy(
                        path =>
                            path,
                        StringComparer.OrdinalIgnoreCase
                    )
                    .ToArray();

            if (
                svgFiles.Length ==
                0
            )
            {
                throw new InvalidOperationException(
                    $"No SVG icons were found in {iconsDirectory}."
                );
            }

            var icons =
                new SortedDictionary<
                    string,
                    SortedDictionary<int, string>>(
                        StringComparer.OrdinalIgnoreCase
                    );

            foreach (
                var svgPath
                in svgFiles
            )
            {
                var iconName =
                    Path.GetFileNameWithoutExtension(
                        svgPath
                    );

                var variants =
                    new SortedDictionary<
                        int,
                        string>();

                foreach (
                    var size
                    in GeneratedSizes
                )
                {
                    variants[size] =
                        Convert.ToBase64String(
                            RenderPng(
                                svgPath,
                                size
                            )
                        );
                }

                icons[iconName] =
                    variants;
            }

            var json =
                JsonSerializer.Serialize(
                    icons,
                    new JsonSerializerOptions
                    {
                        WriteIndented =
                            false
                    }
                ) +
                Environment.NewLine;

            Directory.CreateDirectory(
                outputDirectory
            );

            WriteIfChanged(
                outputPath,
                json
            );

            Console.WriteLine(
                $"Generated {icons.Count} embedded icons at {GeneratedSizes.Length} sizes each."
            );

            Console.WriteLine(
                outputPath
            );

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                ex.Message
            );

            return 1;
        }
    }

    // =========================================================
    // Repository
    // =========================================================

    private static string ResolveRepositoryRoot(
        string[] args)
    {
        var path =
            args.Length > 0 &&
            !string.IsNullOrWhiteSpace(
                args[0]
            )
                ? args[0]
                : Directory.GetCurrentDirectory();

        return Path.GetFullPath(
            path
        );
    }

    // =========================================================
    // SVG -> PNG
    // =========================================================

    private static byte[] RenderPng(
        string svgPath,
        int size)
    {
        using var svg =
            new SKSvg();

        var picture =
            svg.Load(
                svgPath
            );

        if (picture is null)
        {
            throw new InvalidOperationException(
                $"Could not load SVG: {svgPath}"
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
                $"SVG has invalid bounds: {svgPath}"
            );
        }

        var scale =
            Math.Min(
                size /
                bounds.Width,
                size /
                bounds.Height
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
            2F;

        var offsetY =
            (
                size -
                renderedHeight
            ) /
            2F;

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
            ) ??
            throw new InvalidOperationException(
                $"Could not create render surface for {svgPath}."
            );

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

        using var image =
            surface.Snapshot();

        using var data =
            image.Encode(
                SKEncodedImageFormat.Png,
                100
            ) ??
            throw new InvalidOperationException(
                $"Could not encode PNG for {svgPath}."
            );

        return data.ToArray();
    }

    // =========================================================
    // Output
    // =========================================================

    private static void WriteIfChanged(
        string path,
        string content)
    {
        if (
            File.Exists(
                path
            ) &&
            string.Equals(
                File.ReadAllText(
                    path
                ),
                content,
                StringComparison.Ordinal
            )
        )
        {
            return;
        }

        File.WriteAllText(
            path,
            content
        );
    }
}
