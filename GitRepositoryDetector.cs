using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DiscordPresence;

public static class GitRepositoryDetector
{
    private static readonly object CacheLock =
        new();

    private static readonly Dictionary<string, string?>
        RepoNameCache =
            new(StringComparer.OrdinalIgnoreCase);

    private static readonly string[]
        SearchRoots =
        [
            @"D:\Git",
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.UserProfile
                ),
                "source",
                "repos"
            ),
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.UserProfile
                ),
                "Git"
            )
        ];

    public static string? DetectRepositoryName(
        string projectName)
    {
        if (string.IsNullOrWhiteSpace(projectName))
        {
            return null;
        }

        lock (CacheLock)
        {
            if (RepoNameCache.TryGetValue(
                projectName,
                out var cached))
            {
                return cached;
            }
        }

        var repoPath =
            FindRepository(
                projectName
            );

        if (repoPath is null)
        {
            Cache(
                projectName,
                null
            );

            return null;
        }

        var remote =
            RunGit(
                repoPath,
                "remote get-url origin"
            );

        /*
         * Nếu repo không có origin,
         * dùng tên folder repo.
         */
        var repoName =
            ParseRepositoryName(
                remote
            );

        repoName ??=
            Path.GetFileName(
                repoPath.TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar
                )
            );

        Cache(
            projectName,
            repoName
        );

        return repoName;
    }

    private static string? FindRepository(
        string projectName)
    {
        foreach (var searchRoot in SearchRoots)
        {
            if (!Directory.Exists(searchRoot))
            {
                continue;
            }

            /*
             * Case phổ biến:
             *
             * D:\Git\DiscordPresence
             * D:\Git\BasicRotor
             */
            var directPath =
                Path.Combine(
                    searchRoot,
                    projectName
                );

            if (IsGitRepository(
                directPath))
            {
                return directPath;
            }

            /*
             * Nếu folder local không trùng chính xác
             * project name thì tìm các repo con.
             *
             * Chỉ scan depth thấp để tránh quét cả ổ.
             */
            try
            {
                foreach (var directory in
                    Directory.EnumerateDirectories(
                        searchRoot))
                {
                    if (!IsGitRepository(
                        directory))
                    {
                        continue;
                    }

                    var directoryName =
                        Path.GetFileName(
                            directory
                        );

                    if (string.Equals(
                        directoryName,
                        projectName,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return directory;
                    }

                    /*
                     * Repo folder có thể khác project,
                     * nên thử đọc remote name.
                     */
                    var remote =
                        RunGit(
                            directory,
                            "remote get-url origin"
                        );

                    var repositoryName =
                        ParseRepositoryName(
                            remote
                        );

                    if (string.Equals(
                        repositoryName,
                        projectName,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return directory;
                    }
                }
            }
            catch
            {
                /*
                 * Folder inaccessible / disappeared.
                 */
            }
        }

        return null;
    }

    private static bool IsGitRepository(
        string path)
    {
        if (!Directory.Exists(path))
        {
            return false;
        }

        /*
         * Normal repository.
         */
        if (Directory.Exists(
            Path.Combine(
                path,
                ".git"
            )))
        {
            return true;
        }

        /*
         * Worktree/submodule có thể có .git
         * dạng file thay vì directory.
         */
        return File.Exists(
            Path.Combine(
                path,
                ".git"
            )
        );
    }

    private static string? RunGit(
        string workingDirectory,
        string arguments)
    {
        try
        {
            var startInfo =
                new ProcessStartInfo
                {
                    FileName =
                        "git",

                    Arguments =
                        $"-C \"{workingDirectory}\" {arguments}",

                    RedirectStandardOutput =
                        true,

                    RedirectStandardError =
                        true,

                    UseShellExecute =
                        false,

                    CreateNoWindow =
                        true
                };

            using var process =
                Process.Start(
                    startInfo
                );

            if (process is null)
            {
                return null;
            }

            var output =
                process.StandardOutput
                    .ReadToEnd();

            process.WaitForExit(
                2000
            );

            if (process.ExitCode != 0)
            {
                return null;
            }

            return string.IsNullOrWhiteSpace(
                output)
                ? null
                : output.Trim();
        }
        catch
        {
            return null;
        }
    }

    private static string? ParseRepositoryName(
        string? remote)
    {
        if (string.IsNullOrWhiteSpace(remote))
        {
            return null;
        }

        /*
         * HTTPS:
         *
         * https://github.com/the-real-l1near/DiscordPresence.git
         *
         * SSH:
         *
         * git@github.com:the-real-l1near/DiscordPresence.git
         */
        var match =
            Regex.Match(
                remote,
                @"([^/:]+?)(?:\.git)?$",
                RegexOptions.IgnoreCase
            );

        if (!match.Success)
        {
            return null;
        }

        var value =
            match.Groups[1]
                .Value
                .Trim();

        if (value.EndsWith(
            ".git",
            StringComparison.OrdinalIgnoreCase))
        {
            value =
                value[..^4];
        }

        return string.IsNullOrWhiteSpace(
            value)
                ? null
                : value;
    }

    private static void Cache(
        string projectName,
        string? repoName)
    {
        lock (CacheLock)
        {
            RepoNameCache[projectName] =
                repoName;
        }
    }
}