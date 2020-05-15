using System.ComponentModel.DataAnnotations;
using System.Text;
using NuGet.Packaging.Core;

namespace DotnetSpawn.NuGet
{
    internal class NuGetPackageValidationException : Exception
    {
        public NuGetPackageValidationException(
                PackageIdentity package,
                IEnumerable<ValidationResult> validationResults)
            : base(BuildErrorMessage(package, validationResults))
        {
        }

        private static string BuildErrorMessage(
            PackageIdentity package,
            IEnumerable<ValidationResult> validationResults)
        {
            return new StringBuilder()
                .AppendLine($"Validation of {package} resulted in the following errors:")
                .AppendJoin(Environment.NewLine, validationResults.Select(result => $"- {result.ErrorMessage}"))
                .AppendLine()
                .ToString();
        }
    }
}