using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotnetSpawn.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAllImplementationsOf<TService>(
                this IServiceCollection services,
                ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
            where TService : class
        {
            var serviceType = typeof(TService);
            var assembly = serviceType.Assembly;

            var implementations = assembly
                .GetTypes()
                .Where(type =>
                    type != serviceType &&
                    type.IsClass &&
                    !type.IsAbstract &&
                    type.IsAssignableTo(serviceType));

            var descriptors = implementations
                .Select(impl => ServiceDescriptor.Describe(serviceType, impl, serviceLifetime));

            return services.Add(descriptors);
        }
    }
}