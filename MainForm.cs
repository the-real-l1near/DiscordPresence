namespace DiscordPresence;

public sealed class MainForm : Form
{
    // =========================================================
    // Controller
    // =========================================================

    private readonly PresenceController
        _presenceController;

    // =========================================================
    // Detection
    // =========================================================

    private readonly System.Windows.Forms.Timer
        _detectionTimer;

    // =========================================================
    // Settings
    // =========================================================

    private readonly AppSettings
        _settings;

    // =========================================================
    // Tray
    // =========================================================

    private readonly NotifyIcon
        _trayIcon;

    private readonly ContextMenuStrip
        _trayMenu;

    private readonly ToolStripMenuItem
        _traySuspectQuestionItem;

    private readonly ToolStripMenuItem
        _traySuspectYesItem;

    private readonly ToolStripMenuItem
        _traySuspectNoItem;

    private readonly ToolStripMenuItem
        _traySuspectLaterItem;

    private readonly ToolStripSeparator
        _traySuspectSeparator;

    private Guid? _lastNotifiedSuspectId;

    private bool _isExiting;

    // =========================================================
    // UI
    // =========================================================

    private readonly Label
        _detectedProjectLabel;

    private readonly Label
        _detectedAppLabel;

    private readonly Label
        _windowTitleLabel;

    // Suspected game

    private readonly Panel
        _suspectedGamePanel;

    private readonly Label
        _suspectedGameNameLabel;

    private readonly Label
        _suspectedGameProcessLabel;

    private readonly Button
        _suspectedGameYesButton;

    private readonly Button
        _suspectedGameNoButton;

    private readonly Button
        _suspectedGameLaterButton;

    // Settings

    private readonly CheckBox
        _startMinimizedCheckBox;

    private readonly CheckBox
        _startWithWindowsCheckBox;

    // Actions / status

    private readonly Button
        _refreshButton;

    private readonly Button
        _clearButton;

    private readonly Label
        _statusLabel;

    // =========================================================
    // Constructor
    // =========================================================

    public MainForm()
    {
        // -----------------------------------------------------
        // Settings
        // -----------------------------------------------------

        _settings =
            SettingsService.Load();

        // -----------------------------------------------------
        // Controller
        // -----------------------------------------------------

        _presenceController =
            new PresenceController();

        // -----------------------------------------------------
        // Window
        // -----------------------------------------------------

        Text =
            "Custom Discord Presence";

        var appIcon =
            System.Drawing.Icon.ExtractAssociatedIcon(
                Application.ExecutablePath
            );

        if (appIcon is not null)
        {
            Icon =
                appIcon;
        }

        Width =
            540;

        Height =
            430;

        StartPosition =
            FormStartPosition.CenterScreen;

        FormBorderStyle =
            FormBorderStyle.FixedSingle;

        MaximizeBox =
            false;

        // =====================================================
        // Project
        // =====================================================

        var projectTitleLabel =
            new Label
            {
                Text =
                    "Detected project",

                Left =
                    20,

                Top =
                    20,

                Width =
                    180
            };

        _detectedProjectLabel =
            new Label
            {
                Text =
                    "None",

                Left =
                    20,

                Top =
                    45,

                Width =
                    480,

                Font =
                    new Font(
                        Font,
                        FontStyle.Bold
                    )
            };

        // =====================================================
        // Application
        // =====================================================

        var appTitleLabel =
            new Label
            {
                Text =
                    "Detected application",

                Left =
                    20,

                Top =
                    85,

                Width =
                    180
            };

        _detectedAppLabel =
            new Label
            {
                Text =
                    "Idle",

                Left =
                    20,

                Top =
                    110,

                Width =
                    480,

                Font =
                    new Font(
                        Font,
                        FontStyle.Bold
                    )
            };

        // =====================================================
        // Window title
        // =====================================================

        var windowTitleTitleLabel =
            new Label
            {
                Text =
                    "Window title",

                Left =
                    20,

                Top =
                    150,

                Width =
                    180
            };

        _windowTitleLabel =
            new Label
            {
                Text =
                    "-",

                Left =
                    20,

                Top =
                    175,

                Width =
                    480,

                Height =
                    40,

                AutoEllipsis =
                    true
            };

        // =====================================================
        // Suspected game panel
        // =====================================================

        _suspectedGamePanel =
            new Panel
            {
                Left =
                    20,

                Top =
                    220,

                Width =
                    485,

                Height =
                    115,

                BorderStyle =
                    BorderStyle.FixedSingle,

                Visible =
                    false
            };

        var suspectedGameTitleLabel =
            new Label
            {
                Text =
                    "Suspected game",

                Left =
                    10,

                Top =
                    8,

                Width =
                    150,

                Font =
                    new Font(
                        Font,
                        FontStyle.Bold
                    )
            };

        _suspectedGameNameLabel =
            new Label
            {
                Text =
                    "-",

                Left =
                    10,

                Top =
                    30,

                Width =
                    460,

                AutoEllipsis =
                    true
            };

        _suspectedGameProcessLabel =
            new Label
            {
                Text =
                    string.Empty,

                Left =
                    10,

                Top =
                    50,

                Width =
                    460,

                ForeColor =
                    SystemColors.GrayText,
                
                AutoEllipsis =
                    true
            };

        _suspectedGameYesButton =
            new Button
            {
                Text =
                    "Yes",

                Left =
                    10,

                Top =
                    75,

                Width =
                    90,

                Height =
                    28
            };

        _suspectedGameNoButton =
            new Button
            {
                Text =
                    "No",

                Left =
                    110,

                Top =
                    75,

                Width =
                    90,

                Height =
                    28
            };

        _suspectedGameLaterButton =
            new Button
            {
                Text =
                    "Later",

                Left =
                    210,

                Top =
                    75,

                Width =
                    90,

                Height =
                    28
            };

        _suspectedGamePanel.Controls.Add(
            suspectedGameTitleLabel
        );

        _suspectedGamePanel.Controls.Add(
            _suspectedGameNameLabel
        );

        _suspectedGamePanel.Controls.Add(
            _suspectedGameProcessLabel
        );

        _suspectedGamePanel.Controls.Add(
            _suspectedGameYesButton
        );

        _suspectedGamePanel.Controls.Add(
            _suspectedGameNoButton
        );

        _suspectedGamePanel.Controls.Add(
            _suspectedGameLaterButton
        );

        // =====================================================
        // Settings
        // =====================================================

        _startMinimizedCheckBox =
            new CheckBox
            {
                Text =
                    "Start minimized",

                Left =
                    20,

                Width =
                    180,

                Checked =
                    _settings.StartMinimized
            };

        _startWithWindowsCheckBox =
            new CheckBox
            {
                Text =
                    "Start with Windows",

                Left =
                    220,

                Width =
                    180,

                Checked =
                    StartupService.IsEnabled()
            };

        // =====================================================
        // Buttons
        // =====================================================

        _refreshButton =
            new Button
            {
                Text =
                    "Refresh Presence",

                Left =
                    20,

                Width =
                    140,

                Height =
                    35
            };

        _clearButton =
            new Button
            {
                Text =
                    "Clear",

                Left =
                    170,

                Width =
                    100,

                Height =
                    35
            };

        // =====================================================
        // Status
        // =====================================================

        _statusLabel =
            new Label
            {
                Text =
                    "Discord: initializing...",

                Left =
                    20,

                Width =
                    480,

                Height =
                    30
            };

        // =====================================================
        // Add controls
        // =====================================================

        Controls.Add(
            projectTitleLabel
        );

        Controls.Add(
            _detectedProjectLabel
        );

        Controls.Add(
            appTitleLabel
        );

        Controls.Add(
            _detectedAppLabel
        );

        Controls.Add(
            windowTitleTitleLabel
        );

        Controls.Add(
            _windowTitleLabel
        );

        Controls.Add(
            _suspectedGamePanel
        );

        Controls.Add(
            _startMinimizedCheckBox
        );

        Controls.Add(
            _startWithWindowsCheckBox
        );

        Controls.Add(
            _refreshButton
        );

        Controls.Add(
            _clearButton
        );

        Controls.Add(
            _statusLabel
        );

        // =====================================================
        // Main UI events
        // =====================================================

        _suspectedGameYesButton.Click += (_, _) =>
        {
            ResolveSuspectedGameYes();
        };

        _suspectedGameNoButton.Click += (_, _) =>
        {
            ResolveSuspectedGameNo();
        };

        _suspectedGameLaterButton.Click += (_, _) =>
        {
            ResolveSuspectedGameLater();
        };

        _refreshButton.Click += (_, _) =>
        {
            _presenceController.RefreshPresence();

            UpdatePresenceUi();
        };

        _clearButton.Click += (_, _) =>
        {
            _presenceController.ClearPresence();

            UpdatePresenceUi();
        };

        _startMinimizedCheckBox.CheckedChanged += (_, _) =>
        {
            _settings.StartMinimized =
                _startMinimizedCheckBox.Checked;

            SettingsService.Save(
                _settings
            );
        };

        _startWithWindowsCheckBox.CheckedChanged += (_, _) =>
        {
            try
            {
                StartupService.SetEnabled(
                    _startWithWindowsCheckBox.Checked
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not update Windows startup:\n\n{ex.Message}",
                    "Discord Presence",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                _startWithWindowsCheckBox.Checked =
                    StartupService.IsEnabled();
            }
        };

        // =====================================================
        // Tray menu
        // =====================================================

        _trayMenu =
            new ContextMenuStrip();

        _traySuspectQuestionItem =
            new ToolStripMenuItem
            {
                Enabled =
                    false,

                Visible =
                    false
            };

        _traySuspectYesItem =
            new ToolStripMenuItem(
                "Yes"
            )
            {
                Visible =
                    false
            };

        _traySuspectNoItem =
            new ToolStripMenuItem(
                "No"
            )
            {
                Visible =
                    false
            };

        _traySuspectLaterItem =
            new ToolStripMenuItem(
                "Later"
            )
            {
                Visible =
                    false
            };

        _traySuspectSeparator =
            new ToolStripSeparator
            {
                Visible =
                    false
            };

        var openMenuItem =
            new ToolStripMenuItem(
                "Open"
            );

        var clearPresenceMenuItem =
            new ToolStripMenuItem(
                "Clear Presence"
            );

        var exitMenuItem =
            new ToolStripMenuItem(
                "Exit"
            );

        _traySuspectYesItem.Click += (_, _) =>
        {
            ResolveSuspectedGameYes();
        };

        _traySuspectNoItem.Click += (_, _) =>
        {
            ResolveSuspectedGameNo();
        };

        _traySuspectLaterItem.Click += (_, _) =>
        {
            ResolveSuspectedGameLater();
        };

        openMenuItem.Click += (_, _) =>
        {
            ShowFromTray();
        };

        clearPresenceMenuItem.Click += (_, _) =>
        {
            _presenceController.ClearPresence();

            UpdatePresenceUi();
        };

        exitMenuItem.Click += (_, _) =>
        {
            ExitApplication();
        };

        _trayMenu.Items.Add(
            _traySuspectQuestionItem
        );

        _trayMenu.Items.Add(
            _traySuspectYesItem
        );

        _trayMenu.Items.Add(
            _traySuspectNoItem
        );

        _trayMenu.Items.Add(
            _traySuspectLaterItem
        );

        _trayMenu.Items.Add(
            _traySuspectSeparator
        );

        _trayMenu.Items.Add(
            openMenuItem
        );

        _trayMenu.Items.Add(
            clearPresenceMenuItem
        );

        _trayMenu.Items.Add(
            new ToolStripSeparator()
        );

        _trayMenu.Items.Add(
            exitMenuItem
        );

        // =====================================================
        // Tray icon
        // =====================================================

        _trayIcon =
            new NotifyIcon
            {
                Text =
                    "Custom Discord Presence",

                Icon =
                    appIcon ??
                    SystemIcons.Application,

                Visible =
                    true,

                ContextMenuStrip =
                    _trayMenu
            };

        _trayIcon.DoubleClick += (_, _) =>
        {
            ShowFromTray();
        };

        // =====================================================
        // Initialize controller
        // =====================================================

        _presenceController.Initialize();

        UpdatePresenceUi();

        // =====================================================
        // Timer
        // =====================================================

        _detectionTimer =
            new System.Windows.Forms.Timer
            {
                Interval =
                    1000
            };

        _detectionTimer.Tick += (_, _) =>
        {
            _presenceController.Tick();

            UpdatePresenceUi();

            NotifyNewSuspectedGame();
        };

        _detectionTimer.Start();

        // =====================================================
        // Window events
        // =====================================================

        FormClosing +=
            MainForm_FormClosing;

        Shown += (_, _) =>
        {
            if (_settings.StartMinimized)
            {
                BeginInvoke(
                    () => HideToTray()
                );
            }
        };
    }

    // =========================================================
    // Suspected game actions
    // =========================================================

    private void ResolveSuspectedGameYes()
    {
        _presenceController
            .ConfirmSuspectedGame();

        UpdatePresenceUi();
    }

    private void ResolveSuspectedGameNo()
    {
        _presenceController
            .RejectSuspectedGame();

        UpdatePresenceUi();
    }

    private void ResolveSuspectedGameLater()
    {
        _presenceController
            .DeferSuspectedGame();

        UpdatePresenceUi();
    }

    // =========================================================
    // Notification
    // =========================================================

    private void NotifyNewSuspectedGame()
    {
        var suspect =
            _presenceController
                .PendingSuspectedGame;

        if (suspect is null)
        {
            return;
        }

        if (_lastNotifiedSuspectId ==
            suspect.Id)
        {
            return;
        }

        _lastNotifiedSuspectId =
            suspect.Id;

        _trayIcon.ShowBalloonTip(
            5000,
            "Suspected game detected",
            $"Is \"{suspect.ProcessName}.exe\" a game?\n" +
            "Right-click the tray icon to confirm.",
            ToolTipIcon.Info
        );
    }

    // =========================================================
    // Presence UI
    // =========================================================

    private void UpdatePresenceUi()
    {
        _detectedProjectLabel.Text =
            _presenceController
                .DetectedProjectName;

        _detectedAppLabel.Text =
            _presenceController
                .DetectedApplicationName;

        _windowTitleLabel.Text =
            _presenceController
                .WindowTitle;

        _statusLabel.Text =
            _presenceController
                .StatusText;

        UpdateSuspectedGameUi();
    }

    // =========================================================
    // Suspected game UI
    // =========================================================

    private void UpdateSuspectedGameUi()
    {
        var suspect =
            _presenceController
                .PendingSuspectedGame;

        var hasSuspect =
            suspect is not null;

        _suspectedGamePanel.Visible =
            hasSuspect;

        if (suspect is not null)
        {
            _suspectedGameNameLabel.Text =
                $"Is \"{suspect.ProcessName}.exe\" a game?";

            _suspectedGameProcessLabel.Text =
                string.IsNullOrWhiteSpace(
                    suspect.WindowTitle
                )
                    ? "Window: -"
                    : $"Window: {suspect.WindowTitle}";

            _traySuspectQuestionItem.Text =
                $"Is \"{suspect.ProcessName}.exe\" a game?";
        }

        // -----------------------------------------------------
        // Tray suspect block
        // -----------------------------------------------------

        _traySuspectQuestionItem.Visible =
            hasSuspect;

        _traySuspectYesItem.Visible =
            hasSuspect;

        _traySuspectNoItem.Visible =
            hasSuspect;

        _traySuspectLaterItem.Visible =
            hasSuspect;

        _traySuspectSeparator.Visible =
            hasSuspect;

        // -----------------------------------------------------
        // Dynamic window layout
        // -----------------------------------------------------

        if (hasSuspect)
        {
            _startMinimizedCheckBox.Top =
                350;

            _startWithWindowsCheckBox.Top =
                350;

            _refreshButton.Top =
                395;

            _clearButton.Top =
                395;

            _statusLabel.Top =
                450;

            Height =
                530;
        }
        else
        {
            _startMinimizedCheckBox.Top =
                225;

            _startWithWindowsCheckBox.Top =
                225;

            _refreshButton.Top =
                275;

            _clearButton.Top =
                275;

            _statusLabel.Top =
                335;

            Height =
                430;
        }
    }

    // =========================================================
    // Tray window
    // =========================================================

    private void HideToTray()
    {
        Hide();

        ShowInTaskbar =
            false;
    }

    private void ShowFromTray()
    {
        ShowInTaskbar =
            true;

        Show();

        WindowState =
            FormWindowState.Normal;

        Activate();

        BringToFront();
    }

    // =========================================================
    // Exit
    // =========================================================

    private void ExitApplication()
    {
        _isExiting =
            true;

        Close();
    }

    // =========================================================
    // Close button
    // =========================================================

    private void MainForm_FormClosing(
        object? sender,
        FormClosingEventArgs e)
    {
        if (_isExiting)
        {
            return;
        }

        if (e.CloseReason !=
            CloseReason.UserClosing)
        {
            return;
        }

        e.Cancel =
            true;

        using var dialog =
            new CloseActionDialog();

        var result =
            dialog.ShowDialog(this);

        switch (result)
        {
            case DialogResult.Yes:
                HideToTray();
                break;

            case DialogResult.No:
                _isExiting =
                    true;

                Close();
                break;

            case DialogResult.Cancel:
            default:
                break;
        }
    }

    // =========================================================
    // Cleanup
    // =========================================================

    protected override void OnFormClosed(
        FormClosedEventArgs e)
    {
        _detectionTimer.Stop();

        _detectionTimer.Dispose();

        _presenceController.Dispose();

        _trayIcon.Visible =
            false;

        _trayIcon.Dispose();

        _trayMenu.Dispose();

        base.OnFormClosed(e);
    }
}