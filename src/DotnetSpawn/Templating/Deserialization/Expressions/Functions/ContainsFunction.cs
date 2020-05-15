namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{

    internal class ContainsFunction : StringSeekFunction
    {
        private ContainsFunction() { }

        protected override bool Seek(string input, string value, StringComparison comparison)
        {
            return input.Contains(value, comparison);
        }

        public class Metadata : StringSeekFunctionMetadata
        {
            public override string Name => "contains";

            public override string Description =>
                "Returns [teal]true[/] if [green]input[/] contains [green]value[/].";

            public override IEnumerable<string> Examples => new[]
            {
                "contains('ABCDEF', 'CDE')",
                "contains(output('step1', 'output1'), 'XYZ', false)"
            };

            public override Func<Function> CreateInstance => () => new ContainsFunction();
        }
    }
}