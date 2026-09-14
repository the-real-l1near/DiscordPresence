using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DiscordPresence;

// Owns cached images. Callers must not dispose images returned by this service.
internal sealed class AppCacheService : IDisposable
{
    public static AppCacheService Shared { get; } = new(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DiscordPresence", "Cache"));

    private readonly string _directory;
    private readonly object _sync = new();
    private readonly object _processSync = new();
    private readonly Dictionary<string, Image> _images = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, (string? Value, DateTime Expires)> _repositories = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DateTime> _observations = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, ProcessEntry> _processes = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;

    internal AppCacheService(string directory)
    {
        _directory = directory;
        try
        {
            var json = ReadText("processes.json");
            if (json != null)
            {
                var entries = JsonSerializer.Deserialize<Dictionary<string, ProcessEntry?>>(json);
                if (entries != null)
                    foreach (var entry in entries)
                        if (entry.Value != null && !string.IsNullOrWhiteSpace(entry.Value.Path))
                            _processes[Normalize(entry.Key)] = entry.Value;
            }
        }
        catch { /* Invalid optional cache is rebuilt from observed processes. */ }
    }

    public string? ReadText(string name)
    {
        try { return File.ReadAllText(CachePath(name)); }
        catch { return null; }
    }

    public bool IsFresh(string name, TimeSpan lifetime)
    {
        try
        {
            string path = CachePath(name);
            var age = DateTime.UtcNow - File.GetLastWriteTimeUtc(path);
            return File.Exists(path) && age >= TimeSpan.Zero && age < lifetime;
        }
        catch { return false; }
    }

    public void WriteText(string name, string text) => WriteBytes(name, Encoding.UTF8.GetBytes(text));

    private string CachePath(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || Path.GetFileName(name) != name || name is "." or "..")
            throw new ArgumentException("Cache keys must be file names.", nameof(name));
        return Path.Combine(_directory, name);
    }

    private void WriteBytes(string name, byte[] bytes)
    {
        string? temporary = null;
        try
        {
            string destination = CachePath(name);
            Directory.CreateDirectory(_directory);
            temporary = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllBytes(temporary, bytes);
            File.Move(temporary, destination, true);
        }
        catch { /* Cache write failure must not stop detection. */ }
        finally
        {
            if (temporary != null)
                try { File.Delete(temporary); } catch { }
        }
    }

    public Image GetImage(string key, Func<Image> factory)
    {
        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_images.TryGetValue(key, out var image)) return image;
            image = factory();
            _images[key] = image;
            return image;
        }
    }

    public bool TryGetRepository(string project, out string? name)
    {
        lock (_sync)
        {
            if (_repositories.TryGetValue(project, out var entry) && entry.Expires > DateTime.UtcNow)
            { name = entry.Value; return true; }
            _repositories.Remove(project);
            name = null;
            return false;
        }
    }

    public void StoreRepository(string project, string? name)
    {
        lock (_sync)
            _repositories[project] = (name, DateTime.UtcNow.AddMinutes(name == null ? 1 : 10));
    }

    // Called from detection; slow executable/icon I/O runs off the UI thread.
    public void ObserveProcess(int processId, string processName)
    {
        string key = Normalize(processName);
        if (key.Length == 0) return;
        lock (_sync)
        {
            if (_disposed) return;
            string observation = processId + ":" + key;
            if (_observations.TryGetValue(observation, out var next) && next > DateTime.UtcNow) return;
            if (_observations.Count >= 256) _observations.Clear();
            _observations[observation] = DateTime.UtcNow.AddMinutes(1);
        }
        _ = Task.Run(() =>
        {
            try
            {
                using var process = Process.GetProcessById(processId);
                if (!string.Equals(Normalize(process.ProcessName), key, StringComparison.OrdinalIgnoreCase)) return;
                string? path = process.MainModule?.FileName;
                if (!string.IsNullOrWhiteSpace(path)) RememberExecutable(key, path);
            }
            catch { /* Process exited or its executable is inaccessible. Retry later. */ }
        });
    }

    internal void RememberExecutable(string name, string path)
    {
        lock (_processSync)
        {
            try
            {
                if (!File.Exists(path)) return;
                path = Path.GetFullPath(path);
                string key = Normalize(name);
                string stamp = path.ToUpperInvariant() + "|" + File.GetLastWriteTimeUtc(path).Ticks;
                string iconFile = "process-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(stamp))) + ".png";
                lock (_sync)
                {
                    if (_disposed) return;
                    if (_processes.TryGetValue(key, out var old) && old.Path == path &&
                        old.IconFile == iconFile && File.Exists(CachePath(iconFile))) return;
                }
                try
                {
                    using var icon = Icon.ExtractAssociatedIcon(path);
                    if (icon != null)
                    {
                        using var bitmap = icon.ToBitmap();
                        using var stream = new MemoryStream();
                        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        WriteBytes(iconFile, stream.ToArray());
                    }
                }
                catch { /* Persist the path even if the executable has no readable icon. */ }
                string json;
                lock (_sync)
                {
                    if (_disposed) return;
                    _processes[key] = new ProcessEntry { Path = path, IconFile = iconFile };
                    json = JsonSerializer.Serialize(_processes);
                }
                WriteText("processes.json", json);
            }
            catch { /* Keep the previous entry when extraction fails. */ }
        }
    }

    public string? GetExecutablePath(string name)
    {
        lock (_sync) return _processes.TryGetValue(Normalize(name), out var entry) ? entry.Path : null;
    }

    public Image? GetProcessIcon(string name, int size)
    {
        if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
        string? file;
        lock (_sync)
            file = _processes.TryGetValue(Normalize(name), out var entry) ? entry.IconFile : null;
        if (string.IsNullOrEmpty(file)) return null;
        try
        {
            return GetImage("process:" + file + ":" + size, () =>
            {
                using var stream = new MemoryStream(File.ReadAllBytes(CachePath(file)));
                using var original = Image.FromStream(stream);
                return new Bitmap(original, new Size(size, size));
            });
        }
        catch { return null; }
    }

    private static string Normalize(string value)
    {
        string name = Path.GetFileName(value.Trim());
        return (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? name[..^4] : name).ToLowerInvariant();
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed) return;
            _disposed = true;
            foreach (var image in _images.Values) image.Dispose();
            _images.Clear();
            _repositories.Clear();
            _observations.Clear();
        }
    }

    internal sealed class ProcessEntry
    {
        public string Path { get; set; } = "";
        public string IconFile { get; set; } = "";
    }
}
