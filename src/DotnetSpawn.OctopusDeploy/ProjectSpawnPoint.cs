using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpawn.Plugin;
using Octopus.Client;
using Octopus.Client.Model;

namespace DotnetSpawn.OctopusDeploy
{
    [SpawnPointId("OctopusDeploy.Project")]
    [Description("Creates a new Octopus Deploy project in the specified group using the specified lifecycle")]
    internal class ProjectSpawnPoint : ISpawnPoint<ProjectSpawnPoint.Inputs>
    {
        internal class Inputs : CommonInputs
        {
            [Input("The name of the project to create", required: true)]
            public string Name { get; set; }

            [Input("An optional description of the project", required: false)]
            public string Description { get; set; } = string.Empty;

            [Input("The ID of the group this project belongs to", required: true)]
            public string ProjectGroupId { get; set; }

            [Input("The ID of the deployment lifecycle this project should use", required: true)]
            public string LifecycleId { get; set; }
        }

        [Output("The ID of the new project")]
        public string ProjectId { get; set; }

        [Output("The web URL to browse the new project")]
        public string BrowseUrl { get; set; }

        public async Task<SpawnResult> SpawnAsync(
            Inputs inputs, IConsole console, CancellationToken cancellationToken)
        {
            try
            {
                var project = await console.DisplayStatusAsync(
                    "Connecting to server...", async status =>
                    {
                        var octopusEndpoint = new OctopusServerEndpoint(inputs.ApiUrl, inputs.ApiKey);
                        using var octopusClient = await OctopusAsyncClient
                            .Create(octopusEndpoint)
                            .ConfigureAwait(false);

                        console.LogInformation($"Connected to Octopus Deploy API '{inputs.ApiUrl}'");

                        // Octopus.Client does not currently support cancellation tokens
                        cancellationToken.ThrowIfCancellationRequested();

                        status.UpdateStatus("Creating project...");

                        var project = await octopusClient.Repository.Projects.Create(new ProjectResource
                        {
                            Name = inputs.Name,
                            Description = inputs.Description,
                            ProjectGroupId = inputs.ProjectGroupId,
                            LifecycleId = inputs.LifecycleId
                        }).ConfigureAwait(false);

                        console.LogInformation($"Octopus Deploy project '{inputs.Name}' created");

                        return project;
                    })
                    .ConfigureAwait(false);


                ProjectId = project.Id;
                BrowseUrl = inputs.MakeAbsoluteUrl(project.Link("web"));

                console.LogInformation($"Octopus Deploy project URL: {BrowseUrl}");

                return SpawnResult.Success();
            }
            catch (Exception e)
            {
                return SpawnResult.Fail(e.Message);
            }
        }
    }
}