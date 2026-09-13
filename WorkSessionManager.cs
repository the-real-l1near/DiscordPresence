namespace DiscordPresence;

internal sealed class WorkSessionManager
{
    // =========================================================
    // State
    // =========================================================

    private DateTime? _startTime;

    private string? _lastPresenceKey;

    private bool _isIdle =
        true;

    private bool _idlePresenceSent;

    // =========================================================
    // Properties
    // =========================================================

    public DateTime? StartTime =>
        _startTime;

    public bool IsIdle =>
        _isIdle;

    public bool IdlePresenceSent =>
        _idlePresenceSent;

    // =========================================================
    // Active session
    // =========================================================

    public bool UpdateActivePresence(
        string presenceKey)
    {
        if (string.Equals(
            _lastPresenceKey,
            presenceKey,
            StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (_isIdle)
        {
            _isIdle =
                false;

            _idlePresenceSent =
                false;

            _startTime =
                DateTime.UtcNow;
        }

        _lastPresenceKey =
            presenceKey;

        return true;
    }

    // =========================================================
    // Ensure session
    // =========================================================

    public void EnsureStarted()
    {
        _startTime ??=
            DateTime.UtcNow;
    }

    // =========================================================
    // Idle
    // =========================================================

    public bool ShouldEnterIdle()
    {
        return !(
            _isIdle &&
            _idlePresenceSent
        );
    }

    public void EnterIdle()
    {
        _isIdle =
            true;

        _startTime =
            null;

        _lastPresenceKey =
            null;
    }

    public void MarkIdlePresenceSent()
    {
        _idlePresenceSent =
            true;
    }

    public void InvalidateIdlePresence()
    {
        _idlePresenceSent =
            false;
    }
}