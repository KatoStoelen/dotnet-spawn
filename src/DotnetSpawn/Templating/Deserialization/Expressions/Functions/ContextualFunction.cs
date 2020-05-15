using DotnetSpawn.Templating.Execution;

namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal abstract class ContextualFunction<TArgs> : Function
    {
        protected ContextualFunction()
        {
            IsContextual = true;
        }

        protected sealed override object Invoke(object[] args)
        {
            var context = (IExecutionContext)args.First();
            var otherArgs = args.Skip(1).ToArray();

            return Invoke(context, ConvertArguments<TArgs>(otherArgs));
        }

        protected abstract object Invoke(IExecutionContext context, TArgs[] args);
    }
}
