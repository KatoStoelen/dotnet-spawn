namespace DotnetSpawn.NuGet
{
    internal class NuGetPackageInstallResult
    {
        public class Installed : NuGetPackageInstallResult
        {
            public Installed(string pluginDirectoryPath)
            {
                PluginDirectoryPath = pluginDirectoryPath;
            }

            public string PluginDirectoryPath { get; }
        }

        public class AlreadyInstalled : NuGetPackageInstallResult
        {
        }

        public class NotFound : NuGetPackageInstallResult
        {
        }
    }
}