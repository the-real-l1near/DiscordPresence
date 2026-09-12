using System.Diagnostics;

namespace DiscordPresence;

public sealed class MainForm : Form
{
    // =========================================================
    // Discord
    // =========================================================

    private readonly DiscordPresenceService _discordPresence;

    // =========================================================
    // Supported applications
    // =========================================================

    private static readonly Dictionary<string, AppPresenceProfile>
        AppProfiles = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Code"] = new(
                "Visual Studio Code",
                "vscode",
                "Visual Studio Code"
            ),

            ["blender"] = new(
                "Blender",
                "blender",
                "Blender"
            ),

            ["UnrealEditor"] = new(
                "Unreal Engine",
                "unreal_v2",
                "Unreal Engine"
            ),

            ["idea64"] = new(
                "IntelliJ IDEA",
                "intellij",
                "IntelliJ IDEA"
            )
        };

    // =========================================================
    // Detection
    // =========================================================

    private readonly System.Windows.Forms.Timer _detectionTimer;

    private bool _wasDiscordRunning;

    private int _discordPresenceRetryTicksRemaining;

    private const int DiscordPresenceRetryTicks =
        10;

    private string? _lastPresenceKey;

    private AppPresenceProfile? _currentProfile;

    private string? _currentProjectName;

    private string? _currentRepositoryName;

    // =========================================================
    // Work session
    // =========================================================

    private DateTime? _sessionStartTime;

    private bool _isIdle =
        true;

    private bool _idlePresenceSent;

    // =========================================================
    // Game override
    // =========================================================

    private bool _isGameOverrideActive;

    // =========================================================
    // Settings
    // =========================================================

    private readonly AppSettings _settings;

    // =========================================================
    // Tray
    // =========================================================

    private readonly NotifyIcon _trayIcon;

    private readonly ContextMenuStrip _trayMenu;

    private bool _isExiting;

    // =========================================================
    // UI
    // =========================================================

    private readonly Label _detectedProjectLabel;

    private readonly Label _detectedAppLabel;

    private readonly Label _windowTitleLabel;

    private readonly CheckBox _elapsedTimeCheckBox;

    private readonly CheckBox _startMinimizedCheckBox;

    private readonly CheckBox _startWithWindowsCheckBox;

    private readonly Label _statusLabel;

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
            480;

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

                Font = new Font(
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

                Font = new Font(
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
        // Show elapsed time
        // -----------------------------------------------------

        _elapsedTimeCheckBox =
            new CheckBox
            {
                Text =
                    "Show elapsed time",

                Left =
                    20,

                Top =
                    225,

                Width =
                    180,

                Checked =
                    _settings.ShowElapsedTime
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
                    220,

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
                    20,

                Top =
                    255,

                Width =
                    180,

                Checked =
                    StartupService.IsEnabled()
            };

        // -----------------------------------------------------
        // Refresh
        // -----------------------------------------------------

        var refreshButton =
            new System.Windows.Forms.Button
            {
                Text =
                    "Refresh Presence",

                Left =
                    20,

                Top =
                    305,

                Width =
                    140,

                Height =
                    35
            };

        // -----------------------------------------------------
        // Clear
        // -----------------------------------------------------

        var clearButton =
            new System.Windows.Forms.Button
            {
                Text =
                    "Clear",

                Left =
                    170,

                Top =
                    305,

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
                    365,

                Width =
                    480
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
            _elapsedTimeCheckBox
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
        // Discord presence service
        // =====================================================

        _discordPresence =
            new DiscordPresenceService();

        // =====================================================
        // UI events
        // =====================================================

        refreshButton.Click += (_, _) =>
        {
            if (_isGameOverrideActive)
            {
                SetStatus(
                    "Game detected · Presence suspended"
                );

                return;
            }

            if (_isIdle)
            {
                _idlePresenceSent =
                    false;

                SetIdlePresence();
            }
            else
            {
                SetPresence();
            }
        };

        clearButton.Click += (_, _) =>
        {
            ClearPresence();
        };

        _elapsedTimeCheckBox.CheckedChanged += (_, _) =>
        {
            _settings.ShowElapsedTime =
                _elapsedTimeCheckBox.Checked;

            SettingsService.Save(
                _settings
            );

            if (!_discordPresence.IsInitialized)
            {
                return;
            }

            if (_isIdle)
            {
                return;
            }

            if (_currentProfile is null)
            {
                return;
            }

            if (_elapsedTimeCheckBox.Checked)
            {
                _sessionStartTime ??=
                    DateTime.UtcNow;

                SetStatus(
                    "Elapsed time enabled."
                );
            }
            else
            {
                SetStatus(
                    "Elapsed time disabled."
                );
            }

            if (_isGameOverrideActive)
            {
                return;
            }

            SetPresence();
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
        // Initialize Discord Social SDK
        // =====================================================

        var initialized =
            _discordPresence.Initialize();

        if (initialized)
        {
            SetStatus(
                "Discord Social SDK: initialized"
            );

            EnterIdle();
        }
        else
        {
            SetStatus(
                "Discord Social SDK: initialization failed"
            );
        }

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
            /*
             * Nếu SDK đang active thì pump callback.
             *
             * Khi game đang suspend SDK,
             * RunCallbacks() tự return.
             */
            _discordPresence.RunCallbacks();

            /*
             * Game override chạy đầu tiên để chặn
             * tất cả code path có thể gửi presence.
             */
            HandleGameOverride();

            /*
             * Update editor / project state.
             */
            DetectActiveApp();

            /*
             * Discord startup / restart recovery.
             */
            HandleDiscordPresenceRecovery();
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
            ClearPresence();
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
    // Detection
    // =========================================================

    private void DetectActiveApp()
    {
        if (!HasSupportedAppRunning())
        {
            EnterIdle();

            return;
        }

        var activeApp =
            ActiveAppDetector
                .GetForegroundApp();

        if (activeApp is null)
        {
            return;
        }

        /*
         * Foreground không phải app supported:
         * giữ state editor gần nhất.
         */
        if (!AppProfiles.TryGetValue(
            activeApp.ProcessName,
            out var profile))
        {
            return;
        }

        _windowTitleLabel.Text =
            string.IsNullOrWhiteSpace(
                activeApp.WindowTitle
            )
                ? "-"
                : activeApp.WindowTitle;

        var projectName =
            ProjectNameDetector.Detect(
                activeApp
            );

        projectName ??=
            "Unknown Project";

        var repositoryName =
            GitRepositoryDetector
                .DetectRepositoryName(
                    projectName
                );

        var presenceKey =
            $"{activeApp.ProcessName}|{projectName}";

        if (string.Equals(
            _lastPresenceKey,
            presenceKey,
            StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (_isIdle)
        {
            _isIdle =
                false;

            _idlePresenceSent =
                false;

            _sessionStartTime =
                DateTime.UtcNow;
        }

        _lastPresenceKey =
            presenceKey;

        _currentProfile =
            profile;

        _currentProjectName =
            projectName;

        _currentRepositoryName =
            repositoryName;

        _detectedAppLabel.Text =
            profile.DisplayName;

        _detectedProjectLabel.Text =
            projectName;

        /*
         * Game override đang active:
         * chỉ update internal state.
         */
        if (_isGameOverrideActive)
        {
            return;
        }

        SetPresence();
    }

    // =========================================================
    // Check supported applications
    // =========================================================

    private static bool HasSupportedAppRunning()
    {
        Process[] processes;

        try
        {
            processes =
                Process.GetProcesses();
        }
        catch
        {
            return false;
        }

        try
        {
            foreach (var process in processes)
            {
                try
                {
                    if (!AppProfiles.ContainsKey(
                        process.ProcessName
                    ))
                    {
                        continue;
                    }

                    if (process.MainWindowHandle !=
                        IntPtr.Zero)
                    {
                        return true;
                    }
                }
                catch
                {
                }
            }

            return false;
        }
        finally
        {
            foreach (var process in processes)
            {
                process.Dispose();
            }
        }
    }

    // =========================================================
    // Discord process
    // =========================================================

    private static bool IsDiscordRunning()
    {
        Process[] processes;

        try
        {
            processes =
                Process.GetProcessesByName(
                    "Discord"
                );
        }
        catch
        {
            return false;
        }

        try
        {
            foreach (var process in processes)
            {
                try
                {
                    if (process.MainWindowHandle !=
                        IntPtr.Zero)
                    {
                        return true;
                    }
                }
                catch
                {
                }
            }

            return false;
        }
        finally
        {
            foreach (var process in processes)
            {
                process.Dispose();
            }
        }
    }

    // =========================================================
    // Game override
    // =========================================================

    private void HandleGameOverride()
    {
        var gameRunning =
            GameDetector.IsForegroundGame();

        // -----------------------------------------------------
        // Game became active
        // -----------------------------------------------------

        if (gameRunning)
        {
            if (_isGameOverrideActive)
            {
                return;
            }

            /*
             * Set flag trước để tất cả code path
             * gửi presence bị block ngay lập tức.
             */
            _isGameOverrideActive =
                true;

            /*
             * Recovery Discord đang chạy dở cũng không
             * còn cần thiết trong lúc chơi game.
             */
            _discordPresenceRetryTicksRemaining =
                0;

            /*
             * Ngắt hoàn toàn Social SDK client.
             */
            if (_discordPresence.IsInitialized)
            {
                _discordPresence.Suspend();
            }

            SetStatus(
                "Game detected · Presence suspended"
            );

            return;
        }

        // -----------------------------------------------------
        // No suspended session
        // -----------------------------------------------------

        if (!_isGameOverrideActive)
        {
            return;
        }

        // -----------------------------------------------------
        // Game lost foreground / exited
        // -----------------------------------------------------

        _isGameOverrideActive =
            false;

        /*
         * Resume Social SDK client.
         */
        var reinitialized =
            _discordPresence.Reinitialize();

        if (!reinitialized)
        {
            SetStatus(
                "Discord Social SDK: reinitialization failed"
            );

            return;
        }

        /*
         * Nếu Discord hiện đang chạy thì đánh dấu luôn
         * để HandleDiscordPresenceRecovery() không
         * reinitialize thêm lần thứ hai trong cùng tick.
         */
        _wasDiscordRunning =
            IsDiscordRunning();

        if (_wasDiscordRunning)
        {
            /*
             * Cho phép resend thêm vài giây nếu Discord
             * vừa mới recover / chưa ready hoàn toàn.
             */
            _discordPresenceRetryTicksRemaining =
                DiscordPresenceRetryTicks;
        }
        else
        {
            _discordPresenceRetryTicksRemaining =
                0;
        }

        // -----------------------------------------------------
        // Restore current presence
        // -----------------------------------------------------

        if (_isIdle)
        {
            _idlePresenceSent =
                false;

            SetIdlePresence();
        }
        else
        {
            SetPresence();
        }
    }

    // =========================================================
    // Discord presence recovery
    // =========================================================

    private void HandleDiscordPresenceRecovery()
    {
        var discordRunning =
            IsDiscordRunning();

        // -----------------------------------------------------
        // Discord stopped
        // -----------------------------------------------------

        if (!discordRunning)
        {
            _wasDiscordRunning =
                false;

            _discordPresenceRetryTicksRemaining =
                0;

            return;
        }

        // -----------------------------------------------------
        // Game currently has priority
        // -----------------------------------------------------

        if (_isGameOverrideActive)
        {
            /*
             * Không resume SDK khi game đang foreground.
             */
            return;
        }

        // -----------------------------------------------------
        // Discord started / restarted
        // -----------------------------------------------------

        if (!_wasDiscordRunning)
        {
            _wasDiscordRunning =
                true;

            var reinitialized =
                _discordPresence.Reinitialize();

            if (!reinitialized)
            {
                SetStatus(
                    "Discord Social SDK: reinitialization failed"
                );

                return;
            }

            _discordPresenceRetryTicksRemaining =
                DiscordPresenceRetryTicks;
        }

        // -----------------------------------------------------
        // No recovery needed
        // -----------------------------------------------------

        if (_discordPresenceRetryTicksRemaining <=
            0)
        {
            return;
        }

        // -----------------------------------------------------
        // Resend current presence
        // -----------------------------------------------------

        if (_isIdle)
        {
            _idlePresenceSent =
                false;

            SetIdlePresence();
        }
        else
        {
            SetPresence();
        }

        _discordPresenceRetryTicksRemaining--;
    }

    // =========================================================
    // Active Presence
    // =========================================================

    private void SetPresence()
    {
        if (!_discordPresence.IsInitialized)
        {
            SetStatus(
                "Discord chưa kết nối."
            );

            return;
        }

        if (_isGameOverrideActive)
        {
            return;
        }

        var profile =
            _currentProfile;

        if (profile is null)
        {
            SetStatus(
                "Chưa phát hiện ứng dụng được hỗ trợ."
            );

            return;
        }

        var projectName =
            _currentProjectName;

        var repositoryName =
            _currentRepositoryName;

        // -----------------------------------------------------
        // Work session
        // -----------------------------------------------------

        _sessionStartTime ??=
            DateTime.UtcNow;

        DateTime? startTime =
            null;

        if (_elapsedTimeCheckBox.Checked)
        {
            startTime =
                _sessionStartTime;
        }

        // -----------------------------------------------------
        // Presence
        // -----------------------------------------------------

        var updated =
            _discordPresence.SetDevelopmentPresence(
                profile,
                projectName,
                repositoryName,
                startTime
            );

        if (!updated)
        {
            SetStatus(
                "Could not update Discord presence."
            );

            return;
        }

        SetStatus(
            $"Presence updated: " +
            $"{projectName} · " +
            $"{profile.DisplayName}"
        );
    }

    // =========================================================
    // Idle
    // =========================================================

    private void EnterIdle()
    {
        if (_isIdle &&
            _idlePresenceSent)
        {
            return;
        }

        _isIdle =
            true;

        // -----------------------------------------------------
        // End work session
        // -----------------------------------------------------

        _sessionStartTime =
            null;

        _lastPresenceKey =
            null;

        _currentProfile =
            null;

        _currentProjectName =
            null;

        _currentRepositoryName =
            null;

        // -----------------------------------------------------
        // UI
        // -----------------------------------------------------

        _detectedProjectLabel.Text =
            "None";

        _detectedAppLabel.Text =
            "Idle";

        _windowTitleLabel.Text =
            "-";

        // -----------------------------------------------------
        // Game override
        // -----------------------------------------------------

        if (_isGameOverrideActive)
        {
            return;
        }

        SetIdlePresence();
    }

    private void SetIdlePresence()
    {
        if (!_discordPresence.IsInitialized)
        {
            SetStatus(
                "Discord chưa kết nối."
            );

            return;
        }

        if (_isGameOverrideActive)
        {
            return;
        }

        var updated =
            _discordPresence.SetIdlePresence();

        if (!updated)
        {
            SetStatus(
                "Could not update Idle presence."
            );

            return;
        }

        _idlePresenceSent =
            true;

        SetStatus(
            "Idle · No supported app running"
        );
    }

    // =========================================================
    // Clear Presence
    // =========================================================

    private void ClearPresence()
    {
        if (!_discordPresence.IsInitialized)
        {
            SetStatus(
                "Discord chưa kết nối."
            );

            return;
        }

        _discordPresence.ClearPresence();

        SetStatus(
            "Presence cleared."
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
    // UI
    // =========================================================

    private void SetStatus(
        string text)
    {
        if (InvokeRequired)
        {
            BeginInvoke(
                () => SetStatus(text)
            );

            return;
        }

        _statusLabel.Text =
            text;
    }

    // =========================================================
    // Cleanup
    // =========================================================

    protected override void OnFormClosed(
        FormClosedEventArgs e)
    {
        _detectionTimer.Stop();

        _detectionTimer.Dispose();

        _discordPresence.Dispose();

        _trayIcon.Visible =
            false;

        _trayIcon.Dispose();

        _trayMenu.Dispose();

        base.OnFormClosed(e);
    }
}