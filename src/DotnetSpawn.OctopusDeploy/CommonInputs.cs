using System;
using DotnetSpawn.Plugin;

namespace DotnetSpawn.OctopusDeploy
{
    internal class CommonInputs
    {
        [Input("The URL of the Octopus Deploy server", required: true)]
        public string ServerUrl { get; set; }

        [Input("The API key to use for authentication", required: true)]
        public string ApiKey { get; set; }

        public string ApiUrl => MakeAbsoluteUrl("/api");

        public string MakeAbsoluteUrl(string relativeUrl) =>
            new Uri(new Uri(ServerUrl), relativeUrl).ToString();
    }
}
