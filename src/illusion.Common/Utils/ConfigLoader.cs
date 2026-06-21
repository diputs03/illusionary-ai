using Microsoft.Extensions.Configuration;
using illusion.Common.Constants;

namespace illusion.Common.Utils;

/// <summary>
/// Global configuration loader for system/UPP/GGTP settings. The loader searches
/// upward from the application directory so tests, CLI runs, and published builds
/// all resolve the same repository-level config folder.
/// </summary>
public static class ConfigLoader
{
    private static IConfiguration? _configuration;
    private static readonly object _lock = new();

    public static IConfiguration Configuration
    {
        get
        {
            if (_configuration is null)
            {
                lock (_lock)
                {
                    _configuration ??= LoadConfiguration();
                }
            }

            return _configuration;
        }
    }

    public static void ResetForTests()
    {
        lock (_lock)
        {
            _configuration = null;
        }
    }

    private static IConfiguration LoadConfiguration()
    {
        var configPath = ResolveConfigPath(AppContext.BaseDirectory);

        return new ConfigurationBuilder()
            .SetBasePath(configPath)
            .AddYamlFile("system.yaml", optional: false, reloadOnChange: true)
            .AddYamlFile("ggtp.yaml", optional: false, reloadOnChange: true)
            .AddYamlFile("upp.yaml", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables(prefix: "ILLUSION_")
            .Build();
    }

    public static string ResolveConfigPath(string startDirectory)
    {
        if (string.IsNullOrWhiteSpace(startDirectory))
            throw new ArgumentException("start directory cannot be empty", nameof(startDirectory));

        var current = new DirectoryInfo(Path.GetFullPath(startDirectory));
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "config");
            if (Directory.Exists(candidate)
                && File.Exists(Path.Combine(candidate, "system.yaml"))
                && File.Exists(Path.Combine(candidate, "ggtp.yaml"))
                && File.Exists(Path.Combine(candidate, "upp.yaml")))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException($"config directory not found while searching from: {startDirectory}");
    }

    public static string GetSystemName() => Configuration["system:name"] ?? SystemConstants.ProjectName;
    public static string GetLogLevel() => Configuration["system:log_level"] ?? SystemConstants.DefaultLogLevel;
    public static string GetGGTPServerUrl() => Configuration["ggtp:server_url"] ?? GGTPConstants.DefaultServerUrl;
    public static bool IsUPPEncryptionEnabled() => bool.TryParse(Configuration["upp:encryption_enabled"], out var enabled) ? enabled : UPPConstants.DefaultEncryptionEnabled;
    public static string GetUPPStorageDirectory() => ResolveConfiguredPath(Configuration["upp:storage_dir"] ?? "./upp_data");
    public static string GetUPPDefaultNamespace() => Configuration["upp:default_namespace"] ?? UPPConstants.DefaultNamespace;

    private static string ResolveConfiguredPath(string path)
    {
        if (Path.IsPathRooted(path))
            return path;

        var configPath = ResolveConfigPath(AppContext.BaseDirectory);
        return Path.GetFullPath(Path.Combine(configPath, "..", path));
    }
}
