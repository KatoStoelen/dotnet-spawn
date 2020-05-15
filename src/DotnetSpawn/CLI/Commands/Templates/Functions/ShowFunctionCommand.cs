using System.ComponentModel;
using DotnetSpawn.Extensions;
using DotnetSpawn.IO;
using DotnetSpawn.Plugin;
using DotnetSpawn.Templating.Deserialization.Expressions.Functions;
using DotnetSpawn.Templating.Deserialization.Expressions.Functions.Metadata;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DotnetSpawn.CLI.Commands.Templates.Functions
{
    [Description("Display detailed information about template function")]
    internal class ShowFunctionCommand : Command<ShowFunctionCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandArgument(0, "[NAME]")]
            [Description("Name of the function to display. If not specified, a prompt will be shown")]
            public string FunctionName { get; set; }
        }

        private const int Indent = 4;
        private static readonly string s_indentString = new(' ', Indent);

        private readonly SpectreConsole _console;

        public ShowFunctionCommand(SpectreConsole console)
        {
            _console = console;
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            var functionMetadata = GetFunctionMetadata(settings.FunctionName);

            if (functionMetadata == null)
            {
                return 1;
            }

            DisplayFunctionDetails(functionMetadata);

            return 0;
        }

        private FunctionMetadata GetFunctionMetadata(string functionName)
        {
            if (!string.IsNullOrWhiteSpace(functionName))
            {
                if (FunctionRegistry.TryGetMetadata(functionName, out var metadata))
                {
                    return metadata;
                }

                _console.LogError($"Unknown function: {functionName}");

                return null;
            }

            var options = FunctionRegistry.Metadata
                .Select(metadata => new PromptOption<FunctionMetadata>(metadata, metadata.Name));

            return _console.Prompt(options, "Select function:");
        }

        private void DisplayFunctionDetails(FunctionMetadata functionMetadata)
        {
            _console.WriteLine(string.Empty, IO.Style.None);

            var rule = new Rule($"[teal]{functionMetadata.ToString("I").EscapeMarkup()}[/]")
                .LeftAligned()
                .RuleStyle("green");

            SpectreConsole.Render(rule);

            _console.WriteLine(string.Empty, IO.Style.None);
            SpectreConsole.WriteMarkup(functionMetadata.Description);
            _console.WriteLine(string.Empty, IO.Style.None);

            DisplayParameterSection(functionMetadata);
            DisplayReturnsSection(functionMetadata);
            DisplayExamplesSection(functionMetadata);
            DisplayRemarksSection(functionMetadata);
        }

        private void DisplayParameterSection(FunctionMetadata functionMetadata)
        {
            _console.WriteLine("PARAMETERS:", IO.Style.None);

            var parameterIndex = 0;

            foreach (var parameter in functionMetadata.Parameters)
            {
                var parameterType = parameter.IsParams
                    ? "params " + parameter.Type.ToPrettyName()
                    : parameter.Type.ToPrettyName();

                var parameterMarkup = $"{s_indentString}[green]{parameter.Name}[/] [teal]{parameterType}[/]";

                if (parameter.IsOptional)
                {
                    parameterMarkup += $" [ = {parameter.DefaultValue.ConvertToString()} ]".EscapeMarkup();
                }
                else if (parameter.IsParams)
                {
                    var function = functionMetadata.CreateInstance();
                    var minimumParamsArguments = function.MinimumArgumentCount - parameterIndex;

                    parameterMarkup += $" (minimum arguments: {minimumParamsArguments})";
                }

                SpectreConsole.Render(
                    new Markup(parameterMarkup),
                    newLineAfter: true);

                var description = new Grid()
                    .AddColumn(new GridColumn().PadLeft(Indent))
                    .AddRow(parameter.Description);

                SpectreConsole.Render(description);

                _console.WriteLine(string.Empty, IO.Style.None);

                parameterIndex++;
            }
        }

        private void DisplayReturnsSection(FunctionMetadata functionMetadata)
        {
            _console.WriteLine("RETURNS:", IO.Style.None);

            SpectreConsole.Render(
                new Text(s_indentString + functionMetadata.ReturnType.ToPrettyName(), "teal"),
                newLineAfter: true);

            _console.WriteLine(string.Empty, IO.Style.None);
        }

        private void DisplayExamplesSection(FunctionMetadata functionMetadata)
        {
            _console.WriteLine("EXAMPLES:", IO.Style.None);

            foreach (var example in functionMetadata.Examples)
            {
                var exampleMarkup = example.Replace(
                    $"{functionMetadata.Name}(",
                    $"[green]{functionMetadata.Name}[/](",
                    StringComparison.OrdinalIgnoreCase);

                SpectreConsole.Render(new Markup(s_indentString + exampleMarkup), newLineAfter: true);
            }

            _console.WriteLine(string.Empty, IO.Style.None);
        }

        private void DisplayRemarksSection(FunctionMetadata functionMetadata)
        {
            if (string.IsNullOrWhiteSpace(functionMetadata.Remarks))
            {
                return;
            }

            _console.WriteLine("REMARKS:", IO.Style.None);

            var remarks = new Grid()
                .AddColumn(new GridColumn().PadLeft(Indent))
                .AddRow(functionMetadata.Remarks);

            SpectreConsole.Render(remarks);

            _console.WriteLine(string.Empty, IO.Style.None);
        }
    }
}