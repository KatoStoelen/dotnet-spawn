using System.Collections;
using System.ComponentModel;
using System.Reflection;
using DotnetSpawn.Plugin;

namespace DotnetSpawn.Plugins
{
    internal class SpawnPointMetadata
    {
        private readonly Lazy<Type> _inputsType;
        private readonly Lazy<SpawnPointFullyQualifiedName> _fqn;
        private readonly Lazy<string> _description;
        private readonly Lazy<InputMetadata> _inputs;
        private readonly Lazy<IReadOnlyCollection<OutputMetadata>> _outputs;

        public SpawnPointMetadata(SpawnPointType spawnPointType)
        {
            Type = spawnPointType;

            _inputsType = new Lazy<Type>(() => GetInputsType(spawnPointType.Type));
            _fqn = new Lazy<SpawnPointFullyQualifiedName>(() => GetSpawnPointFqn(spawnPointType));
            _description = new Lazy<string>(() => GetSpawnPointDescription(spawnPointType.Type));
            _inputs = new Lazy<InputMetadata>(
                () => new InputMetadata(InputsType));
            _outputs = new Lazy<IReadOnlyCollection<OutputMetadata>>(
                () => OutputMetadata.GetOutputMetadatas(spawnPointType.Type));
        }

        public SpawnPointType Type { get; }
        public SpawnPointFullyQualifiedName Fqn => _fqn.Value;
        public string Description => _description.Value;
        public Type InputsType => _inputsType.Value;
        public bool HasInputs => InputsType != null;
        public InputMetadata Inputs => _inputs.Value;
        public bool HasOutput => Outputs.Any();
        public IReadOnlyCollection<OutputMetadata> Outputs => _outputs.Value;

        public override string ToString()
        {
            return Fqn.ToString();
        }

        private static Type GetInputsType(Type spawnPointType)
        {
            return spawnPointType
                .GetInterfaces()
                .Where(iface =>
                    iface.IsGenericType &&
                    iface.GetGenericTypeDefinition() == typeof(ISpawnPoint<>))
                .SingleOrDefault()?.GetGenericArguments().Single();
        }

        private static SpawnPointFullyQualifiedName GetSpawnPointFqn(
            SpawnPointType spawnPointType)
        {
            var id = spawnPointType.Type
                .GetCustomAttribute<SpawnPointIdAttribute>()?.Id;

            return new SpawnPointFullyQualifiedName(
                spawnPointType.Plugin.Name,
                spawnPointType.Plugin.Version.ToString(),
                id);
        }

        private static string GetSpawnPointDescription(Type spawnPointType)
        {
            return spawnPointType
                .GetCustomAttribute<DescriptionAttribute>()?.Description;
        }

        public class OutputMetadata
        {
            public OutputMetadata(PropertyInfo property, OutputAttribute outputAttribute)
            {
                Name = outputAttribute.OutputName;
                Type = property.PropertyType;
                Description = outputAttribute.Description;
            }

            public string Name { get; }
            public Type Type { get; }
            public string Description { get; }

            public override string ToString()
            {
                return $"{Name} [{Type.FullName}]";
            }

            public static IReadOnlyCollection<OutputMetadata> GetOutputMetadatas(Type type)
            {
                return type
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Select(property =>
                        (
                            Property: property,
                            Attr: property.GetCustomAttribute<OutputAttribute>()
                        ))
                    .Where(tuple => tuple.Attr != null)
                    .Select(tuple => new OutputMetadata(tuple.Property, tuple.Attr))
                    .ToList();
            }
        }

        public class InputMetadata
        {
            private InputMetadata(PropertyInfo property)
            {
                // TODO: Use System.ComponentModel attributes
                var inputAttribute = property.GetCustomAttribute<InputAttribute>();

                Name = property.Name;
                Type = property.PropertyType;
                Description = inputAttribute?.Description;
                Required = inputAttribute?.Required ?? false;
                (KeyType, ElementType) = GetElementType(property.PropertyType);
                NestedInputs = GetInputMetadatas(ElementType ?? Type);
            }

            public InputMetadata(Type type)
            {
                Type = type;
                (KeyType, ElementType) = GetElementType(type);
                NestedInputs = GetInputMetadatas(ElementType ?? Type);
                Description = string.Empty;
            }

            public string Name { get; }
            public Type Type { get; }
            public string Description { get; }
            public bool Required { get; }
            public Type KeyType { get; }
            public Type ElementType { get; }
            public IReadOnlyCollection<InputMetadata> NestedInputs { get; }

            public override string ToString()
            {
                if (string.IsNullOrEmpty(Name))
                {
                    return $"[{Type.FullName}]";
                }

                return $"{Name} [{Type.FullName}]";
            }

            public InputMetadata GetPropertyMetadata(string propertyPath)
            {
                if (propertyPath == null)
                {
                    throw new ArgumentException("Property path not set", nameof(propertyPath));
                }

                if (propertyPath.Length == 0)
                {
                    return this;
                }

                var nested = this;
                var path = propertyPath.Split('.', StringSplitOptions.RemoveEmptyEntries);

                foreach (var propertyName in path)
                {
                    var next = nested.NestedInputs
                        .SingleOrDefault(meta =>
                            meta.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

                    nested = next ?? throw new MissingMemberException(
                        $"Type {nested.Type.FullName} does not contain property '{propertyName}'");
                }

                return nested;
            }

            private static (Type KeyType, Type ElementType) GetElementType(Type type)
            {
                if (type.IsArray)
                {
                    return (null, type.GetElementType());
                }

                var interfaces = type.GetInterfaces();

                var genericDictionaryInterface = interfaces
                    .SingleOrDefault(@interface =>
                        @interface.IsGenericType &&
                        @interface.GetGenericTypeDefinition() == typeof(IDictionary<,>));

                if (genericDictionaryInterface != null)
                {
                    return (
                        genericDictionaryInterface.GenericTypeArguments[0],
                        genericDictionaryInterface.GenericTypeArguments[1]
                    );
                }

                if (type.IsAssignableTo(typeof(IDictionary)))
                {
                    return (typeof(object), typeof(object));
                }

                var genericEnumerableInterface = interfaces
                    .SingleOrDefault(@interface =>
                        @interface.IsGenericType &&
                        @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                if (genericEnumerableInterface != null)
                {
                    return (null, genericEnumerableInterface.GenericTypeArguments[0]);
                }

                if (type.IsAssignableTo(typeof(IEnumerable)))
                {
                    return (null, typeof(object));
                }

                return (null, null);
            }

            private static IReadOnlyCollection<InputMetadata> GetInputMetadatas(Type type)
            {
                if (!type.IsClass ||
                    type == typeof(string) ||
                    type.IsArray ||
                    type.IsAssignableTo(typeof(IEnumerable)))
                {
                    return Array.Empty<InputMetadata>();
                }

                return type
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(property =>
                        property.GetMethod?.IsPublic == true &&
                        property.SetMethod?.IsPublic == true)
                    .Select(property => new InputMetadata(property))
                    .ToList();
            }
        }
    }
}