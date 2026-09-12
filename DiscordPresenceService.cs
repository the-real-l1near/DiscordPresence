namespace DiscordPresence;

internal sealed class DiscordPresenceService : IDisposable
{
    // =========================================================
    // Client
    // =========================================================

    private readonly DiscordSocialClient _client;

    // =========================================================
    // Properties
    // =========================================================

    public bool IsInitialized =>
        _client.IsInitialized;

    // =========================================================
    // Constructor
    // =========================================================

    public DiscordPresenceService()
    {
        _client =
            new DiscordSocialClient();
    }

    // =========================================================
    // Lifecycle
    // =========================================================

    public bool Initialize()
    {
        return _client.Initialize();
    }

    public bool Reinitialize()
    {
        return _client.Reinitialize();
    }

    public void Suspend()
    {
        _client.Suspend();
    }

    public void RunCallbacks()
    {
        _client.RunCallbacks();
    }

    // =========================================================
    // Development presence
    // =========================================================

    public bool SetDevelopmentPresence(
        AppPresenceProfile profile,
        string? projectName,
        string? repositoryName,
        DateTime? startTime)
    {
        return _client.SetPresence(
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
    }

    // =========================================================
    // Idle presence
    // =========================================================

    public bool SetIdlePresence()
    {
        return _client.SetPresence(
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
    }

    // =========================================================
    // Clear
    // =========================================================

    public void ClearPresence()
    {
        _client.ClearPresence();

        _client.RunCallbacks();
    }

    // =========================================================
    // Dispose
    // =========================================================

    public void Dispose()
    {
        _client.Dispose();
    }
}