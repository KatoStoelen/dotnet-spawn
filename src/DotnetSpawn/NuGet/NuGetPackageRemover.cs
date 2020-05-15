using System.IO;
using DotnetSpawn.IO;
using NuGet.Packaging;
using NuGet.Packaging.Core;

namespace DotnetSpawn.NuGet
{
    internal class NuGetPackageRemover
    {
        private readonly string _installRootDirectory;
        private readonly SpectreConsole _console;
        private readonly PackagePathResolver _packagePathResolver;

        public NuGetPackageRemover(string installRootDirectory, SpectreConsole console)
        {
            _installRootDirectory = installRootDirectory;
            _console = console;
            _packagePathResolver = new PackagePathResolver(_installRootDirectory);
        }

        public void RemovePackage(PackageIdentity package)
        {
            _console.LogDebug($"Removing package {package} from {_installRootDirectory}");

            var installedPackagePath = _packagePathResolver.GetInstalledPath(package);
            if (string.IsNullOrEmpty(installedPackagePath))
            {
                _console.LogDebug($"Could not resolve install path of {package}");
                throw new DirectoryNotFoundException("Package install directory not found");
            }

            _console.LogDebug($"Deleting directory {installedPackagePath}");

            Directory.Delete(installedPackagePath, recursive: true);

            _console.LogDebug($"Package {package} successfully removed");
        }
    }
}