namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions.Metadata
{
    internal class ParameterMetadata
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public Type Type { get; init; }
        public bool IsOptional { get; init; }
        public object DefaultValue { get; init; }
        public bool IsParams { get; init; }
    }
}
