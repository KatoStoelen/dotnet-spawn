using System.ComponentModel;
using System.Globalization;

namespace DotnetSpawn.Cli.TypeConverters
{
    internal class FileInfoTypeConverter : TypeConverter
    {
        public override object ConvertFrom(
            ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is not string filePath)
            {
                throw new NotSupportedException(
                    $"Cannot convert value of type {value.GetType().FullName} to {nameof(FileInfo)}");
            }

            return new FileInfo(filePath);
        }
    }
}