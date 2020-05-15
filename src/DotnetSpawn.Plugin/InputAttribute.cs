using System;

namespace DotnetSpawn.Plugin
{
    /// <summary>
    /// Attribute specifying that a property is a spawn point input.
    /// </summary>
    /// <remarks>
    /// Input properties without this attribute can still be set via
    /// spawn templates, but they won't be visible in the spawn point
    /// details CLI output.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class InputAttribute : Attribute
    {
        /// <summary>
        /// Specifies that this property is an input.
        /// </summary>
        /// <remarks>
        /// Whether the input is required or not is just for user information.
        /// No validation of the inputs are performed before the spawn point
        /// is invoked.
        /// </remarks>
        /// <param name="description">The description of the input.</param>
        /// <param name="required">Whether or not the input is required.</param>
        public InputAttribute(string description, bool required)
        {
            Description = description;
            Required = required;
        }

        /// <summary>
        /// The description of the input.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Whether or not this input is required.
        /// </summary>
        public bool Required { get; }
    }
}