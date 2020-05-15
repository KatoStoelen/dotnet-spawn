namespace DotnetSpawn.Plugin
{
    /// <summary>
    /// Tracks progress of one or more tasks.
    /// </summary>
    public interface IProgress
    {
        /// <summary>
        /// Adds a task to track progress of.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        IProgressTask AddTask(string description, double maxValue = 100D);
    }
}
