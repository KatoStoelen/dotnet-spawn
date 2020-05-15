using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpawn.DotnetCLI.AsyncProcess;

namespace DotnetSpawn.DotnetCLI
{
    internal static class Dotnet
    {
        private static readonly TimeSpan s_defaultTimeout = TimeSpan.FromMinutes(1);

        public static Task<int> NewAsync(
            string templateShortName,
            TextWriter outputWriter,
            Dictionary<string, string> options = null,
            string workingDirectory = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var processRunner = new ProcessRunner("dotnet")
                .WithArguments(args =>
                {
                    _ = args
                        .AddVerb("new")
                        .AddNoun(templateShortName);

                    if (options != null)
                    {
                        foreach (var option in options)
                        {
                            _ = args.AddOption(option.Key, option.Value);
                        }
                    }
                })
                .WithWorkingDirectory(workingDirectory);

            return processRunner
                .RunWithTimeoutAsync(outputWriter, timeout, cancellationToken);
        }

        private static async Task<int> RunWithTimeoutAsync(
            this ProcessRunner runner,
            TextWriter outputWriter,
            TimeSpan? timeout,
            CancellationToken cancellationToken)
        {
            using var timeoutCts = new CancellationTokenSource(timeout ?? s_defaultTimeout);
            using var combinedCts = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            return await runner.RunAsync(outputWriter, combinedCts.Token);
        }
    }
}