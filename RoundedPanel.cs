using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace DiscordPresence;

[ToolboxItem(true)]
public class RoundedPanel : Panel
{
    // =========================================================
    // Fields
    // =========================================================

    private int
        _cornerRadius = 10;

    private int
        _borderSize = 1;

    private Color
        _borderColor =
            Color.FromArgb(
                210,
                216,
                226
            );

    // =========================================================
    // Properties
    // =========================================================

    [Category("Appearance")]
    [DefaultValue(10)]
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
    [DefaultValue(
        typeof(Color),
        "210, 216, 226"
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

    public RoundedPanel()
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
            Color.White;
    }

    // =========================================================
    // Background
    // =========================================================

    protected override void OnPaintBackground(
        PaintEventArgs e)
    {
        var graphics =
            e.Graphics;

        graphics.SmoothingMode =
            SmoothingMode.AntiAlias;

        graphics.PixelOffsetMode =
            PixelOffsetMode.HighQuality;

        // -----------------------------------------------------
        // Paint parent background around rounded corners
        // -----------------------------------------------------

        using (
            var outerBrush =
                new SolidBrush(
                    Parent?.BackColor ??
                    SystemColors.Control
                )
        )
        {
            graphics.FillRectangle(
                outerBrush,
                ClientRectangle
            );
        }

        // -----------------------------------------------------
        // Rounded panel background
        // -----------------------------------------------------

        var borderSize =
            Math.Max(
                0,
                BorderSize
            );

        /*
         * Keep the shape inside the control.
         *
         * For a 1px border:
         * path starts at 0.5px
         * visible border reaches exactly to pixel 0.
         *
         * This is what gives the corner a clean,
         * button-like anti-aliased edge.
         */
        var inset =
            borderSize > 0
                ? borderSize / 2F
                : 0F;

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
            CreateRoundedPath(
                bounds,
                CornerRadius
            );

        using var backgroundBrush =
            new SolidBrush(
                BackColor
            );

        graphics.FillPath(
            backgroundBrush,
            path
        );
    }

    // =========================================================
    // Border
    // =========================================================

    protected override void OnPaint(
        PaintEventArgs e)
    {
        base.OnPaint(
            e
        );

        if (
            BorderSize <= 0 ||
            ClientSize.Width <= 0 ||
            ClientSize.Height <= 0
        )
        {
            return;
        }

        var graphics =
            e.Graphics;

        graphics.SmoothingMode =
            SmoothingMode.AntiAlias;

        graphics.PixelOffsetMode =
            PixelOffsetMode.HighQuality;

        var borderSize =
            BorderSize;

        var inset =
            borderSize /
            2F;

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
            CreateRoundedPath(
                bounds,
                CornerRadius
            );

        using var pen =
            new Pen(
                BorderColor,
                borderSize
            )
            {
                Alignment =
                    PenAlignment.Center,

                LineJoin =
                    LineJoin.Round,

                StartCap =
                    LineCap.Round,

                EndCap =
                    LineCap.Round
            };

        graphics.DrawPath(
            pen,
            path
        );
    }

    // =========================================================
    // Geometry
    // =========================================================

    private static GraphicsPath
        CreateRoundedPath(
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
                    bounds.Width,
                    bounds.Height
                ) /
                2F
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
            radius *
            2F;

        // Top-left
        path.AddArc(
            bounds.Left,
            bounds.Top,
            diameter,
            diameter,
            180F,
            90F
        );

        // Top-right
        path.AddArc(
            bounds.Right -
            diameter,
            bounds.Top,
            diameter,
            diameter,
            270F,
            90F
        );

        // Bottom-right
        path.AddArc(
            bounds.Right -
            diameter,
            bounds.Bottom -
            diameter,
            diameter,
            diameter,
            0F,
            90F
        );

        // Bottom-left
        path.AddArc(
            bounds.Left,
            bounds.Bottom -
            diameter,
            diameter,
            diameter,
            90F,
            90F
        );

        path.CloseFigure();

        return path;
    }
}