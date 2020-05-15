using System.ComponentModel.DataAnnotations;
using DotnetSpawn.NuGet;
using NuGet.Packaging;

namespace DotnetSpawn.Plugins
{
    internal class PluginPackageValidator : INuGetPackageValidator
    {
        private const string PluginPackageName = "DotnetSpawn.Plugin";

        public async Task<ValidationResult> ValidateAsync(
            PackageReaderBase packageReader, CancellationToken cancellationToken)
        {
            var packageDependencies = await packageReader
                .GetPackageDependenciesAsync(cancellationToken);

            var hasRequiredPackageReference = packageDependencies
                .SelectMany(frameworkGroup => frameworkGroup.Packages)
                .Any(dependency => dependency.Id == PluginPackageName);

            return hasRequiredPackageReference
                ? null
                : new ValidationResult(
                    "Package does not look like a dotnet-spawn plugin. " +
                    $"Reference to {PluginPackageName} missing.");
        }
    }
}