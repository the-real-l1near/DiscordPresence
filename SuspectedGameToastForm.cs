namespace DiscordPresence;

internal sealed class SuspectedGameToastForm : Form
{
    private readonly System.Windows.Forms.Timer
        _closeTimer;

    private readonly Action
        _onYes;

    private readonly Action
        _onNo;

    private readonly Action
        _onLater;

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

        // =====================================================
        // Window
        // =====================================================

        FormBorderStyle =
            FormBorderStyle.None;

        ShowInTaskbar =
            false;

        TopMost =
            true;

        StartPosition =
            FormStartPosition.Manual;

        Width =
            360;

        Height =
            150;

        BackColor =
            SystemColors.Window;

        // =====================================================
        // Title
        // =====================================================

        var titleLabel =
            new Label
            {
                Text =
                    "Suspected game detected",

                Left =
                    14,

                Top =
                    12,

                Width =
                    330,

                Height =
                    22,

                Font =
                    new Font(
                        Font,
                        FontStyle.Bold
                    )
            };

        // =====================================================
        // Process
        // =====================================================

        var processLabel =
            new Label
            {
                Text =
                    $"Is \"{suspect.ProcessName}.exe\" a game?",

                Left =
                    14,

                Top =
                    38,

                Width =
                    330,

                Height =
                    22,

                AutoEllipsis =
                    true
            };

        // =====================================================
        // Window title
        // =====================================================

        var windowLabel =
            new Label
            {
                Text =
                    string.IsNullOrWhiteSpace(
                        suspect.WindowTitle
                    )
                        ? "Window: -"
                        : $"Window: {suspect.WindowTitle}",

                Left =
                    14,

                Top =
                    62,

                Width =
                    330,

                Height =
                    22,

                ForeColor =
                    SystemColors.GrayText,

                AutoEllipsis =
                    true
            };

        // =====================================================
        // Buttons
        // =====================================================

        var yesButton =
            new Button
            {
                Text =
                    "Yes",

                Left =
                    14,

                Top =
                    100,

                Width =
                    90,

                Height =
                    30
            };

        var noButton =
            new Button
            {
                Text =
                    "No",

                Left =
                    114,

                Top =
                    100,

                Width =
                    90,

                Height =
                    30
            };

        var laterButton =
            new Button
            {
                Text =
                    "Later",

                Left =
                    214,

                Top =
                    100,

                Width =
                    90,

                Height =
                    30
            };

        // =====================================================
        // Auto close
        // =====================================================

        _closeTimer =
            new System.Windows.Forms.Timer
            {
                Interval =
                    8000
            };

        _closeTimer.Tick += (_, _) =>
        {
            _closeTimer.Stop();

            Close();
        };

        Shown += (_, _) =>
        {
            PositionToast();

            _closeTimer.Start();
        };

        // =====================================================
        // Button events
        // =====================================================

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

        // =====================================================
        // Controls
        // =====================================================

        Controls.Add(
            titleLabel
        );

        Controls.Add(
            processLabel
        );

        Controls.Add(
            windowLabel
        );

        Controls.Add(
            yesButton
        );

        Controls.Add(
            noButton
        );

        Controls.Add(
            laterButton
        );
    }

    // =========================================================
    // Position
    // =========================================================

    private void PositionToast()
    {
        var workingArea =
            Screen.PrimaryScreen?.WorkingArea
            ?? Screen.FromControl(this).WorkingArea;

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