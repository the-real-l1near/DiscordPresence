using System.Diagnostics;

namespace DiscordPresence;

internal sealed class PresenceController : IDisposable
{
    // =========================================================
    // Services
    // =========================================================

    private readonly DiscordPresenceService
        _discordPresence;

    private readonly WorkSessionManager
        _workSession;

    private readonly SupportedAppRegistry
        _supportedApps;

    private readonly DiscordDetectableAppService
        _detectableApps;

    private readonly GameOverrideStore
        _gameOverrides;

    private readonly GameDetector
        _gameDetector;

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

    private AppPresenceProfile?
        _currentProfile;

    private string?
        _currentProjectName;

    private string?
        _currentRepositoryName;

    // =========================================================
    // Game override
    // =========================================================

    private bool
        _isGameOverrideActive;

    private string?
        _activeGameProcessName;

    // =========================================================
    // Suspected games
    // =========================================================

    private static readonly TimeSpan
        SuspectDeferDuration =
            TimeSpan.FromMinutes(30);

    private readonly Dictionary<string, DateTime>
        _deferredSuspects =
            new(StringComparer.OrdinalIgnoreCase);

    public SuspectedGame? PendingSuspectedGame
    {
        get;
        private set;
    }

    public bool HasPendingSuspectedGame =>
        PendingSuspectedGame is not null;

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

        _supportedApps =
            new SupportedAppRegistry();

        _detectableApps =
            new DiscordDetectableAppService();

        _gameOverrides =
            new GameOverrideStore();

        _gameDetector =
            new GameDetector(
                _detectableApps,
                _gameOverrides
            );

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

        RestoreIfActiveGameExited();

        HandleGameDetection();

        DetectActiveApp();

        HandlePendingIdlePresence();

        HandleDiscordPresenceRecovery();
    }

    // =========================================================
    // Suspected game decisions
    // =========================================================

    public void ConfirmSuspectedGame()
    {
        var suspect =
            PendingSuspectedGame;

        if (suspect is null)
        {
            return;
        }

        _gameOverrides.Include(
            suspect.ProcessName
        );

        _deferredSuspects.Remove(
            suspect.ProcessName
        );

        PendingSuspectedGame =
            null;

        SetStatus(
            $"Game confirmed: {suspect.ProcessName}.exe"
        );
    }

    public void RejectSuspectedGame()
    {
        var suspect =
            PendingSuspectedGame;

        if (suspect is null)
        {
            return;
        }

        _gameOverrides.Exclude(
            suspect.ProcessName
        );

        _deferredSuspects.Remove(
            suspect.ProcessName
        );

        PendingSuspectedGame =
            null;

        SetStatus(
            $"Ignored as game: {suspect.ProcessName}.exe"
        );
    }

    public void DeferSuspectedGame()
    {
        var suspect =
            PendingSuspectedGame;

        if (suspect is null)
        {
            return;
        }

        _deferredSuspects[
            suspect.ProcessName
        ] =
            DateTime.UtcNow +
            SuspectDeferDuration;

        PendingSuspectedGame =
            null;

        SetStatus(
            $"Game confirmation deferred: {suspect.ProcessName}.exe"
        );
    }

    // =========================================================
    // Game override management
    // =========================================================

    public IReadOnlyList<string> GetIncludedGameOverrides()
    {
        return _gameOverrides
            .GetIncludedProcesses();
    }

    public IReadOnlyList<string> GetExcludedGameOverrides()
    {
        return _gameOverrides
            .GetExcludedProcesses();
    }

    public void ConfirmGameOverride(
        string processName)
    {
        _gameOverrides.Include(
            processName
        );
    }

    public void RejectGameOverride(
        string processName)
    {
        _gameOverrides.Exclude(
            processName
        );
    }

    public void RemoveIncludedGameOverride(
        string processName)
    {
        _gameOverrides.RemoveInclude(
            processName
        );
    }

    public void RemoveExcludedGameOverride(
        string processName)
    {
        _gameOverrides.RemoveExclude(
            processName
        );
    }

    public void ClearGameOverrides()
    {
        _gameOverrides.ClearAll();
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

        CancelPendingIdlePresence();

        _discordPresence.ClearPresence();

        SetStatus(
            "Presence cleared."
        );
    }

    // =========================================================
    // Game detection
    // =========================================================

    private void HandleGameDetection()
    {
        var result =
            _gameDetector.DetectForeground();

        switch (result.Kind)
        {
            case GameDetectionKind.Game:
                HandleConfirmedGame(
                    result
                );
                break;

            case GameDetectionKind.Suspected:
                HandleSuspectedGame(
                    result
                );
                break;

            case GameDetectionKind.NotGame:
            default:
                if (
                    !string.IsNullOrWhiteSpace(
                        result.ProcessName
                    ) &&
                    _supportedApps.IsSupportedProcess(
                        result.ProcessName
                    )
                )
                {
                    RestoreFromGameOverrideIfNeeded();
                }
                break;
        }
    }

    private void HandleConfirmedGame(
        GameDetectionResult result)
    {
        if (
            PendingSuspectedGame is not null &&
            string.Equals(
                PendingSuspectedGame.ProcessName,
                result.ProcessName,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            PendingSuspectedGame =
                null;
        }

        if (!string.IsNullOrWhiteSpace(
            result.ProcessName
        ))
        {
            _activeGameProcessName =
                result.ProcessName;
        }

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
            result.ProcessName is not null
                ? $"Game detected: {result.DisplayName}"
                : "Game detected · Presence suspended"
        );
    }

    private void HandleSuspectedGame(
        GameDetectionResult result)
    {
        var processName =
            result.ProcessName;

        if (string.IsNullOrWhiteSpace(
            processName
        ))
        {
            return;
        }

        // -----------------------------------------------------
        // Deferred
        // -----------------------------------------------------

        if (_deferredSuspects.TryGetValue(
            processName,
            out var deferredUntil
        ))
        {
            if (DateTime.UtcNow <
                deferredUntil)
            {
                return;
            }

            _deferredSuspects.Remove(
                processName
            );
        }

        // -----------------------------------------------------
        // Already pending
        // -----------------------------------------------------

        if (
            PendingSuspectedGame is not null &&
            string.Equals(
                PendingSuspectedGame.ProcessName,
                processName,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return;
        }

        // -----------------------------------------------------
        // New suspect
        // -----------------------------------------------------

        PendingSuspectedGame =
            new SuspectedGame(
                Guid.NewGuid(),
                processName,
                result.DisplayName,
                result.WindowTitle
            );

        SetStatus(
            $"Suspected game: {processName}.exe"
        );
    }

    private void RestoreIfActiveGameExited()
    {
        if (!_isGameOverrideActive)
        {
            return;
        }

        var processName =
            _activeGameProcessName;

        if (string.IsNullOrWhiteSpace(
            processName
        ))
        {
            return;
        }

        Process[] processes;

        try
        {
            processes =
                Process.GetProcessesByName(
                    processName
                );
        }
        catch
        {
            return;
        }

        try
        {
            if (processes.Length > 0)
            {
                return;
            }
        }
        finally
        {
            foreach (var process in processes)
            {
                process.Dispose();
            }
        }

        RestoreFromGameOverrideIfNeeded();
    }

    private void RestoreFromGameOverrideIfNeeded()
    {
        if (!_isGameOverrideActive)
        {
            return;
        }

        _isGameOverrideActive =
            false;

        _activeGameProcessName =
            null;

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
    // App detection
    // =========================================================

    private void DetectActiveApp()
    {
        if (!_supportedApps.HasSupportedAppRunning())
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

        if (_supportedApps.IsSupportedProcess(
            activeApp.ProcessName
        ))
        {
            CancelPendingIdlePresence();
        }

        if (!_supportedApps.TryGetProfile(
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
    // Idle
    // =========================================================

    private void EnterIdle()
    {
        if (_idlePresencePending)
        {
            return;
        }

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

        if (!wasActive)
        {
            SetIdlePresence();

            return;
        }

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
