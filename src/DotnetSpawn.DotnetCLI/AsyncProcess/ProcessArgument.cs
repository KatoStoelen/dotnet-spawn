using System.Collections;
using System.Collections.Generic;

namespace DotnetSpawn.DotnetCLI.AsyncProcess
{
    internal abstract class ProcessArgument : IEnumerable<string>
    {
        public static ProcessArgument Verb(string name) =>
            new VerbArgument(name);

        public static ProcessArgument Noun(string value) =>
            new NounArgument(value);

        public static ProcessArgument Option(string option) =>
            new OptionArgument(option);

        public static ProcessArgument Option(string option, string value) =>
            new OptionArgument(option, value);

        public IEnumerator<string> GetEnumerator() => GetArgumentParts().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected abstract IEnumerable<string> GetArgumentParts();

        private class VerbArgument : ProcessArgument
        {
            private readonly string _name;

            public VerbArgument(string name) => _name = name;

            protected override IEnumerable<string> GetArgumentParts()
            {
                yield return _name;
            }

            public override string ToString() => _name;
        }

        private class NounArgument : ProcessArgument
        {
            private readonly string _value;

            public NounArgument(string value)
            {
                _value = value;
            }

            protected override IEnumerable<string> GetArgumentParts()
            {
                yield return _value;
            }

            public override string ToString() => _value;
        }

        private class OptionArgument : ProcessArgument
        {
            private readonly string _option;
            private readonly string _value;

            public OptionArgument(string option)
                : this(option, null)
            {
            }

            public OptionArgument(string option, string value)
            {
                _option = option;
                _value = value;
            }

            protected override IEnumerable<string> GetArgumentParts()
            {
                yield return _option;

                if (_value != null)
                {
                    yield return _value;
                }
            }

            public override string ToString() =>
                _value == null
                    ? _option
                    : $"{_option} {_value}";
        }
    }
}
