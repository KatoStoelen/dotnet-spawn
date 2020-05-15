namespace DotnetSpawn.Templating.Execution
{
    internal class OutputBag : IReadOnlyOutputBag
    {
        private readonly Dictionary<string, Dictionary<string, object>> _bag =
            new(StringComparer.OrdinalIgnoreCase);

        public object Get(string stepQualifier, string outputName)
        {
            if (!_bag.ContainsKey(stepQualifier))
            {
                throw new StepNotFoundException(stepQualifier);
            }

            var outputs = _bag[stepQualifier];

            if (!outputs.ContainsKey(outputName))
            {
                throw new OutputNotFoundException(stepQualifier, outputName);
            }

            return outputs[outputName];
        }

        public void Set(string stepQualifier, Dictionary<string, object> outputs)
        {
            _bag[stepQualifier] = new Dictionary<string, object>(
                outputs, StringComparer.OrdinalIgnoreCase);
        }
    }
}
