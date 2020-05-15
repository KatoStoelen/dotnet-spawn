using System.Text;
using DotnetSpawn.Extensions;
using DotnetSpawn.Plugin;
using Spectre.Console;

namespace DotnetSpawn.IO
{
    internal class MarkupRenderer : IRenderer
    {
        private static readonly IAnsiConsole s_stdoutConsole;
        private static readonly IAnsiConsole s_stderrConsole;

        static MarkupRenderer()
        {
            s_stdoutConsole = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Ansi = AnsiSupport.Detect,
                ColorSystem = ColorSystemSupport.Detect,
                Out = new AnsiConsoleOutput(Console.Out)
            });

            s_stderrConsole = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Ansi = AnsiSupport.Detect,
                ColorSystem = ColorSystemSupport.Detect,
                Out = new AnsiConsoleOutput(Console.Error)
            });
        }

        public void RenderLog(LogLevel logLevel, RawString message, Exception exception = null)
        {
            var style = logLevel.GetLogLevelStyle();
            var markupBuilder = GetLogStringBuilder(logLevel, style);
            var console = logLevel == LogLevel.Error ? s_stderrConsole : s_stdoutConsole;

            Render(markupBuilder, message, console, style, exception);
        }

        public void RenderLog(LogLevel logLevel, FormattableString message, Exception exception = null)
        {
            if (message.ArgumentCount == 0)
            {
                RenderLog(logLevel, message.Format, exception);

                return;
            }

            var style = logLevel.GetLogLevelStyle();
            var markupBuilder = GetLogStringBuilder(logLevel, style);
            var console = logLevel == LogLevel.Error ? s_stderrConsole : s_stdoutConsole;

            Render(markupBuilder, message, console, style, exception);
        }

        public void Render(RawString message, Style style)
        {
            Render(message, s_stdoutConsole, style, exception: null);
        }

        public void Render(FormattableString message, Style style)
        {
            Render(message, s_stdoutConsole, style, exception: null);
        }

        private static void Render(
            RawString message,
            IAnsiConsole console,
            Style style,
            Exception exception)
        {
            Render(new StringBuilder(), message, console, style, exception);
        }

        private static void Render(
            StringBuilder markupBuilder,
            RawString message,
            IAnsiConsole console,
            Style style,
            Exception exception)
        {
            RenderMarkup(
                markupBuilder
                    .Append(ApplyStyle(message.Value, style.MessageStyle))
                    .ToString(),
                console,
                exception,
                style);
        }

        private static void Render(
            FormattableString message,
            IAnsiConsole console,
            Style style,
            Exception exception)
        {
            Render(new StringBuilder(), message, console, style, exception);
        }

        private static void Render(
            StringBuilder markupBuilder,
            FormattableString message,
            IAnsiConsole console,
            Style style,
            Exception exception)
        {
            markupBuilder.Append(
                string.Format(
                    ApplyStyle(message.Format, style.MessageStyle),
                    message
                        .GetArguments()
                        .Select(arg => ApplyStyle(arg?.ToString() ?? "[null]", style.InterpolationStyle))
                        .ToArray()));

            RenderMarkup(markupBuilder.ToString(), console, exception, style);
        }

        private static void RenderMarkup(
            string markup, IAnsiConsole console, Exception exception, Style style)
        {
            if (style.EmptyLineBefore)
            {
                console.WriteLine();
            }

            console.MarkupLine(markup);

            if (exception != null && Program.LogLevel < LogLevel.Information)
            {
                console.WriteException(exception);
            }

            if (style.EmptyLineAfter)
            {
                console.WriteLine();
            }
        }

        private static StringBuilder GetLogStringBuilder(LogLevel logLevel, Style style)
        {
            return new StringBuilder()
                .Append(ApplyStyle($"{logLevel.GetLogLevelString()}: ", style.LogLevelStyle));
        }

        private static string ApplyStyle(string text, string style)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(style))
            {
                return text;
            }

            return $"[{style}]{text.EscapeMarkup()}[/]";
        }
    }
}