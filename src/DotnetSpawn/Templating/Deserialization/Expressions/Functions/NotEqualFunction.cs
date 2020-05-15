namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal class NotEqualFunction : EqualityFunction
    {
        private NotEqualFunction() { }

        protected override object Invoke(object[] args)
        {
            return !IsEqual(args);
        }

        public class Metadata : EqualityFunctionMetadata
        {
            public override string Name => "ne";

            public override string Description =>
                "Returns [teal]true[/] if the specified arguments are [bold]not[/] equal, " +
                "otherwise [teal]false[/].";

            public override IEnumerable<string> Examples => new[]
            {
                "ne(output('step1', 'output1'), 'xyz')",
                "ne(output('step2', 'output2'), 1337)",
                "ne(output('step3', 'output3'), 'abc', false)"
            };

            public override Func<Function> CreateInstance => () => new NotEqualFunction();
        }
    }
}
