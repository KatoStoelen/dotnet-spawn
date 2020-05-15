namespace DotnetSpawn.Plugin
{
    /// <summary>
    /// An option of a select prompt.
    /// </summary>
    /// <typeparam name="TValue">The type returned when this option is selected.</typeparam>
    public class PromptOption<TValue>
    {
        /// <summary>
        /// Creates a new option.
        /// </summary>
        /// <param name="value">The value to be returned if this option is selected.</param>
        /// <param name="text">The text displayed to the user for this option.</param>
        public PromptOption(TValue value, string text)
        {
            Value = value;
            Text = text;
        }

        /// <summary>
        /// The value to be returned if this option is selected.
        /// </summary>
        public TValue Value { get; }

        /// <summary>
        /// The text displayed to the user for this option.
        /// </summary>
        public string Text { get; }

        /// <inheritdoc/>
        public override string ToString() => Text;
    }
}