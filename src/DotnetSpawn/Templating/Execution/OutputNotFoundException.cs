namespace DotnetSpawn.Templating.Execution
{
    internal class OutputNotFoundException : Exception
    {
        public OutputNotFoundException(string stepQualifier, string outputName)
            : base($"Step '{stepQualifier}' does not produce an output named '{outputName}'")
        {
            StepQualifier = stepQualifier;
            OutputName = outputName;
        }

        public string StepQualifier { get; }
        public string OutputName { get; }
    }
}
