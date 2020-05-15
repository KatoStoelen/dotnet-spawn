namespace DotnetSpawn.Plugin
{
    /// <summary>
    /// Represents a result of a spawn operation.
    /// </summary>
    public class SpawnResult
    {
        private protected SpawnResult() { }

        /// <summary>
        /// Indicates that the spawn operation was completed successfully.
        /// </summary>
        /// <returns>A successful <see cref="SpawnResult"/>.</returns>
        public static SpawnResult Success() => new SuccessResult();

        /// <summary>
        /// Indicates that the spawn operation failed.
        /// </summary>
        /// <param name="reason">An optional reason why the operation failed.</param>
        /// <returns>A failed <see cref="SpawnResult"/>.</returns>
        public static SpawnResult Fail(string? reason) => new FailResult(reason);

        internal sealed class SuccessResult : SpawnResult
        {
            internal SuccessResult()
            {
            }
        }

        internal sealed class FailResult : SpawnResult
        {
            internal FailResult(string? reason)
            {
                Reason = reason;
            }

            internal string? Reason { get; }
        }
    }
}