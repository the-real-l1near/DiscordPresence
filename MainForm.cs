using DiscordRPC;

namespace DiscordPresence;

public sealed class MainForm : Form
{
    // Discord Application ID
    private const string DiscordApplicationId =
        "1547227043429613668";

    // Supported applications
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
                "unreal",
                "Unreal Engine"
            ),

            ["idea64"] = new(
                "IntelliJ IDEA",
                "intellij",
                "IntelliJ IDEA"
            )
        };

    // Discord
    private readonly DiscordRpcClient _discordClient;

    // Detection
    private readonly System.Windows.Forms.Timer _detectionTimer;

    private string? _lastPresenceKey;

    private AppPresenceProfile? _currentProfile;
    private string? _currentProjectName;

    // UI
    private readonly Label _detectedProjectLabel;
    private readonly Label _detectedAppLabel;
    private readonly Label _windowTitleLabel;

    private readonly CheckBox _elapsedTimeCheckBox;

    private readonly Label _statusLabel;

    public MainForm()
    {
        // Window
        Text = "Custom Discord Presence";

        Width = 540;
        Height = 410;

        StartPosition =
            FormStartPosition.CenterScreen;

        FormBorderStyle =
            FormBorderStyle.FixedSingle;

        MaximizeBox = false;

        // -------------------------
        // Detected project
        // -------------------------

        var projectTitleLabel = new Label
        {
            Text = "Detected project",
            Left = 20,
            Top = 20,
            Width = 180
        };

        _detectedProjectLabel = new Label
        {
            Text = "Waiting for project...",
            Left = 20,
            Top = 45,
            Width = 480,

            Font = new Font(
                Font,
                FontStyle.Bold
            )
        };

        // -------------------------
        // Detected application
        // -------------------------

        var appTitleLabel = new Label
        {
            Text = "Detected application",
            Left = 20,
            Top = 85,
            Width = 180
        };

        _detectedAppLabel = new Label
        {
            Text = "Waiting for supported app...",
            Left = 20,
            Top = 110,
            Width = 480,

            Font = new Font(
                Font,
                FontStyle.Bold
            )
        };

        // -------------------------
        // Window title
        // -------------------------

        var windowTitleTitleLabel = new Label
        {
            Text = "Window title",
            Left = 20,
            Top = 150,
            Width = 180
        };

        _windowTitleLabel = new Label
        {
            Text = "-",
            Left = 20,
            Top = 175,
            Width = 480,
            Height = 40,

            AutoEllipsis = true
        };

        // -------------------------
        // Options
        // -------------------------

        _elapsedTimeCheckBox = new CheckBox
        {
            Text = "Show elapsed time",
            Left = 20,
            Top = 225,
            Width = 180,

            Checked = true
        };

        // -------------------------
        // Manual refresh
        // -------------------------

        var refreshButton =
            new System.Windows.Forms.Button
            {
                Text = "Refresh Presence",
                Left = 20,
                Top = 270,
                Width = 140,
                Height = 35
            };

        // -------------------------
        // Clear
        // -------------------------

        var clearButton =
            new System.Windows.Forms.Button
            {
                Text = "Clear",
                Left = 170,
                Top = 270,
                Width = 100,
                Height = 35
            };

        // -------------------------
        // Status
        // -------------------------

        _statusLabel = new Label
        {
            Text = "Discord: connecting...",
            Left = 20,
            Top = 325,
            Width = 480
        };

        // -------------------------
        // Add controls
        // -------------------------

        Controls.Add(projectTitleLabel);
        Controls.Add(_detectedProjectLabel);

        Controls.Add(appTitleLabel);
        Controls.Add(_detectedAppLabel);

        Controls.Add(windowTitleTitleLabel);
        Controls.Add(_windowTitleLabel);

        Controls.Add(_elapsedTimeCheckBox);

        Controls.Add(refreshButton);
        Controls.Add(clearButton);

        Controls.Add(_statusLabel);

        // -------------------------
        // UI events
        // -------------------------

        refreshButton.Click += (_, _) =>
        {
            SetPresence();
        };

        clearButton.Click += (_, _) =>
        {
            ClearPresence();
        };

        _elapsedTimeCheckBox.CheckedChanged += (_, _) =>
        {
            if (_currentProfile is not null)
            {
                SetPresence();
            }
        };

        // -------------------------
        // Discord RPC
        // -------------------------

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

        // -------------------------
        // Foreground detection
        // -------------------------

        _detectionTimer =
            new System.Windows.Forms.Timer
            {
                Interval = 1000
            };

        _detectionTimer.Tick += (_, _) =>
        {
            DetectActiveApp();
        };

        _detectionTimer.Start();
    }

    // =========================================================
    // Detection
    // =========================================================

    private void DetectActiveApp()
    {
        var activeApp =
            ActiveAppDetector.GetForegroundApp();

        if (activeApp is null)
            return;

        // Unsupported app:
        //
        // Chrome / Discord / Explorer / etc.
        //
        // Keep previous Discord presence.
        if (!AppProfiles.TryGetValue(
            activeApp.ProcessName,
            out var profile))
        {
            return;
        }

        // Show raw window title for debugging.
        _windowTitleLabel.Text =
            string.IsNullOrWhiteSpace(
                activeApp.WindowTitle
            )
                ? "-"
                : activeApp.WindowTitle;

        // Detect project from window title.
        var projectName =
            ProjectNameDetector.Detect(
                activeApp
            );

        projectName ??=
            "Unknown Project";

        /*
         * Important:
         *
         * Don't cache only ProcessName.
         *
         * Example:
         *
         * Code | DiscordPresence
         * Code | BasicRotor
         *
         * Same process but different project.
         */
        var presenceKey =
            $"{activeApp.ProcessName}|{projectName}";

        // Nothing changed.
        if (string.Equals(
            _lastPresenceKey,
            presenceKey,
            StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _lastPresenceKey =
            presenceKey;

        _currentProfile =
            profile;

        _currentProjectName =
            projectName;

        // Update GUI
        _detectedAppLabel.Text =
            profile.DisplayName;

        _detectedProjectLabel.Text =
            projectName;

        // Update Discord
        SetPresence();
    }

    // =========================================================
    // Presence
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

        var presence =
            new RichPresence
            {
                // Fixed activity type
                Type =
                    ActivityType.Playing,

                // Example:
                // Working on DiscordPresence
                Details =
                    _currentProjectName is not null
                        ? $"Working on {_currentProjectName}"
                        : null,

                // Example:
                // Visual Studio Code
                State =
                    _currentProfile.DisplayName,

                // Reset when app/project changes.
                Timestamps =
                    _elapsedTimeCheckBox.Checked
                        ? Timestamps.Now
                        : null,

                // App-specific large image.
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

        SetStatus(
            $"Presence updated: " +
            $"{_currentProjectName} · " +
            $"{_currentProfile.DisplayName}"
        );
    }

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

        if (_discordClient.IsInitialized)
        {
            _discordClient.ClearPresence();
        }

        _discordClient.Dispose();

        base.OnFormClosed(e);
    }
}