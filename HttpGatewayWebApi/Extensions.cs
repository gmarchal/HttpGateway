using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Iridium.GatewayCore;

namespace HttpGatewayWebApi
{
    /// <summary>
    /// Defines extension methods useful to configure the gateway behavior.
    /// </summary>
    public static class Extensions
    {
        public static IApplicationBuilder MapGateway(this IApplicationBuilder app, string path, string applicationName, string serviceName)
        {
            var options = GetOptions(path, GetApplicationUri(applicationName, serviceName));
            return app.Map(
                path,
                subApp =>
                {
                    subApp.RunGateway(options);
                });
        }

        private static GatewayOptions GetOptions(string relativePath, Uri serviceUri)
        {
            var unitServiceOptions = new GatewayOptions
            {
                RelativePath = new Uri(relativePath, UriKind.Relative),
                ServiceUri = serviceUri,
                OperationRetrySettings = new OperationRetrySettings(
                                                 TimeSpan.FromSeconds(2),
                                                 TimeSpan.FromSeconds(2),
                                                 30)
            };
            return unitServiceOptions;
        }        

        private static Uri GetApplicationUri(string applicationName, string serviceName)
        {
            if (applicationName == null)
            {
                throw new ArgumentNullException(nameof(applicationName));
            }
            if (serviceName == null)
            {
                throw new ArgumentNullException(nameof(serviceName));
            }

            return new Uri($"fabric:/{applicationName}/{serviceName}", UriKind.Absolute);
        }
    }
}