using System;
using System.Globalization;

namespace DotnetSpawn.Plugin
{
    /// <summary>
    /// Represents a normal <see cref="string"/>, as opposed to a
    /// <see cref="FormattableString"/>.
    /// </summary>
    /// <remarks>
    /// This type is here to enable special handling of interpolated strings. By
    /// having method overloads accepting <see cref="FormattableString"/> or
    /// <see cref="RawString"/>, the overload resolution will choose the method
    /// accepting <see cref="FormattableString"/> if the argument is an
    /// interpolated string. Otherwise, the overload accepting this
    /// <see cref="RawString"/>.
    /// </remarks>
    public class RawString
    {
        private RawString(string value)
        {
            Value = value;
        }

        /// <summary>
        /// The raw string value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Implicit cast operator from <see cref="string"/>.
        /// </summary>
        public static implicit operator RawString(string value) =>
            new RawString(value);

        /// <summary>
        /// Implicit cast operator from <see cref="FormattableString"/>.
        /// </summary>
        /// <remarks>
        /// This cast operator must exist for overload resolution to work.
        /// However, it will not be used if a method overload accepting
        /// <see cref="FormattableString"/> exists.
        /// </remarks>
        public static implicit operator RawString(FormattableString value) =>
            new RawString(value.ToString(CultureInfo.CurrentCulture));
    }
}