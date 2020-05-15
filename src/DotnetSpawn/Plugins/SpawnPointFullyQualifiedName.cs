using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetSpawn.Plugins
{
    internal class SpawnPointFullyQualifiedName :
        IEquatable<SpawnPointFullyQualifiedName>,
        IEquatable<string>
    {
        private readonly string _pluginName;
        private readonly string _version;
        private readonly string _id;

        public SpawnPointFullyQualifiedName(string pluginName, string version, string id)
        {
            _pluginName = pluginName;
            _version = version;
            _id = id;
        }

        public string IdAlias => _id;
        public string VersionedAlias => $"{IdAlias}@{_version}";
        public string Fqn => $"{_pluginName}.{VersionedAlias}";

        public string GetSortestValidQualifier(IEnumerable<SpawnPointFullyQualifiedName> allFqns)
        {
            var fqnsToCompareWith = allFqns
                .Where(fqn => !fqn.Equals(this))
                .ToList();

            if (!fqnsToCompareWith.Any(fqn => fqn.Equals(IdAlias)))
            {
                return IdAlias;
            }

            var idCollisionDueToMultipleVersionsOfSamePlugin = fqnsToCompareWith
                .Where(fqn => fqn.Equals(IdAlias))
                .All(fqn => fqn._pluginName
                    .Equals(_pluginName, StringComparison.OrdinalIgnoreCase));

            if (idCollisionDueToMultipleVersionsOfSamePlugin)
            {
                return VersionedAlias;
            }

            return Fqn;
        }

        public override string ToString() => Fqn;

        public override bool Equals(object obj)
        {
            return obj is SpawnPointFullyQualifiedName other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                _pluginName.ToLowerInvariant(),
                _version.ToLowerInvariant(),
                _id.ToLowerInvariant());
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
                _id.Equals(other._id, StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(string @string)
        {
            if (string.IsNullOrEmpty(@string))
            {
                return false;
            }

            return
                @string.Equals(IdAlias, StringComparison.OrdinalIgnoreCase) ||
                @string.Equals(VersionedAlias, StringComparison.OrdinalIgnoreCase) ||
                @string.Equals(Fqn, StringComparison.OrdinalIgnoreCase);
        }
    }
}