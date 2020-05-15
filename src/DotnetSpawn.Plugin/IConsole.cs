using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetSpawn.Plugin
{
    /// <summary>
    /// Provides a way for plugins to interact with the console (logging,
    /// status/progress indicators of long running tasks and input prompts etc.).
    /// </summary>
    public interface IConsole
    {
        /// <summary>
        /// Logs a trace message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void LogTrace(RawString message);

        /// <summary>
        /// Logs a trace message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void LogTrace(FormattableString message);

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void LogDebug(RawString message);

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void LogDebug(FormattableString message);

        /// <summary>
        /// Logs a information message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void LogInformation(RawString message);

        /// <summary>
        /// Logs a information message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void LogInformation(FormattableString message);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void LogWarning(RawString message);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void LogWarning(FormattableString message);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">
        /// An optional exception providing more detailed error information.
        /// </param>
        void LogError(RawString message, Exception? exception = null);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">
        /// An optional exception providing more detailed error information.
        /// </param>
        void LogError(FormattableString message, Exception? exception = null);

        /// <summary>
        /// Prompts the user for confirmaton.
        /// </summary>
        /// <param name="message">The message of the prompt.</param>
        /// <returns>
        /// <see langword="true"/> if the user confirmed, otherwise
        /// <see langword="false"/>.
        /// </returns>
        bool Confirm(string message);

        /// <summary>
        /// Propts the user for input.
        /// </summary>
        /// <param name="message">The message of the prompt.</param>
        /// <param name="isSecret">
        /// Whether or not the input is secret.
        /// 
        /// <para>
        /// When set to <see langword="true"/>, typed characters are not
        /// displayed in the console.
        /// </para>
        /// </param>
        /// <param name="allowEmpty">
        /// Whether or not to allow an empty value.
        /// </param>
        /// <typeparam name="TInput">
        /// A type to parse the input to. The specified type must be
        /// convertible from a string.
        /// </typeparam>
        /// <returns>The user input.</returns>
        TInput Prompt<TInput>(string message, bool isSecret = false, bool allowEmpty = false);

        /// <summary>
        /// Prompts the user to select one of several options.
        /// </summary>
        /// <param name="options">The options of the prompt.</param>
        /// <param name="message">The message of the prompt.</param>
        /// <typeparam name="TInput">The type of the options' value.</typeparam>
        /// <returns>The selected option.</returns>
        TInput Prompt<TInput>(IEnumerable<PromptOption<TInput>> options, string message);

        /// <summary>
        /// Prompts the user to select one or more of several options.
        /// </summary>
        /// <param name="options">The options of the prompt.</param>
        /// <param name="message">The message of the prompt.</param>
        /// <typeparam name="TInput">The type of the options' value.</typeparam>
        /// <returns>The selected options.</returns>
        IEnumerable<TInput> PromptMultiple<TInput>(
            IEnumerable<PromptOption<TInput>> options, string message);

        /// <summary>
        /// Displays a progress bar of one or more long running tasks.
        /// </summary>
        /// <param name="action">
        /// A delegate, accepting an <see cref="IProgress"/>, executing
        /// the long running task(s).
        /// </param>
        void DisplayProgress(Action<IProgress> action);

        /// <summary>
        /// Displays a progress bar of one or more long running tasks.
        /// </summary>
        /// <param name="action">
        /// A delegate, accepting an <see cref="IProgress"/>, that executes
        /// the long running task(s) and returns a value of type
        /// <typeparamref name="TReturn"/>.
        /// </param>
        /// <typeparam name="TReturn">The return type of the specified delegate.</typeparam>
        /// <returns>The returned value from the specified delegate.</returns>
        TReturn DisplayProgress<TReturn>(Func<IProgress, TReturn> action);

        /// <summary>
        /// Displays a progress bar of one or more long running
        /// asynchronous tasks.
        /// </summary>
        /// <param name="action">
        /// A delegate, accepting an <see cref="IProgress"/>, that returns a
        /// <see cref="Task"/> representing the asynchronous task execution.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous task execution.
        /// </returns>
        Task DisplayProgressAsync(Func<IProgress, Task> action);

        /// <summary>
        /// Displays a progress bar of one or more long running
        /// asynchronous tasks.
        /// </summary>
        /// <param name="action">
        /// A delegate, accepting an <see cref="IProgress"/>, that returns a
        /// <see cref="Task"/> of type <typeparamref name="TReturn"/>,
        /// representing the asynchronous task execution.
        /// </param>
        /// <typeparam name="TReturn">The return type of the specified delegate.</typeparam>
        /// <returns>
        /// A <see cref="Task"/> of type <typeparamref name="TReturn"/>,
        /// representing the asynchronous task execution.
        /// </returns>
        Task<TReturn> DisplayProgressAsync<TReturn>(Func<IProgress, Task<TReturn>> action);

        /// <summary>
        /// Displays a status message while executing long running tasks.
        /// </summary>
        /// <param name="status">The initial status message.</param>
        /// <param name="action">
        /// A delegate, accepting an <see cref="IStatus"/>, executing
        /// the long running task(s).
        /// </param>
        void DisplayStatus(string status, Action<IStatus> action);

        /// <summary>
        /// Displays a status message while executing long running tasks.
        /// </summary>
        /// <param name="status">The initial status message.</param>
        /// <param name="action">
        /// A delegate, accepting an <see cref="IStatus"/>, that executes
        /// the long running task(s) and returns a value of type
        /// <typeparamref name="TReturn"/>.
        /// </param>
        /// <typeparam name="TReturn">The return type of the specified delegate.</typeparam>
        /// <returns>The returned value from the specified delegate.</returns>
        TReturn DisplayStatus<TReturn>(string status, Func<IStatus, TReturn> action);

        /// <summary>
        /// Displays a status message while executing long running
        /// asynchronous tasks.
        /// </summary>
        /// <param name="status">The initial status message.</param>
        /// <param name="action">
        /// A delegate, accepting an <see cref="IStatus"/>, that returns a
        /// <see cref="Task"/> representing the asynchronous task execution.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous task execution.
        /// </returns>
        Task DisplayStatusAsync(string status, Func<IStatus, Task> action);

        /// <summary>
        /// Displays a status message while executing long running
        /// asynchronous tasks.
        /// </summary>
        /// <param name="status">The initial status message.</param>
        /// <param name="action">
        /// A delegate, accepting an <see cref="IStatus"/>, that returns a
        /// <see cref="Task"/> representing the asynchronous task execution.
        /// </param>
        /// <typeparam name="TReturn">The return type of the specified delegate.</typeparam>
        /// <returns>
        /// A <see cref="Task"/> of type <typeparamref name="TReturn"/>,
        /// representing the asynchronous task execution.
        /// </returns>
        Task<TReturn> DisplayStatusAsync<TReturn>(
            string status, Func<IStatus, Task<TReturn>> action);
    }
}