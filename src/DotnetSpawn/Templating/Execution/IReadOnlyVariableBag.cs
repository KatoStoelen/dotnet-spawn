namespace DotnetSpawn.Templating.Execution
{
    internal interface IReadOnlyVariableBag
    {
        object Get(string variableName);
    }
}
