using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpawn.Plugin;
using Octopus.Client;
using Octopus.Client.Model;

namespace DotnetSpawn.OctopusDeploy
{
    [SpawnPointId("OctopusDeploy.Group")]
    [Description("Creates a new Octopus Deploy project group")]
    internal class ProjectGroupSpawnPoint : ISpawnPoint<ProjectGroupSpawnPoint.Inputs>
    {
        internal class Inputs : CommonInputs
        {
            [Input("The name of the project group to create", required: true)]
            public string Name { get; set; }

            [Input("An optional description of the project group", required: false)]
            public string Description { get; set; } = string.Empty;
        }

        [Output("The ID of the new project group")]
        public string GroupId { get; set; }

        public async Task<SpawnResult> SpawnAsync(
            Inputs inputs, IConsole console, CancellationToken cancellationToken)
        {
            try
            {
                console.LogInformation($"Connecting to Octopus Deploy API '{inputs.ApiUrl}'...");

                var octopusEndpoint = new OctopusServerEndpoint(inputs.ApiUrl, inputs.ApiKey);
                using var octopusClient = await OctopusAsyncClient
                    .Create(octopusEndpoint)
                    .ConfigureAwait(false);

                // Octopus.Client does not currently support cancellation tokens
                cancellationToken.ThrowIfCancellationRequested();

                console.LogInformation($"Creating Octopus Deploy project group '{inputs.Name}'...");

                var projectGroup = await octopusClient.Repository.ProjectGroups
                    .Create(new ProjectGroupResource
                    {
                        Name = inputs.Name,
                        Description = inputs.Description
                    }).ConfigureAwait(false);

                GroupId = projectGroup.Id;

                console.LogInformation($"Octopus Deploy project group '{inputs.Name}' created");

                return SpawnResult.Success();
            }
            catch (Exception e)
            {
                return SpawnResult.Fail(e.Message);
            }
        }
    }
}