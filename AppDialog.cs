namespace DiscordPresence;

public enum AppDialogTone
{
    Info,
    Warning,
    Danger
}

public enum AppDialogButtonKind
{
    Primary,
    Secondary,
    Danger
}

public sealed record AppDialogAction(
    string Text,
    DialogResult Result,
    AppDialogButtonKind Kind
);

public class AppDialog : Form
{
    // =========================================================
    // Layout
    // =========================================================

    private const int
        DialogWidth = 430;

    private const int
        DialogHeight = 166;

    private const int
        HorizontalPadding = 18;

    private const int
        ButtonGap = 8;

    // =========================================================
    // Constructor
    // =========================================================

    public AppDialog(
        string windowTitle,
        string title,
        string message,
        AppDialogTone tone,
        params AppDialogAction[] actions)
    {
        if (
            actions is null ||
            actions.Length is < 1 or > 3
        )
        {
            throw new ArgumentException(
                "AppDialog requires between one and three actions.",
                nameof(actions)
            );
        }

        SuspendLayout();

        try
        {
            // =================================================
            // Window
            // =================================================

            Text =
                windowTitle;

            ClientSize =
                new Size(
                    DialogWidth,
                    DialogHeight
                );

            StartPosition =
                FormStartPosition.CenterParent;

            FormBorderStyle =
                FormBorderStyle.FixedDialog;

            MaximizeBox =
                false;

            MinimizeBox =
                false;

            ShowInTaskbar =
                false;

            ShowIcon =
                false;

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
            // Icon tile
            // =================================================

            var iconTile =
                new RoundedPanel
                {
                    Location =
                        new Point(
                            18,
                            18
                        ),

                    Size =
                        new Size(
                            42,
                            42
                        ),

                    CornerRadius =
                        9,

                    BorderSize =
                        0,

                    BackColor =
                        GetToneBackground(
                            tone
                        )
                };

            var icon =
                new PictureBox
                {
                    BackColor =
                        Color.Transparent,

                    Location =
                        new Point(
                            7,
                            7
                        ),

                    Size =
                        new Size(
                            28,
                            28
                        ),

                    SizeMode =
                        PictureBoxSizeMode.Zoom,

                    Image =
                        GetToneIcon(
                            tone,
                            28
                        ),

                    TabStop =
                        false
                };

            iconTile.Controls.Add(
                icon
            );

            // =================================================
            // Title
            // =================================================

            var titleLabel =
                new Label
                {
                    AutoEllipsis =
                        true,

                    Location =
                        new Point(
                            74,
                            18
                        ),

                    Size =
                        new Size(
                            338,
                            23
                        ),

                    Font =
                        new Font(
                            "Segoe UI",
                            10.5F,
                            FontStyle.Bold,
                            GraphicsUnit.Point
                        ),

                    ForeColor =
                        UiTheme.TextPrimary,

                    Text =
                        title
                };

            // =================================================
            // Message
            // =================================================

            var messageLabel =
                new Label
                {
                    AutoEllipsis =
                        true,

                    Location =
                        new Point(
                            74,
                            43
                        ),

                    Size =
                        new Size(
                            338,
                            38
                        ),

                    Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Regular,
                            GraphicsUnit.Point
                        ),

                    ForeColor =
                        UiTheme.TextSecondary,

                    Text =
                        message
                };

            // =================================================
            // Footer separator
            // =================================================

            var separator =
                new Panel
                {
                    BackColor =
                        UiTheme.Border,

                    Location =
                        new Point(
                            18,
                            92
                        ),

                    Size =
                        new Size(
                            394,
                            1
                        )
                };

            // =================================================
            // Actions
            // =================================================

            var buttons =
                actions
                    .Select(
                        CreateActionButton
                    )
                    .ToArray();

            var widths =
                actions
                    .Select(
                        action =>
                            Math.Clamp(
                                TextRenderer.MeasureText(
                                    action.Text,
                                    Font
                                ).Width +
                                28,
                                86,
                                142
                            )
                    )
                    .ToArray();

            var totalWidth =
                widths.Sum() +
                ButtonGap *
                (
                    buttons.Length -
                    1
                );

            var buttonX =
                ClientSize.Width -
                HorizontalPadding -
                totalWidth;

            const int buttonY =
                108;

            for (
                var index = 0;
                index < buttons.Length;
                index++
            )
            {
                var button =
                    buttons[index];

                button.Location =
                    new Point(
                        buttonX,
                        buttonY
                    );

                button.Size =
                    new Size(
                        widths[index],
                        34
                    );

                Controls.Add(
                    button
                );

                buttonX +=
                    widths[index] +
                    ButtonGap;
            }

            AcceptButton =
                buttons.FirstOrDefault(
                    button =>
                        actions[
                            Array.IndexOf(
                                buttons,
                                button
                            )
                        ].Kind ==
                        AppDialogButtonKind.Primary
                ) ??
                buttons.FirstOrDefault(
                    button =>
                        button.DialogResult !=
                        DialogResult.Cancel
                );

            CancelButton =
                buttons.FirstOrDefault(
                    button =>
                        button.DialogResult ==
                        DialogResult.Cancel
                );

            // =================================================
            // Controls
            // =================================================

            Controls.Add(
                iconTile
            );

            Controls.Add(
                titleLabel
            );

            Controls.Add(
                messageLabel
            );

            Controls.Add(
                separator
            );
        }
        finally
        {
            ResumeLayout(
                false
            );
        }
    }

    // =========================================================
    // Action buttons
    // =========================================================

    private RoundedButton CreateActionButton(
        AppDialogAction action)
    {
        var button =
            new RoundedButton
            {
                Text =
                    action.Text,

                DialogResult =
                    action.Result,

                CornerRadius =
                    8,

                ContentSpacing =
                    5,

                FlatStyle =
                    FlatStyle.Flat,

                UseVisualStyleBackColor =
                    false,

                Cursor =
                    Cursors.Hand,

                TabStop =
                    true
            };

        switch (action.Kind)
        {
            case AppDialogButtonKind.Primary:
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
                break;

            case AppDialogButtonKind.Danger:
                button.BackColor =
                    UiTheme.Danger;

                button.ForeColor =
                    Color.White;

                button.HoverForeColor =
                    Color.White;

                button.FlatAppearance.BorderColor =
                    UiTheme.Danger;

                button.FlatAppearance.BorderSize =
                    1;

                button.FlatAppearance.MouseOverBackColor =
                    Color.FromArgb(
                        190,
                        40,
                        55
                    );

                button.FlatAppearance.MouseDownBackColor =
                    Color.FromArgb(
                        170,
                        34,
                        48
                    );
                break;

            case AppDialogButtonKind.Secondary:
            default:
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
                break;
        }

        return button;
    }

    // =========================================================
    // Tone
    // =========================================================

    private static Color GetToneBackground(
        AppDialogTone tone)
    {
        return tone switch
        {
            AppDialogTone.Danger =>
                UiTheme.DangerSoft,

            AppDialogTone.Warning =>
                Color.FromArgb(
                    255,
                    247,
                    230
                ),

            _ =>
                UiTheme.InfoSoft
        };
    }

    private static Image GetToneIcon(
        AppDialogTone tone,
        int size)
    {
        return tone switch
        {
            AppDialogTone.Warning =>
                UiAssets.Warning(
                    size
                ),

            AppDialogTone.Danger =>
                UiAssets.Warning(
                    size
                ),

            _ =>
                UiAssets.Info(
                    size
                )
        };
    }
}
