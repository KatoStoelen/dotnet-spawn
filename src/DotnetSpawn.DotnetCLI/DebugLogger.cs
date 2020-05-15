using System.IO;
using System.Text;
using DotnetSpawn.Plugin;

namespace DotnetSpawn.DotnetCLI
{
    internal class DebugLogger : TextWriter
    {
        private readonly IConsole _console;
        private string _lineBuffer = string.Empty;

        public DebugLogger(IConsole console)
        {
            _console = console;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            if (value == '\r')
            {
                return;
            }

            if (value == '\n')
            {
                _console.LogDebug(_lineBuffer);

                _lineBuffer = string.Empty;

                return;
            }

            _lineBuffer += value;
        }

        public override void Flush()
        {
            if (_lineBuffer.Length > 0)
            {
                _console.LogTrace(_lineBuffer);
            }
        }
    }
}