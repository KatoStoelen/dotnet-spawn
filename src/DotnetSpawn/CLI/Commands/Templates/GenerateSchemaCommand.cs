using System.ComponentModel;
using DotnetSpawn.Cli.Commands;
using DotnetSpawn.IO;
using DotnetSpawn.Plugins;
using DotnetSpawn.Templating.Schema;
using Spectre.Console.Cli;

namespace DotnetSpawn.CLI.Commands.Templates
{
    [Description("(Re)generate the spawn point template schema file")]
    internal class GenerateSchemaCommand : Command<GenerateSchemaCommand.Settings>
    {
        public class Settings : GlobalSettings
        {
        }

        private readonly SpawnPointPluginLoader _pluginLoader;
        private readonly TemplateSchemaGenerator _schemaGenerator;
        private readonly SpectreConsole _console;

        public GenerateSchemaCommand(
            SpawnPointPluginLoader pluginLoader,
            TemplateSchemaGenerator schemaGenerator,
            SpectreConsole console)
        {
            _pluginLoader = pluginLoader;
            _schemaGenerator = schemaGenerator;
            _console = console;
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            try
            {
                var plugins = _pluginLoader.LoadAllPlugins();

                _schemaGenerator.Generate(plugins);

                _console.LogInformation("New template schema file generated");

                return 0;
            }
            catch (Exception e)
            {
                _console.LogError(e.Message, e);

                return 1;
            }
        }
    }
}