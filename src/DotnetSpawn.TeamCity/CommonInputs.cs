using DotnetSpawn.Plugin;

namespace DotnetSpawn.Points.TeamCity
{
    internal class CommonInputs
    {
        [Input("The TeamCity server host name", required: true)]
        public string ServerHostName { get; set; }

        [Input("The access token to use for authentication", required: true)]
        public string AccessToken { get; set; }
    }
}