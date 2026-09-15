namespace DiscordPresence;

public sealed partial class MainForm : Form
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

    private Guid?
        _lastNotifiedSuspectId;

    private SuspectedGameToastForm?
        _suspectedGameToast;

    private bool
        _isExiting;

    // =========================================================
    // Constructor
    // =========================================================

    public MainForm()
    {
        InitializeComponent();

        // -----------------------------------------------------
        // Settings
        // -----------------------------------------------------

        _settings =
            SettingsService.Load();

        _startMinimizedCheckBox.Checked =
            _settings.StartMinimized;

        _startWithWindowsCheckBox.Checked =
            StartupService.IsEnabled();

        // -----------------------------------------------------
        // Controller
        // -----------------------------------------------------

        _presenceController =
            new PresenceController();

        // -----------------------------------------------------
        // Runtime assets
        // -----------------------------------------------------

        ApplyRuntimeAssets();

        // -----------------------------------------------------
        // Application icon
        // -----------------------------------------------------

        var appIcon =
            System.Drawing.Icon.ExtractAssociatedIcon(
                Application.ExecutablePath
            );

        if (appIcon is not null)
        {
            Icon =
                appIcon;
        }

        // -----------------------------------------------------
        // Events
        // -----------------------------------------------------

        WireEvents();

        // -----------------------------------------------------
        // Tray
        // -----------------------------------------------------

        _trayMenu =
            new ContextMenuStrip();

        _traySuspectQuestionItem =
            new ToolStripMenuItem
            {
                Enabled = false,
                Visible = false
            };

        _traySuspectYesItem =
            new ToolStripMenuItem("Yes")
            {
                Visible = false
            };

        _traySuspectNoItem =
            new ToolStripMenuItem("No")
            {
                Visible = false
            };

        _traySuspectLaterItem =
            new ToolStripMenuItem("Later")
            {
                Visible = false
            };

        _traySuspectSeparator =
            new ToolStripSeparator
            {
                Visible = false
            };

        var openMenuItem =
            new ToolStripMenuItem("Open");

        var clearPresenceMenuItem =
            new ToolStripMenuItem(
                "Clear Presence"
            );

        var exitMenuItem =
            new ToolStripMenuItem("Exit");

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
            _presenceController
                .ClearPresence();

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

        // -----------------------------------------------------
        // Controller initialize
        // -----------------------------------------------------

        _presenceController.Initialize();

        UpdatePresenceUi();

        // -----------------------------------------------------
        // Detection timer
        // -----------------------------------------------------

        _detectionTimer =
            new System.Windows.Forms.Timer
            {
                Interval = 1000
            };

        _detectionTimer.Tick += (_, _) =>
        {
            _presenceController.Tick();

            UpdatePresenceUi();

            NotifyNewSuspectedGame();
        };

        _detectionTimer.Start();

        // -----------------------------------------------------
        // Window
        // -----------------------------------------------------

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
    // Runtime assets
    // =========================================================

    private void ApplyRuntimeAssets()
    {
        _projectIcon.Image =
            UiAssets.Folder(
                Math.Min(
                    _projectIcon.Width,
                    _projectIcon.Height
                )
            );

        _applicationIcon.Image =
            UiAssets.App(
                Math.Min(
                    _applicationIcon.Width,
                    _applicationIcon.Height
                )
            );

        _windowIcon.Image =
            UiAssets.Window(
                Math.Min(
                    _windowIcon.Width,
                    _windowIcon.Height
                )
            );

        _suspectedGameIcon.Image =
            UiAssets.Gamepad(
                Math.Min(
                    _suspectedGameIcon.Width,
                    _suspectedGameIcon.Height
                )
            );

        _manageGameOverridesButton.Image =
            UiAssets.Settings(24);

        _refreshButton.Image =
            UiAssets.Refresh(20);

        _clearButton.Image =
            UiAssets.Trash(20);

        _refreshButton.HoverImage =
            UiAssets.RefreshHover(20);

        _clearButton.HoverImage =
            UiAssets.TrashHover(20);

        _statusIcon.Image =
            UiAssets.Info(
                Math.Min(
                    _statusIcon.Width,
                    _statusIcon.Height
                )
            );
    }

    // =========================================================
    // Runtime events
    // =========================================================

    private void WireEvents()
    {
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

        _manageGameOverridesButton.Click += (_, _) =>
        {
            using var dialog =
                new GameOverridesForm(
                    _presenceController
                );

            dialog.ShowDialog(
                this
            );

            UpdatePresenceUi();
        };

        _refreshButton.Click += (_, _) =>
        {
            _presenceController
                .RefreshPresence();

            UpdatePresenceUi();
        };

        _clearButton.Click += (_, _) =>
        {
            _presenceController
                .ClearPresence();

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
                using var dialog =
                    new AppDialog(
                        "Windows startup error",
                        "Could not update Windows startup",
                        ex.Message,
                        AppDialogTone.Danger,

                        new AppDialogAction(
                            "OK",
                            DialogResult.OK,
                            AppDialogButtonKind.Primary
                        )
                    );

                dialog.ShowDialog(
                    this
                );

                _startWithWindowsCheckBox.Checked =
                    StartupService.IsEnabled();
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

        CloseSuspectedGameToast();

        UpdatePresenceUi();
    }

    private void ResolveSuspectedGameNo()
    {
        _presenceController
            .RejectSuspectedGame();

        CloseSuspectedGameToast();

        UpdatePresenceUi();
    }

    private void ResolveSuspectedGameLater()
    {
        _presenceController
            .DeferSuspectedGame();

        CloseSuspectedGameToast();

        UpdatePresenceUi();
    }

    // =========================================================
    // Suspected game toast
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

        if (
            _lastNotifiedSuspectId ==
            suspect.Id
        )
        {
            return;
        }

        _lastNotifiedSuspectId =
            suspect.Id;

        CloseSuspectedGameToast();

        _suspectedGameToast =
            new SuspectedGameToastForm(
                suspect,

                onYes: () =>
                {
                    ResolveSuspectedGameYes();
                },

                onNo: () =>
                {
                    ResolveSuspectedGameNo();
                },

                onLater: () =>
                {
                    ResolveSuspectedGameLater();
                }
            );

        _suspectedGameToast.FormClosed += (_, _) =>
        {
            _suspectedGameToast =
                null;
        };

        _suspectedGameToast.Show();
    }

    private void CloseSuspectedGameToast()
    {
        if (_suspectedGameToast is null)
        {
            return;
        }

        var toast =
            _suspectedGameToast;

        _suspectedGameToast =
            null;

        if (!toast.IsDisposed)
        {
            toast.Close();
        }
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

        UpdateStatusStyle();

        UpdateSuspectedGameUi();
    }

    // =========================================================
    // Status
    // =========================================================

    private void UpdateStatusStyle()
    {
        var status =
            _presenceController
                .StatusText;

        var statusSize =
            Math.Min(
                _statusIcon.Width,
                _statusIcon.Height
            );

        if (
            status.Contains(
                "failed",
                StringComparison.OrdinalIgnoreCase
            ) ||
            status.Contains(
                "could not",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            _statusLabel.ForeColor =
                UiTheme.Danger;

            _statusIcon.Image =
                UiAssets.Warning(
                    statusSize
                );

            return;
        }

        if (
            status.Contains(
                "Suspected",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            _statusLabel.ForeColor =
                UiTheme.Accent;

            _statusIcon.Image =
                UiAssets.Warning(
                    statusSize
                );

            return;
        }

        if (
            status.Contains(
                "updated",
                StringComparison.OrdinalIgnoreCase
            ) ||
            status.Contains(
                "confirmed",
                StringComparison.OrdinalIgnoreCase
            ) ||
            status.Contains(
                "cleared",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            _statusLabel.ForeColor =
                Color.FromArgb(
                    22,
                    163,
                    74
                );

            _statusIcon.Image =
                UiAssets.Check(
                    statusSize
                );

            return;
        }

        _statusLabel.ForeColor =
            UiTheme.TextSecondary;

        _statusIcon.Image =
            UiAssets.Info(
                statusSize
            );
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
        // Tray
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
        // Automatic vertical layout
        // -----------------------------------------------------

        var settingsTop =
            hasSuspect
                ? _suspectedGamePanel.Bottom + 12
                : _informationPanel.Bottom + 16;

        // -----------------------------------------------------
        // Checkboxes
        // -----------------------------------------------------

        _startMinimizedCheckBox.Top =
            settingsTop;

        _startWithWindowsCheckBox.Top =
            settingsTop;

        // -----------------------------------------------------
        // Manage game
        // -----------------------------------------------------

        var checkboxBottom =
            Math.Max(
                _startMinimizedCheckBox.Bottom,
                _startWithWindowsCheckBox.Bottom
            );

        _manageGameOverridesButton.Top =
            checkboxBottom + 10;

        // -----------------------------------------------------
        // Actions
        // -----------------------------------------------------

        _refreshButton.Top =
            _manageGameOverridesButton.Bottom + 10;

        _clearButton.Top =
            _refreshButton.Top;

        // -----------------------------------------------------
        // Status
        // -----------------------------------------------------

        var actionBottom =
            Math.Max(
                _refreshButton.Bottom,
                _clearButton.Bottom
            );

        _statusIcon.Top =
            actionBottom + 12;

        _statusLabel.Top =
            _statusIcon.Top - 1;

        // -----------------------------------------------------
        // Automatic form height
        // -----------------------------------------------------

        var contentBottom =
            Math.Max(
                _statusIcon.Bottom,
                _statusLabel.Bottom
            );

        ClientSize =
            new Size(
                ClientSize.Width,
                contentBottom + 18
            );
    }

    // =========================================================
    // Tray
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
    // Close
    // =========================================================

    private void MainForm_FormClosing(
        object? sender,
        FormClosingEventArgs e)
    {
        if (_isExiting)
        {
            return;
        }

        if (
            e.CloseReason !=
            CloseReason.UserClosing
        )
        {
            return;
        }

        e.Cancel =
            true;

        using var dialog =
            new CloseActionDialog();

        var result =
            dialog.ShowDialog(
                this
            );

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

        CloseSuspectedGameToast();

        _presenceController.Dispose();

        _trayIcon.Visible =
            false;

        _trayIcon.Dispose();

        _trayMenu.Dispose();

        UiAssets.Dispose();

        base.OnFormClosed(
            e
        );
    }
}