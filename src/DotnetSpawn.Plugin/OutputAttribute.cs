using System;
using System.Runtime.CompilerServices;

namespace DotnetSpawn.Plugin
{
    /// <summary>
    /// Attribute for spawn point outputs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class OutputAttribute : Attribute
    {
        /// <summary>
        /// Specifies that this property is an output of this spawn point.
        /// <para>
        /// Outputs can be used as input to subsequent spawn steps and are
        /// displayed in the spawn summary.
        /// </para>
        /// </summary>
        /// <param name="description">The description of the output.</param>
        /// <param name="outputName">
        /// The name of the output.
        /// 
        /// <para>
        /// Defaults to the output property name.
        /// </para>
        /// </param>
        public OutputAttribute(string description, [CallerMemberName] string outputName = "")
        {
            Description = description;
            OutputName = outputName;
        }

        /// <summary>
        /// The name of the output.
        /// </summary>
        public string OutputName { get; }

        /// <summary>
        /// The description of the output.
        /// </summary>
        public string Description { get; }
    }
}