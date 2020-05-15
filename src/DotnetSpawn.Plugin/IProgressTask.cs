namespace DotnetSpawn.Plugin
{
    /// <summary>
    /// Tracks progress of a specific task.
    /// </summary>
    public interface IProgressTask
    {
        /// <summary>
        /// Gets or updates the description of the task.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets or updates the max value of the task's progress.
        /// </summary>
        double MaxValue { get; set; }

        /// <summary>
        /// Gets or updates the value of the task's progress.
        /// </summary>
        double Value { get; set; }

        /// <summary>
        /// Increments the task's progress value.
        /// </summary>
        /// <param name="increment">The increment.</param>
        void IncrementValue(double increment);

        /// <summary>
        /// Marks the task as completed.
        /// </summary>
        /// <remarks>
        /// The task is also marked as completed when <see cref="Value"/>
        /// is greater than or equal to <see cref="MaxValue"/>.
        /// </remarks>
        void MarkAsCompleted();
    }
}