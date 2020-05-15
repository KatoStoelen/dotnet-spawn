using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpawn.Plugin;
using TeamCitySharp;
using TeamCitySharp.DomainEntities;

namespace DotnetSpawn.Points.TeamCity
{
    [SpawnPointId("TeamCity.BuildConfiguration")]
    [Description("Creates a new build configuration within the specified project")]
    internal class BuildConfigurationSpawnPoint : ISpawnPoint<BuildConfigurationSpawnPoint.Inputs>
    {
        internal class Inputs : CommonInputs
        {
            [Input("The name of the build configuration to create", required: true)]
            public string Name { get; set; }

            [Input("An optional description of the build configuration", required: false)]
            public string Description { get; set; } = string.Empty;

            [Input("The ID of the project the build configuration belongs to", required: true)]
            public string ProjectId { get; set; }

            [Input("An optional ID of the template the build configuration should be based on", required: false)]
            public string TemplateId { get; set; }
        }

        [Output("The ID of the new build configuration")]
        public string BuildId { get; set; }

        [Output("The web URL to browse the new build configuration")]
        public string BrowseUrl { get; set; }

        public Task<SpawnResult> SpawnAsync(
            Inputs inputs, IConsole console, CancellationToken cancellationToken)
        {
            try
            {
                console.LogInformation($"Creating build configuration '{inputs.Name}'...");

                var buildConfiguration = CreateBuildConfiguration(inputs);

                BuildId = buildConfiguration.Id;
                BrowseUrl = buildConfiguration.WebUrl;

                console.LogInformation($"Build configuration created @ {buildConfiguration.WebUrl}");

                return Task.FromResult(SpawnResult.Success());
            }
            catch (Exception e)
            {
                return Task.FromResult(SpawnResult.Fail(e.Message));
            }
        }

        private static BuildConfig CreateBuildConfiguration(Inputs inputs)
        {
            var teamCityClient = new TeamCityClient(inputs.ServerHostName);
            teamCityClient.ConnectWithAccessToken(inputs.AccessToken);

            var buildConfig = new BuildConfig
            {
                Name = inputs.Name,
                Description = inputs.Description,
                ProjectId = inputs.ProjectId
            };

            if (!string.IsNullOrEmpty(inputs.TemplateId))
                buildConfig.Template = new Template { Id = inputs.TemplateId };

            return teamCityClient.BuildConfigs.CreateConfiguration(buildConfig);
        }
    }
}