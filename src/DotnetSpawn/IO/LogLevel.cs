using DotnetSpawn.Cli.TypeConverters;

namespace DotnetSpawn.IO
{
    /// <summary>
    /// Log levels.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Trace log level.
        /// </summary>
        [EnumAlias("trce")]
        Trace = 0,

        /// <summary>
        /// Debug log level.
        /// </summary>
        [EnumAlias("dbug")]
        Debug = 1,

        /// <summary>
        /// Information log level.
        /// </summary>
        [EnumAlias("info")]
        Information = 2,

        /// <summary>
        /// Warning log level.
        /// </summary>
        [EnumAlias("warn")]
        Warning = 3,

        /// <summary>
        /// Error log level.
        /// </summary>
        [EnumAlias("fail")]
        Error = 4,

        /// <summary>
        /// Supress all logs.
        /// </summary>
        [EnumAlias("none")]
        None = int.MaxValue
    }
}