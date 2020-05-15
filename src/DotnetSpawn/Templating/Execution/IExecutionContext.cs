namespace DotnetSpawn.Templating.Execution
{
    internal interface IExecutionContext
    {
        IReadOnlyVariableBag Variable { get; }
        IReadOnlyOutputBag Output { get; }
    }
}