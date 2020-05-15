using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpawn.Plugin;
using TeamCitySharp;
using TeamCitySharp.DomainEntities;

namespace DotnetSpawn.Points.TeamCity
{
    [SpawnPointId("TeamCity.Project")]
    [Description("Creates a new project either at root or as a child of the specified parent project")]
    internal class ProjectSpawnPoint : ISpawnPoint<ProjectSpawnPoint.Inputs>
    {
        internal class Inputs : CommonInputs
        {
            [Input("The name of the project to create", required: true)]
            public string Name { get; set; }

            [Input(
                "An optional ID of this project's parant project. If not specified, the project is created at root",
                required: false)]
            public string ParentProjectId { get; set; }
        }


        [Output("The ID of the new project")]
        public string ProjectId { get; set; }

        [Output("The web URL for the new project")]
        public string BrowseUrl { get; set; }

        public Task<SpawnResult> SpawnAsync(
            Inputs inputs, IConsole console, CancellationToken cancellationToken)
        {
            try
            {
                var project = console.DisplayStatus(string.Empty, status =>
                {
                    return CreateProject(inputs, console, status);
                });

                ProjectId = project.Id;
                BrowseUrl = project.WebUrl;

                console.LogInformation($"Project URL: {BrowseUrl}");

                return Task.FromResult(SpawnResult.Success());
            }
            catch (Exception e)
            {
                return Task.FromResult(SpawnResult.Fail(e.Message));
            }
        }

        private static Project CreateProject(Inputs inputs, IConsole console, IStatus status)
        {
            status.UpdateStatus("Connecting to server...");

            var teamCityClient = new TeamCityClient(inputs.ServerHostName);
            teamCityClient.ConnectWithAccessToken(inputs.AccessToken);

            console.LogInformation($"Connected to TeamCity server '{inputs.ServerHostName}'");

            status.UpdateStatus("Creating project...");

            var project = string.IsNullOrEmpty(inputs.ParentProjectId)
                ? teamCityClient.Projects.Create(inputs.Name)
                : teamCityClient.Projects.Create(inputs.Name, inputs.ParentProjectId);

            console.LogInformation($"TeamCity project '{inputs.Name}' created");

            return project;
        }
    }
}