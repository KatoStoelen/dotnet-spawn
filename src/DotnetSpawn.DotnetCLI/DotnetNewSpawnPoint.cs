using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpawn.Plugin;

namespace DotnetSpawn.DotnetCLI
{
    [SpawnPointId("Dotnet.New")]
    [Description("Creates the specified dotnet new template")]
    internal class DotnetNewSpawnPoint : ISpawnPoint<DotnetNewSpawnPoint.Inputs>
    {
        internal class Inputs
        {
            [Input("The working directory within which the 'dotnet new' template should be created", required: true)]
            public string WorkingDirectory { get; set; }

            [Input("The short name of the template to create", required: true)]
            public string TemplateShortName { get; set; }

            [Input("Additional arguments to pass to 'dotnet new'", required: false)]
            public Dictionary<string, string> AdditionalArguments { get; set; }
        }

        public async Task<SpawnResult> SpawnAsync(
            Inputs inputs, IConsole console, CancellationToken cancellationToken)
        {
            try
            {
                var exitCode = await console.DisplayStatusAsync(
                    $"Creating template '{inputs.TemplateShortName}'",
                    _ => Dotnet.NewAsync(
                        inputs.TemplateShortName,
                        new DebugLogger(console),
                        inputs.AdditionalArguments,
                        inputs.WorkingDirectory,
                        cancellationToken: cancellationToken));

                if (exitCode == 0)
                {
                    console.LogInformation($"Template {inputs.TemplateShortName} created in directory {inputs.WorkingDirectory}");

                    return SpawnResult.Success();
                }
                else
                {
                    return SpawnResult.Fail($"Exit code {exitCode} when running 'dotnet new'");
                }
            }
            catch (Exception e)
            {
                return SpawnResult.Fail(e.Message);
            }
        }
    }
}