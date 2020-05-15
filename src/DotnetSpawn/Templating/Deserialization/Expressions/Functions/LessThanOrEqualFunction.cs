namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal class LessThanOrEqualFunction : CompareFunction
    {
        private LessThanOrEqualFunction() { }

        protected override object Invoke(object[] args)
        {
            return Compare(args) <= 0;
        }

        public class Metadata : CompareFunctionMetadata
        {
            public override string Name => "le";

            public override string Description =>
                "Returns [teal]true[/] if [green]first[/] is less than or equal to " +
                "[green]second[/], otherwise [teal]false[/].";

            public override IEnumerable<string> Examples => new[]
            {
                "le(output('step1', 'output1'), 1337)",
                "le(output('step2', 'output2'), '2021-01-01')",
                "le(output('step2', 'output2'), '2021-01-01', false)"
            };

            public override Func<Function> CreateInstance => () => new LessThanOrEqualFunction();
        }
    }
}
