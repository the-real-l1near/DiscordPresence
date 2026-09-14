using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace DiscordPresence;

[DefaultEvent(nameof(Click))]
[ToolboxItem(true)]
public class RoundedButton : Button
{
    // =========================================================
    // State
    // =========================================================

    private bool
        _hovered;

    private bool
        _mousePressed;

    private bool
        _spacePressed;

    private int
        _cornerRadius = 8;

    private int
        _contentSpacing = 5;

    // =========================================================
    // Properties
    // =========================================================

    [Category("Appearance")]
    [DefaultValue(8)]
    [Description("Radius of the button corners.")]
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
    [DefaultValue(5)]
    [Description("Spacing between the image and text.")]
    public int ContentSpacing
    {
        get =>
            _contentSpacing;

        set
        {
            _contentSpacing =
                Math.Max(
                    0,
                    value
                );

            Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(typeof(Color), "Empty")]
    [Description("Text color used while the mouse is over the button.")]
    public Color HoverForeColor
    {
        get;
        set;
    } =
        Color.Empty;

    /*
     * Runtime image supplied by UiAssets.
     * Do not serialize it into Designer code.
     */
    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden
    )]
    public Image?
        HoverImage { get; set; }

    // =========================================================
    // Constructor
    // =========================================================

    public RoundedButton()
    {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor,
            true
        );

        FlatStyle =
            FlatStyle.Flat;

        UseVisualStyleBackColor =
            false;

        Cursor =
            Cursors.Hand;
    }

    // =========================================================
    // State events
    // =========================================================

    protected override void OnMouseEnter(
        EventArgs e)
    {
        _hovered =
            true;

        base.OnMouseEnter(
            e
        );

        Invalidate();
    }

    protected override void OnMouseLeave(
        EventArgs e)
    {
        _hovered =
            false;

        _mousePressed =
            false;

        base.OnMouseLeave(
            e
        );

        Invalidate();
    }

    protected override void OnMouseDown(
        MouseEventArgs e)
    {
        if (
            e.Button ==
            MouseButtons.Left
        )
        {
            _mousePressed =
                true;
        }

        base.OnMouseDown(
            e
        );

        Invalidate();
    }

    protected override void OnMouseUp(
        MouseEventArgs e)
    {
        if (
            e.Button ==
            MouseButtons.Left
        )
        {
            _mousePressed =
                false;
        }

        base.OnMouseUp(
            e
        );

        Invalidate();
    }

    protected override void OnMouseCaptureChanged(
        EventArgs e)
    {
        if (!Capture)
        {
            _mousePressed =
                false;
        }

        base.OnMouseCaptureChanged(
            e
        );

        Invalidate();
    }

    protected override void OnKeyDown(
        KeyEventArgs e)
    {
        if (
            e.KeyCode ==
            Keys.Space
        )
        {
            _spacePressed =
                true;
        }

        base.OnKeyDown(
            e
        );

        Invalidate();
    }

    protected override void OnKeyUp(
        KeyEventArgs e)
    {
        if (
            e.KeyCode ==
            Keys.Space
        )
        {
            _spacePressed =
                false;
        }

        base.OnKeyUp(
            e
        );

        Invalidate();
    }

    protected override void OnLostFocus(
        EventArgs e)
    {
        _spacePressed =
            false;

        _mousePressed =
            false;

        base.OnLostFocus(
            e
        );

        Invalidate();
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

        var pressed =
            Enabled &&
            (
                _spacePressed ||
                (
                    _mousePressed &&
                    _hovered
                )
            );

        var hovering =
            Enabled &&
            _hovered;

        var background =
            GetBackgroundColor(
                hovering,
                pressed
            );

        var foreground =
            GetForegroundColor(
                hovering
            );

        var parentBackground =
            Parent?.BackColor ??
            SystemColors.Control;

        graphics.Clear(
            parentBackground
        );

        var borderWidth =
            Math.Max(
                0,
                FlatAppearance.BorderSize
            );

        var inset =
            borderWidth > 0
                ? borderWidth / 2F
                : 0F;

        var bounds =
            new RectangleF(
                inset,
                inset,
                Math.Max(
                    1F,
                    ClientSize.Width -
                    1F -
                    borderWidth
                ),
                Math.Max(
                    1F,
                    ClientSize.Height -
                    1F -
                    borderWidth
                )
            );

        using var path =
            CreateRoundedRectanglePath(
                bounds,
                CornerRadius
            );

        using (
            var backgroundBrush =
                new SolidBrush(
                    background
                )
        )
        {
            graphics.FillPath(
                backgroundBrush,
                path
            );
        }

        DrawBorder(
            graphics,
            path,
            borderWidth
        );

        DrawContent(
            graphics,
            background,
            foreground,
            hovering
        );

        DrawFocus(
            graphics,
            background,
            foreground
        );
    }

    // =========================================================
    // Colors
    // =========================================================

    private Color GetBackgroundColor(
        bool hovering,
        bool pressed)
    {
        if (!Enabled)
        {
            return BackColor;
        }

        if (pressed)
        {
            var configured =
                FlatAppearance
                    .MouseDownBackColor;

            return configured.IsEmpty
                ? ControlPaint.Dark(
                    BackColor,
                    0.12F
                )
                : configured;
        }

        if (hovering)
        {
            var configured =
                FlatAppearance
                    .MouseOverBackColor;

            return configured.IsEmpty
                ? ControlPaint.Dark(
                    BackColor,
                    0.05F
                )
                : configured;
        }

        return BackColor;
    }

    private Color GetForegroundColor(
        bool hovering)
    {
        if (!Enabled)
        {
            return SystemColors.GrayText;
        }

        if (
            hovering &&
            !HoverForeColor.IsEmpty
        )
        {
            return HoverForeColor;
        }

        return ForeColor;
    }

    // =========================================================
    // Border
    // =========================================================

    private void DrawBorder(
        Graphics graphics,
        GraphicsPath path,
        int borderWidth)
    {
        if (borderWidth <= 0)
        {
            return;
        }

        var borderColor =
            FlatAppearance.BorderColor;

        if (borderColor.IsEmpty)
        {
            borderColor =
                Enabled
                    ? ForeColor
                    : SystemColors.GrayText;
        }

        using var pen =
            new Pen(
                borderColor,
                borderWidth
            )
            {
                Alignment =
                    PenAlignment.Center
            };

        graphics.DrawPath(
            pen,
            path
        );
    }

    // =========================================================
    // Content
    // =========================================================

    private void DrawContent(
        Graphics graphics,
        Color background,
        Color foreground,
        bool hovering)
    {
        var displayedImage =
            hovering &&
            HoverImage is not null
                ? HoverImage
                : Image;

        var flags =
            TextFormatFlags.SingleLine |
            TextFormatFlags.NoPadding |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis;

        if (!UseMnemonic)
        {
            flags |=
                TextFormatFlags.NoPrefix;
        }
        else if (!ShowKeyboardCues)
        {
            flags |=
                TextFormatFlags.HidePrefix;
        }

        var imageWidth =
            displayedImage?.Width ??
            0;

        var imageHeight =
            displayedImage?.Height ??
            0;

        var hasText =
            !string.IsNullOrEmpty(
                Text
            );

        var gap =
            displayedImage is not null &&
            hasText
                ? Math.Max(
                    0,
                    (int)Math.Round(
                        ContentSpacing *
                        DeviceDpi /
                        96.0
                    )
                )
                : 0;

        var inset =
            Math.Max(
                FlatAppearance.BorderSize + 4,
                5
            );

        var availableWidth =
            Math.Max(
                0,
                ClientSize.Width -
                inset * 2 -
                imageWidth -
                gap
            );

        var textWidth =
            0;

        if (hasText)
        {
            textWidth =
                Math.Min(
                    availableWidth,
                    TextRenderer.MeasureText(
                        graphics,
                        Text,
                        Font,
                        new Size(
                            int.MaxValue,
                            int.MaxValue
                        ),
                        flags
                    ).Width
                );
        }

        var totalWidth =
            imageWidth +
            gap +
            textWidth;

        var x =
            (
                ClientSize.Width -
                totalWidth
            ) / 2;

        if (displayedImage is not null)
        {
            var y =
                (
                    ClientSize.Height -
                    imageHeight
                ) / 2;

            if (Enabled)
            {
                graphics.DrawImage(
                    displayedImage,
                    new Rectangle(
                        x,
                        y,
                        imageWidth,
                        imageHeight
                    )
                );
            }
            else
            {
                ControlPaint.DrawImageDisabled(
                    graphics,
                    displayedImage,
                    x,
                    y,
                    background
                );
            }

            x +=
                imageWidth +
                gap;
        }

        if (
            hasText &&
            textWidth > 0
        )
        {
            TextRenderer.DrawText(
                graphics,
                Text,
                Font,
                new Rectangle(
                    x,
                    0,
                    textWidth,
                    ClientSize.Height
                ),
                foreground,
                flags
            );
        }
    }

    // =========================================================
    // Focus
    // =========================================================

    private void DrawFocus(
        Graphics graphics,
        Color background,
        Color foreground)
    {
        if (
            !Focused ||
            !ShowFocusCues
        )
        {
            return;
        }

        var focusBounds =
            Rectangle.Inflate(
                ClientRectangle,
                -4,
                -4
            );

        if (
            focusBounds.Width <= 0 ||
            focusBounds.Height <= 0
        )
        {
            return;
        }

        ControlPaint.DrawFocusRectangle(
            graphics,
            focusBounds,
            foreground,
            background
        );
    }

    // =========================================================
    // Geometry
    // =========================================================

    private static GraphicsPath
        CreateRoundedRectanglePath(
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
                ) / 2F
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

        path.AddArc(
            bounds.Left,
            bounds.Top,
            diameter,
            diameter,
            180F,
            90F
        );

        path.AddArc(
            bounds.Right - diameter,
            bounds.Top,
            diameter,
            diameter,
            270F,
            90F
        );

        path.AddArc(
            bounds.Right - diameter,
            bounds.Bottom - diameter,
            diameter,
            diameter,
            0F,
            90F
        );

        path.AddArc(
            bounds.Left,
            bounds.Bottom - diameter,
            diameter,
            diameter,
            90F,
            90F
        );

        path.CloseFigure();

        return path;
    }
}