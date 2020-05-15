namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal class StartsWithFunction : StringSeekFunction
    {
        private StartsWithFunction() { }

        protected override bool Seek(string input, string value, StringComparison comparison)
        {
            return input.StartsWith(value, comparison);
        }

        public class Metadata : StringSeekFunctionMetadata
        {
            public override string Name => "startsWith";

            public override string Description =>
                "Returns [teal]true[/] if [green]input[/] starts with [green]value[/].";

            public override IEnumerable<string> Examples => new[]
            {
                "startsWith('ABCDEF', 'ABC')",
                "startsWith(output('step1', 'output1'), 'XYZ', false)"
            };

            public override Func<Function> CreateInstance => () => new StartsWithFunction();
        }
    }
}
