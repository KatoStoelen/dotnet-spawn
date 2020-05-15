using System.ComponentModel;
using DotnetSpawn.Cli.TypeConverters;
using DotnetSpawn.IO;
using Spectre.Console.Cli;

namespace DotnetSpawn.Cli.Commands
{
    internal class GlobalSettings : CommandSettings
    {
        [CommandOption("--verbosity <VERBOSITY>")]
        [Description("The minimum log level to use. Default: Information.")]
        [TypeConverter(typeof(EnumTypeConverter<LogLevel>))]
        [DefaultValue(LogLevel.Information)]
        public LogLevel LogLevel { get; set; }
    }
}