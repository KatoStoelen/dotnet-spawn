using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using DotnetSpawn.Cli.Commands;
using DotnetSpawn.Cli.TypeConverters;
using DotnetSpawn.Configuration;
using DotnetSpawn.IO;
using DotnetSpawn.Templating;
using DotnetSpawn.Templating.Serialization;
using Spectre.Console.Cli;

namespace DotnetSpawn.CLI.Commands.Templates
{
    [Description("Create a new spawn template")]
    internal class NewTemplateCommand : Command<NewTemplateCommand.Settings>
    {
        public class Settings : GlobalSettings
        {
            [CommandArgument(0, "<NAME>")]
            [Description("The name of the template")]
            public string Name { get; set; }

            [CommandOption("-d|--description")]
            [Description("An optional description of the template")]
            public string Description { get; set; }

            [CommandOption("-o|--open <EDITOR>")]
            [Description("Open the template in the specified editor")]
            public string Editor { get; set; }

            [CommandOption("--overwrite")]
            [Description("Overwrite template if already exists")]
            public bool Overwrite { get; set; }

            [CommandOption("--output-dir")]
            [Description(
                "Writes the new template to the specified directory instead of the default directory. " +
                "Remarks: Templates stored outside the default directory will not appear in template lists")]
            [TypeConverter(typeof(DirectoryInfoTypeConverter))]
            public DirectoryInfo OutputDirectory { get; set; }
        }

        private readonly DotnetSpawnConfiguration _configuration;
        private readonly SpectreConsole _console;

        public NewTemplateCommand(DotnetSpawnConfiguration configuration, SpectreConsole console)
        {
            _configuration = configuration;
            _console = console;
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            try
            {
                var template = new SpawnTemplate
                {
                    Name = settings.Name,
                    Description = settings.Description ?? string.Empty,
                    Steps = new List<SpawnStep>()
                };

                var fileName = SpawnTemplate.GetFileName(settings.Name);
                var outputPath = settings.OutputDirectory != null
                    ? Path.Combine(settings.OutputDirectory.FullName, fileName)
                    : Path.Combine(_configuration.TemplateRootDirectory, fileName);

                if (File.Exists(outputPath) && !settings.Overwrite)
                {
                    _console.LogError($"Template '{settings.Name}' already exists");

                    return 1;
                }

                _ = Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                var serializerOptions = new JsonSerializerOptions(
                    DotnetSpawnConfiguration.JsonSerializerOptions)
                {
                    Converters =
                    {
                        new SpawnTemplateTypeConverter(_configuration.SpawnTemplateSchemaPath)
                    }
                };

                using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    JsonSerializer.Serialize(fileStream, template, serializerOptions);
                }

                if (!string.IsNullOrWhiteSpace(settings.Editor))
                {
                    _console.LogInformation($"Template {settings.Name} created at {outputPath}");

                    using var process = Process.Start(new ProcessStartInfo(outputPath)
                    {
                        UseShellExecute = true
                    });

                    process.WaitForExit();
                }
                else
                {
                    _console.WriteLine(outputPath, Style.None);
                }


                return 0;
            }
            catch (Exception e)
            {
                _console.LogError($"Error generating new template: {e.Message}", e);

                return 1;
            }
        }
    }
}