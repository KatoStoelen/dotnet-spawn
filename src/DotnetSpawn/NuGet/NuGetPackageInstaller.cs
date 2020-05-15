using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpawn.Extensions;
using DotnetSpawn.IO;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Protocol.Core.Types;

namespace DotnetSpawn.NuGet
{
    internal class NuGetPackageInstaller
    {
        private readonly string _installRootDirectory;
        private readonly SpectreConsole _console;
        private readonly IReadOnlyCollection<INuGetPackageValidator> _validators;
        private readonly PackagePathResolver _packagePathResolver;
        private readonly NuGetLogger _nugetLogger;

        public NuGetPackageInstaller(
            string installRootDirectory,
            SpectreConsole console,
            IEnumerable<INuGetPackageValidator> validators)
        {
            _installRootDirectory = installRootDirectory;
            _console = console;
            _validators = validators.ToList().AsReadOnly();
            _packagePathResolver = new PackagePathResolver(_installRootDirectory);
            _nugetLogger = new NuGetLogger(_console);
        }

        public Task<NuGetPackageInstallResult> InstallAsync(
            PackageIdentity package,
            bool includePrerelease = false,
            bool force = false,
            CancellationToken cancellationToken = default)
        {
            return InstallAsync(
                package,
                Settings.LoadDefaultSettings(root: Directory.GetCurrentDirectory()),
                includePrerelease,
                force,
                cancellationToken);
        }

        public Task<NuGetPackageInstallResult> InstallAsync(
            PackageIdentity package,
            PackageSource packageSource,
            bool includePrerelease = false,
            bool force = false,
            CancellationToken cancellationToken = default)
        {
            var sourceRepository = new SourceRepository(
                packageSource, Repository.Provider.GetCoreV3());

            return InstallAsync(
                package,
                new[] { sourceRepository },
                settings: null,
                includePrerelease,
                force,
                cancellationToken);
        }

        public Task<NuGetPackageInstallResult> InstallAsync(
            PackageIdentity package,
            FileInfo nugetConfigFile,
            bool includePrerelease = false,
            bool force = false,
            CancellationToken cancellationToken = default)
        {
            if (!nugetConfigFile.Exists)
            {
                throw new FileNotFoundException($"NuGet config file '{nugetConfigFile}' not found.");
            }

            return InstallAsync(
                package,
                Settings.LoadDefaultSettings(
                    root: nugetConfigFile.Directory.FullName,
                    nugetConfigFile.Name,
                    new XPlatMachineWideSetting()),
                includePrerelease,
                force,
                cancellationToken);
        }

        private Task<NuGetPackageInstallResult> InstallAsync(
            PackageIdentity package,
            ISettings settings,
            bool includePrerelease,
            bool force,
            CancellationToken cancellationToken = default)
        {
            var sourceRepositoryProvider = new SourceRepositoryProvider(
                new PackageSourceProvider(settings),
                Repository.Provider.GetCoreV3());

            return InstallAsync(
                package,
                sourceRepositoryProvider.GetRepositories(),
                settings,
                includePrerelease,
                force,
                cancellationToken);
        }

        private async Task<NuGetPackageInstallResult> InstallAsync(
            PackageIdentity package,
            IEnumerable<SourceRepository> sourceRepositories,
            ISettings settings,
            bool includePrerelease,
            bool force,
            CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(_installRootDirectory))
            {
                _console.LogDebug($"Creating install directory {_installRootDirectory}");

                _ = Directory.CreateDirectory(_installRootDirectory);
            }

            if (settings == null)
            {
                settings = Settings.LoadDefaultSettings(root: null);
            }

            var (packageToInstall, repository) = await FindPackageAsync(
                package, sourceRepositories, includePrerelease, cancellationToken);

            if (packageToInstall == null)
            {
                return NuGetPackageInstallResult.NotFound;
            }

            var packagePath = _packagePathResolver.GetInstalledPath(packageToInstall);

            if (!string.IsNullOrEmpty(packagePath))
            {
                _console.LogDebug($"Package {packageToInstall} is already installed");

                if (force)
                {
                    _console.LogDebug($"Force install. Deleting package directory {packagePath}");

                    Directory.Delete(packagePath, recursive: true);
                }
                else
                {
                    return NuGetPackageInstallResult.AlreadyInstalled;
                }
            }

            var resource = await repository.GetResourceAsync<DownloadResource>(cancellationToken);

            await _console.DisplayProgressAsync(async progress =>
            {
                var downloadTask = progress.AddTask($"Downloading {packageToInstall}");

                using var downloadResourceResult = await resource.GetDownloadResourceResultAsync(
                    packageToInstall,
                    new PackageDownloadContext(new SourceCacheContext()),
                    SettingsUtility.GetGlobalPackagesFolder(settings),
                    _nugetLogger,
                    cancellationToken);

                if (downloadResourceResult.Status == DownloadResourceResultStatus.AvailableWithoutStream)
                {
                    _console.LogDebug($"Package is available, but no stream");
                    throw new PackagingException($"Failed to get stream of package {packageToInstall}");
                }
                else if (downloadResourceResult.Status == DownloadResourceResultStatus.Cancelled)
                {
                    _console.LogDebug($"Package installation cancelled");
                    throw new OperationCanceledException("Package installation cancelled");
                }

                if (_validators.Any())
                {
                    _console.LogDebug("Running package validators");

                    var validationResults = new List<ValidationResult>();

                    foreach (var validator in _validators)
                    {
                        var validationResult = await validator.ValidateAsync(
                            downloadResourceResult.PackageReader, cancellationToken);

                        if (validationResult != null)
                        {
                            validationResults.Add(validationResult);
                        }
                    }

                    if (!validationResults.Any())
                    {
                        _console.LogDebug($"Package is valid");
                    }
                    else
                    {
                        _console.LogDebug($"Package validation failed");

                        throw new NuGetPackageValidationException(packageToInstall, validationResults);
                    }
                }
                else
                {
                    _console.LogDebug("No package validators registered");
                }

                downloadTask.MaxValue = downloadResourceResult.PackageStream.Length;

                using var memoryStream = new MemoryStream();

                _console.LogDebug($"Downloading {packageToInstall} to memory");

                await downloadResourceResult.PackageStream.CopyToAsync(
                    memoryStream,
                    currentBytesRead => downloadTask.Value = currentBytesRead,
                    cancellationToken: cancellationToken);

                downloadTask.MarkAsCompleted();

                _console.LogDebug($"Extracting {packageToInstall} to {_packagePathResolver.GetInstallPath(packageToInstall)}");

                var extractionTask = progress.AddTask($"Extracting {packageToInstall}");

                var packageExtractionContext = new PackageExtractionContext(
                    PackageSaveMode.Nuspec | PackageSaveMode.Files | PackageSaveMode.Nupkg,
                    XmlDocFileSaveMode.None,
                    ClientPolicyContext.GetClientPolicy(settings, _nugetLogger),
                    _nugetLogger);

                _ = await PackageExtractor.ExtractPackageAsync(
                    downloadResourceResult.PackageSource,
                    memoryStream,
                    _packagePathResolver,
                    packageExtractionContext,
                    cancellationToken);

                extractionTask.Value = extractionTask.MaxValue;
                extractionTask.MarkAsCompleted();
            });

            return NuGetPackageInstallResult.Installed;
        }

        private async Task<(PackageIdentity, SourceRepository)> FindPackageAsync(
            PackageIdentity package,
            IEnumerable<SourceRepository> sourceRepositories,
            bool includePrerelease,
            CancellationToken cancellationToken)
        {
            var cache = new SourceCacheContext();

            foreach (var repository in sourceRepositories)
            {
                _console.LogDebug($"Looking up package {package} in source {repository.PackageSource}");

                var resource = await repository
                    .GetResourceAsync<FindPackageByIdResource>(cancellationToken);

                if (package.HasVersion)
                {
                    _console.LogDebug($"Package version specified. Checking if specified package and version exists.");

                    var packageFoundInRepository = await resource.DoesPackageExistAsync(
                        package.Id, package.Version, cache, _nugetLogger, cancellationToken);

                    if (packageFoundInRepository)
                    {
                        _console.LogDebug($"Package {package} found in source {repository.PackageSource}");

                        return (package, repository);
                    }
                    else
                    {
                        _console.LogDebug($"Package {package} was not found in source {repository.PackageSource}");

                        continue;
                    }
                }

                _console.LogDebug($"No package version specifed. Fetching package versions (include prerelease: {includePrerelease}).");

                var allVersions = (await resource.GetAllVersionsAsync(
                        package.Id, cache, _nugetLogger, cancellationToken)
                    ).ToList();

                if (!allVersions.Any())
                {
                    _console.LogDebug($"Package {package} was not found in source {repository.PackageSource}");

                    continue;
                }
                else
                {
                    var latestVersion = allVersions
                        .OrderByDescending(version => version)
                        .FirstOrDefault(version => includePrerelease || !version.IsPrerelease);

                    if (latestVersion == null)
                    {
                        _console.LogDebug("Only pre-release versions available");

                        return (null, null);
                    }

                    var latestPackage = new PackageIdentity(package.Id, latestVersion);

                    _console.LogDebug($"Package {latestPackage} found in source {repository.PackageSource}");

                    return (latestPackage, repository);
                }
            }

            _console.LogError($"Package {package} not found.");

            return (null, null);
        }
    }
}