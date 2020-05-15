using System;
using System.Linq;
using System.Text;
using DotnetSpawn.Extensions;
using DotnetSpawn.Plugin;
using Spectre.Console;

namespace DotnetSpawn.IO
{
    internal class MarkupRenderer : IRenderer
    {
        public void RenderLog(LogLevel logLevel, RawString message, Exception exception = null)
        {
            var style = logLevel.GetLogLevelStyle();
            var markupBuilder = GetLogStringBuilder(logLevel, style);

            Render(markupBuilder, message, style, exception);
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

            Render(markupBuilder, message, style, exception);
        }

        public void Render(RawString message, Style style)
        {
            Render(message, style, exception: null);
        }

        public void Render(FormattableString message, Style style)
        {
            Render(message, style, exception: null);
        }

        private static void Render(
            RawString message,
            Style style,
            Exception exception)
        {
            Render(new StringBuilder(), message, style, exception);
        }

        private static void Render(
            StringBuilder markupBuilder,
            RawString message,
            Style style,
            Exception exception)
        {
            RenderMarkup(
                markupBuilder
                    .Append(ApplyStyle(message.Value, style.MessageStyle))
                    .ToString(),
                exception,
                style);
        }

        private static void Render(
            FormattableString message,
            Style style,
            Exception exception)
        {
            Render(new StringBuilder(), message, style, exception);
        }

        private static void Render(
            StringBuilder markupBuilder,
            FormattableString message,
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

            RenderMarkup(markupBuilder.ToString(), exception, style);
        }

        private static void RenderMarkup(
            string markup, Exception exception, Style style)
        {
            if (style.EmptyLineBefore)
            {
                Console.WriteLine();
            }

            AnsiConsole.MarkupLine(markup);

            if (exception != null && Program.LogLevel < LogLevel.Information)
            {
                AnsiConsole.WriteException(exception);
            }

            if (style.EmptyLineAfter)
            {
                Console.WriteLine();
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