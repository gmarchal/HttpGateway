using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;

namespace Iridium.GatewayCore
{
    /// <summary>
    /// Defines the gateway middleware.
    /// </summary>
    public class GatewayMiddleware
    {
        private readonly HttpRequestDispatcherProvider dispatcherProvider;

        private readonly GatewayOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="GatewayMiddleware"/> class.
        /// </summary>
        /// <param name="next">
        /// The next.
        /// </param>
        /// <param name="dispatcherProvider">
        /// The dispatcher provider.
        /// </param>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <exception cref="ArgumentNullException">Either the dispatcher provider or the options is null.</exception>
        public GatewayMiddleware(
            RequestDelegate next,
            HttpRequestDispatcherProvider dispatcherProvider,
            IOptions<GatewayOptions> options)
        {
            if (dispatcherProvider == null)
            {
                throw new ArgumentNullException(nameof(dispatcherProvider));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.dispatcherProvider = dispatcherProvider;
            this.options = options.Value;
        }

        /// <summary>
        /// Invokes the middleware logic in the pipeline.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that can be awaited.
        /// </returns>
        /// <exception cref="ArgumentNullException">The input context is null.</exception>
        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // NOTE:
            // Some of the code is copied from
            // https://github.com/AspNet/Proxy/blob/dev/src/Microsoft.AspNetCore.Proxy/ProxyMiddleware.cs for prototype
            // purpose.
            // Reviewing the license of the code will be needed if this code is to be used in production.
            var servicePartitionKey = this.options.ServicePartitionKeySelector == null
                                          ? new ServicePartitionKey()
                                          : this.options.ServicePartitionKeySelector.Invoke(context);
            var servicePartitionClient = new ServicePartitionClient<HttpRequestDispatcher>(
                this.dispatcherProvider,
                this.options.ServiceUri,
                servicePartitionKey,
                this.options.TargetReplicaSelector,
                this.options.ListenerName,
                this.options.OperationRetrySettings);

            try
            {
                await servicePartitionClient.InvokeWithRetryAsync(
                    async dispatcher =>
                    {
                        await this.InvokeAsync(context, dispatcher);
                    });
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error while handling request for address {url}", context.Request.Path);
            }
        }

        private async Task InvokeAsync(HttpContext context, HttpRequestDispatcher dispatcher)
        {
            var requestMessage = new HttpRequestMessage();

            // Copy the request method
            requestMessage.Method = new HttpMethod(context.Request.Method);

            // Copy the request content
            if (!StringComparer.OrdinalIgnoreCase.Equals(context.Request.Method, "GET")
                && !StringComparer.OrdinalIgnoreCase.Equals(context.Request.Method, "HEAD")
                && !StringComparer.OrdinalIgnoreCase.Equals(context.Request.Method, "DELETE")
                && !StringComparer.OrdinalIgnoreCase.Equals(context.Request.Method, "TRACE"))
            {
                requestMessage.Content = new StreamContent(context.Request.Body);
            }

            // Copy the request headers
            foreach (var header in context.Request.Headers)
            {
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray())
                    && requestMessage.Content != null)
                {
                    requestMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            // Flow path base through the custom header X-ServiceFabric-PathBase.
            requestMessage.Headers.TryAddWithoutValidation("X-ServiceFabric-PathBase", context.Request.PathBase);

            // Construct the request URL
            var baseAddress = dispatcher.BaseAddress;
            var pathPrefix = PathString.FromUriComponent(baseAddress).ToString();
            if (pathPrefix == "/")
            {
                pathPrefix = string.Empty;
            }

            var pathAndQuery = context.Request.Path + context.Request.QueryString;

            var requestUri = string.Format("{0}://{1}:{2}", baseAddress.Scheme, baseAddress.Host, baseAddress.Port);
            requestMessage.RequestUri = new Uri(requestUri, UriKind.Absolute);
            requestMessage.RequestUri = new Uri(
                requestMessage.RequestUri,
                pathPrefix + this.options.RelativePath + pathAndQuery);
            
            // Set host header
            requestMessage.Headers.Host = baseAddress.Host + ":" + baseAddress.Port;
            Log.Verbose(
                "Gateway: original path {original} rewritten to {rewritten}",
                dispatcher.BaseAddress,
                requestMessage.RequestUri);

            // Send request and copy the result back to HttpResponse
            using (
                var responseMessage =
                    await
                    dispatcher.SendAsync(
                        requestMessage,
                        HttpCompletionOption.ResponseHeadersRead,
                        context.RequestAborted))
            {
                // If the service is temporarily unavailable, throw to retry later.
                if (responseMessage.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    responseMessage.EnsureSuccessStatusCode();
                }

                // Copy the status code
                context.Response.StatusCode = (int)responseMessage.StatusCode;

                // Copy the response headers
                foreach (var header in responseMessage.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }

                foreach (var header in responseMessage.Content.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }

                // SendAsync removes chunking from the response. This removes the header so it doesn't
                // expect a chunked response.
                context.Response.Headers.Remove("transfer-encoding");

                // Copy the response content
                await responseMessage.Content.CopyToAsync(context.Response.Body);
            }
        }
    }
}