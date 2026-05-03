using illusion.Common.Constants;
//using illusion.Common.Extensions;
using Serilog;
using Serilog.Events;
//using Serilog.Sinks.Console;
using Serilog.Sinks.File;
using Serilog.Sinks.SystemConsole.Themes;

namespace illusion.Common.Utils;

/// <summary>
/// logger utility class that provides a global logger instance configured with Serilog
/// </summary>
public static class LoggerUtils
{
    private static ILogger? _logger;
    private static readonly object _lock = new();

    public static ILogger Log
    {
        get
        {
            if (_logger == null)
            {
                lock (_lock)
                {
                    _logger ??= CreateLogger();
                }
            }
            return _logger;
        }
    }

    private static ILogger CreateLogger()
    {
        var logLevel = ParseLogLevel(ConfigLoader.GetLogLevel());
        var logPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "logs"));
        Directory.CreateDirectory(logPath);

        var logFile = Path.Combine(logPath, $"illusionary-ai-.log");

        return new LoggerConfiguration()
            .MinimumLevel.Is(logLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", SystemConstants.ProjectName)
            .Enrich.WithProperty("Version", SystemConstants.Version)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                theme: AnsiConsoleTheme.Code)
            .WriteTo.File(
                logFile,
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Application} v{Version} {Message:lj}{NewLine}{Exception}",
                retainedFileCountLimit: 30)
            //.WriteTo.Debug()
            .CreateLogger();
    }

    private static LogEventLevel ParseLogLevel(string? logLevelStr)
    {
        return logLevelStr?.ToUpperInvariant() switch
        {
            "DEBUG" => LogEventLevel.Debug,
            "INFO" => LogEventLevel.Information,
            "WARNING" => LogEventLevel.Warning,
            "ERROR" => LogEventLevel.Error,
            "FATAL" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }

    // facilitate logging with different levels and structured logging support
    public static void Debug(string message, params object[] args) => Log.Debug(message, args);
    public static void Info(string message, params object[] args) => Log.Information(message, args);
    public static void Warning(string message, params object[] args) => Log.Warning(message, args);
    public static void Error(string message, params object[] args) => Log.Error(message, args);
    public static void Error(Exception ex, string message, params object[] args) => Log.Error(ex, message, args);
    public static void Fatal(string message, params object[] args) => Log.Fatal(message, args);
}