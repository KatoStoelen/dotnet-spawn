namespace DotnetSpawn.Cli.TypeConverters
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    internal class EnumAliasAttribute : Attribute
    {
        public EnumAliasAttribute(string alias)
        {
            Alias = alias;
        }

        public string Alias { get; }
    }
}