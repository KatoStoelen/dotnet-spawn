using System.ComponentModel;
using DotnetSpawn.IO;
using DotnetSpawn.Templating.Deserialization.Expressions.Functions;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DotnetSpawn.CLI.Commands.Templates.Functions
{
    [Description("List all available functions that can be used in template expressions")]
    internal class ListFunctionsCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            DisplayFunctionsTable();

            return 0;
        }

        private static void DisplayFunctionsTable()
        {
            SpectreConsole.Render(
                new Text("To view detailed information about a function, use:"),
                newLineBefore: true,
                newLineAfter: true);
            SpectreConsole.Render(
                new Text("dotnet spawn template functions show [NAME]", "orange3"),
                newLineAfter: true);

            var table = new Table()
                .AddColumns(
                    new TableColumn("Function").NoWrap(),
                    new TableColumn("Description"))
                .Border(TableBorder.Rounded)
                .Title(new TableTitle("Functions", "teal"));

            foreach (var function in FunctionRegistry.Metadata)
            {
                var outlineMarkup = function
                    .ToString("I")
                    .EscapeMarkup()
                    .Replace(
                        $"{function.Name}(",
                        $"[green]{function.Name}[/](",
                        StringComparison.OrdinalIgnoreCase);

                table.AddRow(
                    new Markup(outlineMarkup),
                    new Markup(function.Description));
            }

            SpectreConsole.Render(table, newLineBefore: true);
        }
    }
}