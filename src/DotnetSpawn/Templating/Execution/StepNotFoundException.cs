namespace DotnetSpawn.Templating.Execution
{
    internal class StepNotFoundException : Exception
    {
        public StepNotFoundException(string stepQualifier)
            : base($"Step '{stepQualifier}' not found")
        {
            StepQualifier = stepQualifier;
        }

        public string StepQualifier { get; }
    }
}