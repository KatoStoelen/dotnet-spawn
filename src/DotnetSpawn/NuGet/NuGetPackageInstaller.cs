using System.ComponentModel.DataAnnotations;
using DotnetSpawn.Configuration;
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
        private readonly NuGetPackageFinder _packageFinder;
        private readonly SpectreConsole _console;
        private readonly IReadOnlyCollection<INuGetPackageValidator> _validators;
        private readonly PackagePathResolver _packagePathResolver;
        private readonly NuGetLogger _nugetLogger;

        public NuGetPackageInstaller(
            DotnetSpawnConfiguration configuration,
            NuGetPackageFinder packageFinder,
            SpectreConsole console,
            IEnumerable<INuGetPackageValidator> validators)
        {
            _installRootDirectory = configuration.PluginRootDirectory;
            _packageFinder = packageFinder;
            _console = console;
            _validators = validators.ToList().AsReadOnly();
            _packagePathResolver = new PackagePathResolver(_installRootDirectory);
            _nugetLogger = new NuGetLogger(_console);
        }

        public async Task<NuGetPackageInstallResult> InstallAsync(
            PackageIdentity package,
            PackageSource packageSource = null,
            FileInfo nugetConfigFile = null,
            bool includePrerelease = false,
            bool force = false,
            CancellationToken cancellationToken = default)
        {
            return await InstallAsync(
                await _packageFinder.FindAsync(
                    package,
                    packageSource,
                    nugetConfigFile,
                    includePrerelease,
                    cancellationToken),
                force,
                cancellationToken);
        }

        public async Task<NuGetPackageInstallResult> InstallAsync(
            NuGetPackageFinder.Result packageResult,
            bool force,
            CancellationToken cancellationToken = default)
        {
            if (!packageResult.Found)
            {
                return new NuGetPackageInstallResult.NotFound();
            }

            if (!Directory.Exists(_installRootDirectory))
            {
                _console.LogDebug($"Creating install directory {_installRootDirectory}");

                _ = Directory.CreateDirectory(_installRootDirectory);
            }

            var packageToInstall = packageResult.Package;
            var repository = packageResult.Repository;
            var settings = packageResult.Settings ?? Settings.LoadDefaultSettings(root: null);

            if (packageToInstall == null)
            {
                return new NuGetPackageInstallResult.NotFound();
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
                    return new NuGetPackageInstallResult.AlreadyInstalled();
                }
            }

            packagePath = _packagePathResolver.GetInstallPath(packageToInstall);

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

                _console.LogDebug($"Extracting {packageToInstall} to {packagePath}");

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

            return new NuGetPackageInstallResult.Installed(packagePath);
        }
    }
}