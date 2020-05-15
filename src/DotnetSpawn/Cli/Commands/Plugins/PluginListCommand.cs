using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DotnetSpawn.IO;
using DotnetSpawn.Plugins;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;

namespace DotnetSpawn.Cli.Commands.Plugins
{
    [Description("List installed spawn point plugins")]
    internal class PluginListCommand : Command<PluginListCommand.Settings>
    {
        public class Settings : GlobalSettings
        {
            [CommandOption("-s|--simple")]
            [Description("Display only plugin name and version")]
            public bool Simple { get; set; }
        }

        private readonly SpawnPointPluginLoader _pluginLoader;
        private readonly SpectreConsole _console;

        public PluginListCommand(SpawnPointPluginLoader pluginLoader, SpectreConsole console)
        {
            _pluginLoader = pluginLoader;
            _console = console;
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            try
            {
                using var plugins = _pluginLoader.LoadPlugins();

                if (plugins.Any())
                {
                    if (settings.Simple)
                    {
                        foreach (var plugin in plugins)
                        {
                            _console.WriteLine($"{plugin.Name}|{plugin.Version}", IO.Style.None);
                        }
                    }
                    else
                    {
                        RenderPluginTable(plugins);
                    }
                }
                else
                {
                    _console.WriteLine("No plugins installed", IO.Style.WarningHighlight);
                }

                return 0;
            }
            catch (Exception e)
            {
                _console.LogError(e.Message, e);

                return 1;
            }
        }

        private static void RenderPluginTable(SpawnPointPluginCollection plugins)
        {
            var spawnPointMetadatasPerPlugin = plugins.GetSpawnPoints()
                .GroupBy(spawnPoint => spawnPoint.Plugin)
                .Select(group =>
                    (
                        Plugin: group.Key,
                        Metadatas: group
                            .Select(spawnPoint => new SpawnPointMetadata(spawnPoint))
                            .ToList()))
                .ToDictionary(
                    tuple => tuple.Plugin,
                    tuple => tuple.Metadatas);

            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn(new TableColumn(new Markup("[orange3]Plugin[/]")))
                .AddColumn(new TableColumn(new Markup("[orange3]Version[/]")));

            var showCompatibleColumn = plugins.Any(plugin => !plugin.IsCompatible);

            if (showCompatibleColumn)
            {
                table.AddColumn(new TableColumn(new Markup("[orange3]Compatible?[/]")));
            }

            table.AddColumn(new TableColumn(new Markup("[orange3]Spawn Points[/]")));

            var index = 0;
            foreach (var plugin in plugins)
            {
                var rowData = new List<IRenderable>
                {
                    new Text(plugin.Name),
                    new Text(plugin.Version.ToString())
                };

                if (showCompatibleColumn)
                {
                    rowData.Add(plugin.IsCompatible
                        ? new Markup("[green]true[/]")
                        : new Markup("[maroon]false[/]"));
                }

                if (!plugin.IsCompatible)
                {
                    rowData.Add(new Markup("[yellow]N/A[/]"));
                }
                else
                {
                    var currentPluginSpawnPointFqns = spawnPointMetadatasPerPlugin[plugin]
                        .Select(meta => meta.Fqn);
                    var allSpawnPointFqns = spawnPointMetadatasPerPlugin
                        .SelectMany(pair => pair.Value)
                        .Select(meta => meta.Fqn);

                    rowData.Add(
                        new Text(
                            string.Join(
                                Environment.NewLine,
                                currentPluginSpawnPointFqns
                                    .Select(fqn => fqn.GetSortestValidQualifier(allSpawnPointFqns)))));
                }

                table.AddRow(rowData);

                if (index < plugins.Count - 1)
                {
                    table.AddEmptyRow();
                }

                index++;
            }

            SpectreConsole.Render(table);
        }
    }
}