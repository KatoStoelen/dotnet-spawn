using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpawn.Plugin;
using DotnetSpawn.Points.Bitbucket.Client;

namespace DotnetSpawn.Points.Bitbucket
{
    [SpawnPointId("Bitbucket.Project")]
    [Description("Creates a new project in Bitbucket Server")]
    internal class ProjectSpawnPoint : ISpawnPoint<ProjectSpawnPoint.Inputs>
    {
        internal class Inputs : CommonInputs
        {
            [Input("The name of the project to create", required: true)]
            public string Name { get; set; }

            [Input("The key of the project to create", required: true)]
            public string Key { get; set; }

            [Input("An optional description of the project to create", required: false)]
            public string Description { get; set; } = string.Empty;
        }

        [Output("The key of the new project")]
        public string ProjectKey { get; set; }

        [Output("The web URL to browse the new project")]
        public string BrowseUrl { get; set; }

        public async Task<SpawnResult> SpawnAsync(
            Inputs inputs, IConsole console, CancellationToken cancellationToken)
        {
            try
            {
                using var bitbucketClient = new BitbucketClient(
                    inputs.ServerUrl, inputs.PersonalAccessToken);

                console.LogInformation($"Connecting to Bitbucket server '{inputs.ServerUrl}'...");

                await bitbucketClient.ConnectAsync(cancellationToken).ConfigureAwait(false);

                console.LogInformation($"Creating Bitbucket project '{inputs.Name} ({inputs.Key})'...");

                var project = await bitbucketClient
                    .CreateProjectAsync(
                        new Client.Models.Write.Project
                        {
                            Key = inputs.Key,
                            Name = inputs.Name,
                            Description = inputs.Description
                        },
                        cancellationToken)
                    .ConfigureAwait(false);

                ProjectKey = project.Key;
                BrowseUrl = project.Links["self"].Single(link => link.Name == null).Href;

                console.LogInformation($"Bitbucket project created @ {BrowseUrl}");

                return SpawnResult.Success();
            }
            catch (Exception e)
            {
                return SpawnResult.Fail(e.Message);
            }
        }
    }
}