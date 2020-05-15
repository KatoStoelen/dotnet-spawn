using System.Globalization;

namespace DotnetSpawn.Extensions
{
    internal static class ObjectExtensions
    {
        public static string ConvertToString(this object obj)
        {
            return obj switch
            {
                null => "null",
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                IConvertible convertible => convertible.ToString(CultureInfo.InvariantCulture),
                _ => obj.ToString()
            };
        }

        public static T ConvertTo<T>(this object obj)
        {
            return obj switch
            {
                null => default,
                IConvertible convertible => (T)convertible.ToType(typeof(T), CultureInfo.InvariantCulture),
                _ => (T)obj
            };
        }
    }
}