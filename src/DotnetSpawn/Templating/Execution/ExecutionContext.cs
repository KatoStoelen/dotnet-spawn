namespace DotnetSpawn.Templating.Execution
{
    internal class ExecutionContext : IExecutionContext
    {
        private readonly VariableBag _variableBag;
        private readonly OutputBag _outputBag;

        public ExecutionContext()
        {
            _variableBag = new VariableBag();
            _outputBag = new OutputBag();
        }

        public IReadOnlyVariableBag Variable => _variableBag;

        public IReadOnlyOutputBag Output => _outputBag;

        public void SetVariable(string name, object value)
        {
            _variableBag.Set(name, value);
        }

        public void SetOutputs(string stepQualifier, Dictionary<string, object> outputs)
        {
            _outputBag.Set(stepQualifier, outputs);
        }
    }
}
