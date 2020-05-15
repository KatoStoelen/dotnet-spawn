namespace DotnetSpawn.IO
{
    internal class Style
    {
        public static readonly Style None = new NoneStyle();
        public static readonly Style LogTrace = new LogTraceStyle();
        public static readonly Style LogDebug = new LogDebugStyle();
        public static readonly Style LogInformation = new LogInformationStyle();
        public static readonly Style LogWarning = new LogWarningStyle();
        public static readonly Style LogError = new LogErrorStyle();
        public static readonly Style Highlight = new HighlightStyle();
        public static readonly Style HighlightAlt = new HighlightAltStyle();
        public static readonly Style WarningHighlight = new WarningHighlightStyle();

        private Style()
        {
        }

        public string LogLevelStyle { get; protected set; }
        public string MessageStyle { get; protected set; }
        public string InterpolationStyle { get; protected set; }
        public bool EmptyLineBefore { get; protected set; }
        public bool EmptyLineAfter { get; protected set; }

        private class LogTraceStyle : Style
        {
            public LogTraceStyle()
            {
                LogLevelStyle = "grey";
                MessageStyle = "grey";
                InterpolationStyle = "silver";
            }
        }

        private class LogDebugStyle : Style
        {
            public LogDebugStyle()
            {
                LogLevelStyle = "grey";
                MessageStyle = "grey";
                InterpolationStyle = "silver";
            }
        }

        private class LogInformationStyle : Style
        {
            public LogInformationStyle()
            {
                LogLevelStyle = "green3";
                MessageStyle = "white";
                InterpolationStyle = "orange3";
            }
        }

        private class LogWarningStyle : Style
        {
            public LogWarningStyle()
            {
                LogLevelStyle = "yellow";
                MessageStyle = "white";
                InterpolationStyle = "orange3";
            }
        }

        private class LogErrorStyle : Style
        {
            public LogErrorStyle()
            {
                LogLevelStyle = "maroon";
                MessageStyle = "white";
                InterpolationStyle = "orange3";
            }
        }

        private class HighlightStyle : Style
        {
            public HighlightStyle()
            {
                MessageStyle = "teal";
                InterpolationStyle = "lime";
                EmptyLineBefore = true;
                EmptyLineAfter = true;
            }
        }

        private class HighlightAltStyle : Style
        {
            public HighlightAltStyle()
            {
                MessageStyle = "green";
                InterpolationStyle = "teal";
                EmptyLineBefore = true;
                EmptyLineAfter = true;
            }
        }

        private class WarningHighlightStyle : Style
        {
            public WarningHighlightStyle()
            {
                MessageStyle = "yellow";
                InterpolationStyle = "orange3";
                EmptyLineBefore = true;
                EmptyLineAfter = true;
            }
        }

        private class NoneStyle : Style
        {
        }
    }
}
