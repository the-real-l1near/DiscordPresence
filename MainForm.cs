using DiscordRPC;

namespace DiscordPresence;

public sealed class MainForm : Form
{
    // =========================================================
    // Discord
    // =========================================================

    private const string DiscordApplicationId =
        "1547227043429613668";

    private readonly DiscordRpcClient _discordClient;

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

    private string? _lastPresenceKey;

    private AppPresenceProfile? _currentProfile;

    private string? _currentProjectName;

    // =========================================================
    // Work session
    // =========================================================

    /*
     * Một work session bắt đầu khi:
     *
     * Idle
     * → mở / chuyển vào một app supported
     *
     * Timer KHÔNG reset khi:
     *
     * VS Code → Blender
     * Blender → Unreal
     * Unreal → IntelliJ
     * project A → project B
     *
     * Timer chỉ reset khi:
     *
     * tất cả app supported đã đóng
     * → Idle
     * → sau đó bắt đầu session mới
     */
    private DateTime? _sessionStartTime;

    private bool _isIdle = true;

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
            Icon = appIcon;
        }

        Width = 540;
        Height = 480;

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

                Left = 20,
                Top = 20,

                Width = 180
            };

        _detectedProjectLabel =
            new Label
            {
                Text =
                    "None",

                Left = 20,
                Top = 45,

                Width = 480,

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

                Left = 20,
                Top = 85,

                Width = 180
            };

        _detectedAppLabel =
            new Label
            {
                Text =
                    "Idle",

                Left = 20,
                Top = 110,

                Width = 480,

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

                Left = 20,
                Top = 150,

                Width = 180
            };

        _windowTitleLabel =
            new Label
            {
                Text =
                    "-",

                Left = 20,
                Top = 175,

                Width = 480,
                Height = 40,

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

                Left = 20,
                Top = 225,

                Width = 180,

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

                Left = 220,
                Top = 225,

                Width = 180,

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

                Left = 20,
                Top = 255,

                Width = 180,

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

                Left = 20,
                Top = 305,

                Width = 140,
                Height = 35
            };

        // -----------------------------------------------------
        // Clear
        // -----------------------------------------------------

        var clearButton =
            new System.Windows.Forms.Button
            {
                Text =
                    "Clear",

                Left = 170,
                Top = 305,

                Width = 100,
                Height = 35
            };

        // -----------------------------------------------------
        // Status
        // -----------------------------------------------------

        _statusLabel =
            new Label
            {
                Text =
                    "Discord: connecting...",

                Left = 20,
                Top = 365,

                Width = 480
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
        // UI events
        // =====================================================

        // -----------------------------------------------------
        // Refresh
        // -----------------------------------------------------

        refreshButton.Click += (_, _) =>
        {
            if (_isIdle)
            {
                SetIdlePresence();
            }
            else
            {
                SetPresence();
            }
        };

        // -----------------------------------------------------
        // Clear
        // -----------------------------------------------------

        clearButton.Click += (_, _) =>
        {
            ClearPresence();
        };

        // -----------------------------------------------------
        // Show elapsed time
        // -----------------------------------------------------

        _elapsedTimeCheckBox.CheckedChanged += (_, _) =>
        {
            _settings.ShowElapsedTime =
                _elapsedTimeCheckBox.Checked;

            SettingsService.Save(
                _settings
            );

            if (!_discordClient.IsInitialized)
            {
                return;
            }

            // Idle không có timer.
            if (_isIdle)
            {
                _discordClient.UpdateClearTime();

                return;
            }

            if (_currentProfile is null)
            {
                return;
            }

            if (_elapsedTimeCheckBox.Checked)
            {
                // Nếu vì lý do nào đó session chưa có start time.
                _sessionStartTime ??=
                    DateTime.UtcNow;

                _discordClient.UpdateStartTime(
                    _sessionStartTime.Value
                );

                SetStatus(
                    "Elapsed time enabled."
                );
            }
            else
            {
                _discordClient.UpdateClearTime();

                SetStatus(
                    "Elapsed time disabled."
                );
            }
        };

        // -----------------------------------------------------
        // Start minimized
        // -----------------------------------------------------

        _startMinimizedCheckBox.CheckedChanged += (_, _) =>
        {
            _settings.StartMinimized =
                _startMinimizedCheckBox.Checked;

            SettingsService.Save(
                _settings
            );
        };

        // -----------------------------------------------------
        // Start with Windows
        // -----------------------------------------------------

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

                // Restore registry state.
                _startWithWindowsCheckBox.Checked =
                    StartupService.IsEnabled();
            }
        };

        // =====================================================
        // Discord RPC
        // =====================================================

        _discordClient =
            new DiscordRpcClient(
                DiscordApplicationId
            );

        _discordClient.OnReady += (_, e) =>
        {
            SetStatus(
                $"Discord: connected as {e.User.Username}"
            );
        };

        _discordClient.OnConnectionEstablished += (_, _) =>
        {
            SetStatus(
                "Discord: connected"
            );
        };

        _discordClient.OnConnectionFailed += (_, _) =>
        {
            SetStatus(
                "Discord: connection failed"
            );
        };

        var initialized =
            _discordClient.Initialize();

        if (!initialized)
        {
            SetStatus(
                "Discord: could not initialize RPC"
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
            DetectActiveApp();
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
                    System.Drawing.Icon.ExtractAssociatedIcon(
                        Application.ExecutablePath
                    ) ?? SystemIcons.Application,

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
        /*
         * Trước tiên check toàn bộ process.
         *
         * Nếu không còn bất kỳ app nào trong mapping chạy
         * thì work session kết thúc và chuyển sang Idle.
         */
        if (!HasSupportedAppRunning())
        {
            EnterIdle();

            return;
        }

        // -----------------------------------------------------
        // Foreground application
        // -----------------------------------------------------

        var activeApp =
            ActiveAppDetector
                .GetForegroundApp();

        if (activeApp is null)
        {
            return;
        }

        /*
         * Foreground là app không hỗ trợ:
         *
         * Chrome
         * Discord
         * Explorer
         * Spotify
         * etc.
         *
         * Nhưng vẫn còn editor/app supported chạy.
         *
         * → giữ nguyên Presence gần nhất.
         * → timer vẫn tiếp tục.
         */
        if (!AppProfiles.TryGetValue(
            activeApp.ProcessName,
            out var profile))
        {
            return;
        }

        // -----------------------------------------------------
        // Window title
        // -----------------------------------------------------

        _windowTitleLabel.Text =
            string.IsNullOrWhiteSpace(
                activeApp.WindowTitle
            )
                ? "-"
                : activeApp.WindowTitle;

        // -----------------------------------------------------
        // Project detection
        // -----------------------------------------------------

        var projectName =
            ProjectNameDetector.Detect(
                activeApp
            );

        projectName ??=
            "Unknown Project";

        var presenceKey =
            $"{activeApp.ProcessName}|{projectName}";

        // -----------------------------------------------------
        // Nothing changed
        // -----------------------------------------------------

        if (string.Equals(
            _lastPresenceKey,
            presenceKey,
            StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // -----------------------------------------------------
        // Leaving Idle → start new work session
        // -----------------------------------------------------

        if (_isIdle)
        {
            _isIdle =
                false;

            _sessionStartTime =
                DateTime.UtcNow;
        }

        /*
         * IMPORTANT:
         *
         * KHÔNG reset _sessionStartTime ở đây.
         *
         * Vì:
         *
         * VS Code → Blender
         * Blender → Unreal
         * Unreal → IntelliJ
         *
         * vẫn thuộc cùng một work session.
         */

        // -----------------------------------------------------
        // Save detection
        // -----------------------------------------------------

        _lastPresenceKey =
            presenceKey;

        _currentProfile =
            profile;

        _currentProjectName =
            projectName;

        // -----------------------------------------------------
        // GUI
        // -----------------------------------------------------

        _detectedAppLabel.Text =
            profile.DisplayName;

        _detectedProjectLabel.Text =
            projectName;

        // -----------------------------------------------------
        // Discord
        // -----------------------------------------------------

        SetPresence();
    }

    // =========================================================
    // Check supported applications
    // =========================================================

    private static bool HasSupportedAppRunning()
    {
        try
        {
            var processes =
                System.Diagnostics.Process.GetProcesses();

            foreach (var process in processes)
            {
                try
                {
                    if (AppProfiles.ContainsKey(
                        process.ProcessName
                    ))
                    {
                        return true;
                    }
                }
                catch
                {
                    /*
                     * Process có thể terminate
                     * ngay lúc đang đọc.
                     */
                }
                finally
                {
                    process.Dispose();
                }
            }
        }
        catch
        {
            /*
             * Nếu Windows process enumeration lỗi,
             * không crash app.
             */
        }

        return false;
    }

    // =========================================================
    // Active Presence
    // =========================================================

    private void SetPresence()
    {
        if (!_discordClient.IsInitialized)
        {
            SetStatus(
                "Discord chưa kết nối."
            );

            return;
        }

        if (_currentProfile is null)
        {
            SetStatus(
                "Chưa phát hiện ứng dụng được hỗ trợ."
            );

            return;
        }

        // -----------------------------------------------------
        // Session time
        // -----------------------------------------------------

        _sessionStartTime ??=
            DateTime.UtcNow;

        Timestamps? timestamps =
            null;

        if (_elapsedTimeCheckBox.Checked)
        {
            timestamps =
                new Timestamps
                {
                    Start =
                        _sessionStartTime.Value
                };
        }

        // -----------------------------------------------------
        // Presence
        // -----------------------------------------------------

        var presence =
            new RichPresence
            {
                Type =
                    ActivityType.Playing,

                // Example:
                // Working on BasicRotor
                Details =
                    _currentProjectName is not null
                        ? $"Working on {_currentProjectName}"
                        : null,

                // Example:
                // Visual Studio Code
                State =
                    _currentProfile.DisplayName,

                // Same work-session timestamp.
                Timestamps =
                    timestamps,

                Assets =
                    new Assets
                    {
                        LargeImageKey =
                            _currentProfile
                                .LargeImageKey,

                        LargeImageText =
                            _currentProfile
                                .LargeImageText
                    }
            };

        _discordClient.SetPresence(
            presence
        );

        // Explicit timestamp removal.
        if (!_elapsedTimeCheckBox.Checked)
        {
            _discordClient.UpdateClearTime();
        }

        SetStatus(
            $"Presence updated: " +
            $"{_currentProjectName} · " +
            $"{_currentProfile.DisplayName}"
        );
    }

    // =========================================================
    // Idle
    // =========================================================

    private void EnterIdle()
    {
        /*
         * Đã Idle rồi.
         *
         * Không cần gửi presence lại mỗi giây.
         */
        if (_isIdle)
        {
            return;
        }

        _isIdle =
            true;

        // -----------------------------------------------------
        // End current work session
        // -----------------------------------------------------

        _sessionStartTime =
            null;

        _lastPresenceKey =
            null;

        _currentProfile =
            null;

        _currentProjectName =
            null;

        // -----------------------------------------------------
        // GUI
        // -----------------------------------------------------

        _detectedProjectLabel.Text =
            "None";

        _detectedAppLabel.Text =
            "Idle";

        _windowTitleLabel.Text =
            "-";

        // -----------------------------------------------------
        // Discord
        // -----------------------------------------------------

        SetIdlePresence();
    }

    private void SetIdlePresence()
    {
        if (!_discordClient.IsInitialized)
        {
            SetStatus(
                "Discord chưa kết nối."
            );

            return;
        }

        var idlePresence =
            new RichPresence
            {
                Type =
                    ActivityType.Playing,

                Details =
                    "Idle",

                State =
                    "Idling",

                Timestamps =
                    null,

                Assets =
                new Assets
                {
                    LargeImageKey = "idle_v2",
                    LargeImageText = "Idle"
                }
            };

        _discordClient.SetPresence(
            idlePresence
        );

        /*
         * Explicitly remove any timestamp
         * left by the previous work session.
         */
        _discordClient.UpdateClearTime();

        SetStatus(
            "Idle · No supported app running"
        );
    }

    // =========================================================
    // Clear Presence
    // =========================================================

    private void ClearPresence()
    {
        if (!_discordClient.IsInitialized)
        {
            SetStatus(
                "Discord chưa kết nối."
            );

            return;
        }

        _discordClient.ClearPresence();

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
        /*
         * Tray → Exit.
         *
         * Không hỏi lại.
         */
        if (_isExiting)
        {
            return;
        }

        /*
         * Chỉ intercept:
         *
         * X
         * Alt + F4
         */
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
            // -------------------------------------------------
            // Minimize to tray
            // -------------------------------------------------

            case DialogResult.Yes:

                HideToTray();

                break;

            // -------------------------------------------------
            // Exit
            // -------------------------------------------------

            case DialogResult.No:

                _isExiting =
                    true;

                Close();

                break;

            // -------------------------------------------------
            // Cancel
            // -------------------------------------------------

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
        // -----------------------------------------------------
        // Detector
        // -----------------------------------------------------

        _detectionTimer.Stop();

        _detectionTimer.Dispose();

        // -----------------------------------------------------
        // Discord
        // -----------------------------------------------------

        if (_discordClient.IsInitialized)
        {
            _discordClient.ClearPresence();
        }

        _discordClient.Dispose();

        // -----------------------------------------------------
        // Tray
        // -----------------------------------------------------

        _trayIcon.Visible =
            false;

        _trayIcon.Dispose();

        _trayMenu.Dispose();

        // -----------------------------------------------------
        // Base
        // -----------------------------------------------------

        base.OnFormClosed(e);
    }
}