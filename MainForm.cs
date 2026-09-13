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

    private readonly CheckBox
        _startMinimizedCheckBox;

    private readonly CheckBox
        _startWithWindowsCheckBox;

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

        // -----------------------------------------------------
        // Detected project
        // -----------------------------------------------------

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

        // -----------------------------------------------------
        // Detected application
        // -----------------------------------------------------

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

        // -----------------------------------------------------
        // Window title
        // -----------------------------------------------------

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

        // -----------------------------------------------------
        // Start minimized
        // -----------------------------------------------------

        _startMinimizedCheckBox =
            new CheckBox
            {
                Text =
                    "Start minimized",

                Left =
                    20,

                Top =
                    225,

                Width =
                    180,

                Checked =
                    _settings.StartMinimized
            };

        // -----------------------------------------------------
        // Start with Windows
        // -----------------------------------------------------

        _startWithWindowsCheckBox =
            new CheckBox
            {
                Text =
                    "Start with Windows",

                Left =
                    220,

                Top =
                    225,

                Width =
                    180,

                Checked =
                    StartupService.IsEnabled()
            };

        // -----------------------------------------------------
        // Refresh
        // -----------------------------------------------------

        var refreshButton =
            new Button
            {
                Text =
                    "Refresh Presence",

                Left =
                    20,

                Top =
                    275,

                Width =
                    140,

                Height =
                    35
            };

        // -----------------------------------------------------
        // Clear
        // -----------------------------------------------------

        var clearButton =
            new Button
            {
                Text =
                    "Clear",

                Left =
                    170,

                Top =
                    275,

                Width =
                    100,

                Height =
                    35
            };

        // -----------------------------------------------------
        // Status
        // -----------------------------------------------------

        _statusLabel =
            new Label
            {
                Text =
                    "Discord: initializing...",

                Left =
                    20,

                Top =
                    335,

                Width =
                    480
            };

        // =====================================================
        // Controls
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
            _startMinimizedCheckBox
        );

        Controls.Add(
            _startWithWindowsCheckBox
        );

        Controls.Add(
            refreshButton
        );

        Controls.Add(
            clearButton
        );

        Controls.Add(
            _statusLabel
        );

        // =====================================================
        // UI events
        // =====================================================

        refreshButton.Click += (_, _) =>
        {
            _presenceController.RefreshPresence();

            UpdatePresenceUi();
        };

        clearButton.Click += (_, _) =>
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
        // Initialize presence
        // =====================================================

        _presenceController.Initialize();

        UpdatePresenceUi();

        // =====================================================
        // Detection timer
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
        };

        _detectionTimer.Start();

        // =====================================================
        // Tray menu
        // =====================================================

        _trayMenu =
            new ContextMenuStrip();

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