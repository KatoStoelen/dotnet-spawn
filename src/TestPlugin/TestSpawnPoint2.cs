using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpawn.Plugin;

namespace TestPlugin
{
    [SpawnPointId("Test.2")]
    [Description("Shows the total")]
    public class TestSpawnPoint2 : ISpawnPoint<TestSpawnPoint2.Inputs>
    {
        public class Inputs
        {
            [Input("The total", required: true)]
            public decimal TotalPrice { get; set; }
        }

        public Task<SpawnResult> SpawnAsync(
            Inputs inputs, IConsole console, CancellationToken cancellationToken)
        {
            console.LogInformation($"Total: {inputs.TotalPrice:N}");

            return Task.FromResult(SpawnResult.Success());
        }
    }
}
