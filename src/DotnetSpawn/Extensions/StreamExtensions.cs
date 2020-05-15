namespace DotnetSpawn.Extensions
{
    internal static class StreamExtensions
    {
        public static async Task CopyToAsync(
            this Stream source,
            Stream target,
            Action<long> progressCallback,
            int bufferSize = 0x14000,
            CancellationToken cancellationToken = default)
        {
            var buffer = new byte[bufferSize];
            var totalBytesRead = 0L;

            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
            {
                await target.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                totalBytesRead += bytesRead;

                progressCallback(totalBytesRead);
            }
        }
    }
}