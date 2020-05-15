using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using DotnetSpawn.Plugin;

namespace DotnetSpawn.Plugins
{
    internal class SpawnPointMetadata
    {
        private readonly Lazy<Type> _inputsType;
        private readonly Lazy<SpawnPointFullyQualifiedName> _fqn;
        private readonly Lazy<string> _description;
        private readonly Lazy<IReadOnlyCollection<InputMetadata>> _inputs;
        private readonly Lazy<IReadOnlyCollection<OutputMetadata>> _outputs;

        public SpawnPointMetadata(SpawnPointType spawnPointType)
        {
            Type = spawnPointType;

            _inputsType = new Lazy<Type>(() => GetInputsType(spawnPointType.Type));
            _fqn = new Lazy<SpawnPointFullyQualifiedName>(() => GetSpawnPointFqn(spawnPointType));
            _description = new Lazy<string>(() => GetSpawnPointDescription(spawnPointType.Type));
            _inputs = new Lazy<IReadOnlyCollection<InputMetadata>>(
                () => GetInputsMetadata(_inputsType.Value));
            _outputs = new Lazy<IReadOnlyCollection<OutputMetadata>>(
                () => GetOuputsMetadata(spawnPointType.Type));
        }

        public SpawnPointType Type { get; }
        public SpawnPointFullyQualifiedName Fqn => _fqn.Value;
        public string Description => _description.Value;
        public bool HasInputs => _inputsType.Value != null;
        public IReadOnlyCollection<InputMetadata> Inputs => _inputs.Value;
        public bool HasOutput => Outputs.Any();
        public IReadOnlyCollection<OutputMetadata> Outputs => _outputs.Value;

        private static Type GetInputsType(Type spawnPointType)
        {
            return spawnPointType
                .GetInterfaces()
                .Where(iface =>
                    iface.IsGenericType &&
                    iface.GetGenericTypeDefinition() == typeof(ISpawnPoint<>))
                .SingleOrDefault()?
                .GetGenericArguments()
                .Single();
        }

        private static SpawnPointFullyQualifiedName GetSpawnPointFqn(
            SpawnPointType spawnPointType)
        {
            var id = spawnPointType.Type
                .GetCustomAttribute<SpawnPointIdAttribute>()?
                .Id;

            return new SpawnPointFullyQualifiedName(
                spawnPointType.Plugin.Name,
                spawnPointType.Plugin.Version.ToString(),
                id);
        }

        private static string GetSpawnPointDescription(Type spawnPointType)
        {
            return spawnPointType
                .GetCustomAttribute<DescriptionAttribute>()?
                .Description;
        }

        private static IReadOnlyCollection<InputMetadata> GetInputsMetadata(Type inputsType)
        {
            return inputsType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property =>
                    (
                        Property: property,
                        Attr: property.GetCustomAttribute<InputAttribute>()
                    ))
                .Where(tuple => tuple.Attr != null)
                .Select(tuple => new InputMetadata
                {
                    Name = tuple.Property.Name,
                    Type = tuple.Property.PropertyType,
                    Description = tuple.Attr.Description,
                    Required = tuple.Attr.Required
                })
                .ToList();
        }

        private static IReadOnlyCollection<OutputMetadata> GetOuputsMetadata(Type spawnPointType)
        {
            return spawnPointType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property =>
                    (
                        Property: property,
                        Attr: property.GetCustomAttribute<OutputAttribute>()
                    ))
                .Where(tuple => tuple.Attr != null)
                .Select(tuple => new OutputMetadata
                {
                    Name = tuple.Attr.OutputName,
                    Type = tuple.Property.PropertyType,
                    Description = tuple.Attr.Description
                })
                .ToList();
        }

        public class OutputMetadata
        {
            public string Name { get; init; }
            public Type Type { get; init; }
            public string Description { get; init; }
        }

        public class InputMetadata
        {
            public string Name { get; init; }
            public Type Type { get; set; }
            public string Description { get; init; }
            public bool Required { get; init; }
        }
    }
}