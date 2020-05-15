using System;

namespace DotnetSpawn.Plugin
{
    /// <summary>
    /// Attribute for setting the ID of the spawn point.
    /// 
    /// <para>
    /// An ID is required, and is used to reference the spawn point
    /// in a spawn template.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SpawnPointIdAttribute : Attribute
    {
        /// <summary>
        /// Sets the ID of this spawn point.
        /// </summary>
        /// <remarks>
        /// The ID is used to reference this spawn point in a spawn
        /// template. Hence, it should be as unique as possible while
        /// still being human friendly.
        /// 
        /// <para>
        /// If ID collisions occur (same spawn point ID found in
        /// multiple plugins), one could still reference the spawn
        /// points by using their fully qualified name, which includes
        /// the plugin (assembly) name.
        /// </para>
        /// </remarks>
        /// <param name="id">The ID of the spawn point.</param>
        public SpawnPointIdAttribute(string id)
        {
            Id = id;
        }

        /// <summary>
        /// The ID of the spawn point.
        /// </summary>
        public string Id { get; }
    }
}