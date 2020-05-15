using System;
using System.Linq;
using DotnetSpawn.Plugin;

namespace DotnetSpawn.Extensions
{
    internal static class TypeExtensions
    {
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
    }
}