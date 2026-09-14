namespace DiscordPresence;

internal static class UiTheme
{
    // =========================================================
    // Colors
    // =========================================================

    public static readonly Color Background =
        Color.FromArgb(247, 249, 252);

    public static readonly Color Card =
        Color.FromArgb(241, 245, 249);

    public static readonly Color CardAccent =
        Color.FromArgb(244, 243, 255);

    public static readonly Color Border =
        Color.FromArgb(210, 216, 226);

    public static readonly Color Accent =
        Color.FromArgb(88, 101, 242);

    public static readonly Color AccentHover =
        Color.FromArgb(71, 82, 196);

    public static readonly Color AccentSoft =
        Color.FromArgb(232, 234, 255);

    public static readonly Color Info =
        Color.FromArgb(24, 119, 242);

    public static readonly Color InfoSoft =
        Color.FromArgb(231, 243, 255);

    public static readonly Color Danger =
        Color.FromArgb(220, 53, 69);

    public static readonly Color DangerSoft =
        Color.FromArgb(255, 238, 241);

    public static readonly Color TextPrimary =
        Color.FromArgb(30, 41, 59);

    public static readonly Color TextSecondary =
        Color.FromArgb(100, 116, 139);

    // =========================================================
    // Form
    // =========================================================

    public static void StyleForm(
        Form form)
    {
        form.BackColor =
            Background;

        form.ForeColor =
            TextPrimary;
    }

    // =========================================================
    // Labels
    // =========================================================

    public static void StyleSectionLabel(
        Label label)
    {
        label.ForeColor =
            TextSecondary;
    }

    public static void StyleValueLabel(
        Label label)
    {
        label.ForeColor =
            TextPrimary;

        label.Font =
            new Font(
                label.Font,
                FontStyle.Bold
            );
    }

    public static void StyleMutedLabel(
        Label label)
    {
        label.ForeColor =
            TextSecondary;
    }

    // =========================================================
    // Buttons
    // =========================================================

    public static void StylePrimaryButton(
        Button button)
    {
        ApplyFlatButtonBase(
            button
        );

        button.BackColor =
            Accent;

        button.ForeColor =
            Color.White;

        button.FlatAppearance.BorderColor =
            Accent;

        button.FlatAppearance.MouseOverBackColor =
            AccentHover;

        button.FlatAppearance.MouseDownBackColor =
            AccentHover;
    }

    public static void StyleSecondaryButton(
        Button button)
    {
        ApplyFlatButtonBase(
            button
        );

        button.BackColor =
            Color.White;

        button.ForeColor =
            TextPrimary;

        button.FlatAppearance.BorderColor =
            Border;

        button.FlatAppearance.MouseOverBackColor =
            Card;
    }

    public static void StyleInfoButton(
        Button button)
    {
        ApplyFlatButtonBase(
            button
        );

        button.BackColor =
            InfoSoft;

        button.ForeColor =
            Color.FromArgb(
                13,
                92,
                182
            );

        button.FlatAppearance.BorderColor =
            Color.FromArgb(
                170,
                215,
                255
            );

        button.FlatAppearance.MouseOverBackColor =
            Color.FromArgb(
                216,
                237,
                255
            );
    }

    public static void StyleDangerButton(
        Button button)
    {
        ApplyFlatButtonBase(
            button
        );

        button.BackColor =
            DangerSoft;

        button.ForeColor =
            Danger;

        button.FlatAppearance.BorderColor =
            Color.FromArgb(
                246,
                190,
                200
            );

        button.FlatAppearance.MouseOverBackColor =
            Color.FromArgb(
                255,
                224,
                230
            );
    }

    public static void StyleNeutralButton(
        Button button)
    {
        ApplyFlatButtonBase(
            button
        );

        button.BackColor =
            Color.White;

        button.ForeColor =
            TextPrimary;

        button.FlatAppearance.BorderColor =
            Border;

        button.FlatAppearance.MouseOverBackColor =
            Card;
    }

    private static void ApplyFlatButtonBase(
        Button button)
    {
        button.FlatStyle =
            FlatStyle.Flat;

        button.UseVisualStyleBackColor =
            false;

        button.FlatAppearance.BorderSize =
            1;

        button.Cursor =
            Cursors.Hand;
    }
}