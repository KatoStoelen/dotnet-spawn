using System.ComponentModel.DataAnnotations;
using NuGet.Packaging;

namespace DotnetSpawn.NuGet
{
    internal interface INuGetPackageValidator
    {
        Task<ValidationResult> ValidateAsync(
            PackageReaderBase packageReader, CancellationToken cancellationToken);
    }
}