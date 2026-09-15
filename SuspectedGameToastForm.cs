using System.Drawing.Drawing2D;

namespace DiscordPresence;

internal sealed class SuspectedGameToastForm : Form
{
    // =========================================================
    // Constants
    // =========================================================

    private const int
        CornerRadius = 12;

    private const int
        CloseDelayMilliseconds = 8000;

    // =========================================================
    // State
    // =========================================================

    private readonly System.Windows.Forms.Timer
        _closeTimer;

    private readonly Action
        _onYes;

    private readonly Action
        _onNo;

    private readonly Action
        _onLater;

    // =========================================================
    // Constructor
    // =========================================================

    public SuspectedGameToastForm(
        SuspectedGame suspect,
        Action onYes,
        Action onNo,
        Action onLater)
    {
        _onYes =
            onYes;

        _onNo =
            onNo;

        _onLater =
            onLater;

        _closeTimer =
            new System.Windows.Forms.Timer
            {
                Interval =
                    CloseDelayMilliseconds
            };

        SuspendLayout();

        try
        {
            // =================================================
            // Window
            // =================================================

            FormBorderStyle =
                FormBorderStyle.None;

            ShowInTaskbar =
                false;

            TopMost =
                true;

            StartPosition =
                FormStartPosition.Manual;

            ClientSize =
                new Size(
                    410,
                    188
                );

            AutoScaleMode =
                AutoScaleMode.Dpi;

            Font =
                new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Regular,
                    GraphicsUnit.Point
                );

            BackColor =
                UiTheme.Background;

            ForeColor =
                UiTheme.TextPrimary;

            // =================================================
            // Card
            // =================================================

            var card =
                new RoundedPanel
                {
                    Dock =
                        DockStyle.Fill,

                    BackColor =
                        Color.White,

                    BorderColor =
                        UiTheme.Border,

                    BorderSize =
                        1,

                    CornerRadius =
                        CornerRadius
                };

            // =================================================
            // Game icon
            // =================================================

            var iconTile =
                new RoundedPanel
                {
                    BackColor =
                        UiTheme.AccentSoft,

                    BorderSize =
                        0,

                    CornerRadius =
                        9,

                    Location =
                        new Point(
                            16,
                            16
                        ),

                    Size =
                        new Size(
                            44,
                            44
                        )
                };

            var gameIcon =
                new PictureBox
                {
                    BackColor =
                        Color.Transparent,

                    Image =
                        UiAssets.Gamepad(
                            30
                        ),

                    Location =
                        new Point(
                            7,
                            7
                        ),

                    Size =
                        new Size(
                            30,
                            30
                        ),

                    SizeMode =
                        PictureBoxSizeMode.Zoom,

                    TabStop =
                        false
                };

            iconTile.Controls.Add(
                gameIcon
            );

            // =================================================
            // Header
            // =================================================

            var titleLabel =
                new Label
                {
                    AutoEllipsis =
                        true,

                    Font =
                        new Font(
                            "Segoe UI",
                            10F,
                            FontStyle.Bold,
                            GraphicsUnit.Point
                        ),

                    ForeColor =
                        UiTheme.TextPrimary,

                    Location =
                        new Point(
                            72,
                            17
                        ),

                    Size =
                        new Size(
                            318,
                            22
                        ),

                    Text =
                        "Suspected game detected"
                };

            var subtitleLabel =
                new Label
                {
                    AutoEllipsis =
                        true,

                    ForeColor =
                        UiTheme.TextSecondary,

                    Location =
                        new Point(
                            72,
                            40
                        ),

                    Size =
                        new Size(
                            318,
                            20
                        ),

                    Text =
                        "Discord Presence needs your input."
                };

            // =================================================
            // Process
            // =================================================

            var processLabel =
                new Label
                {
                    AutoEllipsis =
                        true,

                    Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold,
                            GraphicsUnit.Point
                        ),

                    ForeColor =
                        UiTheme.TextPrimary,

                    Location =
                        new Point(
                            16,
                            72
                        ),

                    Size =
                        new Size(
                            378,
                            20
                        ),

                    Text =
                        $"Is \"{suspect.ProcessName}.exe\" a game?"
                };

            var windowLabel =
                new Label
                {
                    AutoEllipsis =
                        true,

                    ForeColor =
                        UiTheme.TextSecondary,

                    Location =
                        new Point(
                            16,
                            94
                        ),

                    Size =
                        new Size(
                            378,
                            20
                        ),

                    Text =
                        string.IsNullOrWhiteSpace(
                            suspect.WindowTitle
                        )
                            ? "Window: -"
                            : $"Window: {suspect.WindowTitle}"
                };

            // =================================================
            // Divider
            // =================================================

            var divider =
                new Panel
                {
                    BackColor =
                        Color.FromArgb(
                            226,
                            232,
                            240
                        ),

                    Location =
                        new Point(
                            16,
                            122
                        ),

                    Size =
                        new Size(
                            378,
                            1
                        )
                };

            // =================================================
            // Buttons
            // =================================================

            var yesButton =
                CreateButton(
                    "Yes",
                    primary: true
                );

            yesButton.Location =
                new Point(
                    16,
                    136
                );

            yesButton.Size =
                new Size(
                    118,
                    34
                );

            var noButton =
                CreateButton(
                    "No",
                    primary: false
                );

            noButton.Location =
                new Point(
                    146,
                    136
                );

            noButton.Size =
                new Size(
                    118,
                    34
                );

            var laterButton =
                CreateButton(
                    "Later",
                    primary: false
                );

            laterButton.Location =
                new Point(
                    276,
                    136
                );

            laterButton.Size =
                new Size(
                    118,
                    34
                );

            // =================================================
            // Events
            // =================================================

            yesButton.Click += (_, _) =>
            {
                _closeTimer.Stop();

                _onYes();

                Close();
            };

            noButton.Click += (_, _) =>
            {
                _closeTimer.Stop();

                _onNo();

                Close();
            };

            laterButton.Click += (_, _) =>
            {
                _closeTimer.Stop();

                _onLater();

                Close();
            };

            // =================================================
            // Controls
            // =================================================

            card.Controls.Add(
                iconTile
            );

            card.Controls.Add(
                titleLabel
            );

            card.Controls.Add(
                subtitleLabel
            );

            card.Controls.Add(
                processLabel
            );

            card.Controls.Add(
                windowLabel
            );

            card.Controls.Add(
                divider
            );

            card.Controls.Add(
                yesButton
            );

            card.Controls.Add(
                noButton
            );

            card.Controls.Add(
                laterButton
            );

            Controls.Add(
                card
            );

            // =================================================
            // Auto close
            // =================================================

            _closeTimer.Tick += (_, _) =>
            {
                _closeTimer.Stop();

                Close();
            };

            Shown += (_, _) =>
            {
                PositionToast();

                ApplyRoundedRegion();

                _closeTimer.Start();
            };

            SizeChanged += (_, _) =>
            {
                ApplyRoundedRegion();
            };
        }
        finally
        {
            ResumeLayout(
                false
            );
        }
    }

    // =========================================================
    // Buttons
    // =========================================================

    private static RoundedButton CreateButton(
        string text,
        bool primary)
    {
        var button =
            new RoundedButton
            {
                Text =
                    text,

                CornerRadius =
                    8,

                ContentSpacing =
                    5,

                FlatStyle =
                    FlatStyle.Flat,

                UseVisualStyleBackColor =
                    false,

                Cursor =
                    Cursors.Hand
            };

        if (primary)
        {
            button.BackColor =
                UiTheme.Accent;

            button.ForeColor =
                Color.White;

            button.HoverForeColor =
                Color.White;

            button.FlatAppearance.BorderColor =
                UiTheme.Accent;

            button.FlatAppearance.BorderSize =
                1;

            button.FlatAppearance.MouseOverBackColor =
                UiTheme.AccentHover;

            button.FlatAppearance.MouseDownBackColor =
                UiTheme.AccentHover;

            return button;
        }

        button.BackColor =
            Color.White;

        button.ForeColor =
            UiTheme.TextPrimary;

        button.HoverForeColor =
            UiTheme.TextPrimary;

        button.FlatAppearance.BorderColor =
            UiTheme.Border;

        button.FlatAppearance.BorderSize =
            1;

        button.FlatAppearance.MouseOverBackColor =
            UiTheme.Card;

        button.FlatAppearance.MouseDownBackColor =
            UiTheme.AccentSoft;

        return button;
    }

    // =========================================================
    // Position
    // =========================================================

    private void PositionToast()
    {
        var workingArea =
            Screen.PrimaryScreen?.WorkingArea ??
            Screen.FromControl(
                this
            ).WorkingArea;

        const int margin =
            16;

        Left =
            workingArea.Right -
            Width -
            margin;

        Top =
            workingArea.Bottom -
            Height -
            margin;
    }

    // =========================================================
    // Rounded window
    // =========================================================

    private void ApplyRoundedRegion()
    {
        if (
            ClientSize.Width <= 0 ||
            ClientSize.Height <= 0
        )
        {
            return;
        }

        using var path =
            CreateRoundedRectanglePath(
                new RectangleF(
                    0,
                    0,
                    ClientSize.Width,
                    ClientSize.Height
                ),
                CornerRadius
            );

        var newRegion =
            new Region(
                path
            );

        var oldRegion =
            Region;

        Region =
            newRegion;

        oldRegion?.Dispose();
    }

    private static GraphicsPath
        CreateRoundedRectanglePath(
            RectangleF bounds,
            int radius)
    {
        var path =
            new GraphicsPath();

        var diameter =
            radius *
            2F;

        path.AddArc(
            bounds.Left,
            bounds.Top,
            diameter,
            diameter,
            180F,
            90F
        );

        path.AddArc(
            bounds.Right -
            diameter,
            bounds.Top,
            diameter,
            diameter,
            270F,
            90F
        );

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

    // =========================================================
    // Shadow
    // =========================================================

    protected override CreateParams CreateParams
    {
        get
        {
            const int CsDropShadow =
                0x00020000;

            var createParams =
                base.CreateParams;

            createParams.ClassStyle |=
                CsDropShadow;

            return createParams;
        }
    }

    // =========================================================
    // Do not steal focus
    // =========================================================

    protected override bool ShowWithoutActivation =>
        true;

    // =========================================================
    // Cleanup
    // =========================================================

    protected override void Dispose(
        bool disposing)
    {
        if (disposing)
        {
            _closeTimer.Dispose();
        }

        base.Dispose(
            disposing
        );
    }
}
