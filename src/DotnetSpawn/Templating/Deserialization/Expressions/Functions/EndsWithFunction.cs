namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal class EndsWithFunction : StringSeekFunction
    {
        private EndsWithFunction() { }

        protected override bool Seek(string input, string value, StringComparison comparison)
        {
            return input.EndsWith(value, comparison);
        }

        public class Metadata : StringSeekFunctionMetadata
        {
            public override string Name => "endsWith";

            public override string Description =>
                "Returns [teal]true[/] if [green]input[/] ends with [green]value[/].";

            public override IEnumerable<string> Examples => new[]
            {
                "endsWith('ABCDEF', 'DEF')",
                "endsWith(output('step1', 'output1'), 'XYZ', false)"
            };

            public override Func<Function> CreateInstance => () => new EndsWithFunction();
        }
    }
}
