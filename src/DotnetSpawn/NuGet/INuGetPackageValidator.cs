using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Packaging;

namespace DotnetSpawn.NuGet
{
    internal interface INuGetPackageValidator
    {
        Task<ValidationResult> ValidateAsync(
            PackageReaderBase packageReader, CancellationToken cancellationToken);
    }
}