using System;
using Microsoft.Extensions.DependencyInjection;


namespace Iridium.GatewayCore
{
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/>s.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the <see cref="HttpRequestDispatcherProvider"/>.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        /// <returns>
        /// The <see cref="IServiceCollection"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Either the services or the provider is null.</exception>
        public static IServiceCollection AddHttpRequestDispatcherProvider(
            this IServiceCollection services,
            HttpRequestDispatcherProvider provider)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            services.AddSingleton(provider);

            return services;
        }

        /// <summary>
        /// Adds a new <see cref="HttpRequestDispatcherProvider"/> to the services.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        /// <returns>
        /// The <see cref="IServiceCollection"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">The input service collection is null.</exception>
        public static IServiceCollection AddDefaultHttpRequestDispatcherProvider(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddHttpRequestDispatcherProvider(
                new HttpRequestDispatcherProvider(
                    null,
                    new[] { new AlwaysTreatedAsNonTransientExceptionHandler() }));

            return services;
        }
    }
}