using System;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpawn.Cli.Commands;
using DotnetSpawn.Cli.Commands.Plugins;
using DotnetSpawn.Configuration;
using DotnetSpawn.Infrastructure;
using DotnetSpawn.IO;
using DotnetSpawn.NuGet;
using DotnetSpawn.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace DotnetSpawn
{
    internal class Program
    {
        public static readonly CancellationTokenSource Cts = new();
        public static LogLevel LogLevel = LogLevel.Information;

        private static async Task<int> Main(string[] args)
        {
            Console.CancelKeyPress += (_, eventArgs) =>
            {
                if (Cts.IsCancellationRequested)
                {
                    SpectreConsole.WriteMarkup("[maroon]Terminating process[/]");

                    return;
                }

                Cts.Cancel();
                eventArgs.Cancel = true;

                SpectreConsole.WriteMarkup("[yellow]Attempting graceful exit. Press [maroon]Ctrl + C[/] again to terminate process.[/]");
            };

            var configuration = DotnetSpawnConfiguration.Load();

            var serviceCollection = ConfigureServices(new ServiceCollection(), configuration);
            var registrar = new TypeRegistrar(serviceCollection);
            var app = new CommandApp(registrar);

            app.Configure(config =>
            {
                config
                    .SetApplicationName("dotnet spawn")
                    .SetInterceptor(new GlobalSettingsInterceptor());

                config.AddBranch("plugin", plugin =>
                {
                    plugin.SetDescription("Manage spawn point plugins");

                    plugin.AddCommand<PluginInstallCommand>("install").WithAlias("i");
                    plugin.AddCommand<PluginListCommand>("list").WithAlias("ls");
                    plugin.AddCommand<PluginRemoveCommand>("remove").WithAlias("rm");
                    plugin.AddCommand<PluginDetailsCommand>("details").WithAlias("d");
                });
            });

            var exitCode = await app.RunAsync(args);

            Cts.Dispose();

            return exitCode;
        }

        private static IServiceCollection ConfigureServices(
            IServiceCollection services,
            DotnetSpawnConfiguration configuration)
        {
            return services
                .AddSingleton<IRenderer, MarkupRenderer>()
                .AddSingleton<SpectreConsole>()
                .AddSingleton<INuGetPackageValidator, PluginPackageValidator>()
                .AddSingleton(provider =>
                {
                    return new NuGetPackageInstaller(
                        configuration.PluginRootDirectory,
                        provider.GetRequiredService<SpectreConsole>(),
                        provider.GetServices<INuGetPackageValidator>());
                })
                .AddSingleton(provider =>
                {
                    return new NuGetPackageRemover(
                        configuration.PluginRootDirectory,
                        provider.GetRequiredService<SpectreConsole>());
                })
                .AddSingleton(provider =>
                {
                    return new SpawnPointPluginLoader(
                        configuration.PluginRootDirectory,
                        provider.GetRequiredService<SpectreConsole>());
                });
        }
    }
}
