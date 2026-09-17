namespace DiscordPresence;

internal sealed record AppMatchRule(
    string ProcessName,
    string? ProductName,
    string? OriginalFilename
);

internal sealed record AppDatabaseEntry(
    string Id,
    AppPresenceProfile Profile,
    IReadOnlyList<AppMatchRule> MatchRules
);
