using System;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;

namespace Iridium.GatewayCore
{
    /// <summary>
    /// Gateway extension methods.
    /// </summary>
    public static class GatewayExtensions
    {
        /// <summary>
        /// Adds the <see cref="GatewayMiddleware"/> to the pipeline.
        /// </summary>
        /// <param name="app">
        /// The app.
        /// </param>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <returns>
        /// The <see cref="IApplicationBuilder"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Either the application builder or the gateway options is null.
        /// </exception>
        public static IApplicationBuilder RunGateway(this IApplicationBuilder app, GatewayOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<GatewayMiddleware>(Options.Create(options));
        }
    }
}