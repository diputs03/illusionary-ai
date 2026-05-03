using Microsoft.Extensions.Configuration;
using illusion.Common.Constants;
//using illusion.Common.Extensions;

namespace illusion.Common.Utils;

/// <summary>
/// global configuration loader for system/UPP/GGTP
/// supports hot reload and environment variable overrides
/// </summary>
public static class ConfigLoader
{
    private static IConfiguration? _configuration;
    private static readonly object _lock = new();

    /// <summary>
    /// get the global configuration instance, lazy load and thread safe
    /// </summary>
    public static IConfiguration Configuration
    {
        get
        {
            if (_configuration == null)
            {
                lock (_lock)
                {
                    _configuration ??= LoadConfiguration();
                }
            }
            return _configuration;
        }
    }

    private static IConfiguration LoadConfiguration()
    {
        // construct config path
        var basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var configPath = Path.Combine(basePath, "config");

        if (!Directory.Exists(configPath))
        {
            throw new DirectoryNotFoundException($"config directory not found: {configPath}");
        }

        return new ConfigurationBuilder()
            .SetBasePath(configPath)
            .AddYamlFile("system.yaml", optional: false, reloadOnChange: true)
            .AddYamlFile("ggtp.yaml", optional: false, reloadOnChange: true)
            .AddYamlFile("upp.yaml", optional: false, reloadOnChange: true)
            //.AddEnvironmentVariables(prefix: "ILLUSION_") // support environment variable overrides, e.g., ILLUSION_SYSTEM__LOG_LEVEL
            .Build();
    }

    // facilitate access to specific config values with defaults
    public static string GetSystemName() => Configuration["system:name"] ?? SystemConstants.ProjectName;
    public static string GetLogLevel() => Configuration["system:log_level"] ?? SystemConstants.DefaultLogLevel;
    public static string GetGGTPServerUrl() => Configuration["ggtp:server_url"] ?? GGTPConstants.DefaultServerUrl;
    public static bool IsUPPEncryptionEnabled() => bool.TryParse(Configuration["upp:encryption_enabled"], out var enabled) ? enabled : UPPConstants.DefaultEncryptionEnabled;
}