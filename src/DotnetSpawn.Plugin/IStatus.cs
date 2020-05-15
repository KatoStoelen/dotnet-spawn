namespace DotnetSpawn.Plugin
{
    /// <summary>
    /// Represents a status message for a long running task.
    /// </summary>
    public interface IStatus
    {
        /// <summary>
        /// Updates the status message.
        /// </summary>
        /// <param name="newStatus">The new status message.</param>
        void UpdateStatus(string newStatus);
    }
}