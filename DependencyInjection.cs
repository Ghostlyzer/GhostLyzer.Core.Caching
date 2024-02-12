using GhostLyzer.Core.Caching.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GhostLyzer.Core.Caching
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCachingRequest(
            this IServiceCollection services,
            IList<Assembly> assembliesToScan,
            ServiceLifetime lifetime= ServiceLifetime.Transient) 
        {
            // ICacheRequest discovery and registration of services
            services.Scan(scan => scan
                .FromAssemblies(assembliesToScan ?? AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(classes => classes.AssignableTo(typeof(ICacheRequest)), false)
                .AsImplementedInterfaces()
                .WithLifetime(lifetime));

            // IIvalidateCacheRequest discovery and registration of services
            services.Scan(scan => scan
                .FromAssemblies(assembliesToScan ?? AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(classes => classes.AssignableTo(typeof(IInvalidateCacheRequest)), false)
                .AsImplementedInterfaces()
                .WithLifetime(lifetime));

            return services;
        }
    }
}
