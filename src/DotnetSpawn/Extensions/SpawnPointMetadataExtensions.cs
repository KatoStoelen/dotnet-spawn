using DotnetSpawn.Plugins;

namespace DotnetSpawn.Extensions
{
    internal static class SpawnPointMetadataExtensions
    {
        public static ILookup<SpawnPointFullyQualifiedName, string> GetValidQualifiersPerSpawnPoint(
            this IReadOnlyCollection<SpawnPointMetadata> metadatas)
        {
            var allFqns = metadatas.Select(metadata => metadata.Fqn).ToList();

            return metadatas
                .Select(metadata => (metadata.Fqn, Qualifiers: metadata.Fqn.GetValidQualifiers(allFqns)))
                .SelectMany(tuple => tuple.Qualifiers.Select(qualifier => (tuple.Fqn, Qualifier: qualifier)))
                .ToLookup(
                    tuple => tuple.Fqn,
                    tuple => tuple.Qualifier);
        }
    }
}