using DotnetSpawn.Templating.Execution;

namespace DotnetSpawn.Templating
{
    internal interface ISpawnPointInputs
    {
        object GetInstance(IExecutionContext executionContext);
    }
}
