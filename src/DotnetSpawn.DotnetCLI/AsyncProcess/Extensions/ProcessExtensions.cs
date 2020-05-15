using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncProcess.Internal.Extensions
{
    // Slightly modified version of:
    // https://github.com/dotnet/cli/blob/master/test/Microsoft.DotNet.Tools.Tests.Utilities/Extensions/ProcessExtensions.cs
    internal static class ProcessExtensions
    {
        internal static int RunProcessAndWaitForExit(
            string fileName, string arguments, TimeSpan timeout, out string stdout)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var process = Process.Start(startInfo);

            stdout = null;
            if (process.WaitForExit((int)timeout.TotalMilliseconds))
            {
                stdout = process.StandardOutput.ReadToEnd();
            }
            else
            {
                process.Kill();
            }

            return process.ExitCode;
        }

        public static async Task<int> StartAndWaitForExitAsync(
            this Process process, CancellationToken cancellationToken = default)
        {
            var processCompletionSource = new TaskCompletionSource<int>();

            process.EnableRaisingEvents = true;
            process.Exited += OnExited;

            using var _ = cancellationToken.Register(() =>
            {
                process.Exited -= OnExited;
                processCompletionSource.SetCanceled();
            });

            return process.Start()
                ? await processCompletionSource.Task
                : throw new ArgumentException(
                    "Failed to start specified process", nameof(process));

            void OnExited(object sender, EventArgs e)
            {
                process.Exited -= OnExited;

                processCompletionSource.SetResult(process.ExitCode);
            }
        }
    }
}