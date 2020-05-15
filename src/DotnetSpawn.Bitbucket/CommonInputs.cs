using DotnetSpawn.Plugin;

namespace DotnetSpawn.Points.Bitbucket
{
    internal class CommonInputs
    {
        [Input("The URL to the Bitbucket Server instance", required: true)]
        public string ServerUrl { get; set; }

        [Input("The personal access token to use for authentication", required: true)]
        public string PersonalAccessToken { get; set; }
    }
}