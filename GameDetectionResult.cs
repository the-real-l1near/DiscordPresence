namespace DiscordPresence;

internal enum GameDetectionKind
{
    NotGame,
    Suspected,
    Game
}

internal sealed record GameDetectionResult(
    GameDetectionKind Kind,
    string? ProcessName = null,
    string? WindowTitle = null)
{
    public string DisplayName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(
                WindowTitle
            ))
            {
                return WindowTitle;
            }

            if (!string.IsNullOrWhiteSpace(
                ProcessName
            ))
            {
                return $"{ProcessName}.exe";
            }

            return "Unknown application";
        }
    }
}

internal sealed record SuspectedGame(
    Guid Id,
    string ProcessName,
    string DisplayName,
    string? WindowTitle
);