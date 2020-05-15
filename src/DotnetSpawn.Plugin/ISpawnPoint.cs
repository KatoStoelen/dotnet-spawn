using System.Threading;
using System.Threading.Tasks;

namespace DotnetSpawn.Plugin
{
    /// <summary>
    /// Represents a spawn point without the need for user input.
    /// </summary>
    /// <remarks>
    /// To create a spawn point accepting user input, see <see cref="ISpawnPoint{TInputs}"/>.
    /// </remarks>
    public interface ISpawnPoint
    {
        /// <summary>
        /// Executes the spawn operation.
        /// </summary>
        /// <param name="console">
        /// Access to the console for logging, progress/status displays and
        /// input prompting purposes.
        /// </param>
        /// <param name="cancellationToken">A token to track for cancellation request.</param>
        /// <returns>Either <see cref="SpawnResult.Success"/> or <see cref="SpawnResult.Fail(string?)"/>.</returns>
        Task<SpawnResult> SpawnAsync(IConsole console, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Represents a spawn point accepting user inputs.
    /// </summary>
    /// <remarks>
    /// The <typeparamref name="TInputs"/> class must be deserializable from
    /// JSON. 
    /// </remarks>
    /// <typeparam name="TInputs">
    /// The class containing the available user inputs for this spawn point.
    /// </typeparam>
    public interface ISpawnPoint<TInputs> where TInputs : class
    {
        /// <summary>
        /// Executes the spawn operation.
        /// </summary>
        /// <param name="inputs">The inputs of this spawn point.</param>
        /// <param name="console">
        /// Access to the console for logging, progress/status displays and
        /// input prompting purposes.
        /// </param>
        /// <param name="cancellationToken">A token to track for cancellation request.</param>
        /// <returns>Either <see cref="SpawnResult.Success"/> or <see cref="SpawnResult.Fail(string?)"/>.</returns>
        Task<SpawnResult> SpawnAsync(TInputs inputs, IConsole console, CancellationToken cancellationToken);
    }
}