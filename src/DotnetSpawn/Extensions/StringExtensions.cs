namespace DotnetSpawn.Extensions
{
    internal static class StringExtensions
    {
        public static string ReplaceAt(this string input, int index, char newChar)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (index < 0 || index >= input.Length)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index), index, $"Index must be within rage [0, {input.Length})");
            }

            var characters = input.ToCharArray();
            characters[index] = newChar;

            return new string(characters);
        }

        public static bool IsSubPathOf(this string path, string baseDirectoryPath)
        {
            var normalizedPath = Path
                .GetFullPath(
                    path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar))
                .EnsureTrailingSlash();

            var normalizedBaseDirectoryPath = Path
                .GetFullPath(
                    baseDirectoryPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar))
                .EnsureTrailingSlash();

            return normalizedPath
                .StartsWith(normalizedBaseDirectoryPath, StringComparison.OrdinalIgnoreCase);
        }

        private static string EnsureTrailingSlash(this string path)
        {
            return
                path.TrimEnd(Path.DirectorySeparatorChar) +
                Path.DirectorySeparatorChar;
        }
    }
}