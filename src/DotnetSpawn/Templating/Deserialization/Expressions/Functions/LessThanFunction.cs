namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal class LessThanFunction : CompareFunction
    {
        private LessThanFunction() { }

        protected override object Invoke(object[] args)
        {
            return Compare(args) < 0;
        }

        public class Metadata : CompareFunctionMetadata
        {
            public override string Name => "lt";

            public override string Description =>
                "Returns [teal]true[/] if [green]first[/] is less than " +
                "[green]second[/], otherwise [teal]false[/].";

            public override IEnumerable<string> Examples => new[]
            {
                "lt(output('step1', 'output1'), 1337)",
                "lt(output('step2', 'output2'), '2021-01-01')",
                "lt(output('step2', 'output2'), '2021-01-01', false)"
            };

            public override Func<Function> CreateInstance => () => new LessThanFunction();
        }
    }
}
