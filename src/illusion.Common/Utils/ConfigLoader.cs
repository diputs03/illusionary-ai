using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace illusion.Common.Utils;

/// <summary>
/// Global configuration loader for system/UPP/GGTP settings. The loader searches
/// upward from the application directory so tests, CLI runs, and published builds
/// all resolve the same repository-level config folder.
/// </summary>
public static class ConfigLoader
{
    private static IConfiguration? _configuration;
    private static string? _configPath;
    private static readonly object _lock = new();
    public static IConfiguration Configuration
    {
        get
        {
            if (_configuration is null)
            {
                LoadConfiguration();
            }
            return _configuration ?? throw new InvalidOperationException("Configuration has not been loaded.");
        }
    }
    public static void Reset()
    {
        lock (_lock)
        {
            if (_configuration is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _configuration = null;
            _configPath = null;
        }
    }

    public static void LoadConfiguration(bool forced = false)
    {
        lock (_lock)
        {
            if (forced || _configuration is null)
            {
                if (_configuration is IDisposable oldDisposable)
                    oldDisposable.Dispose();
                _configPath ??= ResolveConfigPath(AppContext.BaseDirectory);
                _configuration = new ConfigurationBuilder()
                    .SetBasePath(_configPath)
                    .AddYamlFile("default.yaml", optional: false, reloadOnChange: true)
                    .AddYamlFile("config.yaml", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables(prefix: "ILLUSION_")
                    .Build();
            }
        }
    }

    private static string ResolveConfigPath(string startDirectory)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(startDirectory);

        var current = new DirectoryInfo(Path.GetFullPath(startDirectory));
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "config");
            if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "default.yaml")))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException($"Config directory not found while searching from: {startDirectory}.");
    }

    public static T GetValue<T>(string key) where T : ISpanParsable<T>
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(key);

        string? str;
        lock (_lock)
        {
            str = (_configuration ?? throw new InvalidOperationException("Configuration has not been loaded."))[key];
        }
        if (str is null)
            throw new KeyNotFoundException($"Configuration key '{key}' was not found.");
        if (T.TryParse(str, CultureInfo.InvariantCulture, out T def) is false)
            throw new FormatException($"Value '{str}' for key '{key}' cannot be parsed to type {typeof(T).Name}.");
        return def;
    }
}
