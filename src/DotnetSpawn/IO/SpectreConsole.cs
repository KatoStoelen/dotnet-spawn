using DotnetSpawn.Plugin;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace DotnetSpawn.IO
{
    internal class SpectreConsole : IConsole
    {
        private readonly IRenderer _renderer;

        public SpectreConsole(IRenderer renderer)
        {
            _renderer = renderer;
        }

        public void WriteLine(RawString message, Style style)
        {
            if (Program.LogLevel == LogLevel.None)
            {
                return;
            }

            _renderer.Render(message, style);
        }

        public void WriteLine(FormattableString message, Style style)
        {
            if (Program.LogLevel == LogLevel.None)
            {
                return;
            }

            _renderer.Render(message, style);
        }

        public void LogDebug(RawString message)
        {
            if (Program.LogLevel > LogLevel.Debug)
            {
                return;
            }

            _renderer.RenderLog(LogLevel.Debug, message);
        }

        public void LogDebug(FormattableString message)
        {
            if (Program.LogLevel > LogLevel.Debug)
            {
                return;
            }

            _renderer.RenderLog(LogLevel.Debug, message);
        }

        public void LogError(RawString message, Exception exception = null)
        {
            if (Program.LogLevel > LogLevel.Error)
            {
                return;
            }

            _renderer.RenderLog(LogLevel.Error, message, exception);
        }

        public void LogError(FormattableString message, Exception exception = null)
        {
            if (Program.LogLevel > LogLevel.Error)
            {
                return;
            }

            _renderer.RenderLog(LogLevel.Error, message, exception);
        }

        public void LogInformation(RawString message)
        {
            if (Program.LogLevel > LogLevel.Information)
            {
                return;
            }

            _renderer.RenderLog(LogLevel.Information, message);
        }

        public void LogInformation(FormattableString message)
        {
            if (Program.LogLevel > LogLevel.Information)
            {
                return;
            }

            _renderer.RenderLog(LogLevel.Information, message);
        }

        public void LogTrace(RawString message)
        {
            if (Program.LogLevel > LogLevel.Trace)
            {
                return;
            }

            _renderer.RenderLog(LogLevel.Trace, message);
        }

        public void LogTrace(FormattableString message)
        {
            if (Program.LogLevel > LogLevel.Trace)
            {
                return;
            }

            _renderer.RenderLog(LogLevel.Trace, message);
        }

        public void LogWarning(RawString message)
        {
            if (Program.LogLevel > LogLevel.Warning)
            {
                return;
            }

            _renderer.RenderLog(LogLevel.Warning, message);
        }

        public void LogWarning(FormattableString message)
        {
            if (Program.LogLevel > LogLevel.Warning)
            {
                return;
            }

            _renderer.RenderLog(LogLevel.Warning, message);
        }

        public bool Confirm(string message)
        {
            AnsiConsole.WriteLine();

            var confirmed = AnsiConsole.Confirm(message, defaultValue: false);

            AnsiConsole.WriteLine();

            return confirmed;
        }

        public TInput Prompt<TInput>(string message, bool isSecret = false, bool allowEmpty = false)
        {
            var textPrompt = new TextPrompt<TInput>(message);

            if (isSecret)
            {
                _ = textPrompt.Secret();
            }

            if (allowEmpty)
            {
                _ = textPrompt.AllowEmpty();
            }

            AnsiConsole.WriteLine();

            var input = AnsiConsole.Prompt(textPrompt);

            AnsiConsole.WriteLine();

            return input;
        }

        public TInput Prompt<TInput>(IEnumerable<PromptOption<TInput>> options, string message)
        {
            var selectionPrompt = new SelectionPrompt<PromptOption<TInput>>()
                .Title(message)
                .AddChoices(options)
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]");

            AnsiConsole.WriteLine();

            var selectedOption = AnsiConsole.Prompt(selectionPrompt);

            AnsiConsole.WriteLine();

            return selectedOption.Value;
        }

        public IEnumerable<TInput> PromptMultiple<TInput>(
            IEnumerable<PromptOption<TInput>> options, string message)
        {
            var multiSelectionPrompt = new MultiSelectionPrompt<PromptOption<TInput>>()
                .Title(message)
                .AddChoices(options)
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                .InstructionsText("[grey]([teal]<space>[/] to toggle, [green]<enter>[/] to accept)[/]");

            AnsiConsole.WriteLine();

            var selectedOptions = AnsiConsole.Prompt(multiSelectionPrompt);

            AnsiConsole.WriteLine();

            return selectedOptions.Select(option => option.Value);
        }

        public void DisplayProgress(Action<IProgress> action)
        {
            CreateProgress()
                .Start(context => action(new SpectreProgress(context)));
        }

        public TReturn DisplayProgress<TReturn>(Func<IProgress, TReturn> action)
        {
            return CreateProgress()
                .Start(context => action(new SpectreProgress(context)));
        }

        public Task DisplayProgressAsync(Func<IProgress, Task> action)
        {
            return CreateProgress()
                .StartAsync(context => action(new SpectreProgress(context)));
        }

        public Task<TReturn> DisplayProgressAsync<TReturn>(Func<IProgress, Task<TReturn>> action)
        {
            return CreateProgress()
                .StartAsync(context => action(new SpectreProgress(context)));
        }

        public void DisplayStatus(string status, Action<IStatus> action)
        {
            CreateStatus()
                .Start(status, context => action(new SpectreStatus(context)));
        }

        public TReturn DisplayStatus<TReturn>(string status, Func<IStatus, TReturn> action)
        {
            return CreateStatus()
                .Start(status, context => action(new SpectreStatus(context)));
        }

        public Task DisplayStatusAsync(string status, Func<IStatus, Task> action)
        {
            return CreateStatus()
                .StartAsync(status, context => action(new SpectreStatus(context)));
        }

        public Task<TReturn> DisplayStatusAsync<TReturn>(string status, Func<IStatus, Task<TReturn>> action)
        {
            return CreateStatus()
                .StartAsync(status, context => action(new SpectreStatus(context)));
        }

        public static void RenderKeyValueTable(
            IEnumerable<KeyValuePair<string, string>> keyValues,
            Action<Table> customize = null)
        {
            var keyValueTable = new Table()
                .Border(TableBorder.None)
                .HideHeaders()
                .AddColumns(string.Empty, string.Empty, string.Empty);


            foreach (var pair in keyValues)
            {
                keyValueTable.AddRow(
                    new Text(pair.Key),
                    new Text(":"),
                    new Text(pair.Value, "orange3"));
            }

            customize?.Invoke(keyValueTable);

            Render(keyValueTable);
        }

        public static void Render(
            IRenderable renderable,
            bool newLineBefore = false,
            bool newLineAfter = false)
        {
            if (Program.LogLevel == LogLevel.None)
            {
                return;
            }

            if (newLineBefore)
            {
                AnsiConsole.WriteLine();
            }

            AnsiConsole.Write(renderable);

            if (newLineAfter)
            {
                AnsiConsole.WriteLine();
            }
        }

        public static void WriteMarkup(string markup)
        {
            AnsiConsole.MarkupLine(markup);
        }

        private static Progress CreateProgress()
        {
            return AnsiConsole.Progress();
        }

        private static Status CreateStatus()
        {
            return AnsiConsole
                .Status()
                .Spinner(Spinner.Known.Dots10);
        }

        private class SpectreProgress : IProgress
        {
            private readonly ProgressContext _context;

            public SpectreProgress(ProgressContext context)
            {
                _context = context;
            }

            public IProgressTask AddTask(string description, double maxValue = 100)
            {
                return new SpectreProgressTask(
                    _context.AddTask(description, maxValue: maxValue));
            }
        }

        private class SpectreProgressTask : IProgressTask
        {
            private readonly ProgressTask _progressTask;

            public SpectreProgressTask(ProgressTask progressTask)
            {
                _progressTask = progressTask;
                _progressTask.IsIndeterminate = true;
            }

            public string Description
            {
                get => _progressTask.Description;
                set => _progressTask.Description = value;
            }

            public double MaxValue
            {
                get => _progressTask.MaxValue;
                set => _progressTask.MaxValue = value;
            }

            public double Value
            {
                get => _progressTask.Value;
                set
                {
                    _progressTask.Value = value;
                    _progressTask.IsIndeterminate = false;
                }
            }

            public void IncrementValue(double increment)
            {
                Value += increment;
            }

            public void MarkAsCompleted()
            {
                _progressTask.StopTask();
            }
        }

        private class SpectreStatus : IStatus
        {
            private readonly StatusContext _context;

            public SpectreStatus(StatusContext context)
            {
                _context = context;
            }

            public void UpdateStatus(string newStatus)
            {
                _context.Status = newStatus;
            }
        }
    }
}