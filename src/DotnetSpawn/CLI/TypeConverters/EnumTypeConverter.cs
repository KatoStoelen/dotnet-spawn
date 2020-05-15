using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace DotnetSpawn.Cli.TypeConverters
{
    internal class EnumTypeConverter<TEnum> : TypeConverter where TEnum : struct, Enum
    {
        public override object ConvertFrom(
            ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is not string stringValue)
            {
                throw new NotSupportedException(
                    $"Cannot convert value of type {value.GetType().FullName} to {typeof(TEnum).Name}");
            }

            // Exact match
            if (Enum.TryParse<TEnum>(stringValue, ignoreCase: true, out var enumValue))
            {
                return enumValue;
            }

            // Alias match
            var enumValues = Enum.GetValues<TEnum>();

            var aliasLookup = enumValues
                .ToLookup(
                    value => value,
                    value => typeof(TEnum)
                        .GetMember(value.ToString())
                        .Single()
                        .GetCustomAttributes<EnumAliasAttribute>()
                        .Select(attr => attr.Alias)
                        .ToList());

            var aliasToValueMapping = aliasLookup
                .Where(group => group.Any())
                .SelectMany(group =>
                    group.SelectMany(aliases =>
                        aliases.Select(alias => (Alias: alias, group.Key))))
                .ToImmutableDictionary(
                    tuple => tuple.Alias,
                    tuple => tuple.Key,
                    StringComparer.OrdinalIgnoreCase);

            if (aliasToValueMapping.ContainsKey(stringValue))
            {
                return aliasToValueMapping[stringValue];
            }

            throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    $"The input '{stringValue}' is not a valid '{typeof(TEnum).Name}' " +
                    $"(expected one of {BuildExpectedValuesString(enumValues, aliasLookup)})");
        }

        private static string BuildExpectedValuesString(
            TEnum[] enumValues, ILookup<TEnum, List<string>> aliasLookup)
        {
            return string.Join(", ", enumValues.Select(value =>
            {
                var expectedValue = value.ToString();
                var aliases = aliasLookup[value];

                if (aliases.Any())
                {
                    expectedValue += $" ({string.Join(", ", aliases)})";
                }

                return expectedValue;
            }));
        }
    }
}