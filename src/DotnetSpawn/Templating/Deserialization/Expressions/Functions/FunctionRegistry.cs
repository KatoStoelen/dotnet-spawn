using DotnetSpawn.Templating.Deserialization.Expressions.Functions.Metadata;

namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal static class FunctionRegistry
    {
        public static IReadOnlyCollection<FunctionMetadata> Metadata { get; }

        static FunctionRegistry()
        {
            Metadata = typeof(FunctionMetadata).Assembly
                .GetTypes()
                .Where(type =>
                    type.IsClass &&
                    !type.IsAbstract &&
                    type.IsAssignableTo(typeof(FunctionMetadata)))
                .Select(metadataType => (FunctionMetadata)Activator.CreateInstance(metadataType))
                .OrderBy(metadata => metadata.Name)
                .ToList()
                .AsReadOnly();
        }

        public static bool TryGetMetadata(string functionName, out FunctionMetadata metadata)
        {
            metadata = Metadata
                .SingleOrDefault(
                    func => func.Name.Equals(functionName, StringComparison.OrdinalIgnoreCase));

            return metadata is not null;
        }

        public static bool TryGet(string functionName, out Function function)
        {
            if (TryGetMetadata(functionName, out var functionMetadata))
            {
                function = functionMetadata.CreateInstance();

                return true;
            }

            function = null;

            return false;
        }
    }
}
