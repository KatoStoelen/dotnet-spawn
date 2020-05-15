using System.Diagnostics;

namespace DotnetSpawn.Plugins
{
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    internal class SpawnPointFullyQualifiedName :
        IEquatable<SpawnPointFullyQualifiedName>,
        IEquatable<string>
    {
        private readonly string _pluginName;
        private readonly string _version;

        public SpawnPointFullyQualifiedName(string pluginName, string version, string name)
        {
            _pluginName = pluginName;
            _version = version;

            Name = name;
        }

        public string Name { get; }
        public string VersionedName => $"{Name}@{_version}";
        public string PluginQualifiedName => $"{_pluginName}.{Name}";
        public string FullyQualifiedName => $"{_pluginName}.{VersionedName}";

        public IEnumerable<string> Aliases
        {
            get
            {
                yield return Name;
                yield return VersionedName;
                yield return PluginQualifiedName;
                yield return FullyQualifiedName;
            }
        }

        public string GetSortestValidQualifier(IEnumerable<SpawnPointFullyQualifiedName> allFqns)
        {
            return GetValidQualifiers(allFqns).First();
        }

        public IEnumerable<string> GetValidQualifiers(IEnumerable<SpawnPointFullyQualifiedName> allFqns)
        {
            var otherFqns = allFqns
                .Where(fqn => !fqn.Equals(this))
                .ToList();

            foreach (var alias in Aliases)
            {
                if (!otherFqns.Any(fqn => fqn.Equals(alias)))
                {
                    yield return alias;
                }
            }
        }

        public override string ToString() => FullyQualifiedName;

        public override bool Equals(object obj)
        {
            return obj is SpawnPointFullyQualifiedName other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                _pluginName.ToLowerInvariant(),
                _version.ToLowerInvariant(),
                Name.ToLowerInvariant());
        }

        public bool Equals(SpawnPointFullyQualifiedName other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return
                _pluginName.Equals(other._pluginName, StringComparison.OrdinalIgnoreCase) &&
                _version.Equals(other._version, StringComparison.OrdinalIgnoreCase) &&
                Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(string @string)
        {
            if (string.IsNullOrEmpty(@string))
            {
                return false;
            }

            return Aliases
                .Any(alias => alias.Equals(@string, StringComparison.OrdinalIgnoreCase));
        }
    }
}