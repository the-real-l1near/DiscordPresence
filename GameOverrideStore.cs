using System.Text.Json;

namespace DiscordPresence;

internal sealed class GameOverrideStore
{
    // =========================================================
    // Storage
    // =========================================================

    private readonly string
        _filePath;

    private readonly object
        _sync =
            new();

    // =========================================================
    // Overrides
    // =========================================================

    private readonly HashSet<string>
        _includedProcesses =
            new(StringComparer.OrdinalIgnoreCase);

    private readonly HashSet<string>
        _excludedProcesses =
            new(StringComparer.OrdinalIgnoreCase);

    // =========================================================
    // Constructor
    // =========================================================

    public GameOverrideStore()
    {
        var directory =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData
                ),
                "DiscordPresence"
            );

        _filePath =
            Path.Combine(
                directory,
                "game-overrides.json"
            );

        Load();
    }

    // =========================================================
    // Lookup
    // =========================================================

    public bool IsIncluded(
        string processName)
    {
        var normalized =
            NormalizeProcessName(
                processName
            );

        lock (_sync)
        {
            return _includedProcesses.Contains(
                normalized
            );
        }
    }

    public bool IsExcluded(
        string processName)
    {
        var normalized =
            NormalizeProcessName(
                processName
            );

        lock (_sync)
        {
            return _excludedProcesses.Contains(
                normalized
            );
        }
    }

    // =========================================================
    // Include
    // =========================================================

    public void Include(
        string processName)
    {
        var normalized =
            NormalizeProcessName(
                processName
            );

        if (string.IsNullOrWhiteSpace(
            normalized
        ))
        {
            return;
        }

        lock (_sync)
        {
            _excludedProcesses.Remove(
                normalized
            );

            _includedProcesses.Add(
                normalized
            );

            SaveLocked();
        }
    }

    // =========================================================
    // Exclude
    // =========================================================

    public void Exclude(
        string processName)
    {
        var normalized =
            NormalizeProcessName(
                processName
            );

        if (string.IsNullOrWhiteSpace(
            normalized
        ))
        {
            return;
        }

        lock (_sync)
        {
            _includedProcesses.Remove(
                normalized
            );

            _excludedProcesses.Add(
                normalized
            );

            SaveLocked();
        }
    }

    // =========================================================
    // Load
    // =========================================================

    private void Load()
    {
        if (!File.Exists(
            _filePath
        ))
        {
            return;
        }

        try
        {
            var json =
                File.ReadAllText(
                    _filePath
                );

            var data =
                JsonSerializer.Deserialize<GameOverrideData>(
                    json
                );

            if (data is null)
            {
                return;
            }

            if (data.GameIncludes is not null)
            {
                foreach (var processName in
                    data.GameIncludes)
                {
                    var normalized =
                        NormalizeProcessName(
                            processName
                        );

                    if (!string.IsNullOrWhiteSpace(
                        normalized
                    ))
                    {
                        _includedProcesses.Add(
                            normalized
                        );
                    }
                }
            }

            if (data.GameExcludes is not null)
            {
                foreach (var processName in
                    data.GameExcludes)
                {
                    var normalized =
                        NormalizeProcessName(
                            processName
                        );

                    if (!string.IsNullOrWhiteSpace(
                        normalized
                    ))
                    {
                        _excludedProcesses.Add(
                            normalized
                        );
                    }
                }
            }
        }
        catch
        {
            /*
             * Corrupt / incompatible file:
             * ignore và bắt đầu với empty overrides.
             */
        }
    }

    // =========================================================
    // Save
    // =========================================================

    private void SaveLocked()
    {
        try
        {
            var directory =
                Path.GetDirectoryName(
                    _filePath
                );

            if (!string.IsNullOrWhiteSpace(
                directory
            ))
            {
                Directory.CreateDirectory(
                    directory
                );
            }

            var data =
                new GameOverrideData
                {
                    GameIncludes =
                        _includedProcesses
                            .OrderBy(
                                value => value,
                                StringComparer.OrdinalIgnoreCase
                            )
                            .ToArray(),

                    GameExcludes =
                        _excludedProcesses
                            .OrderBy(
                                value => value,
                                StringComparer.OrdinalIgnoreCase
                            )
                            .ToArray()
                };

            var json =
                JsonSerializer.Serialize(
                    data,
                    new JsonSerializerOptions
                    {
                        WriteIndented =
                            true
                    }
                );

            File.WriteAllText(
                _filePath,
                json
            );
        }
        catch
        {
            /*
             * Override vẫn tồn tại trong RAM nếu
             * persistent storage fail.
             */
        }
    }

    // =========================================================
    // Normalize
    // =========================================================

    private static string NormalizeProcessName(
        string processName)
    {
        var normalized =
            processName.Trim();

        if (normalized.EndsWith(
            ".exe",
            StringComparison.OrdinalIgnoreCase
        ))
        {
            normalized =
                normalized[..^4];
        }

        return normalized;
    }

    // =========================================================
    // DTO
    // =========================================================

    private sealed class GameOverrideData
    {
        public string[] GameIncludes { get; set; } =
            [];

        public string[] GameExcludes { get; set; } =
            [];
    }
}