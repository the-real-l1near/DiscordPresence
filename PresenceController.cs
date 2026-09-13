using System.Diagnostics;

namespace DiscordPresence;

internal sealed class PresenceController : IDisposable
{
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
    // Services
    // =========================================================

    private readonly DiscordPresenceService
        _discordPresence;

    private readonly WorkSessionManager
        _workSession;

    // =========================================================
    // Discord recovery
    // =========================================================

    private bool _wasDiscordRunning;

    private int _discordPresenceRetryTicksRemaining;

    private const int DiscordPresenceRetryTicks =
        10;

    // =========================================================
    // Idle transition
    // =========================================================

    private bool _idlePresencePending;

    private int _idlePresenceDelayTicks;

    // =========================================================
    // Current presence
    // =========================================================

    private AppPresenceProfile? _currentProfile;

    private string? _currentProjectName;

    private string? _currentRepositoryName;

    // =========================================================
    // Game override
    // =========================================================

    private bool _isGameOverrideActive;

    // =========================================================
    // UI state
    // =========================================================

    public string DetectedProjectName { get; private set; } =
        "None";

    public string DetectedApplicationName { get; private set; } =
        "Idle";

    public string WindowTitle { get; private set; } =
        "-";

    public string StatusText { get; private set; } =
        "Discord: initializing...";

    // =========================================================
    // Constructor
    // =========================================================

    public PresenceController()
    {
        _workSession =
            new WorkSessionManager();

        _discordPresence =
            new DiscordPresenceService();
    }

    // =========================================================
    // Initialize
    // =========================================================

    public void Initialize()
    {
        var initialized =
            _discordPresence.Initialize();

        if (!initialized)
        {
            SetStatus(
                "Discord Social SDK: initialization failed"
            );

            return;
        }

        _wasDiscordRunning =
            IsDiscordRunning();

        SetStatus(
            "Discord Social SDK: initialized"
        );

        /*
         * Initial Idle không cần clear trước vì chưa có
         * coding presence từ instance hiện tại.
         */
        _workSession.EnterIdle();

        SetIdleUi();

        SetIdlePresence();
    }

    // =========================================================
    // Tick
    // =========================================================

    public void Tick()
    {
        _discordPresence.RunCallbacks();

        HandleGameOverride();

        DetectActiveApp();

        HandlePendingIdlePresence();

        HandleDiscordPresenceRecovery();
    }

    // =========================================================
    // Refresh
    // =========================================================

    public void RefreshPresence()
    {
        if (_isGameOverrideActive)
        {
            SetStatus(
                "Game detected · Presence suspended"
            );

            return;
        }

        if (_idlePresencePending)
        {
            return;
        }

        if (_workSession.IsIdle)
        {
            _workSession.InvalidateIdlePresence();

            SetIdlePresence();

            return;
        }

        SetPresence();
    }

    // =========================================================
    // Clear
    // =========================================================

    public void ClearPresence()
    {
        if (!_discordPresence.IsInitialized)
        {
            SetStatus(
                "Discord chưa kết nối."
            );

            return;
        }

        /*
         * Manual clear không được tự publish Idle
         * ở tick kế tiếp.
         */
        CancelPendingIdlePresence();

        _discordPresence.ClearPresence();

        SetStatus(
            "Presence cleared."
        );
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
         * Một supported app xuất hiện trong lúc đang chờ
         * publish Idle.
         *
         * Hủy Idle transition vì active state có priority.
         */
        if (AppProfiles.ContainsKey(
            activeApp.ProcessName
        ))
        {
            CancelPendingIdlePresence();
        }

        /*
         * Foreground không phải supported app:
         * giữ active presence gần nhất.
         */
        if (!AppProfiles.TryGetValue(
            activeApp.ProcessName,
            out var profile))
        {
            return;
        }

        WindowTitle =
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

        var presenceChanged =
            _workSession.UpdateActivePresence(
                presenceKey
            );

        if (!presenceChanged)
        {
            return;
        }

        _currentProfile =
            profile;

        _currentProjectName =
            projectName;

        _currentRepositoryName =
            repositoryName;

        DetectedApplicationName =
            profile.DisplayName;

        DetectedProjectName =
            projectName;

        if (_isGameOverrideActive)
        {
            return;
        }

        SetPresence();
    }

    // =========================================================
    // Supported applications
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
    // Development presence
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
            return;
        }

        /*
         * Coding presence luôn có elapsed timestamp.
         */
        _workSession.EnsureStarted();

        var startTime =
            _workSession.StartTime;

        if (!startTime.HasValue)
        {
            return;
        }

        var updated =
            _discordPresence
                .SetDevelopmentPresence(
                    profile,
                    _currentProjectName,
                    _currentRepositoryName,
                    startTime.Value
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
            $"{_currentProjectName} · " +
            $"{profile.DisplayName}"
        );
    }

    // =========================================================
    // Idle transition
    // =========================================================

    private void EnterIdle()
    {
        /*
         * Đang chờ Idle rồi thì không clear lại mỗi tick.
         */
        if (_idlePresencePending)
        {
            return;
        }

        /*
         * Đã Idle và Idle presence cũng đã gửi rồi.
         */
        if (!_workSession.ShouldEnterIdle())
        {
            return;
        }

        var wasActive =
            !_workSession.IsIdle;

        _workSession.EnterIdle();

        _currentProfile =
            null;

        _currentProjectName =
            null;

        _currentRepositoryName =
            null;

        SetIdleUi();

        if (_isGameOverrideActive)
        {
            return;
        }

        /*
         * Startup / đã Idle sẵn:
         * không có active timestamp cần clear.
         */
        if (!wasActive)
        {
            SetIdlePresence();

            return;
        }

        /*
         * Active -> Idle:
         *
         * Coding activity trước có timestamp.
         * Clear hẳn trước để Discord bỏ activity cũ,
         * sau đó tick kế mới publish Idle.
         */
        _discordPresence.ClearPresence();

        _idlePresencePending =
            true;

        _idlePresenceDelayTicks =
            1;

        _discordPresenceRetryTicksRemaining =
            0;

        SetStatus(
            "Idle · Updating presence..."
        );
    }

    private void HandlePendingIdlePresence()
    {
        if (!_idlePresencePending)
        {
            return;
        }

        if (_isGameOverrideActive)
        {
            return;
        }

        if (!_discordPresence.IsInitialized)
        {
            return;
        }

        /*
         * Nếu một supported app đã quay lại,
         * Idle transition không còn hợp lệ.
         */
        if (!_workSession.IsIdle)
        {
            CancelPendingIdlePresence();

            return;
        }

        if (_idlePresenceDelayTicks > 0)
        {
            _idlePresenceDelayTicks--;

            return;
        }

        _idlePresencePending =
            false;

        _idlePresenceDelayTicks =
            0;

        SetIdlePresence();
    }

    private void CancelPendingIdlePresence()
    {
        _idlePresencePending =
            false;

        _idlePresenceDelayTicks =
            0;
    }

    // =========================================================
    // Idle presence
    // =========================================================

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

        _workSession.MarkIdlePresenceSent();

        SetStatus(
            "Idle · No supported app running"
        );
    }

    private void SetIdleUi()
    {
        DetectedProjectName =
            "None";

        DetectedApplicationName =
            "Idle";

        WindowTitle =
            "-";
    }

    // =========================================================
    // Game override
    // =========================================================

    private void HandleGameOverride()
    {
        var gameRunning =
            GameDetector.IsForegroundGame();

        // -----------------------------------------------------
        // Enter game
        // -----------------------------------------------------

        if (gameRunning)
        {
            if (_isGameOverrideActive)
            {
                return;
            }

            _isGameOverrideActive =
                true;

            CancelPendingIdlePresence();

            _discordPresenceRetryTicksRemaining =
                0;

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
        // Still normal
        // -----------------------------------------------------

        if (!_isGameOverrideActive)
        {
            return;
        }

        // -----------------------------------------------------
        // Leave game
        // -----------------------------------------------------

        _isGameOverrideActive =
            false;

        var reinitialized =
            _discordPresence.Reinitialize();

        if (!reinitialized)
        {
            SetStatus(
                "Discord Social SDK: reinitialization failed"
            );

            return;
        }

        _wasDiscordRunning =
            IsDiscordRunning();

        _discordPresenceRetryTicksRemaining =
            _wasDiscordRunning
                ? DiscordPresenceRetryTicks
                : 0;

        if (_workSession.IsIdle)
        {
            _workSession.InvalidateIdlePresence();

            SetIdlePresence();
        }
        else
        {
            SetPresence();
        }
    }

    // =========================================================
    // Discord recovery
    // =========================================================

    private void HandleDiscordPresenceRecovery()
    {
        var discordRunning =
            IsDiscordRunning();

        if (!discordRunning)
        {
            _wasDiscordRunning =
                false;

            _discordPresenceRetryTicksRemaining =
                0;

            return;
        }

        if (_isGameOverrideActive)
        {
            return;
        }

        if (_idlePresencePending)
        {
            return;
        }

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

        if (_discordPresenceRetryTicksRemaining <=
            0)
        {
            return;
        }

        if (_workSession.IsIdle)
        {
            _workSession.InvalidateIdlePresence();

            SetIdlePresence();
        }
        else
        {
            SetPresence();
        }

        _discordPresenceRetryTicksRemaining--;
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
    // Status
    // =========================================================

    private void SetStatus(
        string text)
    {
        StatusText =
            text;
    }

    // =========================================================
    // Dispose
    // =========================================================

    public void Dispose()
    {
        CancelPendingIdlePresence();

        _discordPresence.Dispose();
    }
}