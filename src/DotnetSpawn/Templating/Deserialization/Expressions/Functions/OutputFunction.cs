using DotnetSpawn.Templating.Deserialization.Expressions.Functions.Metadata;
using DotnetSpawn.Templating.Execution;

namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal class OutputFunction : ContextualFunction<string>
    {
        private OutputFunction()
        {
            MinimumArgumentCount = 2;
            MaximumArgumentCount = 2;
        }

        protected override object Invoke(IExecutionContext context, string[] args)
        {
            var stepQualifier = args[0];
            var outputName = args[1];

            try
            {
                return context.Output.Get(stepQualifier, outputName);
            }
            catch (StepNotFoundException snf)
            {
                throw InvalidArgument(snf.Message, argumentIndex: 0);
            }
            catch (OutputNotFoundException onf)
            {
                throw InvalidArgument(onf.Message, argumentIndex: 1);
            }
        }

        public class Metadata : FunctionMetadata
        {
            public override string Name => "output";

            public override string Description =>
                "Returns the specified output from a preceding spawn step.";

            public override Type ReturnType => typeof(object);

            public override IReadOnlyList<ParameterMetadata> Parameters => new[]
            {
                new ParameterMetadata
                {
                    Name = "step",
                    Description = "The name or index (zero based) of the step producing the output.",
                    Type = typeof(string)
                },
                new ParameterMetadata
                {
                    Name = "name",
                    Description = "The name of the output.",
                    Type = typeof(string)
                }
            };

            public override IEnumerable<string> Examples => new[]
            {
                "output('step1', 'output1')",
                "output(2, 'output2')"
            };

            public override Func<Function> CreateInstance => () => new OutputFunction();

            public override string Remarks =>
                "When referencing a subsequent step (i.e. the output has not yet been produced), " +
                "an exception will be thrown.";
        }
    }
}
