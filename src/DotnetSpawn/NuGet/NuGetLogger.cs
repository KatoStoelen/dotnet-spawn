using DotnetSpawn.IO;
using NuGet.Common;
using NuGetLogLevel = NuGet.Common.LogLevel;

namespace DotnetSpawn.NuGet
{
    internal class NuGetLogger : ILogger
    {
        private readonly SpectreConsole _console;

        public NuGetLogger(SpectreConsole console)
        {
            _console = console;
        }

        public void Log(NuGetLogLevel level, string data)
        {
            Action<string> log = level switch
            {
                NuGetLogLevel.Debug => logMessage => LogDebug(logMessage),
                NuGetLogLevel.Verbose => logMessage => LogVerbose(logMessage),
                NuGetLogLevel.Information => logMessage => LogInformation(logMessage),
                NuGetLogLevel.Minimal => logMessage => LogMinimal(logMessage),
                NuGetLogLevel.Warning => logMessage => LogWarning(logMessage),
                NuGetLogLevel.Error => logMessage => LogError(logMessage),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(level), level, $"Unsupported log level: {level}")
            };

            log(data);
        }

        public void Log(ILogMessage message)
        {
            Log(message.Level, message.FormatWithCode());
        }

        public Task LogAsync(NuGetLogLevel level, string data)
        {
            Log(level, data);

            return Task.CompletedTask;
        }

        public Task LogAsync(ILogMessage message)
        {
            Log(message);

            return Task.CompletedTask;
        }

        public void LogDebug(string data)
        {
            _console.LogTrace(data);
        }

        public void LogError(string data)
        {
            _console.LogError(data);
        }

        public void LogInformation(string data)
        {
            _console.LogTrace(data);
        }

        public void LogInformationSummary(string data)
        {
            _console.LogTrace(data);
        }

        public void LogMinimal(string data)
        {
            _console.LogTrace(data);
        }

        public void LogVerbose(string data)
        {
            _console.LogTrace(data);
        }

        public void LogWarning(string data)
        {
            _console.LogWarning(data);
        }
    }
}