MASTER CACHE UPDATE

Copy all 6 .cs files in this folder into the DiscordPresence project:
- Add AppCacheService.cs.
- Replace UiAssets.cs, DiscordDetectableAppService.cs, GitRepositoryDetector.cs,
  GameDetector.cs and GameOverridesForm.cs.
- Keep the current GameOverridesForm.Designer.cs and MainForm files.

Cache folder: %LocalAppData%\DiscordPresence\Cache
- Existing discord-detectable-apps.json is reused (24-hour freshness).
- processes.json stores observed process names and executable paths.
- process-*.png stores executable icons; SVG images stay in memory.
- Repository lookup results share the service's memory cache.

Game overrides and settings are NOT migrated or removed.
Foreground processes are observed in the background, at most once per minute
per process ID/name. Restricted or exited processes are skipped and retried later.
Manage game shows a default icon until the process has been observed successfully.
Cached icons remain available after the target app exits.

Validation: cache tests passed; integrated C# compilation passed against the
project's current sources and installed dependency assemblies. The full app
has not been launched for an end-to-end UI test.
