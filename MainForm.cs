using System.Diagnostics;

namespace DiscordPresence;

public sealed class MainForm : Form
{
    // =========================================================
    // Discord
    // =========================================================

    private readonly DiscordSocialClient _discordClient;

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

    private bool _isIdle = true;

    /*
     * Tránh gửi Idle presence mỗi giây.
     *
     * Đồng thời cho phép gửi Idle lần đầu
     * ngay cả khi _isIdle mặc định = true.
     */
    private bool _idlePresenceSent;

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
                    "Discord: initializing...",

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
        // Discord Social SDK
        // =====================================================

        _discordClient =
            new DiscordSocialClient();

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
                _idlePresenceSent =
                    false;

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

            /*
             * Idle không có timer.
             */
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

            /*
             * Social SDK không có UpdateStartTime /
             * UpdateClearTime kiểu RPC cũ.
             *
             * Gửi lại toàn bộ Activity với hoặc
             * không có timestamp.
             */
            SetPresence();
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

                _startWithWindowsCheckBox.Checked =
                    StartupService.IsEnabled();
            }
        };

        // =====================================================
        // Initialize Discord Social SDK
        // =====================================================

        var initialized =
            _discordClient.Initialize();

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
             * Pump Social SDK callbacks.
             */
            _discordClient.RunCallbacks();

            /*
             * Update app/project trước để recovery
             * luôn resend presence mới nhất.
             */
            DetectActiveApp();

            /*
             * Handle Discord startup / restart.
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
        /*
         * Không còn cửa sổ app supported nào:
         *
         * → kết thúc work session
         * → chuyển Idle.
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
         * Foreground là app không supported
         * nhưng editor vẫn đang chạy:
         *
         * → giữ presence gần nhất
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

        var repositoryName =
            GitRepositoryDetector
                .DetectRepositoryName(
                    projectName
                );

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
        // Leaving Idle
        // -----------------------------------------------------

        if (_isIdle)
        {
            _isIdle =
                false;

            _idlePresenceSent =
                false;

            /*
             * Bắt đầu work session mới.
             */
            _sessionStartTime =
                DateTime.UtcNow;
        }

        /*
         * Không reset timer khi chuyển editor.
         *
         * Code → Blender → Unreal → IntelliJ
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

        _currentRepositoryName =
            repositoryName;

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

                    /*
                     * Chỉ tính app đang hoạt động
                     * nếu còn top-level window.
                     *
                     * Tránh helper/background process
                     * giữ app khỏi vào Idle.
                     */
                    if (process.MainWindowHandle !=
                        IntPtr.Zero)
                    {
                        return true;
                    }
                }
                catch
                {
                    /*
                     * Process có thể đóng đúng lúc
                     * đang kiểm tra.
                     */
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
                    /*
                     * Discord là Electron app nên có
                     * nhiều Discord.exe.
                     *
                     * Chỉ coi Discord desktop đã mở
                     * khi có process sở hữu window.
                     */
                    if (process.MainWindowHandle !=
                        IntPtr.Zero)
                    {
                        return true;
                    }
                }
                catch
                {
                    /*
                     * Process có thể đóng đúng lúc
                     * đang kiểm tra.
                     */
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
            /*
             * Ghi nhận Discord đã biến mất.
             *
             * Lần sau Discord xuất hiện sẽ tạo transition:
             *
             * false → true
             */
            _wasDiscordRunning =
                false;

            _discordPresenceRetryTicksRemaining =
                0;

            return;
        }

        // -----------------------------------------------------
        // Discord just started / restarted
        // -----------------------------------------------------

        if (!_wasDiscordRunning)
        {
            _wasDiscordRunning =
                true;

            /*
             * Native Social SDK client cũ có thể đã được
             * tạo trước khi Discord desktop chạy hoặc đã
             * mất connection sau Discord restart.
             *
             * Tạo lại client đúng một lần khi Discord
             * xuất hiện.
             */
            var reinitialized =
                _discordClient.Reinitialize();

            if (!reinitialized)
            {
                SetStatus(
                    "Discord Social SDK: reinitialization failed"
                );

                return;
            }

            /*
             * Discord đã có window nhưng local RPC có thể
             * chưa ready hoàn toàn.
             *
             * Retry tối đa 10 giây.
             */
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
            /*
             * Cho phép resend Idle trong recovery window.
             */
            _idlePresenceSent =
                false;

            SetIdlePresence();
        }
        else
        {
            /*
             * Giữ nguyên work session hiện tại.
             *
             * Discord restart không reset elapsed time.
             */
            SetPresence();
        }

        _discordPresenceRetryTicksRemaining--;
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
        // Work session time
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
            _discordClient.SetPresence(
                name:
                    profile.DisplayName,

                details:
                    projectName is not null
                        ? $"Working on {projectName}"
                        : null,

                state:
                    repositoryName is not null
                        ? $"Repo: {repositoryName}"
                        : "Repo: Not detected",

                largeImage:
                    profile.LargeImageKey,

                largeText:
                    profile.LargeImageText,

                startTime:
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
        /*
         * Đã gửi Idle rồi:
         * không spam Discord mỗi giây.
         */
        if (_isIdle &&
            _idlePresenceSent)
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

        _currentRepositoryName =
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

        var updated =
            _discordClient.SetPresence(
                name:
                    "Idle",

                details:
                    "Touching grass...",

                state:
                    "...allegedly",

                largeImage:
                    "idle_v2",

                largeText:
                    "Idle",

                startTime:
                    null
            );

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
        // -----------------------------------------------------
        // Detector
        // -----------------------------------------------------

        _detectionTimer.Stop();

        _detectionTimer.Dispose();

        // -----------------------------------------------------
        // Discord
        // -----------------------------------------------------

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