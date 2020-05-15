using DotnetSpawn.Cli.Commands;
using DotnetSpawn.Cli.Commands.Plugins;
using DotnetSpawn.CLI.Commands.Plugins;
using DotnetSpawn.CLI.Commands.Templates;
using DotnetSpawn.CLI.Commands.Templates.Functions;
using DotnetSpawn.Configuration;
using DotnetSpawn.Extensions;
using DotnetSpawn.Infrastructure;
using DotnetSpawn.IO;
using DotnetSpawn.NuGet;
using DotnetSpawn.Plugins;
using DotnetSpawn.Templating;
using DotnetSpawn.Templating.Schema;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace DotnetSpawn
{
    internal class Program
    {
        public static LogLevel LogLevel = LogLevel.Information;

        private static Task<int> Main(string[] args)
        {
            var serviceCollection = ConfigureServices(new ServiceCollection());
            var registrar = new TypeRegistrar(serviceCollection);
            var app = new CommandApp(registrar);

            app.Configure(cli =>
            {
                cli
                    .SetApplicationName("dotnet spawn")
                    .SetInterceptor(new GlobalSettingsInterceptor());

                cli.AddBranch("plugin", plugin =>
                {
                    plugin.SetDescription("Manage plugins");

                    plugin.AddCommand<PluginInstallCommand>("install").WithAlias("i");
                    plugin.AddCommand<PluginListCommand>("list").WithAlias("ls");
                    plugin.AddCommand<PluginDetailsCommand>("details").WithAlias("d");
                    plugin.AddCommand<PluginOutdatedCommand>("outdated").WithAlias("o");
                    plugin.AddCommand<PluginUninstallCommand>("uninstall").WithAlias("rm");
                    plugin.AddCommand<PluginUpgradeCommand>("upgrade").WithAlias("up");
                    plugin.AddCommand<RebuildCacheCommand>("rebuild-cache"); /* .IsHidden(); */
                });

                cli.AddBranch("template", template =>
                {
                    template.SetDescription($"Manage templates");

                    template.AddCommand<NewTemplateCommand>("new");
                    template.AddCommand<TemplateDetailsCommand>("details").WithAlias("d");
                    template.AddCommand<GenerateSchemaCommand>("generate-schema"); /* .IsHidden(); */

                    template.AddBranch("functions", functions =>
                    {
                        functions.SetDescription("Display information about template functions");

                        functions.AddCommand<ListFunctionsCommand>("list").WithAlias("ls");
                        functions.AddCommand<ShowFunctionCommand>("show");
                    });
                });
            });

            return app.RunAsync(args);
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            var configuration = DotnetSpawnConfiguration.Load();

            return services
                .AddSingleton(configuration)
                .AddSingleton<IRenderer, MarkupRenderer>()
                .AddSingleton<SpectreConsole>()
                .AddSingleton<NuGetPackageFinder>()
                .AddSingleton<PluginInstaller>()
                .AddSingleton<PluginUninstaller>()
                .AddSingleton<PluginUpgrader>()
                .AddSingleton<INuGetPackageValidator, PluginPackageValidator>()
                .AddSingleton(_ => PluginSpawnPointCache.Load())
                .AddSingleton<SpawnPointPluginLoader>()
                .AddSingleton<TemplateLoader>()
                .AddSingleton<NuGetPackageInstaller>()
                .AddSingleton<NuGetPackageRemover>()
                .AddSingleton<TemplateSchemaGenerator>()
                .AddSingleton<InputJsonSchemaGenerator>()
                .AddAllImplementationsOf<ISpecificInputJsonSchemaGenerator>();
        }
    }
}
