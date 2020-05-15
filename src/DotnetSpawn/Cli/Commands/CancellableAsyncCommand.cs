using System.Threading;
using System.Threading.Tasks;
using Spectre.Console.Cli;

namespace DotnetSpawn.Cli.Commands
{
    internal abstract class CancellableAsyncCommand<TSettings> : AsyncCommand<TSettings>
        where TSettings : CommandSettings
    {
        public sealed override Task<int> ExecuteAsync(CommandContext context, TSettings settings)
        {
            return ExecuteAsync(settings, Program.Cts.Token);
        }

        protected abstract Task<int> ExecuteAsync(
            TSettings settings, CancellationToken cancellationToken);
    }
}