using System.ComponentModel;
using System.Globalization;

namespace DotnetSpawn.Cli.TypeConverters
{
    internal class DirectoryInfoTypeConverter : TypeConverter
    {
        public override object ConvertFrom(
            ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is not string directoryPath)
            {
                throw new NotSupportedException(
                    $"Cannot convert value of type {value.GetType().FullName} to {nameof(DirectoryInfo)}");
            }

            return new DirectoryInfo(directoryPath);
        }
    }
}
