using DotnetSpawn.IO;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;

namespace DotnetSpawn.NuGet
{
    internal class NuGetPackageFinder
    {
        private readonly SpectreConsole _console;
        private readonly NuGetLogger _nugetLogger;

        public NuGetPackageFinder(SpectreConsole console)
        {
            _console = console;
            _nugetLogger = new NuGetLogger(console);
        }

        public Task<Result> FindAsync(
            PackageIdentity package,
            PackageSource packageSource = null,
            FileInfo nugetConfigFile = null,
            bool includePrerelease = false,
            CancellationToken cancellationToken = default)
        {
            if (packageSource != null)
            {
                return FindAsync(package, packageSource, includePrerelease, cancellationToken);
            }

            if (nugetConfigFile != null)
            {
                return FindAsync(package, nugetConfigFile, includePrerelease, cancellationToken);
            }

            return FindAsync(package, includePrerelease, cancellationToken);
        }

        private Task<Result> FindAsync(
            PackageIdentity package,
            bool includePrerelease,
            CancellationToken cancellationToken)
        {
            return FindAsync(
                package,
                Settings.LoadDefaultSettings(root: Directory.GetCurrentDirectory()),
                includePrerelease,
                cancellationToken);
        }

        private Task<Result> FindAsync(
            PackageIdentity package,
            PackageSource packageSource,
            bool includePrerelease,
            CancellationToken cancellationToken)
        {
            var sourceRepository = new SourceRepository(
                packageSource, Repository.Provider.GetCoreV3());

            _console.LogDebug($"Using package source {packageSource}");

            return FindAsync(
                package,
                new[] { sourceRepository },
                settings: null,
                includePrerelease,
                cancellationToken);
        }

        private Task<Result> FindAsync(
            PackageIdentity package,
            FileInfo nugetConfigFile,
            bool includePrerelease,
            CancellationToken cancellationToken)
        {
            if (!nugetConfigFile.Exists)
            {
                throw new FileNotFoundException($"NuGet config file '{nugetConfigFile}' not found.");
            }

            _console.LogDebug($"Using NuGet config {nugetConfigFile.FullName}");

            return FindAsync(
                package,
                Settings.LoadDefaultSettings(
                    root: nugetConfigFile.Directory.FullName,
                    nugetConfigFile.Name,
                    new XPlatMachineWideSetting()),
                includePrerelease,
                cancellationToken);
        }

        private Task<Result> FindAsync(
            PackageIdentity package,
            ISettings settings,
            bool includePrerelease,
            CancellationToken cancellationToken = default)
        {
            var sourceRepositoryProvider = new SourceRepositoryProvider(
                new PackageSourceProvider(settings),
                Repository.Provider.GetCoreV3());

            return FindAsync(
                package,
                sourceRepositoryProvider.GetRepositories(),
                settings,
                includePrerelease,
                cancellationToken);
        }

        private async Task<Result> FindAsync(
            PackageIdentity package,
            IEnumerable<SourceRepository> sourceRepositories,
            ISettings settings,
            bool includePrerelease,
            CancellationToken cancellationToken)
        {
            var cache = new SourceCacheContext();

            foreach (var repository in sourceRepositories)
            {
                _console.LogDebug($"Looking up package {package} in source {repository.PackageSource}");

                if (package.HasVersion)
                {
                    _console.LogDebug($"Package version specified. Checking if specified package and version exists.");

                    var findPackageResource = await repository
                        .GetResourceAsync<FindPackageByIdResource>(cancellationToken);

                    var packageFoundInRepository = await findPackageResource.DoesPackageExistAsync(
                        package.Id, package.Version, cache, _nugetLogger, cancellationToken);

                    if (packageFoundInRepository)
                    {
                        _console.LogDebug($"Package {package} found in source {repository.PackageSource}");

                        return new Result
                        {
                            Package = package,
                            Repository = repository,
                            Settings = settings,
                        };
                    }
                    else
                    {
                        _console.LogDebug($"Package {package} was not found in source {repository.PackageSource}");

                        continue;
                    }
                }

                _console.LogDebug($"No package version specifed. Fetching latest version (include prerelease: {includePrerelease}).");

                var metadataResource = await repository.GetResourceAsync<MetadataResource>(cancellationToken);

                var latestVersion = await metadataResource.GetLatestVersion(
                    package.Id,
                    includePrerelease,
                    includeUnlisted: false,
                    cache,
                    _nugetLogger,
                    cancellationToken);

                if (latestVersion == null)
                {
                    _console.LogDebug($"Package {package} was not found in source {repository.PackageSource}");

                    continue;
                }
                else
                {
                    var latestPackage = new PackageIdentity(package.Id, latestVersion);

                    _console.LogDebug($"Package {latestPackage} found in source {repository.PackageSource}");

                    return new Result
                    {
                        Package = latestPackage,
                        Repository = repository,
                        Settings = settings
                    };
                }
            }

            _console.LogDebug($"Package {package} not found");

            return new Result
            {
                Package = package,
                Repository = null,
                Settings = settings
            };
        }

        public class Result
        {
            public PackageIdentity Package { get; init; }
            public SourceRepository Repository { get; init; }
            public ISettings Settings { get; init; }
            public bool Found => Repository != null;
        }
    }
}