using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpawn.Plugin;
using DotnetSpawn.Points.Bitbucket.Client;

namespace DotnetSpawn.Points.Bitbucket
{
    [SpawnPointId("Bitbucket.Repository")]
    [Description("Creates a new repository in Bitbucket Server")]
    internal class RepositorySpawnPoint : ISpawnPoint<RepositorySpawnPoint.Inputs>
    {
        internal class Inputs : CommonInputs
        {
            [Input("The name of the repository to create", required: true)]
            public string Name { get; set; }

            [Input("The unique ID of the SCM to use for this repository. Defaults to 'git'", required: true)]
            public string ScmId { get; set; } = "git";

            [Input("The key of the project this repository should be added to", required: true)]
            public string ProjectKey { get; set; }
        }

        [Output("The web URL to browse the new repository")]
        public string BrowseUrl { get; set; }

        [Output("The SSH URL for cloning the new repository")]
        public string CloneSshUrl { get; set; }

        [Output("The HTTP URL for cloning the new repository")]
        public string CloneHttpUrl { get; set; }

        public async Task<SpawnResult> SpawnAsync(
            Inputs inputs, IConsole console, CancellationToken cancellationToken)
        {
            try
            {
                using var bitbucketClient = new BitbucketClient(
                    inputs.ServerUrl, inputs.PersonalAccessToken);

                var repository = await console.DisplayStatusAsync(
                    $"Connecting to server...", async status =>
                    {
                        await bitbucketClient.ConnectAsync(cancellationToken).ConfigureAwait(false);

                        console.LogInformation($"Connected to Bitbucket server '{inputs.ServerUrl}'");

                        status.UpdateStatus("Creating repository...");

                        var repository = await bitbucketClient
                            .CreateRepositoryAsync(
                                inputs.ProjectKey,
                                new Client.Models.Write.Repository
                                {
                                    Name = inputs.Name,
                                    ScmId = inputs.ScmId
                                },
                                cancellationToken)
                            .ConfigureAwait(false);

                        console.LogInformation($"Bitbucket repository '{inputs.Name}' in project '{inputs.ProjectKey}' created");

                        return repository;
                    })
                    .ConfigureAwait(false);

                BrowseUrl = repository.Links["self"].Single(link => link.Name == null).Href;
                CloneSshUrl = repository.Links["clone"].SingleOrDefault(link => link.Name == "ssh").Href;
                CloneHttpUrl = repository.Links["clone"].SingleOrDefault(link => link.Name == "http").Href;

                console.LogInformation($"Bitbucket repository URL: {BrowseUrl}");

                return SpawnResult.Success();
            }
            catch (Exception e)
            {
                return SpawnResult.Fail(e.Message);
            }
        }
    }
}