namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal class GreaterThanOrEqualFunction : CompareFunction
    {
        private GreaterThanOrEqualFunction() { }

        protected override object Invoke(object[] args)
        {
            return Compare(args) >= 0;
        }

        public class Metadata : CompareFunctionMetadata
        {
            public override string Name => "ge";

            public override string Description =>
                "Returns [teal]true[/] if [green]first[/] is greater than or equal to " +
                "[green]second[/], otherwise [teal]false[/].";

            public override IEnumerable<string> Examples => new[]
            {
                "ge(output('step1', 'output1'), 1337)",
                "ge(output('step2', 'output2'), '2021-01-01')",
                "ge(output('step2', 'output2'), '2021-01-01', false)"
            };

            public override Func<Function> CreateInstance => () => new GreaterThanOrEqualFunction();
        }
    }
}
