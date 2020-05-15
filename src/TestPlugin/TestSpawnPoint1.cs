using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpawn.Plugin;

namespace TestPlugin
{
    [SpawnPointId("Test.1")]
    [Description("Test spawn point #1")]
    public class TestSpawnPoint1 : ISpawnPoint<TestSpawnPoint1.Inputs>
    {
        public class Inputs
        {
            [Input("The name", required: true)]
            public string Name { get; set; }

            [Input("The price", required: true)]
            public decimal Price { get; set; }

            [Input("The quantity", required: true)]
            public int Quantity { get; set; }

            [Input("The other", required: true)]
            public NestedInputs Other { get; set; }

            public class NestedInputs
            {
                [Input("The tags", required: true)]
                public IEnumerable<string> Tags { get; set; }

                [Input("The attributes", required: true)]
                public Dictionary<string, string> Attributes { get; set; }
            }
        }

        [Output("The total")]
        public decimal TotalPrice { get; set; }

        public Task<SpawnResult> SpawnAsync(
            Inputs inputs, IConsole console, CancellationToken cancellationToken)
        {
            TotalPrice = inputs.Price * inputs.Quantity;

            return Task.FromResult(SpawnResult.Success());
        }
    }
}