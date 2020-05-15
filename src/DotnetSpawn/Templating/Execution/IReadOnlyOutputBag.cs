namespace DotnetSpawn.Templating.Execution
{
    internal interface IReadOnlyOutputBag
    {
        object Get(string stepQualifier, string outputName);
    }
}
