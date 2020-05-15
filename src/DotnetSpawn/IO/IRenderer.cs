using DotnetSpawn.Plugin;

namespace DotnetSpawn.IO
{
    internal interface IRenderer
    {
        void Render(RawString message, Style style);
        void Render(FormattableString message, Style style);
        void RenderLog(LogLevel logLevel, RawString message, Exception exception = null);
        void RenderLog(LogLevel logLevel, FormattableString message, Exception exception = null);
    }
}
