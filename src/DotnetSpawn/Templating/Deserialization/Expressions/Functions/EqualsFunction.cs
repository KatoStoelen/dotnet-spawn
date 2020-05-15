using System.Text;

namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal class EqualsFunction : EqualityFunction
    {
        private EqualsFunction() { }

        protected override object Invoke(object[] args)
        {
            return IsEqual(args);
        }

        public class Metadata : EqualityFunctionMetadata
        {
            public override string Name => "eq";

            public override string Description =>
                "Returns [teal]true[/] if the specified arguments are equal, " +
                "otherwise [teal]false[/].";

            public override IEnumerable<string> Examples => new[]
            {
                "eq(output('step1', 'output1'), 'xyz')",
                "eq(output('step2', 'output2'), 1337)",
                "eq(output('step3', 'output3'), 'abc', false)"
            };

            public override Func<Function> CreateInstance => () => new EqualsFunction();

            public override string Remarks =>
                new StringBuilder()
                    .AppendLine(base.Remarks)
                    .AppendLine()
                    .Append(
                        "The arguments are also considered equal if both are [teal]null[/].")
                    .ToString();
        }
    }
}
