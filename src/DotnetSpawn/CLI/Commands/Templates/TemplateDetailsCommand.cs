using System.ComponentModel;
using System.Globalization;
using DotnetSpawn.Cli.Commands;
using DotnetSpawn.Cli.TypeConverters;
using DotnetSpawn.IO;
using DotnetSpawn.Plugin;
using DotnetSpawn.Plugins;
using DotnetSpawn.Templating;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DotnetSpawn.CLI.Commands.Templates
{
    [Description("Show details of a spawn template")]
    internal class TemplateDetailsCommand : Command<TemplateDetailsCommand.Settings>
    {
        public class Settings : GlobalSettings
        {
            [CommandArgument(0, "[TEMPLATE]")]
            [Description("The name of the template to show details of")]
            public string TemplateName { get; set; }

            [CommandOption("--file")]
            [TypeConverter(typeof(FileInfoTypeConverter))]
            public FileInfo TemplateFile { get; set; }
        }

        private readonly TemplateLoader _templateLoader;
        private readonly SpawnPointPluginLoader _pluginLoader;
        private readonly SpectreConsole _console;

        public TemplateDetailsCommand(
            TemplateLoader templateLoader,
            SpawnPointPluginLoader pluginLoader,
            SpectreConsole console)
        {
            _templateLoader = templateLoader;
            _pluginLoader = pluginLoader;
            _console = console;
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            try
            {
                var template = GetTemplate(settings);

                DisplayTemplateDetails(template);

                return 0;
            }
            catch (Exception e)
            {
                _console.LogError($"Error while loading template details: {e.Message}");

                return 1;
            }
        }

        private SpawnTemplate GetTemplate(Settings settings)
        {
            if (settings.TemplateFile != null)
            {
                return _templateLoader.Load(settings.TemplateFile);
            }

            if (!string.IsNullOrWhiteSpace(settings.TemplateName))
            {
                return _templateLoader.Load(settings.TemplateName);
            }

            return PromptTemplate();
        }

        private SpawnTemplate PromptTemplate()
        {
            var allTemplates = _templateLoader.LoadAll().ToList();

            if (!allTemplates.Any())
            {
                throw new InvalidOperationException("No templates available");
            }

            if (allTemplates.Count == 1)
            {
                return allTemplates.Single();
            }

            var options = allTemplates
                .Select(template => new PromptOption<SpawnTemplate>(template, template.Name));

            return _console.Prompt(options, "Select template:");
        }

        private void DisplayTemplateDetails(SpawnTemplate template)
        {
            var rule = new Rule($"[teal]{template.Name}[/]")
                .LeftAligned()
                .RuleStyle("green");

            SpectreConsole.Render(rule);

            _console.WriteLine(string.Empty, IO.Style.None);

            if (!string.IsNullOrWhiteSpace(template.Description))
            {
                _console.WriteLine($"[italic]{template.Description}[/]", IO.Style.None);
                _console.WriteLine(string.Empty, IO.Style.None);
            }

            var stepsTable = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Index")
                .AddColumn("Name")
                .AddColumn("Description")
                .AddColumn("Spawn point");

            stepsTable.Title = new TableTitle("[teal]Steps[/]");

            var index = 0;

            foreach (var step in template.Steps)
            {
                var spawnPointMetadata = new SpawnPointMetadata(
                    _pluginLoader.LoadSpawnPoint(step.SpawnPointId));

                stepsTable.AddRow(new[]
                {
                    index.ToString(CultureInfo.InvariantCulture),
                    step.Name,
                    step.Description,
                    spawnPointMetadata.Fqn.FullyQualifiedName
                });

                index++;
            }

            SpectreConsole.Render(stepsTable);

            _console.WriteLine(string.Empty, IO.Style.None);
        }
    }
}