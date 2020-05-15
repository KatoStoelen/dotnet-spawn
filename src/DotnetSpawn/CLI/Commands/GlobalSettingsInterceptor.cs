using Spectre.Console.Cli;

namespace DotnetSpawn.Cli.Commands
{
    internal class GlobalSettingsInterceptor : ICommandInterceptor
    {
        public void Intercept(CommandContext context, CommandSettings settings)
        {
            if (settings is GlobalSettings globalSettings)
            {
                Program.LogLevel = globalSettings.LogLevel;
            }
        }
    }
}