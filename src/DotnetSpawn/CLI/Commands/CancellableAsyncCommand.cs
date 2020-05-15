using System.Runtime.InteropServices;
using DotnetSpawn.IO;
using Spectre.Console.Cli;

namespace DotnetSpawn.Cli.Commands
{
    internal abstract class CancellableAsyncCommand<TSettings> : AsyncCommand<TSettings>
        where TSettings : CommandSettings
    {
        public sealed override async Task<int> ExecuteAsync(CommandContext context, TSettings settings)
        {
            using var cts = new CancellationTokenSource();

            using var sigInt = PosixSignalRegistration.Create(PosixSignal.SIGINT, OnPosixSignal);
            using var sigQuit = PosixSignalRegistration.Create(PosixSignal.SIGQUIT, OnPosixSignal);
            using var sigTerm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, OnPosixSignal);

            return await ExecuteAsync(settings, cts.Token);

            void OnPosixSignal(PosixSignalContext context)
            {
                if (cts.IsCancellationRequested)
                {
                    SpectreConsole.WriteMarkup(
                        "[maroon]Terminating process[/]".PadRight(Console.BufferWidth, ' '));

                    return;
                }

                SpectreConsole.WriteMarkup(
                    "[yellow]Attempting graceful exit. Press [maroon]Ctrl + C[/] again to terminate process.[/]");

                cts.Cancel();
                context.Cancel = true;
            }
        }

        protected abstract Task<int> ExecuteAsync(
            TSettings settings, CancellationToken cancellationToken);
    }
}