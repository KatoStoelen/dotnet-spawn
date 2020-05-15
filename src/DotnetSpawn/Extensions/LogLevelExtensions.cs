using DotnetSpawn.IO;

namespace DotnetSpawn.Extensions
{
    internal static class LogLevelExtensions
    {
        public static string GetLogLevelString(this LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "trce",
                LogLevel.Debug => "dbug",
                LogLevel.Information => "info",
                LogLevel.Warning => "warn",
                LogLevel.Error => "fail",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(logLevel), logLevel, $"Unsupported log level: {logLevel}")
            };
        }

        public static Style GetLogLevelStyle(this LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => Style.LogTrace,
                LogLevel.Debug => Style.LogDebug,
                LogLevel.Information => Style.LogInformation,
                LogLevel.Warning => Style.LogWarning,
                LogLevel.Error => Style.LogError,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(logLevel), logLevel, $"Unsupported log level: {logLevel}")
            };
        }
    }
}