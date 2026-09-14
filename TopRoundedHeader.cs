using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace DiscordPresence;

[ToolboxItem(true)]
public sealed class TopRoundedHeader : Control
{
    // =========================================================
    // Fields
    // =========================================================

    private int
        _cornerRadius = 8;

    private int
        _borderSize = 1;

    private Color
        _borderColor =
            Color.FromArgb(
                88,
                101,
                242
            );

    // =========================================================
    // Properties
    // =========================================================

    [Category("Appearance")]
    [DefaultValue(8)]
    [Description(
        "Radius of the two upper corners."
    )]
    public int CornerRadius
    {
        get =>
            _cornerRadius;

        set
        {
            _cornerRadius =
                Math.Max(
                    0,
                    value
                );

            Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(1)]
    [Description(
        "Width of the outline."
    )]
    public int BorderSize
    {
        get =>
            _borderSize;

        set
        {
            _borderSize =
                Math.Max(
                    0,
                    value
                );

            Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(typeof(Color), "88, 101, 242")]
    [Description(
        "Color of the outline."
    )]
    public Color BorderColor
    {
        get =>
            _borderColor;

        set
        {
            _borderColor =
                value;

            Invalidate();
        }
    }

    // =========================================================
    // Constructor
    // =========================================================

    public TopRoundedHeader()
    {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor,
            true
        );

        BackColor =
            Color.FromArgb(
                244,
                243,
                255
            );

        ForeColor =
            Color.FromArgb(
                88,
                101,
                242
            );

        Font =
            new Font(
                "Segoe UI",
                9F,
                FontStyle.Bold
            );

        Padding =
            new Padding(
                10,
                0,
                10,
                0
            );
    }

    // =========================================================
    // Painting
    // =========================================================

    protected override void OnPaint(
        PaintEventArgs e)
    {
        var graphics =
            e.Graphics;

        graphics.SmoothingMode =
            SmoothingMode.AntiAlias;

        graphics.PixelOffsetMode =
            PixelOffsetMode.HighQuality;

        // -----------------------------------------------------
        // Clear background outside rounded corners
        // -----------------------------------------------------

        graphics.Clear(
            Parent?.BackColor ??
            SystemColors.Control
        );

        // -----------------------------------------------------
        // Geometry
        // -----------------------------------------------------

        var borderSize =
            Math.Max(
                0,
                BorderSize
            );

        /*
         * A 1px GDI+ pen is centered on its path.
         *
         * With a 246px-wide control:
         *
         * left  path = 0.5
         * right path = 245.5
         *
         * This keeps the visible outer border aligned exactly
         * with a 246px-wide body panel below it.
         */
        var inset =
            borderSize / 2F;

        var bounds =
            new RectangleF(
                inset,
                inset,
                Math.Max(
                    1F,
                    ClientSize.Width -
                    borderSize
                ),
                Math.Max(
                    1F,
                    ClientSize.Height -
                    borderSize
                )
            );

        using var path =
            CreateTopRoundedPath(
                bounds,
                CornerRadius
            );

        // -----------------------------------------------------
        // Background
        // -----------------------------------------------------

        using (
            var backgroundBrush =
                new SolidBrush(
                    BackColor
                )
        )
        {
            graphics.FillPath(
                backgroundBrush,
                path
            );
        }

        // -----------------------------------------------------
        // Border
        // -----------------------------------------------------

        if (borderSize > 0)
        {
            using var borderPen =
                new Pen(
                    BorderColor,
                    borderSize
                )
                {
                    Alignment =
                        PenAlignment.Center
                };

            graphics.DrawPath(
                borderPen,
                path
            );
        }

        // -----------------------------------------------------
        // Text
        // -----------------------------------------------------

        var textBounds =
            new Rectangle(
                Padding.Left,
                0,
                Math.Max(
                    0,
                    ClientSize.Width -
                    Padding.Horizontal
                ),
                ClientSize.Height
            );

        TextRenderer.DrawText(
            graphics,
            Text,
            Font,
            textBounds,
            ForeColor,
            TextFormatFlags.Left |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.SingleLine |
            TextFormatFlags.EndEllipsis |
            TextFormatFlags.NoPrefix
        );
    }

    // =========================================================
    // Geometry
    // =========================================================

    private static GraphicsPath
        CreateTopRoundedPath(
            RectangleF bounds,
            int radius)
    {
        var path =
            new GraphicsPath();

        if (
            bounds.Width <= 0 ||
            bounds.Height <= 0
        )
        {
            return path;
        }

        var maximumRadius =
            (int)Math.Floor(
                Math.Min(
                    bounds.Width / 2F,
                    bounds.Height
                )
            );

        radius =
            Math.Clamp(
                radius,
                0,
                maximumRadius
            );

        if (radius <= 0)
        {
            path.AddRectangle(
                bounds
            );

            path.CloseFigure();

            return path;
        }

        var diameter =
            radius * 2F;

        // -----------------------------------------------------
        // Top-left
        // -----------------------------------------------------

        path.StartFigure();

        path.AddArc(
            bounds.Left,
            bounds.Top,
            diameter,
            diameter,
            180F,
            90F
        );

        // -----------------------------------------------------
        // Top-right
        // -----------------------------------------------------

        path.AddArc(
            bounds.Right -
            diameter,
            bounds.Top,
            diameter,
            diameter,
            270F,
            90F
        );

        // -----------------------------------------------------
        // Right side
        // -----------------------------------------------------

        path.AddLine(
            bounds.Right,
            bounds.Top +
            radius,
            bounds.Right,
            bounds.Bottom
        );

        // -----------------------------------------------------
        // Bottom
        // -----------------------------------------------------

        path.AddLine(
            bounds.Right,
            bounds.Bottom,
            bounds.Left,
            bounds.Bottom
        );

        // -----------------------------------------------------
        // Left side
        // -----------------------------------------------------

        path.AddLine(
            bounds.Left,
            bounds.Bottom,
            bounds.Left,
            bounds.Top +
            radius
        );

        path.CloseFigure();

        return path;
    }
}