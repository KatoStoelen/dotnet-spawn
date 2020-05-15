namespace DotnetSpawn.Templating.Execution
{
    internal class VariableBag : IReadOnlyVariableBag
    {
        private readonly Dictionary<string, object> _bag = new(StringComparer.OrdinalIgnoreCase);

        public object Get(string variableName)
        {
            if (!_bag.ContainsKey(variableName))
            {
                throw new ArgumentException($"Invalid variable '{variableName}'");
            }

            return _bag[variableName];
        }

        public void Set(string name, object value)
        {
            _bag[name] = value;
        }
    }
}
