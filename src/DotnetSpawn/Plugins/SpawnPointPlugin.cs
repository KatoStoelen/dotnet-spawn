using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using DotnetSpawn.Extensions;
using DotnetSpawn.Plugin;
using McMaster.NETCore.Plugins;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace DotnetSpawn.Plugins
{
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    internal class SpawnPointPlugin :
        IDisposable,
        IEquatable<SpawnPointPlugin>,
        IComparable<SpawnPointPlugin>,
        IEquatable<PackageIdentity>
    {
        private readonly PluginLoader _loader;
        private readonly Lazy<Assembly> _mainAssembly;

        public SpawnPointPlugin(
            string mainAssemblyPath,
            PackageIdentity packageIdentity,
            NuspecReader nuspecReader,
            IReadOnlyCollection<NuGetFramework> supportedFrameworks,
            NuGetFramework selectedFramework)
        {
            _loader = PluginLoader.CreateFromAssemblyFile(
                mainAssemblyPath,
                isUnloadable: false,
                sharedTypes: new[]
                {
                    typeof(ISpawnPoint)
                });

            _mainAssembly = new Lazy<Assembly>(() => _loader.LoadDefaultAssembly());

            MainAssemblyFile = new FileInfo(mainAssemblyPath);
            PackageIdentity = packageIdentity;
            NuspecReader = nuspecReader;
            SupportedFrameworks = supportedFrameworks;
            SelectedFramework = selectedFramework;
        }

        private SpawnPointPlugin(
            PackageIdentity packageIdentity,
            NuspecReader nuspecReader,
            IReadOnlyCollection<NuGetFramework> supportedFrameworks)
        {
            PackageIdentity = packageIdentity;
            NuspecReader = nuspecReader;
            SupportedFrameworks = supportedFrameworks;
        }

        private SpawnPointPlugin(
                PackageIdentity packageIdentity,
                NuspecReader nuspecReader,
                IReadOnlyCollection<NuGetFramework> supportedFrameworks,
                NuGetFramework selectedFramework)
            : this(packageIdentity, nuspecReader, supportedFrameworks)
        {
            SelectedFramework = selectedFramework;
        }

        public string Name => PackageIdentity.Id;
        public NuGetVersion Version => PackageIdentity.Version;
        public PackageIdentity PackageIdentity { get; }
        public NuspecReader NuspecReader { get; }
        public IReadOnlyCollection<NuGetFramework> SupportedFrameworks { get; }
        public NuGetFramework SelectedFramework { get; }
        public bool IsCompatible => _loader != null;
        public Assembly MainAssembly => _mainAssembly?.Value;
        public FileInfo MainAssemblyFile { get; }

        public IEnumerable<SpawnPointType> GetSpawnPoints()
        {
            return MainAssembly
                .GetTypes()
                .Where(type => type.IsSpawnPointImplementation())
                .Select(type => new SpawnPointType(this, type));
        }

        public override string ToString()
        {
            return $"{Name} (v{Version})";
        }

        public void Dispose()
        {
            _loader?.Dispose();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name.ToLowerInvariant(), Version);
        }

        public override bool Equals(object obj)
        {
            return obj is SpawnPointPlugin other && Equals(other);
        }

        public bool Equals(SpawnPointPlugin other)
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
                Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase) &&
                Version.Equals(other.Version);
        }

        public bool Equals(PackageIdentity packageIdentity)
        {
            return PackageIdentity.Equals(packageIdentity);
        }

        public int CompareTo(SpawnPointPlugin other)
        {
            if (other == null)
            {
                return 1;
            }

            if (Equals(other))
            {
                return 0;
            }

            if (Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase))
            {
                return Version.CompareTo(other.Version);
            }

            return string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public static SpawnPointPlugin IncompatiblePlugin(
            PackageIdentity packageIdentity,
            NuspecReader nuspecReader,
            IReadOnlyCollection<NuGetFramework> supportedFrameworks)
        {
            return new SpawnPointPlugin(packageIdentity, nuspecReader, supportedFrameworks);
        }

        public static SpawnPointPlugin IncompatiblePlugin(
            PackageIdentity packageIdentity,
            NuspecReader nuspecReader,
            IReadOnlyCollection<NuGetFramework> supportedFrameworks,
            NuGetFramework selectedFramework)
        {
            return new SpawnPointPlugin(packageIdentity, nuspecReader, supportedFrameworks, selectedFramework);
        }
    }
}
