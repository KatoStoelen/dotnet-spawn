using DotnetSpawn.Plugin;

namespace DotnetSpawn.Extensions
{
    internal static class TypeExtensions
    {
        private static readonly Dictionary<Type, string> s_typeKeywordMap = new()
        {
            [typeof(bool)] = "bool",
            [typeof(byte)] = "byte",
            [typeof(sbyte)] = "sbyte",
            [typeof(char)] = "char",
            [typeof(decimal)] = "decimal",
            [typeof(double)] = "double",
            [typeof(float)] = "float",
            [typeof(int)] = "int",
            [typeof(uint)] = "uint",
            [typeof(nint)] = "nint",
            [typeof(nuint)] = "nuint",
            [typeof(long)] = "long",
            [typeof(ulong)] = "ulong",
            [typeof(short)] = "short",
            [typeof(ushort)] = "ushort",
            [typeof(object)] = "object",
            [typeof(string)] = "string"
        };

        private static readonly HashSet<Type> s_integerTypes = new()
        {
            typeof(int),
            typeof(uint),
            typeof(short),
            typeof(byte),
            typeof(sbyte),
            typeof(long),
            typeof(ulong)
        };

        private static readonly HashSet<Type> s_floatingPointTypes = new()
        {
            typeof(double),
            typeof(decimal),
            typeof(float)
        };

        public static bool IsSpawnPointImplementation(this Type type)
        {
            return
                type.IsClass &&
                !type.IsAbstract && (
                    typeof(ISpawnPoint).IsAssignableFrom(type) ||
                    type.IsSpawnPointWithInputs()
                );
        }

        public static bool IsSpawnPointWithInputs(this Type type)
        {
            return type
                .GetInterfaces()
                .Any(@interface =>
                    @interface.IsGenericType &&
                    @interface.GetGenericTypeDefinition() == typeof(ISpawnPoint<>));
        }

        public static bool IsIntegerType(this Type type)
        {
            return
                s_integerTypes.Contains(type) ||
                s_integerTypes.Contains(Nullable.GetUnderlyingType(type));
        }

        public static bool IsFloatingPointType(this Type type)
        {
            return
                s_floatingPointTypes.Contains(type) ||
                s_floatingPointTypes.Contains(Nullable.GetUnderlyingType(type));
        }

        public static bool IsNullable(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public static string ToPrettyName(this Type type)
        {
            if (s_typeKeywordMap.ContainsKey(type))
            {
                return s_typeKeywordMap[type];
            }

            var dictionaryPrettyName = type.GetDictionaryPrettyName();
            if (dictionaryPrettyName is not null)
            {
                return dictionaryPrettyName;
            }

            var enumerablePrettyName = type.GetEnumerablePrettyName();
            if (enumerablePrettyName is not null)
            {
                return enumerablePrettyName;
            }

            return type.Name;
        }

        private static string GetDictionaryPrettyName(this Type type)
        {
            var genericDictionaryInterfaceType = type.GetInterfaces()
                .FirstOrDefault(iface =>
                    iface.IsGenericType &&
                    iface.GetGenericTypeDefinition() == typeof(IDictionary<,>));

            if (genericDictionaryInterfaceType is not null)
            {
                var keyType = genericDictionaryInterfaceType.GenericTypeArguments[0];
                var valueType = genericDictionaryInterfaceType.GenericTypeArguments[1];

                return $"Dictionary<{keyType.ToPrettyName()}, {valueType.ToPrettyName()}>";
            }

            return null;
        }

        private static string GetEnumerablePrettyName(this Type type)
        {
            var enumerableInterface =
                type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    ? type
                    : type.GetInterfaces()
                        .FirstOrDefault(iface =>
                            iface.IsGenericType &&
                            iface.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (enumerableInterface is not null)
            {
                var elementType = type.GenericTypeArguments[0];

                return $"{elementType.ToPrettyName()}[]";
            }

            return null;
        }
    }
}

