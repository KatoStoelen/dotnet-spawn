namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal class GreaterThanFunction : CompareFunction
    {
        private GreaterThanFunction() { }

        protected override object Invoke(object[] args)
        {
            return Compare(args) > 0;
        }

        public class Metadata : CompareFunctionMetadata
        {
            public override string Name => "gt";

            public override string Description =>
                "Returns [teal]true[/] if [green]first[/] is greater than " +
                "[green]second[/], otherwise [teal]false[/].";

            public override IEnumerable<string> Examples => new[]
            {
                "gt(output('step1', 'output1'), 1337)",
                "gt(output('step2', 'output2'), '2021-01-01')",
                "gt(output('step2', 'output2'), '2021-01-01', false)"
            };

            public override Func<Function> CreateInstance => () => new GreaterThanFunction();
        }
    }
}
